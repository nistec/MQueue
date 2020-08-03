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
using Nistec.Channels.Tcp;
using System.Net.Sockets;
using Nistec.Channels.Config;


namespace Nistec.Messaging.Channels
{

    /// <summary>
    /// Represent tcp client channel
    /// </summary>
    public class TcpClientQueue : TcpClient<IQueueMessage>, IDisposable
    {

        #region static send methods

        public static QueueAck Enqueue(QueueItem request, string hostAddress, int port, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            //Type type = request.BodyType;
            using (TcpClientQueue client = new TcpClientQueue(hostAddress, port, ProcessTimeout, IsAsync))
            {
                return client.Enqueue(request, enableException);
            }
        }
        
        // public static QueueAck Management(IQueueMessage request, string hostAddress, int port, int ProcessTimeout, bool IsAsync, bool enableException = false)
        //{
        //    //Type type = request.BodyType;
        //    using (TcpClientQueue client = new TcpClientQueue(hostAddress, port, ProcessTimeout, IsAsync))
        //    {
        //        return client.Management(request, enableException);
        //    }
        //}
        public static QueueItem SendDuplex(IQueueMessage request, string hostAddress, int port, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            //Type type = request.BodyType;
            using (TcpClientQueue client = new TcpClientQueue(hostAddress, port,ProcessTimeout,IsAsync))
            {
                return client.Execute(request, enableException);
            }
        }

        public static void SendOut(IQueueMessage request, string hostAddress, int port, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            //request.IsDuplex = false;
            using (TcpClientQueue client = new TcpClientQueue(hostAddress, port, ProcessTimeout, IsAsync))
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
        /// <param name="port"></param>
        /// <param name="connectTimeout"></param>
        public TcpClientQueue(string hostAddress, int port, int connectTimeout)
            : base(hostAddress, port, connectTimeout, false)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <param name="connectTimeout"></param>
        /// <param name="isAsync"></param>
        public TcpClientQueue(string hostAddress, int port, int connectTimeout, bool isAsync)
            : base(hostAddress, port, connectTimeout, isAsync)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <param name="connectTimeout"></param>
        /// <param name="ReceiveBufferSize"></param>
        /// <param name="SendBufferSize"></param>
        /// <param name="isAsync"></param>
        public TcpClientQueue(string hostAddress, int port, int connectTimeout, int ReceiveBufferSize, int SendBufferSize,bool isAsync)
            : base(hostAddress, port, connectTimeout, ReceiveBufferSize, SendBufferSize, isAsync)
        {

        }

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHost"></param>
        public TcpClientQueue(string configHost):base(configHost)
        {
           // Settings = TcpClientQueueSettings.GetTcpClientSettings(configHost);
        }

        /// <summary>
        /// Constractor with settings parameters
        /// </summary>
        /// <param name="settings"></param>
        public TcpClientQueue(TcpSettings settings)
            : base(settings)
        {

        }

        #endregion

        #region override

        /// <summary>
        /// ExecuteMessage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        protected override void ExecuteOneWay(NetworkStream stream, IQueueMessage message)
        {
            // Send a request from client to server
            message.EntityWrite(stream, null);

            if (message.DuplexType ==  DuplexTypes.None)//.TransformType ==  TransformTypes.OneWay)
            {
                return;
            }

            // Receive a response from server.
            MessageReader.ReadQStream(stream, Settings.ReadTimeout, Settings.ReceiveBufferSize);
            //message.ReadAck(stream, Settings.ProcessTimeout, Settings.ReceiveBufferSize);
        }


        /// <summary>
        /// ExecuteMessage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override object ExecuteMessage(NetworkStream stream, IQueueMessage message)//, Type type)
        {
            object response = null;

            // Send a request from client to server
            message.EntityWrite(stream, null);


            if (message.DuplexType ==  DuplexTypes.None)//if (message.TransformType ==  TransformTypes.OneWay)
            {
                return response;
            }

            // Receive a response from server.
            //response = message.ReadAck(stream, type, Settings.ProcessTimeout, Settings.ReceiveBufferSize);
            response = MessageReader.ReadQStream(stream, Settings.ReadTimeout, Settings.ReceiveBufferSize);
            
            return response;
        }
        /// <summary>
        /// ExecuteMessage
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override TResponse ExecuteMessage<TResponse>(NetworkStream stream, IQueueMessage message)
        {
            TResponse response = default(TResponse);

            // Send a request from client to server
            message.EntityWrite(stream, null);


            if (message.DuplexType ==  DuplexTypes.None)//if (message.TransformType== TransformTypes.OneWay)
            {
                return response;
            }

            // Receive a response from server.

            //if (message.Command== QueueCmd.Enqueue)
           if (typeof(IQueueAck).IsAssignableFrom(typeof(TResponse)))
           {
                var ack = MessageReader.ReadAckStream(stream, Settings.ReadTimeout, Settings.ReceiveBufferSize);

                return GenericTypes.Cast<TResponse>(ack);
            }

            var msg = MessageReader.ReadQStream(stream, Settings.ReadTimeout, Settings.ReceiveBufferSize);
            return GenericTypes.Cast<TResponse>(msg);

            //response = Serialization.BinarySerializer.DeserializeFromStream<TResponse>(msg.BodyStream);
            //response = message.ReadAck<TResponse>(stream, Settings.ProcessTimeout, Settings.ReceiveBufferSize);

            //return response;
        }
        
        /// <summary>
        /// connect to the tcp channel and execute request.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public new QueueItem Execute(IQueueMessage message, bool enableException = false)
        {
            return Execute<QueueItem>(message, enableException);
        }
        public new QueueAck Enqueue(QueueItem message, bool enableException = false)
        {
            return Execute<QueueAck>(message, enableException);
        }
        //public new QueueAck Management(IQueueMessage message, bool enableException = false)
        //{
        //    return Execute<QueueAck>(message, enableException);
        //}
        
        #endregion

    }
    /*
    /// <summary>
    /// Represent tcp client channel
    /// </summary>
    public class TcpClientRequest : TcpClient<MessageRequest>, IDisposable
    {

        #region static send methods

        public static object SendDuplex(MessageRequest request, string hostAddress, int port, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            using (var client = new TcpClientRequest(hostAddress, port, ProcessTimeout, IsAsync))
            {
                return client.Execute(request, enableException);
            }
        }

        public static void SendOut(MessageRequest request, string hostAddress, int port, int ProcessTimeout, bool IsAsync, bool enableException = false)
        {
            //request.IsDuplex = false;
            using (var client = new TcpClientRequest(hostAddress, port, ProcessTimeout, IsAsync))
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
        /// <param name="port"></param>
        /// <param name="ProcessTimeout"></param>
        public TcpClientRequest(string hostAddress, int port, int ProcessTimeout)
            : base(hostAddress, port, ProcessTimeout, false)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <param name="ProcessTimeout"></param>
        /// <param name="isAsync"></param>
        public TcpClientRequest(string hostAddress, int port, int ProcessTimeout, bool isAsync)
            : base(hostAddress, port, ProcessTimeout, isAsync)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <param name="ProcessTimeout"></param>
        /// <param name="ReceiveBufferSize"></param>
        /// <param name="SendBufferSize"></param>
        /// <param name="isAsync"></param>
        public TcpClientRequest(string hostAddress, int port, int ProcessTimeout, int ReceiveBufferSize, int SendBufferSize, bool isAsync)
            : base(hostAddress, port, ProcessTimeout, ReceiveBufferSize, SendBufferSize, isAsync)
        {

        }

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHost"></param>
        public TcpClientRequest(string configHost):base(configHost)
        {
            //Settings = TcpClientQueueSettings.GetTcpClientSettings(configHost);
        }

        /// <summary>
        /// Constractor with settings parameters
        /// </summary>
        /// <param name="settings"></param>
        public TcpClientRequest(TcpSettings settings)
            : base(settings)
        {

        }

        #endregion

        #region override
        /// <summary>
        /// ExecuteMessage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        protected override void ExecuteMessage(NetworkStream stream, MessageRequest message)
        {
            // Send a request from client to server
            message.EntityWrite(stream, null);

            if (message.TransformType == TransformTypes.OneWay)
            {
                return;
            }

            // Receive a response from server.

            MessageReader.ReadStream(stream, Settings.ProcessTimeout, Settings.ReceiveBufferSize);
            //message.ReadAck(stream, Settings.ProcessTimeout, Settings.ReceiveBufferSize);
        }
        /// <summary>
        /// ExecuteMessage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected override object ExecuteMessage(NetworkStream stream, MessageRequest message, Type type)
        {
            object response = null;

            // Send a request from client to server
            message.EntityWrite(stream, null);


            if (message.TransformType == TransformTypes.OneWay)
            {
                return response;
            }

            // Receive a response from server.
            //response = message.ReadAck(stream, type, Settings.ProcessTimeout, Settings.ReceiveBufferSize);
            response = MessageReader.ReadStream(stream, Settings.ProcessTimeout, Settings.ReceiveBufferSize);

            return response;
        }
        /// <summary>
        /// ExecuteMessage
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override TResponse ExecuteMessage<TResponse>(NetworkStream stream, MessageRequest message)
        {
            TResponse response = default(TResponse);

            // Send a request from client to server
            message.EntityWrite(stream, null);


            if (message.TransformType == TransformTypes.OneWay)
            {
                return response;
            }

            // Receive a response from server.
            var msg = MessageReader.ReadStream(stream, Settings.ProcessTimeout, Settings.ReceiveBufferSize);
            response = Serialization.BinarySerializer.DeserializeFromStream<TResponse>(msg.BodyStream);
            ////response = message.ReadAck<TResponse>(stream, Settings.ProcessTimeout, Settings.ReceiveBufferSize);

            return response;

        }

        /// <summary>
        /// connect to the tcp channel and execute request.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public new MessageAck Execute(MessageRequest message, bool enableException = false)
        {
            return Execute<MessageAck>(message, enableException);
        }



        #endregion

    }
    */
}