using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using Nistec.Channels;
using Nistec.IO;
using System.Threading.Tasks;
using Nistec.Channels.Http;
using System.Net.Sockets;
using Nistec.Messaging.Config;

namespace Nistec.Messaging.Server.Http
{
    /// <summary>
    /// Represent a queue Http server listner.
    /// </summary>
    public class HttpServerChannel : HttpServer<IQueueMessage>
    {
        //bool isQueue=false;
        //bool isDataQueue=false;
        //bool isSyncQueue=false;
        //bool isSession=false;

        bool IsListener = false;

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            QLogger.InfoFormat("HttpServerChannel started :{0}", this.Settings.HostName);
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            AgentManager.Queue.Stop();
            QLogger.InfoFormat("HttpServerChannel stoped :{0}", this.Settings.HostName);

        }
        /// <summary>
        /// OnLoad
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
            AgentManager.Queue.Start();
        }
        #endregion

        #region ctor

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="hostName"></param>
        public HttpServerChannel(string hostName, bool isListener)
            : base(hostName)
        {
            Settings = QueueServerSettings.LoadHttpConfigServer(hostName);
            IsListener = isListener;
        }

        /// <summary>
        /// Constractor using <see cref="HttpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public HttpServerChannel(HttpSettings settings, bool isListener)
            : base(settings)
        {
            IsListener = isListener;
        }

        #endregion

        #region abstract methods

        protected override string ExecString(IQueueMessage request)
        {
            throw new NotImplementedException();
        }

        protected override TransStream ExecTransStream(IQueueMessage request)
        {
            return AgentManager.Queue.ExecRequset(request);
        }

        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected override IQueueMessage ReadRequest(HttpRequestInfo request)
        {
            //if (IsListener)
            //{
            //    var req = new MessageRequest(stream);
            //    return req;
            //}
            //var item = new QueueItem(stream, null);
            //return item;

            // return QueueItem.Create(stream);
            throw new Exception("Not implemented.");
        }

        #endregion


    }
}
