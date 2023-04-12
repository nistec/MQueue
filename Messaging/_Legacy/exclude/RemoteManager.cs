using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Threading;
using System.Runtime.Remoting;  
using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Permissions;


namespace Nistec.Legacy
{


    [SecurityPermission(SecurityAction.Assert)]
    public class RemoteManager 
    {

		#region <Members>

        const string host = "ipc://portMQueue/RemoteQueueManager.rem";

        IRemoteManager manager;

		#endregion

		#region <Ctor>

        public RemoteManager()
         {

            try
            {
                manager = (IRemoteManager)Activator.GetObject
                (
                typeof(IRemoteManager),
                host
                );

                //Console.WriteLine(manager.GetType().ToString());
                //Console.WriteLine(manager.GetType().IsAssignableFrom(typeof(IRemoteQueueManager)));
                //Console.WriteLine(typeof(IRemoteQueueManager).IsAssignableFrom(manager.GetType()));

                if (manager == null)
                    Console.WriteLine("cannot locate remote queue manager");
                else
                {
                    Console.WriteLine(manager.Reply("Remote queue manager activated"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Remote queue manager Error: "+ex.Message);
                throw ex;
            }
         }

  
		#endregion

        #region static

        public static RemoteManager Client
        {
            get { return new RemoteManager(); }
        }

        public static RemoteQueue Get(string name)
        {
            return new RemoteQueue(name);
        }

        public static RemoteQueue Create(string name)
        {
            if (name == null)
                return null;
            if (RemoteManager.QueueExists(name))
            {
                return new RemoteQueue(name);
            }
           return Create( new McQueueProperties(name));
        }
     
        public static RemoteQueue Create(McQueueProperties prop)
        {
            if (prop == null)
                return null;
            if (RemoteManager.QueueExists(prop.QueueName))
            {
                return new RemoteQueue(prop.QueueName);
            }

            RemoteManager rqc = new RemoteManager();
  
            if (rqc != null)
            {
                rqc.manager.AddQueue(prop.ToDictionary());
                int count = 0;
                while (!(rqc.manager.Initilaized(prop.QueueName)))
                {
                    Thread.Sleep(100);
                    if (count > 5)
                        break;
                    count++;
                }
                rqc = null;
                return new RemoteQueue(prop.QueueName);
            }
            throw new ArgumentException("Invalid queue properties");
        }

        public static void AddQueue(McQueueProperties  prop)
        {
          RemoteManager.Client.manager.AddQueue(prop.ToDictionary());
        }

        public static void RemoveQueue(string queueName)
        {
            RemoteManager.Client.manager.RemoveQueue(queueName);
        }

        public static void ClearAllItems(string name)
        {
            RemoteManager.Client.manager.ClearAllItems(name);
        }

        public static bool QueueExists(string queueName)
        {
            return RemoteManager.Client.manager.QueueExists(queueName);
        }

        public static bool CanQueue(string queueName, uint count)
        {
            return RemoteManager.Client.manager.CanQueue(queueName, count);
        }


        public static string[] QueueList
        {
            get 
            {
                return RemoteManager.Client.manager.QueueList;
            }
        }

        public static DataTable GetStatistic()
        {
            return RemoteManager.Client.manager.GetStatistic();
        }

        //public static int ReEnqueueLog(string queueName)
        //{
        //    RemoteQueue rq = new RemoteQueue(queueName);
        //    return rq.ReEnqueueLog(queueName);
        //}
        //public static void TruncateDB()
        //{
        //    RemoteManager.Client.manager.TruncateDB();
        //}

        #endregion

        #region IRemoteQueueManager members

         public string Reply(string text)
         {
             return manager.Reply(text);
         }

        #endregion

    }
}

    
