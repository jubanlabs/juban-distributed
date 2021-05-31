using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using Jubanlabs.JubanDistributed.RPC;
using Jubanlabs.JubanDistributed.WorkQueue;

namespace Jubanlabs.JubanDistributed {

    public class TypesHelper {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();
        private static ConcurrentDictionary<Type, Type> typeFromInterface = new ConcurrentDictionary<Type, Type> ();
        public static Type GetFirstImplementedTypeFromInterface (Type interfaceType) {
            Type type;
            if (!typeFromInterface.TryGetValue (interfaceType, out type)) {

                var types = RPCContext.TypesDict.Values.Where (p => interfaceType.IsAssignableFrom (p) &&
                    p.IsClass && !p.IsAbstract
                ).ToArray ();

                typeFromInterface[interfaceType] = types[0];
                type = types[0];
                return type;
            } else {
                return typeFromInterface[interfaceType];
            }

        }

        public static Type GetDistributableInterface (Type type) {
            if (type.IsInterface && typeof (IDistributable).IsAssignableFrom (type)) {
                return type;
            }
            var interfaces = type.GetInterfaces ().Except (type.BaseType.GetInterfaces ()).ToList ().ToArray ();
            Type distributableInterface = null;

            for (int i = 0; i < interfaces.Length; i++) {
                if (typeof (IDistributable).IsAssignableFrom (interfaces[i]) &&
                    !(interfaces[i].Equals (typeof (IDistributable))) &&
                    !(interfaces[i].Equals (typeof (IProxyTargetAccessor))) &&
                    !(interfaces[i].Equals (typeof (IWorkerService))) &&
                    !(interfaces[i].Equals (typeof (IRPCService)))
                ) {
                    distributableInterface = interfaces[i];
                    break;

                }
            }

            Logger.ConditionalTrace ("GetInterfaceFromClass - " + distributableInterface);
            return distributableInterface;
        }

        public static void NewAndInvoke(string classname,string methodname)
        {
            var type = RPCContext.TypesDict.Values.Where(s => s.Name == classname).First();
            var methodInfo = type.GetMethod(methodname);
            var invokerSub = FastInvoke.GetMethodInvoker(methodInfo);
            var instance = Activator.CreateInstance(type);
            invokerSub.Invoke(instance, new object[0]);
        }
    }
}