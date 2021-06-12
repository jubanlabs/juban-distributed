
using Castle.DynamicProxy;
using MongoDB.Bson;

namespace Jubanlabs.JubanDistributed.WorkQueue {
    public class WorkInvokerDelayed : IInterceptor   {
        public void Intercept(IInvocation invocation)
        {
            var message = new Message ();
            message.delayed = true;
            message.type = TypesHelper.GetDistributableInterface(invocation.Proxy.GetType()).FullName;
            message.method = invocation.Method.Name;
            if (invocation.Arguments != null) {
                message.types = new string[invocation.Arguments.Length];
                message.values = new byte[invocation.Arguments.Length][];
                for (int i = 0; i < invocation.Arguments.Length; i++) {
                    message.types[i] = invocation.Arguments[i].GetType ().FullName;
                    message.values[i] = Utils.Serialize (invocation.Arguments[i]);
                }
            }
            message.id = ObjectId.GenerateNewId ().ToString();
            var mesg = Utils.Serialize (message);

            string queueName = message.type + "." + message.method;

            DelayedWorkDatabase.Instance.GetDatabase().GetCollection<BsonDocument> (queueName)
                .InsertOne (new BsonDocument { { "_id", ObjectId.Parse( message.id) }, { "msg", mesg } });
            

        }

    }

}