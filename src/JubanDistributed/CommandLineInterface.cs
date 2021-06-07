using System;
using CommandLine;
using Jubanlabs.JubanDistributed.RPC;
using Jubanlabs.JubanDistributed.WorkQueue;
using Jubanlabs.JubanShared.Common;
using NLog;
using Jubanlabs.JubanShared.Common.Config;
using Microsoft.Extensions.Configuration;

namespace Jubanlabs.JubanDistributed {
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

    public class CommandLineInterface {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();
        public void Main (string[] args) {
            /// loadservice worker
            /// loadservice rpc
            /// loadservice delayedworkrunner
            /// kickoff
            /// -f fork
            /// loadtask kickoff 
            /// loadtask resume

            

            //load all assemblies
            CommandLine.Parser.Default.ParseArguments<LoadServiceOptions, LoadTaskOptions> (args)
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
                        var options = (LoadServiceOptions) o;
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

                        System.Environment.Exit(1);
                    }
                });
        }

        public void SetLogTarget () {
            var config = new NLog.Config.LoggingConfiguration ();

            // Targets where to log to: File and Console

            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRuleForAllLevels (logconsole);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }
    }

}