using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Nistec.Legacy
{
 
    /// <summary>
    /// RemoteChannelManager
    /// </summary>
    public class RemoteChannelManager : NetUtils,IDisposable
    {

        #region Members

        static readonly RemoteChannelManager instance;
        bool initilized;
       
        private Thread ThreadSetting;
        //private IRemoteChannel[] asyncChannels;
        private int channelCount;
        private int checkSettingInterval = 60000;
        private Dictionary<string, IRemoteChannel> asyncChannels;
        
        #endregion

        #region Constructor

        static RemoteChannelManager()
        {
            instance = new RemoteChannelManager();
        }

        public RemoteChannelManager()
            : base()
        {
            asyncChannels = new Dictionary<string, IRemoteChannel>();
            Console.WriteLine("Init MainChannelManager");
        }

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

        ///// <summary>
        ///// Init Manager
        ///// </summary>
        ///// <param name="channels"></param>
        //public void Init(IRemoteChannel[] channels)
        //{

        //    if (channels == null || channels.Length == 0)
        //    {
        //        throw new Exception("Invalid Remote Channel");
        //    }
        //    if (RemoteChannelManager.initilized)
        //    {
        //        return;
        //    }

        //    asyncChannels = channels;
        //    channelCount = channels.Length;
        //}
        ///// <summary>
        ///// Init Manager
        ///// </summary>
        ///// <param name="props"></param>
        //public void Init(McChannelProperties[] props)
        //{
        //    if (props == null || props.Length == 0)
        //    {
        //        throw new Exception("Invalid Remote Channel properties");
        //    }
        //    if (RemoteChannelManager.initilized)
        //    {
        //        return;
        //    }

        //    asyncChannels = new IRemoteChannel[props.Length];
        //    for (int i = 0; i < props.Length; i++)
        //    {
        //        asyncChannels[i] = new RemoteChannel(props[i]);

        //        System.Threading.Thread.Sleep(20);
        //    }
        //}

        public virtual void Dispose()
        {
            if (initilized)
            {
                initilized = false;
                if (ThreadSetting != null)
                {
                    ThreadSetting.Abort();
                    ThreadSetting = null;
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

        #endregion

        #region properties

        public static RemoteChannelManager Instance
        {
            get
            {
                return instance;
            }
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

        public int CheckSettingInterval
        {
            get
            {
                return checkSettingInterval;
            }
            set
            {

                if (value < 100)
                {
                    throw new Exception("checkSettingInterval should be > 100");
                }
                checkSettingInterval = value;
            }
        }

  
        #endregion

        #region Methods

        //public static IRemoteChannel[] CreateRemoteChannels(McChannelProperties[] props)
        //{
        //    if (props == null || props.Length == 0 )
        //    {
        //        throw new Exception("Invalid Remote Channel properties");
        //    }

        //    IRemoteChannel[] asyncChannel = new IRemoteChannel[props.Length];
        //    for (int i = 0; i < props.Length; i++)
        //    {
        //        asyncChannel[i] = new RemoteChannel(props[i]);

        //        System.Threading.Thread.Sleep(20);
        //    }
        //    return asyncChannel;
        //}

   
        /// <summary>
        /// StartAsyncManager
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

            foreach (IRemoteChannel ch in asyncChannels.Values)
            {
                ch.StartAsyncQueue();
                //ch.MessageHandler += new Nistec.Util.MessageEventHandler(RemoteChannelManager_MessageHandler);
                //ch.ErrorOcurred += new Nistec.Util.ErrorOcurredEventHandler(RemoteChannelManager_ErrorOcurred);
                System.Threading.Thread.Sleep(20);
            }

            initilized = true;
            ThreadSetting = new Thread(new ThreadStart(OnCheckSetting));
            ThreadSetting.Start();

        }


        /// <summary>
        /// StopAsyncManager
        /// </summary>
        public void StopAsyncManager()//IRemoteChannel[] channels)
        {

            if (asyncChannels == null || asyncChannels.Count == 0)
            {
                //throw new Exception("Invalid Remote Channel");
                initilized = false;

                return;
            }
            if (!initilized)
            {
                return;
            }

            initilized = false;
            ThreadSetting.Abort();


            foreach (IRemoteChannel ch in asyncChannels.Values)
            {
                ch.StopAsyncQueue();//ch.AvailableThread);
                //ch.MessageHandler += new Nistec.Util.MessageEventHandler(RemoteChannelManager_MessageHandler);
                //ch.ErrorOcurred += new Nistec.Util.ErrorOcurredEventHandler(RemoteChannelManager_ErrorOcurred);
                System.Threading.Thread.Sleep(20);
            }
            Dispose();
        }

        /// <summary>
        /// OnCheckSetting
        /// </summary>
        protected virtual void OnCheckSetting()
        {
            while (initilized)
            {

                Thread.Sleep(checkSettingInterval);
            }
        }

        #endregion



    }
}


