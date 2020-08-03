using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Nistec.Data.Entities;


namespace Nistec.Messaging
{

    public class PersistEntity :  IPersistEntity
    {
        public string key { get; set; }
        public object body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }

        public object value()
        {
            return body;
        }
    }

    public sealed class PriorityMemQueue : PriorityQueue
    {


        #region members

        private ConcurrentDictionary<Ptr, IQueueItem> QueueList;
 
       
        #endregion

        #region override

        protected override bool TryAdd(Ptr ptr, IQueueItem item)
        {
            QueueList[ptr]=item.Copy();
            return true;
        }

        protected override bool TryPeek(Ptr ptr, out IQueueItem item)
        {

            item = null;
            return QueueList.TryGetValue(ptr, out item);

            //IQueueItem copy = null;
            //if (QueueList.TryGetValue(ptr, out copy))
            //{
            //    item = ((QueueItemStream)copy).Copy();
            //    return true;
            //}
            //item = null;
            //return false;
        }

        protected override bool TryDequeue(Ptr ptr, out IQueueItem item)
        {
            item = null;
            return QueueList.TryRemove(ptr, out item);
        }

        protected override IQueueItem GetFirstItem()
        {
            IQueueItem item = null;
            try
            {
                if (Count() > 0)
                {
                    //var k= QueueList.Keys.FirstOrDefault<Guid>();
                    //return Dequeue(k);
 
                    foreach (var g in QueueList.Keys)
                    {
                        item = Dequeue(g);
                        if (item != null)
                        {
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

        public override IEnumerable<IPersistEntity> QueryItems()
        {

            List<IPersistEntity> list = new List<IPersistEntity>();
            try
            {
                if (Count() > 0)
                {
                    foreach (var g in QueueList)
                    {
                        list.Add(new PersistEntity() { body = g.Value, key = g.Key.Identifier, name = Name, timestamp = g.Key.ArrivedTime });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }

        protected override void ClearItems()
        {
            QueueList.Clear();
        }

        internal void ReloadItemsInternal()
        {
            ReloadItems();
        }
        internal protected override void ReloadItems()
        {
           // QueueList.Clear();
        }
        protected override int Count()
        {
            return QueueList.Count;
        }


        #endregion

        #region ctor

        public PriorityMemQueue(string name)
            : base(name)
        {
            
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 101;

            QueueList = new ConcurrentDictionary<Ptr, IQueueItem>(concurrencyLevel, initialCapacity);
          
        }

        public override void Dispose()
        {
            //if (thTrans != null)
            //{
            //    transKeepAlive = false;
            //    thTrans.Abort();
            //}

        }

        #endregion

        #region override

        protected override void OnErrorOccured(QueueItemEventArgs e)
        {
            base.OnErrorOccured(e);
            QLogger.Info("PriorityMemQueue OnError : Host:{0}, message:{1}", this.Name, e.Message);
        }

        protected override void OnMessageArrived(QueueItemEventArgs e)
        {
            base.OnMessageArrived(e);
            QLogger.Info("PriorityMemQueue OnMessageArrived : Host:{0}, Item:{1}", this.Name, e.Item.Print());
        }

        protected override void OnMessageReceived(QueueItemEventArgs e)
        {
            base.OnMessageReceived(e);
            QLogger.Info("PriorityMemQueue OnMessageReceived : Host:{0}, Item:{1}", this.Name, e.Item.Print());
        }
       
        #endregion

    }

}
