using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using Nistec.Channels;
using Nistec.Runtime;
using Nistec.Generic;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Nistec.IO;
using Nistec.Messaging.Remote;



namespace Nistec.Messaging.Server
{

    public abstract class PipeServerBase
    {
        #region membrs
        private int numThreads;
        private bool Listen;
        private bool Initilize = false;
        private bool IsAsync = false;
        Thread[] servers;
        #endregion

        #region settings

        public string PipeName { get; set; }
        public PipeDirection PipeDirection { get; set; }
        public PipeOptions PipeOptions { get; set; }
        public int MaxServerConnections { get; set; }
        public int MaxAllowedServerInstances { get; set; }
        public string VerifyPipe { get; set; }
        public uint ConnectTimeout { get; set; }
        public bool IsApi { get; set; }

        public int InBufferSize { get; set; }
        public int OutBufferSize { get; set; }
        public const string ServerName = ".";
        public string FullPipeName { get { return @"\\" + ServerName + @"\pipe\" + PipeName; } }


        #endregion

        #region ctor

        /// <summary>
        /// Constractor with extra parameters
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loadFromSettings"></param>
        protected PipeServerBase(string name, bool loadFromSettings)
        {
            PipeName = name;
            VerifyPipe = name;

            PipeQueueSettings settings = new PipeQueueSettings(name, true, loadFromSettings);
            this.PipeName = settings.PipeName;
            this.ConnectTimeout = settings.ConnectTimeout;
            this.InBufferSize = settings.InBufferSize;
            this.OutBufferSize = settings.OutBufferSize;
            this.PipeDirection = settings.PipeDirection;
            this.PipeOptions = settings.PipeOptions;
            this.VerifyPipe = settings.VerifyPipe;
            this.MaxAllowedServerInstances = settings.MaxAllowedServerInstances;
            this.MaxServerConnections = settings.MaxServerConnections;
            this.IsApi = settings.IsApi;
        }

        /// <summary>
        /// Constractor with settings
        /// </summary>
        /// <param name="settings"></param>
        protected PipeServerBase(PipeQueueSettings settings)
        {
            this.PipeName = settings.PipeName;
            this.ConnectTimeout = settings.ConnectTimeout;
            this.InBufferSize = settings.InBufferSize;
            this.OutBufferSize = settings.OutBufferSize;
            this.PipeDirection = settings.PipeDirection;
            this.PipeOptions = settings.PipeOptions;
            this.VerifyPipe = settings.VerifyPipe;
            this.MaxAllowedServerInstances = settings.MaxAllowedServerInstances;
            this.MaxServerConnections = settings.MaxServerConnections;
            this.IsApi = settings.IsApi;

        }
        #endregion

        #region Initilize

        private void Init()
        {
            if (Initilize)
                return;
            numThreads = MaxServerConnections;
            servers = new Thread[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                //if (IsAsync)
                //    servers[i] = new Thread(RunAsync);
                //else
                //    servers[i] = new Thread(Run);

                servers[i] = new Thread(Run);
                servers[i].IsBackground = true;
                servers[i].Start();
            }
            OnLoad();

            Netlog.InfoFormat("Waiting for client connection {0}...\n", PipeName);

        }

        protected virtual void OnLoad()
        {

        }

        protected virtual void OnStart()
        {

        }

        protected virtual void OnStop()
        {

        }

        public void Start(bool isAsync)
        {
            IsAsync = isAsync;
            Listen = true;
            Init();
            OnStart();
        }

        public void Stop()
        {
            Listen = false;
            OnStop();
        }

        #endregion

        #region Read/Write

        protected virtual void WriteResponse(NamedPipeServerStream pipeServer, NetStream bResponse)
        {
            //if (response == null || response.IsDuplex==false)
            //{
            //    return;
            //}

            //byte[] bResponse = SerilaizeResponse(response);

            if (bResponse == null)
            {
                return;
            }

            int cbResponse = bResponse.iLength;

            pipeServer.Write(bResponse.ToArray(), 0, cbResponse);

            pipeServer.Flush();

        }

        /// <summary>
        /// Exec Requset
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual void Execute(NamedPipeServerStream stream)
        {
            //Message message = new Message();
            //using (var copy = NetStream.CopyStream(stream))
            //{
            //    Netlog.DebugFormat("Server Read Request length:{0}", copy.Length);
            //    message.EntityRead(copy, null);
            //}
            //Netlog.DebugFormat("Server Print Request :{0}", message.Print());

            QueueItemStream item = QueueItemStream.Create(NetStream.CopyStream(stream));

            //stream.Position = 0;
            AgentManager.Queue.Execute(item, stream);
        }


        ///// <summary>
        ///// Exec Requset
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //protected abstract NetStream ExecRequset(QueueRequest request);

        //protected abstract QueueRequest ReadRequest(NamedPipeServerStream stream);

        #endregion

        #region Run
        /*
        /// <summary>
        /// Use the pipe classes in the System.IO.Pipes namespace to create the 
        /// named pipe. This solution is recommended.
        /// </summary>
        private void Run()
        {
            NamedPipeServerStream pipeServer = null;
            QueueMessage message = null;
            bool connected = false;
            //const string ResponseMessage = "Default response from server\0";
            Console.WriteLine("{0} Pipe server start listen Thread<{1}>", PipeName, Thread.CurrentThread.ManagedThreadId);

            while (Listen)
            {


                try
                {
                    // Prepare the security attributes (the pipeSecurity parameter in 
                    // the constructor of NamedPipeServerStream) for the pipe. 
                    PipeSecurity pipeSecurity = null;
                    pipeSecurity = CreateSystemIOPipeSecurity();


                    // Create the named pipe.
                    pipeServer = new NamedPipeServerStream(
                        PipeName,                       // The unique pipe name.
                        PipeDirection,            // The pipe is duplex
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Message,   // Message-based communication
                        PipeOptions,               // No additional parameters
                        InBufferSize,                   // Input buffer size
                        OutBufferSize,                  // Output buffer size
                        pipeSecurity,                   // Pipe security attributes
                        HandleInheritability.None       // Not inheritable
                        );

                    //Netlog.InfoFormat("The named pipe ({0}) is created.", FullPipeName);

                    // Wait for the client to connect.
                    //Netlog.Info("Waiting for the client's connection...");
                    pipeServer.WaitForConnection();
                    connected = true;
                    //Netlog.Info("Client is connected.");

                    //message = QueueItem.ReadServerRquest(pipeServer, InBufferSize);

                    message=ReadRequest(pipeServer);

                    NetStream res = ExecRequset(message);

                    WriteResponse(pipeServer, res);

                    //pipeServer.Flush();

                    // Flush the pipe to allow the client to read the pipe's contents 
                    // before disconnecting. Then disconnect the client's connection.
                    pipeServer.WaitForPipeDrain();
                    pipeServer.Disconnect();
                    connected = false;
                }
                catch (Exception ex)
                {
                    Netlog.Exception("The server throws the error: ", ex, ex.InnerException);
                }
                finally
                {
                    if (pipeServer != null)
                    {
                        if (connected && pipeServer.IsConnected)
                        {
                            pipeServer.Disconnect();
                        }
                        pipeServer.Close();
                        pipeServer = null;
                    }
                    if (message != null)
                    {
                      ((IDisposable) message).Dispose();
                        message = null;
                    }
                }
            }
            Console.WriteLine("{0} Pipe server stope listen Thread<{1}>", PipeName, Thread.CurrentThread.ManagedThreadId);

        }
        */

        /// <summary>
        /// Use the pipe classes in the System.IO.Pipes namespace to create the 
        /// named pipe. This solution is recommended.
        /// </summary>
        /// <param name="isAsync"></param>
        private void Run()
        {
            Console.WriteLine("{0} Pipe server start listen Thread<{1}>", PipeName, Thread.CurrentThread.ManagedThreadId);

            while (Listen)
            {
                Exec(IsAsync);
            }
            Console.WriteLine("{0} Pipe server stope listen Thread<{1}>", PipeName, Thread.CurrentThread.ManagedThreadId);
        }

        private void Exec(bool isAsync)
        {
            NamedPipeServerStream pipeServer = null;
            Message message = null;
            bool connected = false;

            try
            {
                // Prepare the security attributes (the pipeSecurity parameter in 
                // the constructor of NamedPipeServerStream) for the pipe. 
                PipeSecurity pipeSecurity = null;
                pipeSecurity = CreateSystemIOPipeSecurity();


                // Create the named pipe.
                pipeServer = new NamedPipeServerStream(
                    PipeName,                       // The unique pipe name.
                    PipeDirection,            // The pipe is duplex
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message,   // Message-based communication
                    PipeOptions,               // No additional parameters
                    InBufferSize,                   // Input buffer size
                    OutBufferSize,                  // Output buffer size
                    pipeSecurity,                   // Pipe security attributes
                    HandleInheritability.None       // Not inheritable
                    );

                if (isAsync)
                {
                    AsyncCallback myCallback = new AsyncCallback(WaitForConnectionAsyncCallback);
                    IAsyncResult asyncResult = pipeServer.BeginWaitForConnection(myCallback, pipeServer);

                    while (!asyncResult.IsCompleted)
                    {
                        Thread.Sleep(100);
                    }

                    connected = true;
                }
                else
                {
                    //Netlog.InfoFormat("The named pipe ({0}) is created.", FullPipeName);

                    // Wait for the client to connect.
                    //Netlog.Info("Waiting for the client's connection...");
                    pipeServer.WaitForConnection();
                    connected = true;
                    //Netlog.Info("Client is connected.");

                    //message = QueueItem.ReadServerRquest(pipeServer, InBufferSize);

                    //message = ReadRequest(pipeServer);

                    //NetStream res = ExecRequset(message);

                    //WriteResponse(pipeServer, res);

                    Execute(pipeServer);

                    //pipeServer.Flush();
                }
                // Flush the pipe to allow the client to read the pipe's contents 
                // before disconnecting. Then disconnect the client's connection.
                pipeServer.WaitForPipeDrain();
                pipeServer.Disconnect();
                connected = false;
            }
            catch (Exception ex)
            {
                Netlog.Exception("The server throws the error: ", ex, true,true);
            }
            finally
            {
                if (pipeServer != null)
                {
                    if (connected && pipeServer.IsConnected)
                    {
                        pipeServer.Disconnect();
                    }
                    pipeServer.Close();
                    pipeServer = null;
                }
                if (message != null)
                {
                    ((IDisposable)message).Dispose();
                    message = null;
                }
            }

        }

        /*
        /// <summary>
        /// Use the pipe classes in the System.IO.Pipes namespace to create the 
        /// named pipe. This solution is recommended.
        /// </summary>
        private void RunAsync()
        {
            NamedPipeServerStream pipeServerAsync = null;
            bool connected = false;
            //const string ResponseMessage = "Default response from server\0";
            Console.WriteLine("{0} Pipe server async start listen Thread<{1}>", PipeName, Thread.CurrentThread.ManagedThreadId);

            while (Listen)
            {


                try
                {
                    // Prepare the security attributes (the pipeSecurity parameter in 
                    // the constructor of NamedPipeServerStream) for the pipe. 
                    PipeSecurity pipeSecurity = null;
                    pipeSecurity = CreateSystemIOPipeSecurity();


                    // Create the named pipe.
                    pipeServerAsync = new NamedPipeServerStream(
                        PipeName,                       // The unique pipe name.
                        PipeDirection,            // The pipe is duplex
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Message,   // Message-based communication
                        PipeOptions,               // No additional parameters
                        InBufferSize,                   // Input buffer size
                        OutBufferSize,                  // Output buffer size
                        pipeSecurity,                   // Pipe security attributes
                        HandleInheritability.None       // Not inheritable
                        );

                    //Netlog.InfoFormat("The named pipe ({0}) is created.", FullPipeName);

                    // Wait for the client to connect.
                    //Netlog.Info("Waiting for the client's connection...");
                    AsyncCallback myCallback = new AsyncCallback(WaitForConnectionAsyncCallback);
                    IAsyncResult asyncResult = pipeServerAsync.BeginWaitForConnection(myCallback, pipeServerAsync);

                    while (!asyncResult.IsCompleted)
                    {
                        Thread.Sleep(100);
                    }

                    connected = true;
                    //Netlog.Info("Client is connected.");

                    //pipeServer.Flush();

                    // Flush the pipe to allow the client to read the pipe's contents 
                    // before disconnecting. Then disconnect the client's connection.
                    pipeServerAsync.WaitForPipeDrain();
                    pipeServerAsync.Disconnect();
                    connected = false;
                }
                catch (Exception ex)
                {
                    Netlog.Exception("The server throws the error: ", ex, ex.InnerException);
                }
                finally
                {
                    if (pipeServerAsync != null)
                    {
                        if (connected && pipeServerAsync.IsConnected)
                        {
                            pipeServerAsync.Disconnect();
                        }
                        pipeServerAsync.Close();
                        pipeServerAsync = null;
                    }
                }
            }
            Console.WriteLine("{0} Pipe server async stop listen Thread<{1}>", PipeName, Thread.CurrentThread.ManagedThreadId);
        }
        */

        private void WaitForConnectionAsyncCallback(IAsyncResult result)
        {
            Message message = null;
            try
            {
                NamedPipeServerStream pipeServerAsync = (NamedPipeServerStream)result.AsyncState;

                pipeServerAsync.EndWaitForConnection(result);

                //message = QueueItem.ReadServerRquest(pipeServerAsync, InBufferSize);

                //message=ReadRequest(pipeServerAsync);

                //NetStream res = ExecRequset(message);

                //WriteResponse(pipeServerAsync, res);

                Execute(pipeServerAsync);
            }
            catch (OperationCanceledException oex)
            {
                Netlog.Exception("Pipe server error, The pipe was canceled: ", oex);
            }
            catch (Exception ex)
            {
                Netlog.Exception("Pipe server error: ", ex, true);
            }
            finally
            {
                if (message != null)
                {
                    ((IDisposable)message).Dispose();
                    message = null;
                }
            }
        }



        /// <summary>
        /// The CreateSystemIOPipeSecurity function creates a new PipeSecurity 
        /// object to allow Authenticated Users read and write access to a pipe, 
        /// and to allow the Administrators group full access to the pipe.
        /// </summary>
        /// <returns>
        /// A PipeSecurity object that allows Authenticated Users read and write 
        /// access to a pipe, and allows the Administrators group full access to 
        /// the pipe.
        /// </returns>
        /// <see cref="http://msdn.microsoft.com/en-us/library/aa365600(VS.85).aspx"/>
        static PipeSecurity CreateSystemIOPipeSecurity()
        {
            PipeSecurity pipeSecurity = new PipeSecurity();

            // Allow Everyone read and write access to the pipe.
            pipeSecurity.SetAccessRule(new PipeAccessRule("Authenticated Users",
                PipeAccessRights.ReadWrite, AccessControlType.Allow));

            // Allow the Administrators group full access to the pipe.
            pipeSecurity.SetAccessRule(new PipeAccessRule("Administrators",
                PipeAccessRights.FullControl, AccessControlType.Allow));

            return pipeSecurity;
        }
        #endregion
    }

}
