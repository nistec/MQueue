using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.ServiceModel;
using Nistec.Runtime;

namespace Nistec.Messaging.Proxies
{

    public class MQueueProxy
    {
        public MessageState Send(QueueItem msg)
        {
            MessageQueueProxy proxy = new MessageQueueProxy();
            return proxy.Invoke(msg, true);
        }

        //public MessageState Send(QueueMessage msg)
        //{
        //    QueueItemProxy proxy = new QueueItemProxy();
        //    return proxy.Invoke(msg, true);
        //}

        public MessageState SendBatch(QueueItem msg)
        {
            QueueBatchProxy proxy = new QueueBatchProxy();
            return proxy.Invoke(msg, true);
        }
    }



    public class MessageQueueProxy : ServiceProxy<IQueueProxy, QueueItem>
    {

        public MessageQueueProxy()
            : base("QueueProxy")
        {
        }

        protected override MessageState Send(QueueItem msg)
        {
            return Proxy.SendMessage(msg.Serialize(true));//.GetEntityStream(true));
        }
    }

    public class QueueItemProxy : ServiceProxy<IQueueProxy, QueueItem>
    {

        public QueueItemProxy()
            : base("QueueProxy")
        {
        }

        protected override MessageState Send(QueueItem item)
        {
            return Proxy.SendMessage(item.BodyStream);
        }
    }

    public class QueueBatchProxy : ServiceProxy<IQueueProxy, QueueItem>
    {

        public QueueBatchProxy()
            : base("QueueProxy")
        {
        }

        protected override MessageState Send(QueueItem item)
        {
            return Proxy.SendMessage(item.BodyStream);
        }
    }

}
