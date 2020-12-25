using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Jubanlabs.JubanDistributed.RPC;
using Jubanlabs.JubanDistributed.WorkQueue;

namespace Jubanlabs.JubanDistributed {
    public class DService {
        private static ProxyGenerator proxyGenerator= new ProxyGenerator();

        private static Type getInterfaceType (Type type) {
            Type distributableInterface = TypesHelper.GetDistributableInterface (type);

            if (distributableInterface == null) {
                throw new Exception ("distributable interface not found");
            }
            return distributableInterface;

        }

        public static IDistributable getRPCService (Type type) {
            return getRPCService (type, DistributedCallOptions.Default);
        }

        public static IDistributable getRPCService (Type type, DistributedCallOptions options) {
            var t = getInterfaceType (type);
            if (options.RPCQueueFormation == RPCQueueFormationEnum.UniversalRPCQueue) {
                return  (IDistributable)proxyGenerator.CreateInterfaceProxyWithoutTarget(t,new RPCInvoker(options.IsRemoteCall));
               // return (IDistributable) Jubanlabs.ProxyEmitter.ProxyEmitter.CreateProxy (typeof (RPCInvoker), getInterfaceType (type), options.IsRemoteCall);
            } else if (options.RPCQueueFormation == RPCQueueFormationEnum.PerService) {
                 return  (IDistributable)proxyGenerator.CreateInterfaceProxyWithoutTarget(t,new DedicatedRCPInvoker(options.IsRemoteCall,t.FullName));
               // return (IDistributable) Jubanlabs.ProxyEmitter.ProxyEmitter.CreateProxy (typeof (DedicatedRCPInvoker), t, options.IsRemoteCall, t.FullName);
            }
            return null;
        }

        public static IDistributable getWorkerService (Type type) {
            return getWorkerService (type, DistributedCallOptions.Default);
        }

        public static IDistributable getWorkerService (Type type, DistributedCallOptions options) {
            var t = getInterfaceType (type);    
            if (options.IsPersistentAssignment) {
                return  (IDistributable)proxyGenerator.CreateInterfaceProxyWithoutTarget(t,new WorkInvokerDelayed());
               // return (IDistributable) Jubanlabs.ProxyEmitter.ProxyEmitter.CreateProxy (typeof (WorkInvokerDelayed), getInterfaceType (type));
            } else {
                return  (IDistributable)proxyGenerator.CreateInterfaceProxyWithoutTarget(t,new WorkInvoker(options.IsRemoteCall));
                //return (IDistributable) Jubanlabs.ProxyEmitter.ProxyEmitter.CreateProxy (typeof (WorkInvoker), getInterfaceType (type), options.IsRemoteCall);
            }

        }
    }

    public class DistributedCallOptions {
        public bool IsRemoteCall { get; set; }
        public RPCQueueFormationEnum RPCQueueFormation { get; set; }
        public bool IsPersistentAssignment { get; set; }

        public DistributedCallOptions () {
            IsRemoteCall = true;
            IsPersistentAssignment = false;
            RPCQueueFormation = RPCQueueFormationEnum.PerService;
        }

        public static DistributedCallOptions Default = new DistributedCallOptions ();
    }

    public enum RPCQueueFormationEnum {
        UniversalRPCQueue,
        PerService,
    }


    
public static class Ext
{
    public static T AsTrackable<T>(this T instance) where T : class
    {
        return new ProxyGenerator().CreateClassProxyWithTarget
        (
          instance, 
          new PropertyChangeTrackingInterceptor()
        );
    }

    public static HashSet<string> GetChangedProperties<T>(this T instance) 
    where T : class
    {
        var proxy = instance as IProxyTargetAccessor;

        if (proxy != null)
        {
            var interceptor = proxy.GetInterceptors()
                                   .Select(i => i as IChangedProperties)
                                   .First();

            if (interceptor != null)
            {
                return interceptor.Properties;
            }
        }

        return new HashSet<string>();
    }
}

interface IChangedProperties
{
    HashSet<string> Properties { get; }
}

public class PropertyChangeTrackingInterceptor : IInterceptor, IChangedProperties
{
    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();

        this.Properties.Add(invocation.Method.Name);
    }

    private HashSet<string> properties = new HashSet<string>();

    public HashSet<string> Properties
    {
        get { return this.properties; }
        private set { this.properties = value; }
    }
}

}