using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Jubanlabs.JubanDistributed;
using Jubanlabs.JubanShared.Common.Config;
using Jubanlabs.JubanShared.Logging;
using JubanShared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace testConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build().JubanWireUp().JubanDistributedCLI(args);

            await host.RunAsync();
            ;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostContext, loggingBuilder) => { loggingBuilder.SetMinimumLevel(LogLevel.Trace); })
                .ConfigureServices((hostContext, collection) =>
                {
                    //services.AddHostedService<Worker>();
                });
    }

    public interface IWorkQueueTest : IWorkerService
    {
        void sendTest(String str);
        void plus(int n);
    }


    public class WorkQueueTest :
        IWorkQueueTest
    {
        private static readonly ILogger<WorkQueueTest> Logger = JubanLogger.GetLogger<WorkQueueTest>();

        public void sendTest(string str)
        {
            Logger.LogTrace("thread id" + Thread.CurrentThread.ManagedThreadId + " : " + str);
        }

        public void plus(int n)
        {
        }
    }
}