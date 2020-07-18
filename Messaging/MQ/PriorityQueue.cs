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

namespace Nistec.Messaging
{

    public abstract class PriorityQueue : /*ITransScop,*/ IDisposable
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

        private GenericPtrQueue normalQ;
        private GenericPtrQueue mediumQ;
        private GenericPtrQueue highQ;

        //private ConcurrentDictionary<Guid, IQueueItem> QueueList;
 
        //private bool isTrans;
        private string host;
        //private Hashtable hashAsyncTrans;
        //private static object syncTrans;

        //public string RootPath { get; set; }
       
        #endregion

        #region events

        public event QueueItemEventHandler MessageArrived;
        public event QueueItemEventHandler MessageReceived;
        public event QueueItemEventHandler TransactionBegin;
        public event QueueItemEventHandler TransactionEnd;
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

        protected virtual void OnTransBegin(QueueItemEventArgs e)
        {
            if (TransactionBegin != null)
                TransactionBegin(this, e);
        }
        protected virtual void OnTransEnd(QueueItemEventArgs e)
        {
            if (TransactionEnd != null)
                TransactionEnd(this, e);
        }

        protected virtual void OnErrorOccured(QueueItemEventArgs e)
        {
            if (ErrorOccured != null)
                ErrorOccured(this, e);
        }

        protected virtual void OnTryAdd(Ptr ptr, IQueueItem item, bool result)
        {
            QLogger.DebugFormat("OnTryAdd {0} item:{1}", result, item.Print());
        }
        protected virtual void OnTryDequeue(Ptr ptr, IQueueItem item, bool result)
        {
            QLogger.DebugFormat("TryDequeue {0} item:{1}", result, item.Print());
        }
        protected virtual void OnTryPeek(Ptr ptr, IQueueItem item, bool result)
        {
            QLogger.DebugFormat("TryPeek {0} item:{1}", result, item.Print());
        }
        #endregion

        #region abstract

        protected abstract bool TryAdd(Ptr ptr, IQueueItem item);

        protected abstract bool TryDequeue(Ptr ptr, out IQueueItem item);

        protected abstract bool TryPeek(Ptr ptr, out IQueueItem item);

        protected abstract int Count();

        protected abstract void ClearItems();

        internal protected abstract void ReloadItems();

        protected abstract IQueueItem GetFirstItem();

        public abstract IEnumerable<IPersistEntity> QueryItems();

        //protected abstract bool TransBegin(Ptr ptr, out IQueueItem item);

        #endregion

        #region ctor

        public PriorityQueue(string host)
        {
            this.host = host;
            //this.isTrans = false;
            normalQ = new GenericPtrQueue();
            mediumQ = new GenericPtrQueue();
            highQ = new GenericPtrQueue();

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

        //    //QueueList = new ConcurrentDictionary<Guid, IQueueItem>(concurrencyLevel, initialCapacity);
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
        public string Host
        {
            get { return host; }
        }
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

        private void AddTransItem(IQueueItem item)
        {
            Guid ptr = item.ItemId;
            m_TransDispatcher.Add( new TransactionItem(item, null));
        }

        public void TransEnded(IQueueItem item, ItemState state)
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
                    IQueueItem item = ti.Item;
                    if (item.Retry < MaxRetry)
                    {
                        ((IQueueItem)item).DoRetry();
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

        private void TransBegin(IQueueItem item)
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
            public IQueueItem Item;
            public Guid Ptr;
            public ItemState State = ItemState.Wait;
            TimeSpan timeout;
            int status = 0;
            DateTime start;

            //public event EventHandler TransCompleted;

            public TransScop(PriorityQueue owner,IQueueItem item)
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

        private void AddTransItem(IQueueItem item)
        {
            Guid ptr = item.ItemId;
            TransScop ti = new TransScop(this,item);
            lock (hashAsyncTrans)
            {
                hashAsyncTrans[ptr] = ti;
                Console.WriteLine("trans count:{0}",hashAsyncTrans.Count);
            }
            
        }

        public void TransEnded(IQueueItem item, ItemState state)
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
            //        ((QueueItem)item).DoRetry();
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
                    IQueueItem item = ti.Item;
                    if (item.Retry < MaxRetry)
                    {
                        ((IQueueItem)item).DoRetry();
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

        private void TransBegin(IQueueItem item)
        {

            if (!isTrans)
                return;
            AddTransItem(item);
            OnTransBegin(new QueueItemEventArgs(item, ItemState.Wait));
        }
        */
        #endregion

        #region AsyncTask

        private delegate void RemoveItemCallback(Ptr ptr);


        private void RemoveAsyncWorker(Ptr ptr)
        {
            try
            {
                IQueueItem item;
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
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Peek(Ptr ptr)
        {
            IQueueItem item = null;
            TryPeek(ptr,out item);
            return item;
        }

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public virtual IQueueItem Peek(Priority priority)
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

            //IQueueItem item = Peek(ptr);
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
        public virtual IQueueItem Peek()
        {
            Ptr ptr = Ptr.Empty;
            if (highQ.TryPeek(out ptr))
            {
               return Peek(ptr);
            }
            else if (mediumQ.TryPeek(out ptr))
            {
                return Peek(ptr);
            }
            else if (normalQ.TryPeek(out ptr))
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

            //IQueueItem item = Peek(ptr);
            //if (item == null)
            //{
            //    return Peek();
            //}
            //return item;

        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="transactional"></param>
        /// <returns></returns>
        private IQueueItem DequeueScop(Ptr ptr, bool transactional = false)
        {

            IQueueItem item = null;

            if (transactional)
            {

                using (TransactionScope scope = TransHelper.GetTransactionScope())
                {
                    //Transaction.Current.TransactionCompleted += new TransactionCompletedEventHandler(Current_TransactionCompleted);

                    TryDequeue(ptr, out item);

                    if (item != null)
                    {

                        ((QueueItem)item).SetState(MessageState.Receiving);
                        //item.Status = ItemState.Dequeue;
                        //((QueueItem)item).SetSentTime();


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
                    ((QueueItem)item).SetState(MessageState.Receiving);
                    //item.Status = ItemState.Dequeue;
                    //((QueueItem)item).SetSentTime();

                    if (MessageReceived != null)
                    {
                        OnMessageReceived(new QueueItemEventArgs(item, MessageState.Receiving));
                    }
                }
            }

            return item;
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual IQueueItem Dequeue(Ptr ptr)
        {

            try
            {
                return DequeueScop(ptr);
            }
            catch (TransactionAbortedException tex)
            {
                QLogger.Exception("PriorityQueue Dequeue error ", tex);
            }
            catch(Exception ex)
            {
                QLogger.Exception("PriorityQueue Dequeue error ", ex);
            }

            return null;
        }
        
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual IQueueItem Dequeue(Priority priority)
        {
            Ptr ptr = Ptr.Empty;
            try
            {
                //using (TransactionScope tran = new TransactionScope())
                //{
                    switch (priority)
                    {
                        case Priority.Normal:
                            if (normalQ.TryDequeue(out ptr))
                                return DequeueScop(ptr);
                            break;
                        case Priority.Medium:
                            if (mediumQ.TryDequeue(out ptr))
                                return DequeueScop(ptr);
                            break;
                        case Priority.High:
                            if (highQ.TryDequeue(out ptr))
                                return DequeueScop(ptr);
                            break;
                    }

                    return null;

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
                //} 

            }
            catch (Exception ex)
            {
                QLogger.Exception("PriorityQueue Dequeue error ", ex);
            }

            return null;
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual bool TryDequeue(out IQueueItem item)
        {
            Ptr ptr = Ptr.Empty;

            try
            {

                //using (TransactionScope tran = new TransactionScope())
                //{

                    if (highQ.TryDequeue(out ptr))
                    {
                        item= DequeueScop(ptr);
                        return item!=null;
                    }
                    else if (mediumQ.TryDequeue(out ptr))
                    {
                        item = DequeueScop(ptr);
                        return item != null;
                    }
                    else if (normalQ.TryDequeue(out ptr))
                    {
                        item = DequeueScop(ptr);
                        return item != null;
                    }
                    else
                    {
                        item = null;
                        return false;
                    }
                //}
            }
            catch (Exception ex)
            {
                QLogger.Exception("PriorityQueue Dequeue error ", ex);
            }

            item = null;
            return false;
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual IQueueItem Dequeue()
        {
            Ptr ptr = Ptr.Empty;

            try
            {

                //using (TransactionScope tran = new TransactionScope())
                //{

                    if (highQ.TryDequeue(out ptr))
                    {
                        return DequeueScop(ptr);
                    }
                    else if (mediumQ.TryDequeue(out ptr))
                    {
                        return DequeueScop(ptr);
                    }
                    else if (normalQ.TryDequeue(out ptr))
                    {
                        return DequeueScop(ptr);
                    }
                    else
                    {
                        return null;
                    }

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
               //}
            }
            catch (Exception ex)
            {
                QLogger.Exception("PriorityQueue Dequeue error ", ex);
            }

            return null;

        }

        //private IQueueItem GetFirstItem()
        //{
        //    IQueueItem item = null;
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
        public virtual IQueueAck Enqueue(IQueueItem item)
        {
            

            //using (TransactionScope tran = new TransactionScope())
            //{
                //((QueueItem)item).SetState(MessageState.Arrived);
                //((QueueItem)item).ArrivedTime = DateTime.Now;

                Ptr ptr = ((QueueItem)item).SetArrivedPtr(Host);
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
        /// Enqueue Message
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal protected void ReEnqueue(IQueueItem item)
        {
            Ptr ptr = new Ptr(item, Host);

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
        public virtual IQueueItem RemoveItem(Ptr ptr)
        {
            IQueueItem item;
            
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
        //public IQueueItem[] Clone()
        //{
        //    int count =QueueList.Count;
        //    IQueueItem[] list = new IQueueItem[count];
        //    QueueList.Values.CopyTo(list,0);
        //    return list;
        //}

        ///// <summary>
        ///// Queue items
        ///// </summary>
        ///// <returns></returns>
        //public ICollection<IQueueItem> Items
        //{
        //    get { return QueueList.Values; }
        //}


        ///// <summary>
        ///// Find IQueueItem
        ///// </summary>
        ///// <param name="itemId"></param>
        ///// <param name="messageId"></param>
        ///// <returns></returns>
        //public IQueueItem Find(Guid itemId)
        //{
        //    IQueueItem item;
        //    if (QueueList.ContainsKey(itemId))
        //    {
        //        item = (IQueueItem)QueueList[itemId];
        //        return item.Copy();
        //    }

        //    //if (QueueList.TryGetValue(itemId,out item))
        //    //{
        //    //    return item.Copy() as IQueueItem;
        //    //}
        //    return null;
        //}
        ///// <summary>
        ///// Find IQueueItems
        ///// </summary>
        ///// <param name="TransactionId"></param>
        ///// <returns></returns>
        //public IQueueItem[] Find(string TransactionId)
        //{
        //    IQueueItem[] items = Clone();
        //    List<IQueueItem> qitems = new List<IQueueItem>();
        //    foreach (IQueueItem item in items)
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
        //public IQueueItem[] Find(int messageId)
        //{
        //    IQueueItem[] items = Clone();
        //    List<IQueueItem> qitems = new List<IQueueItem>();
        //    foreach (IQueueItem item in items)
        //    {
        //        if (item.MessageId == messageId)
        //            qitems.Add(item);
        //    }

        //    return qitems.ToArray();
        //}
        #endregion
    }

}
