

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Messaging;

using MControl.Threading;
using MControl.Data.SqlClient;
using MControl.Data;
using System.Data;
using System.Data.SqlClient;
using MControl.Runtime;
using MControl.Win;
//using MControl.Messaging.Firebird;


namespace MControl.Messaging
{

 
    /// <summary>
    /// AsyncQueueHandlerBase
    /// </summary>
    [Serializable]
    public class RemoteChannel : RemoteQueue, IRemoteChannel
    {
	
		#region Members

        internal bool initilized=false;
        private Thread thSetting;
        private GenericThreadPool mainThreads;

        private int m_MaxThread = 1;
        private int m_MinThread = 1;
        private int m_AvailableThread = 1;
        private bool m_AutoThreadSettings = false;
        private bool m_UseThreadSettings = false;
        private TimeSpan m_RecieveTimeout;
        internal int m_DequeueWait = 10;
        private int m_IntervalSetting = 600000;
        //string m_ChannelName;

		#endregion

		#region Constructor
  
        /// <summary>
        /// RemoteChannel ctor
        /// </summary>
        /// <param name="prop"></param>
        public RemoteChannel(McChannelProperties prop)
            : base(prop.QueueName, prop.IsTrans)
        {
            m_MinThread = prop.MinThread;
            m_MaxThread = prop.MaxThread;
            m_AvailableThread = prop.AvailableThread;
            m_UseThreadSettings = prop.UseThreadSettings;
            m_IntervalSetting = prop.IntervalSetting;
            m_UseMessageQueueListner = prop.UseMessageQueueListner;
            //m_ChannelName = prop.ChannelName;
            m_RecieveTimeout = TimeSpan.FromSeconds(prop.RecieveTimeout);
            base.MaxCapacity = prop.MaxCapacity;
            RemoteManager.AddQueue(prop.QueueProperties);
            Console.WriteLine("Init RemoteChannel");
           
        }

        ~RemoteChannel()
        {
           Dispose(false);
        }
        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {

                if (thSetting != null)
                {
                    thSetting.Abort();
                    thSetting = null;
                }

                if (disposing)
                {
                    if (mainThreads != null)
                    {
                        mainThreads.Dispose();
                        mainThreads = null;
                    }
                }
            }
            disposed = true;
            base.Dispose(disposing);
        }

        #endregion

        #region MainThread

        /// <summary>
        /// Start AsyncQueue Background multi thread Listner 
        /// </summary>
        public void StartAsyncQueue()
        {
 
            //m_Started = true;

            if (! Enabled || initilized)
                return;

            Console.WriteLine("Create AsyncQueue " + QueueName);
            try
            {
                if (m_AutoThreadSettings)
                    mainThreads = new GenericThreadPool(/*QueueName,*/ m_MinThread, m_MaxThread);//, m_AvailableThread);
                else
                    mainThreads = new GenericThreadPool(/*QueueName,*/ m_MaxThread);
                //mainThreads.AutoThreadSettings = m_AutoThreadSettings;
                initilized = true;
                if (m_UseMessageQueueListner)
                {
                    mainThreads.StartThreadPool(new ThreadStart(MessageQueueListner));//, MaxThread);
                }
                else
                {
                    mainThreads.StartThreadPool(new ThreadStart(AfterReceiveCompleted));//, MaxThread);
                }
                if (m_UseThreadSettings)
                {
                    thSetting = new Thread(new ThreadStart(ThreadSettings));
                    thSetting.IsBackground = true;
                    thSetting.Start();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(QueueName + " Error:" + ex.Message);

            }
        }
  

        /// <summary>
        /// Stop AsyncQueue Background multi thread Listner 
        /// </summary>
        public void StopAsyncQueue()
		{
            Console.WriteLine("Stop AsyncQueue " + QueueName);
          
            initilized = false;
            mainThreads.StopThreadPool();
			mainThreads = null;
            if (m_UseThreadSettings)// m_CoverMode > CoverMode.None)
            {
                thSetting.Abort();
                thSetting = null;
                //thFinall.Abort();
                //thFinall = null;
            }
			//GC.Collect();
			//GC.WaitForPendingFinalizers();
		}

		#endregion

        #region properties

        ///// <summary>
        ///// Get QueueName name
        ///// </summary>
        //public string QueueName
        //{
        //    get
        //    {
        //        return m_QueueName;
        //    }
        //}

        /// <summary>
        /// IsRemote
        /// </summary>
        public bool IsRemote
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Initilize
        /// </summary>
        public bool Initilize
        {
            get
            {
                return initilized;
            }
        }
    

        /// <summary>
        /// Get MaxThread
        /// </summary>
        public int MaxThread
        {
            get { return m_MaxThread; }
        }

    
        /// <summary>
        /// Dequeue Wait interval
        /// </summary>
        public virtual int DequeueWait
        {
            get { return m_DequeueWait; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("required more then 0");
                }
                if (m_DequeueWait != value)
                {
                    m_DequeueWait = value;
                    //_RemoteQueue.SetProperty("DequeueWait", m_DequeueWait);
                }
            }
        }

        /// <summary>
        /// Get or Set wether the queue Use Background MessageQueue Listner
        /// </summary>
        public virtual bool UseMessageQueueListner
        {
            get
            {
                return m_UseMessageQueueListner;
            }
            set
            {
                if (m_UseMessageQueueListner != value)
                {
                    m_UseMessageQueueListner = value;
                    //OnPropertyChanged("UseThreadSettings", useThreadSettings);
                }
            }
        }
        #endregion

        #region Listners

        /// <summary>
        /// After Receive Completed Start Begin Receive Automaticly
        /// </summary>
        protected void AfterReceiveCompleted()
        {
            //mainThreads.ManualReset(true);
            base.BeginReceive(m_RecieveTimeout, null);
        }

       /// <summary>
        /// After Receive Completed Start Begin Receive 
       /// </summary>
       /// <param name="state"></param>
        protected void AfterReceiveCompleted(object state)
        {
            //mainThreads.ManualReset(true);
            base.BeginReceive(m_RecieveTimeout, state);
        }

        /// <summary>
        /// Message Queue Listner worker thread
        /// </summary>
        private void MessageQueueListner()
        {
            Console.WriteLine("Create MessageQueueListner..." + QueueName);

            //Thread.Sleep(6000);

            while (initilized)
            {
                if (HoldDequeue)
                {
                    Thread.Sleep(m_DequeueWait);
                }

                //object state = new object();
                //mainThreads.ManualReset(true);
                try
                {
                    Dequeue();

                    //IQueueItem item = Dequeue();
                    //if (item != null)
                    //{
                    //    OnMessageReceived(new QueueItemEventArgs(item, ItemState.Dequeue));
                    //}
                }
                catch( Exception ex) 
                {
                    OnErrorOcurred(new ErrorOcurredEventArgs(ex.Message));
                }
                //BeginReceive(m_RecieveTimeout, null);
                //mainThreads.ManualReset(false);
                
                Thread.Sleep(m_DequeueWait);
            }
        }


         private void ThreadSettings()
        {

            Console.WriteLine("Create ThreadSettings..." + QueueName);

            while (initilized)
            {
                OnSettings();
                Thread.Sleep(m_IntervalSetting);
            }
        }

        protected virtual void OnSettings()
        {
            ValidateCapacity();
        }
 
   

		#endregion

       

        /// <summary>
        /// FormatMessage
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public string FormatMessage(IQueueItem m)
        {
            return System.Web.HttpUtility.UrlDecode(m.Body.ToString(), new System.Text.UTF8Encoding());
        }
        /// <summary>
        /// FormatMessage
        /// </summary>
        /// <param name="m"></param>
        /// <param name="en"></param>
        /// <returns></returns>
        public string FormatMessage(IQueueItem m, Encoding en)
        {
            return System.Web.HttpUtility.UrlDecode(m.Body.ToString(), en);
        }
    
    }
}


