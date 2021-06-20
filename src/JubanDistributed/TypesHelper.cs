using System;
using System.Collections.Concurrent;
using System.Linq;
using Castle.DynamicProxy;
using Jubanlabs.JubanDistributed.RPC;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;


namespace Jubanlabs.JubanDistributed {

    public class TypesHelper {
        private static ILogger<TypesHelper> Logger =  JubanLogger.GetLogger<TypesHelper>();
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
            }

            return typeFromInterface[interfaceType];

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

            Logger.LogTrace ("GetInterfaceFromClass - " + distributableInterface);
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