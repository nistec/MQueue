using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Nistec.Collections;
using System.Collections.Concurrent;

namespace Nistec.Messaging
{
    

    [Serializable, DebuggerDisplay("Count = {Count}")]
    public class GenericPtrQueue : ConcurrentQueue<Ptr>
    {
 
        #region members
        private string queueName;
        //ConcurrentQueue<Ptr> queue;
        public event PtrItemEventHandler EnqueueMessage;
        public event PtrItemEventHandler DequeueMessage;
 
        #endregion

        #region ctor
        /// <summary>
        /// GenericPtrQueue Ctor
        /// </summary>
        public GenericPtrQueue()
        {
            queueName = Guid.NewGuid().ToString();
            //queue = new ConcurrentQueue<Ptr>();
        }
        /// <summary>
        /// GenericPtrQueue Ctor with queue name
        /// </summary>
        /// <param name="name"></param>
        public GenericPtrQueue(string name)
        {
            queueName = name;
            //queue = new ConcurrentQueue<Ptr>();
        }

        ~GenericPtrQueue()
        {
        }

        #endregion

        #region properties
  
        ///// <summary>
        ///// Get items count in the queue
        ///// </summary>
        //public int SyncCount
        //{
        //    get
        //    {
        //        return base.Count;
        //    }
        //}

        /// <summary>
        /// Get QueueName
        /// </summary>
        public string QueueName
        {
            get { return queueName ; }
        }

        #endregion

        #region AsyncQueueWorker
        /*
        private delegate void AsyncQueueItem(Ptr ptr);

        private void DequeueMessageWorker(Ptr ptr)
        {
            OnDequeueMessage(ptr);
         }

        private void AsyncQueueWorker(Ptr ptr)
        {
            AsyncQueueItem caller = new AsyncQueueItem(DequeueMessageWorker);

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(ptr, null, null);
            Thread.Sleep(10);

            result.AsyncWaitHandle.WaitOne();

            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
            caller.EndInvoke(result);

        }
        */
        #endregion

        #region override

        /// <summary>
        /// Peek IQueueItem from queue
        /// </summary>
        /// <returns></returns>
        public Ptr Peek()
        {
            Ptr g = Ptr.Empty;
            base.TryPeek(out g);
            return g;
        }

        
        /// <summary>
        /// Dequeue IQueueItem from queue
        /// </summary>
        /// <returns></returns>
        public Ptr Dequeue()
        {
            Ptr ptr = Ptr.Empty;

            base.TryDequeue(out ptr);

            if (!ptr.IsEmpty)
            {
                OnDequeueMessage(ptr);
            }
           
            return ptr;
        }

        /// <summary>
        /// Attempts to remove and return the object at the beginning of the queue.
        /// </summary>
        /// <param name="ptr"></param>
        public new bool TryDequeue(out Ptr ptr)
        {
            if (base.TryDequeue(out ptr))
            {
                if (!ptr.IsEmpty)
                {
                    OnDequeueMessage(ptr);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Enqueue IQueueItem in queue
        /// </summary>
        /// <param name="ptr"></param>
        public new void Enqueue(Ptr ptr)
        {
            //Stopwatch w = Stopwatch.StartNew();
            base.Enqueue(ptr);
            OnEnqueueMessage(ptr);
            //w.Stop();
            //Console.WriteLine("GenericPtrQueue ElapsedMilliseconds:{0}", w.ElapsedMilliseconds);
        }

        /// <summary>
        /// ReEnqueue IQueueItem in queue
        /// </summary>
        /// <param name="ptr"></param>
        internal void ReEnqueue(Ptr ptr)
        {
            Enqueue(ptr);
        }

        public void Clear()
        {
            Ptr ptr;
            while (this.TryDequeue(out ptr))
            {
                // do nothing
            }
        }

        private void OnDequeueMessage(Ptr ptr)
        {
            if (DequeueMessage != null)
                OnDequeueMessage(new PtrItemEventArgs(ptr, ItemState.Dequeue));
        }

        private void OnEnqueueMessage(Ptr ptr)
        {
            if (EnqueueMessage != null)
                OnEnqueueMessage( new PtrItemEventArgs(ptr, ItemState.Enqueue));

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

        /// <summary>
        /// Queue items Clone
        /// </summary>
        /// <returns></returns>
        public Ptr[] Clone()
        {
            return base.ToArray();
        }

        /// <summary>
        /// Find IQueueItem
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public Ptr Find(string itemId)
        {
            Ptr[] items = Clone();
            foreach (Ptr ptr in items)
            {
                if (ptr.Identifier == itemId) 
                return ptr;
            }
            return Ptr.Empty;
        }
            
    }
}
