using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Nistec.Messaging;
using Nistec.Messaging.Proxies;
using Nistec.Generic;
using System.IO;
using Nistec.Messaging.Remote;
using Nistec.IO;
using Nistec.Messaging.Server;


namespace Nistec.Queue.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class QueueProxy : IQueueProxy
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }


        public MessageState SendMessage(Stream stream)
        {
            try
            {
                QueueMessage msg = new QueueMessage(stream);
                if (msg == null)
                {
                    throw new MessageException(MessageState.MessageError, "QueueMessage.Deserialize failed");
                }
                return SendMessageInternal(msg);
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("QueueProxy SendMessage MessageException: {0}", mex.Message);
                return mex.MessageState;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("QueueProxy SendMessage Error: {0}", ex.Message);
                return MessageState.UnExpectedError;
            }
        }


        public MessageState EnqueueMessage(QueueMessage msg)
        {
            return SendMessageInternal(msg);
        }

        /*
        public MessageState SendItem(Stream stream)
        {
            try
            {

                QueueItem item = QueueItem.Deserialize(NetStream.EnsureNetStream(stream));
                if (item == null)
                {
                    throw new MessageException(MessageState.BadRequest, "QueueItem.Deserialize failed");
                }
                QueueClient.Enqueu RemoteQueueServer.QueueManager[item.Label].Enqueue(item);

                return MessageState.Received;
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("QueueProxy SendItem MessageException: {0}", mex.Message);
                return mex.MessageState;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("QueueProxy SendItem Error: {0}", ex.Message);
                Netlog.ErrorFormat("QueueProxy SendItem ErrorStackTrace: {0}", ex.StackTrace);
                return MessageState.UnExpectedError;
            }
        }


        public MessageState EnqueueItem(QueueItem item)
        {
            try
            {
                if (item == null)
                {
                    throw new MessageException(MessageState.BadRequest, "QueueItem.Deserialize failed");
                }
                RemoteQueueServer.QueueManager[item.Label].Enqueue(item);
                return MessageState.Received;
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("QueueProxy EnqueueItem error: {0}", mex.Message);
                return mex.MessageState;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("QueueProxy Error: {0}", ex.Message);
                return MessageState.UnExpectedError;
            }
        }


        public MessageState SendBatchItem(string serItem)
        {
            try
            {

                QueueItem item = QueueItem.Deserialize(serItem);
                if (item == null)
                {
                    throw new MessageException(MessageState.BadRequest, "QueueItem.Deserialize failed");
                }
                SendBatchToQueue(item);

                return MessageState.Received;
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("QueueProxy SendBatchItem MessageException: {0}", mex.Message);
                return mex.MessageState;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("QueueProxy SendBatchItem Error: {0}", ex.Message);
                Netlog.ErrorFormat("QueueProxy SendBatchItem ErrorStackTrace: {0}", ex.StackTrace);
                return MessageState.UnExpectedError;
            }
        }
        */

        public bool QueueExists(string queueName)
        {
            return AgentManager.Queue.Exists(queueName);
        }

        public bool CanQueue(string queueName, uint count)
        {
            return AgentManager.Queue.CanQueue(queueName);
        }

        #region internal

        private MessageState SendMessageInternal(QueueMessage msg)
        {
            try
            {
                if (msg == null)
                {
                    throw new MessageException(MessageState.ArgumentsError, "QueueMessage null");
                }
                msg.Validate();

                switch(msg.MessageType)//.DistributionMode)
                {
                    case MessageTypes.DirectToQueue:
                         SendDirectToQueue(msg);break;
                    case MessageTypes.BatchToMailer:
                         SendBatchToQueue(msg);break;
                    case MessageTypes.SplitToQueue:
                         SendSplitToQueue(msg);break;
                }
                
                 return MessageState.Received;
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("QueueProxy SendMessageInternal MessageException : {0}", mex.Message);
                return mex.MessageState;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("QueueProxy SendMessageInternal Error: {0}", ex.Message);
                return MessageState.UnExpectedError;
            }
        }

        /*
        private MessageState SendItemInternal(QueueItem item)
        {
            try
            {
                if (item == null)
                {
                    throw new MessageException(MessageState.ArgumentsError, "QueueItem is null");
                }
                return (MessageState)AgentManager.Queue.Enqueue(item);

                //RemoteQueueServer.QueueManager[item.Label].Enqueue(item);

                //return MessageState.Received;
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("QueueProxy SendItemInternal MessageException : {0}", mex.Message);
                return mex.MessageState;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("QueueProxy SendItemInternal Error: {0}", ex.Message);
                return MessageState.UnExpectedError;
            }
        }

        private void SendBatchToQueue(QueueItem item)
        {
            //mailhost
            MailerHost host = null;

            if (!MailerManager.Channels.TryGetValue(item.Label, out host))
            {
                throw new MessageException(MessageState.InvalidMessageHost, "Mailer host not exists : " + item.Label);
            }

            if ((int)item.Priority > (int)Priority.Normal)// (item.Segments > 0 && (int)item.Priority > (int)Priority.Normal)
            {
                RemoteQueueServer.QueueManager[item.Label].Enqueue(item);
                return;
            }
            //QueueItem item = msg.DeserializeBody();
            string path = host.GetBatchPath(item.OperationId);
            MailerHost.EnsureDirectory(path);
            item.Save(path);
        }
        */

        private void SendDirectToQueue(QueueMessage msg)
        {
          
            AgentManager.Queue.Enqueue(msg);
        }

        private void SendBatchToQueue(QueueMessage msg)
        {
            //mailhost
            MailerHost host = null;

            if (!MailerManager.Channels.TryGetValue(msg.Host, out host))
            {
                throw new MessageException(MessageState.InvalidMessageHost, "Mailer host not exists :" + msg.Host);
            }

            if ((int)msg.Priority > (int)Priority.Normal)// (item.Segments > 0 && (int)item.Priority > (int)Priority.Normal)
            {
                SendDirectToQueue(msg);
                return;
            }

            string path = host.GetBatchPath(msg.Headers.Get<int>("BatchId"));
            MailerHost.EnsureDirectory(path);
            msg.Save(path);
        }

        private void SendSplitToQueue(QueueMessage msg)
        {
            string queueName = msg.Host;

            var items = msg..MessageItems();

            foreach (QueueItem item in items)
            {
                AgentManager.Queue.Enqueue(item);
            }
        }

        #endregion
    }
}
