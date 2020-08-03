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
using Nistec.Messaging.Config;
using Nistec.Logging;

namespace Nistec.Messaging.Server
{
    /// <summary>
    /// Represent a queue tcp server listner.
    /// </summary>
    public class TcpServerChannel : TcpServer<IQueueMessage>
    {
        QueueChannel QueueChannel;

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            AgentManager.StartController();
            Log.Info("TcpServerChannel started :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            AgentManager.StopController();
            Log.Info("TcpServerChannel stoped :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
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
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="hostName"></param>
        public TcpServerChannel(QueueChannel qChannel, string hostName)
         {
            Settings = QueueServerSettings.LoadTcpConfigServer(hostName);
            QueueChannel = qChannel;
        }

        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="settings"></param>
        public TcpServerChannel(QueueChannel qChannel, TcpSettings settings)
        //: base()
        {
            Settings = settings;
            QueueChannel = qChannel;
        }

        #endregion

        #region abstract methods
        /// <summary>
        /// Execute client request and return response as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override TransStream ExecRequset(IQueueMessage message)
        {
            return AgentManager.Queue.ExecRequset(message);
        }
        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="readTimeout"></param>
        /// <returns></returns>
        protected override IQueueMessage ReadRequest(NetworkStream stream, int readTimeout, int ReceiveBufferSize)
        {
            //IQueueMessage message = null;
            //using (var ntStream = new NetStream())
            //{
            //    ntStream.CopyFrom(stream, readTimeout, ReceiveBufferSize);

            //    if (QueueChannel == QueueChannel.Producer)
            //        message= new QueueItem(stream, null);
            //    else
            //        message= new QueueRequest(stream);
            //}
            //return message;

            if (QueueChannel == QueueChannel.Producer)
                return new QueueItem(stream, null);
            else
                return new QueueRequest(stream);
        }
       
        #endregion
    }

}
