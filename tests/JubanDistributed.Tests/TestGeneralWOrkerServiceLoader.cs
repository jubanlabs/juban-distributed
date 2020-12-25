
using Jubanlabs.JubanShared.UnitTest;
using Xunit;
using Xunit.Abstractions;

namespace Jubanlabs.JubanDistributed.Tests {
    public class TestMongodb : IClassFixture<BaseFixture> {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();

        public TestMongodb (ITestOutputHelper outputHelper) {
            LoggingHelper.BindNLog (outputHelper);
        }

        // [Fact]
        // public void testLoader () {
        //     // var a = TestDatabase.Instance.testCollection.GetFirstCollection ();
        //     // long count = a.CountDocuments (new BsonDocument ());
        //     new GeneralWorkerServiceLoader ().LoadWorker ();

        // }


        
    }
}