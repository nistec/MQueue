using Nistec.Messaging.Remote;
using Nistec.Channels;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using Nistec.Logging;
using System.Threading;
using System.Runtime.Serialization;

namespace Nistec.Messaging.Channels
{


    public class PipeClientQueue : PipeClient<IQueueMessage>, IDisposable
    {

        #region static send methods

        public static QueueAck Enqueue(QueueItem request, string PipeName, PipeOptions options, bool enableException = false)
        {
            //Type type = request.BodyType;
            using (PipeClientQueue client = new PipeClientQueue(PipeName, true, options))
            {
                return client.Enqueue(request, enableException);
            }
        }

        public static QueueItem SendDuplex(IQueueMessage request, string PipeName, PipeOptions options, bool enableException = false)
        {
            //Type type = request.BodyType;
            using (PipeClientQueue client = new PipeClientQueue(PipeName, true, options))
            {
                return client.Execute(request, enableException);
            }
        }

        //public static QueueAck Management(IQueueMessage request, string PipeName, bool IsAsync, bool enableException = false)
        //{
        //    //Type type = request.BodyType;
        //    using (PipeClientQueue client = new PipeClientQueue(PipeName, true, IsAsync))
        //    {
        //        return client.Management(request, enableException);
        //    }
        //}
        public static void SendOut(IQueueMessage request, string PipeName, PipeOptions options, bool enableException = false)
        {
            //request.IsDuplex = false;
            using (PipeClientQueue client = new PipeClientQueue(PipeName, false, options))
            {
                client.Execute(request, enableException);
            }
        }


        #endregion

        #region ctor



        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHostName"></param>
        /// <param name="direction"></param>
        protected PipeClientQueue(string configHostName, PipeDirection direction):base(configHostName, direction)
        {

        }
        /// <summary>
        /// Constractor with settings parameters
        /// </summary>
        /// <param name="settings"></param>
        protected PipeClientQueue(PipeSettings settings)
            :base(settings)
        {
        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="ReceiveBufferSize"></param>
        /// <param name="SendBufferSize"></param>
        /// <param name="isDuplex"></param>
        /// <param name="isAsync"></param>
        protected PipeClientQueue(string pipeName, int ReceiveBufferSize, int SendBufferSize, bool isDuplex, PipeOptions options)
            :base(pipeName, ReceiveBufferSize, SendBufferSize, isDuplex, options)
        {
            
        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="isDuplex"></param>
        /// <param name="isAsync"></param>
        protected PipeClientQueue(string pipeName, bool isDuplex, PipeOptions options)
            :base(pipeName, isDuplex, options)
        {
        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="isDuplex"></param>
        /// <param name="connectTimeout"></param>
        protected PipeClientQueue(string pipeName, bool isDuplex, int connectTimeout) 
            : base(pipeName, isDuplex, PipeOptions.None, connectTimeout)
        {

        }
        #endregion

        #region override

        protected override void ExecuteOneWay(IQueueMessage message)
        {
            //object response = null;

            if (PipeDirection != System.IO.Pipes.PipeDirection.In)
            {
                // Send a request from client to server
                message.EntityWrite(pipeClientStream, null);
            }

            if (PipeDirection == System.IO.Pipes.PipeDirection.Out)
            {
                return; //response;
            }

            // Receive a response from server.
            //response = MessageReader.ReadQStream(pipeClientStream, Settings.ReceiveBufferSize);

        }

        protected override object ExecuteMessage(IQueueMessage message)
        {
            object response = null;

            if (PipeDirection != System.IO.Pipes.PipeDirection.In)
            {
                // Send a request from client to server
                message.EntityWrite(pipeClientStream, null);
            }

            if (PipeDirection == System.IO.Pipes.PipeDirection.Out)
            {
                return response;
            }

            // Receive a response from server.
            response = MessageReader.ReadQStream(pipeClientStream, Settings.ReceiveBufferSize);

            //response = message.ReadResponse(pipeClient, type, Settings.ReceiveBufferSize);

            return response;
        }

        
        protected override TResponse ExecuteMessage<TResponse>(IQueueMessage message) 
        {
            TResponse response = default(TResponse);

            if (PipeDirection != System.IO.Pipes.PipeDirection.In)
            {
                // Send a request from client to server
                message.EntityWrite(pipeClientStream, null);
            }

            if (PipeDirection == System.IO.Pipes.PipeDirection.Out)
            {
                return response;
            }

            if (typeof(IQueueAck).IsAssignableFrom(typeof(TResponse)))
            {
                var ack = MessageReader.ReadAckStream(pipeClientStream, Settings.ReceiveBufferSize);
                return GenericTypes.Cast<TResponse>(ack);
            }

            //if (message.Command== QueueCmd.Enqueue)
            //{
            //    var ack = MessageReader.ReadAckStream(pipeClient, Settings.ReceiveBufferSize);
            //    return GenericTypes.Cast<TResponse>(ack);
            //}

            // Receive a response from server.
            var msg = MessageReader.ReadQStream(pipeClientStream, Settings.ReceiveBufferSize);
            return GenericTypes.Cast<TResponse>(msg);

            //response = Serialization.BinarySerializer.DeserializeFromStream<TResponse>(msg.BodyStream);
            //return response;
            
            
            //var ack = MessageReader.ReadAck(pipeClient, Settings.ReceiveBufferSize);
            //return GenericTypes.Cast<TResponse>(res);
            //response = message.ReadAck<TResponse>(pipeClient, Settings.ReceiveBufferSize);
        }


        /// <summary>
        /// connect to the named pipe and execute request.
        /// </summary>
        public QueueItem Execute(IQueueMessage message, bool enableException = false)
        {
            return Execute<QueueItem>(message, enableException);
        }

        public QueueAck Enqueue(QueueItem message, bool enableException = false)
        {
            return Execute<QueueAck>(message, enableException);
        }
        //public QueueAck Management(IQueueMessage message, bool enableException = false)
        //{
        //    return Execute<QueueAck>(message, enableException);
        //}
        

        #endregion

    }
    /*
    public class PipeClientRequest : PipeClient<MessageRequest>, IDisposable
    {

        #region static send methods

        public static object SendDuplex(MessageRequest request, string PipeName, bool IsAsync, bool enableException = false)
        {
            using (var client = new PipeClientRequest(PipeName, true, IsAsync))
            {
                return client.Execute(request, enableException);
            }
        }

        public static void SendOut(MessageRequest request, string PipeName, bool IsAsync, bool enableException = false)
        {
            //request.IsDuplex = false;
            using (var client = new PipeClientRequest(PipeName, false, IsAsync))
            {
                client.Execute(request, enableException);
            }
        }


        #endregion

        #region ctor

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHostName"></param>
        /// <param name="direction"></param>
        protected PipeClientRequest(string configHostName, PipeDirection direction) : base(configHostName, direction)
        {

        }
        /// <summary>
        /// Constractor with settings parameters
        /// </summary>
        /// <param name="settings"></param>
        protected PipeClientRequest(PipeSettings settings) : base(settings)
        {
        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="ReceiveBufferSize"></param>
        /// <param name="SendBufferSize"></param>
        /// <param name="isDuplex"></param>
        /// <param name="isAsync"></param>
        protected PipeClientRequest(string pipeName, int ReceiveBufferSize, int SendBufferSize, bool isDuplex, bool isAsync) : base(pipeName, ReceiveBufferSize, SendBufferSize, isDuplex, isAsync)
        {

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="isDuplex"></param>
        /// <param name="isAsync"></param>
        protected PipeClientRequest(string pipeName, bool isDuplex, bool isAsync) : base(pipeName, isDuplex, isAsync)
        {
        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="isDuplex"></param>
        /// <param name="connectTimeout"></param>
        protected PipeClientRequest(string pipeName, bool isDuplex, int connectTimeout)
            : base(pipeName, isDuplex, connectTimeout)
        {

        }
        #endregion

        #region override

        protected override object ExecuteMessage(MessageRequest message, Type type)
        {
            object response = null;

            if (PipeDirection != System.IO.Pipes.PipeDirection.In)
            {
                // Send a request from client to server
                message.EntityWrite(pipeClient, null);
            }

            if (PipeDirection == System.IO.Pipes.PipeDirection.Out)
            {
                return response;
            }

            // Receive a response from server.
            response = MessageReader.ReadQStream(pipeClient, Settings.ReceiveBufferSize);

            //response = message.ReadResponse(pipeClient, type, Settings.ReceiveBufferSize);

            return response;
        }

        protected override TResponse ExecuteMessage<TResponse>(MessageRequest message)
        {
            TResponse response = default(TResponse);

            if (PipeDirection != System.IO.Pipes.PipeDirection.In)
            {
                // Send a request from client to server
                message.EntityWrite(pipeClient, null);
            }

            if (PipeDirection == System.IO.Pipes.PipeDirection.Out)
            {
                return response;
            }

            // Receive a response from server.
            var res = MessageReader.ReadQStream(pipeClient, Settings.ReceiveBufferSize);

            return GenericTypes.Cast<TResponse>(res);
            //response = message.ReadAck<TResponse>(pipeClient, Settings.ReceiveBufferSize);
        }


        /// <summary>
        /// connect to the named pipe and execute request.
        /// </summary>
        public QueueItem Execute(MessageRequest message, bool enableException = false)
        {
            return Execute<QueueItem>(message, enableException);
        }

        #endregion

    }
    */

#if (false)
    public class PipeClientQueue : IDisposable
    {

    #region static send methods

        public static object SendDuplex(Message request, string PipeName, bool IsAsync, bool enableException = false)
        {
            Type type = request.BodyType;
            using (PipeClientQueue client = new PipeClientQueue(PipeName, true, IsAsync))
            {
                return client.Execute(request, enableException);
            }
        }

        public static void SendOut(Message request, string PipeName, bool IsAsync, bool enableException = false)
        {
            //request.IsDuplex = false;
            using (PipeClientQueue client = new PipeClientQueue(PipeName, false, IsAsync))
            {
                client.Execute(request, enableException);
            }
        }

    #endregion

    #region members
        protected NamedPipeClientStream pipeClient = null;
        const int MaxRetry = 3;

        ILogger _Logger = Logger.Instance;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Log { get { return _Logger; } set { if (value != null)_Logger = value; } }

    #endregion

    #region settings

        protected PipeSettings Settings;

        public PipeDirection PipeDirection { get; set; }
        public string PipeName { get { return Settings.PipeName; } }

        public const string ServerName = ".";
        public string FullPipeName { get { return @"\\" + ServerName + @"\pipe\" + PipeName; } }

    #endregion

    #region ctor


        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="configHostName"></param>
        /// <param name="direction"></param>
        protected PipeClientQueue(string configHostName, PipeDirection direction)
        {
            Settings = PipeClientSettings.GetPipeClientSettings(configHostName);
            this.PipeDirection = direction;

        }
        /// <summary>
        /// Constractor with settings parameters
        /// </summary>
        /// <param name="settings"></param>
        protected PipeClientQueue(PipeSettings settings)
        {
            Settings = settings;
            this.PipeDirection = settings.PipeDirection;

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="ReceiveBufferSize"></param>
        /// <param name="SendBufferSize"></param>
        /// <param name="isDuplex"></param>
        /// <param name="isAsync"></param>
        protected PipeClientQueue(string pipeName, int ReceiveBufferSize, int SendBufferSize, bool isDuplex, bool isAsync)
        {
            Settings = new PipeSettings()
            {
                PipeName = pipeName,
                ConnectTimeout = (uint)PipeSettings.DefaultConnectTimeout,
                ReceiveBufferSize = ReceiveBufferSize,
                SendBufferSize = SendBufferSize,
                PipeDirection = isDuplex ? PipeDirection.InOut : System.IO.Pipes.PipeDirection.Out,
                PipeOptions = isAsync ? PipeOptions.Asynchronous : PipeOptions.None,
                VerifyPipe = pipeName
            };
            this.PipeDirection = isDuplex ? PipeDirection.InOut : System.IO.Pipes.PipeDirection.Out;

        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="isDuplex"></param>
        /// <param name="isAsync"></param>
        protected PipeClientQueue(string pipeName, bool isDuplex, bool isAsync)
        {
            this.PipeDirection = isDuplex ? PipeDirection.InOut : System.IO.Pipes.PipeDirection.Out;

            Settings = new PipeSettings()
            {
                PipeName = pipeName,
                ConnectTimeout = (uint)PipeSettings.DefaultConnectTimeout,
                ReceiveBufferSize = PipeSettings.DefaultReceiveBufferSize,
                SendBufferSize = PipeSettings.DefaultSendBufferSize,
                PipeDirection = isDuplex ? PipeDirection.InOut : System.IO.Pipes.PipeDirection.Out,
                PipeOptions = isAsync ? PipeOptions.Asynchronous : PipeOptions.None,
                VerifyPipe = pipeName
            };
        }

        /// <summary>
        /// Constractor with arguments
        /// </summary>
        /// <param name="pipeName"></param>
        /// <param name="isDuplex"></param>
        /// <param name="connectTimeout"></param>
        protected PipeClientQueue(string pipeName, bool isDuplex, int connectTimeout)
        {
            this.PipeDirection = isDuplex ? PipeDirection.InOut : System.IO.Pipes.PipeDirection.Out;

            Settings = new PipeSettings()
            {
                PipeName = pipeName,
                ConnectTimeout = (uint)connectTimeout,
                ReceiveBufferSize = PipeSettings.DefaultReceiveBufferSize,
                SendBufferSize = PipeSettings.DefaultSendBufferSize,
                PipeDirection = isDuplex ? PipeDirection.InOut : System.IO.Pipes.PipeDirection.Out,
                PipeOptions = PipeOptions.None,
                VerifyPipe = pipeName
            };
        }
    #endregion

    #region IDisposable

        public void Dispose()
        {
            if (pipeClient != null)
            {
                pipeClient.Dispose();
                pipeClient = null;
            }
        }
    #endregion

    #region Read/Write

        /// <summary>
        /// Execute Message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected object ExecuteMessage(Message message)
        {
            object response = null;

            if (PipeDirection != System.IO.Pipes.PipeDirection.In)
            {
                // Send a request from client to server
                message.EntityWrite(pipeClient, null);
            }

            if (PipeDirection == System.IO.Pipes.PipeDirection.Out)
            {
                return response;
            }

            // Receive a response from server.
            response = MessageReader.ReadBodyStream(pipeClient, Settings.ReceiveBufferSize);

            //response = message.ReadResponse(pipeClient, type, Settings.ReceiveBufferSize);

            return response;
        }

    #endregion

    #region Run

        NamedPipeClientStream CreatePipe()
        {
            return new NamedPipeClientStream(
                    ServerName,                 // The server name
                    Settings.PipeName,                   // The unique pipe name
                    Settings.PipeDirection,              // The pipe is duplex
                    Settings.PipeOptions                 // No additional parameters
                    );
        }

        bool Connect()
        {
            int retry = 0;

            while (retry < MaxRetry)
            {

                try
                {
                    pipeClient.Connect((int)Settings.ConnectTimeout);
                    if (!pipeClient.IsConnected)
                    {
                        retry++;
                        if (retry >= MaxRetry)
                        {
                            throw new Exception("Unable to connect to pipe: " + Settings.PipeName);
                        }
                        Thread.Sleep(10);

                        //Netlog.WarnFormat("NativePipeClient retry: {0} ", retry);
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (TimeoutException toex)
                {
                    if (retry >= MaxRetry)
                    {
                        Log.Error("PipeClient connection has timeout exception after retry: {0},timeout:{1}, msg: {2}", retry, Settings.ConnectTimeout, toex.Message);
                        throw toex;
                    }
                    retry++;
                }
                catch (Exception pex)
                {
                    if (retry >= MaxRetry)
                    {
                        Log.Error("PipeClient connection error after retry: {0}, msg: {1}", retry, pex.Message);
                        throw pex;
                    }
                    retry++;
                }
            }

            return pipeClient.IsConnected;
        }

        /// <summary>
        /// connect to the named pipe and execute request.
        /// </summary>
        public void ExecuteOut(Message message, bool enableException = false)
        {
            Execute(message, enableException);
        }

        /// <summary>
        /// connect to the named pipe and execute request.
        /// </summary>
        public object Execute(Message message,bool enableException = false)
        {

            object response = null;// default(TResponse);

            try
            {
                // Try to open the named pipe identified by the pipe name.

                pipeClient = new NamedPipeClientStream(
                    ServerName,                 // The server name
                    Settings.PipeName,                   // The unique pipe name
                    Settings.PipeDirection,              // The pipe is duplex
                    Settings.PipeOptions                 // No additional parameters
                    );

                bool ok = Connect();
                if (!ok)
                {
                    throw new Exception("Unable to connect to pipe:" + PipeName);
                }


                // Set the read mode and the blocking mode of the named pipe.
                pipeClient.ReadMode = PipeTransmissionMode.Message;


                return ExecuteMessage(message);

            }
            catch (TimeoutException toex)
            {
                Log.Exception("The client throws the TimeoutException : ", toex, true);
                if (enableException)
                    throw toex;
                return response;
            }
            catch (SerializationException sex)
            {
                Log.Exception("The client throws the SerializationException : ", sex, true);
                if (enableException)
                    throw sex;
                return response;
            }
            catch (MessageException mex)
            {
                Log.Exception("The tcp client throws the MessageException : ", mex, true);
                if (enableException)
                    throw mex;
                return response;
            }
            catch (Exception ex)
            {
                Log.Exception("The client throws the error: ", ex, true);

                if (enableException)
                    throw ex;

                return response;
            }
            finally
            {
                // Close the pipe.
                if (pipeClient != null)
                {
                    pipeClient.Close();
                    pipeClient = null;
                }
            }
        }


    #endregion

    }
#endif

}

