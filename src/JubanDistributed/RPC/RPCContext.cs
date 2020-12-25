using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Jubanlabs.JubanDistributed.RPC
{
    public static class RPCContext
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();
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
                        Logger.ConditionalTrace(s.GetName() + " can not be loaded" +e.Message+"\n"+e.StackTrace);
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
