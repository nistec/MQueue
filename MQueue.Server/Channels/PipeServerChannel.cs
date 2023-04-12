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

namespace Nistec.Messaging.Server
{
    /// <summary>
    /// Represent a queue Pipe server listner.
    /// </summary>
    public class PipeServerChannel : PipeServer<IQueueRequest>
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
            QLogger.Info("PipeServerChannel started :{0}, QueueChannel:{1}", PipeName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            AgentManager.StopController();
            QLogger.Info("PipeServerChannel stoped :{0}, QueueChannel:{1}", PipeName, QueueChannel.ToString());
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
        public PipeServerChannel(QueueChannel qChannel,string hostName)
            : base(QueueServerSettings.LoadPipeConfigServer(hostName))
        {
            QueueChannel = qChannel;
        }

        /// <summary>
        /// Constractor using <see cref="PipeSettings"/> settings.
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="settings"></param>
        public PipeServerChannel(QueueChannel qChannel, PipeSettings settings)
            : base(settings)
        {
            QueueChannel = qChannel;
        }

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="name"></param>
        /// <param name="loadFromSettings"></param>
        public PipeServerChannel(QueueChannel qChannel, string name, bool loadFromSettings)
            : base(name, loadFromSettings)
        {
            QueueChannel = qChannel;
        }
        #endregion

        #region abstract methods
        /// <summary>
        /// Execute client request and return response as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override TransStream ExecRequset(IQueueRequest message)
        {
            return AgentManager.Queue.ExecRequset(message);
        }
        /// <summary>
        /// ReadRequest
        /// </summary>
        /// <param name="pipeServer"></param>
        /// <returns></returns>
         protected override IQueueRequest ReadRequest(NamedPipeServerStream pipeServer)
        {

            if (QueueChannel == QueueChannel.Producer)
                return new QueueMessage(pipeServer, null);
            else
                return new QueueRequest(pipeServer);
        }
       

        #endregion
    }

}
