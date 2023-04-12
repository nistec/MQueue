using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Linq;
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
using System.Threading.Tasks;

namespace Nistec.Messaging.Server
{
    public class TopicDispatcher : IListener
    {
        #region members
        Dictionary<string, SubscriberTask> Tasks;
         int SenderInterval = 100;
        bool Initialized = false;
        //bool _SendDirect = true;
        //public bool SendDirect { get { return _SendDirect; } set { if (!Initialized) _SendDirect = value; } }
        QueueController Controller;
        public ILogger Logger { get; set; }
        public ListenerState State { get; private set; }
         DynamicWaitType WaitType;
        int MaxThread;
        int Interval;
        int maxTopicConnections = 5;
        internal int MaxRetry = 3;
        #endregion

        #region ctor
        public TopicDispatcher(QueueController controller, DynamicWaitType waitType = DynamicWaitType.DynamicWait, int maxThread = 1, int interval = 100)
        {

            maxTopicConnections=AgentManager.Settings.MaxTopicConnections;
            MaxRetry = AgentManager.Settings.MaxRetry;
            Initialized = false;
            //SendDirect = true;
            Logger = QLogger.Logger.ILog;
            WaitType = waitType;
            MaxThread = maxThread;
            Interval = interval;

            Tasks = new Dictionary<string, SubscriberTask>();
        }

        internal void LoadSubscribers(IQueueReceiver queue)
        {
            SubscriberTask st = null;
            if (Tasks.TryGetValue(queue.QueueName, out st))
            {
                st.LoadSubscribers(queue);
            }
        }
        #endregion

        #region Tasks

        public void TaskAdd(IQueueReceiver queue, QueueController controller)
        {
            if (!Tasks.ContainsKey(queue.QueueName))
            {
                Tasks.Add(queue.QueueName, new SubscriberTask(queue, controller, maxTopicConnections));
            }
        }

        public void TaskRemove(IQueueReceiver queue)
        {
            if (Tasks.ContainsKey(queue.QueueName))
            {
                Tasks.Remove(queue.QueueName);
            }
        }

        public bool IsTaskRunning(string taskName)
        {
            SubscriberTask task;
            if (Tasks.TryGetValue(taskName, out task))
            {
                return task.GetState() == System.Threading.Tasks.TaskStatus.Running;
            }
            return false;
        }
        #endregion

        #region DynamicWorker

        DynamicWorker ActionWorker;
        int actionsOk = 0;

        private void StartDynamicWorker()
        {

            if (ActionWorker != null)
                return;

            ActionWorker = new DynamicWorker(WaitType, MaxThread, Interval)
            {
                ActionLog = (LogLevel level, string message) =>
                {
                    Logger.Log((LoggerLevel)level, message);
                },
                ActionTask = () =>
                {
                    try
                    {
                        //Interlocked.Exchange(ref actionsOk,0);

                        foreach (var subtask in Tasks.Values)
                        {
                            //(task.Status == TaskStatus.Created &&
                            
                            if (subtask.activeConnections < subtask.MaxConnections)
                            {

                                if (subtask.SendTask())
                                {
                                    Interlocked.Increment(ref actionsOk);
                                    //return true;
                                }

                                //Task task = Task.Factory.StartNew(() =>
                                //{
                                //    if (subtask.SendTask())
                                //    {
                                //        Interlocked.Increment(ref actionsOk);
                                //    }
                                //});

                                ////Put the current thread into waiting state until it receives the signal
                                //autoResetEvent.WaitOne();

                                //subtask.SendTask();
                            }
                        }

                        if (Thread.VolatileRead(ref actionsOk) >= 1)
                        {
                            Interlocked.Exchange(ref actionsOk, 0);
                            return true;
                        }
                        return false;

                    }
                    catch (Exception ex)
                    {
                        Logger.Exception("TopicSbscriberListener.ActionWorker Worker error ", ex, false);
                    }
                    return false;
                },
                Interval = 100,
                MaxThreads = 1
            };
            ActionWorker.Start();

        }

        public void Start()
        {
            if (ActionWorker != null)
                return;
            StartDynamicWorker();
            Logger.Info("TopicSbscriberListener Started");
        }

        public void Stop()
        {
            if (ActionWorker != null)
            {
                ActionWorker.Stop();
                Logger.Info("TopicSbscriberListener Stopted");
            }
        }
        public bool Pause(OnOffState onff)
        {
            Logger.Info("TopicSbscriberListener PausePersistQueue");
            return ActionWorker.Pause(onff);
        }

        public void Shutdown(bool waitForWorkers)
        {
            if (ActionWorker != null)
            {
                ActionWorker.Shutdown(waitForWorkers);
                Logger.Info("TopicSbscriberListener Shutdown!");
            }
        }
        #endregion

        #region DynamicWorker
        /*
       DynamicWorker ActionWorker;
       bool EnablePersistQueue = true;

       public void StartDynamicWorker()
       {


           if (EnablePersistQueue)
           {
               if (ActionWorker != null)
                   return;

               Logger.Info("TopicDispatcher DynamicWorker Starting...");

               ActionWorker = new DynamicWorker(DynamicWaitType.DynamicWait)
               {
                   ActionLog = (LogLevel level, string message) =>
                   {
                       if (Logger != null)
                           Logger.Log((LoggerLevel)level, message);
                   },
                   ActionTask = () =>
                   {
                       IQueueMessage item;
                       try
                       {
                           if (EventQueue.TryDequeue(out item))
                           {
                               SendItem(item as QueueMessage);

                               return true;
                           }
                       }
                       catch (Exception ex)
                       {
                           Logger.Exception("TopicDispatcher Sender Worker error ", ex);
                       }
                       return false;
                   },
                   ActionState = (ListenerState state) => {

                       State = state;
                   },
                   Interval = 100,
                   MaxThreads = 1,
                   Name = "TopicDispatcher"
               };
               ActionWorker.Start();

               Logger.Info("TopicDispatcher DynamicWorker Started!");
           }
       }
      

        public bool IsStarted { get; protected set; }

        public void Start()
        {
            if (IsStarted)
                return;
            if (AgentManager.Settings.EnableTopicController)
            {
                StartDynamicWorker();

                if (Logger != null)
                    Logger.Info("TopicDispatcher Started...");
                IsStarted = true;
            }
        }

        public void Stop()
        {
            if (ActionWorker != null)
                ActionWorker.Stop();
            if (Logger != null)
                Logger.Info("TopicDispatcher Stopted!");
            IsStarted = false;
        }

        public bool Pause(OnOffState onff)
        {
            bool paused = false;
            if (ActionWorker != null)
                paused = ActionWorker.Pause(onff);
            if (Logger != null)
                Logger.Info("TopicDispatcher Paused");
            return paused;
        }

        public void Shutdown(bool waitForWorkers)
        {

            if (ActionWorker != null)
                ActionWorker.Shutdown(waitForWorkers);
            if (Logger != null)
                Logger.Info("TopicDispatcher Shutdown!");
            IsStarted = false;
        }
         */
        #endregion

        /*
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
                Thread.Sleep(3000);
                for (int i = 0; i < MaxThreads; i++)
                {
                    //thDequeueWorker[i].Join();
                    //if (!SendDirect)

                    //thSenderWorker[i].Join();
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
                IQueueMessage item;
                try
                {
                    if (EventQueue.TryDequeue(out item))
                    {
                        SendItem(item as QueueMessage);
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
        */

        #region Add/Send

        //public void AddItem(QueueMessage item)
        //{
        //    EventQueue.Enqueue(item);
        //}

        /*
        public IQueueAck AddItem(QueueMessage item, TopicPublisher Publisher)
        {
            Logger.Debug("TopicController AddItem : TopicName:{0}, Identifier:{1}", Publisher.TopicName, item.Identifier);

            foreach (var subscriber in Publisher.Subscribers.Values)
            {
                var copy = item.Copy();
                copy.Host = subscriber.QHost.RawHostAddress;
                if (subscriber.IsHold)
                {
                    DbCover.Add(item);
                    //return new QueueAck(MessageState.Holded, item);
                }
                else
                {
                    //copy.Args = new NameValueArgs();
                    //copy.Args["HostAddress"] = subscriber.Host;
                    EventQueue.Enqueue(copy);
                }
            }

            return new QueueAck(MessageState.Arrived, item);
        }

        public IQueueAck AddItem(QueueMessage item, TopicSubscriber subscriber)
        {
            var copy = item.Copy();
            copy.Host = subscriber.QHost.RawHostAddress;
            if (subscriber.IsHold)
            {
                DbCover.Add(item);
                return new QueueAck(MessageState.Holded, item);
            }
            else
            {
                //copy.Args = new NameValueArgs();
                //copy.Args["HostAddress"] = subscriber.Host;
                EventQueue.Enqueue(copy);
                return new QueueAck(MessageState.Arrived, item);
            }
        }


        public IQueueAck SendItem(QueueMessage item)
        {
            try
            {
                Logger.Debug("TopicController SendItem : Host:{0}, Identifier:{1}", item.Host, item.Identifier);

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

        */

        //public IQueueAck SendSubscriber(TopicSubscriber subscriber, QueueMessage copy)
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

        #endregion

    }

    public class SubscriberTask
    {
        #region members
        Task task;
        int wait = 10;
        bool EnableResetEvent;
        bool lockWasTaken = false;
        object _locker = new object();
        ILogger Logger = QLogger.Logger.ILog;
        IQueueReceiver m_queue;
        string queueName;
        static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        internal int activeConnections = 0;
        internal int MaxConnections=1;
        QueueController Controller;
        internal QueueHost Subscriber;
        bool IsConnectionOk = false;
        int ErrorCounter = 0;
        int MaxRetry = 3;
        const int MaxErrorCounter= 5;
        #endregion

        #region ctor
        public SubscriberTask(IQueueReceiver queue, QueueController controller, int maxConnections = 1, bool enableResetEvent = true)
        {
            //action = act;
            MaxRetry = AgentManager.Settings.MaxRetry;
            Controller = controller;
            MaxConnections = maxConnections;
            m_queue = queue;
            queueName = queue.QueueName;
            EnableResetEvent = enableResetEvent;
            //Subscribers = new Dictionary<string, QueueHost>();
            
            LoadSubscribers(queue);
            //task = new Task(() => action());
        }

        internal void LoadSubscribers(IQueueReceiver queue, bool enablePingValidation = false)
        {
            if (string.IsNullOrEmpty(queue.TargetPath))
            {
                throw new ArgumentException("Invalid TargetPath for SubscriberTask name: " + queue.QueueName);
            }
            var args = queue.TargetPath.SplitTrim('|');
            //int argsCount = args.Length;
            //int counter = 0;
            if(args.Length < 1)
            {
                throw new ArgumentException("TargetPath is incorrecr for SubscriberTask name: " + queue.QueueName);

            }
            Subscriber = QueueHost.Parse(args[0]);

            if(Subscriber==null)
            {
                throw new ArgumentException("Unable to LoadSubscribers name: " + queue.QueueName);
            }

            IsConnectionOk = Subscriber.PingValidate();
            if(IsConnectionOk==false)
            {
                Logger.Warn("PingValidate error: " + Subscriber.RawHostAddress);
            }
        }
        #endregion

        #region State

        public TaskStatus GetState()
        {
            if (task == null)
                return TaskStatus.WaitingForActivation;
            return task.Status;
        }


        void State()
        {
            try
            {
                Monitor.Enter(_locker);
                lockWasTaken = true;

                if (task == null)
                    return;
                if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted || task.Status == TaskStatus.Canceled)
                {
                    task.Dispose();
                    task = null;
                }
            }
            finally
            {
                if (lockWasTaken) Monitor.Exit(_locker);
                lockWasTaken = false;
            }
        }
        #endregion

        #region Action

        internal bool SendTask()
        {
            if (Thread.VolatileRead(ref activeConnections) >= MaxConnections)
            {
                Thread.Sleep(100);
                return false;
            }
            IQueueMessage item;

            try
            {
                Interlocked.Increment(ref activeConnections);
                //autoResetEvent.Set();
                                                              
                if (!IsConnectionOk)
                {
                    IsConnectionOk = Subscriber.PingValidate();
                    if (!IsConnectionOk)
                    {
                        Logger.Warn("PingValidate error: " + Subscriber.RawHostAddress);
                        if (ErrorCounter > MaxErrorCounter)
                            ErrorCounter = 0;
                        ErrorCounter++;
                        Thread.Sleep(ErrorCounter * 1000);
                        return false;
                    }
                }

                if (m_queue.TryDequeue(out item))
                {
                    SendItem(item as QueueMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("SubscriberTask SendTask Worker error ", ex);
            }
            finally
            {
                Interlocked.Decrement(ref activeConnections);
            }
            return false;
        }

        public void SendItem(QueueMessage item)
        {
            try
            {
                Logger.Debug("SubscriberTask SendItem : Host:{0}, Identifier:{1}", item.Host, item.Identifier);
                //bool JournalAdd = false;

                var api = new QueueApi(Subscriber);
                var ack = api.PublishItem(item);
                if (!ack.MessageState.IsStateOk())
                {
                    IsConnectionOk = false;

                    if (item.Retry > MaxRetry)
                    {
                        Controller.JournalAddItem(item);
                    }
                    else
                    {
                        ack = m_queue.Requeue(item);
                    }
                }

                /*  multi subscribers
                foreach (var qh in Subscribers.Values)
                {
                    var api = new QueueApi(qh);
                    var ack = api.PublishItem(item);
                    if (!ack.MessageState.IsStateOk())
                    {
                        if(item.Retry> TopicDispatcher.MaxRetry)
                        {
                            ack= m_queue.Requeue(item);
                            if (!ack.MessageState.IsStateOk())
                            {
                                if (JournalAdd == false)
                                {
                                    Controller.JournalAddItem(item);
                                    JournalAdd = true;
                                }
                            }
                        }
                        
                    }
                }
                */
            }
            catch (Exception ex)
            {
                Logger.Exception("SubscriberTask Topic Sender Subscriber error ", ex);
                Controller.JournalAddItem(item);
            }
            //autoResetEvent.Set();
        }

        void Print(string prefix)
        {
            if (task != null)
                Logger.Debug(prefix + " task : {0}, status: {1} ", task.Id, task.Status.ToString());
            else
                Logger.Debug(prefix + " is null");

        }

        #endregion

        //internal void Run()
        //{

        //    State();
        //    if (task == null)
        //    {
        //        try
        //        {
        //            //Monitor.Enter(_locker);
        //            //lockWasTaken = true;
        //            if (Thread.VolatileRead(ref activeConnections) < MaxConnections)
        //            {
        //                Thread.Sleep(1200);
        //                //counter++;
        //                //if (counter % 10 == 0)
        //                Log.WarnFormat("Scheduler MaxConnection exceeded, Connections:{0}, {1}", m_Connections, counter);
        //            }

        //            Interlocked.Increment(ref activeConnections);
        //            task = new Task(() => DequeueTask());
        //            task.Start();
        //            Print("Start");
        //            task.Wait();
        //            Print("Wait");
        //            //if (EnableResetEvent)
        //            autoResetEvent.WaitOne();
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Exception("SubscriberTask error ", ex, false);
        //        }
        //        //finally
        //        //{
        //        //    if (lockWasTaken) Monitor.Exit(_locker);
        //        //    lockWasTaken = false;
        //        //}
        //    }
        //    Print("Run");
        //}

        //private void Worker()
        //{
        //    autoResetEvent.Set();
        //    Print("autoResetEvent.Set");
        //    action();
        //    Print("Worker");
        //}

        #region DynamicWorker
#if (false)
        DynamicWaitType WaitType = DynamicWaitType.None;
        int MaxThread = 1;
        int Interval = 1000;

        DynamicWorker ActionWorker;

        private void StartDynamicWorker()
        {

            if (ActionWorker != null)
                return;
            int counter = 0;

            ActionWorker = new DynamicWorker(WaitType, MaxThread, Interval, MaxConnections)
            {
                ActionLog = (LogLevel level, string message) =>
                {
                    Logger.Log((LoggerLevel)level, message);
                },
                ActionTask = () =>
                {
                    IQueueMessage item;
                    try
                    {

                        //if (Thread.VolatileRead(ref activeConnections) >= maxConnections)
                        //{
                        //    //Thread.Sleep(100);
                        //    counter++;
                        //    if (counter % 10 == 0)
                        //        Logger.Warn("SubscriberTask MaxConnection exceeded, Connections:{0} of {1}", activeConnections, maxConnections);
                        //    return true;
                        //}
                        //else
                        //{
                        //    Interlocked.Increment(ref activeConnections);

                            //autoResetEvent.WaitOne();

                            if (m_queue.TryDequeue(out item))
                            {
                                SendItem(item as QueueMessage);

                                return true;
                            }
                        
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception("TopicDispatcher Sender Worker error ", ex);
                    }
                    //finally
                    //{
                    //    Interlocked.Decrement(ref activeConnections);
                    //}
                    return false;
                },
                Interval = 100,
                MaxThreads = 1
            };
            ActionWorker.Start();

        }

        public void Start()
        {
            if (ActionWorker != null)
                return;
            StartDynamicWorker();
            Logger.Info("TopicSbscriberListener Started:" + queueName);
        }

        public void Stop()
        {
            if (ActionWorker != null)
            {
                ActionWorker.Stop();
                Logger.Info("TopicSbscriberListener Stopted: " + queueName);
            }
        }
        public bool Pause(OnOffState onff)
        {
            Logger.Info("TopicSbscriberListener PausePersistQueue: " + queueName);
            return ActionWorker.Pause(onff);
        }

        public void Shutdown(bool waitForWorkers)
        {
            if (ActionWorker != null)
            {
                ActionWorker.Shutdown(waitForWorkers);
                Logger.Info("TopicSbscriberListener Shutdown!, " + queueName);
            }
        }
#endif

        #endregion
    }


    /*
     public class TopicController
     {

         bool Initialized;
         bool _SendDirect = true;
         public bool SendDirect { get { return _SendDirect; } set { if (!Initialized) _SendDirect = value; } }
         public ILogger Logger { get; set; }
         public bool IsHold { get; set; }
         //public static SyncTimerDispatcher<TransactionItem> SyncTimer = new SyncTimerDispatcher<TransactionItem>();

         TopicPublisher Publisher;
         //List<TopicSubscriber> Subscribers;
         QueueController Controller;
         MQueue Q;


         public static TopicController Create(string queueName)
         {
             MQueue mq = AgentManager.Queue.Get(queueName);
             if (mq == null)
                 throw new Exception("Queue not exists: " + queueName);

             return new TopicController(mq);
         }

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

         public TopicSubscriber GetSubscriber(string subscriberName) {

             TopicSubscriber ts;
             if (Publisher.TryGetSubscriber(subscriberName, out ts)) {
                 return ts;
             }
             return ts;
         }

         public bool TryGetSubscriber(string subscriberName, out TopicSubscriber ts)
         {
             return (Publisher.TryGetSubscriber(subscriberName, out ts));
         }

         public void AddSubscriber(TopicSubscriber ts)
         {
             Publisher.AddSubscriber(ts);
         }
         public void RemoveSubscriber(string subscriberName)
         {
             Publisher.RemoveSubscriber(subscriberName);
         }

         public void HoldSubscriber(string subscriberName)
         {
             Publisher.HoldSubscriber(subscriberName);
         }

         public void ReleaseHoldSubscriber(string subscriberName)
         {
             TopicSubscriber ts = GetSubscriber(subscriberName);
             if (ts != null)
             {
                 ts.IsHold = false;
                 DbCover.RenqueueAction(ts.Host, 0, (item)=>
                 {
                     if (item != null)
                         SendSubscriber(ts, item);
                 });
             }
         }

         public void HoldTopic()
         {
             IsHold = true;
         }

         public void ReleaseHoldTopic()
         {
             IsHold = false;
             DbCover.RenqueueAction(Q.QueueName, 0, (item) =>
             {
                 if (item != null)
                     Q.Enqueue(item);
             });
         }


         public IQueueAck AddItem(QueueMessage item)
         {
             Logger.Debug("TopicController AddItem : SendDirect:{0}, Identifier: {1}, Destination: {2}", SendDirect, item.Identifier, item.Destination);

             if (IsHold)
             {
                 DbCover.Add(item);
                 return new QueueAck(MessageState.Holded, item);
             }
             else if (SendDirect)
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

                     if (subscriber.IsHold)
                     {

                     }
                     else
                     {
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
                 }

                 return new QueueAck(MessageState.Arrived, item);
             }
             //return new QueueAck(acks.ToArray());
         }


         public IQueueAck SendSubscriber(TopicSubscriber subscriber, QueueMessage copy)
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
     */

#if (false)

    public class b_TopicDispatcher : IListener
    {
        Dictionary<string, IQueueReceiver> Topics;
        PriorityPersistQueue EventQueue;
        //Topics m_Topic;
        //Dictionary<string,bool> HoldSubscribers;
        //int DequeueInterval = 100;
        int SenderInterval = 100;
        bool Initialized = false;
        bool _SendDirect = true;
        public bool SendDirect { get { return _SendDirect; } set { if (!Initialized) _SendDirect = value; } }
        //bool IsStarted = false;
        QueueController Controller;
        public ILogger Logger { get; set; }
        public ListenerState State { get; private set; }
        public b_TopicDispatcher(QueueController controller)
        {
            IsStarted = false;
            Controller = controller;
            Initialized = false;
            SendDirect = true;
            var topicProp = new QProperties("TopicEvent", false, CoverMode.Memory);
            EventQueue = new PriorityPersistQueue(topicProp);
            Logger = QLogger.Logger.ILog;
            //HoldSubscribers = new Dictionary<string, bool>();
            //m_Topic = new Topics();
            Topics = new Dictionary<string, IQueueReceiver>();
        }

                
        internal void AddTopic(IQueueReceiver qr)
        {
            if (qr.IsTopic && !Topics.Keys.Contains(qr.QueueName))
            {
                Topics.Add(qr.QueueName, qr);
            }

        }
        
        internal void RemoveTopic(IQueueReceiver qr)
        {
            if (qr.IsTopic && Topics.Keys.Contains(qr.QueueName))
            {
                Topics.Remove(qr.QueueName);
            }
        }

        //public void HoldSubscriber(string name)
        //{
        //    HoldSubscribers[name] = true;
        //}
        //public void HoldReease(string name)
        //{
        //    HoldSubscribers.Remove(name);

        //    EventQueue.HoldReEnqueue();
        //}

        void TopicActionWorker()
        {

            foreach(var topic in Topics.Values)
            {
                IQueueMessage item;
                try
                {
                    if (topic.TryDequeue(out item))
                    {
                        SendItem(item as QueueMessage);

                        //return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception("TopicDispatcher Sender Worker error ", ex);
                }
               // return false;
            }
        }


        #region DynamicWorker

        DynamicWorker ActionWorker;
        bool EnablePersistQueue=true;

        public void StartDynamicWorker()
        {


            if (EnablePersistQueue)
            {
                if (ActionWorker != null)
                    return;

                Logger.Info("TopicDispatcher DynamicWorker Starting...");

                ActionWorker = new DynamicWorker(DynamicWaitType.DynamicWait)
                {
                    ActionLog = (LogLevel level, string message) =>
                    {
                        if (Logger != null)
                            Logger.Log((LoggerLevel)level, message);
                    },
                    ActionTask = () =>
                    {
                        IQueueMessage item;
                        try
                        {
                            if (EventQueue.TryDequeue(out item))
                            {
                                SendItem(item as QueueMessage);

                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Exception("TopicDispatcher Sender Worker error ", ex);
                        }
                        return false;
                    },
                    ActionState = (ListenerState state) => {

                        State = state;
                    },
                    Interval = 100,
                    MaxThreads = 1,
                    Name="TopicDispatcher"
                };
                ActionWorker.Start();

                Logger.Info("TopicDispatcher DynamicWorker Started!");
            }
        }
        public bool IsStarted { get; protected set; }

        public void Start()
        {
            if (IsStarted)
                return;
            if (AgentManager.Settings.EnableTopicController)
            {
                StartDynamicWorker();

                if (Logger != null)
                    Logger.Info("TopicDispatcher Started...");
                IsStarted = true;
            }
        }

        public void Stop()
        {
            if (ActionWorker != null)
                ActionWorker.Stop();
            if (Logger != null)
                Logger.Info("TopicDispatcher Stopted!");
            IsStarted = false;
        }

        public bool Pause(OnOffState onff)
        {
            bool paused = false;
            if (ActionWorker != null)
                paused= ActionWorker.Pause(onff);
            if (Logger != null)
                Logger.Info("TopicDispatcher Paused");
            return paused;
        }

        public void Shutdown(bool waitForWorkers)
        {

            if (ActionWorker != null)
                ActionWorker.Shutdown(waitForWorkers);
            if (Logger != null)
                Logger.Info("TopicDispatcher Shutdown!");
            IsStarted = false;
        }

        #endregion

        /*
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
                Thread.Sleep(3000);
                for (int i = 0; i < MaxThreads; i++)
                {
                    //thDequeueWorker[i].Join();
                    //if (!SendDirect)

                    //thSenderWorker[i].Join();
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Topic Stop error ", ex);
            }
            Initialized = false;
        }

        //#region background worker

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
                IQueueMessage item;
                try
                {
                    if (EventQueue.TryDequeue(out item))
                    {
                        SendItem(item as QueueMessage);
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
        */


        //public void AddItem(QueueMessage item)
        //{
        //    EventQueue.Enqueue(item);
        //}

        public IQueueAck AddItem(QueueMessage item, TopicPublisher Publisher)
        {
            Logger.Debug("TopicController AddItem : TopicName:{0}, Identifier:{1}", Publisher.TopicName, item.Identifier);

            foreach (var subscriber in Publisher.Subscribers.Values)
            {
                var copy = item.Copy();
                copy.Host = subscriber.QHost.RawHostAddress;
                if (subscriber.IsHold)
                {
                    DbCover.Add(item);
                    //return new QueueAck(MessageState.Holded, item);
                }
                else
                {
                    //copy.Args = new NameValueArgs();
                    //copy.Args["HostAddress"] = subscriber.Host;
                    EventQueue.Enqueue(copy);
                }
            }

            return new QueueAck(MessageState.Arrived, item);
        }

        public IQueueAck AddItem(QueueMessage item, TopicSubscriber subscriber)
        {
            var copy = item.Copy();
            copy.Host = subscriber.QHost.RawHostAddress;
            if (subscriber.IsHold)
            {
                DbCover.Add(item);
                return new QueueAck(MessageState.Holded, item);
            }
            else
            {
                //copy.Args = new NameValueArgs();
                //copy.Args["HostAddress"] = subscriber.Host;
                EventQueue.Enqueue(copy);
                return new QueueAck(MessageState.Arrived, item);
            }
        }


        public IQueueAck SendItem(QueueMessage item)
        {
            try
            {
                Logger.Debug("TopicController SendItem : Host:{0}, Identifier:{1}", item.Host, item.Identifier);

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

        //public IQueueAck SendSubscriber(TopicSubscriber subscriber, QueueMessage copy)
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

        public void SendSubscriber(TopicSubscriber subscriber, QueueMessage item)
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
        public void Send(string topicId, QueueMessage item)
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
                IQueueMessage item;
                try
                {
                    if (EventQueue.TryDequeue(out item))
                    {
                        Send(item.Host, item as QueueMessage);
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
                IQueueMessage item;
                try
                {
                    foreach (var entry in MQ)
                    {
                        var q = entry.Value;

                        if (q.TryDequeue(out item))
                        {

                            if (SendDirect)
                                Send(item.Host, item as QueueMessage);
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