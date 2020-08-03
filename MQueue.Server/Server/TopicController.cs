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
using Nistec.Threading;
using System.Timers;
using System.Threading;
using Nistec.Messaging.Server;

namespace Nistec.Messaging.Server
{

    public class TopicDispatcher
    {

        PriorityPersistQueue EventQueue;
        //Topics m_Topic;

        //int DequeueInterval = 100;
        int SenderInterval = 100;
        bool Initialized = false;
        bool _SendDirect = true;
        public bool SendDirect { get { return _SendDirect; } set { if (!Initialized) _SendDirect = value; } }
        //bool IsStarted = false;
        QueueController Controller;
        public ILogger Logger { get; set;}

        public TopicDispatcher(QueueController controller)
        {
            Controller = controller;
            Initialized = false;
            SendDirect = true;
            var topicProp = new QProperties("TopicEvent", false, CoverMode.Memory);
            EventQueue = new PriorityPersistQueue(topicProp);
            Logger = QLogger.Logger.ILog;
            //m_Topic = new Topics();
        }

        public void Start()
        {

            if (Initialized)
                return;
            KeepAlive = true;
            StartInternal();
            Initialized = true;
        }
        public void Stop()
        {

            KeepAlive = false;

            try
            {
                for (int i = 0; i < MaxThreads; i++)
                {
                    //thDequeueWorker[i].Join();
                    //if (!SendDirect)
                    thSenderWorker[i].Join();
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Topic Stop error ", ex);
            }
            Initialized = false;
        }

        #region background worker

        bool KeepAlive;
        int MaxThreads = 1;
        //Thread[] thDequeueWorker;
        Thread[] thSenderWorker;


        void StartInternal()
        {
            //thDequeueWorker = new Thread[MaxThreads];
            thSenderWorker = new Thread[MaxThreads];

            //for (int i = 0; i < MaxThreads; i++)
            //{
            //    thDequeueWorker[i] = new Thread(new ThreadStart(DequeueWorker));
            //    thDequeueWorker[i].IsBackground = true;
            //    thDequeueWorker[i].Start();
            //}

            //if (!SendDirect)
            //{
            //    for (int i = 0; i < MaxThreads; i++)
            //    {
            //        thSenderWorker[i] = new Thread(new ThreadStart(SenderWorker));
            //        thSenderWorker[i].IsBackground = true;
            //        thSenderWorker[i].Start();
            //    }
            //}

            for (int i = 0; i < MaxThreads; i++)
            {
                thSenderWorker[i] = new Thread(new ThreadStart(SenderWorker));
                thSenderWorker[i].IsBackground = true;
                thSenderWorker[i].Start();
            }

        }

        public void SenderWorker()
        {
            while (KeepAlive)
            {
                IQueueItem item;
                try
                {
                    if (EventQueue.TryDequeue(out item))
                    {
                        SendItem(item as QueueItem);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception("Topic Sender Worker error ", ex);
                }
                Thread.Sleep(SenderInterval);
            }
            Logger.Warn("Topic Sender Worker stoped!");
        }

        #endregion

        //public void AddItem(QueueItem item)
        //{
        //    EventQueue.Enqueue(item);
        //}

        public IQueueAck AddItem(QueueItem item, TopicPublisher Publisher)
        {
            foreach (var subscriber in Publisher.Subscribers.Values)
            {
                var copy = item.Copy();
                copy.Host = subscriber.QHost.RawHostAddress;
                //copy.Args = new NameValueArgs();
                //copy.Args["HostAddress"] = subscriber.Host;
                EventQueue.Enqueue(copy);

            }

            return new QueueAck(MessageState.Arrived, item);
        }

        public IQueueAck AddItem(QueueItem item, TopicSubscriber subscriber)
        {
            var copy = item.Copy();
            copy.Host = subscriber.QHost.RawHostAddress;
            //copy.Args = new NameValueArgs();
            //copy.Args["HostAddress"] = subscriber.Host;
            EventQueue.Enqueue(copy);
            return new QueueAck(MessageState.Arrived, item);
        }


        public IQueueAck SendItem(QueueItem item)
        {
            try
            {
                string address = item.Host;//.Args["HostAddress"];
                QueueHost qh = QueueHost.Parse(address);
                item.Host = qh.HostName;
                var api = new QueueApi(qh);
                var ack = api.PublishItem(item);
                if (!ack.MessageState.IsStateOk())
                {
                    Controller.JournalAddItem(item);
                }

                return ack;
            }
            catch (Exception ex)
            {
                Logger.Exception("Topic Sender Subscriber error ", ex);
                Controller.JournalAddItem(item);
                return new QueueAck(MessageState.FailedEnqueue, item);
            }
        }

        //public IQueueAck SendSubscriber(TopicSubscriber subscriber, QueueItem copy)
        //{
        //    try
        //    {
        //        copy.Host = subscriber.HostName;
        //        var api = new QueueApi(subscriber.QHost);
        //        var ack = api.Send(copy);
        //        if (!ack.MessageState.IsStateOk())
        //        {
        //            Controller.JournalAddItem(copy);
        //        }

        //        return ack;
        //    }
        //    catch (Exception ex)
        //    {
        //        Netlog.Exception("Topic Sender Subscriber error ", ex);
        //        Controller.JournalAddItem(copy);
        //        return new QueueAck(MessageState.FailedEnqueue, copy);
        //    }
        //}

    }


    public class TopicController
    {

        bool Initialized;
        bool _SendDirect = true;
        public bool SendDirect { get { return _SendDirect; } set { if (!Initialized) _SendDirect = value; } }
        public ILogger Logger { get; set; }
        //public static SyncTimerDispatcher<TransactionItem> SyncTimer = new SyncTimerDispatcher<TransactionItem>();

        TopicPublisher Publisher;
        //List<TopicSubscriber> Subscribers;
        QueueController Controller;
        MQueue Q;


        public TopicController(MQueue mq)
        {
            Q = mq;
            Logger = QLogger.Logger.ILog;
            Controller = AgentManager.Queue;
            Initialized = false;
            SendDirect = mq.Mode == CoverMode.Rout;
            AddTopic(mq.QueueName, mq.TargetPath);
        }

        //public void LoadTopicConfig()
        //{

        //    var config = QueueServerConfig.GetConfig();

        //    var items = config.RemoteQueueSettings;

        //    foreach (QueueServerConfigItem item in items)
        //    {
        //        //var prop = new QProperties(item.QueueName, item.IsTrans, (CoverMode)item.CoverMode);
        //        if (item.IsTopic)
        //        {
        //            var publisher = new TopicPublisher()
        //            {
        //                TopicId = item.QueueName,
        //                TopicName = item.QueueName
        //            };

        //            string[] strsubs = item.TargetPath.SplitTrim('|');
        //            foreach (var s in strsubs)
        //            {
        //                publisher.AddSubscriber(TopicSubscriber.Create(s, item.QueueName));
        //            }

        //            var mq = AddQueue(item);
        //            QLogger.InfoFormat("Queue Topic Added: {0}", item.Print());
        //        }
        //    }
        //}

        public void AddTopic(QueueServerConfigItem item)
        {

            Publisher = new TopicPublisher()
            {
                TopicId = item.QueueName,
                TopicName = item.QueueName
            };

            string[] strsubs = item.TargetPath.SplitTrim('|');
            foreach (var s in strsubs)
            {
                AddSubscriber(TopicSubscriber.Create(s, item.QueueName));
            }

            //var mq = AddQueue(item);
            Logger.Info("Queue Topic Added: {0}", item.Print());
            Initialized = true;
        }

        public void AddTopic(string QueueName, string TargetPath)
        {

            Publisher = new TopicPublisher()
            {
                TopicId = QueueName,
                TopicName = QueueName
            };

            string[] strsubs = TargetPath.SplitTrim('|');
            foreach (var s in strsubs)
            {
                AddSubscriber(TopicSubscriber.Create(s, QueueName));
            }
            Logger.Info("Queue Topic Added: {0}", QueueName);
            Initialized = true;
        }

        public void AddSubscriber(TopicSubscriber ts)
        {
            Publisher.AddSubscriber(ts);
        }

        public IQueueAck AddItem(QueueItem item)
        {

            if (SendDirect)
            {
                //ack = SendSubscriber(subscriber, copy);
                return Controller.TopicDispatcher.AddItem(item, Publisher);
            }
            else
            {
                List<IQueueAck> acks = new List<IQueueAck>();
                IQueueAck ack;

                foreach (var subscriber in Publisher.Subscribers.Values)
                {
                    var copy = item.Copy();
                    //if (SendDirect)
                    //{
                    //    //ack = SendSubscriber(subscriber, copy);
                    //    ack = Controller.TopicDispatcher.AddItem(item, subscriber);
                    //}
                    //else
                    //{
                    copy.Host = subscriber.HostName;
                    ack = Q.Enqueue(copy);
                    //}
                    acks.Add(ack);
                }

                return new QueueAck(MessageState.Arrived, item);
            }
            //return new QueueAck(acks.ToArray());
        }


        public IQueueAck SendSubscriber(TopicSubscriber subscriber, QueueItem copy)
        {
            try
            {
                copy.Host = subscriber.HostName;
                var api = new QueueApi(subscriber.QHost);
                var ack = api.PublishItem(copy);
                if (!ack.MessageState.IsStateOk())
                {
                    Controller.JournalAddItem(copy);
                }

                return ack;
            }
            catch (Exception ex)
            {
                Logger.Exception("Topic Sender Subscriber error ", ex);
                Controller.JournalAddItem(copy);
                return new QueueAck(MessageState.FailedEnqueue, copy);
            }
        }

    }


#if (false)
    public class TopicController: QueueController
    {
         //ConcurrentDictionary<string, MQueue> MQ;
         PriorityMemQueue EventQueue;
         //TransactionDispatcher m_TransDispatcher;
         Topics m_Topic;

        int DequeueInterval = 100;
        int SenderInterval = 100;
        bool Initialized;
        bool _SendDirect = true;
        public bool SendDirect { get { return _SendDirect; } set { if(!Initialized) _SendDirect = value; } }

        //public static SyncTimerDispatcher<TransactionItem> SyncTimer = new SyncTimerDispatcher<TransactionItem>();
        

        public TopicController() : base()
        {
            //base.Interval=
            Initialized = false;
            SendDirect = true;
            //int numProcs = Environment.ProcessorCount;
            //int concurrencyLevel = numProcs * 2;
            //int initialCapacity = 5;
            //MQ = new ConcurrentDictionary<string, MQueue>(concurrencyLevel, initialCapacity);
            //m_TransDispatcher = new TransactionDispatcher();
            EventQueue = new PriorityMemQueue("local");
            m_Topic = new Topics();
            //m_TransDispatcher.SyncItemCompleted += new SyncItemEventHandler<TransactionItem>(m_TransDispatcher_SyncCompleted);
        }

        //void m_TransDispatcher_SyncCompleted(object sender, SyncItemEventArgs<TransactionItem> e)
        //{
        //    //TODO:

        //    //e.Item.SyncExpired();
        //}


        public void LoadTopicConfig()
        {

            var config = QueueServerConfig.GetConfig();

            var items = config.RemoteQueueSettings;

            foreach (QueueServerConfigItem item in items)
            {
                //var prop = new QProperties(item.QueueName, item.IsTrans, (CoverMode)item.CoverMode);
                if (item.IsTopic)
                {
                    var publisher = new TopicPublisher()
                    {
                        TopicId = item.QueueName,
                        TopicName = item.QueueName
                    };

                    string[] strsubs = item.TargetPath.SplitTrim('|');
                    foreach (var s in strsubs)
                    {
                        publisher.AddSubscriber(TopicSubscriber.Create(s, item.QueueName));
                    }
                    
                    var mq = AddQueue(item);
                    QLogger.InfoFormat("Queue Topic Added: {0}", item.Print());
                }
            }
        }

        public override void Start() {

            if (Initialized)
                return;
            KeepAlive = true;
            StartInternal();
            IsStarted = true;
        }
        public override void Stop() {

            KeepAlive = false;

            try
            {
                for (int i = 0; i < MaxThreads; i++)
                {
                    thDequeueWorker[i].Join();
                    if (!SendDirect)
                        thSenderWorker[i].Join();
                }
            }
            catch (Exception ex)
            {
                Netlog.Exception("Topic Stop error ", ex);
            }
        }

        bool KeepAlive;
        int MaxThreads = 2;
        Thread[] thDequeueWorker;
        Thread[] thSenderWorker;


        void StartInternal()
        {
            thDequeueWorker = new Thread[MaxThreads];
            thSenderWorker = new Thread[MaxThreads];

            for (int i=0;i<MaxThreads;i++)
            {
                thDequeueWorker[i] = new Thread(new ThreadStart(DequeueWorker));
                thDequeueWorker[i].IsBackground = true;
                thDequeueWorker[i].Start();
            }

            if (!SendDirect)
            {
                for (int i = 0; i < MaxThreads; i++)
                {
                    thSenderWorker[i] = new Thread(new ThreadStart(SenderWorker));
                    thSenderWorker[i].IsBackground = true;
                    thSenderWorker[i].Start();
                }
            }

        }

        public void SendSubscriber(TopicSubscriber subscriber, QueueItem item)
        {
            try
            {
                var api = QueueApi.Get(subscriber.Protocol);
                //var message = item.ToMessage();// Message.Create(item.GetItemStream());
                api.Send(item);
            }
            catch (Exception ex)
            {
                Netlog.Exception("Topic Sender Subscriber error ", ex);
            }
        }
        public void Send(string topicId, QueueItem item)
        {
            TopicPublisher publisher;

            if (m_Topic.TryGetPublisher(topicId, out publisher))
            {
                var subs = m_Topic.GetSubscribers(topicId);

                foreach (var subscriber in subs)
                {
                    SendSubscriber(subscriber, item);
                }

            }
        }

        public void SenderWorker()
        {
            while (KeepAlive)
            {
                IQueueItem item;
                try
                {
                    if (EventQueue.TryDequeue(out item))
                    {
                        Send(item.Host, item as QueueItem);
                    }
                }
                catch (Exception ex)
                {
                    Netlog.Exception("Topic Sender Worker error ", ex);
                }
                Thread.Sleep(SenderInterval);
            }
            Netlog.Warn("Topic Sender Worker stoped!");
        }

        public void DequeueWorker()
        {
            while (KeepAlive)
            {
                IQueueItem item;
                try
                {
                    foreach (var entry in MQ)
                    {
                        var q = entry.Value;

                        if (q.TryDequeue(out item))
                        {

                            if (SendDirect)
                                Send(item.Host, item as QueueItem);
                            else
                                EventQueue.Enqueue(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Netlog.Exception("Topic Dequeue Worker error ", ex);
                }

                Thread.Sleep(DequeueInterval);
            }
            Netlog.Warn("Topic Dequeue Worker stoped!");
        }
     
    }
#endif

}