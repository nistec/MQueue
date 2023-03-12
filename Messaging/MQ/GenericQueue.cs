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
    public interface IGenericQueueItem
    {
        string Id { get; }
    }

    [Serializable, DebuggerDisplay("Count = {Count}")]
    public class GenericQueue<T> : ConcurrentQueue<T>
    {
 
        #region members
        private string queueName;
        //ConcurrentQueue<T> queue;
        public event TItemEventHandler<T> EnqueueMessage;
        public event TItemEventHandler<T> DequeueMessage;
 
        #endregion

        #region ctor
        /// <summary>
        /// GenericPtrQueue Ctor
        /// </summary>
        public GenericQueue()
        {
            queueName = Guid.NewGuid().ToString();
            //queue = new ConcurrentQueue<T>();
        }
        /// <summary>
        /// GenericPtrQueue Ctor with queue name
        /// </summary>
        /// <param name="name"></param>
        public GenericQueue(string name)
        {
            queueName = name;
            //queue = new ConcurrentQueue<T>();
        }

        ~GenericQueue()
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
        
        #region override

        /// <summary>
        /// Peek IQueueItem from queue
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            T g = default(T);
            base.TryPeek(out g);
            return g;
        }

        
        /// <summary>
        /// Dequeue IQueueItem from queue
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            T item = default(T);

            base.TryDequeue(out item);

            if (EqualityComparer<T>.Default.Equals(item, default(T)))
            {
                OnDequeueMessage(item);
            }
           
            return item;
        }

        /// <summary>
        /// Attempts to remove and return the object at the beginning of the queue.
        /// </summary>
        /// <param name="item"></param>
        public new bool TryDequeue(out T item)
        {
            if (base.TryDequeue(out item))
            {
                if (EqualityComparer<T>.Default.Equals(item, default(T)))
                {
                    OnDequeueMessage(item);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Enqueue IQueueItem in queue
        /// </summary>
        /// <param name="item"></param>
        public new void Enqueue(T item)
        {
            //Stopwatch w = Stopwatch.StartNew();
            base.Enqueue(item);
            OnEnqueueMessage(item);
            //w.Stop();
            //Console.WriteLine("GenericPtrQueue ElapsedMilliseconds:{0}", w.ElapsedMilliseconds);
        }

        /// <summary>
        /// ReEnqueue IQueueItem in queue
        /// </summary>
        /// <param name="item"></param>
        internal void ReEnqueue(T item)
        {
            Enqueue(item);
        }

        public void Clear()
        {
            T item;
            while (this.TryDequeue(out item))
            {
                // do nothing
            }
        }

        private void OnDequeueMessage(T item)
        {
            if (DequeueMessage != null)
                OnDequeueMessage(new TItemEventArgs<T>(item, ItemState.Dequeue));
        }

        private void OnEnqueueMessage(T item)
        {
            if (EnqueueMessage != null)
                OnEnqueueMessage( new TItemEventArgs<T>(item, ItemState.Enqueue));

        }

        /// <summary>
        /// OnDequeueMessage
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDequeueMessage(TItemEventArgs<T> e)
        {
            if (DequeueMessage != null)
                DequeueMessage(this, e);
        }
        /// <summary>
        /// OnEnqueueMessage
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnEnqueueMessage(TItemEventArgs<T> e)
        {
            if (EnqueueMessage != null)
                EnqueueMessage(this, e);

        }
 
        #endregion

        /// <summary>
        /// Queue items Clone
        /// </summary>
        /// <returns></returns>
        public T[] Clone()
        {
            return base.ToArray();
        }

        /// <summary>
        /// Find IQueueItem
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public T Find<TI>(string itemId) where TI: IGenericQueueItem
        {
            T[] items = Clone();
            foreach (T item in items)
            {
                if (((IGenericQueueItem)item).Id == itemId) 
                return item;
            }
            return default(T);
        }
            
    }
}
