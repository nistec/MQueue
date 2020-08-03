using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using Nistec.Channels;
using Nistec.IO;
using System.Threading.Tasks;
using Nistec.Channels.Tcp;
using System.Net.Sockets;
using Nistec.Messaging.Listeners;
using Nistec.Logging;
//using Nistec.Messaging.Config;


namespace Nistec.Messaging.Channels
{
    /// <summary>
    /// Represent a queue tcp server listner.
    /// </summary>
    public class TcpServerQueue : TcpServer<IQueueItem>, IChannelService
    {
        QueueChannel QueueChannel = QueueChannel.Consumer;
        IControllerHandler Controller;

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            Log.Info("TcpServerQueue started :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            Log.Info("TcpServerQueue stoped :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnLoad
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
        }
        #endregion

        #region ctor

        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="controller"></param>
        public TcpServerQueue(TcpSettings settings, IControllerHandler controller)
        {
            Settings = settings;
            Controller = controller;
        }

        #endregion

        #region abstract methods
        /// <summary>
        /// Execute client request and return response as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override TransStream ExecRequset(IQueueItem message)
        {
            return Controller.OnMessageReceived(message);
        }
        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="readTimeout"></param>
        /// <returns></returns>
        protected override IQueueItem ReadRequest(NetworkStream stream, int readTimeout,int ReceiveBufferSize = TcpSettings.DefaultReceiveBufferSize)
        {
            //IQueueItem item = null;
            //using (var ntStream = new NetStream())
            //{
            //    ntStream.CopyFrom(stream, readTimeout, ReceiveBufferSize);

            //    item =new QueueItem(stream, null);
            //}
            //return item;

            return new QueueItem(stream, null);
        }
       
        #endregion
    }

}
