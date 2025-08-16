using Nistec;
using Nistec.Channels;
using Nistec.Channels.Tcp;
using Nistec.Collections;
using Nistec.Data.Persistance;
using Nistec.Generic;
using Nistec.Logging;
using Nistec.Messaging;
using Nistec.Messaging.Listeners;
using Nistec.Messaging.Remote;
using Nistec.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nistec.Messaging.Listeners
{
    public class QueueAgentReceiver
    {

        //const string ActiveMQueueName = "controller";

        #region memebers
        private int isQueueRunning = 0;
        private bool keepAlive = false;
        //QueueReciever MQ;
        private Nistec.Threading.GenericThreadPool threadPool;
        static object m_lock = new object();

        //protected int MaxThread = 1;
        protected int WorkerCount = 1;

        //MaxConnection = 30;
        protected int m_Connections;
        //Interval = 30000;
        protected int Server = 0;
        protected int WaitSecond = 120;
        protected int ReadTimeout = -1;
        protected bool Is_Pause = false;
        protected int PauseInterval = 1000;
        public QueueHost Source { get; set; }
        public bool Initilaized { get; protected set; }

        //protected QueueApi QApi;
        protected string HostName;
        protected string QueueAddress;// = "tcp:127.0.0.1:15000?Controller";

        //protected QueueApi QApi { get; private set; }

        ILogger _Logger;
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }

        static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        #endregion

        #region ctor

        public QueueAgentReceiver(QueueAdapter adapter)//, int interval)
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

            //QApi = new QueueApi(adapter.Source);
            //QApi.ReadTimeout = adapter.ReadTimeout;
            //_Listener= new ListenerQ(this, adapter);
            Initilaized = false;
        }

        #endregion


        public QueueAgentReceiver()
        {
            Initilaized = false;
            MaxConnection = 30;
            Interval = 1000;
        }

        protected virtual void Init(string QueueAddress)
        {
            Source = QueueHost.Parse(QueueAddress);
            //QApi = new QueueApi(Source);
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

        public bool Pause(OnOffState onOff, int delay)
        {
            if (State != ListenerState.Started)
            {
                if (Is_Pause)
                {
                    PauseInterval = Math.Max(delay, 1000);
                }
                return Is_Pause;
            }
            if ((onOff == OnOffState.Toggle && State == ListenerState.Paused) || onOff == OnOffState.Off)
            {
                Is_Pause = false;
                OnStateChanged(ListenerState.Started);
            }
            else
            {
                PauseInterval = Math.Max(delay,1000);
                Is_Pause = true;
                OnStateChanged(ListenerState.Paused);
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

        protected virtual int ShouldPause()
        {
            return 0;
        }

        #endregion

        #region Publish

        protected virtual void OnMessageReceived(IQueueMessage message)
        {

        }

        protected virtual void CommitAsync(IAck ack, IQueueMessage message)
        {
            if (ack.IsOk)
                QueueApi.Get(Source).Commit(message.GetPtr()); // Commit(message.GetPtr());
            else
                QueueApi.Get(Source).Abort(message.GetPtr());// Abort(message.GetPtr());
        }

        protected virtual void StartConsume()
        {
            Interlocked.Increment(ref m_Connections);
            OnInfo($"QueueListener StartConsume , m_Connections {m_Connections}");

            ListenerApi apiListener = new ListenerApi(Source);
            apiListener.ConnectTimeout = 5000;// 500000000;
            apiListener.ReadTimeout = ReadTimeout;

            Task.Run(() =>
            {
                apiListener.ConsumeAwait(int.MaxValue, (message) =>
                {

                    if (message != null)
                    {
                        OnInfo(message.Print());
                        OnMessageReceived(message);
                        //QueueApi.Get(Source).Commit(message.GetPtr());
                        autoResetEvent.Set();
                    }
                    else
                    {
                        Console.WriteLine("Get nothing!");
                    }
                    Interlocked.Decrement(ref m_Connections);
                }).ConfigureAwait(false);
            });
            autoResetEvent.WaitOne();
        }

        #endregion

        #region QueueProcess 
        private void QueueProcess(object state)
        {

            while (keepAlive)
            {

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

                    lock (m_lock)
                    {
                        StartConsume();
                    }
                }
                catch (ThreadAbortException)
                {
                    OnError($"Warn: QueueListener ThreadAborted {HostName}");
                }
                catch (Exception ex)
                {
                    OnError($"QueueListener {HostName}, Error:{ex.Message + " Trace:" + ex.StackTrace} ");
                }
                Thread.Sleep(Interval);
            }
            OnError($"Warn: QueueListener not keep Alive {HostName}");
        }


        #endregion


        //ListenerApi apiListener;



        /*
        public async Task ConsumeTask()
        {
            ListenerApi api = new ListenerApi(Source);
            api.ConnectTimeout = 500000000;
            api.ReadTimeout = ReadTimeout;

            await api.ConsumeAwait(60, (item) =>{

                if (item != null)
                {
                    Console.WriteLine(item.Print());
                }
                else
                {
                    Console.WriteLine("Get nothing!");
                }
            });
        }

        public void StartListnning()
        {
            ListenerApi api = new ListenerApi(Source);
            api.QueueListnning(null, (message) =>
            {
                Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}", message.MessageState, message.ArrivedTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), message.Host, message.Label, message.Identifier, message.Duration);

                var body = message.GetBody();
                string sbody = body == null ? "null" : body.ToString();
                Console.WriteLine("body: " + sbody);

            }, (message) =>
            {
                Console.WriteLine(message);
            });
        }


        public void DoListnning()
        {

            var adapter = new QueueAdapter()
            {
                Source = Source,
                IsAsync = true,
                IsMultiTask=true,
                Interval = 10,
                ConnectTimeout = 5000,
                ReadTimeout = 180000,
                WorkerCount = 3,
                MaxConnection=50,
                EnableDynamicWait = true,
                MessageReceivedAction = (message) =>
                {
                    Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}", message.MessageState, message.ArrivedTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), message.Host, message.Label, message.Identifier, message.Duration);

                    var body = message.GetBody();
                    string sbody = body == null ? "null" : body.ToString();
                    Console.WriteLine("body: " + sbody);
                },
                MessageFaultAction = (message) =>
                {
                    Console.WriteLine(message);
                }
            };

            QueueListener listener = new QueueListener(adapter);
            string logpath = NetlogSettings.GetDefaultPath("qlistener");
            listener.Logger = new Logger(logpath);
            //listener.ErrorOcurred += Listener_ErrorOcurred;
            //listener.MessageReceived += Listener_MessageReceived;
            listener.Start();

            //QueueApi api = new QueueApi(host);
            //api.ReceiveCompleted += api_ReceiveCompleted;

            ////api.Listener(10000, message_ReceiveCompleted);
            //bool KeepAlive = true;
            //int connectTimeout=10000;

            //    while (KeepAlive)
            //    {
            //        api.Receive(connectTimeout,message_ReceiveCompleted);
            //        Thread.Sleep(100);
            //    }




            //Console.WriteLine("QueueListener finished...");
            //Console.ReadLine();
        }

        private void Listener_MessageReceived(object sender, Nistec.Generic.GenericEventArgs<IQueueMessage> e)
        {
            var message = e.Args;
            Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}", message.MessageState, message.ArrivedTime, message.Host, message.Label, message.Identifier);

        }

        private void Listener_ErrorOcurred(object sender, Nistec.Generic.GenericEventArgs<string> e)
        {
            Console.WriteLine(e.Args);
        }

        public void DoSbscriberListener()
        {
            var settings = new TcpSettings()
            {
                Address = "127.0.0.1",
                ConnectTimeout = 5000,
                HostName = "Netcell",
                Port = 15002,
                IsAsync = false
            };
            var qhost = QueueHost.Parse(string.Format("file:{0}:Queues?{1}", Assists.EXECPATH, settings.HostName));
            qhost.CoverMode = CoverMode.FileStream;
            qhost.CommitMode = PersistCommitMode.OnMemory;
            qhost.ReloadOnStart = true;

            var listener = new TopicSbscriberListener(qhost, true)
            {
                OnItemReceived = (IQueueMessage message) =>
                {

                    Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}", message.MessageState, message.ArrivedTime, message.Host, message.Label, message.Identifier);

                    return new QueueAck(Nistec.Messaging.MessageState.Received, message).ToTransStream();
                },
                OnError= (string message) => {
                    Console.WriteLine("OnError:{0}", message);

                }
            };
            string logpath = NetlogSettings.GetDefaultPath("topicSubs");
            listener.Logger = new Logger(logpath,LoggerMode.Console| LoggerMode.File);
            listener.InitServerQueue(settings,true);
            //listener.PausePersistQueue(true);
        }
        */
    }

    /*
    public class TopicSubs : TopicSbscriberListener
    {

        public TopicSubs() : base()
        {

            var settings = new TcpSettings()
            {
                Address = "127.0.0.1",
                ConnectTimeout = 5000000,
                HostName = "Netcell",
                Port = 15002,
                IsAsync = false
            };
            InitTcpServerQueue(settings);
        }


        public override TransStream OnMessageReceived(IQueueMessage message)
        {
            Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}", message.MessageState, message.ArrivedTime, message.Host, message.Label, message.Identifier);

            return new QueueAck(Nistec.Messaging.MessageState.Received,message).ToTransStream();
        }
    }
    */
}
