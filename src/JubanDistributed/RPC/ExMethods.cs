using System;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;


namespace Jubanlabs.JubanDistributed.RPC {
    public static class ExMethods {
       
private static ILogger Logger =  JubanLogger.GetLogger(typeof(ExMethods).FullName);
        public static void StartRPCService (this IDistributable instance) {
           Type distributableInterface = TypesHelper.GetDistributableInterface(instance.GetType());
            if (distributableInterface == null) {
                Logger.LogTrace ("worker interface not found");
                return;
            }
            var interpreter = new DedicatedRPCInterpreter();
            interpreter.Instance=instance;
            EventBasedRPCServiceHost worker = new EventBasedRPCServiceHost (interpreter, distributableInterface.FullName);
        }
    }
}