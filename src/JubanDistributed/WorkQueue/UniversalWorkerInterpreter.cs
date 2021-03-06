﻿
using System;
using System.Reflection;
using Jubanlabs.JubanDistributed.RPC;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;


namespace Jubanlabs.JubanDistributed.WorkQueue
{
    
    public class UniversalWorkerInterpreter :IWorkInterpreter
    {
        private static readonly ILogger<UniversalWorkerInterpreter> Logger =  JubanLogger.GetLogger<UniversalWorkerInterpreter>();
        public IDistributable WorkerInstance { get; set; }

        public string queueName{get;set;}
     
        public void Process(byte[] msg)
        {

            var mesgObj = (Message)Utils.Deserialize(msg, typeof(Message));
          

            object[] valuesArray = null;

            MethodInfo methodInfo;
            if (mesgObj.values != null)
            {
                valuesArray = new object[mesgObj.values.Length];

                var typesArray = new Type[mesgObj.types.Length];
                for (int i = 0; i < mesgObj.types.Length; i++)
                {
                    typesArray[i] = RPCContext.TypesDict[mesgObj.types[i]];
                    valuesArray[i] = Utils.Deserialize(mesgObj.values[i], typesArray[i]);
                }
                methodInfo = WorkerInstance.GetType().GetMethod(mesgObj.method, typesArray);
            }
            else
            {
                methodInfo = WorkerInstance.GetType().GetMethod(mesgObj.method);
            }

            var invokerSub = FastInvoke.GetMethodInvoker(methodInfo);

            invokerSub(WorkerInstance, valuesArray);
            if(mesgObj.delayed){
                DelayedWorkDatabase.Instance.GetDatabase().GetCollection<BsonDocument>(queueName).DeleteOne(new BsonDocument {{"_id",ObjectId.Parse(mesgObj.id)}});
            }
        }
    }
}