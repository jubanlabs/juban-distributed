using System;
using System.Collections.Concurrent;
using System.Reflection;
using Jubanlabs.JubanShared.Common;
using NLog;

namespace Jubanlabs.JubanDistributed.RPC {
    public class UniversalRPCInterpreter : IRPCInterpreter {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger ();
        private ConcurrentDictionary<Type, object> objectFromInterface = new ConcurrentDictionary<Type, object> ();

        public byte[] Process (byte[] messsage) {
            try
            {
            ConditionalStopwatch.PunchIn ("UniversalRPCInterpreter");
            var mesgObj = (Message) Utils.Deserialize (messsage, typeof (Message));
            Logger.ConditionalTrace("UniversalRPCInterpreter "+mesgObj.type+" "+mesgObj.method);

            //"JubanDistributed.RPCInvokerCommon.Test.IRemoteUtilsProxy"
            string typeName = mesgObj.type;

            var interfaceType = RPCContext.TypesDict[typeName];
            Type type = TypesHelper.GetFirstImplementedTypeFromInterface (interfaceType);

            object obj;
            if (!objectFromInterface.TryGetValue (interfaceType, out obj)) {
                obj = Activator.CreateInstance (type);
                objectFromInterface[interfaceType] = obj;
            }

            object[] valuesArray = null;

            MethodInfo methodInfo;
            if (mesgObj.values != null) {
                valuesArray = new object[mesgObj.values.Length];

                var typesArray = new Type[mesgObj.types.Length];
                for (int i = 0; i < mesgObj.types.Length; i++) {
                    typesArray[i] = RPCContext.TypesDict[mesgObj.types[i]];
                    valuesArray[i] = Utils.Deserialize (mesgObj.values[i], typesArray[i]);
                }
                methodInfo = type.GetMethod (mesgObj.method, typesArray);
            } else {
                methodInfo = type.GetMethod (mesgObj.method);
            }

            var invokerSub = FastInvoke.GetMethodInvoker (methodInfo);

            ConditionalStopwatch.PunchOutAndIn ("UniversalRPCInterpreter");
            var result = invokerSub (obj, valuesArray);
            ConditionalStopwatch.PunchOutAndIn ("UniversalRPCInterpreter");
            
            ReturnMessage returnMessage = new ReturnMessage ();
            if (result != null) {
                returnMessage.type = result.GetType ().FullName;
                returnMessage.value = Utils.Serialize (result);
            }
            ConditionalStopwatch.PunchOut ("UniversalRPCInterpreter");
            return Utils.Serialize (returnMessage);
            }
            catch(Exception ex)
            {
                Logger.Error(ex,"process error : " + ex.StackTrace);
                throw ex;
            }
        }
    }

}