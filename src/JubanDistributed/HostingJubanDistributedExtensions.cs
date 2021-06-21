using Jubanlabs.JubanDistributed;

namespace JubanShared
{
    using Jubanlabs.JubanShared.Common.Config;
    using Jubanlabs.JubanShared.Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public static class HostingJubanDistributedExtensions
    {
        public static IHost JubanDistributedCLI(this IHost host, string[] args)
        {
            var cli = new CommandLineInterface();
            cli.Main(args);
            return host;
        }
    }
}