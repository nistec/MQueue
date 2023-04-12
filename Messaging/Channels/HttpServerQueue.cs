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
using Nistec.Generic;
using Nistec.Messaging.Listeners;
using Nistec.Logging;
//using Nistec.Messaging.Config;

namespace Nistec.Messaging.Channels
{
   

    /// <summary>
    /// Represent a queue Http server listner.
    /// </summary>
    public class HttpServerQueue : HttpServer<IQueueMessage>, IChannelService
    {

        QueueChannel QueueChannel= QueueChannel.Consumer;
        IControllerHandler Controller;

        #region override

        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            Log.Info("HttpServerQueue started :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            Log.Info("HttpServerQueue stoped :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
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
        /// Constractor using <see cref="HttpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="controller"></param>
        public HttpServerQueue(HttpSettings settings, IControllerHandler controller)
            : base(settings)
        {
            Controller = controller;
        }

        #endregion

        #region abstract methods

        protected override string ExecString(IQueueMessage message)
        {
            var ts=Controller.OnMessageReceived(message);
            return (ts == null) ? null : ts.ReadToJson();
        }

        protected override TransStream ExecTransStream(IQueueMessage message)
        {
            return Controller.OnMessageReceived((QueueMessage)message);
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

            //return new QueueRequest(stream.GetStream());
            return QueueMessage.Create(stream.GetStream());
        }

        #endregion


    }

    public class HttpServerGeneric<T> : HttpServer<T>, IChannelService where T: IHostMessage
    {

        QueueChannel QueueChannel = QueueChannel.Consumer;
        IControllerHandler<T> Controller;

        #region override

        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            Log.Info("HttpServerQueue started :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            Log.Info("HttpServerQueue stoped :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
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
        /// Constractor using <see cref="HttpSettings"/> settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="controller"></param>
        public HttpServerGeneric(HttpSettings settings, IControllerHandler<T> controller)
            : base(settings)
        {
            Controller = controller;
        }

        #endregion

        #region abstract methods

        protected override string ExecString(T message)
        {
            var ts = Controller.OnMessageReceived(message);
            return (ts == null) ? null : ts.ReadToJson();
        }

        protected override TransStream ExecTransStream(T message)
        {
            return Controller.OnMessageReceived((T)message);
        }

        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override T ReadRequest(HttpRequestInfo request)
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

            return Nistec.Runtime.ActivatorUtil.CreateInstance<T>().Parse<T>(stream.GetStream());

            //return new QueueRequest(stream.GetStream());
            //return QueueMessage.Create(stream.GetStream());
        }

        #endregion


    }
}
