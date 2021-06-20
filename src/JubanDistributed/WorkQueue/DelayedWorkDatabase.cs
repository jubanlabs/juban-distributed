using System;
using Jubanlabs.JubanShared.Logging;
using Jubanlabs.JubanShared.Mongodb;
using Microsoft.Extensions.Logging;


namespace Jubanlabs.JubanDistributed.WorkQueue {
    public class DelayedWorkDatabase : MgDatabase {
        private static ILogger<DelayedWorkDatabase> Logger =  JubanLogger.GetLogger<DelayedWorkDatabase>();

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