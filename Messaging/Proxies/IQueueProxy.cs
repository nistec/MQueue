using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;

namespace Nistec.Messaging.Proxies
{
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IQueueProxy
    {

        //[OperationContract]
        //MessageState EnqueueItem(QueueItem item);

        [OperationContract]
        MessageState EnqueueMessage(QueueItem msg);

        [OperationContract]
        MessageState SendMessage(Stream stream);

        //[OperationContract]
        //MessageState SendItem(Stream serItem);

        //[OperationContract]
        //MessageState SendBatchItem(Stream serItem);

        [OperationContract]
        bool QueueExists(string queueName);

        [OperationContract]
        bool CanQueue(string queueName, uint count);
    }

}
