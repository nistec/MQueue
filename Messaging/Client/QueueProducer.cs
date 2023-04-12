using Nistec.Channels;
using Nistec.IO;
using Nistec.Messaging;
using Nistec.Messaging.Remote;
using Nistec.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nistec.Messaging.Client
{
    public class QueueProducer: QueueClient
    {
        public QueueProducer(string queueName, string hostAddress):base(queueName, hostAddress)
        {
           
        }
        public QueueProducer(string queueName, HostProtocol protocol, string endpoint, int port, string hostName) : base(queueName, protocol, endpoint, port, hostName)
        {

        }
                
        public IQueueAck PublishItem(QueueApi q, QueueMessage item, int connectTimeOut)
        {
            return _Api.PublishItem(item, connectTimeOut);
        }

        public void PublishItem(QueueApi q, QueueMessage item, int connectTimeOut, Action<IQueueAck> action)
        {
            _Api.PublishItem(item, connectTimeOut, action);
        }


        public void EnqueueAsync(QueueApi api, QueueMessage item, int connectTimeOut, Action<IQueueAck> action)
        {
            _Api.EnqueueAsync(item, connectTimeOut, action);
        }
        public IQueueAck Enqueue(QueueApi api, string queueName, QueueMessage item, int connectTimeOut)
        {
            return _Api.Enqueue(item, connectTimeOut);
        }

        public static void EnqueueAsync(string hostAddress, string queueName, QueueMessage item, int connectTimeOut, Action<IQueueAck> action)
        {
            var api = GetApi(queueName, hostAddress);

            api.EnqueueAsync(item, connectTimeOut, action);
        }
        public static IQueueAck Enqueue(string hostAddress, string queueName, QueueMessage item, int connectTimeOut)
        {
            var api = GetApi(queueName, hostAddress);

            return api.Enqueue(item, connectTimeOut);
        }
    }

}
