using System;
using Castle.DynamicProxy;
using Jubanlabs.JubanShared.Common;

namespace Jubanlabs.JubanDistributed.RPC {
    public class RPCInvoker : IInterceptor {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();
        protected bool _enableRPC = true;
        protected RPCClient rpcClient;

        protected Type selfType;

        public RPCInvoker () { }
        public RPCInvoker (bool enableRPC) {
            _enableRPC = enableRPC;
            rpcClient = new UniversalRPCClient ();
            selfType = typeof (RPCInvoker);
        }
        private UniversalRPCInterpreter rpcInterpreter;
        public void Intercept(IInvocation invocation)
        {
            var methodName=invocation.Method.Name;
            ConditionalStopwatch.PunchIn ("RPCInvoker",methodName);


            var message = new Message ();
            message.type = TypesHelper.GetDistributableInterface(invocation.Proxy.GetType()).FullName;
            Logger.Info (message.type);
            message.method = methodName;
            if (invocation.Arguments != null) {
                message.types = new string[invocation.Arguments.Length];
                message.values = new byte[invocation.Arguments.Length][];
                for (int i = 0; i < invocation.Arguments.Length; i++) {
                    message.types[i] = invocation.Arguments[i].GetType ().FullName;
                    message.values[i] = Utils.Serialize (invocation.Arguments[i]);
                }
            }
            var mesg = Utils.Serialize (message);
            byte[] result;
            if (_enableRPC) {
                ConditionalStopwatch.PunchOutAndIn ("RPCInvoker",methodName+ " begin rpc call");
                result = rpcClient.CallAsync (mesg).Result;
                ConditionalStopwatch.PunchOutAndIn ("RPCInvoker",methodName+ " end rpc call");
            } else {
                if (rpcInterpreter == null) {
                    ConditionalStopwatch.PunchOutAndIn ("RPCInvoker",methodName + " begin local call");
                    rpcInterpreter = new UniversalRPCInterpreter ();
                    ConditionalStopwatch.PunchOutAndIn ("RPCInvoker",methodName+ " end local call");
                }
                result = rpcInterpreter.Process (mesg);
                ConditionalStopwatch.PunchOutAndIn ("RPCInvoker",methodName+ " local call, got result");
            }
            var returnMessage = Utils.Deserialize<ReturnMessage> (result);
             ConditionalStopwatch.PunchOutAndIn ("RPCInvoker",methodName+ " result got parsed");
            string typeName = returnMessage.type;
            if (typeName != null) {
                var types = RPCContext.TypesDict[typeName];

                ConditionalStopwatch.PunchOut ("RPCInvoker");
                invocation.ReturnValue= Utils.Deserialize (returnMessage.value, types);
            } else {
                ConditionalStopwatch.PunchOut ("RPCInvoker");
                invocation.ReturnValue= null;
            }
        }



    }

    public class UniversalRPCClient : RPCClient {
        public override string RPCName { get { return "universalrpc"; } }

    }

    public class DedicatedRPCClient : RPCClient {
        private string rpcName;
        public DedicatedRPCClient (string rpcName) {
            this.rpcName = rpcName;
        }
        public override string RPCName { get { return this.rpcName; } }

    }

    public class DedicatedRCPInvoker : RPCInvoker {
        public DedicatedRCPInvoker (bool enableRPC, string typeName) {
            _enableRPC = enableRPC;
            rpcClient = new DedicatedRPCClient (typeName);
            selfType = typeof (DedicatedRCPInvoker);
        }

    }

}