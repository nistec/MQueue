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


namespace Nistec.Messaging
{

    public class TopicPriorityQueue : /*ITransScop,*/ IDisposable
    {
      

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

        protected void OnMessageArrived(QueueItemEventArgs e)
        {
            if (MessageArrived != null)
                MessageArrived(this, e);
        }
        protected void OnMessageReceived(QueueItemEventArgs e)
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

        #endregion

        
        #region ctor

        public TopicPriorityQueue(string host)
        {
            this.host = host;
            //this.isTrans = false;
            normalQ = new GenericPtrQueue();
            mediumQ = new GenericPtrQueue();
            highQ = new GenericPtrQueue();

        }

  
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

        
        /// <summary>
        /// Get the queue host name
        /// </summary>
        public string Host
        {
            get { return host; }
        }
        #endregion

  

        #region Queue methods

      
        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public virtual Ptr Peek(Priority priority)
        {
            Ptr ptr = Ptr.Empty;
            switch (priority)
            {
                case Priority.Normal:
                    if (normalQ.TryPeek(out ptr))
                    {
                        return ptr;
                    }
                    break;
                case Priority.Medium:
                    if (mediumQ.TryPeek(out ptr))
                    {
                        return ptr;
                    }
                    break;
                case Priority.High:
                    if (highQ.TryPeek(out ptr))
                    {
                        return ptr;
                    }
                    break;
            }

            return ptr;
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual Ptr Peek()
        {
            Ptr ptr = Ptr.Empty;
            if (highQ.TryPeek(out ptr))
            {
                return ptr;
            }
            else if (mediumQ.TryPeek(out ptr))
            {
                return ptr;
            }
            else if (normalQ.TryPeek(out ptr))
            {
                return ptr;
            }
            else
            {
                return ptr;
            }
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

                        ((QueueItemStream)item).SetState(MessageState.Receiving);
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
                    ((QueueItemStream)item).SetState(MessageState.Receiving);
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
        public virtual Ptr Dequeue(Priority priority)
        {
            Ptr ptr = Ptr.Empty;
            try
            {
                using (TransactionScope tran = new TransactionScope())
                {
                    switch (priority)
                    {
                        case Priority.Normal:
                            if (normalQ.TryDequeue(out ptr))
                                return ptr;
                            break;
                        case Priority.Medium:
                            if (mediumQ.TryDequeue(out ptr))
                                return ptr;
                            break;
                        case Priority.High:
                            if (highQ.TryDequeue(out ptr))
                                return ptr;
                            break;
                    }

                    return ptr;
                } 

            }
            catch (Exception ex)
            {
                Netlog.Exception("PriorityQueue Dequeue error ", ex);
            }

            return ptr;
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public virtual Ptr Dequeue()
        {
            Ptr ptr = Ptr.Empty;

            try
            {

                using (TransactionScope tran = new TransactionScope())
                {

                    if (highQ.TryDequeue(out ptr))
                    {
                        return ptr;
                    }
                    else if (mediumQ.TryDequeue(out ptr))
                    {
                        return ptr;
                    }
                    else if (normalQ.TryDequeue(out ptr))
                    {
                        return ptr;
                    }
                    else
                    {
                        return ptr;
                    }

                }
            }
            catch (Exception ex)
            {
                Netlog.Exception("PriorityQueue Dequeue error ", ex);
            }

            return ptr;

        }

   
        /// <summary>
        /// Enqueue Message
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual void Enqueue(Ptr ptr, Priority pririty)
        {
            using (TransactionScope tran = new TransactionScope())
            {
                    switch (pririty)
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

                    if (MessageArrived != null)
                    {
                        OnMessageArrived(new QueueItemEventArgs(item, MessageState.Arrived));
                    }
               
            }
        }

  
        #endregion

        #region public methods

        public virtual void Clear()
        {
            normalQ.Clear();
            mediumQ.Clear();
            highQ.Clear();

            OnClear();
        }

        protected virtual void OnClear()
        {

        }

     
        #endregion
    }

}
