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
    public class HttpServerListener : HttpServer<IQueueMessage>
    {

        bool IsListener = true;


        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            QLogger.InfoFormat("HttpServerListener started :{0}", this.Settings.HostName);
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            AgentManager.StopController();
            QLogger.InfoFormat("HttpServerListener stoped :{0}", this.Settings.HostName);

        }
        /// <summary>
        /// OnLoad
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
            AgentManager.StartController();
        }
        #endregion

        #region ctor

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="hostName"></param>
        public HttpServerListener(string hostName)
            : base(hostName)
        {
            Settings = QueueServerSettings.LoadHttpConfigServer(hostName);
        }

        /// <summary>
        /// Constractor using <see cref="HttpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public HttpServerListener(HttpSettings settings)
            : base(settings)
        {
        }

        #endregion

        #region abstract methods
  
        protected override string ExecString(IQueueMessage request)
        {
            //throw new NotImplementedException();
            var ms=AgentManager.Queue.ExecRequset(request);
            return ms.ReadJson();
        }

        protected override TransStream ExecTransStream(IQueueMessage request)
        {
            return AgentManager.Queue.ExecRequset(request);
        }

        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override IQueueMessage ReadRequest(HttpRequestInfo request)
        {
            //throw new Exception("Not implemented.");

            MessageStream stream = null;
            if (request.BodyStream != null)
            {
                stream = MessageStream.ParseStream(request.BodyStream, NetProtocol.Http);
            }
            else
            {

                var message = new HttpMessage();

                if (request.QueryString != null)//request.BodyType == HttpBodyType.QueryString)
                    message.EntityRead(request.QueryString, null);
                else if (request.Body != null)
                    message.EntityRead(request.Body, null);
                //else if (request.Url.LocalPath != null && request.Url.LocalPath.Length > 1)
                //    message.EntityRead(request.Url.LocalPath.TrimStart('/').TrimEnd('/'), null);

                stream = message;
            }

            return new QueueRequest(stream.GetStream());
        }

        #endregion


    }
}
