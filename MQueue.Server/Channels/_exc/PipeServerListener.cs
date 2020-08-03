using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using Nistec.Channels;
using Nistec.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using Nistec.Messaging.Config;
using Nistec.Messaging;

namespace Nistec.Messaging.Server.Pipe
{
    /// <summary>
    /// Represent a queue Pipe server listner.
    /// </summary>
    public class PipeServerListener : PipeServer<IQueueMessage>
    {
        bool IsListener = false;

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            AgentManager.StartController();
            QLogger.InfoFormat("PipeServerListener started :{0}", PipeName);
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            AgentManager.StopController();
            QLogger.InfoFormat("PipeServerListener stoped :{0}", PipeName);
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
        public PipeServerListener(string hostName)
            : base(QueueServerSettings.LoadPipeConfigServer(hostName))
        {
        }

        /// <summary>
        /// Constractor using <see cref="PipeSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public PipeServerListener(PipeSettings settings)
            : base(settings)
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
        /// ReadRequest
        /// </summary>
        /// <param name="pipeServer"></param>
        /// <returns></returns>
        protected override IQueueMessage ReadRequest(NamedPipeServerStream pipeServer)
        {
            var req = new QueueRequest(pipeServer);
            return req;
        }
       

        #endregion
    }

}
