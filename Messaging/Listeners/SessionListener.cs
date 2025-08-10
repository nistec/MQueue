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

namespace Nistec.Messaging.Listeners
{

    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection.
    /// </summary>
    public abstract class SessionListener : IListener
    {
        #region members

        public const int DefaultInterval = 1000;

        //protected QueueAdapter Adapter;

        CancellationTokenSource canceller = new CancellationTokenSource();

        public QueueHost Source { get; protected set; }

        public bool EnableResetEvent { get; set; }
        public int Interval { get; set; }//{ get { return MinWait; } }
        public int ConnectTimeout { get; protected set; }
        public int ReadTimeout { get; protected set; } 
        public bool IsAlive { get; protected set; }
        public int WorkerCount { get; protected set; }
        public int MaxConnection { get; set; }
        public bool IsMultiTask { get; set; }

        public bool IsAsync { get; protected set; }
        public ListenerState State { get; private set; }

        ILogger _Logger;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }

        public bool EnableDynamicWait { get; set; }
        public string HostName { get; private set; }

        #endregion

        #region ctor
        public SessionListener()//, int interval)
        {
        }
        public SessionListener(QueueAdapter adapter)//, int interval)
        {
            Init(adapter);
        }

        public virtual void Init(QueueAdapter adapter)
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
        public event GenericEventHandler<IQueueMessage> MessageReceived;
        /// <summary>
        /// OnMessageReceived, when override events will not fired
        /// </summary>
        /// <param name="message"></param>
        protected virtual void OnMessageReceived(IQueueMessage message)
        {
            //Commit(message.GetPtr());
            if (MessageReceived != null)
                MessageReceived(this, new GenericEventArgs<IQueueMessage>(message));
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

        protected abstract IQueueMessage Receive();
        //protected abstract void Receive(IDynamicWait aw);
        protected virtual async Task<IQueueMessage> ReceiveAsync()
        {
            return await Task.Run(() =>
            {
                return Receive();
            });
        }
        protected void Receive(AutoResetEvent are, Action<IQueueMessage> onReceived)
        {
            var message = Receive();
            if (message != null)
                onReceived(message);
            are.Set();
        }
        //protected void Receive(Action<IQueueMessage> onReceived)
        //{
        //    var message = Receive();
        //    if (message != null)
        //        onReceived(message);
        //}
        protected async Task ReceiveAsync(AutoResetEvent are, Action<IQueueMessage> onReceived)
        {
            var message = await ReceiveAsync();
            if (message != null)
                onReceived(message);
            are.Set();
        }
        //protected async Task ReceiveAsync(Action<IQueueMessage> onReceived)
        //{
        //    var message = await ReceiveAsync();
        //    if (message != null)
        //        onReceived(message);
        //}

        public virtual void Commit(Ptr ptr)
        {
            QueueApi.Get(Source).Commit(ptr);
        }

        public virtual void Abort(Ptr ptr)
        {
            QueueApi.Get(Source).Abort(ptr);
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
            OnInfo("SessionListener Started");
        }
        public void Stop()
        {
            Shutdown(true);
            State = ListenerState.Stoped;
            OnInfo("SessionListener Stoped");
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
                OnEvent($"SessionListener.Pause", $"State: {State}, HostName: {HostName}");
                OnInfo($"SessionListener Paused: {HostName}");
            }
            else
            {
                Interlocked.Exchange(ref m_pause, 0);
                State = ListenerState.Started;
                OnInfo($"SessionListener No Paused: {HostName}");
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


#if (false)
    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection.
    /// </summary>
    public abstract class SessionListener : IListener
    {
    #region members

        public const int DefaultInterval = 1000;

        protected QueueAdapter Adapter;

        CancellationTokenSource canceller = new CancellationTokenSource();

        QueueHost _Source;
        public QueueHost Source { get { return _Source; } }

        public bool EnableResetEvent { get; set; }
        //int _Interval;
        public int Interval { get; private set; }//{ get { return MinWait; } }
        int _ConnectTimeout;
        public int ConnectTimeout { get { return _ConnectTimeout; } }
        int _ReadTimeout;
        public int ReadTimeout { get { return _ReadTimeout; } }

        bool _isalive = false;
        public bool IsAlive { get { return _isalive; } }
        int _WorkerCount;
        public int WorkerCount { get { return _WorkerCount; } }
        int _MaxConnection;
        public int MaxConnection { get { return _MaxConnection; } }
        bool _IsMultiTask;
        public bool IsMultiTask { get { return _IsMultiTask; } }

        bool _IsAsync;
        public bool IsAsync { get { return _IsAsync; } }
        public ListenerState State { get; private set; }

        ILogger _Logger;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }

        public bool EnableDynamicWait { get; set; }
        public string HostName { get; private set; }
        //AdapterOperations _AdapterOperation;
        //public AdapterOperations OperationType { get { return _AdapterOperation; } }

        //Action<IQueueMessage> _Action;
        //public Action<IQueueMessage> QueueAction { get { return _Action; } }

        //Action<IQueueAck> _ActionTransfer;
        //public Action<IQueueAck> TransferAction { get { return _ActionTransfer; } }

        //IListenerHandler _Owner;

        //protected QueueApi QApi { get; private set;}

        /*
        const int MaxWait = 1000000;
        const int LargeWait = 5000;
        const int MedWait = 500;
        const int MinWait = 100;
        int IntervalWait = 0;
        public bool EnableDynamicWait { get; set; }
        public int DynamicWait
        {
            get { return (int) (EnableDynamicWait?(IntervalWait < MinWait ? MinWait : IntervalWait): MinWait); }
        }
        public int CalcDynamicWait(bool ack)
        {
            if (ack)
            {
                if (IntervalWait > LargeWait)
                    return Interlocked.Exchange(ref IntervalWait, IntervalWait - 1000);
                else if (IntervalWait > MedWait)
                    return Interlocked.Exchange(ref IntervalWait, IntervalWait - 100);
                else if (IntervalWait > MinWait)
                    return Interlocked.Exchange(ref IntervalWait, IntervalWait - 10);
                //return (int)Interlocked.Decrement(ref IntervalWait);
            }
            else
            {
                if (IntervalWait < MaxWait)
                    return (int)Interlocked.Increment(ref IntervalWait);
            }
            return (int)IntervalWait;
        }
        */
    #endregion

    #region message events
        
        /// <summary>
        /// ErrorOcurred
        /// </summary>
        public event GenericEventHandler<string> ErrorOcurred;
        /// <summary>
        /// QueueMessage Received
        /// </summary>
        public event GenericEventHandler<IQueueMessage> MessageReceived;

        //void DoMessageReceived(IQueueMessage message)
        //{
        //    OnMessageReceived(message);
        //    //OnMessageReceived(new GenericEventArgs<IQueueMessage>(message));
        //}
                
        ///// <summary>
        ///// Occured when message received.
        ///// </summary>
        ///// <param name="e"></param>
        //protected virtual void OnMessageReceived(GenericEventArgs<IQueueMessage> e)
        //{
        //    //if (Adapter.MessageReceivedAction != null)
        //    //    Adapter.MessageReceivedAction(e.Args);
        //    if (MessageReceived != null)
        //        MessageReceived(this, e);
        //}
        
        //void DoErrorOcurred(string message)
        //{
        //    OnErrorOcurred(message);
        //}


        protected virtual void OnMessageReceived(IQueueMessage message)
        {
            if (message != null)
            {
                //if (_Action != null)
                //    Task.Factory.StartNew(() => _Action(item));
                //else
                //    Task.Factory.StartNew(() => DoMessageReceived(item));
                if (EnableDynamicWait)
                    ActionWorker.DynamicWaitAck(true);
            }
            else
            {
                if (EnableDynamicWait)
                    ActionWorker.DynamicWaitAck(false);
            }
        }
        ///// <summary>
        ///// Occured when operation has error.
        ///// </summary>
        ///// <param name="e"></param>
        //protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        //{

        //    if (ErrorOcurred != null)
        //        ErrorOcurred(this, e);

        //}
        protected virtual void OnEvent(string source,string msg)
        {

        }

        protected virtual void OnErrorOcurred(string msg)
        {
            Console.WriteLine("ErrorOcurred: " + msg);
            if (ErrorOcurred != null)
                ErrorOcurred(this, new GenericEventArgs<string>(msg)); //OnErrorOcurred(new GenericEventArgs<string>(msg));
            else if (Adapter.MessageFaultAction != null)
                Adapter.MessageFaultAction(msg);
        }

        protected void OnFault(string message)
        {
            OnErrorOcurred(message);
            //OnErrorOcurred(new GenericEventArgs<string>(message));
        }

        protected void OnDynamicWorkerCompleted(IQueueMessage item)
        {
            OnMessageReceived(item);

            if (MessageReceived != null)
                MessageReceived(this, new GenericEventArgs<IQueueMessage>(item));
            else if (Adapter.MessageReceivedAction != null)
                Adapter.MessageReceivedAction(item);
        }
     
    #endregion

    #region ctor

        public SessionListener(QueueAdapter adapter)//, int interval)
        {
            if (adapter == null)
            {
                throw new ArgumentNullException("adapter");
            }
            if (adapter.Source == null)
            {
                throw new ArgumentNullException("adapter.Source");
            }
            Adapter = adapter;

            //_Owner = owner;
            _Source = adapter.Source;
            HostName = _Source.HostName;

            //_TransferTo = adapter.TransferTo;

            //_ServerName = channel.ServerName;
            //_QueueName = channel.Source;
            //IntervalWait = interval < MinWait ? MinWait : interval;// 1000;

            Interval = adapter.Interval;
            _ConnectTimeout = adapter.ConnectTimeout;
            _ReadTimeout = adapter.ReadTimeout;
            _WorkerCount = adapter.WorkerCount;
            _MaxConnection = adapter.MaxConnection;
            _IsMultiTask= adapter.IsMultiTask;
            _IsAsync = adapter.IsAsync;
            //_Action = adapter.QueueAction;
            EnableResetEvent = adapter.EnableResetEvent;
            EnableDynamicWait = adapter.EnableDynamicWait;
            //_ActionTransfer = adapter.AckAction;
            //_AdapterOperation = adapter.OperationType;

            //QApi = new QueueApi(adapter.Source);
            //QApi.ReadTimeout = adapter.ReadTimeout;

            State = ListenerState.Initilaized;
        }

    #endregion

    #region override

        //protected abstract IQueueAck Send(QueueMessage message);

        protected abstract IQueueMessage Receive();
        protected abstract void Receive(IDynamicWait aw);
        protected virtual async Task ReceiveAsync(IDynamicWait aw)
        {
            await Task.Run(() =>
            {
                Receive(aw);
            });
        }

        //protected abstract IQueueMessage ReceiveRound();

        //protected abstract void ReceiveAsync(int connectTimeout, Action<QueueMessage> action);

        //protected abstract IQueueAck ReceiveTo();// QueueHost target, int connectTimeout, Action<QueueMessage> recieveAction);

        public virtual void Commit(Ptr ptr)
        {
            QueueApi.Get(Source).Commit(ptr);
        }

        public virtual void Abort(Ptr ptr)
        {
            QueueApi.Get(Source).Abort(ptr);
        }

        //protected virtual void OnMessageTransfer(IQueueAck ack)
        //{
        //    if (ack != null)
        //    {
        //        if (_Action != null)
        //        {
        //            Task.Factory.StartNew(() => _ActionTransfer(ack));
        //        }
        //        //else
        //        //    Task.Factory.StartNew(() => DoMessageTransfer(item));
        //    }
        //}

    #endregion

    #region ThreadWorker

        //public ListenerState State
        //{
        //    get
        //    {
        //        if (ActionWorker == null)
        //            return ListenerState.Down;
        //        return ActionWorker.State;
        //    }
        //}
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
                return ActionWorker.State== ListenerState.Started;
            }
        }

        public int ActiveConnections
        {
            get
            {
                return ActionWorker.ActiveConnections;
            }
        }
        public NameValueArgs Report()
        {
            return ActionWorker.Report();
        }

        public DynamicWorker ActionWorker { get; private set; }
        public void Start()
        {
            if (ActionWorker != null)
                return;

            ActionWorker = new DynamicWorker( DynamicWaitType.DynamicWait,WorkerCount,Interval,MaxConnection,IsMultiTask)
            {
                ActionTask = () =>
                {
                    try
                    {
                        //in case of ResetEvent and fixed interval using
                        Receive(ActionWorker);//.ConfigureAwait(false);
                        //ReceiveAsync(ActionWorker).ConfigureAwait(false);
                        //return false;

                        //in case of DynamicWait or fixed interval using
                        var ack = Receive();
                        if (ack != null)
                        {
                            OnDynamicWorkerCompleted(ack);
                            return true;
                        }
                        return false;// ack != null;
                    }
                    catch (Exception ex) {
                        OnErrorOcurred(ex.Message);
                        if (_Logger != null)
                            _Logger.Exception("Session listener "+ HostName + " ActionTask error " , ex);
                        return false;
                    }
                },
                ActionLog = (LogLevel level, string message) =>
                {
                    OnEvent("SessionListener.Start.ActionLog", message);
                    if (_Logger != null)
                        _Logger.Log((LoggerLevel)level, message);
                },
                ActionState = (ListenerState state) => {
                    State = state;
                },
                Name = "SessionListener",
                Interval = 100,
                MaxThreads = 1
            };
            ActionWorker.Start();
            State = ListenerState.Started;
            OnEvent("SessionListener.Started", string.Format("SessionListener Started: {0}", HostName));

            if (_Logger != null)
                _Logger.Info("SessionListener Started: {0}", HostName);
        }
        public void Stop()
        {
            if (ActionWorker == null)
                return;
            ActionWorker.Stop();
            State = ListenerState.Stoped;
            OnEvent("SessionListener.Stoped", string.Format("SessionListener Stoped: {0}", HostName));
            if (_Logger != null)
                _Logger.Info("SessionListener Stoped: {0}", HostName);
        }
        public bool Pause(OnOffState onOff)
        {
            if (ActionWorker == null)
                return false;
            bool paused = ActionWorker.Pause(onOff);
            OnEvent("SessionListener.Pause", string.Format("SessionListener Pause: {0}", HostName));
            if (_Logger != null)
                _Logger.Info("SessionListener Paused: {0}, {1}", paused, HostName);

            if (paused)
                State = ListenerState.Paused;
            return paused;
        }
        public void Shutdown(bool waitForWorkers)
        {
            if (ActionWorker == null)
                return;
            ActionWorker.Shutdown(waitForWorkers);
            State = ListenerState.Down;
            Adapter.Dispose();
            OnEvent("SessionListener.Shutdown", string.Format("SessionListener Shutdown: {0}", HostName));
            if (_Logger != null)
                _Logger.Info("SessionListener Shutdown: {0}", HostName);
        }

    #endregion

    #region start/stop
        /*
        bool lockWasTaken = false;
        object _locker = new object();
        Thread[] _workers;
        long delay;
 
        public void Start()
        {
            if (IsAlive)
            {
                return;
            }
            _workers = new Thread[_WorkerCount];
            ThreadStart threadWorker = IsAsync ? new ThreadStart(TaskWorkerAsync) : new ThreadStart(TaskWorker);
            //ThreadStart threadWorker = new ThreadStart(TaskWorkerAsync);
            for (int i = 0; i < _WorkerCount; i++)
            {
                _workers[i] = new Thread(new ThreadStart(threadWorker));
                _workers[i].IsBackground = true;
                _workers[i].Start();
            }

            if (_Logger != null)
                _Logger.Info("SessionListener Started");
        }
        public void Stop()
        {
            Shutdown(true);
            if (_Logger != null)
                _Logger.Info("SessionListener Stoped");
        }
        public void Shutdown(bool waitForWorkers)
        {
            _isalive = false;

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in _workers)
                    worker.Join();
        }
        */
    #endregion

    #region worker
        /*
                public void Delay(TimeSpan time)
                {
                    Interlocked.Exchange(ref delay, (long)time.TotalMilliseconds);
                }

                static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

                protected virtual void TaskWorker()
                {
                    _isalive = true;
                    // Start queue listener...
                    if (_Logger != null)
                        _Logger.Info("QListener started...");

                    while (IsAlive)
                    {

                        try
                        {

                            if (Interlocked.Read(ref delay) > 0)
                            {
                                Thread.Sleep((int)delay);
                                Interlocked.Exchange(ref delay, 0);
                            }

                            Monitor.Enter(_locker);
                            lockWasTaken = true;

                            var task = Task.Factory.StartNew(() => 
                              OnCompleted(Receive())

                            );
                            if (EnableResetEvent)
                                autoResetEvent.WaitOne();

                         }
                        catch (Exception ex)
                        {
                            if (_Logger != null)
                                _Logger.Error("QListener error: " + ex.Message);
                            Task.Factory.StartNew(() => DoErrorOcurred(ex.Message));
                        }
                        finally
                        {
                            if (lockWasTaken) Monitor.Exit(_locker);
                        }
                        Thread.Sleep(DynamicWait);
                    }

                    if (_Logger != null)
                        _Logger.Info("QListener stoped");

                }

                protected virtual void TaskWorkerAsync()
                {
                    _isalive = true;
                    // Start queue listener...
                    if (_Logger != null)
                        _Logger.Info("QListener started...");

                    while (IsAlive)
                    {
                        try
                        {

                            if (Interlocked.Read(ref delay) > 0)
                            {
                                Thread.Sleep((int)delay);
                                Interlocked.Exchange(ref delay, 0);
                            }

                            Monitor.Enter(_locker);
                            lockWasTaken = true;

                            var task = Task.Factory.StartNew(() =>
                                //RecieveInternal()
                                ReceiveAsync(autoResetEvent)
                                //autoResetEvent.Set();
                                //return message;
                            );
                            if(EnableResetEvent)
                            autoResetEvent.WaitOne();
                        }
                        catch (Exception ex)
                        {
                            if (_Logger != null)
                                _Logger.Error("QListener error: " + ex.Message);

                            Task.Factory.StartNew(() => DoErrorOcurred(ex.Message));
                        }
                        finally
                        {
                            if (lockWasTaken) Monitor.Exit(_locker);
                        }
                        Thread.Sleep(DynamicWait);
                    }

                    if (_Logger != null)
                        _Logger.Info("QListener stoped...");

                }
        */
    #endregion
    }

#endif
}
