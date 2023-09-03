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
    public abstract class QueueClient
    {
        protected QueueApi _Api;
        protected QueueClient(string queueName, string hostAddress)
        {
            _Api = new QueueApi(queueName, hostAddress);
        }
        protected QueueClient(string queueName, HostProtocol protocol, string endpoint, int port, string hostName)
        {
            _Api = new QueueApi(queueName, protocol, endpoint, port, hostName);
        }


        public static QueueHost GetHost(string host_address, string queueName)
        {

            //"tcp:127.0.0.1:15000?NC_Quick"
            //"tcp:127.0.0.1:15000
            var host = QueueHost.Parse(host_address + "?" + queueName);
            return host;
        }
        public static QueueHost GetHost(string protocol, string host_address, string queueName)
        {
            //127.0.0.1:15000
            var host = QueueHost.Parse(protocol + ":" + host_address + "?" + queueName);
            return host;
        }

        public static QueueApi GetApi(string queueName, string hostAddress)
        {
            QueueApi q = new QueueApi(queueName, hostAddress);
            q.IsAsync = false;
            return q;
        }
        public static QueueApi GetApi(string queueName, HostProtocol protocol, string endpoint, int port, string hostName)
        {
            QueueApi q = new QueueApi(queueName, protocol, endpoint, port, hostName);
            q.IsAsync = false;
            return q;
        }

        public static QueueApi GetApi(QueueHost host)
        {
            QueueApi q = new QueueApi(host);
            q.IsAsync = false;
            return q;
        }

        public static QueueMessage CreateQueueItem(string body, string label)
        {
            QueueMessage msg = new QueueMessage();
            msg.SetBody(body);
            msg.Label = label;

            return msg;
        }

        public static QueueMessage CreateQueueItem(string body, string label, Priority priority, string id)
        {
            QueueMessage msg = new QueueMessage();
            msg.SetBody(body);
            msg.Label = label;
            //mqh-msg.MessageType = MQTypes.Message;
            msg.Priority = priority;
            msg.Command = QueueCmd.Enqueue.ToString();
            //msg.Identifier= identifier;
            msg.CustomId = id;
            //msg.GroupId = groupId;

            return msg;
        }

        public static QueueMessage CreateQueueItem(NetStream body, Type type, string label, Priority priority, string id)
        {
            QueueMessage msg = new QueueMessage();
            msg.SetBody(body, type.Name);
            msg.Label = label;
            //mqh-msg.MessageType = MQTypes.Message;
            msg.Priority = priority;
            msg.Command = QueueCmd.Enqueue.ToString();
            //msg.Identifier= identifier;
            msg.CustomId = id;
            //msg.GroupId = groupId;

            return msg;
        }

        public static QueueRequest CreateQueueRequest(QueueCmd command, int version, Priority priority, TransformType transformType, string host, NetStream bodyStream)
        {
            return new QueueRequest(bodyStream,typeof(NetStream))
            {
                //Version = version,
                //MessageType = messageType,
                Command = command.ToString(),
                Priority = priority,
                TransformType = transformType,
                Host = host
                //Creation = DateTime.Now,
                //Modified = DateTime.Now,
                //ArrivedTime = Assists.NullDate,
                //BodyStream = bodyStream
            };
        }
    }

}
