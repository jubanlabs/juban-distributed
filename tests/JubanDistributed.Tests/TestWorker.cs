using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jubanlabs.JubanDistributed.WorkQueue;
using Jubanlabs.JubanShared.Common.Config;
using Jubanlabs.JubanShared.UnitTest;
using Quartz;
using Quartz.Impl;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace Jubanlabs.JubanDistributed.Tests {
    public class TestWorker : IClassFixture<BaseFixture> {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();

        public TestWorker (ITestOutputHelper outputHelper) {
            LoggingHelper.BindNLog (outputHelper);
        }

        public interface IWorkQueueTest : IWorkerService {

            void sendTest (String str);
            void plus (int n);
        }

        public static Boolean flag1 = false;
        public static Boolean flag2 = false;

        public static int num = 0;

        public class WorkQueueTest : 
        IWorkQueueTest {
            private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();
            public void sendTest (string str) {
                if (str.Equals ("flag1")) {
                    TestWorker.flag1 = true;
                }

                if (str.Equals ("flag2")) {
                    TestWorker.flag2 = true;
                }
                Logger.ConditionalTrace ("thread id" + Thread.CurrentThread.ManagedThreadId + " : " + str);
            }

            public void plus (int n) {
                Logger.ConditionalTrace ("plus " + n);
                TestWorker.num += n;
            }
        }

        public class testProcess : IWorkInterpreter {
            public string received = "";
            public int cnt = 0;
            public void Process (byte[] msg) {
                cnt++;
                received = Encoding.UTF8.GetString (msg);
            }
        }

        [Fact]
        public void testPoolingLease () {

        }

        [Fact]
        public void testSendWorkQueue () {
            TestWorker.flag1 = false;
            var worker = new WorkQueueTest ();

            worker.StartWorker ();

            var service = (IWorkQueueTest) DService.getWorkerService (worker.GetType (), new DistributedCallOptions () { IsPersistentAssignment = false });
            service.sendTest ("flag1");

            int cnt = 0;
            while (cnt < 5) {
                if (TestWorker.flag1) {
                    Assert.True (true);
                    return;
                } else {
                    Thread.Sleep (1000);
                    cnt++;
                }
            }
            Assert.True (false);
        }

        [Fact]
        public void testSendWorkQueueLocal () {
            Logger.ConditionalTrace (AppSettings.Instance.GetValue ("jubandistributed.messagingServer"));
            TestWorker.flag2 = false;
            var service = (IWorkQueueTest) DService.getWorkerService (typeof (IWorkQueueTest),
                new DistributedCallOptions () { IsRemoteCall = false });
            service.sendTest ("flag2");
            Logger.ConditionalTrace ("abc");
            Assert.True (TestWorker.flag2);
        }

        [Fact]
        public void testBasicWorkQueueChain () {

            Assignment ass = new Assignment ("testBasicWorkQueueChain");
            var process = new testProcess ();
            EventBasedWorkerHost worker = new EventBasedWorkerHost (process, "testBasicWorkQueueChain");
            ass.Send (Encoding.UTF8.GetBytes ("testBasicWorkQueueChainMessage"));
            int cnt = 0;
            while (cnt < 5) {
                if (process.received == "testBasicWorkQueueChainMessage") {
                    Assert.True (true);
                    return;
                } else {
                    Thread.Sleep (1000);
                    cnt++;
                }
            }
            Assert.True (false);
        }

        [Fact]
        public void testSendWorkDelayed () {
            var worker = new WorkQueueTest ();

            worker.StartWorker ();
            new DelayedWorkRunnerScheduler ().Schedule ();
            Logger.ConditionalTrace (AppSettings.Instance.GetValue ("jubandistributed.messagingServer"));
            TestWorker.num = 0;
            var service = (IWorkQueueTest) DService.getWorkerService (typeof (IWorkQueueTest),
                new DistributedCallOptions () { IsPersistentAssignment = true });
            service.plus (1);
            //new DelayedWorkRunner().Run("JubanDistributed.Tests.TestWorker+IWorkQueueTest.sendTest");
            Task.Delay (5000);
            service.plus (1);
            int cnt = 0;
            while (cnt < 30) {
                Logger.ConditionalTrace ("TestWorker.num "+TestWorker.num);
                if (TestWorker.num == 2) {
                    Assert.True (true);
                    return;
                } else {
                    Thread.Sleep (1000);
                    cnt++;
                }
            }
            Logger.ConditionalTrace("Timeout: TestWorker.num "+TestWorker.num);
            Assert.True (false);
        }

        [Fact]
        public void testQuartz () { }

        [Fact]
        public void testCircuitBreaker () {
            Assignment ass = new Assignment ("testCircuitBreaker");
            ass.CircuitBreakerEvent += Ass_CircuitBreakerEvent;

            ass.BufferCheckRate = 1;
            ass.BufferCount = 1;

            ass.Send (Encoding.UTF8.GetBytes ("hello"));
            Thread.Sleep (1000);
            ass.Send (Encoding.UTF8.GetBytes ("hello"));
            int cnt = 0;
            while (cnt < 5 || ass.MessageCount > 0) {
                if (circuitBreakerHit) {
                    Assert.True (true);
                    return;
                } else {
                    Thread.Sleep (1000);
                    cnt++;
                }
            }
        }

        private EventBasedWorkerHost worker = null;
        private bool circuitBreakerHit = false;

        private void Ass_CircuitBreakerEvent (string str) {
            circuitBreakerHit = true;
            testProcess process = null;
            if (process == null) {
                process = new testProcess ();
                worker = new EventBasedWorkerHost (process, "testCircuitBreaker");
            }
        }
    }

}