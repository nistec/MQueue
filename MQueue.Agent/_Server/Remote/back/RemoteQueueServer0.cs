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

        //private static RemoteQueueManager _QueueHandler;


        //static RemoteQueueServer()
        //{
        //    try
        //    {
        //        _QueueHandler = new RemoteQueueManager();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public RemoteQueueServer()
        {

        }


        //public void AddQueue(IDictionary prop)
        //{
        //    McQueueProperties mqp = McQueueProperties.Create(prop);

        //    if (_QueueHandler.ContainsKey(mqp.QueueName))
        //    {
        //        //throw new ArgumentException(aqt.QueueName + " allready exists");
        //        return;// _QueueHandler[mqp.QueueName];
        //    }
        //    mqp.Mode = QueueMode.Manual;
        //    McQueue rq = new McQueue(mqp);
        //    //rq.ErrorOcurred += new ErrorOcurredEventHandler(rq_ErrorOcurred);
        //    //rq.ReceiveCompleted += new QueueItemEventHandler(rq_ReceiveCompleted);
        //    _QueueHandler[mqp.QueueName] = rq;

        //   //return rq as IAsyncQueue;
        //}

        //public void RemoveQueue(string name)
        //{
        //    if (_QueueHandler.ContainsKey(name))
        //    {
        //        //this[name].RemoveQueue(name);
        //        _QueueHandler.Remove(name);
        //    }
        //}

        //public string[] QueueList
        //{
        //    get
        //    {
        //        if (_QueueHandler.Count == 0)
        //        {
        //            return null;
        //        }
        //        string[] list = new string[_QueueHandler.Count];
        //        int index = 0;
        //        foreach (string k in _QueueHandler.Keys)
        //        {
        //            list[index] = k;
        //            index++;
        //        }

        //        return list;

        //    }
        //}

        public string Reply(string text)
        {
            return text; 
        }

        public bool IsTrans(string queueName)
        {
            return RemoteQueueManager.Queue(queueName).IsTrans; 
        }
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

        public string Dequeue(string queueName)
        {
            IQueueItem item = RemoteQueueManager.Queue(queueName).Dequeue();
            if (item == null)
                return null;
            return item.Serialize();
        }

        public void Enqueue(string queueName, string xmlItem)
        {
            QueueItem item = QueueItem.Deserialize(xmlItem);
            if (item != null)
            {
                RemoteQueueManager.Queue(queueName).Enqueue(item as IQueueItem);
            }
        }

        public void Completed(string queueName, string xmlItem, int status)
        {

            QueueItem item = QueueItem.Deserialize(xmlItem);
            if (item != null)
            {
                RemoteQueueManager.Queue(queueName).Completed(item as IQueueItem, (ItemState)status);
            }

        }

        public void ReEnqueue(string queueName, string xmlItem)
        {
            QueueItem item = QueueItem.Deserialize(xmlItem);
            if (item != null)
            {
                RemoteQueueManager.Queue(queueName).ReEnqueue(item as IQueueItem);
            }
        }

        public void AbortTrans(string queueName, string xmlItem)
        {
            QueueItem item = QueueItem.Deserialize(xmlItem);
            if (item != null)
            {
                RemoteQueueManager.Queue(queueName).AbortTrans(item as IQueueItem);
            }

        }

        public void CommitTrans(string queueName, string xmlItem)
        {
            QueueItem item = QueueItem.Deserialize(xmlItem);
            if (item != null)
            {
                RemoteQueueManager.Queue(queueName).CommitTrans(item as IQueueItem);
            }

        }
        //public void TransBegin(string queueName, string xmlItem)
        //{
        //    QueueItem item = QueueItem.Deserialize(xmlItem);
        //    if (item != null)
        //    {
        //        _QueueHandler[queueName].TransBegin(item as IQueueItem);
        //    }

        //}
        public int Count(string queueName)
        {
            return RemoteQueueManager.Queue(queueName).Count;
        }
        public int MaxCapacity(string queueName)
        {
            return RemoteQueueManager.Queue(queueName).MaxCapacity;
        }
        public DataTable GetQueueItemsTable(string queueName)
        {
            return RemoteQueueManager.Queue(queueName).GetQueueItemsTable();
        }
        public IQueueItem[] GetQueueItems(string queueName)
        {
            return RemoteQueueManager.Queue(queueName).GetQueueItems();
        }
        public object ExecuteCommand(string queueName, string commandName, string command, params string[] param)
        {
            return RemoteQueueManager.Queue(queueName).ExecuteCommand(commandName, command, param);
        }

        public void SetProperty(string queueName, string propertyName, object propertyValue)
        {
            RemoteQueueManager.Queue(queueName).SetProperty(propertyName, propertyValue);
        }
        public void ValidateCapacity(string queueName)
        {
            RemoteQueueManager.Queue(queueName).ValidateCapacity();
        }

    }


}

    
