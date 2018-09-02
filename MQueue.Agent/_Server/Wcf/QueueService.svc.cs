using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MControl.Messaging;
using MControl.Messaging.Proxies;


namespace MControl.Queue.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class QueueService : IQueueService
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public AcknowledgeStatus Enqueue(MessageQueue msg)
        {
            try
            {
                QueueItem item = QueueItem.Deserialize(msg.SerilaizedValue);
                if (item == null)
                {
                    throw new MessageException(AcknowledgeStatus.BadRequest, "QueueItem.Deserialize failed");
                }
                RemoteQueueServer.QueueManager[msg.QueueName].Enqueue(item);
                return AcknowledgeStatus.Received;
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("RemoteService QueueService EnqueueItem error: {0}", mex.Message);
                return mex.AcknowledgeStatus;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("RemoteService QueueService Error: {0}", ex.Message);
                return AcknowledgeStatus.UnExpectedError;
            }
        }

        public AcknowledgeStatus EnqueueItem(QueueItem item, string queueName)
        {
            try
            {
                RemoteQueueServer.QueueManager[queueName].Enqueue(item);
                return AcknowledgeStatus.Received;
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("RemoteService QueueService EnqueueItem error: {0}", mex.Message);
                return mex.AcknowledgeStatus;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("RemoteService QueueService Error: {0}", ex.Message);
                return AcknowledgeStatus.UnExpectedError;
            }
        }

        public AcknowledgeStatus EnqueueMessage(string serItem, string queueName)
        {
            try
            {

                QueueItem item = QueueItem.Deserialize(serItem);
                if (item == null)
                {
                    throw new MessageException(AcknowledgeStatus.BadRequest, "QueueItem.Deserialize failed");
                }
                RemoteQueueServer.QueueManager[queueName].Enqueue(item);

                return AcknowledgeStatus.Received;
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("RemoteService QueueService EnqueueMessage error: {0}", mex.Message);
                return mex.AcknowledgeStatus;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("RemoteService QueueService Error: {0}", ex.Message);
                Netlog.ErrorFormat("RemoteService QueueService ErrorStackTrace: {0}", ex.StackTrace);
                return AcknowledgeStatus.UnExpectedError;
            }
        }


        public bool QueueExists(string queueName)
        {
            return RemoteManager.QueueExists(queueName);
        }

        public bool CanQueue(string queueName, uint count)
        {
            return RemoteManager.QueueExists(queueName);
        }
    }
}
