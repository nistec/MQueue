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
//using Nistec.Messaging.Config;
using Nistec.Messaging;
using Nistec.Messaging.Listeners;
using Nistec.Logging;

namespace Nistec.Messaging.Channels
{
    /// <summary>
    /// Represent a queue Pipe server listner.
    /// </summary>
    public class PipeServerQueue : PipeServer<IQueueItem>, IChannelService
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
            Log.Info("PipeServerQueue started :{0}, QueueChannel:{1}", PipeName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            Log.Info("PipeServerQueue stoped :{0}, QueueChannel:{1}", PipeName, QueueChannel.ToString());
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
        /// Constractor using <see cref="PipeSettings"/> settings.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="settings"></param>
        public PipeServerQueue(PipeSettings settings, IControllerHandler controller)
            : base(settings)
        {
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
        /// ReadRequest
        /// </summary>
        /// <param name="pipeServer"></param>
        /// <returns></returns>
        protected override IQueueItem ReadRequest(NamedPipeServerStream pipeServer)
        {
            return new QueueItem(pipeServer, null);
        }
       

        #endregion
    }

}
