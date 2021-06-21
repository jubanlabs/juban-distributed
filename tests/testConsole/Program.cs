using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Jubanlabs.JubanDistributed;
using Jubanlabs.JubanShared.Common.Config;
using Jubanlabs.JubanShared.Logging;
using JubanShared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace testConsole {
    class Program {
        static async Task Main (string[] args)
        {
            
            var host = CreateHostBuilder(args).Build().JubanWireUp().JubanDistributedCLI(args);
          
            await host.RunAsync();;
        }
       
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, collection) =>
                {
                    //services.AddHostedService<Worker>();
                });
    }

  
}