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

        public async Task PublishItemAsync(QueueApi q, QueueMessage item, int connectTimeOut, Action<IQueueAck> action)
        {
           await _Api.PublishItemAsync(item, connectTimeOut, action);
        }

        public async Task EnqueueAsync(QueueApi api, QueueMessage item, int connectTimeOut, Action<IQueueAck> action)
        {
            await _Api.EnqueueAsync(item, connectTimeOut, action);
        }

        public void Enqueue(QueueApi api, QueueMessage item, int connectTimeOut, Action<IQueueAck> action)
        {
            _Api.Enqueue(item, connectTimeOut, action);
        }
        public IQueueAck Enqueue(QueueApi api, string queueName, QueueMessage item, int connectTimeOut)
        {
            return _Api.Enqueue(item, connectTimeOut);
        }
        public static async Task EnqueueAsync(string hostAddress, string queueName, QueueMessage item, int connectTimeOut, Action<IQueueAck> action)
        {
            var api = GetApi(queueName, hostAddress);

            await api.EnqueueAsync(item, connectTimeOut, action);
        }
        public static void Enqueue(string hostAddress, string queueName, QueueMessage item, int connectTimeOut, Action<IQueueAck> action)
        {
            var api = GetApi(queueName, hostAddress);

            api.Enqueue(item, connectTimeOut, action);
        }
        public static IQueueAck Enqueue(string hostAddress, string queueName, QueueMessage item, int connectTimeOut)
        {
            var api = GetApi(queueName, hostAddress);

            return api.Enqueue(item, connectTimeOut);
        }
    }

}
