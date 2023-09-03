using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Nistec.Collections;
using System.Transactions;
using System.Collections.Concurrent;
using Nistec.Generic;
using Nistec.Messaging.Transactions;
using Nistec.Logging;
using Nistec.Data.Entities;
using Nistec.Runtime.Advanced;

namespace Nistec.Messaging
{
    public interface IPriorityQueue
    {
 
        #region abstract

        IEnumerable<IPersistEntity> QueryItems();

        #endregion

        #region Properties

        /// <summary>
        /// Get the total number of items in queue
        /// </summary>
        int TotalCount { get; }
       
        /// <summary>
        /// Get the item count in queue by priority
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        int QueueCount(Priority priority);
        
        /// <summary>
        /// Get if the queue is transactional
        /// </summary>
        bool IsTrans { get; }

        /// <summary>
        /// Get the queue host name
        /// </summary>
        string Name { get;}

        #endregion

        #region Queue methods

        /// <summary>
        /// Clear all Message
        /// </summary>
        /// <returns></returns>
        void Clear();


        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        IQueueMessage Peek(Ptr ptr);

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        IQueueMessage Peek(Priority priority);

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        IQueueMessage Peek();
     
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        IQueueMessage Dequeue(Ptr ptr);

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        IQueueMessage Dequeue(Priority priority);

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        bool TryDequeue(out IQueueMessage item);
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        IQueueMessage Dequeue();


        /// <summary>
        /// Consume Message
        /// </summary>
        /// <param name="maxSecondWait"></param>
        /// <returns></returns>
        IQueueMessage Consume(int maxSecondWait);
  
        /// <summary>
        /// Enqueue Message
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        IQueueAck Enqueue(IQueueMessage item);

        /// <summary>
        /// Re Enqueue Message
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        IQueueAck Requeue(IQueueMessage item);

        /// <summary>
        /// Remove Item
        /// </summary>
        /// <param name="ptr"></param>
        IQueueMessage RemoveItem(Ptr ptr);

        bool ItemExists(Ptr ptr);

        #endregion
    }

    public abstract class PriorityQueue : /*ITransScop,*/ IDisposable, IPriorityQueue
    {
        //internal static PriorityQueue Factory(IQProperties prop)
        //{
        //    switch (prop.Mode)
        //    {
        //        //case QueueMode.Transactional:
        //        //    return new PriorityTransQueue(prop.QueueName);
        //        //case CoverMode.Db:
        //        //    Assists.Exception_QueueDbNotSupported();
        //        //    return null;
        //        //case CoverMode.File:
        //        //    return new PriorityFileQueue(prop.QueueName);
        //        case CoverMode.Persistent:
        //            return new PriorityPersistQueue(prop);
        //        case CoverMode.Memory:
        //        default:
        //            return new PriorityMemQueue(prop.QueueName);
        //    }
        //}

        #region members
        const int ThreadWait = 10;
        const int TransWait = 1000;
        const int MaxRetry = 3;

        int ConsumeInterval = 100;
        private GenericPtrQueue normalQ;
        private GenericPtrQueue mediumQ;
        private GenericPtrQueue highQ;

        SyncTimerDispatcher<IQueueMessage> transactionalItems;

       //ConcurrentDictionary<Ptr,IQueueMessage> TransactionalItems;
        //private Dictionary<string,IQueueMessage> holdItems;

        ILogger _Logger;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }
        //private ConcurrentDictionary<Guid, IQueueMessage> QueueList;

        //private bool isTrans;
        //private string host;
        //private Hashtable hashAsyncTrans;
        //private static object syncTrans;

        //public string RootPath { get; set; }

        #endregion

        #region events

        public event QueueItemEventHandler MessageArrived;
        public event QueueItemEventHandler MessageReceived;
        //public event QueueItemEventHandler TransactionBegin;
        //public event QueueItemEventHandler TransactionEnd;
        public event QueueItemEventHandler ErrorOccured;

        protected virtual void OnMessageArrived(QueueItemEventArgs e)
        {
            if (MessageArrived != null)
                MessageArrived(this, e);
        }
        protected virtual void OnMessageReceived(QueueItemEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }

        //protected virtual void OnTransBegin(QueueItemEventArgs e)
        //{
        //    if (TransactionBegin != null)
        //        TransactionBegin(this, e);
        //}
        //protected virtual void OnTransEnd(QueueItemEventArgs e)
        //{
        //    if (TransactionEnd != null)
        //        TransactionEnd(this, e);
        //}

        protected virtual void OnErrorOccured(QueueItemEventArgs e)
        {
            if (ErrorOccured != null)
                ErrorOccured(this, e);
        }

        protected virtual void OnTryAdd(Ptr ptr, IQueueMessage item, bool result)
        {
            Logger.Debug("OnTryAdd {0} item:{1}", result, item.Print());
        }
        protected virtual void OnTryDequeue(Ptr ptr, IQueueMessage item, bool result)
        {
            Logger.Debug("TryDequeue {0} item:{1}", result, item.Print());
        }
        protected virtual void OnTryPeek(Ptr ptr, IQueueMessage item, bool result)
        {
            Logger.Debug("TryPeek {0} item:{1}", result, item.Print());
        }
        #endregion

        #region abstract

        protected abstract bool TryAdd(Ptr ptr, IQueueMessage item);

        protected abstract bool TryDequeue(Ptr ptr, out IQueueMessage item);

        protected abstract bool TryPeek(Ptr ptr, out IQueueMessage item);

        protected abstract int Count();

        protected abstract void ClearItems();

        internal protected abstract void ReloadItems();

        protected abstract IQueueMessage GetFirstItem();

        public abstract IEnumerable<IPersistEntity> QueryItems();

        public abstract bool ItemExists(Ptr ptr);

        //protected abstract bool TransBegin(Ptr ptr, out IQueueMessage item);

        #endregion

        #region ctor

        public PriorityQueue(string name,int consumeInterval=100)
        {
            ConsumeInterval = Math.Max(consumeInterval,10);
            this.Name = name;
            this.isTrans = false;
            normalQ = new GenericPtrQueue();
            mediumQ = new GenericPtrQueue();
            highQ = new GenericPtrQueue();
            TransactionExpiryMinuts = 1;
            //transactionalItems = new SyncTimerDispatcher<TransactionItem>();
            //holdItems = new Dictionary<string, IQueueMessage>();
            _Logger = QLogger.Logger.ILog;
        }

   
        //internal PriorityQueue(string host)
        //{
        //    this.host = host;
        //    //this.isTrans = isTrans;
        //    normalQ = new GenericPtrQueue();
        //    mediumQ = new GenericPtrQueue();
        //    highQ = new GenericPtrQueue();

        //    //if (isTrans)
        //    //{
        //    //    m_TransDispatcher = new TransactionDispatcher();
        //    //    m_TransDispatcher.SyncCompleted += new SyncTimerEventHandler(m_TransDispatcher_SyncCompleted);
        //    //}

        //    //int numProcs = Environment.ProcessorCount;
        //    //int concurrencyLevel = numProcs * 2;
        //    //int initialCapacity= 101;

        //    //QueueList = new ConcurrentDictionary<Guid, IQueueMessage>(concurrencyLevel, initialCapacity);
        //    //if (isTrans)
        //    //{
        //    //    syncTrans = new object();
        //    //    CreateHashAsyncTrans();
        //    //}
        //}

 
        public virtual void Dispose()
        {
            //if (thTrans != null)
            //{
            //    transKeepAlive = false;
            //    thTrans.Abort();
            //}

        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            get { return highQ.IsEmpty && mediumQ.IsEmpty && normalQ.IsEmpty; }
        }

        /// <summary>
        /// Get the total number of items in queue
        /// </summary>
        public int TotalCount
        {
            get { return highQ.Count + mediumQ.Count + normalQ.Count; }
        }
        /// <summary>
        /// Get the item count in queue by priority
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public int QueueCount(Priority priority)
        {
            switch (priority)
            {
                case Priority.High:
                    return highQ.Count;
                case Priority.Medium:
                    return mediumQ.Count;
                case Priority.Normal:
                    return normalQ.Count;
            }
            return 0;
        }

        internal bool isTrans = false;
        /// <summary>
        /// Get if the queue is transactional
        /// </summary>
        public bool IsTrans
        {
            get { return isTrans; }
        }
        /// <summary>
        /// Get the queue host name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Transaction Expiry in Minuts, deafult is 1 minute
        /// </summary>
        public int TransactionExpiryMinuts { get; set; }

        #endregion

        #region Trans
        /*
        TransactionDispatcher m_TransDispatcher;

        internal TransactionDispatcher TransDispatcher
        {
            get { return m_TransDispatcher; }
        }

  
        void m_TransDispatcher_SyncCompleted(object sender, SyncTimerEventArgs e)
        {
            //TODO:
        }

        private void AddTransItem(IQueueMessage item)
        {
            Guid ptr = item.ItemId;
            m_TransDispatcher.Add( new TransactionItem(item, null));
        }

        public void TransEnded(IQueueMessage item, ItemState state)
        {
            m_TransDispatcher.Remove(item.ItemId);
            OnTransEnd(new QueueItemEventArgs(item, state));
        }



        /// <summary>
        /// Commit Transaction
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void CommitTrans(Guid itemId)
        {
            if (!isTrans)
                return;

            var ti = m_TransDispatcher.Get(itemId);
            if (ti != null)
            {
                ti.TransComplete(2);
            }
        }

        /// <summary>
        /// Abort Transaction
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void AbortTrans(Guid itemId)
        {
            if (!isTrans)
                return;

            lock (hashAsyncTrans)
            {
                TransScop ti = (TransScop)hashAsyncTrans[itemId];
                ti.TransComplete(1);
            }
        }
        /// <summary>
        /// Abort Transaction
        /// </summary>
        /// <param name="item"></param>
        /// <param name="hasAttach"></param>
        /// <param name="rollback"></param>
        /// <returns></returns>
        public void AbortTrans(Guid itemId, bool hasAttach, bool rollback)
        {
            if (!isTrans)
                return;

            lock (hashAsyncTrans)
            {
                TransScop ti = (TransScop)hashAsyncTrans[itemId];
                if (rollback)
                {
                    IQueueMessage item = ti.Item;
                    if (item.Retry < MaxRetry)
                    {
                        ((IQueueMessage)item).DoRetry();
                        Enqueue(item);
                    }
                }
                ti.TransComplete(1);
            }
        }


        private void RemoveTrans(Guid ptr)
        {

            lock (hashAsyncTrans)
            {
                hashAsyncTrans.Remove(ptr);
            }

        }

        private void TransBegin(IQueueMessage item)
        {

            if (!isTrans)
                return;
            AddTransItem(item);
            OnTransBegin(new QueueItemEventArgs(item, ItemState.Wait));
        }
        */
        #endregion

        #region Trans old

        #region trans item
        /*
        private class TransScop
        {
            private PriorityQueue owner;
            public IQueueMessage Item;
            public Guid Ptr;
            public ItemState State = ItemState.Wait;
            TimeSpan timeout;
            int status = 0;
            DateTime start;

            //public event EventHandler TransCompleted;

            public TransScop(PriorityQueue owner,IQueueMessage item)
            {
                start = item.ArrivedTime;
                this.owner=owner;
                this.Item = item;
                this.Ptr = item.ItemId;
                this.timeout =TimeSpan.FromSeconds(item.TimeOut);
                StartAsyncTrans();
            }
            public void Dispose()
            {

            }

            public void TransComplete(int status)
            {
                this.status = status;
            }

            //protected void OnTransCompleted(EventArgs e)
            //{
            //    if(TransCompleted!=null)
            //        TransCompleted(this, e);
            //}

            public void StartAsyncTrans()
            {
                Thread thTrans = new Thread(new ThreadStart(TransWorker));
                thTrans.IsBackground = true;
                thTrans.Start();
            }

            private void TransWorker()
            {
                Console.WriteLine("Trans scope:{0}", Ptr);

                try
                {
                    //Create the transaction scope
                    using (TransactionScope scope = new TransactionScope())
                    {
                        //Transaction.Current.TransactionCompleted += new TransactionCompletedEventHandler(Current_TransactionCompleted);

                        while (true)//!to.IsTimeOut)
                        {
                            TimeSpan ts = DateTime.Now.Subtract(start);
                            if (status > 0 || ts > timeout)
                            {
                                //to.Dispose();
                                break;
                            }
                            Thread.Sleep(100);
                        }
                        Console.WriteLine("Complete the transaction scope:{0}", Ptr);

                        if (status == 2)//commit
                        {
                            State = ItemState.Commit;
                        }
                        else if (status == 1) //Abort
                        {
                            State = ItemState.Abort;
                        }
                        else//timeout
                        {
                            State = ItemState.Wait;
                        }
                        scope.Complete();
                    }
                }
                catch (System.Transactions.TransactionException ex)
                {
                    Console.WriteLine(ex);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Cannot complete transaction:{0}",ex.Message);
                    //throw;
                }
                finally
                {
                    //OnTransCompleted(EventArgs.Empty); 
                    owner.TransEnded(Item,State);
                }
            }

        }
        */
        #endregion

        /*
        internal  void CreateHashAsyncTrans()
        {
            if (this.hashAsyncTrans == null)
            {
                lock (syncTrans)
                {
                    if (this.hashAsyncTrans == null)
                    {
                        Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
                        Thread.MemoryBarrier();
                        this.hashAsyncTrans = hashtable;
                    }
                }
            }
        }

        private void AddTransItem(IQueueMessage item)
        {
            Guid ptr = item.ItemId;
            TransScop ti = new TransScop(this,item);
            lock (hashAsyncTrans)
            {
                hashAsyncTrans[ptr] = ti;
                Console.WriteLine("trans count:{0}",hashAsyncTrans.Count);
            }
            
        }

        public void TransEnded(IQueueMessage item, ItemState state)
        {
            lock (hashAsyncTrans)
            {
                hashAsyncTrans.Remove(item.ItemId);
                Console.WriteLine("End trans count:{0}", hashAsyncTrans.Count);
            }
            //if (state == ItemState.Wait)//timeout
            //{
            //    if (item.Retry < MaxRetry)
            //    {
            //        ((QueueMessage)item).DoRetry();
            //        Enqueue(item);
            //    }
            //}
            OnTransEnd(new QueueItemEventArgs(item, state));
        }

       

        /// <summary>
        /// Commit Transaction
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void CommitTrans(Guid itemId)
        {
            if (!isTrans)
                return;

            lock (hashAsyncTrans)
            {
                TransScop ti=(TransScop) hashAsyncTrans[itemId];
                ti.TransComplete(2);
            }
        }

        /// <summary>
        /// Abort Transaction
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void AbortTrans(Guid itemId)
        {
            if (!isTrans)
                return;

            lock (hashAsyncTrans)
            {
                TransScop ti =(TransScop) hashAsyncTrans[itemId];
                ti.TransComplete(1);
            }
        }
        /// <summary>
        /// Abort Transaction
        /// </summary>
        /// <param name="item"></param>
        /// <param name="hasAttach"></param>
        /// <param name="rollback"></param>
        /// <returns></returns>
        public void AbortTrans(Guid itemId, bool hasAttach, bool rollback)
        {
            if (!isTrans)
                return;

            lock (hashAsyncTrans)
            {
                TransScop ti = (TransScop)hashAsyncTrans[itemId];
                if (rollback)
                {
                    IQueueMessage item = ti.Item;
                    if (item.Retry < MaxRetry)
                    {
                        ((IQueueMessage)item).DoRetry();
                        Enqueue(item);
                    }
                }
                ti.TransComplete(1);
            }
        }


        private void RemoveTrans(Guid ptr)
        {

            lock (hashAsyncTrans)
            {
               hashAsyncTrans.Remove(ptr);
            }

        }

        private void TransBegin(IQueueMessage item)
        {

            if (!isTrans)
                return;
            AddTransItem(item);
            OnTransBegin(new QueueItemEventArgs(item, ItemState.Wait));
        }
        */
        #endregion

        #region Transactional

        public SyncTimerDispatcher<IQueueMessage> TransactionalDispatcher
        {
            get
            {
                if(transactionalItems==null)
                {
                    transactionalItems = new SyncTimerDispatcher<IQueueMessage>();
                    transactionalItems.SyncItemCompleted += TransactionalItems_SyncItemCompleted;
                }
                return transactionalItems;
            }
        }

        private void TransactionalItems_SyncItemCompleted(object sender, SyncItemEventArgs<IQueueMessage> e)
        {
            OnTransactionExpired(e);
        }

        protected virtual void OnTransactionExpired(SyncItemEventArgs<IQueueMessage> e)
        {
            //if (this.SyncItemCompleted != null)
            //{
            //    this.SyncItemCompleted(this, e);
            //}
        }

        protected virtual void TransactionAdd(IQueueMessage item, int expirationMinurs = 1)
        {
            if (TransactionalDispatcher.Initialized)
                TransactionalDispatcher.Add(item, expirationMinurs);
        }
        protected virtual bool TransactionCommit(IQueueMessage item)
        {
            return TransactionalDispatcher.Remove(item);
        }

        protected virtual void TransactionAbort(IQueueMessage item)
        {
            OnTransactionExpired(new SyncItemEventArgs<IQueueMessage>(item));
        }


        #endregion

        #region AsyncTask

        private delegate void RemoveItemCallback(Ptr ptr);


        private void RemoveAsyncWorker(Ptr ptr)
        {
            try
            {
                IQueueMessage item;
                TryDequeue(ptr, out item);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// RemoveAsync
        /// </summary>
        /// <returns></returns>
        private void RemoveAsync(Ptr ptr)
        {

            RemoveItemCallback caller = new RemoveItemCallback(RemoveAsyncWorker);

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(ptr, new AsyncCallback(CallbackMethod), caller);
        }

        static void CallbackMethod(IAsyncResult ar)
        {
            // Retrieve the delegate.
            RemoveItemCallback caller = (RemoveItemCallback)ar.AsyncState;

            // Call EndInvoke to retrieve the results.
            caller.EndInvoke(ar);

            //Console.WriteLine("The call executed return value \"{0}\".", returnValue);
        }

        #endregion

        #region Queue methods

        /// <summary>
        /// Clear all Message
        /// </summary>
        /// <returns></returns>
        public virtual void Clear()
        {
            normalQ.Clear();
            mediumQ.Clear();
            highQ.Clear();

            ClearItems();
        }


        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public IQueueMessage Peek(Ptr ptr)
        {
            IQueueMessage item = null;
            TryPeek(ptr,out item);
            return item;
        }

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public virtual IQueueMessage Peek(Priority priority)
        {
            Ptr ptr = Ptr.Empty;
            switch (priority)
            {
                case Priority.Normal:
                    if (normalQ.TryPeek(out ptr))
                    {
                        return Peek(ptr);
                    }
                    break;
                case Priority.Medium:
                    if (mediumQ.TryPeek(out ptr))
                    {
                        return Peek(ptr);
                    }
                    break;
                case Priority.High:
                    if (highQ.TryPeek(out ptr))
                    {
                        return Peek(ptr);
                    }
                    break;
            }

            return null;
            
            //if (ptr.IsEmpty)
            //{
            //    return null;
            //}

            //IQueueMessage item = Peek(ptr);
            //if (item == null)
            //{
            //    return Peek(priority);
            //}
            //return item;

        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual IQueueMessage Peek()
        {
            Ptr ptr = Ptr.Empty;
            if (!highQ.IsEmpty && highQ.TryPeek(out ptr))
            {
               return Peek(ptr);
            }
            else if (!mediumQ.IsEmpty && mediumQ.TryPeek(out ptr))
            {
                return Peek(ptr);
            }
            else if (!normalQ.IsEmpty && normalQ.TryPeek(out ptr))
            {
                return Peek(ptr);
            }
            else
            {
                return null;
            }

            //if (ptr.IsEmpty)
            //{
            //    return null;
            //}

            //IQueueMessage item = Peek(ptr);
            //if (item == null)
            //{
            //    return Peek();
            //}
            //return item;

        }
        /*
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="transactional"></param>
        /// <returns></returns>
        private IQueueMessage DequeueScop(Ptr ptr, bool transactional = false)
        {

            IQueueMessage item = null;

            if (transactional)
            {

                using (TransactionScope scope = TransHelper.GetTransactionScope())
                {
                    //Transaction.Current.TransactionCompleted += new TransactionCompletedEventHandler(Current_TransactionCompleted);

                    TryDequeue(ptr, out item);

                    if (item != null)
                    {

                        ((QueueMessage)item).SetState(MessageState.Receiving);
                        ((QueueMessage)item).QCommand = QueueCmd.Dequeue;
                        //item.Status = ItemState.Dequeue;
                        //((QueueMessage)item).SetSentTime();


                        if (MessageReceived != null)
                        {
                            OnMessageReceived(new QueueItemEventArgs(item, MessageState.Receiving));
                        }
                    }

                    scope.Complete();
                }
            }
            else
            {
                TryDequeue(ptr, out item);

                if (item != null)
                {
                    ((QueueMessage)item).SetState(MessageState.Receiving);
                    ((QueueMessage)item).QCommand = QueueCmd.Dequeue;
                    //item.Status = ItemState.Dequeue;
                    //((QueueMessage)item).SetSentTime();

                    if (MessageReceived != null)
                    {
                        OnMessageReceived(new QueueItemEventArgs(item, MessageState.Receiving));
                    }
                }
            }

            return item;
        }
        */
        private bool DequeueScopEvent(IQueueMessage item)
        {

            if (item != null)
            {
                if (IsTrans)
                {
                    TransactionAdd(item, TransactionExpiryMinuts);
                }
                ((QueueMessage)item).SetState(MessageState.Receiving);
                ((QueueMessage)item).Command = QueueCmd.Dequeue.ToString();
                //item.Status = ItemState.Dequeue;
                //((QueueMessage)item).SetSentTime();

                if (MessageReceived != null)
                {
                    OnMessageReceived(new QueueItemEventArgs(item, MessageState.Receiving));
                }
            }
            return item != null;
        }

       

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual IQueueMessage Dequeue(Ptr ptr)
        {
            IQueueMessage item = null;
            try
            {
                if (TryDequeue(ptr, out item))
                {
                    DequeueScopEvent(item);
                    return item;
                }
                //return DequeueScop(ptr);
            }
            catch (TransactionAbortedException tex)
            {
                Logger.Exception("PriorityQueue Dequeue error ", tex);
            }
            catch (Exception ex)
            {
                Logger.Exception("PriorityQueue Dequeue error ", ex);
            }

            return null;
        }
        
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual IQueueMessage Dequeue(Priority priority)
        {
            Ptr ptr = Ptr.Empty;
            IQueueMessage item = null;
            try
            {
                using (TransactionScope scope = TransHelper.GetTransactionScope())
                {
                    switch (priority)
                    {
                        case Priority.Normal:
                            if (normalQ.TryDequeue(out ptr))
                                TryDequeue(ptr, out item);// item = DequeueScop(ptr);
                            break;
                        case Priority.Medium:
                            if (mediumQ.TryDequeue(out ptr))
                                TryDequeue(ptr, out item); //item = DequeueScop(ptr);
                            break;
                        case Priority.High:
                            if (highQ.TryDequeue(out ptr))
                                TryDequeue(ptr, out item); //item = DequeueScop(ptr);
                            break;
                    }

                    scope.Complete();
                    //return null;

                    //if (ptr.IsEmpty)
                    //{
                    //    return GetFirstItem();
                    //}
                    //else
                    //{
                    //    return DequeueScop(ptr);
                    //}

                    //if (item == null)
                    //{
                    //    item = Dequeue(priority);
                    //}
                }
                DequeueScopEvent(item);
            }
            catch (Exception ex)
            {
                Logger.Exception("PriorityQueue Dequeue error ", ex);
            }

            return item;
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual bool TryDequeue(out IQueueMessage item)
        {
            Ptr ptr = Ptr.Empty;

            try
            {

                using (TransactionScope scope = TransHelper.GetTransactionScope())
                {

                    if (!highQ.IsEmpty && highQ.TryDequeue(out ptr))
                    {
                        TryDequeue(ptr, out item);
                        //item = DequeueScop(ptr);
                        //return item!=null;
                    }
                    else if (!mediumQ.IsEmpty && mediumQ.TryDequeue(out ptr))
                    {
                        TryDequeue(ptr, out item);
                        //item = DequeueScop(ptr);
                        //return item != null;
                    }
                    else if (!normalQ.IsEmpty && normalQ.TryDequeue(out ptr))
                    {
                        TryDequeue(ptr, out item);
                        //item = DequeueScop(ptr);
                        //return item != null;
                    }
                    else
                    {
                        item = null;
                        //return false;
                    }
                    scope.Complete();
                }

                return DequeueScopEvent(item);
            }
            catch (Exception ex)
            {
                Logger.Exception("PriorityQueue Dequeue error ", ex);
            }

            item = null;
            return false;
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual IQueueMessage Dequeue()
        {
            Ptr ptr = Ptr.Empty;
            IQueueMessage item = null;
            try
            {

                using (TransactionScope scope = TransHelper.GetTransactionScope())
                {

                    if (!highQ.IsEmpty && highQ.TryDequeue(out ptr))
                    {
                        TryDequeue(ptr, out item); //item = DequeueScop(ptr);
                    }
                    else if (!mediumQ.IsEmpty && mediumQ.TryDequeue(out ptr))
                    {
                        TryDequeue(ptr, out item); //item = DequeueScop(ptr);
                    }
                    else if (!normalQ.IsEmpty && normalQ.TryDequeue(out ptr))
                    {
                        TryDequeue(ptr, out item); //item = DequeueScop(ptr);
                    }
                    else
                    {
                        item = null;
                    }
                    scope.Complete();
                    //changed:syncCount:
                    //if (ptr == Guid.Empty)
                    //    ptr = mediumQ.Dequeue();
                    //else if (ptr == Guid.Empty)
                    //    ptr = normalQ.Dequeue();

                    //if (ptr.IsEmpty)
                    //{
                    //    return GetFirstItem();
                    //}
                    //else
                    //{
                    //    return DequeueScop(ptr);
                    //}

                    //if (item == null)
                    //{
                    //    Thread.Sleep(300);
                    //    return Dequeue();
                    //}
                }
                DequeueScopEvent(item);
            }
            catch (Exception ex)
            {
                Logger.Exception("PriorityQueue Dequeue error ", ex);
            }

            return item;
        }


        /// <summary>
        /// Consume Message
        /// </summary>
        /// <param name="maxSecondWait"></param>
        /// <returns></returns>
        public virtual IQueueMessage Consume(int maxSecondWait)
        {
            Ptr ptr = Ptr.Empty;
            IQueueMessage item = null;
            DateTime start = DateTime.Now;
            bool wait = true;
            int sycle = 0;
            try
            {
                do
                {
                    if(sycle>0)
                        Thread.Sleep(ConsumeInterval);

                    using (TransactionScope scope = TransHelper.GetTransactionScope())
                    {
                        if (!highQ.IsEmpty && highQ.TryDequeue(out ptr))
                        {
                            TryDequeue(ptr, out item); //item = DequeueScop(ptr);
                        }
                        else if (!mediumQ.IsEmpty && mediumQ.TryDequeue(out ptr))
                        {
                            TryDequeue(ptr, out item); //item = DequeueScop(ptr);
                        }
                        else if (!normalQ.IsEmpty && normalQ.TryDequeue(out ptr))
                        {
                            TryDequeue(ptr, out item); //item = DequeueScop(ptr);
                        }
                        else if (maxSecondWait > 0 && DateTime.Now.Subtract(start).TotalSeconds > maxSecondWait)
                        {
                            wait = false;
                        }
                        //else
                        //{
                        //    Thread.Sleep(ConsumeInterval);
                        //}
                        scope.Complete();
                    }
                    sycle++;
                } while (item == null && wait);

                DequeueScopEvent(item);
            }
            catch (Exception ex)
            {
                Logger.Exception("PriorityQueue Consume error ", ex);
            }

            return item;

        }

        //private IQueueMessage GetFirstItem()
        //{
        //    IQueueMessage item = null;
        //    try
        //    {
        //        if (Count() > 0)
        //        {

        //            foreach (object o in QueueList.Keys)
        //            {
        //                item = Dequeue((Guid)o);
        //                if (item != null)
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    return item;
        //}


        /// <summary>
        /// Enqueue Message
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual IQueueAck Enqueue(IQueueMessage item)
        {
            

            //using (TransactionScope tran = new TransactionScope())
            //{
                //((QueueMessage)item).SetState(MessageState.Arrived);
                //((QueueMessage)item).ArrivedTime = DateTime.Now;

                Ptr ptr = ((QueueMessage)item).SetArrivedPtr(Name);
                //Ptr ptr = new Ptr(item, Host);

                if (TryAdd(ptr, item))
                {
                    switch (item.Priority)
                    {
                        case Priority.High:
                            highQ.Enqueue(ptr);
                            break;
                        case Priority.Medium:
                            mediumQ.Enqueue(ptr);
                            break;
                        default:
                            normalQ.Enqueue(ptr);
                            break;
                    }
                    //tran.Complete();
                    if (MessageArrived != null)
                    {
                        OnMessageArrived(new QueueItemEventArgs(item, MessageState.Arrived));
                    }
                    return new QueueAck(MessageState.Arrived,item);// new Ptr(ptr, PtrState.Arrived);
                }
                
            //}
            //Thread.Sleep(ThreadWait);
            return new QueueAck(MessageState.FailedEnqueue, item);// ptr;
        }


        /// <summary>
        /// ReEnqueue Message
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual IQueueAck Requeue(IQueueMessage item)
        {
            if(item.Retry > MaxRetry)
            {
                return new QueueAck(MessageState.RetryExceeds, item);
            }
            ((QueueMessage)item).DoRetry();

            Ptr ptr = new Ptr(item.Identifier, Name);

            switch (item.Priority)
            {
                case Priority.High:
                    highQ.Enqueue(ptr);
                    break;
                case Priority.Medium:
                    mediumQ.Enqueue(ptr);
                    break;
                default:
                    normalQ.Enqueue(ptr);
                    break;
            }

            if (!ItemExists(ptr))
            {
                if(!TryAdd(ptr, item))
                {
                    return new QueueAck(MessageState.FailedEnqueue, item);
                }
            }
            return  new QueueAck(MessageState.Received, item);

            //if (MessageArrived != null)
            //{
            //    OnMessageArrived(new QueueItemEventArgs(item, MessageState.Arrived));
            //}
            //return new QueueAck(MessageState.Arrived, item);// new Ptr(ptr, PtrState.Arrived);
        }


        /// <summary>
        /// Remove Item
        /// </summary>
        /// <param name="ptr"></param>
        public virtual IQueueMessage RemoveItem(Ptr ptr)
        {
            IQueueMessage item;
            
            TryDequeue(ptr, out item);

            return item;


            //if (isTrans)
            //{
            //    lock (hashAsyncTrans)
            //    {
            //        hashAsyncTrans.Remove(ptr);
            //    }
            //}
        }

        #endregion

        #region hold Queue

        /*
        /// <summary>
        /// Enqueue Message to hold queue
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual IQueueAck HoldAdd(IQueueMessage item)
        {

            var ptr = item.GetPtr();
            //holdItems[]
            if (TryAdd(ptr, item))
            {

              //  holdQ.Enqueue(ptr);

                //tran.Complete();
                if (MessageArrived != null)
                {
                    OnMessageArrived(new QueueItemEventArgs(item, MessageState.Holded));
                }
                return new QueueAck(MessageState.Holded, item);// new Ptr(ptr, PtrState.Arrived);
            }

            //}
            //Thread.Sleep(ThreadWait);
            return new QueueAck(MessageState.FailedHoldEnqueue, item);// ptr;
        }

        /// <summary>
        /// ReEnqueue Message from hold queue to normalQ
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="transactional"></param>
        /// <returns></returns>
        public virtual int HoldReEnqueue()
        {
            int count = 0;
            bool isEmpty = false;
            //while (!isEmpty)
            //{
            //    var ptr = holdQ.Dequeue();
            //    if (ptr.IsEmpty)
            //        isEmpty = true;
            //    else
            //    {
            //        normalQ.Enqueue(ptr);
            //        count++;
            //    }
            //}
            
            return count;
        }
        #endregion

        #region public methods

        public virtual void Clear()
        {
            ClearItems();
            normalQ.Clear();
            mediumQ.Clear();
            highQ.Clear();

            OnClear();
            //if (isTrans)
            //{
            //    lock (hashAsyncTrans)
            //    {
            //        hashAsyncTrans.Clear();
            //    }
            //}
        }

        protected virtual void OnClear()
        {

        }

        ///// <summary>
        ///// Reset Queue
        ///// </summary>
        //public void Reset()
        //{
        //    normalQ.Reset();
        //    mediumQ.Reset();
        //    highQ.Reset();

        //}

        ///// <summary>
        ///// Queue items Clone
        ///// </summary>
        ///// <returns></returns>
        //public IQueueMessage[] Clone()
        //{
        //    int count =QueueList.Count;
        //    IQueueMessage[] list = new IQueueMessage[count];
        //    QueueList.Values.CopyTo(list,0);
        //    return list;
        //}

        ///// <summary>
        ///// Queue items
        ///// </summary>
        ///// <returns></returns>
        //public ICollection<IQueueMessage> Items
        //{
        //    get { return QueueList.Values; }
        //}


        ///// <summary>
        ///// Find IQueueMessage
        ///// </summary>
        ///// <param name="itemId"></param>
        ///// <param name="messageId"></param>
        ///// <returns></returns>
        //public IQueueMessage Find(Guid itemId)
        //{
        //    IQueueMessage item;
        //    if (QueueList.ContainsKey(itemId))
        //    {
        //        item = (IQueueMessage)QueueList[itemId];
        //        return item.Copy();
        //    }

        //    //if (QueueList.TryGetValue(itemId,out item))
        //    //{
        //    //    return item.Copy() as IQueueMessage;
        //    //}
        //    return null;
        //}
        ///// <summary>
        ///// Find IQueueItems
        ///// </summary>
        ///// <param name="TransactionId"></param>
        ///// <returns></returns>
        //public IQueueMessage[] Find(string TransactionId)
        //{
        //    IQueueMessage[] items = Clone();
        //    List<IQueueMessage> qitems = new List<IQueueMessage>();
        //    foreach (IQueueMessage item in items)
        //    {
        //        if (item.TransactionId == TransactionId)
        //            qitems.Add(item);
        //    }

        //    return qitems.ToArray();
        //}

        ///// <summary>
        ///// Find IQueueItems
        ///// </summary>
        ///// <param name="messageId"></param>
        ///// <returns></returns>
        //public IQueueMessage[] Find(int messageId)
        //{
        //    IQueueMessage[] items = Clone();
        //    List<IQueueMessage> qitems = new List<IQueueMessage>();
        //    foreach (IQueueMessage item in items)
        //    {
        //        if (item.MessageId == messageId)
        //            qitems.Add(item);
        //    }

        //    return qitems.ToArray();
        //}
        */
        #endregion
    }

}
