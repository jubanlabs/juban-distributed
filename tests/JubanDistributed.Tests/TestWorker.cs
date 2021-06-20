using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jubanlabs.JubanDistributed.WorkQueue;
using Jubanlabs.JubanShared.Common.Config;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quartz;
using Quartz.Impl;
using RabbitMQ.Client;

namespace Jubanlabs.JubanDistributed.Tests {
    [TestClass]
    public class TestWorker :JubanTestBase {
        private static ILogger<TestWorker> Logger =  JubanLogger.GetLogger<TestWorker>();


        public interface IWorkQueueTest : IWorkerService {

            void sendTest (String str);
            void plus (int n);
        }

        public static Boolean flag1 = false;
        public static Boolean flag2 = false;

        public static int num = 0;

        public class WorkQueueTest : 
        IWorkQueueTest {
            private static readonly ILogger<WorkQueueTest> Logger =  JubanLogger.GetLogger<WorkQueueTest>();
            public void sendTest (string str) {
                if (str.Equals ("flag1")) {
                    TestWorker.flag1 = true;
                }

                if (str.Equals ("flag2")) {
                    TestWorker.flag2 = true;
                }
                Logger.LogTrace ("thread id" + Thread.CurrentThread.ManagedThreadId + " : " + str);
            }

            public void plus (int n) {
                Logger.LogTrace ("plus " + n);
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

        [TestMethod]
        public void testPoolingLease () {

        }

        [TestMethod]
        public void testSendWorkQueue () {
            TestWorker.flag1 = false;
            var worker = new WorkQueueTest ();

            worker.StartWorker ();

            var service = (IWorkQueueTest) DService.getWorkerService (worker.GetType (), new DistributedCallOptions () { IsPersistentAssignment = false });
            service.sendTest ("flag1");

            int cnt = 0;
            while (cnt < 5) {
                if (TestWorker.flag1) {
                    Assert.IsTrue (true);
                    return;
                } else {
                    Thread.Sleep (1000);
                    cnt++;
                }
            }
            Assert.IsTrue (false);
        }

        [TestMethod]
        public void testSendWorkQueueLocal () {
            Logger.LogTrace (AppSettings.Instance.GetValue ("jubandistributed.messagingServer"));
            TestWorker.flag2 = false;
            var service = (IWorkQueueTest) DService.getWorkerService (typeof (IWorkQueueTest),
                new DistributedCallOptions () { IsRemoteCall = false });
            service.sendTest ("flag2");
            Logger.LogTrace ("abc");
            Assert.IsTrue (TestWorker.flag2);
        }

        [TestMethod]
        public void testBasicWorkQueueChain () {

            Assignment ass = new Assignment ("testBasicWorkQueueChain");
            var process = new testProcess ();
            EventBasedWorkerHost worker = new EventBasedWorkerHost (process, "testBasicWorkQueueChain");
            ass.Send (Encoding.UTF8.GetBytes ("testBasicWorkQueueChainMessage"));
            int cnt = 0;
            while (cnt < 5) {
                if (process.received == "testBasicWorkQueueChainMessage") {
                    Assert.IsTrue (true);
                    return;
                } else {
                    Thread.Sleep (1000);
                    cnt++;
                }
            }
            Assert.IsTrue (false);
        }

        [TestMethod]
        public void testSendWorkDelayed () {
            var worker = new WorkQueueTest ();
            worker.StartWorker ();

            TestWorker.num = 0;
            var service = (IWorkQueueTest) DService.getWorkerService (typeof (IWorkQueueTest),
                new DistributedCallOptions () { IsPersistentAssignment = true });
            service.plus (1);
            Logger.LogTrace("plus 1");
            service.plus (1);
            Logger.LogTrace("plus 1");

            Thread.Sleep (1000);
            new DelayedWorkRunnerScheduler ().Schedule ();
            
            int cnt = 0;
            while (cnt < 20) {
                Logger.LogTrace ("TestWorker.num "+TestWorker.num);
                if (TestWorker.num == 2) {
                    Assert.IsTrue (true);
                    return;
                } else {
                    Thread.Sleep (1000);
                    cnt++;
                }
            }
            Logger.LogTrace("Timeout: TestWorker.num "+TestWorker.num);
            Assert.IsTrue (false);
        }

        [TestMethod]
        public void testQuartz () { }

        [TestMethod]
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
                    Assert.IsTrue (true);
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