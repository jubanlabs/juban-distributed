using System;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jubanlabs.JubanDistributed.Tests
{
    public class JubanTestBase
    {
        public JubanTestBase()
        {
            
            Environment.SetEnvironmentVariable("JUBAN_ENVIRONMENT_NAME", "testing");
            
            
            ILoggerFactory loggerFactory =
                LoggerFactory.Create(builder => builder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Trace));
            JubanLogger.SetLoggerFactory(loggerFactory);
            Console.WriteLine("JubanTestBase construction done");
        }
    }
}