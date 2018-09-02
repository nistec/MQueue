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

namespace Nistec.Messaging.Topic
{
    public class TopicController: QueueController
    {
         //ConcurrentDictionary<string, MQueue> MQ;
         PriorityMemQueue EventQueue;
         //TransactionDispatcher m_TransDispatcher;
         Topics m_Topic;

        int DequeueInterval = 100;
        int SenderInterval = 100;
        bool Initialized;

        //public static SyncTimerDispatcher<TransactionItem> SyncTimer = new SyncTimerDispatcher<TransactionItem>();
        

        public TopicController() : base()
        {
            //base.Interval=
            Initialized = false;
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


        public override void Start() {

            if (Initialized)
                return;
            KeepAlive = true;
            StartInternal();
        }
        public override void Stop() {

            KeepAlive = false;

            try
            {
                for (int i = 0; i < MaxThreads; i++)
                {
                    thDequeueWorker[i].Join();
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

            for (int i = 0; i < MaxThreads; i++)
            {
                thSenderWorker[i] = new Thread(new ThreadStart(SenderWorker));
                thSenderWorker[i].IsBackground = true;
                thSenderWorker[i].Start();
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
}
