using System;
using System.Linq;
using Jubanlabs.JubanDistributed;
using Jubanlabs.JubanDistributed.RPC;
using Jubanlabs.JubanDistributed.WorkQueue;

public class GeneralWorkerServiceLoader {
    private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();
    public void LoadWorker () {
        var types = RPCContext.TypesDict.Values.Where (p => typeof (IWorkerService).IsAssignableFrom (p) &&
            p.IsClass && !p.IsAbstract
        ).ToArray ();
        foreach (var item in types) {
            var instance = Activator.CreateInstance (item);
            Jubanlabs.JubanDistributed.WorkQueue.ExMethods.StartWorker ((IDistributable) instance);

            Logger.ConditionalTrace (item.FullName);
        }
    }
}