using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Jubanlabs.JubanShared.Common.Config;
using Jubanlabs.JubanShared.Logging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jubanlabs.JubanDistributed.RabbitMQ {
    public class MQConnectionContext {
        private static ILogger<MQConnectionContext> Logger =  JubanLogger.GetLogger<MQConnectionContext>();
        private static MQConnectionContext instance = new MQConnectionContext ();
        public static MQConnectionContext Instance { get { return instance; } }

        private MQConnectionContext () {
            var factory = new ConnectionFactory {
                HostName = AppSettings.Instance.GetValue ("jubandistributed.messagingServer"),
                Port = 5672
            };
            Logger.LogTrace (factory.HostName);
            ConnectionForPublisher = factory.CreateConnection ();
            ConnectionForConsumer = factory.CreateConnection ();
            //System.Threading.Thread.CurrentThread.ManagedThreadId

            channelForPublisher = new Lazy<IModel> (
                () => {
                    return ConnectionForPublisher.CreateModel ();
                });
            channelForConsumer = new Lazy<IModel> (
                () => {
                    return ConnectionForPublisher.CreateModel ();
                });

            replyQueueNameForRPC = new ConcurrentDictionary<string, Lazy<string>> ();

            CallbackMapper =
            new ConcurrentDictionary<string, TaskCompletionSource<byte[]>> ();
        }

        public IConnection ConnectionForPublisher { get; set; }
        public IConnection ConnectionForConsumer { get; set; }

        private Lazy<IModel> channelForPublisher;
        private Lazy<IModel> channelForConsumer;

        
        public ConcurrentDictionary<string, TaskCompletionSource<byte[]>> CallbackMapper;

        private ConcurrentDictionary<string, Lazy<string>> replyQueueNameForRPC;

        public IModel GetChannelForPublisher () {
            return channelForPublisher.Value;
        }
        public IModel GetChannelForConsumer () {

            return channelForConsumer.Value;
        }

        public string GetReplyQueueNameForRPC (string rpcName) {
            return replyQueueNameForRPC.GetOrAdd (rpcName, x => new Lazy<string> (
                () => {

                    var channel = GetChannelForConsumer ();
                    lock (channel) {
                        var replyQueueName = channel.QueueDeclare ().QueueName;

                        var consumer = new EventingBasicConsumer (channel);
                        consumer.Received += (model, ea) => {
                            //Logger.LogTrace("trace rpc time: MQReceivedResponse " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                            //Logger.LogTrace(rpcName + "-" + System.Threading.Thread.CurrentThread.ManagedThreadId); 

                            if (!CallbackMapper.TryRemove (ea.BasicProperties.CorrelationId, out TaskCompletionSource<byte[]> tcs))
                                return;

                            tcs.TrySetResult (ea.Body.ToArray());
                        };
                        channel.BasicConsume (replyQueueName, true, consumer);
                        return replyQueueName;
                    }
                    // var props = GetChannelForPublisher ().CreateBasicProperties ();
                    // props.ReplyTo = replyQueueName;
                    // props.CorrelationId = corrId;

                })).Value;
        }

    }
}