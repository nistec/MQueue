using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using Nistec.Channels;
using Nistec.IO;
using Nistec.Runtime;

namespace Nistec.Messaging.Server.Pipe
{
    /// <summary>
    /// Represent a queue managment pipe server listner.
    /// </summary>
    public class PipeManagerServer : PipeServer<IQueueMessage>//PipeServerBase
    {

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            //AgentManager.Queue.Start();
            QLogger.InfoFormat("PipeServerManager started :{0}", PipeName);
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            QLogger.InfoFormat("PipeServerManager stoped :{0}", PipeName);

            //AgentManager.Queue.Stop();
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


        static PipeSettings GetSettings()
        {
            return new PipeSettings()
            {
                HostName = "nistec_queue_manager",
                ConnectTimeout = 5000,
                ReceiveBufferSize = 8192,
                MaxAllowedServerInstances = 255,
                MaxServerConnections = 1,
                SendBufferSize = 8192,
                PipeDirection = PipeDirection.InOut,
                PipeName = "nistec_queue_manager",
                PipeOptions = PipeOptions.None,
                VerifyPipe = "nistec_queue_manager"

            };
        }

        /// <summary>
        /// Constractor default
        /// </summary>
        public PipeManagerServer()
            : base(GetSettings())
        {
            //LoadRemoteQueue();
        }

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loadFromSettings"></param>
        public PipeManagerServer(string name, bool loadFromSettings)
            : base(name, loadFromSettings)
        {
            //LoadRemoteQueue();
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
