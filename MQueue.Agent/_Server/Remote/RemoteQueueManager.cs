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


    internal class RemoteQueueManager : MarshalByRefObject, IRemoteManager
    {

        public RemoteQueueManager()
        {

        }


        public string Reply(string text)
        {
            return text;
        }

        public bool Initilaized(string queueName)
        {
            return RemoteQueueServer.QueueManager[queueName].Initilaized;
        }

        public DataTable GetStatistic()
        {
            return RemoteQueueServer.QueueManager.GetStatistic();
        }

        //public static DataTable GetAllCoverItems()
        //{
        //    //return McQueue.GetAllCoverItems();//ServiceConfig.Provider, ServiceConfig.ConnectionString);
        //    return RemoteQueueServer.QueueManager.GetAllCoverItems();
        //}

        //public void TruncateDB()
        //{
        //     RemoteQueueServer.QueueManager.TruncateDB();
        //}

        public static void WriteXmlBackup()
        {
            try
            {
                //string fileName = Environment.CurrentDirectory + "\\McQueue.xml";
                //GetAllCoverItems().WriteXml(fileName);
            }
            catch { }
        }

        #region static methods

        public static void AddQueue(string name)
        {
            //McQueueProperties prop = new McQueueProperties(name);
            //McQueue rq = McQueue.Create(prop);
            //RemoteQueueServer.QueueManager[prop.QueueName] = rq;
            RemoteQueueServer.QueueManager.Create(name,McLock.Lock.ValidateLock());
        }

        public void AddQueue(IDictionary prop)
        {
            AddQueue(McQueueProperties.Create(prop));
        }

        public static void AddQueue(McQueueProperties prop)
        {
            //McQueue rq = McQueue.Create(prop);
            //RemoteQueueServer.QueueManager[prop.QueueName] = rq;
            RemoteQueueServer.QueueManager.Create(prop,McLock.Lock.ValidateLock());
        }

        public void RemoveQueue(string name)
        {
            RemoveQueueInternal(name);
        }
      
        public static void RemoveQueueInternal(string name)
        {
            if (RemoteQueueServer.QueueManager.ContainsKey(name))
            {
                //TODO:DELETE FROM DB
                RemoteQueueServer.QueueManager.Remove(name);
                //McQueue.Delete(queueName);
            }
        }

        public void ClearAllItems(string name)
        {
            McQueue q = RemoteQueueServer.QueueManager[name];
            if (q != null)
            {
                q.ClearQueueItems(QueueItemType.AllItems);
            }
        }

        public string[] QueueList
        {
            get { return QueueListInternal; }
        }

        public static string[] QueueListInternal
        {
            get
            {
                return RemoteQueueServer.QueueManager.GetQueueList();
                //if (RemoteQueueServer.QueueManager.Count == 0)
                //{
                //    return null;
                //}
                //string[] list = new string[RemoteQueueServer.QueueManager.Count];
                //int index = 0;
                //foreach (string k in RemoteQueueServer.QueueManager.Keys)
                //{
                //    list[index] = k;
                //    index++;
                //}
                //return list;
            }
        }

        public bool QueueExists(string name)
        {
            return QueueExistsInternal(name);
        }

        public bool CanQueue(string queueName, uint count)
        {
            McQueue Q = RemoteQueueServer.QueueManager[queueName];
            if (Q == null)
            {
                return false; 
            }
            return Q.CanQueue(count);
        }

        public static bool QueueExistsInternal(string name)
        {
            return RemoteQueueServer.QueueManager.ContainsKey(name);
        }

       
        #endregion

    }


}

    
