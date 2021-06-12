using System;
using System.Threading;
using System.Threading.Tasks;
using Jubanlabs.JubanDistributed.RabbitMQ;
using NLog;
using RabbitMQ.Client;

namespace Jubanlabs.JubanDistributed.RPC {

	public abstract class RPCClient {
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger ();
		private IModel publisherChannel;
		private string replyQueueName;

		public Task<byte[]> CallAsync (byte[] message, CancellationToken cancellationToken = default (CancellationToken)) {

			//try {
			//make sure check the last assigned variable
			if (publisherChannel == null) {
				replyQueueName = MQConnectionContext.Instance.GetReplyQueueNameForRPC (RPCName);
				publisherChannel = MQConnectionContext.Instance.GetChannelForPublisher ();
			}
			//ConditionalStopwatch.PunchIn ("RPCClient.Call");
			//Logger.ConditionalTrace("trace rpc time RPCClient: " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
			var correlationId = Guid.NewGuid ().ToString ();
			lock (publisherChannel) {
				IBasicProperties props = publisherChannel.CreateBasicProperties ();

				props.CorrelationId = correlationId;
				props.ReplyTo = replyQueueName;

				publisherChannel.BasicPublish ("", "rpc_queue_" + RPCName, props, message);
			}

			//Logger.ConditionalTrace (Task.CurrentId + " " + this.GetHashCode () + " " +
			//	" " + replyProperties.ReplyTo);
			var tcs = new TaskCompletionSource<byte[]> ();
			MQConnectionContext.Instance.CallbackMapper.TryAdd (correlationId, tcs);

			cancellationToken.Register (() => MQConnectionContext.Instance.CallbackMapper.TryRemove (correlationId, out var tmp));
			return tcs.Task;

			// } catch (Exception ex) {
			// 	Logger.Error(ex,"rpc call error");
			// 	throw ex;
			// }
		}

		public abstract string RPCName { get; }

	}

}