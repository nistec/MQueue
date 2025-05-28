using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using Nistec.Channels;
using Nistec.IO;
using System.Threading.Tasks;
using Nistec.Channels.Tcp;
using System.Net.Sockets;
using Nistec.Messaging.Config;
using Nistec.Logging;
using System.Net;
using System.Threading;
using Nistec.Generic;

namespace Nistec.Messaging.Server
{

    public class TcpServerChannel : TcpSoketServer//<IQueueRequest, TransStream>
    {
        QueueChannel QueueChannel;

        #region membrs
        int ReceiveBufferSize = 4096;
        #endregion

        #region settings

        private ChannelServiceState _State = ChannelServiceState.None;
        /// <summary>
        /// Get <see cref="ChannelServiceState"/> State.
        /// </summary>
        public ChannelServiceState ServiceState { get { return _State; } }
        /// <summary>
        /// Get current <see cref="TcpSettings"/> settings.
        /// </summary>
        public TcpSettings Settings { get; protected set; }
        ILogger _Logger = Logger.Instance;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Log { get { return _Logger; } set { if (value != null) _Logger = value; } }

        /// <summary>
        /// Get current <see cref="TcpSettings"/> settings.
        /// </summary>
        public bool IsReady { get; protected set; }

        #endregion

        #region ctor

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="hostName"></param>
        public TcpServerChannel(QueueChannel qChannel, string hostName)
        {
            Settings = QueueServerSettings.LoadTcpConfigServer(hostName);
            QueueChannel = qChannel;
            //Settings.AllowedIp = allowedIps;
        }

        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="settings"></param>
        public TcpServerChannel(QueueChannel qChannel, TcpSettings settings)
        //: base()
        {
            Settings = settings;
            QueueChannel = qChannel;
        }

        #endregion

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected virtual void OnStart()
        {
            //base.OnStart();
            AgentManager.StartController();
            Log.Info("TcpServerChannel started :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected virtual void OnStop()
        {
            //base.OnStop();
            AgentManager.StopController();
            Log.Info("TcpServerChannel stoped :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        #endregion

        #region Initilize

        //private bool IsAllowedIP(string ip)
        //{
        //    return Array.Exists(AllowedIPs, allowedIp => allowedIp == ip);
        //}
        protected void Init()
        {

            if (Initilized)
                return;
            IsReady = false;

            //AllowedIPs = Settings.AllowedListIp;
            Port = Settings.Port;
            ReceiveBufferSize = Settings.ReceiveBufferSize;
            OnLoad();
            Log.Info("TcpServer Initilized...\n");
            IsReady = true;
        }

        protected virtual void OnLoad()
        {

        }

        protected virtual void OnPause()
        {

        }
        protected override void OnInfo(string message)
        {
            Console.WriteLine(message);
            Log.Info(message);
        }

        protected override void OnFault(string message)
        {
            Console.WriteLine(message);
            Log.Error(message);
        }

        //protected virtual void OnFault(string message, Exception ex)
        //{
        //    Log.Exception(message, ex, true);
        //}

        public override void Start()
        {
            try
            {
                if (_State == ChannelServiceState.Paused)
                {
                    if (Initilized)
                    {
                        _State = ChannelServiceState.Started;
                        OnStart();
                        return;
                    }
                }
                if (_State == ChannelServiceState.Started)
                    return;

                //Listen = true;
                Init();
                _State = ChannelServiceState.Started;
                OnStart();
                base.Start();
            }
            catch (Exception ex)
            {
                //Listen = false;
                _State = ChannelServiceState.None;
                OnFault("The tcp server on start throws the error: " + ex.Message);
            }
        }

        public override void Stop()
        {
            //Listen = false;
            base.Stop();
            Initilized = false;
            _State = ChannelServiceState.Stoped;
            OnStop();
            Log.Info("TcpServer stoped: {0}", Settings.HostName);
        }

        public void Pause()
        {
            //Listen = false;
            _State = ChannelServiceState.Paused;
            OnPause();
            Log.Debug("TcpServer paused: {0}", Settings.HostName);
        }

        #endregion

        #region override methods

        protected override Task<IDataStream> ServerHandle(IDataStream request)
        {

            if (QueueChannel == QueueChannel.Producer)
            {
                QueueMessage qmessage = (QueueMessage)request.ReadBody();
                var ack = AgentManager.Queue.ExecSet(qmessage);
                return Task.Run(() => (IDataStream)new TransStream(ack, "Ack", TransType.Object)); //binary

                //ack.ToTransStream
                //return Task.Run(() => TransBinary.FromBytes(response.GetBytes()));
                //var response = ExecRequset(message);
            }
            else if (QueueChannel == QueueChannel.Consumer)
            {
                QueueRequest qrequest = (QueueRequest)request.ReadBody();
                var resmessgae = AgentManager.Queue.ExecGet(qrequest);
                return Task.Run(() => (IDataStream)new TransStream(resmessgae, request.Message, TransType.Object)); //binary
            }
            else if (QueueChannel == QueueChannel.Manager)
            {
                QueueRequest qrequest = (QueueRequest)request.ReadBody();
                var resmessgae = AgentManager.Queue.ExecRequset(qrequest);
                return Task.Run(() => (IDataStream)resmessgae);// new TransBinary(resmessgae, request.Message, TransType.Object));
            }
            else
            {
                object qrequest = request.ReadBody();
                if (qrequest is QueueMessage)
                {
                    var ack = AgentManager.Queue.ExecSet((QueueMessage)qrequest);
                    return Task.Run(() => (IDataStream)new TransStream(ack, "Ack", TransType.Object)); //binary
                }
                else //if (qrequest is QueueRequest)
                {
                    var resmessgae = ExecRequset((QueueRequest)qrequest);
                    return Task.Run(() => (IDataStream)resmessgae );// new TransStream(resmessgae, request.Message, TransType.Object));
                }
            }
            //var message = ReadRequest(new NetStream(request.BodyStream));// request.GetStream());
            //var response= ExecRequset(message);
            //return Task.Run(()=> TransBinary.FromBytes(response.GetBytes()));
        }

        //protected override Task<TransBinary> ServerHandle(TransBinary request)
        //{

        //    if (QueueChannel == QueueChannel.Producer)
        //    {
        //        QueueMessage qmessage = (QueueMessage)request.GetContent();
        //        var ack = AgentManager.Queue.ExecSet(qmessage);
        //        return Task.Run(() => new TransBinary(ack, "Ack", TransType.Object));

        //        //ack.ToTransStream
        //        //return Task.Run(() => TransBinary.FromBytes(response.GetBytes()));
        //        //var response = ExecRequset(message);
        //    }
        //    else
        //    {
        //        QueueRequest qrequest = (QueueRequest)request.GetContent();
        //        var resmessgae = AgentManager.Queue.ExecGet(qrequest);
        //        return Task.Run(() => new TransBinary(resmessgae, request.Message, TransType.Object));
        //    }

        //    //var message = ReadRequest(new NetStream(request.BodyStream));// request.GetStream());
        //    //var response= ExecRequset(message);
        //    //return Task.Run(()=> TransBinary.FromBytes(response.GetBytes()));
        //}

        /// <summary>
        /// Execute client request and return response as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual TransStream ExecRequset(IQueueRequest message)
        {
            return AgentManager.Queue.ExecRequset(message);
        }

        #endregion

        #region Legacy

        /*
        protected override Task<NetStream> ServerHandle(NetStream request)
        {

            if (QueueChannel == QueueChannel.Producer)
            {
                QueueMessage qmessage = QueueMessage.Deserialize(request.ToArray());
                var ack = AgentManager.Queue.ExecSet(qmessage);
                return Task.Run(() => new NetStream(ack.Serialize()));
            }
            else
            {
                QueueRequest qrequest = QueueRequest.Deserialize(request.ToArray());
                var resmessgae = AgentManager.Queue.ExecGet(qrequest);
                return Task.Run(() => new NetStream(resmessgae.Serialize()));
            }

            //var message = ReadRequest(new NetStream(request.BodyStream));// request.GetStream());
            //var response= ExecRequset(message);
            //return Task.Run(()=> TransBinary.FromBytes(response.GetBytes()));
        }

        protected override Task<NetStream> ServerHandle(IDataStream request)
        {

            if (QueueChannel == QueueChannel.Producer)
            {
                QueueMessage qmessage = (QueueMessage)request.ReadBody();
                var ack = AgentManager.Queue.ExecSet(qmessage);
                return Task.Run(() => new NetStream(ack.Serialize()));

                //ack.ToTransStream
                //return Task.Run(() => TransBinary.FromBytes(response.GetBytes()));
                //var response = ExecRequset(message);
            }
            else
            {
                QueueRequest qrequest = (QueueRequest)request.ReadBody();
                var resmessgae = AgentManager.Queue.ExecGet(qrequest);
                return Task.Run(() => resmessgae.ToStream());// new TransBinary(resmessgae, request.Message, TransType.Object));
            }

            //var message = ReadRequest(new NetStream(request.BodyStream));// request.GetStream());
            //var response= ExecRequset(message);
            //return Task.Run(()=> TransBinary.FromBytes(response.GetBytes()));
        }
        */

        /*
        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected virtual IQueueRequest ReadRequest(NetStream stream)
        {
            //IQueueMessage message = null;
            //using (var ntStream = new NetStream())
            //{
            //    ntStream.CopyFrom(stream, readTimeout, ReceiveBufferSize);

            //    if (QueueChannel == QueueChannel.Producer)
            //        message= new QueueMessage(stream, null);
            //    else
            //        message= new QueueRequest(stream);
            //}
            //return message;

            if (QueueChannel == QueueChannel.Producer)
                return new QueueMessage(stream, null);
            else
                return new QueueRequest(stream);
        }

        /// <summary>
        /// Write response to client.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bResponse"></param>
        protected virtual async Task WriteResponseAsync(NetworkStream stream, TransStream bResponse)
        {
            if (bResponse == null)
            {
                return;
            }
            var bytes = bResponse.GetBytes();
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
        */

        //private static TcpSoketServer _serverSocket;
        //private static bool _isRunning = false;

        //public void StartServer()
        //{
        //    if (_isRunning)
        //    {
        //        OnInfo("Server is already running.");
        //        return;
        //    }
        //    //_serverSocket = new TcpSoketServer(Settings.Port, Settings.AllowedListIp);
        //    //_serverSocket.Start();
        //    _isRunning = true;
        //    OnStart();
        //    OnInfo(string.Format("Server started on port :{0},  AllowedIPs: {1}", Port, AllowedIPs == null ? "NA" : AllowedIPs.JoinTrim()));
        //    //Task.Run(() => AcceptClientsAsync());
        //}

        //public void StopServer()
        //{
        //    if (_isRunning)
        //    {
        //        _isRunning = false;
        //        _serverSocket.Stop();
        //        OnInfo("Server stopped.");
        //    }
        //}

        #endregion
    }


#if(false)
    public class TcpServerChannel //<IQueueRequest, TransStream>
    {
        QueueChannel QueueChannel;

        #region membrs
        //volatile bool Listen;
        private bool Initilized = false;

        private static int Port;// = 5000;
        private static string[] AllowedIPs; //= { "127.0.0.1", "192.168.1.100" }; // Whitelisted IPs
        //private static TcpListener _listener;
        //private static bool _isRunning = false;
        private static CancellationTokenSource _cts;
        private static int ReceiveBufferSize = 4096;

        #endregion

        #region settings

        private ChannelServiceState _State = ChannelServiceState.None;
        /// <summary>
        /// Get <see cref="ChannelServiceState"/> State.
        /// </summary>
        public ChannelServiceState ServiceState { get { return _State; } }
        /// <summary>
        /// Get current <see cref="TcpSettings"/> settings.
        /// </summary>
        public TcpSettings Settings { get; protected set; }
        ILogger _Logger = Logger.Instance;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Log { get { return _Logger; } set { if (value != null) _Logger = value; } }

        /// <summary>
        /// Get current <see cref="TcpSettings"/> settings.
        /// </summary>
        public bool IsReady { get; protected set; }

        #endregion

        #region ctor

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="hostName"></param>
        public TcpServerChannel(QueueChannel qChannel, string hostName)
        {
            Settings = QueueServerSettings.LoadTcpConfigServer(hostName);
            QueueChannel = qChannel;
            //Settings.AllowedIp = allowedIps;
        }

        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="settings"></param>
        public TcpServerChannel(QueueChannel qChannel, TcpSettings settings)
        //: base()
        {
            Settings = settings;
            QueueChannel = qChannel;
        }


        #endregion

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected virtual void OnStart()
        {
            //base.OnStart();
            AgentManager.StartController();
            Log.Info("TcpServerChannel started :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected virtual void OnStop()
        {
            //base.OnStop();
            AgentManager.StopController();
            Log.Info("TcpServerChannel stoped :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        #endregion

        #region Initilize
        private void Init()
        {

            if (Initilized)
                return;
            IsReady = false;

            AllowedIPs = Settings.AllowedListIp;
            Port = Settings.Port;
            ReceiveBufferSize = Settings.ReceiveBufferSize;
            OnLoad();
            Log.Info("TcpServer Initilized...\n");
            IsReady = true;
        }

        protected virtual void OnLoad()
        {

        }

        protected virtual void OnPause()
        {

        }

        protected virtual void OnInfo(string message)
        {
            Console.WriteLine(message);
            Log.Info(message);
        }

        protected virtual void OnFault(string message)
        {
            Console.WriteLine(message);
            Log.Error(message);
        }

        //protected virtual void OnFault(string message, Exception ex)
        //{
        //    Log.Exception(message, ex, true);
        //}

        public void Start()
        {
            try
            {
                if (_State == ChannelServiceState.Paused)
                {
                    if (Initilized)
                    {
                        _State = ChannelServiceState.Started;
                        OnStart();
                        return;
                    }
                }
                if (_State == ChannelServiceState.Started)
                    return;

                //Listen = true;
                Init();
                _State = ChannelServiceState.Started;
                OnStart();
                StartServer();
            }
            catch (Exception ex)
            {
                //Listen = false;
                _State = ChannelServiceState.None;
                OnFault("The tcp server on start throws the error: " + ex.Message);
            }
        }

        public void Stop()
        {
            //Listen = false;
            StopServer();
            Initilized = false;
            _State = ChannelServiceState.Stoped;
            OnStop();
            Log.Info("TcpServer stoped: {0}", Settings.HostName);
        }

        public void Pause()
        {
            //Listen = false;
            _State = ChannelServiceState.Paused;
            OnPause();
            Log.Debug("TcpServer paused: {0}", Settings.HostName);
        }

        #endregion

        #region Read/Write
        /*
        /// <summary>
        /// Read Request from client.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected virtual IQueueRequest ReadRequest(NetStream stream)
        {

        }

        /// <summary>
        /// Exec client requset.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual TransStream ExecRequset(IQueueRequest request)
        {

        }

        /// <summary>
        /// Write response to client.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bResponse"></param>
        protected virtual void WriteResponse(NetworkStream stream, TransStream bResponse)
        {
            if (bResponse == null)
            {
                return;
            }
            var bytes = bResponse.GetBytes();
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }
            stream.Write(bytes, 0, bytes.Length);
        }
        */
        #endregion

        #region Read/Write Async
        /*
        /// <summary>
        /// Read Request from client.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected virtual async Task<IQueueRequest> ReadRequestAsync(NetStream stream)
        {
            return await Task.Run(() =>
            {
                return ReadRequest(stream);
            });
        }

        /// <summary>
        /// Exec client requset.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual async Task<TransStream> ExecRequsetAsync(IQueueRequest request)
        {
            return await Task.Run(() =>
            {
                return ExecRequset(request);
            });
        }

        /// <summary>
        /// Write response to client.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bResponse"></param>
        protected virtual async Task WriteResponseAsync(NetworkStream stream, TransStream bResponse)
        {
            if (bResponse == null)
            {
                return;
            }
            var bytes = bResponse.GetBytes();
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
        */
        #endregion

        #region abstract methods
        /// <summary>
        /// Execute client request and return response as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected virtual TransStream ExecRequset(IQueueRequest message)
        {
            return AgentManager.Queue.ExecRequset(message);
        }
        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected virtual IQueueRequest ReadRequest(NetStream stream)
        {
            //IQueueMessage message = null;
            //using (var ntStream = new NetStream())
            //{
            //    ntStream.CopyFrom(stream, readTimeout, ReceiveBufferSize);

            //    if (QueueChannel == QueueChannel.Producer)
            //        message= new QueueMessage(stream, null);
            //    else
            //        message= new QueueRequest(stream);
            //}
            //return message;

            if (QueueChannel == QueueChannel.Producer)
                return new QueueMessage(stream, null);
            else
                return new QueueRequest(stream);
        }

        /// <summary>
        /// Write response to client.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bResponse"></param>
        protected virtual async Task WriteResponseAsync(NetworkStream stream, TransStream bResponse)
        {
            if (bResponse == null)
            {
                return;
            }
            var bytes = bResponse.GetBytes();
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        #endregion

        #region Server

        private static Socket _serverSocket;
        private static bool _isRunning = false;

        public void StartServer()
        {
            if (_isRunning)
            {
                OnInfo("Server is already running.");
                return;
            }
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            _serverSocket.Listen(100);
            _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); // Enable Keep-Alive
            _isRunning = true;
            OnStart();
            OnInfo(string.Format("Server started on port :{0},  AllowedIPs: {1}", Port, AllowedIPs == null ? "NA" : AllowedIPs.JoinTrim()));
            Task.Run(() => AcceptClientsAsync());
        }

        public void StopServer()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _serverSocket.Close();
                OnInfo("Server stopped.");
            }
        }

        public async Task AcceptClientsAsync()
        {
            Console.WriteLine($"Server started on port {Port}...");

            while (_isRunning)
            {
                var clientSocket = await _serverSocket.AcceptAsync();
                string clientIP = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();

                if (!IsAllowedIP(clientIP))
                {
                    OnFault("Rejected connection from " + clientIP);
                    clientSocket.Close();
                    continue;
                }

                Console.WriteLine("Client connected!");

                _ = Task.Run(() => HandleClientAsync(clientSocket)); // Handle client in a separate task
            }
        }
          
        private async Task HandleClientAsync(Socket clientSocket)
        {
            try
            {
                byte[] buffer = new byte[8192]; // Optimized buffer size
                NetStream stream = new NetStream();

                // 🔹 Step 1: Read data from client
                int bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (bytesRead == 0) // 🔥 Detect EOF (client disconnected)
                {
                    Console.WriteLine("Client disconnected before sending data.");
                    return;
                }
                else //if (bytesRead > 0)
                {
                    OnInfo($"Received {bytesRead} bytes...");
                    await stream.WriteAsync(buffer, 0, bytesRead);

                    var item = ReadRequest(stream);

                    OnInfo("ReadRequestAsync ok");
                    var ts = ExecRequset(item);

                    OnInfo("ExecRequsetAsync ok");
                    if (item.DuplexType.IsDuplex())
                    {
                        //await WriteResponseAsync(clientSocket, ts);

                        await clientSocket.SendAsync(new ArraySegment<byte>(ts.GetBytes()), SocketFlags.None);

                        OnInfo("WriteResponseAsync completed");
                    }

                    //string receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    //Console.WriteLine($"📩 Received: {receivedMessage}");

                    //// 🔹 Step 2: Process the message (modify response)
                    //string responseMessage = $"✅ Server received: {receivedMessage}";
                    //byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseMessage);

                    // 🔹 Step 3: Send response to client
                    //await clientSocket.SendAsync(new ArraySegment<byte>(responseBytes), SocketFlags.None);
                    //Console.WriteLine($"📤 Sent response: {responseMessage}");
                }
            }
            catch (Exception ex)
            {
                OnFault($"Error handling client: {ex.Message}");
            }
            finally
            {
                clientSocket.Close();
                Console.WriteLine("Client disconnected.");
            }
        }

        //private async Task HandleClientAsync(Socket clientSocket)
        //{
        //    try
        //    {
        //        byte[] buffer = new byte[8192]; // Optimized buffer size
        //        int bytesRead;
        //        //int offset = 0;
        //        NetStream stream = new NetStream();
        //        while ((bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None)) > 0)
        //        {
        //            Console.WriteLine($"Received {bytesRead} bytes...");
        //            await stream.WriteAsync(buffer, 0, bytesRead);
        //            //await stream.WriteAsync(buffer, offset, buffer.Length);
        //            //offset += bytesRead;
        //            // Process data (e.g., echo back)
        //            //await clientSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), SocketFlags.None);
        //            Console.WriteLine($"Sent {bytesRead} bytes...");
        //        }
        //        OnInfo($"Total received bytes: {stream.Length}");

        //        var item = ReadRequest(stream);

        //        OnInfo("ReadRequestAsync ok");
        //        var ts = ExecRequset(item);

        //        OnInfo("ExecRequsetAsync ok");
        //        if (item.DuplexType.IsDuplex())
        //        {
        //            //await WriteResponseAsync(clientSocket, ts);
                    
        //            await clientSocket.SendAsync(new ArraySegment<byte>(ts.GetBytes()), SocketFlags.None);

        //            OnInfo("WriteResponseAsync completed");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error handling client: {ex.Message}");
        //    }
        //    finally
        //    {
        //        clientSocket.Close();
        //        Console.WriteLine("Client disconnected.");
        //    }
        //}

        private bool IsAllowedIP(string ip)
        {
            return Array.Exists(AllowedIPs, allowedIp => allowedIp == ip);
        }



        //private async Task AcceptClientsAsync(CancellationToken token)
        //{
        //    while (!token.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            TcpClient client = await _listener.AcceptTcpClientAsync();
        //            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

        //            if (!IsAllowedIP(clientIP))
        //            {
        //                OnFault("Rejected connection from " + clientIP);
        //                client.Close();
        //                continue;
        //            }

        //            OnInfo($"🔗 Client connected from {clientIP}");

        //            _ = HandleClientAsync(client);
        //        }
        //        catch (Exception ex)
        //        {
        //            if (!token.IsCancellationRequested)
        //                OnFault($"⚠️ Error accepting client: {ex.Message}");
        //        }
        //    }
        //}



        //private async Task HandleClientAsync(TcpClient client)
        //{
        //    try
        //    {
        //        using (NetworkStream stream = client.GetStream())
        //        {
        //            //using (MemoryStream memoryStream = new MemoryStream()) // Efficient byte storage
        //            NetStream memoryStream = new NetStream();
        //            {
        //                //StringBuilder fullMessage = new StringBuilder();
        //                byte[] buffer = new byte[ReceiveBufferSize]; // Large buffer size
        //                int bytesRead;

        //                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //                {
        //                    await memoryStream.WriteAsync(buffer, 0, bytesRead); // Store bytes in memory
        //                    OnInfo($"📩 Received {bytesRead} bytes...");
        //                }
        //                //byte[] receivedData = memoryStream.ToArray(); // Convert to byte array
        //                OnInfo($"✅ Total received bytes: {memoryStream.Length}");

        //                var request = ReadRequest(memoryStream);
        //                OnInfo("ReadRequestAsync ok");
        //                var response = ExecRequset(request);
        //                OnInfo("ExecRequsetAsync ok");
        //                if (request.DuplexType.IsDuplex())
        //                {
        //                    await WriteResponseAsync(stream, response);
        //                    OnInfo("WriteResponseAsync completed");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        OnFault($"⚠️ Error handling client: {ex.Message}");
        //    }
        //    finally
        //    {
        //        client.Close();
        //        OnInfo("🔌 Client disconnected.");
        //    }
        //}

        #endregion
    }
#endif

    /*
    /// <summary>
    /// Represent a queue tcp server listner.
    /// </summary>
    public class __TcpServerChannel : SecureTcpServer<IQueueRequest, TransStream>
    {
        QueueChannel QueueChannel;

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            AgentManager.StartController();
            Log.Info("TcpServerChannel started :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            AgentManager.StopController();
            Log.Info("TcpServerChannel stoped :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
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
        /// <param name="qChannel"></param>
        /// <param name="hostName"></param>
        public TcpServerChannel(QueueChannel qChannel, string hostName)
        {
            Settings = QueueServerSettings.LoadTcpConfigServer(hostName);
            QueueChannel = qChannel;
            //Settings.AllowedIp = allowedIps;
        }

        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="settings"></param>
        public TcpServerChannel(QueueChannel qChannel, TcpSettings settings)
        //: base()
        {
            Settings = settings;
            QueueChannel = qChannel;
        }

        #endregion

        #region abstract methods
        /// <summary>
        /// Execute client request and return response as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override TransStream ExecRequset(IQueueRequest message)
        {
            return AgentManager.Queue.ExecRequset(message);
        }
        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected override IQueueRequest ReadRequest(NetStream stream)
        {
            //IQueueMessage message = null;
            //using (var ntStream = new NetStream())
            //{
            //    ntStream.CopyFrom(stream, readTimeout, ReceiveBufferSize);

            //    if (QueueChannel == QueueChannel.Producer)
            //        message= new QueueMessage(stream, null);
            //    else
            //        message= new QueueRequest(stream);
            //}
            //return message;

            if (QueueChannel == QueueChannel.Producer)
                return new QueueMessage(stream, null);
            else
                return new QueueRequest(stream);
        }

        #endregion
    }
    */


    /*
    /// <summary>
    /// Represent a queue tcp server listner.
    /// </summary>
    public class TcpServerChannel : TcpServer<IQueueRequest>
    {
        QueueChannel QueueChannel;

        #region override
        /// <summary>
        /// OnStart
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            AgentManager.StartController();
            Log.Info("TcpServerChannel started :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
        }
        /// <summary>
        /// OnStop
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            AgentManager.StopController();
            Log.Info("TcpServerChannel stoped :{0}, QueueChannel:{1}", this.Settings.HostName, QueueChannel.ToString());
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
        /// <param name="qChannel"></param>
        /// <param name="hostName"></param>
        public TcpServerChannel(QueueChannel qChannel, string hostName)
         {
            Settings = QueueServerSettings.LoadTcpConfigServer(hostName);
            QueueChannel = qChannel;
        }

        /// <summary>
        /// Constractor using <see cref="TcpSettings"/> settings.
        /// </summary>
        /// <param name="qChannel"></param>
        /// <param name="settings"></param>
        public TcpServerChannel(QueueChannel qChannel, TcpSettings settings)
        //: base()
        {
            Settings = settings;
            QueueChannel = qChannel;
        }

        #endregion

        #region abstract methods
        /// <summary>
        /// Execute client request and return response as stream.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected override TransStream ExecRequset(IQueueRequest message)
        {
            return AgentManager.Queue.ExecRequset(message);
        }
        /// <summary>
        /// Read Request
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected override IQueueRequest ReadRequest(NetworkStream stream)
        {
            //IQueueMessage message = null;
            //using (var ntStream = new NetStream())
            //{
            //    ntStream.CopyFrom(stream, readTimeout, ReceiveBufferSize);

            //    if (QueueChannel == QueueChannel.Producer)
            //        message= new QueueMessage(stream, null);
            //    else
            //        message= new QueueRequest(stream);
            //}
            //return message;

            if (QueueChannel == QueueChannel.Producer)
                return new QueueMessage(stream, null);
            else
                return new QueueRequest(stream);
        }
       
        #endregion
    }
    */


}
