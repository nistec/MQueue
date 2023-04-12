using System;
using System.IO.Pipes;
using System.Text;
using System.IO;
using Nistec.Messaging.Remote;
using Nistec.Channels;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Runtime;
using System.Runtime.Serialization;
using System.Threading;
using Nistec.Channels.Http;
using System.Net.Sockets;
using Nistec.Serialization;


namespace Nistec.Messaging.Channels
{

    /// <summary>
    /// Represent Http client channel
    /// </summary>
    public class HttpClientQueue : HttpClient<IQueueRequest>, IDisposable
    {

        #region static send methods

        public static QueueAck Enqueue(QueueMessage request, string hostAddress, string method, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            //Type type = request.BodyType;
            using (HttpClientQueue client = new HttpClientQueue(hostAddress, method, ProcessTimeout))
            {
                var response = client.Execute<QueueAck>(request, enableException);
                return response;
                //byte[] body = response == null ? null : Encoding.UTF8.GetBytes(response);
                //var item = request.Copy();
                //item.SetBodyText(response);
                //item.SetArrived();
                //return item;// new QueueMessage(request, body, typeof(string));
            }

            //return null;
        }
        
        public static QueueAck Management(IQueueRequest request, string hostAddress, string method, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            //Type type = request.BodyType;
            using (HttpClientQueue client = new HttpClientQueue(hostAddress, method, ProcessTimeout))
            {
                var response = client.Execute<QueueAck>(request, enableException);
                return response;
                //byte[] body = response == null ? null : Encoding.UTF8.GetBytes(response);
                //var item = request.Copy();
                //item.SetBodyText(response);
                //item.SetArrived();
                //return item;// new QueueMessage(request, body, typeof(string));
            }

            //return null;
        }
        public static QueueMessage SendDuplex(IQueueRequest request, string hostAddress, string method, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            //Type type = request.BodyType;
            using (HttpClientQueue client = new HttpClientQueue(hostAddress, method, ProcessTimeout))
            {
                var response = client.Execute<QueueMessage>(request, enableException);
                return response;

                //var response= client.Execute(request, enableException);
                //byte[] body = response == null ? null : Encoding.UTF8.GetBytes(response);
                //var item = request.Copy();
                //item.SetBodyText(response);
                //item.SetArrived();
                //return item;// new QueueMessage(request, body, typeof(string));
            }

            //return null;
        }

        public static void SendOut(IQueueRequest request, string hostAddress, string method, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            //request.IsDuplex = false;
            using (HttpClientQueue client = new HttpClientQueue(hostAddress, method, ProcessTimeout))
            {
                client.Execute(request, enableException);
            }
        }

        #endregion
 
        #region ctor

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="method"></param>
        public HttpClientQueue(string hostAddress, string method)
            : base(hostAddress, method,QueueDefaults.DefaultConnectTimeOut)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="method"></param>
        /// <param name="ProcessTimeout"></param>
        public HttpClientQueue(string hostAddress, string method, int ProcessTimeout)
            : base(hostAddress, method, ProcessTimeout)
        {

        }

       

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHost"></param>
        public HttpClientQueue(string configHost)
            : base(configHost)
        {
           
        }

        /// <summary>
        /// Constractor with settings parameters
        /// </summary>
        /// <param name="settings"></param>
        public HttpClientQueue(HttpSettings settings)
            : base(settings)
        {

        }

        #endregion

        #region override

        /// <summary>
        /// Serialize json request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override NetStream RequestToStream(IQueueRequest message)
        {
            return message.ToStream();
        }

        /// <summary>
        /// Serialize json request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override byte[] RequestToBinary(IQueueRequest message)
        {
            //return message.ToStream().GetStream().ToArray();
            return message.ToStream().ToArray();
        }

       
        /// <summary>
        /// Serialize json request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override string RequestToJson(IQueueRequest message)
        {
            return message.ToJson();// JsonSerializer.Serialize(message);
        }
        /// <summary>
        ///  Deserialize json response
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        protected override TResponse ReadJsonResponse<TResponse>(string response)
        {
            return JsonSerializer.Deserialize<TResponse>(response);
        }
        /// <summary>
        ///  Deserialize json response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected override object ReadJsonResponse(string response, Type type)
        {
            return JsonSerializer.Deserialize(response, type);
        }

        #endregion
    }

    /*
    /// <summary>
    /// Represent Http client channel
    /// </summary>
    public class HttpClientRequest : HttpClient<MessageRequest>, IDisposable
    {

        #region static send methods

        public static object SendDuplex(MessageRequest request, string hostAddress, string method, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            //Type type = request.BodyType;
            using (var client = new HttpClientRequest(hostAddress, method, ProcessTimeout))
            {
                return client.Execute(request, enableException);
            }

            //return null;
        }

        public static void SendOut(MessageRequest request, string hostAddress, string method, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            //request.IsDuplex = false;
            using (var client = new HttpClientRequest(hostAddress, method, ProcessTimeout))
            {
                client.Execute(request, enableException);
            }
        }

        #endregion

        #region ctor

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="method"></param>
        public HttpClientRequest(string hostAddress, string method)
            : base(hostAddress, method)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="method"></param>
        /// <param name="ProcessTimeout"></param>
        public HttpClientRequest(string hostAddress, string method, int ProcessTimeout)
            : base(hostAddress, method, ProcessTimeout)
        {

        }



        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHost"></param>
        public HttpClientRequest(string configHost)
            : base(configHost)
        {

        }

        /// <summary>
        /// Constractor with settings parameters
        /// </summary>
        /// <param name="settings"></param>
        public HttpClientRequest(HttpSettings settings)
            : base(settings)
        {

        }

        #endregion

        #region override

        /// <summary>
        /// Serialize json request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override string RequestToJson(MessageRequest message)
        {
            return JsonSerializer.Serialize(message);
        }
        /// <summary>
        ///  Deserialize json response
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        protected override TResponse ReadJsonResponse<TResponse>(string response)
        {
            return JsonSerializer.Deserialize<TResponse>(response);
        }
        /// <summary>
        ///  Deserialize json response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected override object ReadJsonResponse(string response, Type type)
        {
            return JsonSerializer.Deserialize(response, type);
        }

        #endregion
    }
    */
}