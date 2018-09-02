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


namespace Nistec.Messaging.Channels
{
    /// <summary>
    /// Represent a queue tcp server listner.
    /// </summary>
    public class TcpServerQueue : TcpServer<QItemStream>
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
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="hostName"></param>
        public TcpServerQueue(string hostName)
         {
             Settings = ServerQueueSettings.LoadTcpConfigServer(hostName);

        }

        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public TcpServerQueue(TcpSettings settings)
        //: base()
        {
            Settings = settings;

        }

        #endregion

        #region abstract methods
        /// <summary>
        /// Execute client request and return response as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override NetStream ExecRequset(QItemStream message)
        {
            return AgentManager.Queue.ExecRequset(message);
        }
        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected override QItemStream ReadRequest(NetworkStream stream)
        {
            return QItemStream.Create(stream);
        }
       
        #endregion
    }
}
