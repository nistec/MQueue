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

namespace Nistec.Messaging
{

    public sealed class PriorityPersistQueue : PriorityQueue
    {
 
        #region members

        PersistentBinary<IQueueItem> m_db;

        #endregion

        #region override

        protected override bool TryAdd(Ptr ptr, IQueueItem item)
        {
            var res = m_db.TryAdd(ptr.Identifier, item.Copy());
            OnTryAdd(ptr, item, res);
            return res;
        }

        protected override bool TryPeek(Ptr ptr, out IQueueItem item)
        {
            var res = m_db.TryGetValue(ptr.Identifier, out item);
            OnTryPeek(ptr, item, res);
            return res;
        }

        protected override bool TryDequeue(Ptr ptr, out IQueueItem item)
        {
            var res = m_db.TryRemove(ptr.Identifier, out item);
            OnTryDequeue(ptr, item, res);
            return res;
        }

        protected override IQueueItem GetFirstItem()
        {
            IQueueItem item = null;
            try
            {
                item = base.Dequeue();
                if (item != null)
                {
                    IQueueItem qi;

                    m_db.TryRemove(item.Identifier, out qi);
                }

            }
            catch (Exception ex)
            {
                Logger.Exception("GetFirstItem", ex);
            }
            return item;
        }

        public override IEnumerable<IPersistEntity> QueryItems()
        {

            try
            {
                if (Count() > 0)
                {
                    var items = m_db.QueryItems("*", null);
                    return items == null ? null : items.Cast<IPersistEntity>();
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("QueryItems", ex);
            }
            //if no items
            return new List<IPersistEntity>();
        }

        protected override void ClearItems()
        {
            m_db.Clear();
        }

        internal void ReloadItemsInternal()
        {
            ReloadItems();
        }
        protected override void ReloadItems()
        {
            m_db.LoadDb();
        }
        protected override int Count()
        {
            return m_db.Count;
        }

        #endregion

        #region ctor

        public PriorityPersistQueue(IQProperties qp)
            : base(qp.QueueName)
        {

            DbLiteSettings settings = new DbLiteSettings()
            {
                Name = qp.QueueName,
                CommitMode = (CommitMode)(int)qp.CommitMode,
                DbPath = AgentManager.Settings.QueuesPath
            };
            //settings.SetFast();
            m_db = new PersistentBinary<IQueueItem>(settings);
            //m_db = new PersistentDictionary(settings);
            m_db.BeginLoading += M_db_BeginLoading;
            m_db.LoadCompleted += M_db_LoadCompleted;
            m_db.ErrorOcurred += M_db_ErrorOcurred;
            m_db.ClearCompleted += M_db_ClearCompleted;
            //m_db.ItemChanged += M_db_ItemChanged;

            m_db.ItemLoaded = (item) => {
                this.ReEnqueue(item);
            };

            if (qp.ReloadOnStart)
                Logger.Info("PriorityPersistQueue will load items to : {0}", qp.QueueName);
            else
                Logger.Info("PriorityPersistQueue will clear all items from : {0}", qp.QueueName);

            m_db.ReloadOrClearPersist(qp.ReloadOnStart);

        }

        private void M_db_ClearCompleted(object sender, EventArgs e)
        {
            Logger.Info("PriorityPersistQueue ClearCompleted : {0}", m_db.Name);
        }

        //private void M_db_ItemChanged(object sender, Generic.GenericEventArgs<string, string, IQueueItem> e)
        //{
        //    QLogger.InfoFormat("PriorityPersistQueue ItemChanged : action- {0}, key- {1}", e.Args1, e.Args2, e.Args3);
        //}

        private void M_db_ErrorOcurred(object sender, Generic.GenericEventArgs<string> e)
        {
            Logger.Error("PriorityPersistQueue ErrorOcurred : {0}", e.Args);
        }

        private void M_db_LoadCompleted(object sender, Generic.GenericEventArgs<string, int> e)
        {
            Logger.Info("PriorityPersistQueue LoadCompleted : {0}, Count:{1}", e.Args1, e.Args2);
        }

        private void M_db_BeginLoading(object sender, EventArgs e)
        {
            Logger.Info("PriorityPersistQueue BeginLoading : {0}", m_db.Name);
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

        //public void Dispose()
        //{
        //    //if (thTrans != null)
        //    //{
        //    //    transKeepAlive = false;
        //    //    thTrans.Abort();
        //    //}

        //}

        #endregion

        #region override events

        protected override void OnErrorOccured(QueueItemEventArgs e)
        {
            base.OnErrorOccured(e);
            Logger.Info("PriorityPersistQueue OnError : Host:{0}, message:{1}", this.Name, e.Message);
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

    }
#if (false)
    public sealed class PriorityComplexQueue2 : PriorityQueue
    {

    #region members

        PersistentBinary<IQueueItem> m_db;
        ConcurrentDictionary<Ptr, IQueueItem> QueueList;
        CommitMode CommitMode = CommitMode.OnMemory;
        CoverMode CoverMode = CoverMode.Memory;
    #endregion

    #region ctor

        public PriorityComplexQueue(IQProperties qp)
            : base(qp.QueueName)
        {

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 101;

            QueueList = new ConcurrentDictionary<Ptr, IQueueItem>(concurrencyLevel, initialCapacity);

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
                m_db = new PersistentBinary<IQueueItem>(settings);
                //m_db = new PersistentDictionary(settings);
                m_db.BeginLoading += M_db_BeginLoading;
                m_db.LoadCompleted += M_db_LoadCompleted;
                m_db.ErrorOcurred += M_db_ErrorOcurred;
                m_db.ClearCompleted += M_db_ClearCompleted;
                //m_db.ItemChanged += M_db_ItemChanged;

                m_db.ItemLoaded = (item) =>
                {
                    this.ReEnqueue(item);
                };

                if (qp.ReloadOnStart)
                    QLogger.InfoFormat("PriorityComplexQueue will load items to : {0}", qp.QueueName);
                else
                    QLogger.InfoFormat("PriorityComplexQueue will clear all items from : {0}", qp.QueueName);

                m_db.ReloadOrClearPersist(qp.ReloadOnStart);
            }
        }

        private void M_db_ClearCompleted(object sender, EventArgs e)
        {
            QLogger.InfoFormat("PriorityComplexQueue ClearCompleted : {0}", m_db.Name);
        }

        //private void M_db_ItemChanged(object sender, Generic.GenericEventArgs<string, string, IQueueItem> e)
        //{
        //    QLogger.InfoFormat("PriorityPersistQueue ItemChanged : action- {0}, key- {1}", e.Args1, e.Args2, e.Args3);
        //}

        private void M_db_ErrorOcurred(object sender, Generic.GenericEventArgs<string> e)
        {
            QLogger.ErrorFormat("PriorityComplexQueue ErrorOcurred : {0}", e.Args);
        }

        private void M_db_LoadCompleted(object sender, Generic.GenericEventArgs<string, int> e)
        {
            QLogger.InfoFormat("PriorityComplexQueue LoadCompleted : {0}, Count:{1}", e.Args1, e.Args2);
        }

        private void M_db_BeginLoading(object sender, EventArgs e)
        {
            QLogger.InfoFormat("PriorityComplexQueue BeginLoading : {0}", m_db.Name);
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
 
    #region override

        protected override bool TryAdd(Ptr ptr, IQueueItem item)
        {
            var copy = item.Copy();
            QueueList[ptr] = copy;

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

        protected override bool TryPeek(Ptr ptr, out IQueueItem item)
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
                else if (QueueList.TryGetValue(ptr, out item))
                {
                    return true;
                }
            }
            else if (QueueList.TryGetValue(ptr, out item))
            {
                return true;
            }


            return false;
        }

        protected override bool TryDequeue(Ptr ptr, out IQueueItem item)
        {

            if (CoverMode == CoverMode.Persistent)
            {
                if (QueueList.TryRemove(ptr, out item))
                {
                    if (CommitMode == CommitMode.OnDisk)
                    {
                        IQueueItem item_pers = null;
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
                if (QueueList.TryRemove(ptr, out item))
                {
                    return true;
                }
            }

            return false;
        }

        protected override IQueueItem GetFirstItem()
        {

            IQueueItem item = null;
            try
            {
                if (CoverMode == CoverMode.Persistent)
                {
                    item = base.Dequeue();
                    if (item != null)
                    {
                        IQueueItem qi;

                        m_db.TryRemove(item.Identifier, out qi);
                    }
                }
                else {
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

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                        foreach (var g in QueueList)
                        {
                            list.Add(new PersistItem() { body = g.Value, key = g.Key.Identifier, name = Host, timestamp = g.Key.ArrivedTime });
                        }
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //if no items
            return new List<IPersistEntity>();

        }

        protected override void ClearItems()
        {
            QueueList.Clear();
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
                m_db.LoadDb();
        }
        protected override int Count()
        {
            if (CoverMode == CoverMode.Persistent)
            {
                if (CommitMode == CommitMode.OnDisk)
                    return m_db.Count;
                else
                    return QueueList.Count;
            }
            else
            {
                return QueueList.Count;
            }
        }

    #endregion

    #region override events

        protected override void OnErrorOccured(QueueItemEventArgs e)
        {
            base.OnErrorOccured(e);
            QLogger.InfoFormat("PriorityPersistQueue OnError : Host:{0}, message:{1}", this.Host, e.Message);
        }

        protected override void OnMessageArrived(QueueItemEventArgs e)
        {
            base.OnMessageArrived(e);
            QLogger.InfoFormat("PriorityPersistQueue OnMessageArrived : Host:{0}, Item:{1}", this.Host, e.Item.Print());
        }

        protected override void OnMessageReceived(QueueItemEventArgs e)
        {
            base.OnMessageReceived(e);
            QLogger.InfoFormat("PriorityPersistQueue OnMessageReceived : Host:{0}, Item:{1}", this.Host, e.Item.Print());
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

    }


    public sealed class PriorityPersistQueue : PriorityQueue
    {

        public override IEnumerable<IPersistEntity> QueryItems()
        {
            
            try
            {
                if (Count() > 0)
                {
                    var items = m_db.QueryItems("*",null);
                    return items == null ? null : items.Cast<IPersistEntity>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //if no items
            return new List<IPersistEntity>();
        }

     
    #region members

        PersistentBinary<IQueueItem> m_db;
        ConcurrentDictionary<Ptr, IQueueItem> QueueList;
        CommitMode CommitMode = CommitMode.OnMemory;
    #endregion

        bool PersistItemRemove(Ptr ptr)
        {

            IQueueItem persistItem = null;

            Task tsk = Task.Factory.StartNew(() =>
                m_db.TryRemove(ptr.Identifier, out persistItem)
            );
            return true;
        }

        bool PersistItemAdd(Ptr ptr, IQueueItem item)
        {

            Task tsk = Task.Factory.StartNew(() =>
                m_db.TryAdd(ptr.Identifier, item)
            );
            return true;
        }

    #region override

        protected override bool TryAdd(Ptr ptr, IQueueItem item)
        {
            var copy= item.Copy();
            QueueList[ptr] = copy;

            if (CommitMode != CommitMode.OnDisk)
            {
                return PersistItemAdd(ptr, copy);
            }
            else
            {
                if (m_db.TryAdd(ptr.Identifier, copy))
                {
                    OnTryAdd(ptr, item, true);
                    return true;
                }
                return false;
            }
        }

        protected override bool TryPeek(Ptr ptr, out IQueueItem item)
        {
            if (QueueList.TryGetValue(ptr, out item))
            {
                if (CommitMode != CommitMode.OnDisk)
                {
                    return true;
                }
                else
                {
                    if (m_db.TryGetValue(ptr.Identifier, out item))
                    {
                        OnTryPeek(ptr, item, true);
                        return true;
                    }
                }
            }
            else
            {
                if (m_db.TryGetValue(ptr.Identifier, out item))
                {
                    OnTryPeek(ptr, item, true);
                    return true;
                }
            }
            return false;
        }

        protected override bool TryDequeue(Ptr ptr, out IQueueItem item)
        {

            if (QueueList.TryRemove(ptr, out item))
            {
                if (CommitMode != CommitMode.OnDisk)
                {
                    return PersistItemRemove(ptr);
                }
                else
                {
                    var res = m_db.TryRemove(ptr.Identifier, out item);
                    OnTryDequeue(ptr, item, res);
                    return res;
                }
            }
            else
            {
                if (m_db.TryRemove(ptr.Identifier, out item))
                {
                    OnTryDequeue(ptr, item, true);
                    return true;
                }
            }
            return false;
        }

        protected override IQueueItem GetFirstItem()
        {
            IQueueItem item = null;
            try
            {
                item = base.Dequeue();
                if (item != null)
                {
                    IQueueItem qi;

                    m_db.TryRemove(item.Identifier, out qi);
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
            m_db.Clear();
        }

        internal void ReloadItemsInternal()
        {
            ReloadItems();
        }
        protected override void ReloadItems()
        {
            m_db.LoadDb();
        }
        protected override int Count()
        {
            if (CommitMode != CommitMode.OnDisk)
            {
                return QueueList.Count;
            }
            else
            {
                return m_db.Count;
            }
        }

    #endregion

    #region ctor

        public PriorityPersistQueue(IQProperties qp)
            : base(qp.QueueName)
        {

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 101;

            QueueList = new ConcurrentDictionary<Ptr, IQueueItem>(concurrencyLevel, initialCapacity);

            CommitMode = (CommitMode)(int)qp.CommitMode;

            DbLiteSettings settings = new DbLiteSettings()
            {
                Name = qp.QueueName,
                CommitMode =(CommitMode) (int)qp.CommitMode,
                DbPath = AgentManager.Settings.QueuesPath
            };
            //settings.SetFast();
            m_db = new PersistentBinary<IQueueItem>(settings);
            //m_db = new PersistentDictionary(settings);
            m_db.BeginLoading += M_db_BeginLoading;
            m_db.LoadCompleted += M_db_LoadCompleted;
            m_db.ErrorOcurred += M_db_ErrorOcurred;
            m_db.ClearCompleted += M_db_ClearCompleted;
            //m_db.ItemChanged += M_db_ItemChanged;

            m_db.ItemLoaded = (item) => {
                this.ReEnqueue(item);
            };

            if (qp.ReloadOnStart)
                QLogger.InfoFormat("PriorityPersistQueue will load items to : {0}", qp.QueueName);
            else
                QLogger.InfoFormat("PriorityPersistQueue will clear all items from : {0}", qp.QueueName);

            m_db.ReloadOrClearPersist(qp.ReloadOnStart);

        }

        private void M_db_ClearCompleted(object sender, EventArgs e)
        {
            QLogger.InfoFormat("PriorityPersistQueue ClearCompleted : {0}", m_db.Name);
        }

        //private void M_db_ItemChanged(object sender, Generic.GenericEventArgs<string, string, IQueueItem> e)
        //{
        //    QLogger.InfoFormat("PriorityPersistQueue ItemChanged : action- {0}, key- {1}", e.Args1, e.Args2, e.Args3);
        //}

        private void M_db_ErrorOcurred(object sender, Generic.GenericEventArgs<string> e)
        {
            QLogger.ErrorFormat("PriorityPersistQueue ErrorOcurred : {0}", e.Args);
        }

        private void M_db_LoadCompleted(object sender, Generic.GenericEventArgs<string, int> e)
        {
            QLogger.InfoFormat("PriorityPersistQueue LoadCompleted : {0}, Count:{1}", e.Args1,e.Args2);
        }

        private void M_db_BeginLoading(object sender, EventArgs e)
        {
            QLogger.InfoFormat("PriorityPersistQueue BeginLoading : {0}", m_db.Name);
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

        //public void Dispose()
        //{
        //    //if (thTrans != null)
        //    //{
        //    //    transKeepAlive = false;
        //    //    thTrans.Abort();
        //    //}

        //}

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

    }
#endif
}
