using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Nistec.Collections;
using System.Transactions;
using System.Collections.Concurrent;
using Nistec.Messaging.Db;
using Nistec.IO;
using Nistec.Data.Sqlite;
using Nistec.Messaging.Server;
using Nistec.Messaging.Config;

namespace Nistec.Messaging
{

    public sealed class PriorityPersistQueue : PriorityQueue
    {


        #region members

        PersistentQueue m_db;
      
        #endregion

        #region override

        protected override bool TryAdd(Ptr ptr, IQueueItem item)
        {
            return m_db.TryAdd(ptr.Identifier, ((QueueItem)item).Copy());
        }

        protected override bool TryPeek(Ptr ptr, out IQueueItem item)
        {
            IPersistItem qi;
            bool res = m_db.TryGetValue(ptr.Identifier, out qi);
            item = new QueueItem(qi);
            return res;

            //var bytes = m_db.TryGetValue(ptr.Host, ptr.Identifier);
            //if (bytes != null)
            //{
            //    item = QueueItem.Create(new NetStream(bytes));
            //    return true;
            //}

            //item = null;
            //return false;
        }

        protected override bool TryDequeue(Ptr ptr, out IQueueItem item)
        {
            IPersistItem qi;
            bool res=m_db.TryRemove(ptr.Identifier, out qi);
            item = new QueueItem(qi);
            return res;

            //var bytes = m_db.DequeueStream(ptr.Host, ptr.Identifier);
            //if (bytes != null)
            //{
            //    item = QueueItem.Create(new NetStream(bytes));
            //    return true;
            //}

            //item = null;
            //return false;
        }

        protected override IQueueItem GetFirstItem()
        {
            IQueueItem item = null;
            try
            {
                item= base.Dequeue();
                if (item != null)
                {
                    IPersistItem qi;
                    m_db.TryRemove(item.Identifier, out qi);
                }

                //return m_db.Dequeue(this.Host);


                //if (Count() > 0)
                //{

                //    foreach (object o in QueueList.Keys)
                //    {
                //        item = Dequeue((Guid)o);
                //        if (item != null)
                //        {
                //            break;
                //        }
                //    }
                //}
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

        internal protected override void ReloadItems()
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
            m_db = new PersistentQueue(settings);

            // AdapterProperties ap = new AdapterProperties()
            //{
            //    Source = new QueueHost(qp.QueueName, ".", qp.CoverPath, HostAddressTypes.db),
            //    OperationType = AdapterOperations.Sync,
            //    ProtocolType = AdapterProtocols.Db,
            //    ConnectTimeout = qp.ConnectTimeout
            //};
            //m_adapter = new DbAdapter(ap);
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
