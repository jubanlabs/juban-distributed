using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Jubanlabs.JubanDistributed;
using Jubanlabs.JubanShared.Logging;
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
            cli.Main(args);
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