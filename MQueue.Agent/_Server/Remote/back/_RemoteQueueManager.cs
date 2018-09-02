using System;
using System.Collections.Generic;
using System.Text;
using MControl.Threading;
using System.Collections;
using MControl.Util;

namespace MControl.Messaging.Service
{

    internal class RemoteQueueServer : MarshalByRefObject, IRemoteQueue
    {
 
        private static Dictionary<string, McQueue> _QueueHandler;

        ///// <summary>
        ///// ErrorOcurred
        ///// </summary>
        //public event ErrorOcurredEventHandler ErrorOcurred;
        ///// <summary>
        ///// MessageHandler
        ///// </summary>
        //public event QueueItemEventHandler ReceiveCompleted;


       
        static RemoteQueueServer()
        {
            try
            {
                _QueueHandler = new Dictionary<string, McQueue>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RemoteQueueServer()
        {
           
        }

        public static RemoteQueueServer Instance
        {
            get 
            {

                return new RemoteQueueServer(); 
            }
        }

        public McQueue AddQueue(McQueueProperties mqp)
        {
            if (_QueueHandler.ContainsKey(mqp.QueueName))
            {
                //throw new ArgumentException(aqt.QueueName + " allready exists");
                return _QueueHandler[mqp.QueueName];
            }

            McQueue rq = new McQueue(mqp);
            //rq.ErrorOcurred += new ErrorOcurredEventHandler(rq_ErrorOcurred);
            //rq.ReceiveCompleted += new QueueItemEventHandler(rq_ReceiveCompleted);
            _QueueHandler[mqp.QueueName] = rq;

            return rq;
        }

        public void RemoveQueue(string name)
        {
            if (_QueueHandler.ContainsKey(name))
            {
                this[name].RemoveQueue(name);
                _QueueHandler.Remove(name);
            }
        }

        public string[] QueueList
        {
            get
            {
                if (_QueueHandler.Count==0)
                {
                    return null;
                }
                string[] list=new string[_QueueHandler.Count];
                int index = 0;
                foreach (string k in _QueueHandler.Keys)
                {
                    list[index] = k;
                    index++;
                }

                return list;

            }
        }

        public string Test
        {
            get { return "Remote Queue Manager"; }
        }

        //void rq_ReceiveCompleted(object sender, QueueItemEventArgs e)
        //{
        //    if (ReceiveCompleted != null)
        //        this.ReceiveCompleted(this, e);
        //}

        //void rq_ErrorOcurred(object sender, ErrorOcurredEventArgs e)
        //{
        //    if (ErrorOcurred != null)
        //        this.ErrorOcurred(this, e);
        //}

  
        public McQueue this[string queueName]
        {
            get
            {
            if(!_QueueHandler.ContainsKey(queueName))
            {
                throw new ArgumentException(queueName + " Not exists");
            }
                return _QueueHandler[queueName];
            }
        }

  


        //public void SetConfig(ManagerConfig config)
        //{
        //    ChannelMode = config.ChannelMode;
        //    IntervalSetting = config.IntervalSetting;
        //    IntervalCacheRefresh = config.IntervalCacheRefresh;
        //}

 
 
    }

}
