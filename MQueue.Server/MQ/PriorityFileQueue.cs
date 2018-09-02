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
using Nistec.Messaging.Io;
using Nistec.Messaging.Server;
using Nistec.Messaging.Adapters;


namespace Nistec.Messaging
{

    public sealed class PriorityFileQueue : PriorityQueue
    {
        #region members

        FileMessage m_fm;

        //string GetHostAddress()
        //{
        //    return FileMessage.FormatQueuePath(QueueSettings.RootPath, this.Host);
        //}
        string GetFilename(string identifier)
        {
            return FileMessage.FormatFullFilename(AgentManager.Settings.RootPath, this.Host, identifier);
        }
        #endregion

        #region override

        protected override bool TryAdd(Ptr ptr, IQueueItem item)
        {
            try
            {
                m_fm.Enqueue((QItemStream)item);
                //((QItemStream)item).SaveFile(ptr);
                return true;
            }
            catch (Exception ex)
            {
                QLog.Exception("TryAdd file error: ", ex, true, true);
                return false;
            }
        }

        protected override bool TryPeek(Ptr ptr, out IQueueItem item)
        {
            try
            {
                //string qfile = IoAssists.GetQueueFilename(m_fm.RootPath, m_fm.HostName, ptr.Identifier);

                item = m_fm.ReadFile(ptr.Identifier, false);//GetFilename(ptr.Identifier), false);
                return item != null;

                //item=null;
                //var state= ptr.ReadFile(out item);
                //return state== ReadFileState.Completed;
            }
            catch (Exception ex)
            {
                QLog.Exception("TryPeek file error: ", ex, true, true);
                item = null;
                return false;
            }
        }

        protected override bool TryDequeue(Ptr ptr, out IQueueItem item)
        {
            try
            {
                //string qfile = IoAssists.GetQueueFilename(m_fm.RootPath, m_fm.HostName, ptr.Identifier);

                item = m_fm.DequeueItem(ptr.Identifier);//GetFilename(ptr.Identifier));
                return item != null;

                //item = null;
                //var state = FileMessage.DequeueFile(ptr, QueueSettings.QueuesPath, IsTrans, out item);
                //return state == ReadFileState.Completed;
            }
            catch (Exception ex)
            {
                QLog.Exception("TryDequeue file error: ", ex, true, true);
                item = null;
                return false;
            }
        }

        protected override IQueueItem GetFirstItem()
        {
            IQueueItem item = null;
            try
            {

                var items = m_fm.DequeueItems(FileOrderTypes.ByName,1);
                if (items != null)
                {
                    item = items[0];
                }

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
                QLog.Exception("GetFirstItem file error: ", ex, true, true);
                return null;
            }
            return item;
        }

        protected override void ClearItems()
        {
            m_fm.ClearItems();
        }

        protected override int Count()
        {
           return m_fm.Count();
        }

        #endregion

        #region ctor

        public PriorityFileQueue(string host)
            : base(host)
        {
            m_fm = new FileMessage(host, AgentManager.Settings.QueuesPath());
        }

        public override void Dispose()
        {
            base.Dispose();
            if (m_fm != null)
            {
                m_fm.Dispose();
                m_fm = null;
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
