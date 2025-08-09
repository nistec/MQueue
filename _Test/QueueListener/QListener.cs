
using Nistec.Messaging;
using Nistec.Messaging.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueListenerDemo
{
    public class QListener:QueueListener
    {

        public static QueueAdapter CreateAdapter(QueueHost host)
        {
            var adapter = new QueueAdapter()
            {
                Source = host,
                IsAsync = true,
                IsMultiTask = true,
                Interval = 10,
                ConnectTimeout = 5000,
                ReadTimeout = 180000,
                WorkerCount = 2,
                MaxConnection = 2
                //EnableDynamicWait = true,
                //MessageReceivedAction = (message) =>
                //{
                //    Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}", message.MessageState, message.ArrivedTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), message.Host, message.Label, message.Identifier, message.Duration);

                //    var body = message.GetBody();
                //    string sbody = body == null ? "null" : body.ToString();
                //    Console.WriteLine("body: " + sbody);
                //},
                //MessageFaultAction = (message) =>
                //{
                //    Console.WriteLine(message);
                //}

            };
            return adapter;
        }

        public QListener(QueueAdapter adapter) : base(adapter)
        {

        }

        protected override void OnMessageReceived(IQueueMessage message)
        {
            //base.OnMessageReceived(message);
            //var message = e.Args;
            Console.WriteLine("OnMessageReceived State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}", message.MessageState, message.ArrivedTime, message.Host, message.Label, message.Identifier);

        }

        protected override IQueueMessage Receive()
        {
            return base.Receive();
        }

       
    }
}
