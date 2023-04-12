using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Nistec.Collections;
using System.Transactions;


namespace Nistec.Legacy
{

    public sealed class PriorityQueue : IQueueTrans,IDisposable
    {
        #region members
        const int ThreadWait = 10;
        const int TransWait = 1000;
        const int MaxRetry = 3;

        private GenericPtrQueue normalQ;
        private GenericPtrQueue mediumQ;
        private GenericPtrQueue highQ;

        //private Nistec.Collections.GenericList<Guid, IQueueItem> QueueList;
        private Hashtable QueueList;
 
        private bool isTrans;
        private Hashtable hashAsyncTrans;
        private static object syncTrans;

        private object syncRoot;

        #endregion

        #region events

        public event QueueItemEventHandler MessageArrived;
        public event QueueItemEventHandler MessageReceived;
        public event QueueItemEventHandler MessageTransBegin;
        public event QueueItemEventHandler MessageTransEnd;

        private void OnMessageArrived(QueueItemEventArgs e)
        {
            if (MessageArrived != null)
                MessageArrived(this, e);
        }
        private void OnMessageReceived(QueueItemEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }
        private void OnMessageTransBegin(QueueItemEventArgs e)
        {
            if (MessageTransBegin != null)
                MessageTransBegin(this, e);
        }
        private void OnMessageTransEnd(QueueItemEventArgs e)
        {
            if (MessageTransEnd != null)
                MessageTransEnd(this, e);
        }
       

        #endregion

        #region ctor

        public PriorityQueue(bool isTrans)
        {
            syncRoot = new object();
            this.isTrans = isTrans;
            normalQ = new GenericPtrQueue();
            mediumQ = new GenericPtrQueue();
            highQ = new GenericPtrQueue();
            QueueList = new Hashtable(100,0.4F);// GenericList<Guid, IQueueItem>();
            if (isTrans)
            {
                syncTrans = new object();
                CreateHashAsyncTrans();
            }
        }

        public void Dispose()
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
        /// Get SyncRoot
        /// </summary>
        public object SyncRoot { get { return syncRoot; } }

        /// <summary>
        /// Get the total number of items in queue
        /// </summary>
        public int TotalCount
        {
            get { return highQ.SyncCount + mediumQ.SyncCount + normalQ.SyncCount; }
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
                    return highQ.SyncCount;
                case Priority.Medium:
                    return mediumQ.SyncCount;
                case Priority.Normal:
                    return normalQ.SyncCount;
            }
            return 0;
        }
        /// <summary>
        /// Get if the queue is transactional
        /// </summary>
        public bool IsTrans
        {
            get { return isTrans; }
        }
        #endregion

        #region Trans

        #region trans item

        private class TransScop
        {
            private PriorityQueue owner;
            public IQueueItem Item;
            public Guid Ptr;
            public ItemState State = ItemState.Wait;
            TimeSpan timeout;
            int status = 0;
            DateTime start;

            public event EventHandler TransCompleted;

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

            protected void OnTransCompleted(EventArgs e)
            {
                if(TransCompleted!=null)
                    TransCompleted(this, e);
            }

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
                    owner.TransCompleted(Item,State);
                }
            }

        }
        #endregion


        private void CreateHashAsyncTrans()
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

        void TransCompleted(IQueueItem item, ItemState state)
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
            OnMessageTransEnd(new QueueItemEventArgs(item, state));
        }

       

        /// <summary>
        /// Commit Transaction
        /// </summary>
        /// <param name="item"></param>
        /// <param name="hasAttach"></param>
        /// <returns></returns>
        public void CommitTrans(Guid itemId, bool hasAttach)
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
          /// <param name="hasAttach"></param>
      /// <returns></returns>
        public void AbortTrans(Guid itemId, bool hasAttach)
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
                        ((QueueItem)item).DoRetry();
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
            OnMessageTransBegin(new QueueItemEventArgs(item, ItemState.Wait));
        }

        #endregion

        #region AsyncTask

        private delegate void RemoveItemCallback(Guid ptr);


        private void RemoveAsyncWorker(Guid ptr)
        {
            try
            {
                QueueList.Remove(ptr);
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
        private void RemoveAsync(Guid ptr)
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
        public IQueueItem Peek(Guid ptr)
        {

            IQueueItem item = null;

            lock (QueueList.SyncRoot)
            {
                //if (QueueList.ContainsKey(ptr))
                //{
                    try
                    {
                        item = (IQueueItem)QueueList[ptr];
                    }
                    catch
                    {
                        Console.WriteLine("Peek Invalid Item");
                    }
                //}
                //int index = QueueList.IndexOfKey(ptr);
                //if (index > -1)
                //{
                //    item = QueueList.Values[index];
                //}
            }
            if (item == null)
            {
                return null;
            }

            return item;
        }

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Peek(Priority priority)
        {
            Guid ptr = Guid.Empty;
            switch (priority)
            {
                case Priority.Normal:
                    if (normalQ.SyncCount > 0)
                    {
                        ptr = normalQ.Peek();
                    }
                    break;
                case Priority.Medium:
                    if (mediumQ.SyncCount > 0)
                    {
                        ptr = normalQ.Peek();
                    }
                    break;
                case Priority.High:
                    if (highQ.SyncCount > 0)
                    {
                        ptr = highQ.Peek();
                    }
                    break;
            }

            if (ptr == Guid.Empty)
            {
                return null;
            }

            IQueueItem item = Peek(ptr);
            if (item == null)
            {
                return Peek(priority);
            }
            return item;

        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Peek()
        {
            Guid ptr = Guid.Empty;
            if (highQ.SyncCount > 0)
            {
                ptr = highQ.Peek();
            }
            else if (mediumQ.SyncCount > 0)
            {
                ptr = mediumQ.Peek();
            }
            else if (normalQ.SyncCount > 0)
            {
                ptr = normalQ.Peek();
            }
            else
            {
                return null;
            }

            if (ptr == Guid.Empty)
            {
                return null;
            }

            IQueueItem item = Peek(ptr);
            if (item == null)
            {
                return Peek();
            }
            return item;

        }



        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Dequeue(Guid ptr)
        {

            IQueueItem item = null;

            lock (QueueList.SyncRoot)
            {
                //if (QueueList.ContainsKey(ptr))
                //{
                try
                {
                    item = (IQueueItem)QueueList[ptr];
                    if (item != null)
                    {
                        item.Status = ItemState.Dequeue;
                        ((QueueItem)item).SetSentTime();
                        if (isTrans)
                        {
                            TransBegin(item);
                        }
                    }

                    QueueList.Remove(ptr);
                   
                    //RemoveAsync(ptr);
                }
                catch
                {
                 
                    Console.WriteLine("Dequeue Inavlid Item");
                }
                //}

                //int index = QueueList.IndexOfKey(ptr);
                //if (index > -1)
                //{
                //    item = QueueList.Values[index];
                //    QueueList.RemoveAt(index);
                //}
            }
            if (item == null)
            {
                return null;
            }
            //item.Status = ItemState.Dequeue;
            //((QueueItem)item).SetSentTime();

            //if (isTrans)
            //{
            //    TransBegin(item);
            //}

            if (MessageReceived != null)
            {
                OnMessageReceived(new QueueItemEventArgs(item, ItemState.Enqueue));
            }

            return item;
        }
        
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Dequeue(Priority priority)
        {
            Guid ptr = Guid.Empty;
            switch (priority)
            {
                case Priority.Normal:
                    if (normalQ.SyncCount > 0)
                        ptr = normalQ.Dequeue();
                    break;
                case Priority.Medium:
                    if (mediumQ.SyncCount > 0)
                        ptr = mediumQ.Dequeue();
                    break;
                case Priority.High:
                    if (highQ.SyncCount > 0)
                        ptr = highQ.Dequeue();
                    break;
            }

            if (ptr == Guid.Empty)
            {
                return GetFirstItem();
                //return null;
            }

            IQueueItem item = Dequeue(ptr);
            if (item == null)
            {
                return Dequeue(priority);
            }
            return item;

        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Dequeue()
        {
            Guid ptr = Guid.Empty;
            
            if (highQ.SyncCount > 0)
            {
                ptr = highQ.Dequeue();
            }
            else if (mediumQ.SyncCount > 0)
            {
                ptr = mediumQ.Dequeue();
            }
            else if (normalQ.SyncCount > 0)
            {
                ptr = normalQ.Dequeue();
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

            if (ptr == Guid.Empty)
            {
                return GetFirstItem();
                //return null;
            }

            IQueueItem item = Dequeue(ptr);
            if (item == null)
            {
                Thread.Sleep(300);
                return Dequeue();
            }
            return item;

        }

        private IQueueItem GetFirstItem()
        {
            IQueueItem item = null;
            try
            {
                if (QueueList.Count > 0)
                {

                     foreach (object o in QueueList.Keys)
                    {
                        item = Dequeue((Guid)o);
                        if (item != null)
                        {
                            //changed:syncCount:
                            //if (item.Priority == Priority.High)
                            //    highQ.ClearCount();
                            //if (item.Priority == Priority.Medium)
                            //    mediumQ.ClearCount();
                            //if (item.Priority == Priority.Normal)
                            //    normalQ.ClearCount();

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return item;
        }

 
        /// <summary>
        /// Enqueue Message
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(IQueueItem item)
        {
            Guid ptr = item.ItemId;
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
            ((QueueItem)item).arrivedTime = DateTime.Now;
            QueueList[ptr] = item;
            if (MessageArrived != null)
            {
                OnMessageArrived(new QueueItemEventArgs(item, ItemState.Enqueue));
            }
            Thread.Sleep(ThreadWait);
        }

        ///// <summary>
        ///// ReEnqueue IQueueItem in queue
        ///// </summary>
        ///// <param name="item"></param>
        //internal void ReEnqueue(IQueueItem item)
        //{
        //    ((QueueItem) item).Retry++;
        //    Enqueue(item);
        //}

        /// <summary>
        /// Remove Item
        /// </summary>
        /// <param name="item"></param>
       public void RemoveItem(Guid ptr)
        {
            lock (QueueList.SyncRoot)
            {
                QueueList.Remove(ptr);
            }
            if (isTrans)
            {
                lock (hashAsyncTrans)
                {
                    hashAsyncTrans.Remove(ptr);
                }
            }
        }

        #endregion

        #region public methods

        public void Clear()
        {
            QueueList.Clear();
            normalQ.Clear();
            mediumQ.Clear();
            highQ.Clear();
            if (isTrans)
            {
                lock (hashAsyncTrans)
                {
                    hashAsyncTrans.Clear();
                }
            }
        }

        /// <summary>
        /// Reset Queue
        /// </summary>
        public void Reset()
        {
            normalQ.Reset();
            mediumQ.Reset();
            highQ.Reset();

        }

        /// <summary>
        /// Queue items Clone
        /// </summary>
        /// <returns></returns>
        public IQueueItem[] Clone()
        {
            int count =QueueList.Count;
            IQueueItem[] list=new QueueItem[count];
            QueueList.Values.CopyTo(list,0);
            return list;
        }

        /// <summary>
        /// Queue items
        /// </summary>
        /// <returns></returns>
        public ICollection Items
        {
            get { return QueueList.Values; }
        }


        /// <summary>
        /// Find IQueueItem
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public IQueueItem Find(Guid itemId)
        {
            IQueueItem item;
            if (QueueList.ContainsKey(itemId))
            {
                item = (IQueueItem)QueueList[itemId];
                return item.Copy();
            }

            //if (QueueList.TryGetValue(itemId,out item))
            //{
            //    return item.Copy() as IQueueItem;
            //}
            return null;
        }
        /// <summary>
        /// Find IQueueItems
        /// </summary>
        /// <param name="TransactionId"></param>
        /// <returns></returns>
        public IQueueItem[] Find(string TransactionId)
        {
            IQueueItem[] items = Clone();
            List<IQueueItem> qitems = new List<IQueueItem>();
            foreach (IQueueItem item in items)
            {
                if (item.TransactionId == TransactionId)
                    qitems.Add(item);
            }

            return qitems.ToArray();
        }

        /// <summary>
        /// Find IQueueItems
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public IQueueItem[] Find(int messageId)
        {
            IQueueItem[] items = Clone();
            List<IQueueItem> qitems = new List<IQueueItem>();
            foreach (IQueueItem item in items)
            {
                if (item.MessageId == messageId)
                    qitems.Add(item);
            }

            return qitems.ToArray();
        }
        #endregion
    }

}
