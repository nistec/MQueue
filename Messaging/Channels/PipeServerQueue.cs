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


namespace Nistec.Messaging.Channels
{
    /// <summary>
    /// Represent a queue Pipe server listner.
    /// </summary>
    public class PipeServerQueue : PipeServer<QItemStream>
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
        public PipeServerQueue(string hostName)
            : base(ServerQueueSettings.LoadPipeConfigServer(hostName))
        {

            
        }

        /// <summary>
        /// Constractor using <see cref="PipeSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public PipeServerQueue(PipeSettings settings)
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
        protected override NetStream ExecRequset(QItemStream message)
        {
            return AgentManager.Queue.ExecRequset(message);
        }
        /// <summary>
        /// ReadRequest
        /// </summary>
        /// <param name="pipeServer"></param>
        /// <returns></returns>
         protected override QItemStream ReadRequest(NamedPipeServerStream pipeServer)
        {
            //QueueItemStream message = new QueueItemStream();

            return QItemStream.Create(pipeServer);
        }
       

        #endregion
    }
}
