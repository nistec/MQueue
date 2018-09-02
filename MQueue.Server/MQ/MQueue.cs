using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.Messaging;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Nistec.Runtime;
using Nistec.Messaging.Remote;
using Nistec.Generic;
using Nistec.Threading;
using System.IO;
using Nistec.Logging;
using System.Threading.Tasks;
using Nistec.Messaging.Server;
using Nistec.Messaging.Io;
using System.Transactions;
using Nistec.Messaging.Config;
using Nistec.Data.Entities;

namespace Nistec.Messaging
{


    /// <summary>
    /// Provides access to a queue on a McMessage Queuing
    /// </summary>
    [Serializable]
    public sealed class MQueue : IReceiveCompleted, IQueuePerformance, IDisposable
    {
        #region Members

        private PriorityQueue Q;


        #endregion

        #region Members


        public const string DefaultDateFormat = "s";

        public const string DefaultCollate = "Hebrew_CI_AS";
        public const string DefaultDB = "MControlDB";
        public const int HoldPart = 3;
        public const int DefaultIntervalMinuteRecover = 43200;
        public const byte DefaultMaxRetry = 3;


        internal DateTime TimeStarted;

        //internal int m_Server;
        internal string m_QueueName;
        internal CoverMode m_CoverMode = CoverMode.Memory;
        bool _Coverable = false;
        //QCover _QCover;

        internal int m_EnqueueWait = 10;
        internal int m_DequeueWait = 10;

        internal bool m_enabled = true;

        //internal bool m_UseDuration=false;
        internal bool m_isTrans = false;
        internal byte m_maxRetry = DefaultMaxRetry;
        //private int m_MaxItemsPerSecond = 0;
        ManualResetEvent resetEvent;

        //private string dateFormat = DefaultDateFormat;
        internal string collate = DefaultCollate;
        private int maxCapacity = 1000000;
        private bool holdDequeue = false;
        private bool holdEnqueue = false;
        internal bool enqueueHoldItems = false;
        internal bool reEnqueueItems = false;

        //private bool serializeBody = false;


        internal bool IsDbQueue
        {
            get
            {
                return Mode == CoverMode.Db;
            }
        }
        internal bool IsFileQueue
        {
            get
            {
                return Mode == CoverMode.File;// || Mode == QueueMode.FileStream;
            }
        }
        internal bool IsCoverable
        {
            get
            {
                return Mode != CoverMode.Memory;
            }
        }


        private int m_Lock;

        internal void LOCK()
        {
            Interlocked.Exchange(ref m_Lock, 1);
        }
        internal void UNLOCK()
        {
            Interlocked.Exchange(ref m_Lock, 0);
        }
        internal int ISLOCK()
        {
            //if (0 == Interlocked.CompareExchange(ref m_Lock, 1, m_Lock))
            //    return 0;
            return m_Lock;
        }



        #endregion

        #region message events

        /// <summary>
        /// Message Received
        /// </summary>
        public event QueueItemEventHandler MessageReceived;
        /// <summary>
        /// Message Arraived
        /// </summary>
        public event QueueItemEventHandler MessageArraived;

        /// <summary>
        /// OnMessageArraived
        /// </summary>
        /// <param name="e"></param>
        void OnMessageArraived(QueueItemEventArgs e)
        {
            if (MessageArraived != null)
                MessageArraived(this, e);
        }
        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="e"></param>
        void OnMessageReceived(QueueItemEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }

        #endregion

        #region Constructor
        /// <summary>
        /// MQueue Ctor
        /// </summary>
        /// <param name="mqp"></param>
        public MQueue(IQProperties prop)
        {
            Console.WriteLine("Init MQueue " + prop);


            TimeStarted = DateTime.Now;
            LOCK();

            m_enabled = true;

            //m_Server = prop.Server;
            m_QueueName = prop.QueueName;
            m_CoverMode = prop.Mode;
            m_maxRetry = prop.MaxRetry;
            m_isTrans = prop.IsTrans;
            RoutHost = prop.GetRoutHost();
            if (IsCoverable)
            {
                //_QCover = new QCover()
                //{
                //    ConnectTimeout = prop.ConnectTimeout,
                //    CoverProvider = IsDbQueue ? CoverProviders.Db : CoverProviders.File,
                //    CoverPath = prop.CoverPath
                //};
                m_QueuesPath = AgentManager.Settings.QueuesPath;
            }

            resetEvent = new ManualResetEvent(false);

            m_Perfmon = new QueuePerformanceCounter(this, QueueAgentType.MQueue, m_QueueName);

            Q = prop.Factory();

            Q.MessageArrived += new QueueItemEventHandler(Q_MessageArrived);
            Q.MessageReceived += new QueueItemEventHandler(Q_MessageReceived);
            Q.TransactionBegin += new QueueItemEventHandler(Q_MessageTransBegin);
            Q.TransactionEnd += new QueueItemEventHandler(Q_MessageTransEnd);
            Q.ErrorOccured += new QueueItemEventHandler(Q_ErrorOccured);
            InitRecoverQueue(DefaultIntervalMinuteRecover);
            UNLOCK();
            //if (recoverable)
            //{
            //    InitRecoverQueue(DefaultIntervalMinuteRecover);
            //}
            //else
            //{
            //    UNLOCK();
            //}

        }



        /// <summary>
        /// MQueue Ctor
        /// </summary>
        /// <param name="queueName"></param>
        public MQueue(string queueName)
            : this(new QProperties(queueName, false, CoverMode.Memory))
        {
        }



        internal void InitRecoverQueue(int intervalMinuteRecover)
        {

            Console.WriteLine("Init RecoverQueue");

            try
            {
                if (IsFileQueue)
                {
                    Assists.EnsureQueueSectionPath(m_QueuesPath, Assists.FolderQueue, m_QueueName);
                    Assists.EnsureQueueSectionPath(m_QueuesPath, Assists.FolderInfo, m_QueueName);
                    Assists.EnsureQueueSectionPath(m_QueuesPath, Assists.FolderCovered, m_QueueName);
                    Assists.EnsureQueueSectionPath(m_QueuesPath, Assists.FolderSuspend, m_QueueName);

                    //string path = GetQueuePath();
                    //if (!Directory.Exists(path))
                    //{
                    //    Directory.CreateDirectory(Path.Combine(m_QueuesPath, IoAssists.FolderQueue));
                    //    Directory.CreateDirectory(Path.Combine(m_QueuesPath, IoAssists.FolderInfo));
                    //    Directory.CreateDirectory(Path.Combine(m_QueuesPath, IoAssists.FolderCovered));
                    //    Directory.CreateDirectory(Path.Combine(m_QueuesPath, IoAssists.FolderSuspend));
                    //    Console.WriteLine("Create MQueue Folder: " + path);
                    //}

                    AsyncReEnqueueItems();

                }

                if (IsTrans)
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(QueueName + " Error:" + ex.Message);

            }
        }

        internal void Reload()
        {
            if (Q is PriorityMemQueue)
                ((PriorityMemQueue)Q).ReloadItemsInternal();
            else if (Q is PriorityPersistQueue)
                ((PriorityPersistQueue)Q).ReloadItemsInternal();

            //Q.ReloadItems();
        }
        internal void Clear()
        {
            Q.Clear();
        }

        ~MQueue()
        {
            Dispose(false);
        }

        bool IsDisposed
        {
            get { return disposed; }
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                    if (Q != null)
                    {
                        resetEvent = null;

                        Q.MessageArrived -= new QueueItemEventHandler(Q_MessageArrived);
                        Q.MessageReceived -= new QueueItemEventHandler(Q_MessageReceived);
                        Q.TransactionBegin -= new QueueItemEventHandler(Q_MessageTransBegin);
                        Q.TransactionEnd -= new QueueItemEventHandler(Q_MessageTransEnd);
                    }

                    m_QueueName = null;
                    //connection = null;
                    collate = null;
                    //dateFormat = null;

                    //m_StartupPath = null;
                    //m_QueuesPath = null;

                    GC.SuppressFinalize(this);
                }
            }
            disposed = true;
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (!IsDisposed)
        //    {
        //        if (disposing)
        //        {
        //            if (Q != null)
        //            {
        //                resetEvent = null;

        //                Q.MessageArrived -= new QueueItemEventHandler(Q_MessageArrived);
        //                Q.MessageReceived -= new QueueItemEventHandler(Q_MessageReceived);
        //                Q.TransactionBegin -= new QueueItemEventHandler(Q_MessageTransBegin);
        //                Q.TransactionEnd -= new QueueItemEventHandler(Q_MessageTransEnd);
        //            }
        //        }
        //        base.Dispose(disposing);
        //    }
        //    //GC.SuppressFinalize(this);
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region MQueueBase

        #region Events

        /// <summary>
        /// ErrorOcurred
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> ErrorOcurred;
        ///// <summary>
        ///// MessageHandler
        ///// </summary>
        //public event QueueItemEventHandler ReceiveCompleted;
        /// <summary>
        /// PropertyChanged
        /// </summary>
        public event EventHandler<GenericEventArgs<string, object>> PropertyChanged;

        /// <summary>
        /// ErrorOcurred
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> CapacityExceeds;


        /// <summary>
        /// OnCapacityExceeds
        /// </summary>
        /// <param name="e"></param>
        void OnCapacityExceeds(GenericEventArgs<string> e)
        {
            if (CapacityExceeds != null)
                CapacityExceeds(this, e);
        }

        /// <summary>
        /// OnErrorOcurred
        /// </summary>
        /// <param name="msg"></param>
        private void OnErrorOcurred(string msg)
        {
            Console.WriteLine("ErrorOcurred: " + msg);
            OnErrorOcurred(new GenericEventArgs<string>(msg));
        }
        /// <summary>
        /// OnErrorOcurred
        /// </summary>
        /// <param name="e"></param>
        void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
                ErrorOcurred(this, e);
        }

        /// <summary>
        /// OnPropertyChanged
        /// </summary>
        /// <param name="e"></param>
        void OnPropertyChanged(GenericEventArgs<string, object> e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        internal void OnPropertyChanged(string propertyName, object propertyValue)
        {
            OnPropertyChanged(new GenericEventArgs<string, object>(propertyName, propertyValue));
        }

        #endregion

        #region properties


        /// <summary>
        /// Get max retry for item
        /// </summary>
        public byte MaxRetry
        {
            get { return m_maxRetry; }
        }
        /// <summary>
        /// Get if is it trnsactional queue
        /// </summary>
        public bool IsTrans
        {
            get { return m_isTrans; }
        }

        /// <summary>
        /// Get QueueMode
        /// </summary>
        public CoverMode Mode
        {
            get
            {
                return m_CoverMode;
            }
        }

        ///// <summary>
        ///// QueueName
        ///// </summary>
        public string QueueName
        {
            get { return m_QueueName; }
        }

        ///// <summary>
        ///// Get or Set Max Items Per Second,if zero no limit item Per Second
        ///// </summary>
        //public virtual int MaxItemsPerSecond
        //{
        //    get
        //    {
        //        return m_MaxItemsPerSecond;
        //    }
        //    set
        //    {
        //        if (value < 0)
        //        {
        //            throw new ArgumentException("required more then or equal to 0");
        //        }
        //        if (m_MaxItemsPerSecond != value)
        //        {
        //            m_MaxItemsPerSecond = value;
        //            m_UseDuration = m_MaxItemsPerSecond > 0;
        //            OnPropertyChanged("MaxItemsPerSecond", MaxItemsPerSecond);
        //        }
        //    }
        //}


        /// <summary>
        /// Get or Set a value indicating whether the Queue is holding Dequeue.
        /// </summary>
        public bool HoldDequeue
        {
            get { return holdDequeue; }
            set
            {
                if (holdDequeue != value)
                {
                    holdDequeue = value;
                    OnPropertyChanged("HoldDequeue", holdDequeue);
                }
            }
        }
        /// <summary>
        /// Get or Set a value indicating whether the Queue is holding Enqueue.
        /// </summary>
        public bool HoldEnqueue
        {
            get { return holdEnqueue; }
            set
            {
                if (holdEnqueue != value)
                {
                    holdEnqueue = value;
                    OnPropertyChanged("HoldEnqueue", holdEnqueue);
                }
            }
        }
        /// <summary>
        /// Get or Set a value indicating whether the Queue is Enabled.
        /// </summary>
        public bool Enabled
        {
            get { return m_enabled; }
            set
            {
                if (m_enabled != value)
                {
                    m_enabled = value;
                    OnPropertyChanged("Enabled", m_enabled);
                }
            }
        }


        /// <summary>
        /// Get or Set a value indicating whether the Max Capacity in Queue.
        /// </summary>
        public int MaxCapacity
        {
            get { return maxCapacity; }
            set
            {
                if (value > 0 && maxCapacity != value)
                {
                    maxCapacity = value;
                    //SetCapacity(MinCapacity, value);
                    OnPropertyChanged("MaxCapacity", MaxCapacity);
                }
            }
        }
        /// <summary>
        /// Get a value indicating whether the Min Capacity in Queue.
        /// </summary>
        internal int MinCapacity
        {
            get
            {
                return (int)(maxCapacity * .75);

                //if (minCapacity <= 0)
                //    return (int)(maxCapacity * .75);
                //return minCapacity; 
            }
        }


        /// <summary>
        /// Get or Set the DB Collation.
        /// </summary>
        public string Collate
        {
            get
            {
                if (string.IsNullOrEmpty(collate))
                    return DefaultCollate;
                return collate;
            }
            set
            {
                if (collate != value)
                {
                    if (string.IsNullOrEmpty(value))
                        value = DefaultCollate;
                    collate = value;
                    OnPropertyChanged("Collate", collate);
                }
            }
        }

        #endregion

        #region Duration

        //private int itemCount;
        //private DateTime startCircle;

        //internal bool ShouldWait()
        //{

        //    TimeSpan ts = DateTime.Now.Subtract(startCircle);
        //    if (ts.TotalSeconds > 1)
        //    {
        //        startCircle = DateTime.Now;
        //        Interlocked.Exchange(ref itemCount, 0);
        //        return false;
        //    }
        //    else
        //    {
        //        if (itemCount >= MaxItemsPerSecond)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            Interlocked.Increment(ref this.itemCount);
        //            return false;
        //        }
        //    }
        //    // return false;
        //}

        #endregion

        #region Cover items

        public bool IsCapacityExceeds()
        {
            int maxCapacity = MaxCapacity;
            if (maxCapacity == 0)
                return false;
            if (Count >= maxCapacity)
            {
                OnCapacityExceeds(new GenericEventArgs<string>(this.QueueName));
                return true;
            }
            return false;
        }

        /// <summary>
        /// CompletedMessage
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="status"></param>
        public void Completed(Guid ItemId, int status)
        {

            //if (CoverMode == CoverMode.FileSystem)
            //{
            //    SysUtil.DeleteFile(GetFilename(ItemId.ToString()));

            //}
        }

        #endregion

        #region public methods


        /// <summary>
        /// Abort Transaction
        /// </summary>
        /// <param name="ItemId"></param>
        /// <returns></returns>
        public void AbortTrans(Guid ItemId)
        {

            //Q.AbortTrans(ItemId);
            Completed(ItemId, (int)ItemState.Abort);

            //ReEnqueue(item);
        }

        /// <summary>
        /// Commit Transaction
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="hasAttach"></param>
        /// <returns></returns>
        public void CommitTrans(Guid ItemId)
        {
            //Q.CommitTrans(ItemId);
            Completed(ItemId, (int)ItemState.Commit);
        }

        #endregion

        #region properties

        /// <summary>
        /// Get if Queue Initilaized.
        /// </summary>
        public bool Initilaized
        {
            get { return ISLOCK() == 0; }
        }

        /// <summary>
        /// Get Count of items in Queue.
        /// </summary>
        public int Count
        {
            get { return Q.TotalCount; }
        }

        public bool CanQueue(uint count)
        {
            return (Count + count) < MaxCapacity;
        }

        /// <summary>
        /// Get the capacity for auto re enqueue hold items
        /// </summary>
        public int CapacityReEnqueue
        {
            get
            {
                int reCap = MaxCapacity - Count;
                return (reCap < 0) ? 0 : reCap;

                //return m_Capacity;
            }
        }

        /// <summary>
        /// Get a value indicating whether the Queue in EnqueueHoldItems Proccess.
        /// </summary>
        public bool EnqueueHoldItems
        {
            get { return enqueueHoldItems; }
        }

        #endregion

        /// <summary>
        /// ClearQueueItems
        /// </summary>
        /// <param name="type"></param>
        public void ClearQueueItems(QueueItemType type)
        {
            switch (type)
            {
                case QueueItemType.QueueItems:
                case QueueItemType.AllItems:
                    Q.Clear();
                    break;
            }
        }

        #region Invoke re enqueue

        private void CleanFolder()
        {
            string path = GetQueuePath();
            string pathrelay = GetRelayPath();
            string pathback = GetBackupPath();

            if (Directory.Exists(path))
            {

                if (!Directory.Exists(pathrelay))
                {
                    Directory.CreateDirectory(pathrelay);
                }

                if (!Directory.Exists(pathback))
                {
                    Directory.CreateDirectory(pathback);
                }
                //clean relay files to backup
                string[] relays = Directory.GetFiles(pathrelay, "*.mcq");
                if (relays != null)
                {
                    foreach (string rely in relays)
                    {
                        string relyID = Path.GetFileNameWithoutExtension(rely);
                        string backfile = SysUtil.PathFix(pathback + relyID + ".mcq");
                        SysUtil.MoveFile(rely, backfile);
                    }

                    //Netlog.InfoFormat("CleanFolder Buckup files: {0} ", relays.Length);
                }

                //clean folder items and move them to relay
                string[] messages = Directory.GetFiles(path, "*.mcq");
                if (messages != null)
                {
                    foreach (string message in messages)
                    {
                        string messageID = Path.GetFileNameWithoutExtension(message);
                        string newfile = SysUtil.PathFix(pathrelay + messageID + ".mcq");
                        SysUtil.MoveFile(message, newfile);
                    }
                    //Netlog.InfoFormat("CleanFolder items files: {0} ", messages.Length);
                }
            }
        }

        private void ReEnqueueFiles()
        {
            if (reEnqueueItems)
                return;
            try
            {
                Console.WriteLine("Start ReEnqueueQueueItems");

                reEnqueueItems = true;

                string path = GetRelayPath();

                if (Directory.Exists(path))
                {
                    string[] messages = Directory.GetFiles(path, "*.mcq");
                    if (messages == null || messages.Length == 0)
                    {
                        return;
                    }

                    Console.WriteLine("{0} items found to ReEnqueue", messages.Length);

                    Netlog.InfoFormat("ReEnqueueFiles: {0} ", messages.Length);


                    foreach (string message in messages)
                    {
                        //while (this.Count > 1000)
                        //{

                        //    Thread.Sleep(1000);
                        //}

                        QueueItem item = QueueItem.ReadFile(message);
                        if (item != null)
                        {
                            Enqueue(item as IQueueItem);
                        }
                        SysUtil.DeleteFile(message);
                        Thread.Sleep(100);
                    }
                    Netlog.Info("ReEnqueueFiles finished. ");
                }

            }
            catch (Exception ex)
            {
                string s = ex.Message;

            }
            finally
            {
                reEnqueueItems = false;
            }
        }

        private void AsyncReEnqueueItems()
        {
            Thread th = null;

            if (IsFileQueue)
            {
                th = new Thread(new ThreadStart(ReEnqueueFiles));
            }
            else
            {
                return;
                //th= new Thread(new ThreadStart(ReEnqueueDB));
            }
            th.Start();
        }
        #endregion


        #endregion

        #region Queue events

        void Q_MessageReceived(object sender, QueueItemEventArgs e)
        {
            //if (base.CoverMode > CoverMode.None)
            //{
            //    OnCoverQueueItem(e.Item, e.State);//IsTrans ? ItemState.Wait : ItemState.Dequeue);
            //}

            if (AgentManager.Settings.EnablePerformanceCounter)
                Task.Factory.StartNew(() => m_Perfmon.AddDequeue(0));

            OnMessageReceived(e);// OnDequeueMessage(e);
        }

        void Q_MessageArrived(object sender, QueueItemEventArgs e)
        {
            //if (base.CoverMode > CoverMode.None)
            //{
            //    //Console.WriteLine("OnEnqueueMessage");

            //    OnCoverQueueItem(e.Item, e.State);
            //    //AsyncQueueWorker(e.Item);
            //}

            //if (_Coverable)
            //{
            //    _QCover.Save(e.Item);
            //}

            if (AgentManager.Settings.EnablePerformanceCounter)
                Task.Factory.StartNew(() => m_Perfmon.AddEnqueue(0));

            OnMessageArraived(e);// OnEnqueueMessage(e); //normalQ.Dequeue();
        }

        void Q_MessageTransBegin(object sender, QueueItemEventArgs e)
        {
            //TODO:FIX
            //TransBegin(e.Item);
            //base.TransBegin(e.Item);
        }
        void Q_MessageTransEnd(object sender, QueueItemEventArgs e)
        {
            //TODO:FIX
            //if (e.State == ItemState.Wait)
            //{
            //    ReEnqueue(e.Item);
            //}
        }

        void Q_ErrorOccured(object sender, QueueItemEventArgs e)
        {
            if (AgentManager.Settings.EnablePerformanceCounter)
                Task.Factory.StartNew(() => m_Perfmon.AddStateCounter(e.State));
        }

        #endregion

        #region ICachePerformance

        QueuePerformanceCounter m_Perfmon;
        /// <summary>
        /// Get <see cref="CachePerformanceCounter"/> Performance Counter.
        /// </summary>
        internal IQueuePerformance PerformanceCounter
        {
            get { return m_Perfmon as IQueuePerformance; }
        }

        public IDictionary<string, object> GetReport()
        {
            if (m_Perfmon == null)
                return null;
            return m_Perfmon.GetPerformanceReport();
        }

        /// <summary>
        ///  Sets the memory size as an atomic operation.
        /// </summary>
        /// <param name="memorySize"></param>
        void IQueuePerformance.MemorySizeExchange(ref long memorySize)
        {
            LogAction(MessageState.None, "Memory Size Exchange:" + m_QueueName);
            //long size = GetMemorySize();
            //Interlocked.Exchange(ref memorySize, size);
        }

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long IQueuePerformance.GetMaxSize()
        {
            return this.MaxCapacity;
        }
        bool IQueuePerformance.IsRemote
        {
            get { return true; }
        }
        int IQueuePerformance.IntervalSeconds
        {
            get { return 0; }
        }
        bool IQueuePerformance.Initialized
        {
            get { return this.Initilaized; }
        }

        #endregion

        #region  asyncInvoke
#if (false)
        /// <summary>
        /// DefaultMaxTimeout
        /// </summary>
        public static readonly TimeSpan DefaultMaxTimeout = TimeSpan.FromMilliseconds(4294967295);

        //private Hashtable hashAsyncRequests;
        private AsyncCallback onRequestCompleted;
        private ManualResetEvent resetEvent;
       
        /// <summary>
        /// Receive Completed event
        /// </summary>
        public event ReceiveItemCompletedEventHandler ReceiveCompleted;



        /// <summary>
        /// OnReceiveCompleted
        /// </summary>
        /// <param name="e"></param>
        void OnReceiveCompleted(ReceiveItemCompletedEventArgs e)
        {
            if (ReceiveCompleted != null)
                ReceiveCompleted(this, e);
        }

        //public void RequestCompleted(IAsyncResult asyncResult)
        //{
        //    OnReceiveCompleted(new ReceiveCompletedEventArgs(this, asyncResult));
        //}

        private IQueueItem ReceiveItemWorker(TimeSpan timeout, object state)
        {
            if (this.HoldDequeue)
            {
                return null;
            }
            DateTime startReceive = DateTime.Now;

            IQueueItem item=null;

            
            while (item == null)
            {
                if (timeout < DateTime.Now.Subtract(startReceive))
                {
                    //TODO: raise event
                    state = ReceiveState.Timeout; 
                    break;
                }
                item = Dequeue();
                if (item == null)
                {
                    Thread.Sleep(m_DequeueWait);
                }
            }
            if (item != null)
                state = ReceiveState.Success;
           
            //Console.WriteLine("Dequeue item :{0}", item.ItemId);
            return item;//.Copy() as IQueueItem;
        }

        /// <summary>
        /// AsyncReceive
        /// </summary>
        /// <returns></returns>
        public IQueueItem AsyncReceive()
        {
            object state = new object();
            TimeSpan ts = TimeSpan.FromSeconds(QueueDefaults.DefaultRecieveTimeOutInSecond);

            ReceiveItemCallback caller = new ReceiveItemCallback(ReceiveItemWorker);

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(ts,state,  CreateCallBack(), caller);
            Thread.Sleep(10);

            result.AsyncWaitHandle.WaitOne();
            
            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
            IQueueItem item = caller.EndInvoke(result);
            AsyncCompleted(item);
            return item;

        }

        /// <summary>Initiates an asynchronous receive operation that has a specified time-out and a specified state object. The state object provides associated information throughout the lifetime of the operation. This overload receives notification, through a callback, of the identity of the event handler for the operation. The operation is not complete until either a message becomes available in the queue or the time-out occurs.</summary>
        /// <returns></returns>
        public IAsyncResult BeginReceive(object state)
        {
            return BeginReceive(TimeSpan.FromSeconds(QueueDefaults.DefaultRecieveTimeOutInSecond), state, null);
        }
        /// <summary>Initiates an asynchronous receive operation that has a specified time-out and a specified state object. The state object provides associated information throughout the lifetime of the operation. This overload receives notification, through a callback, of the identity of the event handler for the operation. The operation is not complete until either a message becomes available in the queue or the time-out occurs.</summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public IAsyncResult BeginReceive(TimeSpan timeout, object state)
        {
            return BeginReceive(timeout, state, null);
        }

        /// <summary>Initiates an asynchronous receive operation that has a specified time-out and a specified state object. The state object provides associated information throughout the lifetime of the operation. This overload receives notification, through a callback, of the identity of the event handler for the operation. The operation is not complete until either a message becomes available in the queue or the time-out occurs.</summary>
        /// <param name="callback">The <see cref="T:System.AsyncCallback"></see> that receives the notification of the asynchronous operation completion. </param>
        /// <param name="state">A state object, specified by the application, that contains information associated with the asynchronous operation. </param>
        /// <param name="timeout">A <see cref="T:System.TimeSpan"></see> that indicates the interval of time to wait for a message to become available. </param>
        /// <returns>The <see cref="T:System.IAsyncResult"></see> that identifies the posted asynchronous request.</returns>
        public IAsyncResult BeginReceive(TimeSpan timeout, object state, AsyncCallback callback)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if ((totalMilliseconds < 0L) || (totalMilliseconds > 4294967295L))
            {
                throw new ArgumentException("InvalidParameter", "timeout");
            }
            ReceiveItemCallback caller = new ReceiveItemCallback(ReceiveItemWorker);

            if (callback == null)
            {
                callback = CreateCallBack();
            }
            if (state == null)
            {
                state = new object();
            }
            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke(timeout,state, callback,caller);
            this.resetEvent.Set();
            return result;
        }


        /// <summary>Completes the specified asynchronous receive operation.</summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public IQueueItem EndReceive(IAsyncResult asyncResult)
        {
            // Retrieve the delegate.
            ReceiveItemCallback caller = (ReceiveItemCallback)asyncResult.AsyncState;

            // Call EndInvoke to retrieve the results.
            IQueueItem item = (IQueueItem)caller.EndInvoke(asyncResult);

            AsyncCompleted(item);
            this.resetEvent.WaitOne();
            return item;
        }

        private AsyncCallback CreateCallBack()
        {
            if (this.onRequestCompleted == null)
            {
                this.onRequestCompleted = new AsyncCallback(this.OnRequestCompleted);
            }
            return this.onRequestCompleted;
        }

        private void AsyncCompleted(IQueueItem item)
        {
            if (item != null)
            {
                if (item != null && IsTrans)
                {
                    //this.TransBegin(item);
                }
                else
                {
                    this.Completed(item.ItemId, (int)ItemState.Commit);
                }
            }
        }

        private void OnRequestCompleted(IAsyncResult asyncResult)
        {
            OnReceiveCompleted(new ReceiveItemCompletedEventArgs(this, asyncResult));
        }
#endif

        #endregion

        #region public methods

        /// <summary>
        /// Get persist items
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IPersistEntity> QueryItems()
        {
            return Q.QueryItems();
        }

        /// <summary>
        /// Get persist items
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IPersistEntity> QueryItemsTable()
        {
            return Q.QueryItems();
        }

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Peek()
        {
            return Q.Peek();
        }

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public IQueueItem Peek(Priority priority)
        {
            return Q.Peek(priority);
        }

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Peek(Ptr ptr)
        {
            return Q.Peek(ptr);
        }

        public bool TryDequeue(out IQueueItem item)
        {
            return Q.TryDequeue(out item);
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Dequeue()
        {

            //if (m_UseDuration)
            //{
            //    if (ShouldWait())
            //    {
            //        Thread.Sleep(100);
            //        return null;
            //    }
            //}

            return Q.Dequeue();
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public IQueueItem Dequeue(Priority priority)
        {

            //if (m_UseDuration)
            //{
            //    if (ShouldWait())
            //    {
            //        Thread.Sleep(100);
            //        return null;
            //    }
            //}

            return Q.Dequeue(priority);
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Dequeue(Ptr ptr)
        {
            return Q.Dequeue(ptr);
        }

        /// <summary>
        /// Enqueue Message
        /// </summary>
        /// <param name="item"></param>
        public IQueueAck Enqueue(IQueueItem item)
        {
            if (this.HoldEnqueue || IsCapacityExceeds())
            {
                //if (CoverMode > CoverMode.None)
                //{
                //    OnCoverHoldItem(item, 0, 0);
                //}
                //Thread.Sleep(m_EnqueueWait);

                return new QueueAck(MessageState.QueueInHold, item);// Ptr.Get(item,PtrState.QueueInHold);
            }

            while (ISLOCK() > 0)
            {
                Thread.Sleep(100);
            }

            //using (TransactionScope tran = new TransactionScope())
            //{

            var ack = Q.Enqueue(item);

            //if (_Coverable)
            //{
            //    _QCover.Save(item);
            //}
            return ack;
            //}

        }


        ///// <summary>
        ///// ReEnqueueMessage
        ///// </summary>
        ///// <param name="item"></param>
        //public override Ptr ReEnqueue(IQueueItem item)
        //{

        //    if (this.HoldEnqueue)
        //    {
        //        //OnCoverHoldItem(item, 0, 0);
        //        return Ptr.Get(PtrState.QueueInHold);
        //    }
        //    if (item.Retry > MaxRetry)
        //    {
        //        //Thread.Sleep(m_EnqueueWait);
        //        //TOTDO:REPORT THIS
        //        return Ptr.Get(PtrState.MaxRetryExceeds);
        //    }

        //    ((IQueueItem)item).DoRetry();//.Retry += 1;
        //    return Q.Enqueue(item);
        //}


        #endregion

        //======================================================


        //internal bool recoverable;

        //private string m_StartupPath = "";
        private string m_QueuesPath = "";
        internal QueueHost RoutHost { get; set; }

        #region sys file

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ErrorHandler.DumpError((Exception)e.ExceptionObject, new System.Diagnostics.StackTrace());
        }

        private string GetFilename(string itemId)
        {
            return Path.Combine(GetQueuePath(), itemId) + ".mcq";
        }
        private string GetQueuePath()
        {
            return Path.Combine(m_QueuesPath, QueueName) + "\\";
        }
        private string GetRelayPath()
        {
            return Path.Combine(GetQueuePath(), "Relay") + "\\";
        }
        private string GetBackupPath()
        {
            return Path.Combine(GetQueuePath(), "Backup") + "\\";
        }

        #endregion

        #region View
        /*
        /// <summary>
        /// GetQueueItems
        /// </summary>
        /// <returns></returns>
        public IQueueItem[] GetQueueItems()
        {
            if (Q.TotalCount == 0)
                return null;
            return Q.Clone();
        }

        /// <summary>
        /// GetQueueItemsTable
        /// </summary>
        /// <returns></returns>
        public DataTable GetQueueItemsTable()
        {
            if (Q.TotalCount == 0)
                return null;

            DataTable dt = QueueManager.QueueItemSchema();
            IQueueItem[] items = Q.Items.ToArray();

            foreach (QueueItem item in items)
            {
                dt.Rows.Add(item.ItemArray());
            }
            return dt;
        }
        */
        #endregion

        #region commands properties

        public MQprop Property()
        {
            //MQueue copy = new MQueue(this.QueueName)
            //{
            //    //CapacityReEnqueue = this.CapacityReEnqueue,
            //    Collate = this.Collate,
            //    Count = this.Q.TotalCount,
            //    Enabled = this.Enabled,
            //    EnqueueHoldItems = this.enqueueHoldItems,
            //    HoldDequeue = this.HoldDequeue,
            //    HoldEnqueue = this.HoldEnqueue,
            //    //Initilaized = this.Initilaized,
            //    IsCoverable = this.Mode != CoverMode.Memory,
            //    IsDbQueue = this.IsDbQueue,
            //    IsFileQueue = this.IsFileQueue,
            //    IsTrans = this.IsTrans,
            //    MaxCapacity = this.MaxCapacity,
            //    MaxRetry = this.MaxRetry,
            //    MinCapacity = this.MinCapacity,
            //    Mode = this.Mode,
            //    QueueName = this.QueueName,
            //    RoutHost = this.RoutHost
            //}
            return new MQprop(this);
        }

        public Dictionary<string,object> QueueProperty()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            //var dic = DictionaryUtil.ToDictionary(this, "MQueue-" + this.QueueName);

            dic.Add("CapacityReEnqueue", this.CapacityReEnqueue);
            dic.Add("Collate", this.Collate);
            dic.Add("Count", this.Count);
            dic.Add("Enabled", this.Enabled);
            dic.Add("EnqueueHoldItems", this.EnqueueHoldItems);
            dic.Add("HoldDequeue", this.HoldDequeue);
            dic.Add("HoldEnqueue", this.HoldEnqueue);
            dic.Add("Initilaized", this.Initilaized);
            dic.Add("IsCoverable", this.IsCoverable);
            dic.Add("IsDbQueue", this.IsDbQueue);
            dic.Add("IsFileQueue", this.IsFileQueue);
            dic.Add("IsTrans", this.IsTrans);
            dic.Add("MaxCapacity", this.MaxCapacity);
            dic.Add("MaxRetry", this.MaxRetry);
            dic.Add("MinCapacity", this.MinCapacity);
            dic.Add("Mode", this.Mode);
            dic.Add("QueueName", this.QueueName);
            dic.Add("RoutHost", this.RoutHost);


            return dic;
        }

        ///// <summary>
        ///// ValidateHoldItems
        ///// </summary>
        //public void ValidateHoldItems()
        //{
        //    if (HoldItemsCount() > 0)
        //    {
        //        if (Q.TotalCount < MinCapacity && !HoldEnqueue)
        //        {
        //            HoldItemsEnqueue(CapacityReEnqueue);
        //        }
        //    }
        //}

        /// <summary>
        /// SetProperty
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void SetProperty(string propertyName, object propertyValue)
        {
            switch (propertyName)
            {
                //case "MaxItemsPerSecond":
                //    MaxItemsPerSecond = Types.ToInt(propertyValue, MaxItemsPerSecond);
                //    break;
                //case "EnqueueWait":
                //    EnqueueWait = Types.ToInt(propertyValue, EnqueueWait);
                //    break;
                //case "DequeueWait":
                //    DequeueWait = Types.ToInt(propertyValue, DequeueWait);
                //    break;
                //case "UseThreadSettings":
                //    UseThreadSettings = Types.ToBool(propertyValue, UseThreadSettings);
                //    break;
                //case "SerializeBody":
                //    SerializeBody = Types.ToBool(propertyValue, SerializeBody);
                //    break;
                //case "HoldInterval":
                //    HoldInterval = Types.ToInt(propertyValue, HoldInterval);
                //    break;
                //case "CapacityReEnqueue":
                //    CapacityReEnqueue = Types.ToInt(propertyValue, CapacityReEnqueue);
                //    break;
                //case "DateFormat":
                //    DateFormat = Types.NZ(propertyValue, DateFormat);
                //    break;
                //case "LogItems":
                //    LogItems = Types.ToBool(propertyValue, LogItems);
                //    break;
                case "HoldDequeue":
                    HoldDequeue = Types.ToBool(propertyValue, HoldDequeue);
                    break;
                case "HoldEnqueue":
                    HoldEnqueue = Types.ToBool(propertyValue, HoldEnqueue);
                    break;
                case "Enabled":
                    Enabled = Types.ToBool(propertyValue, Enabled);
                    break;
                case "MaxCapacity":
                    MaxCapacity = Types.ToInt(propertyValue, MaxCapacity);
                    break;
                //case "MinCapacity":
                //    MinCapacity = Types.ToInt(propertyValue, MinCapacity);
                //    break;
                case "Collate":
                    Collate = Types.NZ(propertyValue, Collate);
                    break;
                case "IsTrans":
                    m_isTrans = Types.ToBool(propertyValue, IsTrans);
                    break;
                case "MaxRetry":
                    m_maxRetry = (byte)Types.ToInt(propertyValue, MaxRetry);
                    break;
                case "Mode":
                    m_CoverMode = (CoverMode)Types.ParseEnum(typeof(CoverMode), propertyValue.ToString(), CoverMode.Memory);
                    break;
                //case "Provider":
                //    m_Provider = (QueueProvider)Types.ParseEnum(Provider.GetType(), propertyValue.ToString(), Provider);
                //    break;
                //case "Server":
                //    m_Server = Types.ToInt(propertyValue, Server);
                //    break;
                default:
                    return;

            }
        }


        /// <summary>
        /// GetProperty
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                //case "MaxItemsPerSecond":
                //    return MaxItemsPerSecond;
                //case "SerializeBody":
                //    return SerializeBody;
                //case "HoldInterval":
                //    return HoldInterval;
                case "CapacityReEnqueue":
                    return CapacityReEnqueue;
                //case "DateFormat":
                //    return DateFormat;
                case "HoldDequeue":
                    return HoldDequeue;
                case "HoldEnqueue":
                    return HoldEnqueue;
                case "Enabled":
                    return Enabled;
                case "MaxCapacity":
                    return MaxCapacity;
                case "MinCapacity":
                    return MinCapacity;
                case "Collate":
                    return Collate;
                case "IsTrans":
                    return IsTrans;
                case "MaxRetry":
                    return MaxRetry;
                case "Mode":
                    return Mode;
                //case "Provider":
                //    return Provider;
                //case "Server":
                //    return Server;
                default:
                    return null;

            }
        }
        #endregion

        internal static void LogAction(MessageState state, string message)
        {
            if ((int)state < 100)
                QLogger.InfoFormat("State:{0}, Message:{1}", state, message);
            else
                QLogger.ErrorFormat("State:{0}, Message:{1}", state, message);
        }
           

    }
}


