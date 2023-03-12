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

namespace Nistec.Messaging
{

    public class PersistQueue
    {

        public IEnumerable<IPersistEntity> QueryItems()
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
                Console.WriteLine(ex.Message);
            }
            //if no items
            return new List<IPersistEntity>();
        }


        #region members

        PersistentBinary<IQueueItem> m_db;

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

        protected bool TryAdd(Ptr ptr, IQueueItem item)
        {
            var copy = item.Copy();

            if (m_db.TryAdd(ptr.Identifier, copy))
            {
                OnTryAdd(ptr, item, true);
                return true;
            }
            return false;
        }

        protected bool TryPeek(Ptr ptr, out IQueueItem item)
        {

            if (m_db.TryGetValue(ptr.Identifier, out item))
            {
                OnTryPeek(ptr, item, true);
                return true;
            }
            return false;
        }

        protected bool TryDequeue(Ptr ptr, out IQueueItem item)
        {

            if (m_db.TryRemove(ptr.Identifier, out item))
            {
                OnTryDequeue(ptr, item, true);
                return true;
            }
            return false;
        }

        protected override IQueueItem GetFirstItem()
        {
            IQueueItem item = null;
            try
            {
                item = Dequeue();
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

        protected IQueueItem GetFirstItem()
        {
            IQueueItem item = null;
            try
            {
                if(TryDequeue()
                item = base.Dequeue();
                if (item != null)
                {
                    IQueueItem qi;

                    m_db.GetOrAdd(item.Identifier, out qi);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return item;
        }

        protected  void ClearItems()
        {
            m_db.Clear();
        }

        internal void ReloadItemsInternal()
        {
            ReloadItems();
        }
        protected void ReloadItems()
        {
            m_db.LoadDb();
        }
        protected int Count()
        {
                 return m_db.Count;
        }

        protected virtual void OnTryAdd(Ptr ptr, IQueueItem item, bool result)
        {
            QLogger.DebugFormat("OnTryAdd {0} item:{1}", result, item.Print());
        }
        protected virtual void OnTryDequeue(Ptr ptr, IQueueItem item, bool result)
        {
            QLogger.DebugFormat("TryDequeue {0} item:{1}", result, item.Print());
        }
        protected virtual void OnTryPeek(Ptr ptr, IQueueItem item, bool result)
        {
            QLogger.DebugFormat("TryPeek {0} item:{1}", result, item.Print());
        }
        #endregion

        #region ctor

        public PersistQueue(IQProperties qp)
        {

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 101;

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
                QLogger.InfoFormat("PersistQueue will load items to : {0}", qp.QueueName);
            else
                QLogger.InfoFormat("PersistQueue will clear all items from : {0}", qp.QueueName);

            m_db.ReloadOrClearPersist(qp.ReloadOnStart);

        }

        private void M_db_ClearCompleted(object sender, EventArgs e)
        {
            QLogger.InfoFormat("PersistQueue ClearCompleted : {0}", m_db.Name);
        }

        //private void M_db_ItemChanged(object sender, Generic.GenericEventArgs<string, string, IQueueItem> e)
        //{
        //    QLogger.InfoFormat("PersistQueue ItemChanged : action- {0}, key- {1}", e.Args1, e.Args2, e.Args3);
        //}

        private void M_db_ErrorOcurred(object sender, Generic.GenericEventArgs<string> e)
        {
            QLogger.ErrorFormat("PersistQueue ErrorOcurred : {0}", e.Args);
        }

        private void M_db_LoadCompleted(object sender, Generic.GenericEventArgs<string, int> e)
        {
            QLogger.InfoFormat("PersistQueue LoadCompleted : {0}, Count:{1}", e.Args1, e.Args2);
        }

        private void M_db_BeginLoading(object sender, EventArgs e)
        {
            QLogger.InfoFormat("PersistQueue BeginLoading : {0}", m_db.Name);
        }

        public void Dispose()
        {
            //base.Dispose();
            if (m_db != null)
            {
                //m_db.Dispose();
                m_db = null;
            }
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

}
