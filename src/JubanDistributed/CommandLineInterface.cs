using System;
using System.Collections.Generic;
using System.Reflection;
using CommandLine;
using Jubanlabs.JubanDistributed.WorkQueue;
using Jubanlabs.JubanShared.Common;
using Jubanlabs.JubanShared.Common.Config;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Config;
using NLog.Targets;

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

    public class CommandLineInterface {
        private static Logger Logger = LogManager.GetCurrentClassLogger ();

        public void Main (IEnumerable<string> args) {

            
            Logger.Info("JubanDistributed.CLI location: "+Assembly.GetExecutingAssembly().Location);
            Logger.Info("JubanDistributed.CLI version: " + Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            Logger.Info("JubanDistributed.CLI assembly version: " + Assembly.GetExecutingAssembly().GetName().Version);
            
            //load all assemblies
            Parser.Default.ParseArguments<LoadServiceOptions, LoadTaskOptions> (args)
                .WithParsed<BaseOptions> (o => {
                    if (o.Verbose) {
                        Logger.ConditionalTrace ($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                        Logger.ConditionalTrace ("Quick Start Example! App is in Verbose mode!");
                    } else {
                        Logger.ConditionalTrace ($"Current Arguments: -v {o.Verbose}");
                        Logger.ConditionalTrace ("Quick Start Example!");
                    }
                    if (!AppSession.Instance.IsEnvironmentNameSet() || !o.Environment.Equals("testing",StringComparison.OrdinalIgnoreCase)){
                        AppSession.Instance.SetEnvironmentName( o.Environment);
                    }

                    Logger.Info("environment:" +AppSession.Instance.GetEnvironmentName());
                    IConfigurationRoot configlist =AppSettings.Instance.Config;
                    Logger.Info(configlist.GetDebugView());
                    
                    if (o is LoadServiceOptions) {
                        var options = (LoadServiceOptions)o;
                        if (options.service.Equals ("worker")) {
                            Logger.ConditionalTrace ("loading workers");
                            new GeneralWorkerServiceLoader ().LoadWorker ();
                        }

                        if (options.service.Equals("delayedworkrunner"))
                        {
                            Logger.ConditionalTrace("delayedworkrunner");
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

        public void SetLogTarget () {
            var config = new LoggingConfiguration ();

            // Targets where to log to: File and Console

            var logconsole = new ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRuleForAllLevels (logconsole);

            // Apply config           
            LogManager.Configuration = config;
        }
    }

}