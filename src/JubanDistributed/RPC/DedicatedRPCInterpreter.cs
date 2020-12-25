using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jubanlabs.JubanDistributed.RPC {
    public class DedicatedRPCInterpreter : IRPCInterpreter {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();
        private ConcurrentDictionary<Type, object> objectFromInterface = new ConcurrentDictionary<Type, object> ();

        public IDistributable Instance { get; internal set; }

        public byte[] Process (byte[] messsage) {
            var mesgObj = (Message) Utils.Deserialize (messsage, typeof (Message));

            object[] valuesArray = null;

            MethodInfo methodInfo;
            if (mesgObj.values != null) {
                valuesArray = new object[mesgObj.values.Length];

                var typesArray = new Type[mesgObj.types.Length];
                for (int i = 0; i < mesgObj.types.Length; i++) {
                    typesArray[i] = RPCContext.TypesDict[mesgObj.types[i]];
                    valuesArray[i] = Utils.Deserialize (mesgObj.values[i], typesArray[i]);
                }
                methodInfo = Instance.GetType ().GetMethod (mesgObj.method, typesArray);
            } else {
                methodInfo = Instance.GetType ().GetMethod (mesgObj.method);
            }

            var invokerSub = FastInvoke.GetMethodInvoker (methodInfo);

            var result = invokerSub (Instance, valuesArray);
            ReturnMessage returnMessage = new ReturnMessage ();
            if (result != null) {
                returnMessage.type = result.GetType ().FullName;
                returnMessage.value = Utils.Serialize (result);
            }
            return Utils.Serialize (returnMessage);
        }
    }

}