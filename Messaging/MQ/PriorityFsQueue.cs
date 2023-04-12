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
using Nistec.Data.Entities;
using System.Threading.Tasks;
using Nistec.Messaging.Io;
using Nistec.Logging;

namespace Nistec.Messaging
{

    public sealed class PriorityFsQueue : PriorityQueue
    {

        #region members

        FileMessage m_fs;
        ConcurrentDictionary<Ptr, IQueueMessage> QueueItems;
        PersistCommitMode CommitMode = PersistCommitMode.OnMemory;
        CoverMode CoverMode = CoverMode.FileStream;
        public string RootPath { get; private set; }
        #endregion

        #region ctor

        public PriorityFsQueue(QueueHost qp, int consumeInterval=100)
            : base(qp.HostName, consumeInterval)
        {

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 101;

            QueueItems = new ConcurrentDictionary<Ptr, IQueueMessage>(concurrencyLevel, initialCapacity);

            //CommitMode = qp.CommitMode;
            //CoverMode = qp.Mode;
            RootPath = qp.HostAddress;
            CommitMode = qp.CommitMode;
            CoverMode = qp.CoverMode;

            if (qp.CoverMode == CoverMode.FileStream)
            {
                CoverMode = CoverMode.FileStream;
                //DbLiteSettings settings = new DbLiteSettings()
                //{
                //    Name = qp.QueueName,
                //    CommitMode = (CommitMode)(int)qp.CommitMode,
                //    DbPath = AgentManager.Settings.QueuesPath
                //};
                //settings.SetFast();
                //QueueHost queueHost = qp.GetRoutHost();
                m_fs = new FileMessage(qp);
                m_fs.Logger = Logger;
                //InitRecoverQueue();

                //m_db = new PersistentBinary<IQueueMessage>(settings);
                ////m_db = new PersistentDictionary(settings);
                //m_db.BeginLoading += M_db_BeginLoading;
                //m_db.LoadCompleted += M_db_LoadCompleted;
                //m_db.ErrorOcurred += M_db_ErrorOcurred;
                //m_db.ClearCompleted += M_db_ClearCompleted;
                ////m_db.ItemChanged += M_db_ItemChanged;

                //m_db.ItemLoaded = (item) =>
                //{
                //    this.ReEnqueue(item);
                //};

                if (qp.ReloadOnStart)
                {
                    Logger.Info("PriorityFsQueue will load items to : {0}", qp.HostName);
                    m_fs.ReloadItemsAsync(FolderType.Queue, 0, (IQueueMessage item) =>
                    {
                        this.Requeue(item);
                    });
                }
                else
                {
                    Logger.Info("PriorityFsQueue will clear all items from : {0}", qp.HostName);
                    m_fs.ClearItems(FolderType.Queue);
                }
            }
        }

        //internal void InitRecoverQueue()
        //{

        //    Console.WriteLine("Init RecoverQueue");

        //    try
        //    {
 
        //            Assists.EnsureQueueSectionPath(RootPath, Assists.FolderQueue, Name);
        //            Assists.EnsureQueueSectionPath(RootPath, Assists.FolderInfo, Name);
        //            Assists.EnsureQueueSectionPath(RootPath, Assists.FolderCovered, Name);
        //            Assists.EnsureQueueSectionPath(RootPath, Assists.FolderSuspend, Name);

        //            //string path = GetQueuePath();
        //            //if (!Directory.Exists(path))
        //            //{
        //            //    Directory.CreateDirectory(Path.Combine(m_QueuesPath, IoAssists.FolderQueue));
        //            //    Directory.CreateDirectory(Path.Combine(m_QueuesPath, IoAssists.FolderInfo));
        //            //    Directory.CreateDirectory(Path.Combine(m_QueuesPath, IoAssists.FolderCovered));
        //            //    Directory.CreateDirectory(Path.Combine(m_QueuesPath, IoAssists.FolderSuspend));
        //            //    Console.WriteLine("Create MQueue Folder: " + path);
        //            //}

        //            //AsyncReEnqueueItems();

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(Name + " Error:" + ex.Message);
        //    }
        //}
        //private void M_db_ClearCompleted(object sender, EventArgs e)
        //{
        //    QLogger.InfoFormat("PriorityComplexQueue ClearCompleted : {0}", m_db.Name);
        //}

        //private void M_db_ItemChanged(object sender, Generic.GenericEventArgs<string, string, IQueueMessage> e)
        //{
        //    QLogger.InfoFormat("PriorityPersistQueue ItemChanged : action- {0}, key- {1}", e.Args1, e.Args2, e.Args3);
        //}

        //private void M_db_ErrorOcurred(object sender, Generic.GenericEventArgs<string> e)
        //{
        //    QLogger.ErrorFormat("PriorityComplexQueue ErrorOcurred : {0}", e.Args);
        //}

        //private void M_db_LoadCompleted(object sender, Generic.GenericEventArgs<string, int> e)
        //{
        //    QLogger.InfoFormat("PriorityComplexQueue LoadCompleted : {0}, Count:{1}", e.Args1, e.Args2);
        //}

        //private void M_db_BeginLoading(object sender, EventArgs e)
        //{
        //    QLogger.InfoFormat("PriorityComplexQueue BeginLoading : {0}", m_db.Name);
        //}

        public override void Dispose()
        {
            base.Dispose();
            if (m_fs != null)
            {
                //m_db.Dispose();
                m_fs.Dispose();
                m_fs = null;
            }
        }

        #endregion

        #region Persist Tasks

        bool PersistItemRemove(IQueueMessage item)
        {
            return Task<bool>.Factory.StartNew(() =>
                m_fs.DeleteItem(item)
            ).Result;
            //return true;
        }

        bool PersistItemAdd(IQueueMessage item)
        {

            Task tsk = Task.Factory.StartNew(() =>
                m_fs.Enqueue(item)
            );
            return true;
        }
        #endregion

        #region override


        protected override bool TryAdd(Ptr ptr, IQueueMessage item)
        {
            var copy = item.Copy();
            QueueItems[ptr] = copy;

            if (CoverMode == CoverMode.FileStream)
            {
                if (CommitMode == PersistCommitMode.OnDisk)
                {
                    if (m_fs.Enqueue(item).MessageState== MessageState.Received)
                    {
                        OnTryAdd(ptr, item, true);
                        return true;
                    }
                }
                else //if (CommitMode == CommitMode.OnMemory)
                {
                    return PersistItemAdd(copy);
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
            if (CoverMode == CoverMode.FileStream)
            {

                if (CommitMode == PersistCommitMode.OnDisk)
                {
                    item= m_fs.ReadItem(ptr.Identifier,true);
                    OnTryPeek(ptr, item, true);
                    return true;

                    //if (m_fs.TryGetValue(ptr.Identifier, out item))
                    //{
                    //    OnTryPeek(ptr, item, true);
                    //    return true;
                    //}
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

            if (CoverMode == CoverMode.FileStream)
            {
                if (QueueItems.TryRemove(ptr, out item))
                {
                    if (CommitMode == PersistCommitMode.OnDisk)
                    {
                        //IQueueMessage item_pers = null;

                        if(m_fs.DeleteItem(item))
                        {
                            OnTryDequeue(ptr, item, true);
                            return true;
                        }
                        //if (m_fs.TryRemove(ptr.Identifier, out item_pers))
                        //{
                        //    OnTryDequeue(ptr, item, true);
                        //    return true;
                        //}
                    }
                    else
                    {
                        return PersistItemRemove(item);
                        
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
                if (CoverMode == CoverMode.FileStream)
                {
                    item = base.Dequeue();
                    if (item != null)
                    {
                        //IQueueMessage qi;

                        //m_fs.TryRemove(item.Identifier, out qi);

                        m_fs.DeleteItem(item);
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
                    //if (CoverMode == CoverMode.FileStream)
                    //{
                    //    var items = m_fs.QueryItems("*", null);
                    //    return items == null ? null : items.Cast<IPersistEntity>();
                    //}
                    //else
                    //{
                        List<IPersistEntity> list = new List<IPersistEntity>();
                        foreach (var g in QueueItems)
                        {
                            list.Add(new PersistEntity() { body = g.Value, key = g.Key.Identifier, name = Name, timestamp = g.Key.ArrivedTime });
                        }
                        return list;
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //if no items
            return new List<IPersistEntity>();

        }

        public override bool ItemExists(Ptr ptr)
        {
            try
            {
                if (QueueItems.ContainsKey(ptr))
                {
                   return m_fs.ItemExists(ptr.Identifier);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ItemExists error: " + ex.Message);
            }
            return false;
        }


        protected override void ClearItems()
        {
            QueueItems.Clear();
            if (CoverMode == CoverMode.FileStream)
                m_fs.ClearItems();
        }

        internal void ReloadItemsInternal()
        {
            ReloadItems();
        }
        internal protected override void ReloadItems()
        {
            if (CoverMode == CoverMode.FileStream)
                m_fs.ReloadItemsAsync(FolderType.Queue,0, (IQueueMessage item) =>
                {
                    this.Requeue(item);
                });
        }
        protected override int Count()
        {
            if (CoverMode == CoverMode.Persistent)
            {
                if (CommitMode == PersistCommitMode.OnDisk)
                    return m_fs.Count();
                else
                    return QueueItems.Count;
            }
            else
            {
                return QueueItems.Count;
            }
        }

        /*
        /// <summary>
        /// Enqueue Message
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override void Requeue(IQueueMessage item)
        {
            base.Requeue(item);

            QueueItems[item.GetPtr()] = item;

            //if (MessageArrived != null)
            //{
            //    OnMessageArrived(new QueueItemEventArgs(item, MessageState.Arrived));
            //}
            //return new QueueAck(MessageState.Arrived, item);// new Ptr(ptr, PtrState.Arrived);
        }
        */
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

}
