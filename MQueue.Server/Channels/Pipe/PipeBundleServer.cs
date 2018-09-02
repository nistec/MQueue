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
    public class PipeServerChannel : PipeServer<IQueueMessage>
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
            QLogger.InfoFormat("PipeServerChannel started :{0}", PipeName);
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            AgentManager.Queue.Stop();
            QLogger.InfoFormat("PipeServerChannel stoped :{0}", PipeName);
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
        public PipeServerChannel(string hostName, bool isListener)
            : base(QueueServerSettings.LoadPipeConfigServer(hostName))
        {
            IsListener = isListener;

        }

        /// <summary>
        /// Constractor using <see cref="PipeSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public PipeServerChannel(PipeSettings settings, bool isListener)
            : base(settings)
        {

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
        /// ReadRequest
        /// </summary>
        /// <param name="pipeServer"></param>
        /// <returns></returns>
         protected override IQueueMessage ReadRequest(NamedPipeServerStream pipeServer)
        {

            if (IsListener)
            {
                var req = new QueueRequest(pipeServer);
                return req;
            }

            var item = new QueueItem(pipeServer,null);
            return item;
        }
       

        #endregion
    }

}
