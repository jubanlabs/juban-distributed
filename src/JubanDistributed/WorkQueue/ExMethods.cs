using System;
using System.Collections.Generic;
using System.Reflection;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;


namespace Jubanlabs.JubanDistributed.WorkQueue {
    public static class ExMethods {
        private static ILogger Logger =  JubanLogger.GetLogger(typeof(ExMethods).FullName);
        public static void StartWorker (this IDistributable instance) {
            Type distributableInterface =TypesHelper.GetDistributableInterface(instance.GetType());
            if (distributableInterface == null) {
                Logger.LogTrace ("worker interface not found");
                return;
            }

            var methods = GetMethods(distributableInterface);

            List<string> queueNameList = new List<string> ();
            foreach (var method in methods) {
                if (method.IsPublic) {
                    queueNameList.Add (distributableInterface.FullName + "." + method.Name);
                }

            }

            foreach (var queueName in queueNameList) {
                var workerInterpreter = new UniversalWorkerInterpreter ();
                workerInterpreter.WorkerInstance = instance;
                workerInterpreter.queueName = queueName;
                EventBasedWorkerHost worker = new EventBasedWorkerHost (workerInterpreter, queueName);
                Logger.LogTrace (queueName);
            }
        }

        private static IEnumerable<MethodInfo> GetMethods(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                                       BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                yield return method;
            }
            if (type.IsInterface)
            {
                foreach (var iface in type.GetInterfaces())
                {
                    foreach (var method in GetMethods(iface))
                    {
                        yield return method;
                    }
                }
            }
        }

    }
}