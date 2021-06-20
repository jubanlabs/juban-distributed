using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jubanlabs.JubanDistributed
{
    public class JubanLogger
    {
        private static ILoggerFactory lf ;

        public static void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            lf = loggerFactory;
        }

        public static ILogger<T> GetLogger<T>()
        {
            return lf == null ? NullLogger<T>.Instance : lf.CreateLogger<T>();
        }
    }
}