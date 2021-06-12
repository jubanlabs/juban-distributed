using System;
using NLog;

namespace Jubanlabs.JubanDistributed.RPC {
    public static class ExMethods {
       
private static Logger Logger = LogManager.GetCurrentClassLogger ();
        public static void StartRPCService (this IDistributable instance) {
           Type distributableInterface = TypesHelper.GetDistributableInterface(instance.GetType());
            if (distributableInterface == null) {
                Logger.ConditionalTrace ("worker interface not found");
                return;
            }
            var interpreter = new DedicatedRPCInterpreter();
            interpreter.Instance=instance;
            EventBasedRPCServiceHost worker = new EventBasedRPCServiceHost (interpreter, distributableInterface.FullName);
        }
    }
}