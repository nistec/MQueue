//#define Embedded

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Messaging;

using Nistec.Threading;
using Nistec.Data.SqlClient;
using Nistec.Data;
using System.Data;
using System.Data.SqlClient;
using Nistec.Runtime;
//using Nistec.Win;
//using Nistec.Messaging.Firebird;
//using Nistec.Loggers;


namespace Nistec.Legacy
{

 
    /// <summary>
    /// AsyncQueueHandlerBase
    /// </summary>
    [Serializable]
    public abstract class QueueBase : NetComponent, IDisposable//, IQueueProperty
    {

        //internal static Logger logger = Logger.Instance;

        static QueueBase()
        {
            //logger.RegisterLogger(@"D:\Nistec\Services\RemoteQueue\logQ.txt", true);
            //logger.WriteLoge("========== Started ==========");
            //Nistec.Messaging.McQueue q = new MessageQueue();
            //q.BeginReceive(
        }
		#region Members

#if Embedded
                public const string DefaultDateFormat = "MM/dd/yyyy hh:mm:ss";
#else
                public const string DefaultDateFormat = "s";
#endif

        public const string DefaultCollate = "Hebrew_CI_AS";
        public const string DefaultDB = "MControlDB";
        public const int HoldPart = 3;
        public const int DefaultIntervalMinuteRecover = 43200;
        public const byte DefaultMaxRetry = 3;

        //internal bool initilized=false;
        //private Thread thSetting;
        //private GenericThreadPool mainThreads;
        internal DateTime TimeStarted;

        internal int m_Server;
        //internal string connection;
        internal string m_QueueName;
        //internal bool m_Started;


        //private int m_MaxThread = 1;
        //private bool m_Recoverable=false;
        internal CoverMode m_CoverMode = CoverMode.None;
        
        internal int m_EnqueueWait = 10;
        internal int m_DequeueWait = 10;

        internal bool m_enabled = true;
        //internal int m_HoldInterval = 1440;
        //internal int m_Capacity = 100;
        //internal bool m_LogItems = false;

        internal bool m_UseDuration=false;
        internal bool m_isTrans = false;
        internal byte m_maxRetry = DefaultMaxRetry;
        //private QueueMode m_Mode;
        private int m_MaxItemsPerSecond = 0;

        //internal QueueProvider m_Provider;

        private string dateFormat = DefaultDateFormat;
        internal string collate = DefaultCollate;
        //internal string db = DefaultDB;
        private int maxCapacity = 1000000;
        //private int minCapacity = 0;
        private bool holdDequeue = false;
        private bool holdEnqueue = false;
        internal bool enqueueHoldItems = false;
        internal bool reEnqueueItems = false;

        //internal bool useThreadSettings = false;
        private bool serializeBody = false;


        /// <summary>
        /// ErrorOcurred
        /// </summary>
        public event ErrorOcurredEventHandler ErrorOcurred;
        ///// <summary>
        ///// MessageHandler
        ///// </summary>
        //public event QueueItemEventHandler ReceiveCompleted;
        /// <summary>
        /// PropertyChanged
        /// </summary>
        public event PropertyItemChangedEventHandler PropertyChanged;



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

		#region Constructor
        /// <summary>
        /// AsyncQueue Ctor
        /// </summary>
        /// <param name="queueName"></param>
        internal QueueBase(McQueueProperties mqp)
            : base()
        {
            TimeStarted = DateTime.Now;
            LOCK();
            if (mqp != null)
            {
                InitProperties(mqp);
            }
        }
 

        internal void InitProperties(McQueueProperties mqp)
        {
            Console.WriteLine("Init AsyncQueue ");
            //m_Started = false;
            m_enabled = true;

            //m_Provider = mqp.Provider;
            
            m_Server = mqp.Server;
            //connection = mqp.ConnectionString;
            m_QueueName = mqp.QueueName;
            //m_MaxThread = mqp.MaxThread;
            m_CoverMode = mqp.CoverMode;
            //enabled = mqp.Enabled;
            m_maxRetry = mqp.MaxRetry;
            m_isTrans = mqp.IsTrans;
            //m_Mode = mqp.QueueMode;

            Console.WriteLine("Init McQueue " + mqp);
        }


        ~QueueBase()
        {
           Dispose(false);
        }
   
        
        ///// <summary>
        ///// Dispose
        ///// </summary>
        //public virtual void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //private bool disposed = false;

        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                m_QueueName = null;
                //connection = null;
                collate = null;
                GC.SuppressFinalize(this);
            }

            //disposed = true;
        }


        #endregion
                
        #region public methods

        /// <summary>
        /// DequeueMessage
        /// </summary>
        /// <returns></returns>
        public abstract  IQueueItem Dequeue();
   
        /// <summary>
        /// EnqueueMessage
        /// </summary>
        /// <param name="item"></param>
        public abstract void Enqueue(IQueueItem item);

        /// <summary>
        /// ReEnqueueMessage
        /// </summary>
        /// <param name="item"></param>
        public abstract void ReEnqueue(IQueueItem item);

        /// <summary>
        /// CompletedMessage
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="status"></param>
        /// <param name="hasAttach"></param>
        public abstract void Completed(Guid ItemId, int status, bool hasAttach);
  
        /// <summary>
        /// Abort Transaction
        /// </summary>
        /// <param name="item"></param>
        /// <param name="hasAttach"></param>
        /// <returns></returns>
        public abstract void AbortTrans(Guid ItemId, bool hasAttach);
 
        /// <summary>
        /// Commit Transaction
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="hasAttach"></param>
        /// <returns></returns>
        public abstract void CommitTrans(Guid ItemId, bool hasAttach);
/*
        /// <summary>
        /// Execute Command
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public abstract object ExecuteCommand(string commandName, string command,params string[] param);
*/
        /// <summary>
        /// Get Count of items in Queue.
        /// </summary>
        public abstract int Count { get;}

        #endregion

        #region MainThread

        ///// <summary>
        ///// Start AsyncQueue Background multi thread Listner 
        ///// </summary>
        //protected  void StartAsyncQueue()
        //{
 
        //    m_Started = true;

        //    if (!enabled || initilized)
        //        return;

        //    Console.WriteLine("Create McQueue " + QueueName);
        //    try
        //    {
        //        //if (m_Recoverable)
        //        //{
        //        //    ClearFinallAllItems();

        //        //    ClearQueueItems(intervalMinuteRecover);
        //        //}

        //        mainThreads = new GenericThreadPool(QueueName, 1, m_MaxThread);
        //        initilized = true;
        //        //useQueueListner = true;
        //        mainThreads.StartThreadPool(new ThreadStart(MessageQueueListner), MaxThread);

        //        if (m_CoverMode > CoverMode.None)
        //        {
        //            //ReEnqueueQueueItems(intervalMinuteRecover);
        //        }
        //        if (useThreadSettings)
        //        {
        //            thSetting = new Thread(new ThreadStart(ThreadSettings));
        //            thSetting.IsBackground = true;
        //            thSetting.Start();

        //            //thFinall = new Thread(new ThreadStart(ThreadFinallSettings));
        //            //thFinall.IsBackground = true;
        //            //thFinall.Start();
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(QueueName + " Error:" + ex.Message);

        //    }
        //}
  

        ///// <summary>
        ///// Stop AsyncQueue Background multi thread Listner 
        ///// </summary>
        //protected void StopAsyncQueue()
        //{
        //    Console.WriteLine("Stop AsyncQueue " + QueueName);
          
        //    initilized = false;
        //    mainThreads.StopThreadPool();
        //    mainThreads = null;
        //    if (m_CoverMode > CoverMode.None)
        //    {
        //        thSetting.Abort();
        //        thSetting = null;
        //        //thFinall.Abort();
        //        //thFinall = null;
        //    }
        //    //GC.Collect();
        //    //GC.WaitForPendingFinalizers();
        //}

		#endregion

        #region properties

        
        ///// <summary>
        ///// Get max retry for item
        ///// </summary>
        //public QueueMode QueueMode
        //{
        //    get { return m_Mode; }
        //}
     
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
        /// Get Server
        /// </summary>
        public int Server
        {
            get { return m_Server; }
        }

        ///// <summary>
        ///// Get Connection String
        ///// </summary>
        //public string ConnectionString
        //{
        //    get { return connection; }
        //}

        ///// <summary>
        ///// Get Queue Provider
        ///// </summary>
        //public QueueProvider Provider
        //{
        //    get { return m_Provider; }
        //}

  
        ///// <summary>
        ///// Initilize
        ///// </summary>
        //public bool Initilize
        //{
        //    get
        //    {
        //        return initilized;
        //    }
        //}

        /// <summary>
        /// Get CoverMode
        /// </summary>
        public CoverMode CoverMode
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
        ///// Get MaxThread
        ///// </summary>
        //public int MaxThread
        //{
        //    get { return m_MaxThread; }
        //}


        /// <summary>
        /// Get or Set Max Items Per Second,if zero no limit item Per Second
        /// </summary>
        public virtual int MaxItemsPerSecond
        {
            get
            {
                return m_MaxItemsPerSecond;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("required more then or equal to 0");
                }
                if (m_MaxItemsPerSecond != value)
                {
                    m_MaxItemsPerSecond = value;
                    m_UseDuration = m_MaxItemsPerSecond > 0;
                    OnPropertyChanged("MaxItemsPerSecond", MaxItemsPerSecond);
                }
            }
        }

        ///// <summary>
        ///// Enqueue Wait interval
        ///// </summary>
        //public virtual int EnqueueWait
        //{
        //    get { return m_EnqueueWait; }
        //    set
        //    {
        //        if (value < 0)
        //        {
        //            throw new ArgumentException("required more then 0");
        //        }
        //        if (m_EnqueueWait != value)
        //        {
        //            m_EnqueueWait = value;
        //            OnPropertyChanged("EnqueueWait", m_EnqueueWait);
        //        }
        //    }
        //}
        ///// <summary>
        ///// Dequeue Wait interval
        ///// </summary>
        //public virtual int DequeueWait
        //{
        //    get { return m_DequeueWait; }
        //    set
        //    {
        //        if (value < 0)
        //        {
        //            throw new ArgumentException("required more then 0");
        //        }
        //        if (m_DequeueWait != value)
        //        {
        //            m_DequeueWait = value;
        //            OnPropertyChanged("DequeueWait", m_DequeueWait);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Get or Set wether the queue Use Background Thread Settings
        ///// </summary>
        //public virtual bool UseThreadSettings
        //{
        //    get
        //    {
        //        return useThreadSettings;
        //    }
        //    set
        //    {
        //        if (useThreadSettings != value)
        //        {
        //            useThreadSettings = value;
        //            OnPropertyChanged("UseThreadSettings", useThreadSettings);
        //        }
        //    }
        //}

        /// <summary>
        /// Get or Set wether the queue Use Serialization to body
        /// </summary>
        public virtual bool SerializeBody
        {
            get
            {
                return serializeBody;
            }
            set
            {
                if (serializeBody != value)
                {
                    serializeBody = value;
                    OnPropertyChanged("SerializeBody", serializeBody);
                }
            }
        }

    
  
        ///// <summary>
        ///// Get or Set wether the minute interval for auto enqueue hold items
        ///// </summary>
        //public virtual int HoldInterval
        //{
        //    get
        //    {
        //        return m_HoldInterval;
        //    }
        //    set
        //    {
        //        if (value < 1)
        //        {
        //            throw new ArgumentException("required more then 0");
        //        }
        //        if (m_HoldInterval != value)
        //        {
        //            m_HoldInterval = value;
        //            OnPropertyChanged("HoldInterval", m_HoldInterval);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Get or Set wether the capacity for auto re enqueue hold items
        ///// </summary>
        //public virtual int CapacityReEnqueue
        //{
        //    get
        //    {
        //        return m_Capacity;
        //    }
        //    set
        //    {
        //        if (value < 1)
        //        {
        //            throw new ArgumentException("required more then 0");
        //        }
        //        if (m_Capacity != value)
        //        {
        //            m_Capacity = value;
        //            OnPropertyChanged("CapacityReEnqueue", m_Capacity);
        //        }
        //    }
        //}

   
        ///// <summary>
        ///// Get ConnectionString for Queue log
        ///// </summary>
        //public string ConnectionString
        //{
        //    get { return connection; }
        //}
        ///// <summary>
        ///// Get a value indicating whether the Queue in EnqueueHoldItems Proccess.
        ///// </summary>
        //public bool EnqueueHoldItems
        //{
        //    get { return enqueueHoldItems; }
        //}

        /// <summary>
        /// Get or Set Date format
        /// </summary>
        public string DateFormat
        {
            get { return dateFormat; }
            set
            {
                if (dateFormat != value)
                {
                    dateFormat = value;
                    OnPropertyChanged("DateFormat", dateFormat);
                }
            }
        }
        ///// <summary>
        ///// Get or Set a value indicating whether the Queue is Log Items to QueueItems_log Table.
        ///// </summary>
        //public bool LogItems
        //{
        //    get { return m_LogItems; }
        //    set
        //    {
        //        if (m_LogItems != value)
        //        {
        //            m_LogItems = value;
        //            OnPropertyChanged("LogItems", m_LogItems);
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
        /// Get or Set a value indicating whether the Min Capacity in Queue.
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
            //set
            //{
            //    if (minCapacity != value)
            //    {
            //        SetCapacity(value, maxCapacity);
            //        OnPropertyChanged("MinCapacity", minCapacity);
            //    }
            //}
        }
        ///// <summary>
        ///// Set Capacity
        ///// </summary>
        ///// <param name="min"></param>
        ///// <param name="max"></param>
        //public void SetCapacity(int min, int max)
        //{
        //    if (min >= max || min < 0 || max <= 0)
        //    {
        //        throw new ArgumentException("Incorrect value");
        //    }
        //    maxCapacity = max;
        //    minCapacity = min <= 0 ? (int)(max*0.75) : min;
        //}

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

        private int itemCount;
        private DateTime startCircle;

        internal bool ShouldWait()
        {

            TimeSpan ts = DateTime.Now.Subtract(startCircle);
            if (ts.TotalSeconds > 1)
            {
                startCircle = DateTime.Now;
                Interlocked.Exchange(ref itemCount, 0);
                return false;
            }
            else
            {
                if (itemCount >= MaxItemsPerSecond)
                {
                    return true;
                }
                else
                {
                    Interlocked.Increment(ref this.itemCount);
                    return false;
                }
            }
            // return false;
        }

        #endregion
        
        #region Listners

        ///// <summary>
        ///// Message Queue Listner worker thread
        ///// </summary>
        //private void MessageQueueListner()
        //{
        //    Console.WriteLine("Create MessageQueueListner..." + m_QueueName);

        //    Thread.Sleep(60000);

        //    while (initilized)
        //    {
        //        if (holdDequeue)
        //        {
        //            Thread.Sleep(m_DequeueWait);
        //        }
        //        IQueueItem itm = (IQueueItem)this.Dequeue();
        //        if (itm != null)
        //        {
        //            OnReceiveCompleted(itm, ItemState.Dequeue);
        //        }
        //        Thread.Sleep(m_DequeueWait);
        //    }
        //}

        // protected virtual void ThreadSettings()
        //{

        //    Console.WriteLine("Create ThreadSettings..." + m_QueueName);

        //    while (initilized)
        //    {
        //        OnSettings();
        //        Thread.Sleep(60000);
        //    }
        //}


        ///// <summary>
        ///// OnSettings
        ///// </summary>
        //protected virtual void OnSettings()
        //{
        //    //Console.WriteLine("Peek Message Event ..." + QueueName);
        //}

        //private void OnReceiveCompleted(IQueueItem item, ItemState state)
        //{

        //    QueueItemEventArgs e = new QueueItemEventArgs(item, state);

        //    OnReceiveCompleted(e);

        //    if (e.State == ItemState.Abort)
        //    {
        //        ReEnqueue(item);
        //    }
        //    else if (IsTrans)
        //    {
        //        OnTransBegin(item);
        //    }
        //    else
        //    {
        //        Completed(item, ItemState.Commit, "");
        //    }

        //    Thread.Sleep(100);
        //}


        ///// <summary>
        ///// TransBegin
        ///// </summary>
        ///// <param name="item"></param>
        //public virtual void TransBegin(IQueueItem item)
        //{

        //}

        ///// <summary>
        ///// OnReceiveCompleted
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //protected virtual void OnReceiveCompleted(QueueItemEventArgs e)
        //{
        //    //Console.WriteLine("Peek Message Event ..." + QueueName);
        //    if (ReceiveCompleted != null)
        //        ReceiveCompleted(this, e);
        //}

 
        /// <summary>
        /// OnErrorOcurred
        /// </summary>
        /// <param name="msg"></param>
        private void OnErrorOcurred(string msg)
        {
            Console.WriteLine("ErrorOcurred: " + msg);
            OnErrorOcurred(new ErrorOcurredEventArgs(msg));
        }
        /// <summary>
        /// OnErrorOcurred
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnErrorOcurred(ErrorOcurredEventArgs e)
        {
            if (ErrorOcurred != null)
                ErrorOcurred(this, e);
        }

        /// <summary>
        /// OnPropertyChanged
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(PropertyItemChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        internal void OnPropertyChanged(string propertyName,object propertyValue)
        {
            OnPropertyChanged(new PropertyItemChangedEventArgs(propertyName,propertyValue));
        }

		#endregion
         
        #region View
        /// <summary>
        /// GetQueueItemsTable
        /// </summary>
        /// <returns></returns>
        public abstract DataTable GetQueueItemsTable();
        ///// <summary>
        ///// GetQueueItemsTable by priority
        ///// </summary>
        ///// <returns></returns>
        //public abstract DataTable GetQueueItemsTable(Priority p);

        /// <summary>
        /// GetQueueItems
        /// </summary>
        /// <returns></returns>
        public abstract IQueueItem[] GetQueueItems();

        ///// <summary>
        ///// GetQueueItems
        ///// </summary>
        ///// <param name="priority"></param>
        ///// <returns></returns>
        //public abstract IQueueItem[] GetQueueItems(Priority priority);

 
        #endregion

        /// <summary>
        /// FormatMessage
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public string FormatMessage(QueueItem m)
        {
            return System.Web.HttpUtility.UrlDecode(m.Body.ToString(), new System.Text.UTF8Encoding());
        }
        /// <summary>
        /// FormatMessage
        /// </summary>
        /// <param name="m"></param>
        /// <param name="en"></param>
        /// <returns></returns>
        public string FormatMessage(QueueItem m, Encoding en)
        {
            return System.Web.HttpUtility.UrlDecode(m.Body.ToString(), en);
        }
  
    }
}


