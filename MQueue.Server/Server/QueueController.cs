using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using Nistec.Generic;
using Nistec.Messaging.Remote;
using Nistec.IO;
using System.Collections.Concurrent;
using Nistec.Runtime;
using System.Runtime.Serialization;
using Nistec.Runtime.Advanced;
using Nistec.Messaging.Transactions;
using Nistec.Messaging;
using Nistec.Messaging.Config;
using System.IO;
using Nistec.Logging;
using Nistec.Channels;

namespace Nistec.Messaging.Server
{
    public class QueueController
    {
        protected ConcurrentDictionary<string, MQueue> MQ;

        internal TransactionDispatcher m_TransDispatcher;

        internal TransactionDispatcher TransDispatcher
        {
            get { return m_TransDispatcher; }
        }

        internal PriorityPersistQueue JournalQueue;

        internal TopicDispatcher TopicDispatcher;


        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Logger { get; set; }

        public QueueController()
        {
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 5;
            MQ = new ConcurrentDictionary<string, MQueue>(concurrencyLevel, initialCapacity);
            m_TransDispatcher = new TransactionDispatcher();
            TopicDispatcher = new TopicDispatcher(this);
            Logger = QLogger.Logger.ILog;
        }

        void LoadJournalQueue()
        {
            if (JournalQueue != null)
                return;
            var journalProp = new QProperties("journal", false, CoverMode.Memory);
            journalProp.ReloadOnStart = true;
            JournalQueue = new PriorityPersistQueue(journalProp);
        }

        internal IQueueAck JournalAddItem(QueueItem item)
        {
            if (JournalQueue != null)
                return JournalQueue.Enqueue(item);
            return new QueueAck(MessageState.None, item);
        }

        public virtual void Start()
        {
            if (IsStarted)
                return;
            TopicDispatcher.Start();
            IsStarted = true;
            Logger.Info("QueueController started...");
        }
  
        public virtual void Stop()
        {
            if (!IsStarted)
                return;
            TopicDispatcher.Stop();
            IsStarted = false;
            Logger.Info("QueueController stoped...");
        }

        public bool IsStarted { get; protected set; }

        MQueue GetValidQ(string qname)
        {
            MQueue mq = AgentManager.Queue.Get(qname);
            if (mq == null)
                throw new Exception("Queue not exists: " + qname);
            return mq;
        }

        #region static

        public static IQueueAck Requeue(IQueueItem item)
        {
            MQueue mq = AgentManager.Queue.Get(item.Host);
            if (mq == null)
                throw new Exception("Queue not exists: " + item.Host);
            return mq.Requeue(item);
        }

        #endregion


        #region queue response

        public TransStream DoResponse(IQueueItem item, MessageState state)
        {
            if (item == null)
            {
                return null;
                //throw new MessageException(MessageState.MessageError, "Invalid queue item to write response");
            }

            var ts = ((QueueItem)item).ToTransStream(state);

            //((QueueItem)item).SetState(state);
            QLogger.Debug("QueueController DoResponse IQueueAck: {0}", item.Print());
            //return item.ToStream();

            return ts;

            //if (item != null)
            //{
            //    ((QueueItem)item).SetState(state);
            //    return item.GetItemStream();
            //}
            //else
            //{
            //    Message response = Message.Ack(MessageState.UnExpectedError, new MessageException(MessageState.UnExpectedError, "WriteResponse error: there is no item stream to write reponse"));
            //    return response.ToStream();
            //}

            // QLogger.DebugFormat("Server WriteResponse State:{0}, MessageId: {1}", item.MessageState, item.MessageId);
        }

        public TransStream DoResponse(IQueueAck item)
        {
            if (item == null)
            {
                return null;
                //throw new MessageException(MessageState.MessageError, "Invalid queue item to write response");
            }
            QLogger.Debug("QueueController DoResponse IQueueAck: {0}", item.Print());
            return item.ToTransStream();
        }
        public TransStream DoResponse(IQueueItem item)
        {
            if (item == null)
            {
                return null;
                //throw new MessageException(MessageState.MessageError, "Invalid queue item to write response");
            }
            QLogger.Debug("QueueController DoResponse IQueueAck: {0}", item.Print());
            return item.ToTransStream();
        }
        public TransStream DoReportValue(object value)
        {
            return new TransStream(value);//, TransType.Object);
        }

        public TransStream DoReport(object item, QueueCmd cmd, MessageState state, string lbl)
        {
            if (item == null)
            {
                throw new MessageException(MessageState.PipeError, "Invalid item to write response");
            }
            if (item != null)
            {
                var message = QueueItem.Ack(state, cmd, lbl, null);

                message.SetBody(item);
                return message.ToTransStream();
            }
            else
            {
                QueueItem response = QueueItem.Ack(MessageState.UnExpectedError, cmd, new MessageException(MessageState.UnExpectedError, "WriteReport error: there is no item stream to write reponse"));
                return response.ToTransStream();
            }

            // QLogger.DebugFormat("Server WriteReport State:{0}, MessageType: {1}", state, msgType);

        }
        #endregion

        #region queue request
        internal TransStream ExecRequset(IQueueMessage request)
        {
            bool responseAck = false;
            try
            {
                if (request.QCommand == QueueCmd.QueueHasValue)
                {
                    return DoReportValue(QueueCount(request.Host));
                }

                Logger.Debug("QueueController ExecRequset : {0}", request.Print());

                switch (request.QCommand)
                {
                    case QueueCmd.Reply:
                        return TransStream.WriteState(0, "Reply: " + request.Identifier);//, TransType.Object);
                    case QueueCmd.Enqueue:
                        {
                            responseAck = true;
                            //MQueue Q = Get(request.Host);
                            var ack = ExecSet((QueueItem)request);
                            return DoResponse(ack);
                        }
                    case QueueCmd.Dequeue:
                    case QueueCmd.DequeuePriority:
                    case QueueCmd.DequeueItem:
                    case QueueCmd.Peek:
                    case QueueCmd.PeekPriority:
                    case QueueCmd.PeekItem:
                    case QueueCmd.Consume:
                        return DoResponse(ExecGet(request), MessageState.Receiving);
                    case QueueCmd.Commit:
                        break;
                    case QueueCmd.Abort:
                        break;

                    //operation
                    case QueueCmd.AddQueue:
                        {
                            responseAck = true;
                            MQueue mq = null;
                            return DoResponse(AddQueue(new QProperties(request.BodyStream), out mq));
                        }
                    case QueueCmd.RemoveQueue:
                        responseAck = true;
                        return DoResponse(RemoveQueue(request.Host));
                    case QueueCmd.HoldEnqueue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.ReleaseHoldEnqueue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.HoldDequeue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.ReleaseHoldDequeue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.EnableQueue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.DisableQueue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.ClearQueue:
                        throw new Exception("Operation not supported");


                    //publish\subscribe
                    case QueueCmd.TopicAdd:
                        throw new Exception("Operation not supported");
                    case QueueCmd.TopicRemove:
                        throw new Exception("Operation not supported");
                    case QueueCmd.TopicPublish:
                        throw new Exception("Operation not supported");
                    case QueueCmd.TopicSubscribe:
                        throw new Exception("Operation not supported");
                    case QueueCmd.TopicRemoveItem:
                        throw new Exception("Operation not supported");
                    case QueueCmd.TopicCommit:
                        throw new Exception("Operation not supported");
                    case QueueCmd.TopicAbort:
                        throw new Exception("Operation not supported");
                    case QueueCmd.TopicHold:
                        {
                            MQueue mq = GetValidQ(request.Host);
                            //mq.Topic.HoldTopic();
                            TopicDispatcher.Pause( OnOffState.On);
                            return TransStream.WriteState((int)MessageState.Ok, "Ok");// TransType.State);
                        }
                    case QueueCmd.TopicHoldRelease:
                        {
                            MQueue mq = GetValidQ(request.Host);
                            //mq.Topic.ReleaseHoldTopic();
                            TopicDispatcher.Pause(OnOffState.Off);
                            return TransStream.WriteState((int)MessageState.Ok, "Ok");// TransType.State);
                        }
                    case QueueCmd.TopicSubscribeHold:
                        {
                            MQueue mq = GetValidQ(request.Host);
                            //mq.Topic.HoldSubscriber(request.Label);
                            LoadTopicSubscribers(mq, request.Label, "remove");
                            return TransStream.WriteState((int)MessageState.Ok, "Ok");// TransType.State);
                        }
                    case QueueCmd.TopicSubscribeRelease:
                        {
                            MQueue mq = GetValidQ(request.Host);
                            //mq.Topic.ReleaseHoldSubscriber(request.Label);
                            LoadTopicSubscribers(mq, request.Label, "remove");
                            return TransStream.WriteState((int)MessageState.Ok, "Ok");// TransType.State);
                        }
                    case QueueCmd.TopicSubscribeAdd:
                        {
                            MQueue mq = GetValidQ(request.Host);
                            LoadTopicSubscribers(mq, request.Label, "add");
                            return TransStream.WriteState((int)MessageState.Ok, "Ok");// TransType.State);
                        }
                    case QueueCmd.TopicSubscribeRemove:
                        {
                            MQueue mq = GetValidQ(request.Host);
                            //TopicController tc = new TopicController(mq);
                            //tc.RemoveSubscriber(request.Label);
                            LoadTopicSubscribers(mq, request.Label, "remove");
                            return TransStream.WriteState((int)MessageState.Ok, "Ok");// TransType.State);
                        }
                    //reports
                    case QueueCmd.Exists:
                        responseAck = true;
                        return MessageAckServer.DoResponse(Exists(request.Host));
                    case QueueCmd.ReportQueueList:
                        var list = GetQueueList();
                        return new TransStream(list);
                    case QueueCmd.QueueProperty:
                    case QueueCmd.ReportQueueItems:
                        return ExecQuery(request);
                    case QueueCmd.ReportQueueStatistic:
                        return GetQueueReport(request);

                    //throw new Exception("Operation not supported");
                    case QueueCmd.PerformanceCounter:
                        throw new Exception("Operation not supported");
                    case QueueCmd.QueueCount:
                        return DoReport(QueueCount(request.Host), QueueCmd.QueueCount, MessageState.Ok, null);
                }
            }
            catch (MessageException mex)
            {
                Logger.Exception("ExecRequset MessageException: ", mex, true);
                return MessageAckServer.DoError(mex.MessageState, request, responseAck, mex);
            }
            catch (ArgumentException ase)
            {
                Logger.Exception("ExecRequset ArgumentException: ", ase, true, true);
                return MessageAckServer.DoError(MessageState.ArgumentsError, request, responseAck, ase);
            }
            catch (SerializationException se)
            {
                Logger.Exception("ExecRequset SerializationException: ", se, true);
                return MessageAckServer.DoError(MessageState.SerializeError, request, responseAck, se);
            }
            catch (Exception ex)
            {
                Logger.Exception("ExecRequset Exception: ", ex, true, true);
                return MessageAckServer.DoError(MessageState.UnExpectedError, request, responseAck, ex);
            }
            return null;
        }

        internal IQueueItem ExecGet(IQueueMessage request)
        {
            if (request.Host == null)
            {
                throw new MessageException(MessageState.InvalidMessageHost, "Invalid message.Host ");
            }
            MQueue Q = Get(request.Host);
            if (Q == null)
            {
                throw new MessageException(MessageState.InvalidMessageHost, "message.HostName not found " + request.Host);
            }
            switch (request.QCommand)
            {
                case QueueCmd.Dequeue:
                    return Q.Dequeue();
                case QueueCmd.DequeuePriority:
                    return Q.Dequeue(request.Priority);
                case QueueCmd.Peek:
                    return Q.Peek();
                case QueueCmd.PeekPriority:
                    return Q.Peek(request.Priority);
                case QueueCmd.Consume:
                    return Q.Consume(request.Expiration);// (Guid.NewGuid().ToString());// request.Identifier);
            }

            return null;
        }


        internal IQueueAck ExecSet(QueueItem item)
        {

            if (item == null)
            {
                throw new ArgumentNullException("QueueManager.ExecSet request");
            }
            if (item.Host == null)
            {
                throw new ArgumentNullException("QueueManager.ExecSet request.Host is invalid");
            }
            if (string.IsNullOrEmpty(item.Host))
            {
                throw new ArgumentNullException("QueueManager.Get queueName is null or empty");
            }
            MQueue Q;

            if (MQ.TryGetValue(item.Host, out Q))

            {
                Logger.Debug("QueueController ExecSet : Mode:{0}, {1}", Q.Mode.ToString(), item.Print());

                if (Q.Mode == CoverMode.Rout)
                {
                    return ExecRout(item, Q.RoutHost);
                }
                if (Q.IsTopic)
                {
                    return Q.EnqueueTopicItem(item);
                }
                //var item = new QueueItem(request, QueueCmd.Enqueue);
                var ack = Q.Enqueue(item);
                return ack;// ptr.MessageState;
            }
            else
            {
                throw new MessageException(MessageState.InvalidMessageHost, "message.HostName not found " + item.Host);
            }

            //if (TryGet(item.Host, out Q))
            //{
            //    if (Q.IsTopic)
            //    {
            //        return Q.EnqueueTopicItem(item);
            //    }
            //    //var item = new QueueItem(request, QueueCmd.Enqueue);
            //    if (Q.Mode == CoverMode.Rout)
            //    {
            //        return ExecRout(item, Q.RoutHost);
            //    }
            //    var ack = Q.Enqueue(item);
            //    return ack;// ptr.MessageState;
            //}
            //else
            //{
            //    throw new MessageException(MessageState.InvalidMessageHost, "message.HostName not found " + item.Host);
            //}

            //MQueue Q = Get(item.Host);
            //if (Q == null)
            //{
            //    throw new MessageException(MessageState.InvalidMessageHost, "message.HostName not found " + item.Host);
            //}
            //if (Q.IsTopic)
            //{
            //    return Q.EnqueueTopicItem(item);
            //}
            ////var item = new QueueItem(request, QueueCmd.Enqueue);
            //if (Q.Mode == CoverMode.Rout)
            //{
            //    return ExecRout(item, Q.RoutHost);
            //}
            //var ack = Q.Enqueue(item);
            //return ack;// ptr.MessageState;

            ////return QueueItem.Ack(item, ptr.MessageState, ptr.Retry, null, ptr.Identifier);

        }

        IQueueAck ExecRout(QueueItem item, QueueHost qh)
        {

            if (qh == null)
            {
                throw new MessageException(MessageState.InvalidMessageHost, "Invalid QueueHost for Routing " + item.Host);
            }
            IQueueAck ack = null;
            if (qh.IsLocal)
            {
                item.Host = qh.HostName;
                MQueue Q = Get(item.Host);
                if (Q == null)
                {
                    throw new MessageException(MessageState.InvalidMessageHost, "message.RoutHostName not found " + item.Host);
                }
                if (Q.IsTopic)
                {
                    return Q.EnqueueTopicItem(item);
                }
                ack = Q.Enqueue(item);
                return ack;// ptr.MessageState;
            }

            var api = new QueueApi(qh);
            ack = api.SendAsync(item, 0);
            return ack;
        }

        #endregion

        #region queue query


        internal TransStream ExecQuery(IQueueMessage request)
        {

            if (request == null)
            {
                throw new ArgumentNullException("QueueManager.ExecQuery request");
            }
            if (request.Host == null)
            {
                throw new ArgumentNullException("QueueManager.ExecQuery request.Host is invalid");
            }

            MQueue Q = Get(request.Host);
            if (Q == null)
            {
                throw new MessageException(MessageState.InvalidMessageHost, "request.HostName not found " + request.Host);
            }

            switch (request.QCommand)
            {
                case QueueCmd.QueueProperty:
                    var res = Q.Property();//.QueueProperty();
                    return new TransStream(res);
                case QueueCmd.ReportQueueItems:
                    var items = Q.QueryItems();
                    return new TransStream(items);
                default:
                    throw new NotSupportedException(request.QCommand.ToString());
            }

        }

        #endregion

        #region Queue message
#if (false)
        internal NetStream ExecRequset(QueueMessage request)
        {
            switch (request.Command)
            {
                case QueueCmd.Abort:
                  
                    break;
                case QueueCmd.AddQueue:
                    MQueue mq = null;
                    return QueueMessage.DoAck(AddQueue(QProperties.Get(request.Headers), out mq), null);
                case QueueCmd.Commit:
                    break;
                case QueueCmd.Dequeue:
                case QueueCmd.DequeueItem:
                case QueueCmd.DequeuePriority:
                case QueueCmd.Peek:
                case QueueCmd.PeekItem:
                case QueueCmd.PeekPriority:
                    return QueueMessage.DoResponse(ExecGet(request));
                case QueueCmd.Enqueue:
                    return QueueMessage.DoAck(ExecSet(request), null);
                case QueueCmd.Exists:
                    return QueueMessage.DoAck(Exists(request.Host), null);
                 case QueueCmd.QueueProperty:
                    break;
                case QueueCmd.RemoveQueue:
                    return QueueMessage.DoAck(RemoveQueue(request.Host), null);
                //case QueueCmd.ReportQueueItems:
                //    GetQueueItems(message.GetHostName());
                //    break;
                //case QueueCmd.ReportQueueItemsTable:
                //    GetQueueItemsTable(message.GetHostName());
                //    break;
                case QueueCmd.ReportQueueList:
                    GetQueueList();
                    break;
            }

            return null;
        }

        internal IQueueItem ExecGet(QueueMessage request)
        {
            try
            {
                MQueue Q = Get(request.Host);
                if (Q == null)
                {
                    throw new MessageException(MessageState.InvalidMessageHost, "message.Host is invalid " + request.Host);
                }
                switch (request.Command)
                {
                    case QueueCmd.Dequeue:
                        return Q.Dequeue();
                    case QueueCmd.DequeuePriority:
                        return Q.Dequeue(request.Priority);
                    case QueueCmd.Peek:
                        return Q.Peek();
                    case QueueCmd.PeekPriority:
                        return Q.Peek(request.Priority);
                }
            }
            catch (MessageException mex)
            {
                QLogger.ErrorFormat("ExecGet MessageException: " + mex.Message);
            }
            catch (ArgumentException ase)
            {
                QLogger.ErrorFormat("ExecGet ArgumentException: " + ase.Message);
            }
            catch (SerializationException se)
            {
                QLogger.ErrorFormat("ExecGet SerializationException: " + se.Message);
            }
            catch (Exception ex)
            {
                QLogger.ErrorFormat("ExecGet Exception: " + ex.Message);
            }
            return null;
        }
      

        /// <summary>
        /// Enqueue message <see cref=""/> to queue.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="SerializationException"></exception>
        /// <exception cref="Exception"></exception>
        public NetStream Enqueue(QueueMessage request)
        {
            var state = ExecSet(request);
            return QueueMessage.DoAck(state, null);// BinarySerializer.SerializeToStream((int)ExecSet(message));
        }

        internal MessageState ExecSet(QueueMessage request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException("QueueManager.ExecSet message");
                }
                if (request.Host == null)// || message.Host.Count == 0)
                {
                    throw new ArgumentNullException("QueueManager.ExecSet message.Host is invalid");
                }

                MQueue Q = Get(request.Host);
                if (Q == null)
                {
                    throw new MessageException(MessageState.InvalidMessageHost, "message.Host is invalid " + request.Host);
                }

                Q.Enqueue(request.GetQueueItem());

                //TODO: DISTREBUTING TO HOSTS LIST


                //foreach (var host in request.Host)
                //{
                //    if (host.IsLocal)
                //    {
                //        MQueue Q = Get(host.HostName);
                //        if (Q == null)
                //        {
                //            throw new MessageException(MessageState.InvalidMessageHost, "message.Host is invalid " + message.Host);
                //        }
                //            Q.Enqueue(item);
                //    }
                //    else
                //    {
                //        //TODO:not supported
                //        throw new Exception("Host not IsLocal not supported!");
                //    }
                //}

            }
            catch (MessageException mex)
            {
                QLogger.ErrorFormat("ExecSet MessageException: " + mex.Message);
                return MessageState.MessageError;
            }
            catch (ArgumentException ase)
            {
                QLogger.ErrorFormat("ExecSet ArgumentException: " + ase.Message);
                return MessageState.ArgumentsError;
            }
            catch (SerializationException se)
            {
                QLogger.ErrorFormat("ExecSet SerializationException: " + se.Message);
                return MessageState.SerializeError;
            }
            catch (Exception ex)
            {
                QLogger.ErrorFormat("ExecSet Exception: " + ex.Message);
                return MessageState.UnExpectedError;
            }
            return MessageState.Ok;
        }
#endif
        #endregion

        #region Queue Management

        /// <summary>
        /// Get queue <see cref="MQueue"/> using queue name.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public MQueue Get(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException("QueueManager.Get queueName is null or empty");
            }

            MQueue queue;

            //queue = MQ[queueName];

            //if (queue == null)
            //    throw new KeyNotFoundException("Queue not found: " + queueName);

            //return queue;

            if (MQ.TryGetValue(queueName, out queue))
            {
                return queue;
            }
            else
            {
                //return null;
                throw new KeyNotFoundException("Queue not found: " + queueName);
            }
        }

        /// <summary>
        /// Get queue <see cref="MQueue"/> using queue name.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public bool TryGet(string queueName, out MQueue queue)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException("QueueManager.Get queueName is null or empty");
            }
            return MQ.TryGetValue(queueName, out queue);

            //return null;
            // throw new KeyNotFoundException("Queue not found: " + queueName);
        }

        /// <summary>
        /// Queues
        /// </summary>
        /// <returns></returns>
        public ICollection<MQueue> Items
        {
            get { return MQ.Values; }
        }

        public string[] GetQueueList()
        {
            string[] array = new string[MQ.Count];
            MQ.Keys.CopyTo(array, 0);
            return array;

        }

        public TransStream GetQueueReport(IQueueMessage message)
        {
            MQueue queue = Get(message.Host);
            if (queue == null)
            {
                var ack = new QueueItem()//MessageState.QueueNotFound, "QueueNotFound: " + message.Host, null, message.Host);
                {
                    MessageState = MessageState.QueueNotFound,
                    Label = "QueueNotFound: " + message.Host,
                    Host = message.Host
                };
                Logger.Info("QueueController GetQueueReport QueueNotFound : {0}", message.Host);
                return ack.ToTransStream();
            }
            var report = queue.GetReport();
            string result = null;
            if (report != null)
                result = Nistec.Serialization.JsonSerializer.Serialize(report);
            var item = new QueueItem()//MessageState.Ok, result, null, message.Host);
            {
                MessageState = MessageState.Ok,
                Label = result,
                Host = message.Host
            };
            item.SetBody(report);
            Logger.Info("QueueController GetQueueReport : {0}", result);

            return item.ToTransStream();
        }

        /// <summary>Creates a non-transactional Message Queuing queue at the specified path.</summary>
        /// <returns>A <see cref="T:Nistec.Messaging.MQueue"></see> that represents the new queue.</returns>
        /// <param name="queueName">The path of the queue to create. </param>
        public MQueue AddQueue(string queueName)
        {
            return AddQueue(queueName, false, CoverMode.Memory);
        }

        /// <summary>Creates a transactional or non-transactional Message Queuing queue at the specified path.</summary>
        /// <returns>A <see cref="T:Nistec.Messaging.MQueue"></see> that represents the new queue.</returns>
        /// <param name="queueName">The path of the queue to create. </param>
        /// <param name="isTrans">true to create a transactional queue; false to create a non-transactional queue. </param>
        /// <param name="mode">One of the <see cref="CoverMode"/> options.</param>
        public MQueue AddQueue(string queueName, bool isTrans, CoverMode mode)
        {

            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            if (queueName.Length == 0)
            {
                throw new ArgumentException("InvalidParameter", "queueName");
            }
            if (MQ.ContainsKey(queueName))
            {
                return (MQueue)MQ[queueName];
            }
            //CoverMode mode= CoverMode.None; 
            //if (!string.IsNullOrEmpty(Connection) && QueueProvider > QueueProvider.None)
            //{
            //    mode= CoverMode.ItemsOnly;
            //}

            return AddQueue(new QProperties(queueName, isTrans, mode));
        }

        /// <summary>Creates Message Queuing queue by specified properties.</summary>
        /// <param name="prop">The queue properties. </param>
        /// <returns>A <see cref="T:Nistec.Messaging.MQueue"></see> that represents the new queue.</returns>
        public MQueue AddQueue(IQProperties prop)
        {
            //prop.IsValid(lockKey);

            if (MQ.ContainsKey(prop.QueueName))
            {
                //if (prop.ReloadOnStart)
                //{
                //    MQueue q = new MQueue(prop);
                //    MQ[prop.QueueName] = q;
                //    return q;
                //}
                return (MQueue)MQ[prop.QueueName];
            }
            //if (prop.IsDbQueue)
            //{
            //    AddDbQueue(prop.QueueName, prop.IsTrans);
            //}

            MQueue queue = new MQueue(prop);
            //LoadQueue(queue,prop);
            MQ[prop.QueueName] = queue;

            if (prop.IsTopic)
            {
                TopicDispatcher.TaskAdd(queue, this);
            }

            Logger.Info("AddQueue : {0}", prop.QueueName);

            return queue;
        }



        public IQueueItem AddQueue(QProperties prop, out MQueue mq)
        {

            if (MQ.ContainsKey(prop.QueueName))
            {
                //if (prop.ReloadOnStart)
                //{
                //    MQueue q = new MQueue(prop);
                //    MQ[prop.QueueName] = q;
                //    mq = q;
                //}
                //else
                //{
                //    mq = MQ[prop.QueueName];
                //}
                mq = MQ[prop.QueueName];
                return QueueItem.Ack(MessageState.AllreadyExists, QueueCmd.AddQueue, "AllreadyExists, Name: " + prop.QueueName, null);
            }
            //if (prop.IsDbQueue)
            //{
            //    AddDbQueue(prop.QueueName, prop.IsTrans);
            //}
            MQueue queue = new MQueue(prop);
            //LoadQueue(queue, prop);
            MQ[prop.QueueName] = queue;
            mq = queue;

            if (prop.IsTopic)
            {
                TopicDispatcher.TaskAdd(mq, this);
            }

            Logger.Info("AddQueue : {0}", prop.Print());
            //return  MessageState.Ok;

            return QueueItem.Ack(MessageState.Ok, QueueCmd.AddQueue);
        }

        /// <summary>Deletes a queue on a Message Queuing server.</summary>
        /// <param name="queueName">The location of the queue to be deleted. </param>
        public IQueueItem RemoveQueue(string queueName)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            if (queueName.Length == 0)
            {
                throw new ArgumentException("InvalidParameter", "queueName");
            }
            //RemoveDbQueue(queueName);

            MQueue queue;
            bool removed = MQ.TryRemove(queueName, out queue);

            if (queue.IsTopic)
            {
                TopicDispatcher.TaskRemove(queue);
            }

            Logger.Info("RemoveQueue : {0}, {1}", queueName, removed);

            return QueueItem.Ack(removed ? MessageState.Ok : MessageState.OperationFailed, QueueCmd.RemoveQueue, removed ? "Queue was removed" : "Queue was not removed", null);
        }
        /// <summary>Determines whether a Message Queuing queue exists at the specified path.</summary>
        /// <returns>true if a queue with the specified path exists; otherwise, false.</returns>
        /// <param name="path">The location of the queue to find. </param>
        public IQueueItem Exists(string queueName)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            if (queueName.Length == 0)
            {
                throw new ArgumentException("InvalidParameter", "queueName");
            }
            bool exists = MQ.ContainsKey(queueName);
            return QueueItem.Ack(exists ? MessageState.Ok : MessageState.None, QueueCmd.Exists, exists ? "Queue exists" : "Queue not exists", null);

        }

        public int QueueCount(string queueName)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            if (queueName.Length == 0)
            {
                throw new ArgumentException("InvalidParameter", "queueName");
            }
            MQueue queue = null;
            if (MQ.TryGetValue(queueName, out queue))
            {
                return queue.Count;
            }

            return 0;
        }

        public bool CanQueue(string queueName)
        {
            //TOTDO: FIX THIS
            return MQ.ContainsKey(queueName);
            //return Exists(queueName);
        }

        internal bool IsFatalError(int value)
        {
            bool flag = value == 0;
            return (((value & -1073741824) != 0x40000000) && !flag);
        }

        internal bool IsMemoryError(int value)
        {
            if ((((value != -1072824294) && (value != -1072824226)) && ((value != -1072824221) && (value != -1072824277))) && ((((value != -1072824286) && (value != -1072824285)) && ((value != -1072824222) && (value != -1072824223))) && ((value != -1072824280) && (value != -1072824289))))
            {
                return false;
            }
            return true;
        }

        internal void LoadTopicSubscribers(MQueue mq, string label, string action)
        {
            List<string> hosts = new List<string>();
            if (action=="add")
                hosts.Add(label);

            if (string.IsNullOrEmpty(mq.TargetPath))
            {
                if (action == "add")
                    mq.TargetPath = label;
            }
            else 
            {
                string[] args = mq.TargetPath.SplitTrim('|');
                hosts.AddRange(args);

                if (action == "add") {
                    if (!hosts.Contains(label))
                        hosts.Add(label);
                }
                else if (action == "remove")
                {
                    if (hosts.Contains(label))
                        hosts.Remove(label);
                }
                mq.TargetPath = string.Join("|", hosts);
            }

            //mq.TargetPath = mq.TargetPath.Replace(request.Label, "");
            //mq.TargetPath += request.Label;
            TopicDispatcher.LoadSubscribers(mq);
        }


        public void LoadQueueConfig(bool EnableJournalQueue)
        {

            var config = QueueServerConfig.GetConfig();

            var items = config.RemoteQueueSettings;

            foreach (QueueServerConfigItem item in items)
            {
                //var prop = new QProperties(item.QueueName, item.IsTrans, (CoverMode)item.CoverMode);
                //if (item.IsTopic == false)
                //{
                var mq = AddQueue(item);
                Logger.Info("Queue Added: {0}", item.Print());
                //}
            }

            if (EnableJournalQueue)
            {
                LoadJournalQueue();
            }
        }


        #endregion

        #region Queue Report

        public DataTable GetStatistic()
        {
            DataTable dt = QueueStatisticSchema;
            foreach (MQueue item in this.Items)
            {
                DataRow dr = dt.NewRow();
                dr["QueueName"] = item.QueueName;
                dr["IsTrans"] = item.IsTrans;
                dr["MaxRetry"] = item.MaxRetry;
                dr["Mode"] = item.Mode;
                dr["Enabled"] = item.Enabled;
                dr["Count"] = item.Count;
                dr["MaxCapacity"] = item.MaxCapacity;
                //dr["MaxItemsPerSecond"] = item.MaxItemsPerSecond;

                dt.Rows.Add(dr);
            }
            return dt;

        }

        static DataTable queueStatisticSchema;
        public static DataTable QueueStatisticSchema
        {
            get
            {
                if (queueStatisticSchema == null)
                {
                    DataTable dt = new DataTable("QueueProperties");
                    DataColumn colItemId = new DataColumn("QueueName", typeof(string));
                    dt.Columns.Add(colItemId);
                    dt.Columns.Add(new DataColumn("IsTrans", typeof(bool)));
                    dt.Columns.Add(new DataColumn("MaxRetry", typeof(int)));
                    dt.Columns.Add(new DataColumn("Mode", typeof(CoverMode)));
                    dt.Columns.Add(new DataColumn("Enabled", typeof(bool)));
                    dt.Columns.Add(new DataColumn("Count", typeof(int)));
                    dt.Columns.Add(new DataColumn("MaxCapacity", typeof(int)));
                    dt.Columns.Add(new DataColumn("MaxItemsPerSecond", typeof(int)));


                    dt.PrimaryKey = new DataColumn[] { colItemId };
                    queueStatisticSchema = dt;
                }
                return queueStatisticSchema.Clone();
            }

        }

        public static DataTable QueueItemSchema()
        {

            DataTable dt = new DataTable("QueueItem");
            dt.Columns.Add(new DataColumn("ItemId", typeof(Guid)));
            dt.Columns.Add(new DataColumn("Status", typeof(int)));
            dt.Columns.Add(new DataColumn("MessageId", typeof(int)));
            dt.Columns.Add(new DataColumn("Priority", typeof(int)));
            dt.Columns.Add(new DataColumn("Retry", typeof(int)));
            dt.Columns.Add(new DataColumn("ArrivedTime", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("Subject", typeof(string)));
            dt.Columns.Add(new DataColumn("Sender", typeof(string)));
            dt.Columns.Add(new DataColumn("Host", typeof(string)));
            dt.Columns.Add(new DataColumn("SenderId", typeof(int)));
            dt.Columns.Add(new DataColumn("OperationId", typeof(int)));
            dt.Columns.Add(new DataColumn("Identifer", typeof(int)));
            dt.Columns.Add(new DataColumn("Label", typeof(string)));
            dt.Columns.Add(new DataColumn("TransactionId", typeof(string)));
            dt.Columns.Add(new DataColumn("AppSpecific", typeof(int)));
            dt.Columns.Add(new DataColumn("Segments", typeof(int)));
            dt.Columns.Add(new DataColumn("ClientContext", typeof(string)));
            dt.Columns.Add(new DataColumn("Server", typeof(int)));
            dt.Columns.Add(new DataColumn("TimeOut", typeof(int)));
            dt.Columns.Add(new DataColumn("Host", typeof(string)));

            return dt.Clone();
        }

        #endregion

        #region static methods

        //public IQueueItem[] GetQueueItems(string queueName)
        //{
        //    MQueue q = Get(queueName);
        //    if (q != null)
        //    {
        //       return q.GetQueueItems();
        //    }
        //    return null;
        //}

        //public DataTable GetQueueItemsTable(string queueName)
        //{
        //    MQueue q = Get(queueName);
        //    if (q != null)
        //    {
        //        return q.GetQueueItemsTable();
        //    }
        //    return null;
        //}

        public void ClearAllItems(string queueName)
        {
            MQueue q = Get(queueName);
            if (q != null)
            {
                q.ClearQueueItems(QueueItemType.AllItems);
            }
        }

        public bool CanQueue(string queueName, uint count)
        {
            MQueue q = Get(queueName);
            if (q != null)
            {
                return q.CanQueue(count);
            }
            return false;
        }


        #endregion

    }
}
