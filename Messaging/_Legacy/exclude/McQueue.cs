using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.Messaging;

using Nistec.Threading;
using Nistec.Data.SqlClient;
using Nistec.Data;
using System.Data;
using System.Data.SqlClient;
using Nistec.Runtime;
//using Nistec.Messaging.Firebird;

namespace Nistec.Legacy
{
    
  
    /// <summary>
    /// Provides access to a queue on a McMessage Queuing
    /// </summary>
    [Serializable]
    public class McQueue : McQueueSys, IAsyncQueue,IReceiveCompleted, IDisposable
    {
        #region Members
      
        private PriorityQueue Q;
   
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
        protected virtual void OnMessageArraived(QueueItemEventArgs e)
        {
            if (MessageArraived != null)
                MessageArraived(this, e);
        }
        /// <summary>
        /// OnMessageReceived
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessageReceived(QueueItemEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }

        #endregion

        #region Constructor
        /// <summary>
        /// McQueue Ctor
        /// </summary>
        /// <param name="mqp"></param>
        public McQueue(McQueueProperties mqp)
            : base(mqp)
        {
            Console.WriteLine("Init McQueue " + mqp);

            resetEvent = new ManualResetEvent(false);


            Q = new PriorityQueue(mqp.IsTrans);

            Q.MessageArrived += new QueueItemEventHandler(Q_MessageArrived);
            Q.MessageReceived += new QueueItemEventHandler(Q_MessageReceived);
            Q.MessageTransBegin += new QueueItemEventHandler(Q_MessageTransBegin);
            Q.MessageTransEnd += new QueueItemEventHandler(Q_MessageTransEnd);
            if (recoverable)
            {
                InitRecoverQueue(DefaultIntervalMinuteRecover);
            }
            else
            {
                UNLOCK();
            }

        }
   
        /// <summary>
        /// McQueue Ctor
        /// </summary>
        /// <param name="queueName"></param>
        public McQueue(string queueName)
            : this(new McQueueProperties( queueName, false, CoverMode.None))
        {
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Q != null)
                {
                    resetEvent = null;

                    Q.MessageArrived -= new QueueItemEventHandler(Q_MessageArrived);
                    Q.MessageReceived -= new QueueItemEventHandler(Q_MessageReceived);
                    Q.MessageTransBegin -= new QueueItemEventHandler(Q_MessageTransBegin);
                    Q.MessageTransEnd -= new QueueItemEventHandler(Q_MessageTransEnd);
                }
            }
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }
      
        #endregion

        #region McQueueBase

        #region public methods

        ///// <summary>
        ///// CompletedMessage
        ///// </summary>
        ///// <param name="item"></param>
        ///// <param name="status"></param>
        ///// <param name="result"></param>
        //public override void Completed(IQueueItem item, ItemState status, string result)
        //{
        //    Guid ptr = item.ItemId;
        //    Q.RemoveItem(ptr);
        //    base.Completed(item, status, result);
        //}

        /// <summary>
        /// Abort Transaction
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="hasAttach"></param>
        /// <returns></returns>
        public override void AbortTrans(Guid ItemId, bool hasAttach)
        {
            Q.AbortTrans(ItemId, hasAttach);
            Completed(ItemId, (int)ItemState.Abort, hasAttach);

            //ReEnqueue(item);
        }

        /// <summary>
        /// Commit Transaction
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="hasAttach"></param>
        /// <returns></returns>
        public override void CommitTrans(Guid ItemId, bool hasAttach)
        {
            Q.CommitTrans(ItemId,hasAttach);
            Completed(ItemId, (int)ItemState.Commit, hasAttach);
        }

        ///// <summary>
        ///// TransBegin
        ///// </summary>
        ///// <param name="item"></param>
        //public override void TransBegin(IQueueItem item)
        //{
        //    //Q.TransBegin(item);
        //    //base.TransBegin(item);
        //}

        #endregion

        #region properties

        /// <summary>
        /// Get if Queue Initilaized.
        /// </summary>
        public bool Initilaized
        {
            get { return ISLOCK()==0; }
        }

        /// <summary>
        /// Get Count of items in Queue.
        /// </summary>
        public override int Count
        {
            get { return Q.TotalCount; }
        }

        public bool CanQueue(uint count)
        {
            return (Count + count) < MaxCapacity;
        }
 
        ///// <summary>
        ///// Get or Set the interval (in minute) for auto enqueue hold items
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

        /// <summary>
        /// Get or Set the capacity for auto re enqueue hold items
        /// </summary>
        public virtual int CapacityReEnqueue
        {
            get
            {
                int reCap= MaxCapacity - Count;
                return (reCap < 0)? 0: reCap;

                //return m_Capacity;
            }
            //set
            //{
            //    if (value < 1)
            //    {
            //        throw new ArgumentException("required more then 0");
            //    }
            //    if (m_Capacity != value)
            //    {
            //        m_Capacity = value;
            //        OnPropertyChanged("CapacityReEnqueue", m_Capacity);
            //    }
            //}
        }
 
        /// <summary>
        /// Get a value indicating whether the Queue in EnqueueHoldItems Proccess.
        /// </summary>
        public bool EnqueueHoldItems
        {
            get { return enqueueHoldItems; }
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
        //    minCapacity = min;
        //}


 
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
            //base.ClearQueueCoverItems(type);

            //base.ClearFinalItems();
        }

 
        #endregion

        #region Queue events

        void Q_MessageReceived(object sender, QueueItemEventArgs e)
        {
            if (base.CoverMode > CoverMode.None)
            {
                OnCoverQueueItem(e.Item, IsTrans ? ItemState.Wait : ItemState.Dequeue);
            }

            OnMessageReceived(e);// OnDequeueMessage(e);
        }

        void Q_MessageArrived(object sender, QueueItemEventArgs e)
        {
            if (base.CoverMode > CoverMode.None)
            {
                //Console.WriteLine("OnEnqueueMessage");

                OnCoverQueueItem(e.Item, ItemState.Enqueue);
                //AsyncQueueWorker(e.Item);
            }

            OnMessageArraived(e);// OnEnqueueMessage(e); //normalQ.Dequeue();
        }

        void Q_MessageTransBegin(object sender, QueueItemEventArgs e)
        {
            //TransBegin(e.Item);
            //base.TransBegin(e.Item);
        }
        void Q_MessageTransEnd(object sender, QueueItemEventArgs e)
        {
            if (e.State == ItemState.Wait)
            {
                ReEnqueue(e.Item);
            }
        }

        #endregion

        #region static

        public static readonly McQueues Queues = new McQueues();

        /// <summary>Creates a non-transactional Message Queuing queue at the specified path.</summary>
        /// <returns>A <see cref="T:Nistec.Messaging.McQueue"></see> that represents the new queue.</returns>
        /// <param name="queueName">The path of the queue to create. </param>
        public static McQueue Create(string queueName)
        {
            return Create(queueName, false);
        }

        /// <summary>Creates a transactional or non-transactional Message Queuing queue at the specified path.</summary>
        /// <returns>A <see cref="T:Nistec.Messaging.McQueue"></see> that represents the new queue.</returns>
        /// <param name="isTrans">true to create a transactional queue; false to create a non-transactional queue. </param>
        /// <param name="queueName">The path of the queue to create. </param>
        public static McQueue Create(string queueName, bool isTrans)
        {
            return Queues.Create(queueName, isTrans);//, "b3734452-d268-4bc3-9f8f-fe7a37c1eb8f");
        }

        /// <summary>Creates Message Queuing queue by specified properties.</summary>
        /// <param name="prop">The queue properties. </param>
        /// <returns>A <see cref="T:Nistec.Messaging.McQueue"></see> that represents the new queue.</returns>
        public static McQueue Create(McQueueProperties prop)//string queueName, bool isTrans)
        {
            return Queues.Create(prop);//, "b3734452-d268-4bc3-9f8f-fe7a37c1eb8f");
        }


        /// <summary>Deletes a queue on a Message Queuing server.</summary>
        /// <param name="queueName">The location of the queue to be deleted. </param>
        public static void RemoveQueue(string queueName)
        {
            Queues.Remove(queueName);
        }

        /// <summary>Determines whether a Message Queuing queue exists at the specified path.</summary>
        /// <returns>true if a queue with the specified path exists; otherwise, false.</returns>
        /// <param name="path">The location of the queue to find. </param>
        public static bool Exists(string queueName)
        {
            return Queues.Exists(queueName);
        }


        //static McQueue()
        //{
        //    syncRoot = new object();
        //    hashQueueList=new Dictionary<string, McQueue>();
        //}

        ////private static Hashtable hashQueueList;
        //private static readonly Dictionary<string,McQueue> hashQueueList;
        //private static string dbConnection;
        //private static QueueProvider queueProvider;
        //private static object syncRoot;
        ///// <summary>
        ///// Connection string 
        ///// </summary>
        //public static string Connection
        //{
        //    get { return McQueue.dbConnection; }
        //    set { McQueue.dbConnection = value; }
        //}
        ///// <summary>
        ///// QueueProvider
        ///// </summary>
        //public static QueueProvider QueueProvider
        //{
        //    get { return McQueue.queueProvider; }
        //    set { McQueue.queueProvider = value; }
        //}


        //public static Dictionary<string, McQueue> Queues
        //{
        //    get
        //    {
        //        //if (hashQueueList == null)
        //        //{
        //        //    lock (syncRoot)
        //        //    {
        //        //        Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
        //        //        Thread.MemoryBarrier();
        //        //        hashQueueList = hashtable;
        //        //    }
        //        //}
        //        return hashQueueList;
        //    }
        //}

        ///// <summary>
        ///// GetAllCoverItems
        ///// </summary>
        ///// <returns></returns>
        //public static DataTable GetAllCoverItems()
        //{
        //    using (IQueueCommand cmd = McQueueCommand.Factory(QueueProvider))
        //    {
        //        if (QueueProvider == QueueProvider.SqlServer)
        //        {
        //            cmd.ConnectionString = Connection;
        //        }
        //        return cmd.Execute_DataTable("QueueItems", SQLCMD.SqlSelectAllItems);
        //    }
        //}

        //private static string[] GetQueueList()
        //{
        //    List<string> list = new List<string>();
        //    foreach (object o in hashQueueList.Keys)
        //    {
        //        list.Add(o.ToString());
        //    }
        //    return list.ToArray();
        //}

        //private static DataTable GetQueueCoverTableList()
        //{
        //    using (IQueueCommand cmd = McQueueCommand.Factory(QueueProvider))
        //    {
        //        cmd.ConnectionString = Connection;
        //        return cmd.Execute_DataTable("QueueList", SQLCMD.SqlSelectQueues);
        //    }
        //}

        //private static void AddDbQueue(string queueName, bool isTrans)
        //{
        //    try
        //    {
        //        using (IQueueCommand cmd = McQueueCommand.Factory(QueueProvider))
        //        {
        //            cmd.ConnectionString = Connection;
        //            cmd.ExecuteCmd(string.Format(SQLCMD.SqlNewQueue, queueName, Types.BoolToInt(isTrans)));
        //        }
        //    }
        //    catch { }
        //}

        //private static void RemoveDbQueue(string queueName)
        //{
        //    try
        //    {
        //        using (IQueueCommand cmd = McQueueCommand.Factory(QueueProvider))
        //        {
        //            cmd.ConnectionString = Connection;
        //            cmd.ExecuteCmd(string.Format(SQLCMD.SqlRemoveQueue, queueName));
        //        }
        //    }
        //    catch { }
        //}

        ///// <summary>Creates a non-transactional Message Queuing queue at the specified path.</summary>
        ///// <returns>A <see cref="T:Nistec.Messaging.McQueue"></see> that represents the new queue.</returns>
        ///// <param name="queueName">The path of the queue to create. </param>
        //public static McQueue Create(string queueName)
        //{
        //    return Create(queueName,false);
        //}

        ///// <summary>Creates a transactional or non-transactional Message Queuing queue at the specified path.</summary>
        ///// <returns>A <see cref="T:Nistec.Messaging.McQueue"></see> that represents the new queue.</returns>
        ///// <param name="isTrans">true to create a transactional queue; false to create a non-transactional queue. </param>
        ///// <param name="queueName">The path of the queue to create. </param>
        //public static McQueue Create(string queueName, bool isTrans)
        //{
            
        //    if (queueName == null)
        //    {
        //        throw new ArgumentNullException("queueName");
        //    }
        //    if (queueName.Length == 0)
        //    {
        //        throw new ArgumentException("InvalidParameter", "queueName");
        //    }
        //    if (Queues.ContainsKey(queueName))
        //    {
        //        return (McQueue)Queues[queueName];
        //    }
        //    CoverMode mode= CoverMode.None; 
        //    if (!string.IsNullOrEmpty(Connection) && QueueProvider > QueueProvider.None)
        //    {
        //        mode= CoverMode.ItemsOnly;
        //    }
        //    McQueueProperties prop = new McQueueProperties(queueName );
        //    prop.IsTrans = isTrans;
        //    prop.Provider = QueueProvider;
        //    prop.connectionString = Connection;
        //    prop.CoverMode = mode;
        //    return Create(prop);
        //}

        ///// <summary>Creates Message Queuing queue by specified properties.</summary>
        ///// <param name="prop">The queue properties. </param>
        ///// <returns>A <see cref="T:Nistec.Messaging.McQueue"></see> that represents the new queue.</returns>
        //public static McQueue Create(McQueueProperties prop)//string queueName, bool isTrans)
        //{
        //    prop.IsValid();

        //    if (Queues.ContainsKey(prop.QueueName))
        //    {
        //        if (prop.ReloadOnStart)
        //        {
        //            McQueue q = new McQueue(prop);
        //            Queues[prop.QueueName] = q;
        //            return q;
        //        }
        //        return (McQueue)Queues[prop.QueueName];
        //    }
        //    AddDbQueue(prop.QueueName,prop.IsTrans);
        //    McQueue queue=new McQueue(prop);
        //    Queues[prop.QueueName] = queue;

        //    return queue;
        //}
    

        ///// <summary>Deletes a queue on a Message Queuing server.</summary>
        ///// <param name="queueName">The location of the queue to be deleted. </param>
        //public static void RemoveQueue(string queueName)
        //{
        //    if (queueName == null)
        //    {
        //        throw new ArgumentNullException("queueName");
        //    }
        //    if (queueName.Length == 0)
        //    {
        //        throw new ArgumentException("InvalidParameter", "queueName");
        //    }
        //    McQueue.RemoveDbQueue(queueName);
        //    Queues.Remove(queueName);
            
        //}
        ///// <summary>Determines whether a Message Queuing queue exists at the specified path.</summary>
        ///// <returns>true if a queue with the specified path exists; otherwise, false.</returns>
        ///// <param name="path">The location of the queue to find. </param>
        //public static bool Exists(string queueName)
        //{
        //    if (queueName == null)
        //    {
        //        throw new ArgumentNullException("queueName");
        //    }
        //    if (queueName.Length == 0)
        //    {
        //        throw new ArgumentException("InvalidParameter", "queueName");
        //    }
        //    return Queues.ContainsKey(queueName);
        //}

  
        // internal static bool IsFatalError(int value)
        //{
        //    bool flag = value == 0;
        //    return (((value & -1073741824) != 0x40000000) && !flag);
        //}

        //internal static bool IsMemoryError(int value)
        //{
        //    if ((((value != -1072824294) && (value != -1072824226)) && ((value != -1072824221) && (value != -1072824277))) && ((((value != -1072824286) && (value != -1072824285)) && ((value != -1072824222) && (value != -1072824223))) && ((value != -1072824280) && (value != -1072824289))))
        //    {
        //        return false;
        //    }
        //    return true;
        //}
 
        #endregion

        #region  asyncInvoke
        /// <summary>
        /// DefaultMaxTimeout
        /// </summary>
        public static readonly TimeSpan DefaultMaxTimeout = TimeSpan.FromMilliseconds(4294967295);

        //private Hashtable hashAsyncRequests;
        private AsyncCallback onRequestCompleted;
        private ManualResetEvent resetEvent;
        ///// <summary>
        ///// Receive Item Callback delegate
        ///// </summary>
        ///// <param name="timeout"></param>
        ///// <returns></returns>
        //public delegate IQueueItem ReceiveItemCallback(TimeSpan timeout,object state);
        /// <summary>
        /// Receive Completed event
        /// </summary>
        public event ReceiveCompletedEventHandler ReceiveCompleted;


        //internal Hashtable HashAsyncRequests
        //{
        //    get
        //    {
        //        if (this.hashAsyncRequests == null)
        //        {
        //            lock (syncRoot)
        //            {
        //                if (this.hashAsyncRequests == null)
        //                {
        //                    Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
        //                    Thread.MemoryBarrier();
        //                    this.hashAsyncRequests = hashtable;
        //                }
        //            }
        //        }
        //        return this.hashAsyncRequests;
        //    }
        //}

        /// <summary>
        /// OnReceiveCompleted
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnReceiveCompleted(ReceiveCompletedEventArgs e)
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
            TimeSpan ts = QueueItem.DefaultTimeOut;

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
            return BeginReceive(QueueItem.DefaultTimeOut, state, null);
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
                    this.Completed(item.ItemId, (int)ItemState.Commit, item.HasAttach);
                }
            }
        }

        private void OnRequestCompleted(IAsyncResult asyncResult)
        {
            OnReceiveCompleted(new ReceiveCompletedEventArgs(this, asyncResult));
        }

        //================================================
  
        #endregion
    
        #region public methods

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
        public IQueueItem Peek(Messaging.Priority priority)
        {
            return Q.Peek(priority);
        }

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Peek(Guid ptr)
        {
            return Q.Peek(ptr);
        }

        /// <summary>
        /// Remove Item Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem RemoveItem()
        {

            lock (Q.SyncRoot)
            {
                IQueueItem item = Q.Dequeue();
                if (item == null)
                {
                    return null;
                }
                Console.WriteLine("RemoveItem:{0}", item.ItemId);
                return item;
            }
        }

        public void Clear()
        {

            lock (Q.SyncRoot)
            {
                Q.Clear();
               
                Console.WriteLine("Clear items");
            }
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public override IQueueItem Dequeue()
        {

            if (this.HoldDequeue)
            {
                return null;
            }

            if (m_UseDuration)
            {
                if (ShouldWait())
                {
                    Thread.Sleep(100);
                    return null;
                }
            }

            lock (Q.SyncRoot)
            {
                IQueueItem item = Q.Dequeue();
                if (item == null)
                {
                    return null;
                }
                //if (item.IsTimeOut)//.TimeOut < DateTime.Now)
                //{
                //    Completed(item, ItemState.Abort);//, "Time out expired");
                //    return null;
                //}
                //logger.WriteLoge("Dequeue item:{0}", Nistec.Loggers.Mode.INFO, item.ItemId.ToString());
                Console.WriteLine("Dequeue:{0}", item.ItemId);
                return item;
            }
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public IQueueItem Dequeue(Messaging.Priority priority)
        {
            if (this.HoldDequeue)
            {
                return null;
            }
            if (m_UseDuration)
            {
                if (ShouldWait())
                {
                    Thread.Sleep(100);
                    return null;
                }
            }
            lock (Q.SyncRoot)
            {
                IQueueItem item = Q.Dequeue(priority);

                if (item == null)
                {
                    return null;
                }

                //if (item.IsTimeOut)//.TimeOut < DateTime.Now)
                //{
                //    OnErrorOcurred(new ErrorOcurredEventArgs("Time out expired"));
                //    Completed(item, ItemState.Abort);//, "Time out expired");
                //    return null;
                //}

                return item;
            }
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Dequeue(Guid ptr)
        {
            if (this.HoldDequeue)
            {
                return null;
            }
            lock (Q.SyncRoot)
            {
                IQueueItem item = Q.Dequeue(ptr);
                if (item == null)
                {
                    return null;
                }
                //if (item.IsTimeOut)//.TimeOut < DateTime.Now)
                //{
                //    OnErrorOcurred(new ErrorOcurredEventArgs("Time out expired"));
                //    Completed(item, ItemState.Abort);//, "Time out expired");
                //    return null;
                //}
                //logger.WriteLoge("Dequeue item:{0}", Nistec.Loggers.Mode.INFO, item.ItemId.ToString());
                Console.WriteLine("Dequeue:{0}", item.ItemId);
                return item;
            }

        }

        /// <summary>
        /// Enqueue Message
        /// </summary>
        /// <param name="item"></param>
        public override void Enqueue(IQueueItem item)
        {
            if (base.HoldEnqueue || this.Count > base.MaxCapacity)
            {
                if (CoverMode > Messaging.CoverMode.Memory)
                {
                    OnCoverHoldItem(item, 0, 0);
                }
                goto label_01;
            }
            //logger.WriteLoge("Enqueue item:{0}", Nistec.Loggers.Mode.INFO, item.ItemId.ToString());

            while (ISLOCK() > 0)
            {
                //Monitor.Wait(highQ);
                Thread.Sleep(100);
            }
            //Release the waiting thread.
            //Monitor.Pulse(highQ);
            lock (Q.SyncRoot)
            {
                Q.Enqueue(item);
            }
            Console.WriteLine("Enqueue :" + item.ItemId.ToString());

            label_01:
            Thread.Sleep(m_EnqueueWait);
        }

 
        /// <summary>
        /// ReEnqueueMessage
        /// </summary>
        /// <param name="item"></param>
        public override void ReEnqueue(IQueueItem item)
        {

            //if (CoverMode > CoverMode.None)
            //{
            //    DeleteCoverItem(item);
            //}

            if (base.HoldEnqueue)
            {
                OnCoverHoldItem(item, 0, 0);
                goto label_01;
            }
            if (item.Retry > MaxRetry)
            {

                goto label_01;
            }
            lock (Q.SyncRoot)
            {
                ((QueueItem)item).retry += 1;
                Q.Enqueue(item);
            }
            //logger.WriteLoge("ReEnqueue item:{0}", Nistec.Loggers.Mode.INFO, item.ItemId.ToString());

        label_01:

            //if (IsTrans)
            //{
            //    //TODO:delete from trans list
            //}
            Thread.Sleep(m_EnqueueWait);
        }

 
        #endregion
    
        #region View

        /// <summary>
        /// GetQueueItemsTable
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public override DataTable GetQueueItemsTable()
        {
            //if (Q.TotalCount == 0)
            //    return SQLCMD.QueueItemTableSchema;
            return SQLCMD.GetQueueItemsTable(Q.Clone());

            //DataTable dt = SQLCMD.QueueItemTableSchema;
            ////IQueueItem[] items = Q.Clone();

            //IList<IQueueItem> items = Q.Items;

            //foreach (QueueItem item in items)
            //{
            //    DataRow dr = dt.NewRow();
            //    item.FillDataRow(dr);
            //    dt.Rows.Add(dr);
            //}
            //return dt;
        }

        /// <summary>
        /// GetQueueItems
        /// </summary>
        /// <returns></returns>
        public override IQueueItem[] GetQueueItems()
        {
            if (Q.TotalCount == 0)
                return null;
            return Q.Clone();
        }
/*
        /// <summary>
        /// QueueItemRow
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public virtual DataRow QueueItemRow(Guid itemId)//, int messageId)
        {
            IQueueItem item = Q.Find(itemId);
            if (item == null)
                return null;
            return ((QueueItem)item).ToDataRow();
        }
*/
  
        #endregion

        /// <summary>
        /// ValidateCapacity
        /// </summary>
        public void ValidateCapacity()
        {
            if (HoldItemsCount() > 0)
            {
                if (Q.TotalCount < MinCapacity && !HoldEnqueue)
                {
                    HoldItemsEnqueue(CapacityReEnqueue);
                }
            }
        }

        /// <summary>
        /// SetProperty
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        public void SetProperty(string propertyName, object propertyValue)
        {
            switch (propertyName)
            {
                case "MaxItemsPerSecond":
                    MaxItemsPerSecond = Types.ToInt(propertyValue, MaxItemsPerSecond);
                    break;
                //case "EnqueueWait":
                //    EnqueueWait = Types.ToInt(propertyValue, EnqueueWait);
                //    break;
                //case "DequeueWait":
                //    DequeueWait = Types.ToInt(propertyValue, DequeueWait);
                //    break;
                //case "UseThreadSettings":
                //    UseThreadSettings = Types.ToBool(propertyValue, UseThreadSettings);
                //    break;
                case "SerializeBody":
                    SerializeBody = Types.ToBool(propertyValue, SerializeBody);
                    break;
                //case "HoldInterval":
                //    HoldInterval = Types.ToInt(propertyValue, HoldInterval);
                //    break;
                //case "CapacityReEnqueue":
                //    CapacityReEnqueue = Types.ToInt(propertyValue, CapacityReEnqueue);
                //    break;
                case "DateFormat":
                    DateFormat = Types.NZ(propertyValue, DateFormat);
                    break;
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
                case "CoverMode":
                    m_CoverMode = (Messaging.CoverMode)Types.ParseEnum(typeof(Messaging.CoverMode), propertyValue.ToString(), CoverMode);
                    break;
                //case "Provider":
                //    m_Provider = (QueueProvider)Types.ParseEnum(Provider.GetType(), propertyValue.ToString(), Provider);
                //    break;
                case "Server":
                    m_Server = Types.ToInt(propertyValue, Server);
                    break;
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
                case "MaxItemsPerSecond":
                    return MaxItemsPerSecond;
                case "SerializeBody":
                    return SerializeBody;
                //case "HoldInterval":
                //    return HoldInterval;
                case "CapacityReEnqueue":
                    return CapacityReEnqueue;
                case "DateFormat":
                    return DateFormat;
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
                case "CoverMode":
                    return CoverMode;
                //case "Provider":
                //    return Provider;
                case "Server":
                    return Server;
                default:
                    return null;

            }
        }
 
    }
}


