using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MControl.Messaging;
using MControl.Messaging.Mail;
using MControl.Messaging.Proxies;


namespace MControl.Queue.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class MailerService :  IMailerService
    {

        public string GetData(string value)
        {
            return string.Format("You entered: {0}", value);
        }

        public AcknowledgeStatus EnqueueItem(QueueItem item)
        {
            try
            {
                EnqueueMessage(item);
                return AcknowledgeStatus.Received;
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("RemoteService MailerService EnqueueItem error: {0}", mex.Message);
                return mex.AcknowledgeStatus;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("RemoteService MailerService Error: {0}", ex.Message);
                return AcknowledgeStatus.UnExpectedError;
            }
        }

        public AcknowledgeStatus EnqueueMessage(string serItem)
        {
            try
            {

                QueueItem item = QueueItem.Deserialize(serItem);
                if (item == null)
                {
                    throw new MessageException(AcknowledgeStatus.BadRequest, "QueueItem.Deserialize failed");
                }
                EnqueueMessage(item);

                return AcknowledgeStatus.Received;
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("RemoteService MailerService EnqueueMessage error: {0}", mex.Message);
                return mex.AcknowledgeStatus;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("RemoteService MailerService Error: {0}", ex.Message);
                Netlog.ErrorFormat("RemoteService MailerService ErrorStackTrace: {0}", ex.StackTrace);
                return AcknowledgeStatus.UnExpectedError;
            }
        }

       static void EnqueueMessage(QueueItem item)
       {
           //mailhost
           MailHost host = null;

           if (!MailHosts.TryGetValue(item.Label, out host))
           {
               throw new MessageException(AcknowledgeStatus.InvalidMailHost, "Mail host not exists");
           }
           if (item.Segments > 0 && (int)item.Priority > (int)Priority.Normal)
           {
               RemoteQueueServer.QueueManager[host.ChunkQueueName].Enqueue(item);

               return;
           }
           int batchId = item.Segments > 0 ? item.OperationId : item.MessageId;

           string path = host.GetBatchPath(batchId);
           MailHost.CreateDirectory(path);
           item.Save(path);
       }

    }
}
