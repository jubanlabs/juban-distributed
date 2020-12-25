using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Quartz;
using Quartz.Impl;

namespace Jubanlabs.JubanDistributed.WorkQueue {
    public class DelayedWorkRunner {

        public async Task<int> Run (string queueName) {

            var collection = DelayedWorkDatabase.Instance.GetDatabase().GetCollection<BsonDocument> (queueName);
            var list = await collection.Find (new BsonDocument ()).Sort (Builders<BsonDocument>.Sort.Ascending ("_id")).Limit (5000).ToListAsync ();
            foreach (var item in list) {
                AssignmentManager.GetAssignment (queueName).Send ((byte[]) item["msg"]);
            }

            return list.Count;

        }
    }

    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class DelayedWorkRunnerJob : IJob {

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();
        public async Task Execute (IJobExecutionContext context) {
            try {
                JobDataMap dataMap = context.JobDetail.JobDataMap;

                string queueName = dataMap.GetString ("queueName");
                string lastId = dataMap.GetString ("lastId");

                var collection = DelayedWorkDatabase.Instance.GetDatabase().GetCollection<BsonDocument> (queueName);
                FilterDefinition<BsonDocument> filter;
                if (lastId != null) {
                    Logger.ConditionalTrace (DateTime.Now + " lastId " + lastId);
                    filter = Builders<BsonDocument>.Filter.Gt ("_id", ObjectId.Parse (lastId));
                } else {
                    filter = new BsonDocument ();
                }
                var list = await collection.Find (filter).Sort (Builders<BsonDocument>.Sort.Ascending ("_id")).Limit (5000).ToListAsync ();
                foreach (var item in list) {
                    AssignmentManager.GetAssignment (queueName).Send ((byte[]) item["msg"]);
                }

                int count = list.Count;

                if (count == 0) {
                    Logger.ConditionalTrace (DateTime.Now + " no new record found for " + queueName);
                    int interval = dataMap.GetInt ("interval");
                    if (interval < 30) {
                        interval++;
                        dataMap.Put ("interval", interval);
                    }
                    Task.Delay (interval * 1000).Wait ();
                } else {
                    dataMap.Put ("interval", 0);
                    dataMap.Put ("lastId", list[list.Count - 1]["_id"].ToString ());
                }

                Logger.ConditionalTrace (DateTime.Now + " Delayed work runner job for queue " + queueName);
            } catch (Exception ex) {
                Logger.Error (ex, DateTime.Now + "Error: Delayed work runner job for queue " + ex.Message + " " + ex.StackTrace);
                throw ex;
            }
        }
    }



    [DisallowConcurrentExecution]
    [PersistJobDataAfterExecution]
    public class DelayedWorkRunnerJobScheduler : IJob
    {

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public async Task Execute(IJobExecutionContext context)
        {
            Logger.Info("DelayedWorkRunnerJobScheduler started 1");
            IScheduler sched = await new StdSchedulerFactory().GetScheduler();
            
            Logger.Info("DelayedWorkRunnerJobScheduler started 2");
            var queueNameList = DelayedWorkDatabase.Instance.GetDatabase().ListCollectionNames().ToList();
            
            Logger.Info("DelayedWorkRunnerJobScheduler started 3");

            foreach (var item in queueNameList)
            {
                if(!sched.CheckExists(new JobKey(item)).Result)
                {

                    // define the job and tie it to our HelloJob class
                    IJobDetail job = JobBuilder.Create<DelayedWorkRunnerJob>()
                        .UsingJobData("queueName", item)
                        .WithIdentity(new JobKey(item))
                        .Build();
                   
                    // Trigger the job to run now, and then every 40 seconds
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity(item)
                        .StartNow()
                        .WithSimpleSchedule(x => x.WithInterval(new TimeSpan(10000))
                           .RepeatForever())
                        .Build();

                    await sched.ScheduleJob(job, trigger);
                }
                else
                {
                    Logger.Info("job exist:"+ item);
                }
            }
        }
    }

    public class DelayedWorkRunnerScheduler {
        public async void Schedule () {
            StdSchedulerFactory factory = new StdSchedulerFactory ();
            
            // get a scheduler
            IScheduler sched = await factory.GetScheduler ();
            await sched.Start ();

            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<DelayedWorkRunnerJobScheduler> ()
                .WithIdentity ("delayedworkscheduler")
                .Build ();

            // Trigger the job to run now, and then every 40 seconds
            ITrigger trigger = TriggerBuilder.Create ()
                .WithIdentity ("delayedworkscheduler")
                .StartNow ()
                .WithSimpleSchedule (x => x.WithInterval (new TimeSpan(0, 0, 30))
                    .RepeatForever ())
                .Build ();

            sched.ScheduleJob (job, trigger).Wait();
           
        }
    }
}