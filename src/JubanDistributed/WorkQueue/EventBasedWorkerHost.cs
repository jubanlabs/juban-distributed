using System;
using Jubanlabs.JubanDistributed.RabbitMQ;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jubanlabs.JubanDistributed.WorkQueue {
    public class EventBasedWorkerHost {

        private readonly string queueName;

        private readonly IWorkInterpreter processInstance;

        private static readonly ILogger<EventBasedWorkerHost> Logger =  JubanLogger.GetLogger<EventBasedWorkerHost>();
        private IModel channel;
        private EventingBasicConsumer consumer;
        public EventBasedWorkerHost (IWorkInterpreter obj, string name) {
            processInstance = obj;
            queueName = name;
            channel = MQConnectionContext.Instance.GetChannelForConsumer ();
            lock (channel) {
                channel.QueueDeclare (queueName, false, false, false, null);
                channel.BasicQos (0, 250, false);
                consumer = new EventingBasicConsumer (channel);

                consumer.Received += (model, ea) => {
                    try
                    {
                        byte[] body = ea.Body.ToArray();

                        Logger.LogTrace(DateTime.Now + " " + processInstance.GetType().FullName + " begin process");
                        processInstance.Process(body);
                        Logger.LogTrace(DateTime.Now + " " + processInstance.GetType().FullName + " finish process");
                        channel.BasicAck(ea.DeliveryTag, true);
                    }
                    catch(Exception ex)
                    {
                        Logger.LogInformation(ex.Message);
                        throw;
                    }
                };
                channel.BasicConsume (queueName, false, consumer);
            }
        }

    }
}