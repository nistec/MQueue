using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MControl.Threading;

namespace MControl.Messaging
{
 
    /// <summary>
    /// RemoteChannelManager
    /// </summary>
    public class RemoteChannels : NetComponent//, IDisposable
    {

        #region Members

        bool initilized;
       
        private Thread thSetting;
        private int channelCount;
        private int m_IntervalSetting = 60000;
        private Dictionary<string, IRemoteChannel> asyncChannels;

        private GenericThreadPool mainThreads;

        private int m_MaxThread = 1;
        private int m_MinThread = 1;
        private int m_AvailableThread = 1;
        private bool m_AutoThreadSettings = false;
        private bool m_UseThreadSettings = false;
        private TimeSpan m_RecieveTimeout;
        internal int m_DequeueWait = 10;

        
        #endregion

        #region Constructor

        public RemoteChannels(McThredSettings prop)
            : base()
        {
            asyncChannels = new Dictionary<string, IRemoteChannel>();

            m_MinThread = prop.MinThread;
            m_MaxThread = prop.MaxThread;
            m_AvailableThread = prop.AvailableThread;
            m_UseThreadSettings = prop.UseThreadSettings;
            m_RecieveTimeout = TimeSpan.FromSeconds(prop.RecieveTimeout);
            m_IntervalSetting = prop.IntervalSetting;
            Console.WriteLine("Init RemoteChannel");

        }

        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {

                if (initilized)
                {
                    initilized = false;
                    if (thSetting != null)
                    {
                        thSetting.Abort();
                        thSetting = null;
                    }
                    if (mainThreads != null)
                    {
                        mainThreads.Dispose();
                        mainThreads = null;
                    }
                    if (asyncChannels != null)
                    {
                        foreach (IRemoteChannel ch in asyncChannels.Values)
                        {
                            ch.Dispose();
                        }
                        asyncChannels = null;
                    }
                }
            }
            base.Dispose(disposing);
        }
     
        #endregion

        #region Channels collection methods
        /// <summary>
        /// Add Channel
        /// </summary>
        /// <param name="prop"></param>
        public void AddChannel(McChannelProperties prop)
        {
            if (prop == null || !prop.IsValid())
            {
                throw new Exception("Invalid Remote Channel properties");
            }
            if (initilized)
            {
                return;
            }

            AddChannel(new RemoteChannel(prop));
        }

        /// <summary>
        /// Add Channel
        /// </summary>
        /// <param name="props"></param>
        public void AddChannel(McChannelProperties[] props)
        {
            if (props == null || props.Length == 0)
            {
                throw new Exception("Invalid Remote Channel properties");
            }
            if (initilized)
            {
                return;
            }
            foreach (McChannelProperties prop in props)
            {
                AddChannel(prop);
            }
        }

        /// <summary>
        /// Add Channel Range
        /// </summary>
        /// <param name="channel"></param>
        public void AddChannel(IRemoteChannel channel)
        {
            if (channel == null)
            {
                throw new Exception("Invalid channel");
            }
            if (initilized)
            {
                return;
            }
            if (!asyncChannels.ContainsKey(channel.QueueName))
            {
                asyncChannels.Add(channel.QueueName, channel);
                channelCount = asyncChannels.Count;
            }
        }

        /// <summary>
        /// Add Channel Range
        /// </summary>
        /// <param name="channels"></param>
        public void AddChannel(IRemoteChannel[] channels)
        {
            if (channels == null)
            {
                throw new Exception("Invalid channels");
            }
            if (initilized)
            {
                return;
            }
            foreach (IRemoteChannel channel in channels)
            {
                AddChannel(channel);
            }
        }

        /// <summary>
        /// Remove Channel
        /// </summary>
        /// <param name="channelName"></param>
        public void RemoveChannel(string channelName)
        {
            if (string.IsNullOrEmpty(channelName))
            {
               return;
            }
            if (initilized)
            {
                return;
            }
            if (!asyncChannels.ContainsKey(channelName))
            {
                asyncChannels.Remove(channelName);
                channelCount = asyncChannels.Count;
            }
        }

        /// <summary>
        /// Clear Channel
        /// </summary>
        /// <param name="channelName"></param>
        public void ClearChannel()
        {
            if (asyncChannels == null)
            {
                return;
            }
            if (initilized)
            {
                return;
            }
            asyncChannels.Clear();
            channelCount = 0;
        }

   
     

        #endregion

        #region properties

        /// <summary>
        /// Get MaxThread
        /// </summary>
        public int MaxThread
        {
            get { return m_MaxThread; }
        }

        /// <summary>
        /// Get MinThread
        /// </summary>
        public int MinThread
        {
            get { return m_MinThread; }
        }

        /// <summary>
        /// Get AvailableThread
        /// </summary>
        public int AvailableThread
        {
            get { return m_AvailableThread; }
        }

        public bool Initilize
        {
            get
            {
                return initilized;
            }
        }

        public int ChannelCount
        {
            get
            {
                return channelCount;
            }
        }

        public int IntervalSetting
        {
            get
            {
                return m_IntervalSetting;
            }
            set
            {

                if (value < 100)
                {
                    throw new Exception("IntervalSetting should be > 100");
                }
                IntervalSetting = value;
            }
        }

  
        #endregion

        #region Methods

    
        /// <summary>
        /// Start AsyncQueue Background multi thread Listner 
        /// </summary>
        public void StartAsyncManager()
        {

            if (asyncChannels == null || asyncChannels.Count == 0)
            {
                throw new Exception("Invalid Remote Channel");
            }
            if (initilized)
            {
                return;
            }

            Console.WriteLine("Create AsyncManager...");
            try
            {
                if (m_AutoThreadSettings)
                    mainThreads = new GenericThreadPool(/*"RemoteChannels",*/ m_MinThread, m_MaxThread);//, m_AvailableThread);
                else
                    mainThreads = new GenericThreadPool(/*"RemoteChannels",*/ m_MaxThread);
                //mainThreads.AutoThreadSettings = m_AutoThreadSettings;
                initilized = true;
                mainThreads.StartThreadPool(new ThreadStart(MessageQueueListner));//, m_MaxThread);

                if (m_UseThreadSettings)
                {
                    thSetting = new Thread(new ThreadStart(OnCheckSetting));
                    thSetting.IsBackground = true;
                    thSetting.Start();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("RemoteChannels Error:" + ex.Message);

            }
        }


        /// <summary>
        /// Stop AsyncQueue Background multi thread Listner 
        /// </summary>
        public void StopAsyncManager()
        {
            Console.WriteLine("Stop RemoteChannels...");

            if (!initilized)
            {
                return;
            }

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

        /// <summary>
        /// OnCheckSetting
        /// </summary>
        protected virtual void OnCheckSetting()
        {
            while (initilized)
            {

                Thread.Sleep(m_IntervalSetting);
            }
        }

        #endregion

     #region Listners

        /// <summary>
        /// Message Queue Listner worker thread
        /// </summary>
        private void MessageQueueListner()
        {
            Console.WriteLine("Create RemoteChannels Listner...");

            Thread.Sleep(6000);

            while (initilized)
            {
               
                //mainThreads.ManualReset(true);
                
                foreach (IRemoteChannel ch in asyncChannels.Values)
                {
                    lock (ch)
                    {
                        if (ch.HoldDequeue)
                        {
                            continue;
                        }
                        object state = new object();
                        ch.BeginReceive(m_RecieveTimeout, state);
                        System.Threading.Thread.Sleep(20);
                    }
                }
                //mainThreads.ManualReset(false);

                Thread.Sleep(m_DequeueWait);
            }
        }


		#endregion


    }
}


