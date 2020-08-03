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
using Nistec.Messaging.Adapters;
using Nistec.Messaging.Db;
using Nistec.IO;


namespace Nistec.Messaging
{

    public sealed class PriorityDbQueue : PriorityQueue
    {
        #region members

        DbMessageContext m_db;
      
        #endregion

        #region override

        protected override bool TryAdd(Ptr ptr, IQueueItem item)
        {

            var ack = m_db.Enqueue(((QItemStream)item).Copy());
            return ack > 0;

        }

        protected override bool TryPeek(Ptr ptr, out IQueueItem item)
        {

            var bytes = m_db.PeekStream(ptr.Host, ptr.Identifier);
            if (bytes != null)
            {
                item = QItemStream.Create(new NetStream(bytes));
                return true;
            }

            item = null;
            return false;
        }

        protected override bool TryDequeue(Ptr ptr, out IQueueItem item)
        {

            var bytes = m_db.DequeueStream(ptr.Host, ptr.Identifier);
            if (bytes != null)
            {
                item = QItemStream.Create(new NetStream(bytes));
                return true;
            }

            item = null;
            return false;
        }

        protected override IQueueItem GetFirstItem()
        {
            IQueueItem item = null;
            try
            {


                return m_db.Dequeue(this.Host);


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

            m_db.ClearAllItems(this.Host);

        }

        protected override int Count()
        {

            return m_db.Count(this.Host);

        }

        #endregion

        #region ctor

        public PriorityDbQueue(string host)
            : base(host)
        {
            m_db = new DbMessageContext();

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
                m_db.Dispose();
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
