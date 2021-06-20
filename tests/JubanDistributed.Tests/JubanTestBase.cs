using System;
using Jubanlabs.JubanShared.Common.Config;
using Jubanlabs.JubanShared.Logging;
using JubanShared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jubanlabs.JubanDistributed.Tests
{
    public class JubanTestBase
    {
        public JubanTestBase()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "testing");
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((hostContext, loggingBuilder) => { loggingBuilder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Trace); })
                .Build().JubanWireUp();
            Console.WriteLine("JubanTestBase construction done");
        }
    }
}