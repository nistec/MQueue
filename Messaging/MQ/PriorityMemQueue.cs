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

        private ConcurrentDictionary<Ptr, IQueueMessage> QueueItems;


        #endregion

        #region override

        protected override bool TryAdd(Ptr ptr, IQueueMessage item)
        {
            QueueItems[ptr]=item.Copy();
            return true;
        }

        protected override bool TryPeek(Ptr ptr, out IQueueMessage item)
        {

            item = null;
            return QueueItems.TryGetValue(ptr, out item);

            //IQueueMessage copy = null;
            //if (QueueItems.TryGetValue(ptr, out copy))
            //{
            //    item = ((QueueItemStream)copy).Copy();
            //    return true;
            //}
            //item = null;
            //return false;
        }

        protected override bool TryDequeue(Ptr ptr, out IQueueMessage item)
        {
            item = null;
            return QueueItems.TryRemove(ptr, out item);
        }

        protected override IQueueMessage GetFirstItem()
        {
            IQueueMessage item = null;
            try
            {
                if (Count() > 0)
                {
                    //var k= QueueItems.Keys.FirstOrDefault<Guid>();
                    //return Dequeue(k);
 
                    foreach (var g in QueueItems.Keys)
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
                    foreach (var g in QueueItems)
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
            QueueItems.Clear();
        }

        internal void ReloadItemsInternal()
        {
            ReloadItems();
        }
        internal protected override void ReloadItems()
        {
           // QueueItems.Clear();
        }
        protected override int Count()
        {
            return QueueItems.Count;
        }

        public override  bool ItemExists(Ptr ptr)
        {
            return QueueItems.ContainsKey(ptr);
        }

        #endregion

        #region ctor

        public PriorityMemQueue(string name, int consumeIterval)
            : base(name, consumeIterval)
        {
            
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 101;

            QueueItems = new ConcurrentDictionary<Ptr, IQueueMessage>(concurrencyLevel, initialCapacity);
          
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
