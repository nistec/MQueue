using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Runtime.Remoting;  
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Permissions;
using MControl.Messaging;


namespace MControl.Queue.Service
{


    internal class RemoteQueueServer : MarshalByRefObject, IRemoteQueue
    {
        
        //internal static readonly Dictionary<string, McQueue> QueueManager = new Dictionary<string, McQueue>();

        //internal static readonly McQueue mcq = new McQueue("QueueManager");

        internal static McQueues QueueManager
        {
            get { return McQueue.Queues; }
        }

        public RemoteQueueServer()
        {
            
        }


        #region static methods

        //public static void AddQueue(string name)
        //{
        //    McQueueProperties prop = new McQueueProperties(name);
        //    McQueue rq = McQueue.Create(prop);
        //    QueueManager[prop.QueueName] = rq;
        //}

        //public void AddQueue(IDictionary prop)
        //{
        //    AddQueue(McQueueProperties.Create(prop));
        //}

        //public static void AddQueue(McQueueProperties prop)
        //{
        //    McQueue rq = McQueue.Create(prop);
        //    QueueManager[prop.QueueName] = rq;
        //}

        //public void RemoveQueue(string name)
        //{
        //    RemoveQueueInternal(name);
        //}

        // public static void RemoveQueueInternal(string name)
        //{
        //    if (QueueManager.ContainsKey(name))
        //    {
        //        //TODO:DELETE FROM DB
        //        QueueManager.Remove(name);
        //        //McQueue.Delete(queueName);
        //    }
        //}

        //public string[] QueueList
        //{
        //    get { return QueueListInternal; }
        //}

        //public static string[] QueueListInternal
        //{
        //    get
        //    {
        //        if (QueueManager.Count == 0)
        //        {
        //            return null;
        //        }
        //        string[] list = new string[QueueManager.Count];
        //        int index = 0;
        //        foreach (string k in QueueManager.Keys)
        //        {
        //            list[index] = k;
        //            index++;
        //        }
        //        return list;
        //    }
        //}

        //public bool QueueExists(string name)
        //{
        //    return QueueExistsInternal(name);
        //}

        //public static bool QueueExistsInternal(string name)
        //{
        //    return QueueManager.ContainsKey(name);
        //}

        #endregion

        #region private methods


        #endregion

        #region public methods

        public string Reply(string text)
        {
            return text;
        }
        public bool CanQueue(string queueName, uint count)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return false;
            return Q.CanQueue(count);
        }
        public DataTable GetQueueItemsTable(string queueName)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return null;
            return Q.GetQueueItemsTable();
        }
        public IQueueItem[] GetQueueItems(string queueName)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return null;
            return Q.GetQueueItems();
        }
        //public object ExecuteCommand(string queueName,string commandName, string command, params string[] param)
        //{
        //    McQueue Q = QueueManager[queueName];
        //    if (Q == null)
        //        return null;
        //    return Q.ExecuteCommand(commandName, command, param);
        //}
        public void SetProperty(string queueName,string propertyName, object propertyValue)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return;
            Q.SetProperty(propertyName, propertyValue);
        }
        public object GetProperty(string queueName, string propertyName)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return null;
            return Q.GetProperty(propertyName);
        }
        public void ValidateCapacity(string queueName)
        {
            QueueManager[queueName].ValidateCapacity();
        }

    
        #endregion

        #region Queue methods

        public string Peek(string queueName)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return null;

            IQueueItem item = Q.Peek();
            return SerializeItem(item);
        }
        public string Peek(string queueName, Priority priority)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return null;

            IQueueItem item = Q.Peek(priority);
            return SerializeItem(item);
        }
        public string Peek(string queueName, Guid ptr)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return null;

            IQueueItem item = Q.Peek(ptr);
            return SerializeItem(item);
        }
        public string Dequeue(string queueName)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return null;

            IQueueItem item = Q.Dequeue();
            //Netlog.InfoFormat("Dequeue:{0} item:{1}", DateTime.Now, item.MessageId);
            return SerializeItem(item);
        }
        public string Dequeue(string queueName,Priority priority)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return null;

            IQueueItem item = Q.Dequeue(priority);
            return SerializeItem(item);
        }
        public string Dequeue(string queueName,Guid ptr)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return null;

            IQueueItem item = Q.Dequeue(ptr);
            return SerializeItem(item);
        }
        private string SerializeItem(IQueueItem item)
        {
            if (item == null)
                return null;
            return item.Serialize();
        }
        public void Enqueue(string queueName, string serItem)
        {
            QueueItem item = QueueItem.Deserialize(serItem);
            if (item != null)
            {
                McQueue Q = QueueManager[queueName];
                if (Q == null)
                    return;

                Q.Enqueue(item as IQueueItem);
            }
        }

        public void Completed(string queueName, Guid ItemId, int status, bool hasAttach)
        {

            //QueueItem item = QueueItem.Deserialize(serItem);
            //if (item != null)
            //{
            //    QueueManager[queueName].Completed(item.ItemId, (ItemState)status);
            //}
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return;

            Q.Completed(ItemId, status,hasAttach);
        }

        public void ReEnqueue(string queueName,string serItem)
        {
            QueueItem item = QueueItem.Deserialize(serItem);
            if (item != null)
            {
                McQueue Q = QueueManager[queueName];
                if (Q == null)
                    return;

                Q.ReEnqueue(item as IQueueItem);
            }
        }

        //public int ReEnqueueLog(string queueName)
        //{
        //    return QueueManager[queueName].ReEnqueueLog();

        //}

        public void AbortTrans(string queueName, Guid ItemId, bool hasAttach)
        {
            //QueueItem item = QueueItem.Deserialize(serItem);
            //if (item != null)
            //{
            //    QueueManager[queueName].AbortTrans(serItem);
            //}
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return;

            Q.AbortTrans(ItemId,hasAttach);

        }

        public void CommitTrans(string queueName, Guid ItemId, bool hasAttach)
        {
            //QueueItem item = QueueItem.Deserialize(serItem);
            //if (item != null)
            //{
            //    QueueManager[queueName].CommitTrans(serItem);
            //}
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return;

            Q.CommitTrans(ItemId,hasAttach);
        }

        #endregion

        #region properties

        public bool IsTrans(string queueName)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return false;

           return Q.IsTrans; 
        }

        public int Count(string queueName)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return 0;

            return Q.Count; 
        }
        public int MaxCapacity(string queueName)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return 0;
            return Q.MaxCapacity; 
        }

        public bool Initilaized(string queueName)
        {
            McQueue Q = QueueManager[queueName];
            if (Q == null)
                return false;

            return Q.Initilaized; 
        }

        #endregion


        //private McQueue this[string queueName]
        //{
        //    get
        //    {
        //        if (!_QueueHandler.ContainsKey(queueName))
        //        {
        //            throw new ArgumentException(queueName + " Not exists");
        //        }
        //        return _QueueHandler[queueName];
        //    }
        //}

   
        //public void TransBegin(string queueName, string serItem)
        //{
        //    QueueItem item = QueueItem.Deserialize(serItem);
        //    if (item != null)
        //    {
        //        _QueueHandler[queueName].TransBegin(item as IQueueItem);
        //    }

        //}
     
  

    }


}

    
