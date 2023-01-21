using Nistec.Channels;
using Nistec.Channels.Http;
using Nistec.IO;
using Nistec.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueListenerDemo
{
 
    public class HttpServerChannel : HttpServer<string>
    {

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            //AgentManager.StartController();
            //Log.Info("HttpServerChannel started :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            //AgentManager.StopController();
            //Log.Info("HttpServerChannel stoped :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnLoad
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
            //AgentManager.StartController();
        }
        #endregion

        #region ctor

        static HttpSettings GetSettings()
        {

            return new HttpSettings("http://localhost", 8080, "post")
            {

                HostName = "RemoteScan",
                //Address = "localhost",
                //Port = 8080,
                SslPort = 443,
                SslEnabled = false,
                //Method = "Post",
                ConnectTimeout = 5000
                //ProcessTimeout = DefaultProcessTimeout;
                //ReadTimeout = DefaultReadTimeout;
                //MaxServerConnections = 0;
                //MaxErrors = DefaultMaxErrors;
                //HostAddress = "http://localhost:8080",
                //SslHostAddress = "http://localhost:443"

            };
        }


        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="hostName"></param>
        public HttpServerChannel()
            : base(GetSettings())
        {
            //Settings = QueueServerSettings.LoadHttpConfigServer(hostName);
            //QueueChannel = qChannel;


        }

        #endregion

        #region abstract methods

        protected override string ExecString(string request)
        {
            Log.Info("HttpServerChannel ExecString :{0}, Request:{1}", this.Settings.HostName, request);
            //var ms = AgentManager.Queue.ExecRequset(request);
            //return ms.ReadJson();
            return request;
        }

        protected override TransStream ExecTransStream(string request)
        {
            Log.Info("HttpServerChannel ExecTransStream :{0}, Request:{1}", this.Settings.HostName, request);
            return TransStream.Write(request, TransType.Stream);
            //return AgentManager.Queue.ExecRequset(request);
        }

        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override string ReadRequest(HttpRequestInfo request)
        {

            if (request.BodyType== HttpBodyType.Body)
                return request.Body;

            else if (request.BodyType == HttpBodyType.QueryString)
                return request.Url.Query;
            else
                return request.Url.OriginalString;
            //StringMessage message = null;
            //using (var ntStream = new NetStream())
            //{
            //    ntStream.CopyFrom(networkStream, Settings.ReadTimeout, Settings.ReceiveBufferSize);

            //    message = new StringMessage(ntStream);
            //}
            //return request.Body;



            //MessageStream stream = null;
            //if (request.BodyStream != null)
            //{
            //    stream = MessageStream.ParseStream(request.BodyStream, NetProtocol.Http);
            //}
            //else
            //{

            //    var message = new HttpMessage();

            //    if (request.QueryString != null)//request.BodyType == HttpBodyType.QueryString)
            //        message.EntityRead(request.QueryString, null);
            //    else if (request.Body != null)
            //        message.EntityRead(request.Body, null);
            //    //else if (request.Url.LocalPath != null && request.Url.LocalPath.Length > 1)
            //    //    message.EntityRead(request.Url.LocalPath.TrimStart('/').TrimEnd('/'), null);

            //    stream = message;
            //}

            ////return QueueItem.Create(stream.GetStream());

            //if (QueueChannel == QueueChannel.Producer)
            //    return QueueItem.Create(stream.GetStream());
            //else
            //    return new QueueRequest(stream.GetStream());
        }

        #endregion

    }
}
