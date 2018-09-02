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


    internal class RemoteQueueManager : MarshalByRefObject, IRemoteQueueManager
    {

        internal static readonly Dictionary<string, McQueue> _QueueHandler= new Dictionary<string, McQueue>();



        public static McQueue Queue(string name)
        {

            try
            {
                //if (!_QueueHandler.ContainsKey(name))
                //{
                //    AddQueue(name);
                //}
                return _QueueHandler[name];
            }
            catch
            {
                throw new ArgumentException(name + " Not exists");
            }
        }

        private static void AddQueue(string name)
        {
            McQueueProperties mqp = new McQueueProperties(name);
            McQueue rq = McQueue.Create(mqp);
            _QueueHandler[mqp.QueueName] = rq;
        }

        //static RemoteQueueManager()
        //{
        //    try
        //    {
        //        _QueueHandler = new Dictionary<string, McQueue>();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public RemoteQueueManager()
        {

        }

        internal McQueue this[string queueName]
        {
            get
            {
                if (!_QueueHandler.ContainsKey(queueName))
                {
                    throw new ArgumentException(queueName + " Not exists");
                }
                return _QueueHandler[queueName];
            }
        }

        public bool Initilaized(string queueName)
        {
                return _QueueHandler[queueName].Initilaized;
        }

        //internal static void LoadQueues()
        //{

        //    McQueueProperties[] props = null;// McQueueProperties.CreateFromRegistry(ServiceConfig.Provider, ServiceConfig.ConnectionString);
        //    if (props == null)
        //        return;

        //    foreach (McQueueProperties p in props)
        //    {
        //        AddQueue(p);
        //    }
        // }

        public static DataTable QueueItems()
        {
            return McQueue.GetAllCoverItems();//ServiceConfig.Provider, ServiceConfig.ConnectionString);
        }

        public static void WriteXmlBackup()
        {
            try
            {
                string fileName = Environment.CurrentDirectory + "\\McQueue.xml";
                QueueItems().WriteXml(fileName);
            }
            catch { }
        }

        internal static void AddQueue(McQueueProperties prop)
        {
            if (_QueueHandler.ContainsKey(prop.QueueName))
            {
                return;
            }

            McQueue rq = McQueue.Create(prop);
            _QueueHandler[prop.QueueName] = rq;
        }


        public void AddQueue(IDictionary prop)
        {
            McQueueProperties mqp = McQueueProperties.Create(prop);
            AddQueue(prop);
        }

        public void RemoveQueue(string queueName)//IDictionary prop)
        {
            //McQueueProperties mqp = McQueueProperties.Create(prop);

            if (_QueueHandler.ContainsKey(queueName))//mqp.QueueName))
            {
                _QueueHandler.Remove(queueName);
                McQueue.Delete(queueName);
                //mqp.RemoveQueue();
            }

            //if (_QueueHandler.ContainsKey(name))
            //{
            //    //this[name].RemoveQueue(name);
            //    _QueueHandler.Remove(name);

            //    McQueueProperties.RemoveFromRegistry(name);
            //}
        }

        public bool QueueExists(string name)
        {
            return _QueueHandler.ContainsKey(name);
        }

        public string[] QueueList
        {
            get
            {
                if (_QueueHandler.Count == 0)
                {
                    return null;
                }
                string[] list = new string[_QueueHandler.Count];
                int index = 0;
                foreach (string k in _QueueHandler.Keys)
                {
                    list[index] = k;
                    index++;
                }

                return list;

            }
        }

 
        public string Reply(string text)
        {
            return text;
        }
        
        public object ExecuteCommand(string queueName, string commandName, string command, params string[] param)
        {
           return this[queueName].ExecuteCommand(commandName, command,  param);
        }

        public void SetProperty(string queueName, string propertyName, object propertyValue)
        {
            this[queueName].SetProperty(propertyName,propertyValue);
        }
        public void ValidateCapacity(string queueName)
        {
            this[queueName].ValidateCapacity();
        }


        public DataTable GetQueueItemsTable(string queueName)
        {
          return  this[queueName].GetQueueItemsTable();
        }
    }


}

    
