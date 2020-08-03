using System;
using System.Collections.Generic;
//using System.Text;
//using System.Collections;
//using System.Runtime.InteropServices;
//using System.Diagnostics;
//using System.Threading;
//using Nistec.Collections;
//using System.Transactions;
using System.Collections.Concurrent;
//using System.Linq;
using Nistec.Data.Entities;
using Nistec.Data.Sqlite;

namespace Nistec.Messaging.Controller
{

    public sealed class PriorityMemQueue : PriorityQueue
    {

        public override IEnumerable<IPersistEntity> QueryItems()
        {

            List<IPersistEntity> list = new List<IPersistEntity>();
            try
            {
                if (Count() > 0)
                {
                    foreach (var g in QueueList)
                    {
                        list.Add(new PersistItem() { body = g.Value, key = g.Key.Identifier, name = Host, timestamp = g.Key.ArrivedTime });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }


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

        protected override void ClearItems()
        {
            QueueList.Clear();
        }

        internal void ReloadItemsInternal()
        {
            ReloadItems();
        }
        protected override void ReloadItems()
        {
           // QueueList.Clear();
        }
        protected override int Count()
        {
            return QueueList.Count;
        }


        #endregion

        #region ctor

        public PriorityMemQueue(string host)
            : base(host)
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
            QLogger.InfoFormat("PriorityMemQueue OnError : Host:{0}, message:{1}", this.Host, e.Message);
        }

        protected override void OnMessageArrived(QueueItemEventArgs e)
        {
            base.OnMessageArrived(e);
            QLogger.InfoFormat("PriorityMemQueue OnMessageArrived : Host:{0}, Item:{1}", this.Host, e.Item.Print());
        }

        protected override void OnMessageReceived(QueueItemEventArgs e)
        {
            base.OnMessageReceived(e);
            QLogger.InfoFormat("PriorityMemQueue OnMessageReceived : Host:{0}, Item:{1}", this.Host, e.Item.Print());
        }
       
        #endregion

    }

}
