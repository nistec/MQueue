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
using Nistec.Logging;
using Nistec.Threading;
using Nistec.Channels;

namespace Nistec.Messaging.Listeners
{

    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection.
    /// </summary>
    public abstract class AgentSessionListener<T> where T : IAgentMessage//, IListener
    {
        #region members

        public const int DefaultInterval = 1000;

        //protected QueueAdapter Adapter;

        CancellationTokenSource canceller = new CancellationTokenSource();

        public HostChannel Source { get; protected set; }

        public bool EnableResetEvent { get; set; }
        public int Interval { get; set; }//{ get { return MinWait; } }
        public int ConnectTimeout { get; protected set; }
        public int ReadTimeout { get; protected set; } 
        public bool IsAlive { get; protected set; }
        public int WorkerCount { get; protected set; }
        public int MaxConnection { get; set; }
        public bool IsMultiTask { get; set; }

        public bool IsAsync { get; protected set; }
        public ListenerState State { get; protected set; }

        ILogger _Logger;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }

        public bool EnableDynamicWait { get; set; }
        public string HostName { get; private set; }

        #endregion

        #region ctor
        public AgentSessionListener()//, int interval)
        {
        }
        public AgentSessionListener(AgentAdapter adapter)//, int interval)
        {
            Init(adapter);
        }

        public virtual void Init(AgentAdapter adapter)
        {
            if (adapter == null)
            {
                throw new ArgumentNullException("adapter");
            }
            if (adapter.Source == null)
            {
                throw new ArgumentNullException("adapter.Source");
            }
            //Adapter = adapter;

            //_Owner = owner;
            Source = adapter.Source;
            HostName = Source.HostName;

            //_TransferTo = adapter.TransferTo;

            //_ServerName = channel.ServerName;
            //_QueueName = channel.Source;
            //IntervalWait = interval < MinWait ? MinWait : interval;// 1000;

            Interval = adapter.Interval;
            ConnectTimeout = adapter.ConnectTimeout;
            ReadTimeout = adapter.ReadTimeout;
            WorkerCount = adapter.WorkerCount;
            MaxConnection = adapter.MaxConnection;
            IsMultiTask = adapter.IsMultiTask;
            IsAsync = adapter.IsAsync;
            EnableResetEvent = true;// adapter.EnableResetEvent;
            EnableDynamicWait = adapter.EnableDynamicWait;
            //_ActionTransfer = adapter.AckAction;
            //_AdapterOperation = adapter.OperationType;

            //QApi = new QueueApi(adapter.Source);
            //QApi.ReadTimeout = adapter.ReadTimeout;

            State = ListenerState.Initilaized;
        }

        #endregion

        #region message events

        /// <summary>
        /// ErrorOcurred
        /// </summary>
        public event GenericEventHandler<string> ErrorOcurred;
        /// <summary>
        /// QueueMessage Received
        /// </summary>
        public event GenericEventHandler<T> MessageReceived;
        /// <summary>
        /// OnMessageReceived, when override events will not fired
        /// </summary>
        /// <param name="message"></param>
        protected virtual void OnMessageReceived(T message)
        {
            //Commit(message.GetPtr());
            if (MessageReceived != null)
                MessageReceived(this, new GenericEventArgs<T>(message));
            //else if (Adapter.MessageReceivedAction != null)
            //    Adapter.MessageReceivedAction(message);

        }

        protected virtual void OnEvent(string source, string msg)
        {
            
        }
        protected virtual void OnInfo(string message)
        {
            if (_Logger != null)
                _Logger.Info(message);
        }

        protected void OnErrorOcurred(string msg)
        {
            Console.WriteLine("ErrorOcurred: " + msg);
            if (ErrorOcurred != null)
                ErrorOcurred(this, new GenericEventArgs<string>(msg)); //OnErrorOcurred(new GenericEventArgs<string>(msg));
            //else if (Adapter.MessageFaultAction != null)
            //    Adapter.MessageFaultAction(msg);
        }

        protected virtual void OnError(string message)
        {
            OnErrorOcurred(message);
            if (_Logger != null)
                _Logger.Error(message);
            //OnErrorOcurred(new GenericEventArgs<string>(message));
        }

        #endregion

        #region override

        protected virtual void OnStateChanged(ListenerState state)
        {
            //NLog.InfoFormat("OnStateChanged {0}, State: {1}", HostName, state.ToString());
        }
        protected virtual bool ShouldPause()
        {
            return false;//Hold Sender Service
        }

        //protected abstract IQueueAck Send(QueueMessage message);

        protected abstract T Receive();
        //protected abstract void Receive(IDynamicWait aw);
        protected virtual async Task<T> ReceiveAsync()
        {
            return await Task.Run(() =>
            {
                return Receive();
            });
        }
        protected void Receive(AutoResetEvent are, Action<T> onReceived)
        {
            var message = Receive();
            if (message != null)
                onReceived(message);
            are.Set();
        }
        //protected void Receive(Action<T> onReceived)
        //{
        //    var message = Receive();
        //    if (message != null)
        //        onReceived(message);
        //}
        protected async Task ReceiveAsync(AutoResetEvent are, Action<T> onReceived)
        {
            var message = await ReceiveAsync();
            if (message != null)
                onReceived(message);
            are.Set();
        }
        //protected async Task ReceiveAsync(Action<T> onReceived)
        //{
        //    var message = await ReceiveAsync();
        //    if (message != null)
        //        onReceived(message);
        //}

        //public virtual void Commit(Ptr ptr)
        //{
        //    QueueApi.Get(Source).Commit(ptr);
        //}

        //public virtual void Abort(Ptr ptr)
        //{
        //    QueueApi.Get(Source).Abort(ptr);
        //}
        protected virtual void CommitAsync(IAck ack)
        {
            //await Task.Run(null);
        }
        protected virtual void Commit(IAck ack)
        {

        }
        protected virtual void Abort(IAck ack)
        {

        }
        #endregion

        #region start/stop

        bool lockWasTaken = false;
        object _locker = new object();
        Thread[] _workers;
        long delay;
        long m_conecctions = 0;
        long m_pause = 0;

        public void Start()
        {
            if (IsAlive)
            {
                return;
            }
            _workers = new Thread[WorkerCount];
            ThreadStart threadWorker = IsAsync ? new ThreadStart(TaskWorkerAsync) : new ThreadStart(TaskWorker);
            for (int i = 0; i < WorkerCount; i++)
            {
                _workers[i] = new Thread(new ThreadStart(threadWorker));
                _workers[i].IsBackground = true;
                _workers[i].Start();
            }
            State = ListenerState.Started;
            OnInfo("AgentSessionListener Started");
        }
        public void Stop()
        {
            Shutdown(true);
            State = ListenerState.Stoped;
            OnInfo("AgentSessionListener Stoped");
        }
        public void Shutdown(bool waitForWorkers)
        {
            IsAlive = false;

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in _workers)
                    worker.Join();
        }

        public bool Pause(OnOffState onOff)
        {
            //if (ActionWorker == null)
            //    return false;
            bool paused = onOff == OnOffState.On;// ActionWorker.Pause(onOff);

            if (paused)
            {
                Interlocked.Exchange(ref m_pause, 1);
                State = ListenerState.Paused;
                OnEvent($"AgentSessionListener.Pause", $"State: {State}, HostName: {HostName}");
                OnInfo($"AgentSessionListener Paused: {HostName}");
            }
            else
            {
                Interlocked.Exchange(ref m_pause, 0);
                State = ListenerState.Started;
                OnInfo($"AgentSessionListener No Paused: {HostName}");
            }
            return paused;
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
        public bool IsRunning
        {
            get
            {
                return State == ListenerState.Started;
            }
        }

        public int ActiveConnections
        {
            get
            {
                return (int)m_conecctions;
            }
        }
        public NameValueArgs Report()
        {

            var args = new NameValueArgs();
            args.Add("HostName", HostName);
            args.Add("MaxConnection", MaxConnection);
            args.Add("ActiveConnections", ActiveConnections);
            args.Add("Interval", Interval);
            //args.Add("WaitType", WaitType.ToString());
            args.Add("EnableDynamicWait", EnableDynamicWait);
            args.Add("EnableResetEvent", EnableResetEvent);
            args.Add("State", State.ToString());
            args.Add("IsMultiTask", IsMultiTask);
            args.Add("MaxThreads", WorkerCount);
            return args;
        }
        #endregion

        #region worker

        public void Delay(TimeSpan time)
        {
            Interlocked.Exchange(ref delay, (long)time.TotalMilliseconds);
        }

        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        protected virtual void TaskWorker()
        {
            IsAlive = true;
            // Start queue listener...
            OnInfo("QListener started...");

            while (IsAlive)
            {

                try
                {

                    if (Interlocked.Read(ref delay) > 0)
                    {
                        Task.Delay((int)delay);
                        Interlocked.Exchange(ref delay, 0);
                    }
                    while (Interlocked.Read(ref m_pause) > 0)
                    {
                        Task.Delay(1000);
                    }
                    while (Interlocked.Read(ref m_conecctions) >= MaxConnection)
                    {
                        Task.Delay(1000);
                    }

                    Monitor.Enter(_locker);
                    lockWasTaken = true;

                    Interlocked.Increment(ref m_conecctions);
                    Task.Run(() =>
                    {
                        Receive(autoResetEvent,OnMessageReceived);
                    });
                    autoResetEvent.WaitOne();
                }
                catch (Exception ex)
                {
                    autoResetEvent.Set();
                    OnError("QListener error: " + ex.Message);
                }
                finally
                {
                    if (lockWasTaken) Monitor.Exit(_locker);
                }
                Interlocked.Decrement(ref m_conecctions);
                Task.Delay(100);
            }

            OnInfo("QListener stoped");

        }

        protected virtual void TaskWorkerAsync()
        {
            IsAlive = true;
            // Start queue listener...
            OnInfo("QListener async started...");

            while (IsAlive)
            {
                try
                {

                    if (Interlocked.Read(ref delay) > 0)
                    {
                        Task.Delay((int)delay);
                        Interlocked.Exchange(ref delay, 0);
                    }
                    while (Interlocked.Read(ref m_pause) > 0)
                    {
                        Task.Delay(1000);
                    }
                    while (Interlocked.Read(ref m_conecctions) >= MaxConnection)
                    {
                        Task.Delay(1000);
                    }
                    Monitor.Enter(_locker);
                    lockWasTaken = true;

                    Interlocked.Increment(ref m_conecctions);
                    var task = Task.Run(async () =>
                    {
                        await ReceiveAsync(autoResetEvent,OnMessageReceived);
                    });
                    autoResetEvent.WaitOne(Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    autoResetEvent.Set();
                    OnError("QListener async error: " + ex.Message);
                }
                finally
                {
                    if (lockWasTaken) Monitor.Exit(_locker);
                }
                Interlocked.Decrement(ref m_conecctions);
                Task.Delay(Interval);
            }

            OnInfo("QListener stoped...");

        }

        #endregion
    }

}
