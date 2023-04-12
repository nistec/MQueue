using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Nistec.Collections;

namespace Nistec.Legacy
{
    

    [Serializable, /*ComVisible(false), DebuggerTypeProxy(typeof(System_QueueDebugView<>)),*/ DebuggerDisplay("Count = {Count}")]
    public class GenericPtrQueue : Queue<Guid>
    {
 
        #region members
        private string queueName;
        private bool isSynchronized=true;

        public event PtrItemEventHandler EnqueueMessage;
        public event PtrItemEventHandler DequeueMessage;
        //changed:syncCount:int syncCount;

        #endregion

        #region ctor
        /// <summary>
        /// GenericDBQueue Ctor
        /// </summary>
        public GenericPtrQueue()
        {
            //coverable = false;
            queueName = Guid.NewGuid().ToString();
            Init();
        }
        /// <summary>
        /// GenericDBQueue Ctor with queue name
        /// </summary>
        /// <param name="name"></param>
        public GenericPtrQueue(string name)
        {
            //coverable = false;
            queueName = name;
            Init();
        }

        private void Init()
        {
            //FinallItems = new List<IQueueItem>();
            //Nistec.Util.Net.netFramework.NetFram("NetUtils", "CTL");
        }

        ~GenericPtrQueue()
        {
        }

        #endregion

        #region properties

        static readonly object SyncLock = new object();
   
        /// <summary>
        /// Get items count in the queue
        /// </summary>
        public int SyncCount
        {
            get 
            {
                lock (SyncLock)
                {
                    return base.Count; //changed:syncCount:syncCount;
                }
            }
        }
        /// <summary>
        /// Get QueueName
        /// </summary>
        public string QueueName
        {
            get { return queueName ; }
        }

        /// <summary>
        /// Get or Set a value indicating whether access to the ICollection is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized
        {
            get { return isSynchronized; }
            //set { isSynchronized = value; }
        }

        #endregion

        #region AsyncQueueWorker

        private delegate void AsyncQueueItem(Guid item);

        private void DequeueMessageWorker(Guid item)
        {
            OnDequeueMessage(item);
            //OnCoverQueueItem(item, 1, 0, "");
        }

        private void AsyncQueueWorker(Guid item)
        {
            AsyncQueueItem caller = new AsyncQueueItem(DequeueMessageWorker);

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(item, null, null);
            Thread.Sleep(10);

            result.AsyncWaitHandle.WaitOne();

            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
            caller.EndInvoke(result);

        }

        #endregion

        #region override

        //changed:syncCount:
        //internal void ClearCount()
        //{
        //    Interlocked.Exchange(ref syncCount,0);
        //}

        //internal void DecrementCount()
        //{
        //    Interlocked.Decrement(ref syncCount);
        //}

        /// <summary>
        /// Peek IQueueItem from queue
        /// </summary>
        /// <returns></returns>
        public override Guid Peek()
        {
            return base.Peek();
        }

        /// <summary>
        /// Dequeue IQueueItem from queue
        /// </summary>
        /// <returns></returns>
        public override Guid Dequeue()
        {
            Guid item = Guid.Empty;
            lock (this.SyncRoot)
            {
                try
                {
                    item = base.Dequeue();
                    //Interlocked.Decrement(ref syncCount);
                }
                catch
                {
                    return Guid.Empty;
                }
            }
            if (item != Guid.Empty)
            {
                //changed:syncCount: Interlocked.Decrement(ref syncCount);
                OnDequeueMessage(item);
            }
            //changed:syncCount:
            //else if (syncCount > 0)
            //{
            //    Interlocked.Exchange(ref syncCount,0);

            //}
            return item;
        }

        /// <summary>
        /// Enqueue IQueueItem in queue
        /// </summary>
        /// <param name="item"></param>
        public override void Enqueue(Guid item)
        {
            lock (this.SyncRoot)
            {
                base.Enqueue(item);
                //changed:syncCount:Interlocked.Increment(ref syncCount);
            }
                OnEnqueueMessage(item);
        }

        /// <summary>
        /// ReEnqueue IQueueItem in queue
        /// </summary>
        /// <param name="item"></param>
        internal void ReEnqueue(Guid item)
        {
            lock (this.SyncRoot)
            {
                base.Enqueue(item);
                //changed:syncCount: Interlocked.Increment(ref syncCount);
            }
            OnEnqueueMessage(item);
        }


        private void OnDequeueMessage(Guid item)
        {
            if (DequeueMessage != null)
                OnDequeueMessage(new PtrItemEventArgs(item, ItemState.Dequeue));
        }

        private void OnEnqueueMessage(Guid item)
        {
            if (EnqueueMessage != null)
                OnEnqueueMessage( new PtrItemEventArgs(item, ItemState.Enqueue));

        }

        /// <summary>
        /// OnDequeueMessage
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDequeueMessage(PtrItemEventArgs e)
        {
            if (DequeueMessage != null)
                DequeueMessage(this, e);
        }
        /// <summary>
        /// OnEnqueueMessage
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnEnqueueMessage(PtrItemEventArgs e)
        {
            if (EnqueueMessage != null)
                EnqueueMessage(this, e);

        }
 
        #endregion


        public void Reset()
        {
            //changed:syncCount:
            //lock (this.SyncRoot)
            //{
            //    lock (SyncLock)
            //    {
            //        syncCount = base.Count;
            //    }
            //}
        }

        /// <summary>
        /// Queue items Clone
        /// </summary>
        /// <returns></returns>
        public Guid[] Clone()
        {
            Guid[] items = new Guid[Count];
            base.CopyTo(items, 0);
            return items;
        }

        /// <summary>
        /// Find IQueueItem
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public Guid Find(Guid itemId)
        {
            Guid[] items = Clone();
            foreach (Guid item in items)
            {
                if (item == itemId) 
                return item;
            }
            return Guid.Empty;
        }
            
    }



}
