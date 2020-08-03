using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using Nistec.Generic;
using Nistec.Messaging.Remote;
using Nistec.IO;
using System.Collections.Concurrent;
using Nistec.Runtime;
using System.Runtime.Serialization;
using Nistec.Runtime.Advanced;
using Nistec.Messaging.Transactions;
using Nistec.Messaging;
using Nistec.Messaging.Config;
using System.IO;
using Nistec.Logging;

namespace Nistec.Messaging.Session
{
    public class SessionController
    {
        protected ConcurrentDictionary<string, MQueue> MQ;

        internal TransactionDispatcher m_TransDispatcher;

         //public static SyncTimerDispatcher<TransactionItem> SyncTimer = new SyncTimerDispatcher<TransactionItem>();
         internal TransactionDispatcher TransDispatcher
         {
             get { return m_TransDispatcher; }
         }


         public SessionController()
        {
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            int initialCapacity = 5;
            MQ = new ConcurrentDictionary<string, MQueue>(concurrencyLevel, initialCapacity);
            m_TransDispatcher = new TransactionDispatcher();
            //m_TransDispatcher.SyncItemCompleted += new SyncItemEventHandler<TransactionItem>(m_TransDispatcher_SyncCompleted);
        }
        //void m_TransDispatcher_SyncCompleted(object sender, SyncItemEventArgs<TransactionItem> e)
        //{
        //    //TODO:

        //    //e.Item.SyncExpired();
        //}

            

        public virtual void Start() { }
        public virtual void Stop() { }

  
        #region queue request
        /*
        internal void Execute(QItemStream request, Stream stream)
        {
            try
            {
                switch (request.Command)
                {
                    case QueueCmd.Enqueue:
                        MessageAckServer.WriteAck(stream, ExecSet(request), null);
                        break;
                    case QueueCmd.Dequeue:
                    case QueueCmd.DequeuePriority:
                    case QueueCmd.DequeueItem:
                    case QueueCmd.Peek:
                    case QueueCmd.PeekPriority:
                    case QueueCmd.PeekItem:
                        MessageAckServer.WriteResponse(stream, ExecGet(request), MessageState.Receiving);
                        break;
                    case QueueCmd.Commit:

                        break;
                    case QueueCmd.Abort:

                        break;

                    //operation
                    case QueueCmd.AddQueue:
                        MQueue mq = null;
                        MessageAckServer.WriteAck(stream, AddQueue(QProperties.Get(request.GetHeader()), out mq), null);
                        break;
                    case QueueCmd.RemoveQueue:
                        MessageAckServer.WriteAck(stream, RemoveQueue(request.Host.HostName), null);
                        break;
                    case QueueCmd.HoldEnqueue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.NoneHoldEnqueue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.HoldDequeue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.NoneHoldDequeue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.EnableQueue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.DisableQueue:
                        throw new Exception("Operation not supported");
                    //reports
                    case QueueCmd.Exists:
                        MessageAckServer.WriteAck(stream, Exists(request.Host.HostName), null);
                        break;
                    case QueueCmd.QueueProperty:
                        throw new Exception("Operation not supported");
                    case QueueCmd.ReportQueueList:
                        GetQueueList();
                        break;
                    case QueueCmd.ReportQueueItems:
                        throw new Exception("Operation not supported");
                    case QueueCmd.ReportQueueStatistic:
                        throw new Exception("Operation not supported");
                    case QueueCmd.PerformanceCounter:
                        throw new Exception("Operation not supported");
                    case QueueCmd.QueueCount:
                        MessageAckServer.WriteReport(stream, QueueCount(request.Host.HostName), QueueCmd.QueueCount, MessageState.Ok, null);
                        break;
                }
            }
            catch (MessageException mex)
            {
                MessageAckServer.WriteError(stream, mex.MessageState, mex);
                Netlog.Exception("ExecGet MessageException: ", mex, true);
            }
            catch (ArgumentException ase)
            {
                MessageAckServer.WriteError(stream, MessageState.ArgumentsError, ase);
                Netlog.Exception("ExecGet ArgumentException: ", ase, true, true);
            }
            catch (SerializationException se)
            {
                MessageAckServer.WriteError(stream, MessageState.SerializeError, se);
                Netlog.Exception("ExecGet SerializationException: ", se, true);
            }
            catch (Exception ex)
            {
                MessageAckServer.WriteError(stream, MessageState.UnExpectedError, ex);
                Netlog.Exception("ExecGet Exception: ", ex, true, true);
            }

        }
        */
        internal NetStream ExecRequset(QItemStream request)
        {
            try
            {
                switch (request.Command)
                {
                    case QueueCmd.Enqueue:
                        return MessageAckServer.DoAck(ExecSet(request), null);
                    case QueueCmd.Dequeue:
                    case QueueCmd.DequeuePriority:
                    case QueueCmd.DequeueItem:
                    case QueueCmd.Peek:
                    case QueueCmd.PeekPriority:
                    case QueueCmd.PeekItem:
                        return MessageAckServer.DoResponse(ExecGet(request), MessageState.Receiving);
                    case QueueCmd.Commit:
                        break;
                    case QueueCmd.Abort:
                        break;

                    //operation
                    case QueueCmd.AddQueue:
                        MQueue mq = null;
                        return MessageAckServer.DoAck(AddQueue(new QProperties(request.GetBodyStream()), out mq), null);
                    case QueueCmd.RemoveQueue:
                        return MessageAckServer.DoAck(RemoveQueue(request.Destination), null);
                    case QueueCmd.HoldEnqueue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.NoneHoldEnqueue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.HoldDequeue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.NoneHoldDequeue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.EnableQueue:
                        throw new Exception("Operation not supported");
                    case QueueCmd.DisableQueue:
                        throw new Exception("Operation not supported");

                    //reports
                    case QueueCmd.Exists:
                        return MessageAckServer.DoAck(Exists(request.Destination), null);
                    case QueueCmd.QueueProperty:
                        throw new Exception("Operation not supported");
                    case QueueCmd.ReportQueueList:
                        GetQueueList();
                        break;
                    case QueueCmd.ReportQueueItems:
                        throw new Exception("Operation not supported");
                    case QueueCmd.ReportQueueStatistic:
                        throw new Exception("Operation not supported");
                    case QueueCmd.PerformanceCounter:
                        throw new Exception("Operation not supported");
                    case QueueCmd.QueueCount:
                        return MessageAckServer.DoReport(QueueCount(request.Destination), QueueCmd.QueueCount, MessageState.Ok, null);
                }
            }
            catch (MessageException mex)
            {
                Netlog.Exception("ExecGet MessageException: ", mex, true);
                return MessageAckServer.DoError(mex.MessageState, mex);
            }
            catch (ArgumentException ase)
            {
                Netlog.Exception("ExecGet ArgumentException: ", ase, true, true);
                return MessageAckServer.DoError(MessageState.ArgumentsError, ase);
            }
            catch (SerializationException se)
            {
                Netlog.Exception("ExecGet SerializationException: ", se, true);
                return MessageAckServer.DoError(MessageState.SerializeError, se);
            }
            catch (Exception ex)
            {
                Netlog.Exception("ExecGet Exception: ", ex, true, true);
                return MessageAckServer.DoError(MessageState.UnExpectedError, ex);
            }
            return null;
        }

        internal IQueueItem ExecGet(QItemStream request)
        {
            if (request.Destination == null)
            {
                throw new MessageException(MessageState.InvalidMessageHost, "Invalid message.Host " );
            }
            MQueue Q = Get(request.Destination);
            if (Q == null)
            {
                throw new MessageException(MessageState.InvalidMessageHost, "message.HostName not found " + request.Destination);
            }
            switch (request.Command)
            {
                case QueueCmd.Dequeue:
                    return Q.Dequeue();
                case QueueCmd.DequeuePriority:
                    return Q.Dequeue(request.Priority);
                case QueueCmd.Peek:
                    return Q.Peek();
                case QueueCmd.PeekPriority:
                    return Q.Peek(request.Priority);
            }

            return null;
        }

        internal MessageState ExecSet(QItemStream item)
        {

            if (item == null)
            {
                throw new ArgumentNullException("QueueManager.ExecSet request");
            }
            if (item.Destination == null)
            {
                throw new ArgumentNullException("QueueManager.ExecSet request.Host is invalid");
            }

            MQueue Q = Get(item.Destination);
            if (Q == null)
            {
                throw new MessageException(MessageState.InvalidMessageHost, "message.HostName not found " + item.Destination);
            }
            //var item = new QItemStream(request, QueueCmd.Enqueue);
            var ptr = Q.Enqueue(item);
            return ptr.MessageState;
        }

        #endregion

        
        /// <summary>
        /// Get queue <see cref="MQueue"/> using queue name.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public MQueue Get(string queueName)
        {
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException("QueueManager.Get queueName is null or empty");
            }
            MQueue queue;
            if (MQ.TryGetValue(queueName, out queue))
            {
                return queue;
            }

            //return null;
            throw new KeyNotFoundException("Queue not found: " + queueName);
        }

        /// <summary>
        /// Queues
        /// </summary>
        /// <returns></returns>
        public ICollection<MQueue> Items
        {
            get { return MQ.Values; }
        }

        public string[] GetQueueList()
        {
            string[] array = new string[MQ.Count];
            MQ.Keys.CopyTo(array, 0);
            return array;
            
        }
      
        /// <summary>Creates a non-transactional Message Queuing queue at the specified path.</summary>
        /// <returns>A <see cref="T:Nistec.Messaging.MQueue"></see> that represents the new queue.</returns>
        /// <param name="queueName">The path of the queue to create. </param>
        public MQueue AddQueue(string queueName)
        {
            return AddQueue(queueName, false, CoverMode.Memory);
        }

        /// <summary>Creates a transactional or non-transactional Message Queuing queue at the specified path.</summary>
        /// <returns>A <see cref="T:Nistec.Messaging.MQueue"></see> that represents the new queue.</returns>
        /// <param name="queueName">The path of the queue to create. </param>
        /// <param name="isTrans">true to create a transactional queue; false to create a non-transactional queue. </param>
        /// <param name="mode">One of the <see cref="CoverMode"/> options.</param>
        public MQueue AddQueue(string queueName, bool isTrans, CoverMode mode)
        {

            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            if (queueName.Length == 0)
            {
                throw new ArgumentException("InvalidParameter", "queueName");
            }
            if (MQ.ContainsKey(queueName))
            {
                return (MQueue)MQ[queueName];
            }
            //CoverMode mode= CoverMode.None; 
            //if (!string.IsNullOrEmpty(Connection) && QueueProvider > QueueProvider.None)
            //{
            //    mode= CoverMode.ItemsOnly;
            //}

            return AddQueue(new QProperties(queueName, isTrans, mode));
        }

        /// <summary>Creates Message Queuing queue by specified properties.</summary>
        /// <param name="prop">The queue properties. </param>
        /// <returns>A <see cref="T:Nistec.Messaging.MQueue"></see> that represents the new queue.</returns>
        public MQueue AddQueue(QProperties prop)
        {
            //prop.IsValid(lockKey);

            if (MQ.ContainsKey(prop.QueueName))
            {
                if (prop.ReloadOnStart)
                {
                    MQueue q = new MQueue(prop);
                    MQ[prop.QueueName] = q;
                    return q;
                }
                return (MQueue)MQ[prop.QueueName];
            }
            //if (prop.IsDbQueue)
            //{
            //    AddDbQueue(prop.QueueName, prop.IsTrans);
            //}
            MQueue queue=new MQueue(prop);
            MQ[prop.QueueName] = queue;

            return queue;
        }

        public MessageState AddQueue(QProperties prop, out MQueue mq)
        {

            if (MQ.ContainsKey(prop.QueueName))
            {
                if (prop.ReloadOnStart)
                {
                    MQueue q = new MQueue(prop);
                    MQ[prop.QueueName] = q;
                    mq = q;
                }
                else
                {
                    mq = MQ[prop.QueueName];
                }
                return MessageState.AllreadyExists;
            }
            //if (prop.IsDbQueue)
            //{
            //    AddDbQueue(prop.QueueName, prop.IsTrans);
            //}
            MQueue queue = new MQueue(prop);
            MQ[prop.QueueName] = queue;
            mq=queue;
            Netlog.InfoFormat("AddQueue : {0}", prop.Print());
            return  MessageState.Ok;
        }

        /// <summary>Deletes a queue on a Message Queuing server.</summary>
        /// <param name="queueName">The location of the queue to be deleted. </param>
        public bool RemoveQueue(string queueName)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            if (queueName.Length == 0)
            {
                throw new ArgumentException("InvalidParameter", "queueName");
            }
            //RemoveDbQueue(queueName);
 
            MQueue queue;
            if (MQ.TryRemove(queueName, out queue))
            {
                Netlog.InfoFormat("RemoveQueue : {0}", queueName);
                return true;
            }
            else
            {
                Netlog.InfoFormat("RemoveQueue not removed : {0}", queueName);
                return false;
            }
            
        }
        /// <summary>Determines whether a Message Queuing queue exists at the specified path.</summary>
        /// <returns>true if a queue with the specified path exists; otherwise, false.</returns>
        /// <param name="path">The location of the queue to find. </param>
        public bool Exists(string queueName)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            if (queueName.Length == 0)
            {
                throw new ArgumentException("InvalidParameter", "queueName");
            }
            return MQ.ContainsKey(queueName);
        }

        public int QueueCount(string queueName)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }
            if (queueName.Length == 0)
            {
                throw new ArgumentException("InvalidParameter", "queueName");
            }
            MQueue queue = null;
            if (MQ.TryGetValue(queueName, out queue))
            {
                return queue.Count;
            }

            return 0;
        }

        public bool CanQueue(string queueName)
        {
            //TOTDO: FIX THIS
            return Exists(queueName);
        }
  
         internal bool IsFatalError(int value)
        {
            bool flag = value == 0;
            return (((value & -1073741824) != 0x40000000) && !flag);
        }

        internal bool IsMemoryError(int value)
        {
            if ((((value != -1072824294) && (value != -1072824226)) && ((value != -1072824221) && (value != -1072824277))) && ((((value != -1072824286) && (value != -1072824285)) && ((value != -1072824222) && (value != -1072824223))) && ((value != -1072824280) && (value != -1072824289))))
            {
                return false;
            }
            return true;
        }


        public void LoadQueueConfig()
        {

            var config = QueueServerConfig.GetConfig();

            var items = config.RemoteQueueSettings;

            foreach (QueueServerConfigItem item in items)
            {
                var prop = new QProperties(item.QueueName, item.IsTrans, (CoverMode)item.CoverMode);
                AddQueue(prop);
                QLog.Info("Queue Added: {0}", prop.Print());
            }

            //if (!QueueLoaded)
            //{
            //    System.Configuration.Configuration config =
            //ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //    XmlDocument doc = new XmlDocument();
            //    doc.Load(config.FilePath);

            //    Console.WriteLine("Load Config: " + config.FilePath);

            //    XmlNode root = doc.SelectSingleNode("//remoteSettings");
            //    XmlNodeList list = root.ChildNodes;

            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        //n.FirstChild.ChildNodes[1].InnerText
            //        McQueueProperties prop =
            //            new McQueueProperties(list[i]);
            //        //prop.ConnectionString = ConnectionString;
            //        //prop.Provider = Provider;
            //        Console.WriteLine("Load: " + prop.QueueName);

            //        RemoteQueueManager.AddQueue(prop);
            //    }
            //    QueueLoaded = true;
            //}
        }


        public DataTable GetStatistic()
        {
            DataTable dt = QueueStatisticSchema;
            foreach (MQueue item in this.Items)
            {
                DataRow dr = dt.NewRow();
                dr["QueueName"] = item.QueueName;
                dr["IsTrans"] = item.IsTrans;
                dr["MaxRetry"] = item.MaxRetry;
                dr["Mode"] = item.Mode;
                dr["Enabled"] = item.Enabled;
                dr["Count"] = item.Count;
                dr["MaxCapacity"] = item.MaxCapacity;
                //dr["MaxItemsPerSecond"] = item.MaxItemsPerSecond;

                dt.Rows.Add(dr);
            }
            return dt;

        }

        static DataTable queueStatisticSchema;
        public static DataTable QueueStatisticSchema
        {
            get
            {
                if (queueStatisticSchema == null)
                {
                    DataTable dt = new DataTable("QueueProperties");
                    DataColumn colItemId = new DataColumn("QueueName", typeof(string));
                    dt.Columns.Add(colItemId);
                    dt.Columns.Add(new DataColumn("IsTrans", typeof(bool)));
                    dt.Columns.Add(new DataColumn("MaxRetry", typeof(int)));
                    dt.Columns.Add(new DataColumn("Mode", typeof(CoverMode)));
                    dt.Columns.Add(new DataColumn("Enabled", typeof(bool)));
                    dt.Columns.Add(new DataColumn("Count", typeof(int)));
                    dt.Columns.Add(new DataColumn("MaxCapacity", typeof(int)));
                    dt.Columns.Add(new DataColumn("MaxItemsPerSecond", typeof(int)));


                    dt.PrimaryKey = new DataColumn[] { colItemId };
                    queueStatisticSchema = dt;
                }
                return queueStatisticSchema.Clone();
            }

        }

        public static DataTable QueueItemSchema()
        {

            DataTable dt = new DataTable("QueueItem");
            dt.Columns.Add(new DataColumn("ItemId", typeof(Guid)));
            dt.Columns.Add(new DataColumn("Status", typeof(int)));
            dt.Columns.Add(new DataColumn("MessageId", typeof(int)));
            dt.Columns.Add(new DataColumn("Priority", typeof(int)));
            dt.Columns.Add(new DataColumn("Retry", typeof(int)));
            dt.Columns.Add(new DataColumn("ArrivedTime", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("Subject", typeof(string)));
            dt.Columns.Add(new DataColumn("Sender", typeof(string)));
            dt.Columns.Add(new DataColumn("Destination", typeof(string)));
            dt.Columns.Add(new DataColumn("SenderId", typeof(int)));
            dt.Columns.Add(new DataColumn("OperationId", typeof(int)));
            dt.Columns.Add(new DataColumn("Identifer", typeof(int)));
            dt.Columns.Add(new DataColumn("Label", typeof(string)));
            dt.Columns.Add(new DataColumn("TransactionId", typeof(string)));
            dt.Columns.Add(new DataColumn("AppSpecific", typeof(int)));
            dt.Columns.Add(new DataColumn("Segments", typeof(int)));
            dt.Columns.Add(new DataColumn("ClientContext", typeof(string)));
            dt.Columns.Add(new DataColumn("Server", typeof(int)));
            dt.Columns.Add(new DataColumn("TimeOut", typeof(int)));
            dt.Columns.Add(new DataColumn("Host", typeof(string)));

            return dt.Clone();
        }

        #region static methods

        //public IQueueItem[] GetQueueItems(string queueName)
        //{
        //    MQueue q = Get(queueName);
        //    if (q != null)
        //    {
        //       return q.GetQueueItems();
        //    }
        //    return null;
        //}

        //public DataTable GetQueueItemsTable(string queueName)
        //{
        //    MQueue q = Get(queueName);
        //    if (q != null)
        //    {
        //        return q.GetQueueItemsTable();
        //    }
        //    return null;
        //}

        public void ClearAllItems(string queueName)
        {
            MQueue q = Get(queueName);
            if (q != null)
            {
                q.ClearQueueItems(QueueItemType.AllItems);
            }
        }

        public bool CanQueue(string queueName, uint count)
        {
            MQueue q = Get(queueName);
            if (q != null)
            {
              return  q.CanQueue(count);
            }
            return false;
        }

       
        #endregion
   
    }
}
