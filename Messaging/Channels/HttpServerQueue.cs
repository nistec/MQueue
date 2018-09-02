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


namespace Nistec.Messaging.Channels
{
    /// <summary>
    /// Represent a queue Http server listner.
    /// </summary>
    public class HttpServerQueue : HttpServer<QItemStream>
    {
        bool isQueue=false;
        bool isDataQueue=false;
        bool isSyncQueue=false;
        bool isSession=false;

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            //AgentManager.Queue.Stop();
        }
        /// <summary>
        /// OnLoad
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
            //AgentManager.Queue.Start();
        }
        #endregion

        #region ctor

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="hostName"></param>
        public HttpServerQueue(string hostName)
            : base(hostName)
        {

        }

        /// <summary>
        /// Constractor using <see cref="HttpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        public HttpServerQueue(HttpSettings settings)
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
        /// Read Request
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected override QItemStream ReadRequest(HttpRequestInfo request)
        {
           // return QItemStream.Create(stream);
            throw new Exception("Not implemented.");
        }

        #endregion


    }
}
