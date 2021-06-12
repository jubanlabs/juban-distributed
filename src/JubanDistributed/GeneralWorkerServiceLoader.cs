using System;
using System.Linq;
using Jubanlabs.JubanDistributed;
using Jubanlabs.JubanDistributed.RPC;
using Jubanlabs.JubanDistributed.WorkQueue;
using NLog;

public class GeneralWorkerServiceLoader {
    private static Logger Logger = LogManager.GetCurrentClassLogger ();
    public void LoadWorker () {
        var types = RPCContext.TypesDict.Values.Where (p => typeof (IWorkerService).IsAssignableFrom (p) &&
            p.IsClass && !p.IsAbstract
        ).ToArray ();
        foreach (var item in types) {
            var instance = Activator.CreateInstance (item);
            ((IDistributable) instance).StartWorker ();

            Logger.ConditionalTrace (item.FullName);
        }
    }
}