using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Nistec.Collections;
using System.Collections.Concurrent;
using Nistec.Serialization;

namespace Nistec.Messaging
{
    
    [Serializable, DebuggerDisplay("Count = {Count}")]
    public class SequenceGQueue : GQueue<Guid>
    {
 
        #region members
        private string queueName;
        //ConcurrentQueue<Ptr> queue;
        public event PtrItemEventHandler EnqueueMessage;
        public event PtrItemEventHandler DequeueMessage;
 
        #endregion

        #region ctor
        /// <summary>
        /// SequenceGQueue Ctor
        /// </summary>
        public SequenceGQueue()
        {
            queueName = Guid.NewGuid().ToString();
            //queue = new ConcurrentQueue<Ptr>();
        }
        /// <summary>
        /// SequenceGQueue Ctor with queue name
        /// </summary>
        /// <param name="name"></param>
        public SequenceGQueue(string name)
        {
            queueName = name;
            //queue = new ConcurrentQueue<Ptr>();
        }

        ~SequenceGQueue()
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
        
        public bool TryPeek(out Guid ptr)
        {
            ptr = base.Peek();
            if (!ptr.IsEmpty())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to remove and return the object at the beginning of the queue.
        /// </summary>
        /// <param name="ptr"></param>
        public bool TryDequeue(out Guid ptr)
        {
            ptr = base.Dequeue();
            if (!ptr.IsEmpty())
            {
                return true;
            }
            return false;
        }
        /*
        /// <summary>
        /// Enqueue IQueueMessage in queue
        /// </summary>
        /// <param name="ptr"></param>
        public new void Enqueue(Ptr ptr)
        {
            //Stopwatch w = Stopwatch.StartNew();
            base.Enqueue(ptr);
            //events-reference
            //OnEnqueueMessage(ptr);

            //w.Stop();
            //Console.WriteLine("SequenceGQueue ElapsedMilliseconds:{0}", w.ElapsedMilliseconds);
        }
        */
        /// <summary>
        /// ReEnqueue IQueueMessage in queue
        /// </summary>
        /// <param name="ptr"></param>
        internal void ReEnqueue(Guid ptr)
        {
            Enqueue(ptr);
        }
        /*
        public void Clear()
        {
            Ptr ptr;
            while (this.TryDequeue(out ptr))
            {
                ptr.Dispose();
                // do nothing
            }
        }
        */
        /*
        //events-reference
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
        */
        #endregion

        

        /// <summary>
        /// Queue items Clone
        /// </summary>
        /// <returns></returns>
        public Guid[] Clone()
        {
            return base.ToArray();
        }

        /// <summary>
        /// Find IQueueMessage
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public Guid Find(Guid itemId)
        {
            Guid[] items = Clone();
            foreach (Guid ptr in items)
            {
                if (ptr == itemId) 
                return ptr;
            }
            return Guid.Empty;
        }
            
    }
}
