using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using Nistec.Collections;
using System.Transactions;
using System.Collections.Concurrent;
using Nistec.Messaging.Db;
using Nistec.IO;
using Nistec.Data.Sqlite;
using Nistec.Messaging.Server;
using Nistec.Messaging.Config;
using Nistec.Data.Entities;
using System.Threading.Tasks;
using Nistec.Logging;
using Nistec.Data.Persistance;

namespace Nistec.Messaging
{

    public sealed class PriorityComplexQueue : PriorityQueue
    {

        #region members

        PersistentBinary<IQueueMessage> m_db;
        ConcurrentDictionary<Ptr, IQueueMessage> QueueItems;
        CommitMode CommitMode = CommitMode.OnMemory;
        CoverMode CoverMode = CoverMode.Memory;
        #endregion

        #region ctor

        public PriorityComplexQueue(IQProperties qp)
            : base(qp.QueueName, qp.ConsumeInterval)
        {

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 101;

            QueueItems = new ConcurrentDictionary<Ptr, IQueueMessage>(concurrencyLevel, initialCapacity);

            CommitMode = (CommitMode)(int)qp.CommitMode;
            CoverMode = qp.Mode;

            if (CoverMode == CoverMode.Persistent)
            {

                DbLiteSettings settings = new DbLiteSettings()
                {
                    Name = qp.QueueName,
                    CommitMode = (CommitMode)(int)qp.CommitMode,
                    DbPath = AgentManager.Settings.QueuesPath
                };
                //settings.SetFast();
                m_db = new PersistentBinary<IQueueMessage>(settings);
                //m_db = new PersistentDictionary(settings);
                m_db.BeginLoading += M_db_BeginLoading;
                m_db.LoadCompleted += M_db_LoadCompleted;
                m_db.ErrorOcurred += M_db_ErrorOcurred;
                m_db.ClearCompleted += M_db_ClearCompleted;
                //m_db.ItemChanged += M_db_ItemChanged;

                m_db.ItemLoaded = (item) =>
                {
                    this.Requeue(item);
                };

                if (qp.ReloadOnStart)
                    Logger.Info("PriorityComplexQueue will load items to : {0}", qp.QueueName);
                else
                    Logger.Info("PriorityComplexQueue will clear all items from : {0}", qp.QueueName);

                m_db.ReloadOrClearPersist(qp.ReloadOnStart);
            }
        }

        private void M_db_ClearCompleted(object sender, EventArgs e)
        {
            Logger.Info("PriorityComplexQueue ClearCompleted : {0}", m_db.Name);
        }

        //private void M_db_ItemChanged(object sender, Generic.GenericEventArgs<string, string, IQueueMessage> e)
        //{
        //    QLogger.InfoFormat("PriorityPersistQueue ItemChanged : action- {0}, key- {1}", e.Args1, e.Args2, e.Args3);
        //}

        private void M_db_ErrorOcurred(object sender, Generic.GenericEventArgs<string> e)
        {
            QLogger.Error("PriorityComplexQueue ErrorOcurred : {0}", e.Args);
        }

        private void M_db_LoadCompleted(object sender, Generic.GenericEventArgs<string, int> e)
        {
            QLogger.Info("PriorityComplexQueue LoadCompleted : {0}, Count:{1}", e.Args1, e.Args2);
        }

        private void M_db_BeginLoading(object sender, EventArgs e)
        {
            QLogger.Info("PriorityComplexQueue BeginLoading : {0}", m_db.Name);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            if (m_db != null)
            {
                //m_db.Dispose();
                m_db = null;
            }
        }

        #endregion

        #region Persist Tasks

        bool PersistItemRemove(Ptr ptr)
        {

            IQueueMessage persistItem = null;

            Task tsk = Task.Factory.StartNew(() =>
                m_db.TryRemove(ptr.Identifier, out persistItem)
            );
            return true;
        }

        bool PersistItemAdd(Ptr ptr, IQueueMessage item)
        {

            Task tsk = Task.Factory.StartNew(() =>
                m_db.TryAdd(ptr.Identifier, item)
            );
            return true;
        }
        #endregion

        #region override

        protected override bool TryAdd(Ptr ptr, IQueueMessage item)
        {
            var copy = item.Copy();
            QueueItems[ptr] = copy;

            if (CoverMode == CoverMode.Persistent)
            {
                if (CommitMode == CommitMode.OnDisk)
                {
                    if (m_db.TryAdd(ptr.Identifier, copy))
                    {
                        OnTryAdd(ptr, item, true);
                        return true;
                    }
                }
                else //if (CommitMode == CommitMode.OnMemory)
                {
                    return PersistItemAdd(ptr, copy);
                }
                return false;
            }
            else //if (CommitMode == CommitMode.OnMemory)
            {
                return true;
            }

        }

        protected override bool TryPeek(Ptr ptr, out IQueueMessage item)
        {
            if (CoverMode == CoverMode.Persistent)
            {

                if (CommitMode == CommitMode.OnDisk)
                {
                    if (m_db.TryGetValue(ptr.Identifier, out item))
                    {
                        OnTryPeek(ptr, item, true);
                        return true;
                    }
                }
                else if (QueueItems.TryGetValue(ptr, out item))
                {
                    return true;
                }
            }
            else if (QueueItems.TryGetValue(ptr, out item))
            {
                return true;
            }


            return false;
        }

        protected override bool TryDequeue(Ptr ptr, out IQueueMessage item)
        {

            if (CoverMode == CoverMode.Persistent)
            {
                if (QueueItems.TryRemove(ptr, out item))
                {
                    if (CommitMode == CommitMode.OnDisk)
                    {
                        IQueueMessage item_pers = null;
                        if (m_db.TryRemove(ptr.Identifier, out item_pers))
                        {
                            OnTryDequeue(ptr, item, true);
                            return true;
                        }
                    }
                    else
                    {
                        return PersistItemRemove(ptr);
                    }
                }
            }
            else
            {
                if (QueueItems.TryRemove(ptr, out item))
                {
                    return true;
                }
            }

            return false;
        }

        protected override IQueueMessage GetFirstItem()
        {

            IQueueMessage item = null;
            try
            {
                if (CoverMode == CoverMode.Persistent)
                {
                    item = base.Dequeue();
                    if (item != null)
                    {
                        IQueueMessage qi;

                        m_db.TryRemove(item.Identifier, out qi);
                    }
                }
                else {
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

            }
            catch (Exception ex)
            {
                Logger.Exception("GetFirstItem ", ex);
            }

            return item;
        }

        public override IEnumerable<IPersistEntity> QueryItems()
        {
            try
            {
                if (Count() > 0)
                {
                    if (CoverMode == CoverMode.Persistent)
                    {
                        var items = m_db.QueryItems("*", null);
                        return items == null ? null : items.Cast<IPersistEntity>();
                    }
                    else
                    {
                        List<IPersistEntity> list = new List<IPersistEntity>();
                        foreach (var g in QueueItems)
                        {
                            list.Add(new PersistItem() { body = g.Value, key = g.Key.Identifier, name = Name, timestamp = g.Key.ArrivedTime });
                        }
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("QueryItems ", ex);
            }
            //if no items
            return new List<IPersistEntity>();

        }

        protected override void ClearItems()
        {
            QueueItems.Clear();
            if (CoverMode == CoverMode.Persistent)
                m_db.Clear();
        }

        internal void ReloadItemsInternal()
        {
            ReloadItems();
        }
        protected override void ReloadItems()
        {
            if (CoverMode == CoverMode.Persistent)
                m_db.LoadDbAsync();
        }
        protected override int Count()
        {
            if (CoverMode == CoverMode.Persistent)
            {
                if (CommitMode == CommitMode.OnDisk)
                    return m_db.Count;
                else
                    return QueueItems.Count;
            }
            else
            {
                return QueueItems.Count;
            }
        }

        public override bool ItemExists(Ptr ptr)
        {
            try
            {
                if (QueueItems.ContainsKey(ptr))
                {
                   // return m_db.ContainsKey(ptr.Identifier);

                    return m_db.SelectValue(ptr.Identifier)!= null;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("ItemExists error: " + ex.Message);
                Logger.Error("PriorityComplexQueue ItemExists : Host:{0}, message:{1}", this.Name, ex.Message);
            }
            return false;
        }

        ///// <summary>
        ///// Enqueue Message
        ///// </summary>
        ///// <param name="item"></param>
        ///// <returns></returns>
        //protected override void Requeue(IQueueMessage item)
        //{
        //    base.Requeue(item);

        //    QueueItems[item.GetPtr()] = item;

        //    //if (MessageArrived != null)
        //    //{
        //    //    OnMessageArrived(new QueueItemEventArgs(item, MessageState.Arrived));
        //    //}
        //    //return new QueueAck(MessageState.Arrived, item);// new Ptr(ptr, PtrState.Arrived);
        //}

        #endregion

        #region override events

        protected override void OnErrorOccured(QueueItemEventArgs e)
        {
            base.OnErrorOccured(e);
            Logger.Error("PriorityPersistQueue OnError : Host:{0}, message:{1}", this.Name, e.Message);
        }

        protected override void OnMessageArrived(QueueItemEventArgs e)
        {
            base.OnMessageArrived(e);
            Logger.Info("PriorityPersistQueue OnMessageArrived : Host:{0}, Item:{1}", this.Name, e.Item.Print());
        }

        protected override void OnMessageReceived(QueueItemEventArgs e)
        {
            base.OnMessageReceived(e);
            Logger.Info("PriorityPersistQueue OnMessageReceived : Host:{0}, Item:{1}", this.Name, e.Item.Print());
        }

        #endregion

        #region override trans

        //protected override void OnTransBegin(QueueItemEventArgs e)
        //{
        //    //e.Item.

        //    base.OnTransBegin(e);
        //}

        //protected override void OnTransEnd(QueueItemEventArgs e)
        //{
        //    base.OnTransEnd(e);
        //}

        #endregion

        #region internal

        internal IEnumerable<IQueueMessage> GetAllItems()
        {
            return QueueItems.Values;
        }

        #endregion

    }

}
