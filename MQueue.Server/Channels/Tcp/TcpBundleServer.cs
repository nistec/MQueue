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


namespace Nistec.Messaging.Server.Tcp
{
    /// <summary>
    /// Represent a queue tcp server listner.
    /// </summary>
    public class TcpServerChannel : TcpServer<IQueueMessage>
    {
        bool IsListener = false;

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
                AgentManager.Queue.Start();
            QLogger.InfoFormat("TcpServerChannel started :{0}", this.Settings.HostName);
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();

                AgentManager.Queue.Stop();
            QLogger.InfoFormat("TcpServerChannel stoped :{0}", this.Settings.HostName);
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
        /// <param name="hostName"></param>
        public TcpServerChannel(string hostName, bool isListener)
         {
             Settings = QueueServerSettings.LoadTcpConfigServer(hostName);
            IsListener = isListener;
        }

        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public TcpServerChannel(TcpSettings settings, bool isListener)
        //: base()
        {
            Settings = settings;
            IsListener = isListener;
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
        /// <returns></returns>
        protected override IQueueMessage ReadRequest(NetworkStream stream)
        {

            if(IsListener)
            {
                var req = new QueueRequest(stream);
                return req;
            }

            var item = new QueueItem(stream, null);
            return item;

        }
       
        #endregion
    }

}
