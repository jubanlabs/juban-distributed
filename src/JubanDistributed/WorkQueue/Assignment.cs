using System;
using System.Threading;
using Jubanlabs.JubanDistributed.RabbitMQ;
using RabbitMQ.Client;

namespace Jubanlabs.JubanDistributed.WorkQueue {
    public class Assignment {
        public delegate void CircuitBreakerHandler (String str);
        public event CircuitBreakerHandler CircuitBreakerEvent;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger ();
        // private static Bridge instance;
        //  private static readonly object initLocker = new object();
       
        private readonly string channelName;
        private IModel channel;
        private int cnt;
        public uint BufferCount = 1000;
        public uint BufferCheckRate = 1000;
        public Assignment (string name) {
            channelName = name;
            channel = MQConnectionContext.Instance.GetChannelForPublisher ();
            lock (channel) {
                channel.QueueDeclare (channelName, false, false, false, null);
            }
            cnt = 0;
        }

        public uint MessageCount {
            get {
                // if (!channel.IsOpen) {
                //     Reconnect ();
                // }
                lock (channel) {
                    return channel.QueueDeclarePassive (channelName).MessageCount;
                }
            }
        }

        public string ChannelName {
            get {
                return channelName;
            }
        }

        public int Test () {
            return 777;
        }

        public int Send (byte[] data) {
            lock (channel) {
                try {
                    // if (!channel.IsOpen) {
                    //     Reconnect ();
                    // }
                    cnt++;
                    channel.BasicPublish ("", channelName, null, data);
                    if (cnt >= BufferCheckRate) {
                        CircuitBreaker ();
                        cnt = 0;
                    }

                } catch (Exception ex) {
                    Logger.Error (ex, "send message failed. msg:" + data);

                    throw;
                }

                return 1;
            }
        }

        private void CircuitBreaker () {
            while (true) {
                if (MessageCount >= BufferCount) {
                    OnCircuitBreaker ("circuit breaker occured");
                    Logger.ConditionalTrace ("queue is not empty,sleeping ...");
                    Thread.Sleep (1000);
                    continue;
                }
                break;
            }
        }

        protected void OnCircuitBreaker (String msg) {
            if (CircuitBreakerEvent != null) {
                CircuitBreakerEvent (msg);
            }
        }

        //public async Task<int> SendAsync(AssignmentMessage data)
        //{
        //    byte[] body = helper.CompressSmallString(helper.ObjectToByteArray(data));
        //    lock (_locker)
        //    {
        //        if (!channel.IsOpen)
        //        {
        //            Reconnect();
        //        }
        //    }
        //    await Task.Run(() => channel.BasicPublish("", channelName, null, body));

        //    return 1;
        //}

        // public void Dispose () {
        //     channel.Dispose ();
        // }
    }
}