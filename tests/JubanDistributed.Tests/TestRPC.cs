using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Jubanlabs.JubanDistributed.RPC;
using Jubanlabs.JubanShared.Common;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;

namespace Jubanlabs.JubanDistributed.Tests {

    public interface IRPCTestService : IRPCService {
        int SimpleTestReturnInputInteger (int input);
    }

    public class RPCTestService : IRPCTestService {
        public int SimpleTestReturnInputInteger (int input) {
            return input;
        }
    }

    public interface IRemoteUtils : IRPCService {
        string MethodWithSimpleReturnObject (String abc);
        DummySubClass MethodWithReturnObject (int val);
        // void MethodWithNoReturnObject (String abc);
        // void MethodWithNoParameterAndNoReturnObject ();
        // string VerifyMethodWithNoReturnObject ();
        // string VerifyMethodWithNoParameterAndNoReturnObject ();
        DummySubClass MethodWithParameterObjectAndReturnObject (DummySubClass val);
    }

    public class RemoteUtils : IRemoteUtils {

        private static readonly ILogger<RemoteUtils> Logger =  JubanLogger.GetLogger<RemoteUtils>();
        public string MethodWithSimpleReturnObject (String abc) {
            // Stopwatch stopWatch = Stopwatch.StartNew ();
            // Logger.LogTrace ("begin rpc function sleep");
            // Task.Delay (3000).Wait ();
            // stopWatch.Stop ();
            // TimeSpan timespan = stopWatch.Elapsed;
            // Logger.LogTrace ("end rpc function sleep, time spent:" + timespan.TotalMilliseconds);
            return abc;
        }

        public DummySubClass MethodWithReturnObject (int val) {
            DummySubClass dummy = new DummySubClass ();
            dummy.Prop1 = val;
            return dummy;
        }

        // private string str1;
        // public void MethodWithNoReturnObject (string abc) {
        //     str1 = abc;
        // }

        // private string str2;
        // public void MethodWithNoParameterAndNoReturnObject () {
        //     str2 = "MethodWithNoParameterAndNoReturnObject";
        // }

        public DummySubClass MethodWithParameterObjectAndReturnObject (DummySubClass val) {
            return val;
        }

        // public string VerifyMethodWithNoReturnObject () {
        //     return str1;
        // }

        // public string VerifyMethodWithNoParameterAndNoReturnObject () {
        //     return str2;
        // }
    }

    delegate int delType (int a, string b);

    [TestClass]
    public class TestRPC :JubanTestBase{
        private static readonly ILogger<TestRPC> Logger =  JubanLogger.GetLogger<TestRPC>();


        private void consumeRPC (IRemoteUtils service) {
            Stopwatch stopWatch = Stopwatch.StartNew ();

            //var service =new RemoteUtils();
            var task = Task.Run (() => {
                TimeSpan s1 = stopWatch.Elapsed;
                Logger.LogTrace ("a begin " + s1.TotalMilliseconds);
                //Task.Delay(500).Wait();
                String str = service.MethodWithSimpleReturnObject ("xxx");
                TimeSpan s2 = stopWatch.Elapsed;
                // stopWatch.Restart ();

                Logger.LogTrace ("a end " + s2.TotalMilliseconds);
                Assert.AreEqual ("xxx", str);
            });

            DummySubClass dummy = service.MethodWithReturnObject (111);

            Assert.AreEqual (111, dummy.Prop1);
            dummy.Prop1 = 234;
            Assert.AreEqual (234, service.MethodWithParameterObjectAndReturnObject (dummy).Prop1);
            dummy.Prop1 = 235;
            Assert.AreEqual (235, service.MethodWithParameterObjectAndReturnObject (dummy).Prop1);
            // Assert.Null (service.VerifyMethodWithNoReturnObject ());
            //Assert.Null (service.VerifyMethodWithNoParameterAndNoReturnObject ());
            // service.MethodWithNoParameterAndNoReturnObject ();
            // Assert.AreEqual ("MethodWithNoParameterAndNoReturnObject", service.VerifyMethodWithNoParameterAndNoReturnObject ());
            //  service.MethodWithNoReturnObject ("MethodWithNoReturnObject");
            // Logger.LogTrace (service.VerifyMethodWithNoReturnObject ());
            //Assert.AreEqual ("MethodWithNoReturnObject", service.VerifyMethodWithNoReturnObject ());
            dummy.Prop1 = 236;
            Assert.AreEqual (236, service.MethodWithParameterObjectAndReturnObject (dummy).Prop1);
            dummy = service.MethodWithReturnObject (111);
            Assert.AreEqual (111, dummy.Prop1);
            dummy.Prop1 = 237;
            Assert.AreEqual (237, service.MethodWithParameterObjectAndReturnObject (dummy).Prop1);
            Logger.LogTrace ("xxx");
            task.Wait ();
            stopWatch.Stop ();
            TimeSpan timespan = stopWatch.Elapsed;
            Logger.LogTrace ("end, time spent:" + timespan.TotalMilliseconds);

        }

        [TestMethod]
        public void testRPCTestService () {
            var serviceProvider = new RPCTestService ();
            serviceProvider.StartRPCService ();
            ConditionalStopwatch.PunchIn ("t1");
            Task.Delay (1000).Wait ();
            IRPCTestService service = (IRPCTestService) DService.getRPCService (typeof (IRPCTestService),
                new DistributedCallOptions () { RPCQueueFormation = RPCQueueFormationEnum.PerService });

            ConditionalStopwatch.PunchOutAndIn ("t1");

            Assert.AreEqual (123, service.SimpleTestReturnInputInteger (123));
            ConditionalStopwatch.PunchOutAndIn ("t1");
            Assert.AreEqual (123, service.SimpleTestReturnInputInteger (123));
            ConditionalStopwatch.PunchOutAndIn ("t1");
            Assert.AreEqual (123, service.SimpleTestReturnInputInteger (123));

            ConditionalStopwatch.PunchOut ("t1");

            // Debug.Assert()
        }

        [TestMethod]
        public void testRPCClientWithRPCServer () {
            UniversalRPCServer.Start ();
            UniversalRPCServer.Start ();
            UniversalRPCServer.Start ();
            UniversalRPCServer.Start ();
            UniversalRPCServer.Start ();

            var service = (IRemoteUtils) DService.getRPCService (typeof (IRemoteUtils),
                new DistributedCallOptions () { IsRemoteCall = true, RPCQueueFormation = RPCQueueFormationEnum.UniversalRPCQueue });
            //consumeRPC (service);
            int[] ids = new [] { 1, 2, 3, 4, 5 };
            Task.WhenAll (ids.Select (i => Task.Run (() => { consumeRPC (service); }))).Wait ();
            return;
        }

        [TestMethod]
        public void testRPCClientWithoutRPCServer () {

            var service = (IRemoteUtils) DService.getRPCService (typeof (IRemoteUtils),
                new DistributedCallOptions () { IsRemoteCall = false, RPCQueueFormation = RPCQueueFormationEnum.UniversalRPCQueue });
            consumeRPC (service);
        }

        [TestMethod]
        public void testInvoker () {
            // while(!Debugger.IsAttached) Thread.Sleep(500);
            //
            //            Type t2 = service.GetType();
            //            MethodInfo m2 = t2.GetMethod("");
            //            var delegateType= Expression.GetFuncType(typeof(int));
            //           // dynamic getter = m2.CreateDelegate();
            //         //   getter("", "");
            var obj = new DummyClass ();
            var dummyClass = Type.GetType ("Jubanlabs.JubanDistributed.Tests.DummyClass");
            Logger.LogTrace ("zzz");
            Logger.LogTrace (dummyClass.ToString ());
            var method = dummyClass.GetMethod ("DummyCall", new Type[] { typeof (int), typeof (String) });
            var method1 = dummyClass.GetMethod ("DummyCall", new Type[] { typeof (DummySubClass) });
            var m = (delType) method.CreateDelegate (typeof (delType), obj);
            Logger.LogTrace (m (3, "5").ToString ());
            Func<int, int> twice = x => x * 2;
            const int LOOP = 5000000; // 5M
            var watch = Stopwatch.StartNew ();
            for (int i = 0; i < LOOP; i++) {
                twice.Invoke (3);
            }
            watch.Stop ();
            Logger.LogTrace ("Invoke: {0}ms", watch.ElapsedMilliseconds);
            watch = Stopwatch.StartNew ();
            for (int i = 0; i < LOOP; i++) {
                twice.DynamicInvoke (3);
            }
            watch.Stop ();
            Logger.LogTrace ("DynamicInvoke: {0}ms", watch.ElapsedMilliseconds);

            watch = Stopwatch.StartNew ();
            for (int i = 0; i < LOOP; i++) {
                m (5, "");
            }
            watch.Stop ();
            Logger.LogTrace ("delegate: {0}ms", watch.ElapsedMilliseconds);

            object[] objarr = new object[] { 5, "" };
            watch = Stopwatch.StartNew ();
            for (int i = 0; i < LOOP; i++) {
                method.Invoke (obj, objarr);
            }
            watch.Stop ();
            Logger.LogTrace ("method.invoke: {0}ms", watch.ElapsedMilliseconds);

            var invoker = FastInvoke.GetMethodInvoker (method);
            watch = Stopwatch.StartNew ();
            for (int i = 0; i < LOOP; i++) {

                invoker (obj, objarr);
            }
            watch.Stop ();
            Logger.LogTrace ("fastinvoker: {0}ms", watch.ElapsedMilliseconds);

            var invokerSub = FastInvoke.GetMethodInvoker (method1);
            var sub = new DummySubClass ();
            sub.Prop1 = 271;
            Logger.LogTrace (invokerSub (obj, new object[] { sub }).ToString ());

            // var service =DService.newRPCService<IDummyClass> (false);        
            // watch = Stopwatch.StartNew ();
            // for (int i = 0; i < LOOP; i++) {
            //     service.DummyCall(5,"");

            // }
            // watch.Stop ();
            // Logger.LogTrace ("fastinvoker: {0}ms", watch.ElapsedMilliseconds);
        }

        public object TypeConvert (object source, Type DestType) {

            object NewObject = System.Convert.ChangeType (source, DestType);

            return (NewObject);
        }
    }

    [ProtoContract]
    public class DummySubClass {
        [ProtoMember (1)]
        public int Prop1 { get; set; }
    }

    public interface IDummyClass {
        int DummyCall (int a, String b);
    }
    public class DummyClass : IDummyClass {
        public int DummyCall (int a, String b) {
            return 555;
        }

        public int DummyCall (DummySubClass sub) {
            return sub.Prop1;
        }

        //  public int DummyCall(DummySubClass sub,int a){
        //     return sub.Prop1+a;
        // }
private static ILogger<DummyClass> Logger =  JubanLogger.GetLogger<DummyClass>();
        public void DummyCall (DummySubClass sub, int a) {
            Logger.LogTrace ("xxx");
        }
    }
}