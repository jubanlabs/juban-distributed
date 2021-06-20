using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Jubanlabs.JubanDistributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace testConsole {
    class Program {
        static async Task Main (string[] args)
        {
            var host = CreateHostBuilder(args).Build().JubanWireup();
            
            var cli = host.Services.GetService<ICommandLineInterface>();
            var config =host.Services.GetService<IConfiguration>();
            var logger = JubanLogger.GetLogger<Program>();
            logger.LogInformation("hello from juban logger");
            
            Console.WriteLine(config["jubandistributed.delayedWorkerStorage.mongodb"]);
            var hostTask= host.RunAsync();
            Console.WriteLine("abc");
            await hostTask;
        }
       
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, collection) =>
                {
                    collection.AddSingleton<ICommandLineInterface, CommandLineInterface>();
                    //services.AddHostedService<Worker>();
                });
    }

  
}