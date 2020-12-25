using System;
using Castle.DynamicProxy;
using Jubanlabs.JubanDistributed;

namespace testConsole {
    class Program {
        static void Main (string[] args) {
            var cml =new CommandLineInterface();
            cml.SetLogTarget();
            cml.Main(args);
            
 var gen=new ProxyGenerator();
 var worker = (ICalcWorker)gen.CreateInterfaceProxyWithoutTarget(typeof(ICalcWorker),new TestingInterceptor());
 Console.WriteLine(worker.plus(2,3));
 worker.noReturn(4);
            Console.WriteLine("Press any key to close the application");
            Console.ReadKey();
        }
       
    }


public class TestingInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
       Console.WriteLine("abc");
       
foreach (var item in invocation.Proxy.GetType().GetInterfaces())
{
    Console.WriteLine(item.FullName);
}
        var methodInfo=invocation.Method;
        Console.WriteLine(methodInfo.ReturnType.FullName);
        if(methodInfo.ReturnType!=typeof(void)){
        invocation.ReturnValue=4;
        }
    }

}
  
}