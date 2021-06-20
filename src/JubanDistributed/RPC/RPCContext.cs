using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;


namespace Jubanlabs.JubanDistributed.RPC
{
    public static class RPCContext
    {
        private static ILogger Logger =  JubanLogger.GetLogger(typeof(RPCContext).FullName);
        public static Dictionary<string, Type> TypesDict;
        static RPCContext()
        {
            TypesDict=new Dictionary<string, Type>();
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s =>
                {
                    try
                    {
                        return s.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        Logger.LogTrace(s.GetName() + " can not be loaded" +e.Message+"\n"+e.StackTrace);
                        return new Type[0];
                    }
                })
                .ToList();
            foreach (var element in types)
            {
                TypesDict[element.FullName] = element;
            }
        }
    }
}
