using System;
using Jubanlabs.JubanDistributed.RabbitMQ;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jubanlabs.JubanDistributed.WorkQueue {
    public class EventBasedWorkerHost {

        private readonly string queueName;

        private readonly IWorkInterpreter processInstance;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger ();
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

                        Logger.ConditionalTrace(DateTime.Now + " " + processInstance.GetType().FullName + " begin process");
                        processInstance.Process(body);
                        Logger.ConditionalTrace(DateTime.Now + " " + processInstance.GetType().FullName + " finish process");
                        channel.BasicAck(ea.DeliveryTag, true);
                    }
                    catch(Exception ex)
                    {
                        Logger.Info(ex.Message);
                        throw;
                    }
                };
                channel.BasicConsume (queueName, false, consumer);
            }
        }

    }
}