using System;
using System.Collections.Generic;
using System.Reflection;
using CommandLine;
using Jubanlabs.JubanDistributed.WorkQueue;
using Jubanlabs.JubanShared.Common;
using Jubanlabs.JubanShared.Common.Config;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jubanlabs.JubanDistributed
{
    public class BaseOptions {
        [Option ('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option ('f', "fork", Required = false, HelpText = "Fork process.")]
        public bool Fork { get; set; }

        [Option ("environment", Default = "testing")]
        public string Environment { get; set; }

        
        
        [Option ("servicelibs", Required=false)]
        public string serviceLibs { get; set; }
        
        [Option ("classname", Required=false)]
        public string ClassName { get; set; }
    }

    [Verb ("loadservice", HelpText = "loadservice <worker|rpc|delayedworkrunner>")]
    class LoadServiceOptions : BaseOptions {
        //normal options here
        [Value (0, Required = true)]
        public string service { get; set; }
    }


    [Verb ("loadtask", HelpText = "loadtask <classname> <kickoff|resume> ")]
    class LoadTaskOptions: BaseOptions{
        [Value (0, Required = true)]
        public string className { get; set; }
        [Value (1, Required = true)]
        public string actionType { get; set; }
    }

    /// loadservice worker
    /// loadservice rpc
    /// loadservice delayedworkrunner
    /// -f fork
    /// loadtask kickoff  
    /// loadtask resume


    public interface ICommandLineInterface
    {
        void Main(IEnumerable<string> args);
    }
    public class CommandLineInterface : ICommandLineInterface{
        private static ILogger<CommandLineInterface> Logger =  JubanLogger.GetLogger<CommandLineInterface>();

        public void Main (IEnumerable<string> args) {

            
            Logger.LogInformation("JubanDistributed.CLI location: "+Assembly.GetExecutingAssembly().Location);
            Logger.LogInformation("JubanDistributed.CLI version: " + Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            Logger.LogInformation("JubanDistributed.CLI assembly version: " + Assembly.GetExecutingAssembly().GetName().Version);
            
            //load all assemblies
            Parser.Default.ParseArguments<LoadServiceOptions, LoadTaskOptions> (args)
                .WithParsed<BaseOptions> (o => {
                    if (o.Verbose) {
                        Logger.LogTrace ($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                        Logger.LogTrace ("Quick Start Example! App is in Verbose mode!");
                    } else {
                        Logger.LogTrace ($"Current Arguments: -v {o.Verbose}");
                        Logger.LogTrace ("Quick Start Example!");
                    }
                    if (!AppSession.Instance.IsEnvironmentNameSet() || !o.Environment.Equals("testing",StringComparison.OrdinalIgnoreCase)){
                        AppSession.Instance.SetEnvironmentName( o.Environment);
                    }

                    Logger.LogInformation("environment:" +AppSession.Instance.GetEnvironmentName());
                    IConfigurationRoot configlist =AppSettings.Instance.Config;
                    Logger.LogInformation(configlist.GetDebugView());
                    Logger.LogInformation((o is LoadServiceOptions).ToString());
                    if (o is LoadServiceOptions) {
                        var options = (LoadServiceOptions)o;
                        Logger.LogInformation(options.service);
                        if (options.service.Equals ("worker")) {
                            Logger.LogInformation("loading workers");
                            new GeneralWorkerServiceLoader ().LoadWorker ();
                        }

                        if (options.service.Equals("delayedworkrunner"))
                        {
                            Logger.LogTrace("delayedworkrunner");
                            new DelayedWorkRunnerScheduler().Schedule();
                        }
                    }

                    if (o is LoadTaskOptions)
                    {
                        var options = (LoadTaskOptions)o;
                        if (options.actionType.Equals("kickoff",StringComparison.Ordinal))
                        {
                            TypesHelper.NewAndInvoke(options.className, "FreshStart");
                        }

                        if (options.actionType.Equals("resume",StringComparison.Ordinal))
                        {
                            TypesHelper.NewAndInvoke(options.className, "Resume");
                        }
                    }

                    Console.WriteLine("Please press Ctrl+C to exit.");
                });
        }

    }

}