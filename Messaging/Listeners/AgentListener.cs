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
using Nistec.Channels;

namespace Nistec.Messaging.Listeners
{

    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection for client. 4.7.2.5
    /// </summary>
    public abstract class AgentListener<T> where T : IAgentMessage //: INetcellAgent//: QueueReciever
    {
        //const string ActiveMQueueName = "controller";

        #region memebers
        private int isQueueRunning = 0;
        private bool keepAlive = false;
        private GenericThreadPool threadPool;
        static object m_lock = new object();

        //protected int MaxThread = 1;
        //MaxConnections = 30;
        protected int m_connections;
        //Interval = 30000;
        protected int Server = 0;
        protected int WaitSecond = 120;
        protected int ReadTimeout = -1;
        protected int ConnectTimeout = -1;
        protected bool Is_Pause = false;
        protected int PauseInterval = 0;
        public HostChannel Source { get; set; }
        public bool Initilaized { get; protected set; }

        protected string HostName;
        //protected string HostAddress;// = "tcp:127.0.0.1:15000?Controller";

        public int ActiveConnections
        {
            get
            {
                return m_connections;
            }
        }

        ILogger _Logger;
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }
        #endregion

        #region ctor

        public AgentListener(IAgentAdapter adapter)//, int interval)
        {
            Source = adapter.Source;
            HostName = Source.HostName;
            ReadTimeout = adapter.ReadTimeout;
            MaxConnection = adapter.MaxConnection;
            Interval = adapter.Interval;
            ConnectTimeout = adapter.ConnectTimeout;
            WorkerCount = adapter.WorkerCount;
            Initilaized = false;
        }
        
        public AgentListener()
        {
            Initilaized = false;
            MaxConnection = 1;
            Interval = 1000;
        }

        protected virtual void Init(string HostAddress)
        {
            Source = HostChannel.Parse(HostAddress);
            HostName = Source.HostName;
            Initilaized = true;
            OnStateChanged(ListenerState.Initilaized);
        }
        #endregion

        #region message events
        /*
        /// <summary>
        /// OnError
        /// </summary>
        public event GenericEventHandler<string> ErrorOcurred;
        /// <summary>
        /// QueueMessage Received
        /// </summary>
        public event GenericEventHandler<T> MessageReceived;
        */
        #endregion

        #region INetcellAgent
        //public DynamicWorker ActionWorker { get; private set; }
        public int WorkerCount { get; set; }
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
            //if (ErrorOcurred != null)
            //    ErrorOcurred(this, new GenericEventArgs<string>(message)); //OnError(new GenericEventArgs<string>(msg));
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
                OnInfo($"AgentListener not Initilaized {HostName}");
                return;
            }
            if (keepAlive || !Initilaized)
                return;
            OnInfo($"AgentListener Start {HostName}");
            keepAlive = true;
            threadPool = new GenericThreadPool(WorkerCount);
            threadPool.StartThreadPool(new ParameterizedThreadStart(AgentProcess));
            OnInfo($"AgentListener Started {HostName}");
        }

        public void Stop()
        {
            OnInfo($"AgentListener Stop {HostName}");
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
        public bool Pause(OnOffState onOff, int delay)
        {
            //if (ActionWorker == null)
            //    return false;
            bool paused = onOff == OnOffState.On;// ActionWorker.Pause(onOff);

            if (paused)
            {
                Interlocked.Exchange(ref PauseInterval, Math.Max(delay, 1000));
                State = ListenerState.Paused;
                //OnEvent($"AgentSessionListener.Pause", $"State: {State}, HostName: {HostName}");
                OnInfo($"AgentSessionListener Paused: {HostName} delay: {delay}");
            }
            else
            {
                Interlocked.Exchange(ref PauseInterval, 0);
                State = ListenerState.Started;
                OnInfo($"AgentSessionListener No Paused: {HostName}");
            }
            return paused;
        }
        /*
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
        */
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
            args.Add("IsMultiTask", IsMultiTask);
            args.Add("ActiveConnections", ActiveConnections);
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

        protected virtual int ShouldPause()
        {
            return 0;
        }

        #endregion

        #region Publish

        protected virtual void OnMessageReceived(T message)
        {
            if (message != null)
            {
                PublishMessage(message, (ack) => {
                    OnInfo($"AgentListener OnReceivedEvent Result: {ack.ToString()}");
                    CommitAsync(ack);//.ConfigureAwait(false);
                });//.ConfigureAwait(false);
            }
        }

        protected virtual void PublishMessage(T message)
        {
            //await Task.Run(null);
        }

        protected virtual void PublishMessage(T message, Action<IAck> ack)
        {
            //await Task.Run(null);
        }

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

        protected abstract T ReadMessage(int WaitSecond);

        protected string GetIdentifier(T message)
        {
            return message == null ? "" : message.Identifier;
        }

        #endregion

        #region Connection 
        int connectionfactor = 0;
        int connectionmax = 0;
        protected int ExchangeFactor = 10;
        protected void ConnectionExchangeBegin()
        {
            Interlocked.Increment(ref m_connections);
        }
        protected void ConnectionExchange(bool hasValue)
        {
            if (hasValue)
            {
                if (Interlocked.CompareExchange(ref connectionmax, 0, 0) < MaxConnection)
                {
                    Interlocked.Exchange(ref connectionmax, MaxConnection);
                    Interlocked.Exchange(ref connectionfactor, 0);
                    OnInfo($"AgentListener MaxConnection Increased {connectionmax}");
                }
                if (Interlocked.CompareExchange(ref m_connections, 0, 0) > 0)
                    Interlocked.Decrement(ref m_connections);
            }
            else if (Interlocked.CompareExchange(ref connectionfactor, 0, 0) < ExchangeFactor)
            {
                Interlocked.Increment(ref connectionfactor);
            }
            else if (Interlocked.CompareExchange(ref connectionmax, 0, 0) > 1)
            {
                Interlocked.Exchange(ref connectionmax, 1);
                OnInfo($"AgentListener MaxConnection is {connectionmax}");
            }
        }
        #endregion

        #region AgentProcess
        int pause = 0;
        private void AgentProcess(object state)
        {
            connectionmax = MaxConnection;
            while (keepAlive)
            {

                T message = default(T);

                try
                {
                    /*
                    ShouldPause();
                   
                    while (Interlocked.CompareExchange(ref PauseInterval,0,0) > 0)
                    {
                        Thread.Sleep((int)PauseInterval);
                    }
                    while (Interlocked.CompareExchange(ref m_connections,0,0) >= MaxConnection)
                    {
                        Thread.Sleep(1000);
                    }
                    */
                    if (!keepAlive)
                    {
                        break;
                    }
                    Interlocked.Exchange(ref pause , ShouldPause());
                    while (Interlocked.CompareExchange(ref pause, 0, 0) > 0)//(!(keepAlive && App_Servers.IsEnableQueue(server)))
                    {
                        OnInfo($"AgentListener QueueProcess ShouldPause {pause}");
                        Thread.Sleep(pause);

                        //if (pause == 10)
                        //{
                        //    OnInfo($"AgentListener QueueProcess ShouldPause {pause}");
                        //    pause = 0;
                        //}
                        //++pause;

                        //Thread.Sleep(60000);
                    }
                    //pause = 0;
                    //while (Thread.VolatileRead(ref m_connections) >= connectionmax)
                    //{
                    //    OnInfo($"AgentListener AgentProcess ShouldPause {++pause}, m_Connections {m_connections}");
                    //    Thread.Sleep(100);
                    //}
                    
                    if (!keepAlive)
                    {
                        break;
                    }

                    Interlocked.Increment(ref isQueueRunning);
                    if (Interlocked.CompareExchange(ref m_connections, 0, 0) < connectionmax)
                    {
                        lock (m_lock)
                        {
                            message = ReadMessage(WaitSecond);

                            if (message != null)
                            {
                                if (message.Body != null && message.AckState == (int)ChannelState.Received)
                                {
                                    //Interlocked.Increment(ref m_connections);
                                    ThreadPool.QueueUserWorkItem(AgentWorker, message);
                                    //Task.Run(()=> AgentItemWorker(message));
                                    //ConnectionExchange(true);
                                }
                                else if ((int)message.AckState >= 400)
                                {
                                    OnError("AgentListener read failed : " + message.Label + ", " + message.Print());
                                }
                            }
                            else
                            {
                                ConnectionExchange(false);
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    OnError($"Warn: AgentListener ThreadAborted {HostName}");
                }
                catch (Exception ex)
                {
                    OnError($"AgentListener {HostName}, Error:{ex.Message + " Trace:" + ex.StackTrace} ");

                    try
                    {

                        lock (m_lock)
                        {
                            Abort(MessageAck.DoAck(ChannelState.InternalServerError, ex.Message, GetIdentifier(message)));
                        }

                    }
                    catch (Exception exx)
                    {
                        OnError($"AgentListener {HostName}, Error:{exx.Message} ");
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref isQueueRunning);
                }
                Thread.Sleep(Interval);
            }
            OnError($"Warn: AgentListener not keep Alive {HostName}");
        }
        void AgentWorker(Object threadContext)
        {
            try
            {
                OnInfo($"AgentListener AgentWorker started");

                using (T message = (T)threadContext)
                {

                    if (message != null)// && !queueItem.IsEmpty)
                    {
                        OnInfo($"AgentListener ExecuteAsync {message.Print()}");
                        ConnectionExchangeBegin();
                        OnMessageReceived(message);//.ConfigureAwait(false);
                        ConnectionExchange(true);

                        //lock (m_lock)
                        //{
                        //    Commit(message); //Queue_Context.Commit(queueItem.QueueId);
                        //}
                        //activeCampaign.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                OnError($"AgentListener {HostName},  Error :{ex.Message}");
            }
            //if (Interlocked.CompareExchange(ref m_connections, 0, 0) > 0)
            //    Interlocked.Decrement(ref m_connections);

            OnInfo($"AgentListener AgentWorker finished");
        }
        #endregion
    }



#if(false)
    public abstract class __AgentListener<T> where T : IAgentMessage
    {
        #region memebers

        //private int isQueueRunning = 0;
        private bool keepAlive = false;
        //private GenericThreadPool threadPool;
        //static object m_lock = new object();
        public bool IsAsync { get; protected set; } =false;
        //protected int MaxThread = 1;
        //MaxConnection = 30;
        protected int m_Connections;
        public int MaxConnection { get; set; } = 2;
        public int Interval { get; set; }  = 1000;
        public int WorkerCount { get; protected set; }  = 2;
        public bool IsMultiTask { get; set; } = false;
        public bool EnableDynamicWait { get; set; } = false;
        public ListenerState State {get; protected set; }
    
        protected int Server = 0;
        protected int WaitSecond = 120;
        public int ReadTimeout { get; protected set; }= -1;
        public int ConnectTimeout { get; protected set; } = -1;
        protected bool Is_Pause = false;
        protected int PauseInterval = 1000;
        public HostChannel Source { get; set; }
        public bool Initilaized { get; protected set; }

        //protected QueueApi QApi;
        protected string HostName;
        //protected string HostAddress;// = "tcp:127.0.0.1:15000?Controller";

        //protected QueueApi QApi { get; private set; }

        public int ActiveConnections
        {
            get
            {
                return m_Connections;
            }
        }

        ILogger _Logger;
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }
        #endregion

        #region ctor

        public __AgentListener(IAgentAdapter adapter)//, int interval)
        {
            Source = adapter.Source;
            HostName = Source.HostName;
            ReadTimeout = adapter.ReadTimeout;
            MaxConnection = adapter.MaxConnection;
            Interval = adapter.Interval;
            ConnectTimeout = adapter.ConnectTimeout;
            WorkerCount = adapter.WorkerCount;
            Initilaized = false;
        }

        public __AgentListener()
        {
            Initilaized = false;
            MaxConnection = 1;
            Interval = 1000;
        }

        protected virtual void Init(string HostAddress)
        {
            Source = HostChannel.Parse(HostAddress);
            HostName = Source.HostName;
            Initilaized = true;
            OnStateChanged(ListenerState.Initilaized);
        }

        #endregion

        #region message events

        protected virtual void OnEvent(string source, string msg)
        {

        }
        protected virtual void OnInfo(string message)
        {
            if (_Logger != null)
                _Logger.Info(message);
        }

        protected virtual void OnError(string message)
        {
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

        protected abstract T ReadMessage(int WaitSecond);

        /// <summary>
        /// OnMessageReceived, when override events will not fired
        /// </summary>
        /// <param name="message"></param>
        protected virtual void OnMessageReceived(T message)
        {

        }

        #endregion

        #region start/stop

        bool lockWasTaken = false;
        static object _locker = new object();
        Thread[] _workers;
        long delay;
        long m_connections = 0;
        long m_pause = 0;

        public void Start()
        {
            if (keepAlive)
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
            keepAlive = false;

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in _workers)
                    worker.Join();
        }

        public bool Pause(OnOffState onOff, int delay)
        {
            //if (ActionWorker == null)
            //    return false;
            bool paused = onOff == OnOffState.On;// ActionWorker.Pause(onOff);

            if (paused)
            {
                Interlocked.Exchange(ref m_pause, Math.Max(delay, 1000));
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

        public NameValueArgs Report()
        {

            var args = new NameValueArgs();
            args.Add("HostName", HostName);
            args.Add("MaxConnection", MaxConnection);
            args.Add("ActiveConnections", ActiveConnections);
            args.Add("Interval", Interval);
            //args.Add("WaitType", WaitType.ToString());
            //args.Add("EnableDynamicWait", EnableDynamicWait);
            //args.Add("EnableResetEvent", EnableResetEvent);
            args.Add("State", State.ToString());
            //args.Add("IsMultiTask", IsMultiTask);
            //args.Add("MaxThreads", WorkerCount);
            return args;
        }
        #endregion

        #region worker

        public void Delay(TimeSpan time)
        {
            Interlocked.Exchange(ref delay, (long)time.TotalMilliseconds);
        }

        private static readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        protected virtual void TaskWorker()
        {
            keepAlive = true;
            // Start queue listener...
            OnInfo("AgentListener started...");

            while (keepAlive)
            {

                try
                {
                    ShouldPause();

                    if (Interlocked.Read(ref delay) > 0)
                    {
                        Task.Delay((int)delay);
                        Interlocked.Exchange(ref delay, 0);
                    }
                    while (Interlocked.Read(ref m_pause) > 0)
                    {
                        Task.Delay((int)m_pause);
                    }
                    while (Interlocked.Read(ref m_connections) >= MaxConnection)
                    {
                        Task.Delay(1000);
                    }

                    //Monitor.Enter(_locker);
                    //lockWasTaken = true;
                    lock (_locker)
                    {
                        T message = ReadMessage(WaitSecond);

                        if (message != null)
                        {
                            if (message.Body != null && message.AckState == (int)ChannelState.Received)
                            {
                                Interlocked.Increment(ref m_connections);
                                Task.Run(() => ItemWorker(message));
                                autoResetEvent.WaitOne();
                                //OnInfo($"AgentListener AgentWorker finished");
                            }
                            else if ((int)message.AckState >= 400)
                            {
                                OnError("AgentListener read failed : " + message.Label + ", " + message.Print());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    autoResetEvent.Set();
                    OnError("AgentListener error: " + ex.Message);
                }
                finally
                {
                    if (lockWasTaken) Monitor.Exit(_locker);
                }
                Interlocked.Decrement(ref m_connections);
                Task.Delay(100);
            }

            OnInfo("AgentListener stoped");

        }

        protected async virtual void TaskWorkerAsync()
        {
            keepAlive = true;
            // Start queue listener...
            OnInfo("AgentListener async started...");

            while (keepAlive)
            {
                try
                {
                    ShouldPause();

                    if (Interlocked.Read(ref delay) > 0)
                    {
                        await Task.Delay((int)delay);
                        Interlocked.Exchange(ref delay, 0);
                    }
                    while (Interlocked.Read(ref m_pause) > 0)
                    {
                        await Task.Delay((int)m_pause);
                    }
                    while (Interlocked.Read(ref m_connections) >= MaxConnection)
                    {
                        await Task.Delay(1000);
                    }
                    //Monitor.Enter(_locker);
                    //lockWasTaken = true;

                    lock (_locker)
                    {
                       T message = ReadMessage(WaitSecond);

                        if (message != null)
                        {
                            if (message.Body != null && message.AckState == (int)ChannelState.Received)
                            {
                                Interlocked.Increment(ref m_connections);
                                Task.Run(() => ItemWorker(message));
                                autoResetEvent.WaitOne();
                                //OnInfo($"AgentListener AgentWorker finished");
                            }
                            else if ((int)message.AckState >= 400)
                            {
                                OnError("AgentListener read failed : " + message.Label + ", " + message.Print());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    autoResetEvent.Set();
                    OnError("AgentListener async error: " + ex.Message);
                }
                finally
                {
                    if (lockWasTaken) Monitor.Exit(_locker);
                }
                await Task.Delay(Interval);
            }

            OnInfo("AgentListener stoped...");

        }

        void ItemWorker(T message)
        {
            try
            {
                //OnInfo($"AgentListener AgentItemWorker started");

                if (message != null)// && !queueItem.IsEmpty)
                {
                    OnInfo($"AgentListener AgentItemWorker {message.Print()}");

                    OnMessageReceived(message);//.ConfigureAwait(false);

                    OnInfo($"AgentListener AgentWorker finished");

                }
            }
            catch (Exception ex)
            {
                OnError($"AgentListener ItemWorker,  Error :{ex.Message}");
            }
            autoResetEvent.Set();
            Interlocked.Decrement(ref m_connections);
        }

        #endregion
    }
#endif

#if (false)
    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection for client.
    /// </summary>
    public abstract class AgentListener<T> where T : IAgentMessage //: INetcellAgent//: QueueReciever
    {
        //const string ActiveMQueueName = "controller";

    #region memebers
        private int isQueueRunning = 0;
        private bool keepAlive = false;
        private GenericThreadPool threadPool;
        static object m_lock = new object();

        //protected int MaxThread = 1;
        //MaxConnection = 30;
        protected int m_Connections;
        //Interval = 30000;
        protected int Server = 0;
        protected int WaitSecond = 120;
        protected int ReadTimeout = -1;
        protected int ConnectTimeout = -1;
        protected bool Is_Pause = false;
        protected int PauseInterval = 1000;
        public HostChannel Source { get; set; }
        public bool Initilaized { get; protected set; }

        //protected QueueApi QApi;
        protected string HostName;
        //protected string HostAddress;// = "tcp:127.0.0.1:15000?Controller";

        //protected QueueApi QApi { get; private set; }

        public int ActiveConnections
        {
            get
            {
                return m_Connections;
            }
        }

        ILogger _Logger;
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }
    #endregion

    #region ctor

        public AgentListener(IAgentAdapter adapter)//, int interval)
        {
            Source = adapter.Source;
            HostName = Source.HostName;
            ReadTimeout = adapter.ReadTimeout;
            MaxConnection = adapter.MaxConnection;
            Interval = adapter.Interval;
            ConnectTimeout = adapter.ConnectTimeout;
            WorkerCount = adapter.WorkerCount;
            Initilaized = false;
        }

    #endregion


        public AgentListener()
        {
            Initilaized = false;
            MaxConnection = 1;
            Interval = 30000;
        }

        protected virtual void Init(string HostAddress)
        {
            Source = HostChannel.Parse(HostAddress);
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
        public event GenericEventHandler<T> MessageReceived;

    #endregion

    #region INetcellAgent
        //public DynamicWorker ActionWorker { get; private set; }
        public int WorkerCount { get; set; }
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
                OnInfo($"AgentListener not Initilaized {HostName}");
                return;
            }
            if (keepAlive || !Initilaized)
                return;
            OnInfo($"AgentListener Start {HostName}");
            keepAlive = true;
            threadPool = new GenericThreadPool(WorkerCount);
            threadPool.StartThreadPool(new ParameterizedThreadStart(AgentProcess));
            OnInfo($"AgentListener Started {HostName}");

            //base.Start();
        }

        public void Stop()
        {
            OnInfo($"AgentListener Stop {HostName}");
            try
            {

                keepAlive = false;
                int count = 0;
                while (Thread.VolatileRead(ref isQueueRunning) > 0 && count < 30)
                {
                    Task.Delay(1000);
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

        public bool Pause(OnOffState onOff, int delay)
        {
            if (State != ListenerState.Started)
            {
                if (Is_Pause)
                    PauseInterval = Math.Max(delay, 1000);
                return Is_Pause;
            }
            if ((onOff == OnOffState.Toggle && State == ListenerState.Paused) || onOff == OnOffState.Off)
            {
                Is_Pause = false;
                OnStateChanged(ListenerState.Started);
            }
            else
            {
                Is_Pause = true;
                OnStateChanged(ListenerState.Paused);
                PauseInterval = Math.Max(delay, 1000);
                //int intervalSeconds = 60;
                //PauseInterval = 1000 * ((intervalSeconds < 1) ? 60 : intervalSeconds);
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

        //protected virtual void OnMessageReceived(T message)
        //{

        //}

        protected virtual void OnMessageReceived(T message)
        {
            if (message != null)
            {
                PublishMessage(message, (ack) =>
                {
                    OnInfo($"AgentListener OnReceivedEvent Result: {ack.ToJson()}");
                    CommitAsync(ack);//.ConfigureAwait(false);
                    //are.Set();
                });//.ConfigureAwait(false);
            }
            //else
            //{
            //    are.Set();
            //}
        }

        protected virtual void PublishMessage(T message)
        {
            //await Task.Run(null);
        }

        protected virtual void PublishMessage(T message, Action<IAck> ack)
        {
            //await Task.Run(null);
        }

        //protected virtual void PublishMessageAsync(T message, Action<IAck> ack)
        //{
        //    //await Task.Run(null);
        //}
        /*
        protected virtual async Task OnMessageReceived(T message)
        {
            if (message != null)
            {
                await PublishMessageAsync(message, (ack) => {
                    OnInfo($"AgentListener OnReceivedEvent Result: {ack.ToString()}");
                    CommitAsync(ack);//.ConfigureAwait(false);
                });//.ConfigureAwait(false);
            }
        }

        protected virtual async Task PublishMessageAsync(T message, Action<IAck> ack)
        {
            await Task.Run(null);
        }
        */
        //{
        //    OnInfo("SwifterAgent PublishMessageAsync ExecuteAsync {0}", message.Print());
        //    var msgIn = message.GetBody<MessageIn>();
        //    var publisher = new PublishIn(msgIn);
        //    await publisher.InvokeAsync(ack);

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

        protected abstract T ReadMessage(int WaitSecond);

        protected string GetIdentifier(T message)
        {
            return message == null ? "" : message.Identifier;
        }

    #endregion

    #region AgentProcess

        //public override void OnMessageReceived(IQueueItem item)
        //{
        //    base.OnMessageReceived(item);
        //}

        //public override void OnMessageFault(string message)
        //{
        //    base.OnMessageFault(message);
        //}

        //private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        int pause = 0;
        private void AgentProcess(object state)
        {

            while (keepAlive)
            {
                //Queue_Queue activeQueue = null;

                T message = default(T);

                try
                {

                    while (keepAlive && ShouldPause())//(!(keepAlive && App_Servers.IsEnableQueue(server)))
                    {
                        if (pause == 10)
                        {
                            OnInfo($"AgentListener QueueProcess ShouldPause {pause}");
                            pause = 0;
                        }
                        ++pause;

                        Task.Delay(60000);
                    }
                    pause = 0;
                    while (Thread.VolatileRead(ref m_Connections) >= MaxConnection)
                    {
                        OnInfo($"AgentListener AgentProcess ShouldPause {++pause}, m_Connections {m_Connections}");
                        Task.Delay(100);
                    }

                    if (!keepAlive)
                    {
                        break;
                    }


                    //Console.WriteLine("Queue load...{0}",Thread.CurrentThread.Name);

                    Interlocked.Increment(ref isQueueRunning);

                    lock (m_lock)
                    {
                        message = ReadMessage(WaitSecond);

                        if (message != null)// && message.AckState ==  (int)ChannelState.Received && message.Body != null)
                        {
                            if (message.Body != null && message.AckState == (int)ChannelState.Received)
                            {
                                Interlocked.Increment(ref m_Connections);
                                //ThreadPool.QueueUserWorkItem(AgentWorker, message);
                                Task.Run(()=> OnMessageReceived(message));
                                //autoResetEvent.WaitOne();
                                //OnMessageReceived(message);//.ConfigureAwait(false);

                                OnInfo($"AgentListener AgentWorker finished");

                            }
                            else if ((int)message.AckState >= 400)
                            {
                                OnError("AgentListener read failed : " + message.Label + ", " + message.Print());
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    //autoResetEvent.Set();
                    OnError($"Warn: AgentListener ThreadAborted {HostName}");
                }
                catch (Exception ex)
                {
                    OnError($"AgentListener {HostName}, Error:{ex.Message + " Trace:" + ex.StackTrace} ");

                    try
                    {

                        lock (m_lock)
                        {
                            Abort(MessageAck.DoAck( ChannelState.InternalServerError,ex.Message, GetIdentifier(message)));
                        }

                    }
                    catch (Exception exx)
                    {
                        OnError($"AgentListener {HostName}, Error:{exx.Message} ");
                    }
                    //autoResetEvent.Set();
                }
                finally
                {
                    Interlocked.Decrement(ref isQueueRunning);
                    Interlocked.Decrement(ref m_Connections);
                }
                Task.Delay(Interval);
            }
            OnError($"Warn: AgentListener not keep Alive {HostName}");
        }

        /*
        void AgentItemWorker(T message)
        {
            try
            {
                OnInfo($"AgentListener AgentItemWorker started");

                if (message != null)// && !queueItem.IsEmpty)
                {
                    OnInfo($"AgentListener AgentItemWorker {message.Print()}");

                    //var message = queueItem.GetBody<QueueMessage>();    //Message.Deserialize(queueItem.ToJson();//.BodyStream);

                    OnMessageReceived(message);//.ConfigureAwait(false);

                    //lock (m_lock)
                    //{
                    //    Commit(message); //Queue_Context.Commit(queueItem.QueueId);
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
                OnError($"AgentListener Error ThreadAborted {HostName}");
            }
            catch (Exception ex)
            {
                OnError($"AgentListener {HostName},  Error :{ex.Message}");
            }

            Interlocked.Decrement(ref m_Connections);

            OnInfo($"AgentListener AgentWorker finished");
        }

        void AgentWorker(Object threadContext)
        {
            try
            {
                OnInfo($"AgentListener AgentWorker started");


                T message = (T)threadContext;

                if (message != null)// && !queueItem.IsEmpty)
                {
                    OnInfo($"AgentListener ExecuteAsync {message.Print()}");

                    //var message = queueItem.GetBody<QueueMessage>();    //Message.Deserialize(queueItem.ToJson();//.BodyStream);

                    OnMessageReceived(message);//.ConfigureAwait(false);
                   
                    //lock (m_lock)
                    //{
                    //    Commit(message); //Queue_Context.Commit(queueItem.QueueId);
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
                OnError($"AgentListener Error ThreadAborted {HostName}");
            }
            catch (Exception ex)
            {
                OnError($"AgentListener {HostName},  Error :{ex.Message}");
            }

            Interlocked.Decrement(ref m_Connections);

            OnInfo($"AgentListener AgentWorker finished");
        }
        */
    #endregion
    }

#endif

}
