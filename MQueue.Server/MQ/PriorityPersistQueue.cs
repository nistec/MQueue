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

namespace Nistec.Messaging
{

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

        #endregion

        #region override

        protected override bool TryAdd(Ptr ptr, IQueueItem item)
        {
            return m_db.TryAdd(ptr.Identifier,item.Copy());
        }

        protected override bool TryPeek(Ptr ptr, out IQueueItem item)
        {
            return m_db.TryGetValue(ptr.Identifier, out item); 
        }

        protected override bool TryDequeue(Ptr ptr, out IQueueItem item)
        {

            return m_db.TryRemove(ptr.Identifier, out item);

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

}
