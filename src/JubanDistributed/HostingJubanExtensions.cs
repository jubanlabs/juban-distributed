using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jubanlabs.JubanDistributed
{
    public static class HostingJubanExtensions
    {
        public static IHost JubanWireup(this IHost host)
        {
            var lf = host.Services.GetService<ILoggerFactory>();
            JubanLogger.SetLoggerFactory(lf);
            return host;
        }
    }
}