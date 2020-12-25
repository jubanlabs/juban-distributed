using System;
using Jubanlabs.JubanShared.Mongodb;

namespace Jubanlabs.JubanDistributed.WorkQueue {
    public class DelayedWorkDatabase : MgDatabase {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();

        private static readonly Lazy<DelayedWorkDatabase> lazy =
            new Lazy<DelayedWorkDatabase>
            (() => new DelayedWorkDatabase ());

        public static DelayedWorkDatabase Instance { get { return lazy.Value; } }

        protected override string GetDatabaseName()
        {
            return "delayed-work";
        }

        protected override string GetServerKey()
        {
            return "jubandistributed.delayedWorkerStorage.mongodb";
        }

        public override void InitCollections () {

        }
    }
}