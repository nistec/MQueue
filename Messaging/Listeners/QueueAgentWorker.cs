using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Generic;
using System.Collections.ObjectModel;
using Nistec.Messaging.Remote;
using Nistec.Runtime;
using Nistec.Threading;
using System.Security;
using Nistec.Logging;

namespace Nistec.Messaging.Listeners
{
    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection for client.
    /// </summary>
    public class QueueAgentWorker //: INetcellAgent//: QueueReciever
    {
        //const string ActiveMQueueName = "controller";

        #region memebers
        private int isQueueRunning = 0;
        private bool keepAlive = false;
        //QueueReciever MQ;
        private Nistec.Threading.GenericThreadPool threadPool;
        static object m_lock = new object();
        static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        //protected int MaxThread = 1;
        protected int WorkerCount = 1;
        
        //MaxConnection = 30;
        protected int m_Connections;
        //Interval = 30000;
        protected int Server = 0;
        protected int WaitSecond = int.MaxValue;// 120;
        protected int ReadTimeout = -1;
        protected bool Is_Pause = false;
        protected int PauseInterval = 1000;
        public QueueHost Source { get; set; }
        public bool Initilaized { get; protected set; }

        //protected QueueApi QApi;
        protected string HostName;
        protected string QueueAddress;// = "tcp:127.0.0.1:15000?Controller";

        protected QueueApi QApi { get; private set; }

        ILogger _Logger;
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }
        #endregion

        #region ctor

        public QueueAgentWorker(QueueAdapter adapter)//, int interval)
        {

            ReadTimeout = adapter.ReadTimeout;
            MaxConnection = adapter.MaxConnection;
            WorkerCount = adapter.WorkerCount;
            Interval = adapter.Interval;
            //ConnectTimeout = adapter.ConnectTimeout;
            //IsAsync = adapter.IsAsync;
            //IsMultiTask = adapter.IsMultiTask;
            //IsTopic = adapter.IsTopic;
            //IsTrans = adapter.IsTrans;
            //WorkerCount = adapter.WorkerCount;

            QApi = new QueueApi(adapter.Source);
            QApi.ReadTimeout = adapter.ReadTimeout;
            //_Listener= new ListenerQ(this, adapter);
            Initilaized = false;
        }

        #endregion


        public QueueAgentWorker()
        {
            Initilaized = false;
            MaxConnection = 30;
            Interval = 1000;
        }

        protected virtual void Init(string QueueAddress)
        {
            Source = QueueHost.Parse(QueueAddress);
            QApi = new QueueApi(Source);
            HostName = Source.HostName;
            Initilaized = true;
            OnStateChanged(ListenerState.Initilaized);
        }

        #region message events

        /// <summary>
        /// OnError
        /// </summary>
        public event GenericEventHandler<string> ErrorOcurred;
        /// <summary>
        /// QueueMessage Received
        /// </summary>
        public event GenericEventHandler<IQueueMessage> MessageReceived;

        #endregion

        #region INetcellAgent
        //public DynamicWorker ActionWorker { get; private set; }
        public int MaxConnection { get; set; }
        public int Interval { get; set; }
        public bool IsMultiTask { get; set; }
        public bool EnableDynamicWait { get; set; }
        #endregion

        #region start/stop
        protected virtual void OnInfo(string message)
        {

        }
        protected virtual void OnError(string message)
        {
            if (ErrorOcurred != null)
                ErrorOcurred(this, new GenericEventArgs<string>(message)); //OnError(new GenericEventArgs<string>(msg));
            //else if (Adapter.MessageFaultAction != null)
            //    Adapter.MessageFaultAction(msg);
        }
        protected virtual void OnStateChanged(ListenerState state)
        {
            State = state;
            //if (ActionState != null)
            //    ActionState(state);
            //ActionLog(LogLevel.Debug, Name + " " + state.ToString());
            //Log.Info(Types.NZ(HostName, "Unnkown host") + " " + state.ToString());
        }
        public ListenerState State { get; protected set; }

        public void Start()
        {
            if (!Initilaized)
            {
                OnInfo($"QueueListener not Initilaized {HostName}");
                return;
            }
            if (keepAlive || !Initilaized)
                return;
            OnInfo($"QueueListener Start {HostName}");
            keepAlive = true;
            threadPool = new Nistec.Threading.GenericThreadPool(WorkerCount);
            threadPool.StartThreadPool(new ParameterizedThreadStart(QueueProcess));
            OnInfo($"QueueListener Started {HostName}");

            //base.Start();
        }

        public void Stop()
        {
            OnInfo($"QueueListener Stop {HostName}");
            try
            {

                keepAlive = false;
                int count = 0;
                while (Thread.VolatileRead(ref isQueueRunning) > 0 && count < 30)
                {
                    Thread.Sleep(1000);
                    count++;
                    //Log.Debug("QueueStop:" + count.ToString());
                }

                if (threadPool != null)
                {
                    try
                    {
                        threadPool.StopThreadPool();
                        threadPool.Dispose();
                    }
                    catch (SecurityException)
                    {
                        //e = e;
                    }
                    catch (ThreadStateException)
                    {
                        //ex = ex;
                        // In case the thread has been terminated 
                        // after the check if it is alive.
                    }
                }
                State = ListenerState.Stoped;
                OnInfo($"SessionListener Stoped: {HostName}");
            }
            catch (Exception ex)
            {
                OnError($"MQueueAgent Queue {HostName},  error:{ex.Message}");
            }
        }

        public bool Pause(OnOffState onOff)
        {
            if (State != ListenerState.Started)
                return Is_Pause;

            if ((onOff == OnOffState.Toggle && State == ListenerState.Paused) || onOff == OnOffState.Off)
            {
                Is_Pause = false;
                OnStateChanged(ListenerState.Started);
            }
            else
            {
                Is_Pause = true;
                OnStateChanged(ListenerState.Paused);
                int intervalSeconds = 60;
                PauseInterval = 1000 * ((intervalSeconds < 1) ? 60 : intervalSeconds);
            }
            return Is_Pause;
        }

        public void Shutdown(bool waitForWorkers)
        {
            if (threadPool == null)
                return;
            threadPool.StopThreadPool();
            threadPool.Dispose();
            //ActionWorker.Shutdown(waitForWorkers);
            State = ListenerState.Down;
            //Adapter.Dispose();
            OnInfo($"SessionListener.Shutdown: {HostName}");
        }

        public virtual NameValueArgs Report()
        {
            var args = new NameValueArgs();
            args.Add("Name", HostName);
            args.Add("Interval", Interval);
            //args.Add("WaitType", WaitType.ToString());
            //args.Add("EnableDynamicWait", EnableDynamicWait);
            //args.Add("EnableResetEvent", EnableResetEvent);
            args.Add("State", State.ToString());
            args.Add("MaxConnection", MaxConnection);
            //args.Add("IsMultiTask", IsMultiTask);
            //args.Add("ActiveConnections", ActiveConnections);
            //args.Add("ActiveConnections", ActiveConnections);
            if (threadPool != null)
                args.Add("MaxThreads", threadPool.MaxThread);
            return args;
        }

        public string Command(string cmd, bool wait)
        {
            switch (cmd)
            {
                case "Stop":
                    Stop(); break;
                case "Start":
                    Start(); break;
                case "Shutdown":
                    Shutdown(wait); break;
                default:
                    return "Commnd not suppported, " + cmd;
            }
            return State.ToString();
        }

        protected virtual bool ShouldPause()
        {
            return false;
        }

        #endregion

        #region Publish

        protected virtual void OnMessageReceived(IQueueMessage message)
        {
           
        }

        /*
        protected virtual void OnMessageReceived(IQueueMessage message)
        {
            if (message != null)
            {
                PublishMessageAsync(message, (ack) => {
                    OnInfo("SwifterAgent OnReceivedEvent Result: {0}", ack.ToString());
                    CommitAsync(ack, message);
                });//.ConfigureAwait(false);
            }
        }

        protected virtual void PublishMessageAsync(IQueueMessage message, Action<IAck> ack)
        {

        }

        //{
        //    OnInfo("SwifterAgent PublishMessageAsync ExecuteAsync {0}", message.Print());
        //    var msgIn = message.GetBody<MessageIn>();
        //    var publisher = new PublishIn(msgIn);
        //    await publisher.InvokeAsync(ack);

        //}
        */

        protected virtual void CommitAsync(IAck ack, IQueueMessage message)
        {
            if (ack.IsOk)
                QueueApi.Get(Source).Commit(message.GetPtr()); // Commit(message.GetPtr());
            else
                QueueApi.Get(Source).Abort(message.GetPtr());// Abort(message.GetPtr());
        }

        protected virtual IQueueMessage Consume()
        {
            return QApi.Consume(WaitSecond);
        }
        private void ConsumeInternal()
        {
            Task.Run(() =>
            {
                var message = Consume();
                if (message != null)
                {
                    OnMessageReceived(message);
                    OnInfo($"QueueListener QApi.Consume end Identifier: {message.Identifier}, MessageState: {message.MessageState}  BodyLength: {message.BodyLength()}, m_Connections {m_Connections}");
                    QueueApi.Get(Source).Commit(message.GetPtr());
                }
                autoResetEvent.Set();
            });
        }

        #endregion

        #region QueueProcess 4.7.2.3

        //public override void OnMessageReceived(IQueueItem item)
        //{
        //    base.OnMessageReceived(item);
        //}

        //public override void OnMessageFault(string message)
        //{
        //    base.OnMessageFault(message);
        //}
        int pause = 0;
        private void QueueProcess(object state)
        {

            while (keepAlive)
            {
                //Queue_Queue activeQueue = null;

                IQueueMessage queueItem = null;

                try
                {
                    /*
                    while (keepAlive &&  ShouldPause())//(!(keepAlive && App_Servers.IsEnableQueue(server)))
                    {
                        if (pause == 10)
                        {
                            OnInfo($"QueueListener QueueProcess ShouldPause {pause}");
                            pause = 0;
                        }
                        ++pause;

                        Thread.Sleep(60000);
                    }
                    pause = 0;
                    while (Thread.VolatileRead(ref m_Connections) >= MaxConnection)
                    {
                        OnInfo($"QueueListener QueueProcess ShouldPause {++pause}, m_Connections {m_Connections}");
                        Thread.Sleep(100);
                    }
                    */
                    while (Is_Pause)//(!(keepAlive && App_Servers.IsEnableQueue(server)))
                    {
                        Task.Delay(Interval);
                    }

                    while (Thread.VolatileRead(ref m_Connections) >= MaxConnection)
                    {
                        Task.Delay(100);
                    }

                    if (!keepAlive)
                    {
                        break;
                    }


                    //Console.WriteLine("Queue load...{0}",Thread.CurrentThread.Name);

                    Interlocked.Increment(ref isQueueRunning);

                    //ThreadPool.QueueUserWorkItem(QueueMessageWorker, null);

                    //OnInfo($"QueueListener QueueProcess Begin {isQueueRunning}, m_Connections {m_Connections}");

                    lock (m_lock)
                    {
                        ConsumeInternal();
                        autoResetEvent.WaitOne();
                    }

                    /*
                    lock (m_lock)
                    {
                        OnInfo($"QueueListener QApi.Consume begin, m_Connections {m_Connections}");

                        queueItem = Consume();

                        if (queueItem != null)
                            OnInfo($"QueueListener QApi.Consume end Identifier: {queueItem.Identifier}, MessageState: {queueItem.MessageState}  BodyLength: {queueItem.BodyLength()}, m_Connections {m_Connections}");
                        else
                            OnInfo($"QueueListener QApi.Consume end, m_Connections {m_Connections}");

                        //queueItem = QApi.Dequeue();
                        //Log.WarnFormat("QueueProcess Dequeue");

                        //OnInfo($"QueueListener QApi.Consume , m_Connections {m_Connections}");

                        if (queueItem != null && queueItem.MessageState == Nistec.Messaging.MessageState.Receiving && queueItem.BodyStream() != null)
                        {
                            Interlocked.Increment(ref m_Connections);
                            OnInfo($"QueueListener QApi.Dequeue {queueItem.Identifier}, m_Connections {m_Connections}");
                            ThreadPool.QueueUserWorkItem(QueueWorker, queueItem);
                        }
                        else if (queueItem != null && (int)queueItem.MessageState >= 20)
                        {
                            OnError("QueueListener dequeue failed : " + queueItem.Label + ", " + queueItem.Print());
                        }
                        }
                        */
                    }
                catch (ThreadAbortException)
                {
                    OnError($"Warn: QueueListener ThreadAborted {HostName}");
                }
                catch (Exception ex)
                {
                    OnError($"QueueListener {HostName}, Error:{ex.Message + " Trace:" + ex.StackTrace} ");

                    try
                    {

                        //lock (typeof(QueueApi))
                       // {

                            QApi.Abort(queueItem.GetPtr());
                            // Queue_Context.Commit(queueItem.QueueId, (int)QueueState.Error);
                        //}

                    }
                    catch (Exception exx)
                    {
                        OnError($"QueueListener {HostName}, Error:{exx.Message} ");
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref isQueueRunning);
                }
                Task.Delay(100);// Interval);
            }
            OnError($"Warn: QueueListener not keep Alive {HostName}");
        }
 /*
        void QueueWorker(Object threadContext)
        {
            try
            {
                OnInfo($"QueueListener QueueWorker started");


                IQueueMessage queueItem = (IQueueMessage)threadContext;

                if (queueItem != null)// && !queueItem.IsEmpty)
                {
                    OnInfo($"QueueListener ExecuteAsync {queueItem.Print()}");

                    //var message = queueItem.GetBody<QueueMessage>();    //Message.Deserialize(queueItem.ToJson();//.BodyStream);

                    OnMessageReceived(queueItem);

                    //using (MessageExecuter sch = new MessageExecuter(message))
                    //{
                    //    sch.ExecuteAsync();
                    //}

                    //lock (typeof(QueueApi))
                    //{
                        QApi.Commit(queueItem.GetPtr()); //Queue_Context.Commit(queueItem.QueueId);
                    //}
                    //activeCampaign.Dispose();
                }
            }
            //catch (NetcellException ex)
            //{
            //    if (((int)ex.Status) > 5000)
            //    {
            //        this.HoldDequeue = true;
            //    }
            //}
            catch (ThreadAbortException)
            {
                OnError($"QueueListener Error ThreadAborted {HostName}");
            }
            catch (Exception ex)
            {
                OnError($"QueueListener {HostName},  Error :{ex.Message} ");
            }

            Interlocked.Decrement(ref m_Connections);

            OnInfo($"QueueListener QueueWorker finished");
        }
        */
/*
        void QueueMessageWorker(object state)
        {
            IQueueMessage queueItem = null;

            try
            {
                OnInfo($"QueueMessageWorker started");

                queueItem = QApi.Consume(WaitSecond);
                //queueItem = QApi.Dequeue();
                //Log.WarnFormat("QueueProcess Dequeue");

                //OnInfo($"QueueListener QApi.Consume , m_Connections {m_Connections}");

                if (queueItem != null && queueItem.MessageState == Nistec.Messaging.MessageState.Receiving && queueItem.BodyStream() != null)
                {
                    Interlocked.Increment(ref m_Connections);
                    OnInfo($"QueueMessageWorker QApi.Dequeue {queueItem.Identifier}, m_Connections {m_Connections}");
                    ThreadPool.QueueUserWorkItem(QueueWorker, queueItem);
                    OnMessageReceived(queueItem);
                    QApi.Commit(queueItem.GetPtr());
                }
                else if (queueItem != null && (int)queueItem.MessageState >= 20)
                {
                    OnError("QueueMessageWorker dequeue failed : " + queueItem.Label + ", " + queueItem.Print());
                }

            }
            //catch (NetcellException ex)
            //{
            //    if (((int)ex.Status) > 5000)
            //    {
            //        this.HoldDequeue = true;
            //    }
            //}
            catch (ThreadAbortException)
            {
                OnError($"QueueMessageWorker Error ThreadAborted {HostName}");
            }
            catch (Exception ex)
            {
                OnError($"QueueMessageWorker {HostName},  Error :{ex.Message} ");
            }

            if (queueItem != null)
                Interlocked.Decrement(ref m_Connections);

            OnInfo($"QueueMessageWorker finished");
        }
*/
        #endregion

    }

}
