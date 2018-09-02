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
using MControl.Util;

namespace MControl.Messaging.Service
{


    internal class RemoteQueueServer : MarshalByRefObject, IRemoteQueue
    {
        
        internal static readonly Dictionary<string, McQueue> QueueManager = new Dictionary<string, McQueue>();

        string m_QueueName;

        public RemoteQueueServer(string queueName)
        {
            m_QueueName = queueName;
        }


        #region static methods

        public static void AddQueue(string name)
        {
            McQueueProperties prop = new McQueueProperties(name);
            McQueue rq = McQueue.Create(prop);
            QueueManager[mqp.QueueName] = rq;
        }

        public static void AddQueue(IDictionary prop)
        {
            AddQueue(McQueueProperties.Create(prop));
        }

        public static void AddQueue(McQueueProperties prop)
        {
            McQueue rq = McQueue.Create(prop);
            QueueManager[prop.QueueName] = rq;
        }

        public static void RemoveQueue(string name)
        {
            if (QueueManager.ContainsKey(name))
            {
                //TODO:DELETE FROM DB
                QueueManager.Remove(name);
                //McQueue.Delete(queueName);
            }
        }

        public static string[] QueueList
        {
            get
            {
                if (_QueueHandler.Count == 0)
                {
                    return null;
                }
                string[] list = new string[QueueManager.Count];
                int index = 0;
                foreach (string k in QueueManager.Keys)
                {
                    list[index] = k;
                    index++;
                }
                return list;
            }
        }

        public static bool QueueExists(string name)
        {
            return QueueManager.ContainsKey(name);
        }

        #endregion

        #region private methods


        #endregion

        #region public methods

        public string Reply(string text)
        {
            return text;
        }

        public DataTable GetQueueItemsTable()
        {
            return QueueManager[m_QueueName].GetQueueItemsTable();
        }
        public IQueueItem[] GetQueueItems()
        {
            return QueueManager[m_QueueName].GetQueueItems();
        }
        public object ExecuteCommand(string commandName, string command, params string[] param)
        {
            return QueueManager[m_QueueName].ExecuteCommand(commandName, command, param);
        }

        public void SetProperty(string propertyName, object propertyValue)
        {
            QueueManager[m_QueueName].SetProperty(propertyName, propertyValue);
        }
        public void ValidateCapacity()
        {
            QueueManager[m_QueueName].ValidateCapacity();
        }

        public void ValidateCapacity()
        {
            QueueManager[m_QueueName].ValidateCapacity();
        }

        #endregion

        #region Queue methods

        public string Dequeue()
        {
            IQueueItem item = QueueManager[m_QueueName].Dequeue();
            if (item == null)
                return null;
            return item.Serialize();
        }

        public void Enqueue(string queueName, string xmlItem)
        {
            QueueItem item = QueueItem.Deserialize(xmlItem);
            if (item != null)
            {
                QueueManager[m_QueueName].Enqueue(item as IQueueItem);
            }
        }

        public void Completed(string xmlItem, int status)
        {

            QueueItem item = QueueItem.Deserialize(xmlItem);
            if (item != null)
            {
                QueueManager[m_QueueName].Completed(item as IQueueItem, (ItemState)status);
            }

        }

        public void ReEnqueue(string xmlItem)
        {
            QueueItem item = QueueItem.Deserialize(xmlItem);
            if (item != null)
            {
                QueueManager[m_QueueName].ReEnqueue(item as IQueueItem);
            }
        }

        public void AbortTrans(string xmlItem)
        {
            QueueItem item = QueueItem.Deserialize(xmlItem);
            if (item != null)
            {
                QueueManager[m_QueueName].AbortTrans(item as IQueueItem);
            }

        }

        public void CommitTrans(string xmlItem)
        {
            QueueItem item = QueueItem.Deserialize(xmlItem);
            if (item != null)
            {
                QueueManager[m_QueueName].CommitTrans(item as IQueueItem);
            }

        }

        #endregion

        #region properties

        public bool IsTrans
        {
            get { return QueueManager[m_QueueName].IsTrans; }
        }

        public int Count
        {
            get { return QueueManager[m_QueueName].Count; }
        }
        public int MaxCapacity
        {
            get { return QueueManager[m_QueueName].MaxCapacity; }
        }

        public bool Initilaized
        {
            get { return QueueManager[m_QueueName].Initilaized; }
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

   
        //public void TransBegin(string queueName, string xmlItem)
        //{
        //    QueueItem item = QueueItem.Deserialize(xmlItem);
        //    if (item != null)
        //    {
        //        _QueueHandler[queueName].TransBegin(item as IQueueItem);
        //    }

        //}
     
  

    }


}

    
