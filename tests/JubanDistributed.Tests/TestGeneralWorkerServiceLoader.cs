
using System;
using System.Linq;
using Jubanlabs.JubanDistributed.RPC;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jubanlabs.JubanDistributed.Tests
{
    [TestClass]
    public class TestMongodb :JubanTestBase
    {
        private static ILogger<TestMongodb> Logger =  JubanLogger.GetLogger<TestMongodb>();

      

        // [TestMethod]
        // public void testLoader () {
        //     // var a = TestDatabase.Instance.testCollection.GetFirstCollection ();
        //     // long count = a.CountDocuments (new BsonDocument ());
        //     new GeneralWorkerServiceLoader ().LoadWorker ();

        // }

        [TestMethod]
        public void testKickoff()
        {
            
            JubanLogger.GetLogger<KickoffTest>().LogInformation("abx");
            TypesHelper.NewAndInvoke("KickoffTest", "FreshStart");
            Assert.AreEqual(1, freshStartCount);

            TypesHelper.NewAndInvoke("KickoffTest", "Resume");
            TypesHelper.NewAndInvoke("KickoffTest", "Resume");
            
            Assert.AreEqual(2, resumeCount);
        }
        public static int freshStartCount = 0;
        public static int resumeCount = 0;
    }

    public class KickoffTest : IKickoff
    {
        private static ILogger<KickoffTest> Logger =  JubanLogger.GetLogger<KickoffTest>();
        public void FreshStart()
        {
            Logger.LogInformation("freshstart consumed");
            TestMongodb.freshStartCount++;
        }

        public void Resume()
        {
            Logger.LogInformation("resume consumed");
            TestMongodb.resumeCount++;
        }
    }
}