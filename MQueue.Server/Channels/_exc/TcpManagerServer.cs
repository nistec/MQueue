using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using Nistec.Channels;
using Nistec.IO;
using Nistec.Channels.Tcp;
using System.Net.Sockets;
using Nistec.Messaging.Config;

namespace Nistec.Messaging.Server.Tcp
{
    /// <summary>
    /// Represent a queue managment tcp server listner.
    /// </summary>
    public class TcpManagerServer : TcpServer<IQueueMessage>
    {

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            AgentManager.Queue.Start();
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();

            AgentManager.Queue.Stop();
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
        /// Constractor default
        /// </summary>
        public TcpManagerServer()
            : base()
        {
            //LoadRemoteQueue();
        }

       
        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="hostName"></param>
        public TcpManagerServer(string hostName)
            : base()
        {
            Settings = QueueServerSettings.LoadTcpConfigServer(hostName);
        }
        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public TcpManagerServer(TcpSettings settings)
            :base(settings)
        {
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
            var req = new QueueRequest(stream);
            return req;
        }

        #endregion

    }
}
