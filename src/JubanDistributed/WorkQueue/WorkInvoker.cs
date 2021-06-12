
using System;
using Castle.DynamicProxy;
using Jubanlabs.JubanDistributed.RPC;

namespace Jubanlabs.JubanDistributed.WorkQueue
{
    public class WorkInvoker : IInterceptor 
    {

        private bool _enableRPC = true;
        public WorkInvoker()
        {
            
        }

        public WorkInvoker(bool enableRPC)
        {
            _enableRPC = enableRPC;
        }
        private object _locker =new object();
        private UniversalWorkerInterpreter workerInterpreter;
        public void Intercept(IInvocation invocation)
        {
            
            var message=new Message();
            message.type=TypesHelper.GetDistributableInterface(invocation.Proxy.GetType()).FullName;
            message.method=invocation.Method.Name;;
            if (invocation.Arguments != null)
            {
                message.types = new string[invocation.Arguments.Length];
                message.values = new byte[invocation.Arguments.Length][];
                for (int i = 0; i < invocation.Arguments.Length; i++)
                {
                    message.types[i] = invocation.Arguments[i].GetType().FullName;
                    message.values[i] = Utils.Serialize(invocation.Arguments[i]);
                }
            }
            var mesg=Utils.Serialize(message);

            if (_enableRPC)
            {
                string queueName = message.type+"."+message.method;
                AssignmentManager.GetAssignment(queueName).Send(mesg);
            }
            else{
                if (workerInterpreter == null)
                {
                    lock (_locker)
                    {
                        if (workerInterpreter == null)
                        {
                            workerInterpreter = new UniversalWorkerInterpreter();
                            var interfaceType = RPCContext.TypesDict[message.type];
                            Type type = TypesHelper.GetFirstImplementedTypeFromInterface(interfaceType);
                            workerInterpreter.WorkerInstance = (IDistributable)Activator.CreateInstance(type);
                        }
                    }
                }
                workerInterpreter.Process(mesg);
            }
           
            
        }

    }
    
    

    
    
  
}
