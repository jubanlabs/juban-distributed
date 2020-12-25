using System;
using Jubanlabs.JubanDistributed.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jubanlabs.JubanDistributed.RPC {
    public class EventBasedRPCServiceHost {
        private readonly string channelName;

        private readonly IRPCInterpreter processInstance;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();
        private IModel channel;
        private EventingBasicConsumer consumer;
        public EventBasedRPCServiceHost (IRPCInterpreter obj, string rpcName) {
            processInstance = obj;
            channelName = "rpc_queue_" + rpcName;
            channel = MQConnectionContext.Instance.GetChannelForConsumer ();
            lock (channel) {
                channel.QueueDeclare (channelName, false, false, false, null);
                channel.BasicQos (0, 1, false);
                consumer = new EventingBasicConsumer (channel);

                consumer.Received += (model, ea) => {
                    Logger.ConditionalTrace ("trace rpc time: ServiceReceivedRequest " + DateTime.Now.ToString ("MM/dd/yyyy hh:mm:ss.fff tt"));
                    var body = ea.Body;
                    var props = ea.BasicProperties;

                    byte[] response = null;
                    try {
                        response = processInstance.Process (body.ToArray ());
                    } catch (Exception e) {
                        Logger.Error (" [.] " + e.Message);
                        response = null;
                    } finally {
                        lock (channel) {
                            var replyProps = channel.CreateBasicProperties ();
                            replyProps.CorrelationId = props.CorrelationId;
                            channel.BasicPublish ("", props.ReplyTo, replyProps,
                                response);
                            channel.BasicAck (ea.DeliveryTag, false);
                            Logger.ConditionalTrace ("trace rpc time: ServiceSendbackResponse " + DateTime.Now.ToString ("MM/dd/yyyy hh:mm:ss.fff tt"));
                        }
                    }
                };
                channel.BasicConsume (channelName, false, consumer);
            }
        }

    }
}