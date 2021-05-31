
using System;
using System.Linq;
using Jubanlabs.JubanDistributed.RPC;
using Jubanlabs.JubanShared.UnitTest;
using Xunit;
using Xunit.Abstractions;

namespace Jubanlabs.JubanDistributed.Tests
{
    public class TestMongodb : IClassFixture<BaseFixture>
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public TestMongodb(ITestOutputHelper outputHelper)
        {
            LoggingHelper.BindNLog(outputHelper);
        }

        // [Fact]
        // public void testLoader () {
        //     // var a = TestDatabase.Instance.testCollection.GetFirstCollection ();
        //     // long count = a.CountDocuments (new BsonDocument ());
        //     new GeneralWorkerServiceLoader ().LoadWorker ();

        // }

        [Fact]
        public void testKickoff()
        {
            TypesHelper.NewAndInvoke("KickoffTest", "FreshStart");
            Assert.Equal(1, freshStartCount);

            TypesHelper.NewAndInvoke("KickoffTest", "Resume");
            TypesHelper.NewAndInvoke("KickoffTest", "Resume");
            
            Assert.Equal(2, resumeCount);
        }
        public static int freshStartCount = 0;
        public static int resumeCount = 0;
    }

    public class KickoffTest : IKickoff
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public void FreshStart()
        {
            Logger.Info("freshstart consumed");
            TestMongodb.freshStartCount++;
        }

        public void Resume()
        {
            Logger.Info("resume consumed");
            TestMongodb.resumeCount++;
        }
    }
}