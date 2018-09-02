﻿using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using Nistec.Runtime;
using Nistec.Generic;
using Nistec.IO;
using System.Security.Principal;
using Nistec.Logging;
using Nistec.Channels;


namespace Nistec.Messaging.Server.Pipe
{


    /// <summary>
    /// Represent a pipe server listner
    /// </summary>
    public abstract class PipeServerBase //where TRequest : IDisposable
    {
        #region membrs
        private int numThreads;
        private bool Listen;
        private bool Initilize = false;
        private bool IsAsync = false;
        Thread[] servers;

        ILogger _Logger = Logger.Instance;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Log { get { return _Logger; } set { if (value != null)_Logger = value; } }

        #endregion

        #region settings
        /// <summary>
        /// Get or Set the host name.
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Get or Set the pipe name.
        /// </summary>
        public string PipeName { get; set; }
        /// <summary>
        /// Get or Set the pipe direction
        /// </summary>
        public PipeDirection PipeDirection { get; set; }
        /// <summary>
        /// Get or Set the pipe options.
        /// </summary>
        public PipeOptions PipeOptions { get; set; }
        /// <summary>
        /// Get or Set the max server connections.
        /// </summary>
        public int MaxServerConnections { get; set; }
        /// <summary>
        /// Get or Set the max allowed server instances
        /// </summary>
        public int MaxAllowedServerInstances { get; set; }
        /// <summary>
        /// Get or Set the verify pipe for security.
        /// </summary>
        public string VerifyPipe { get; set; }
        /// <summary>
        /// Get or Set the connection timeout.
        /// </summary>
        public uint ConnectTimeout { get; set; }
        //public bool IsApi { get; set; }
        /// <summary>
        /// Get or Set the in buffer size in bytes.
        /// </summary>
        public int ReceiveBufferSize { get; set; }
        /// <summary>
        /// Get or Set the out buffer size in bytes.
        /// </summary>
        public int SendBufferSize { get; set; }
        /// <summary>
        /// Local Server name
        /// </summary>
        public const string ServerName = ".";
        /// <summary>
        /// Get the full pipe name.
        /// </summary>
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
            //PipeName = name;
            //VerifyPipe = name;

            PipeSettings settings = new PipeSettings(name, true, loadFromSettings);
            this.HostName = name;
            this.PipeName = settings.PipeName;
            this.ConnectTimeout = settings.ConnectTimeout;
            this.ReceiveBufferSize = settings.ReceiveBufferSize;
            this.SendBufferSize = settings.SendBufferSize;
            this.PipeDirection = settings.PipeDirection;
            this.PipeOptions = settings.PipeOptions;
            this.VerifyPipe = settings.VerifyPipe;
            this.MaxAllowedServerInstances = settings.MaxAllowedServerInstances;
            this.MaxServerConnections = settings.MaxServerConnections;
        }

        /// <summary>
        /// Constractor with settings
        /// </summary>
        /// <param name="settings"></param>
        protected PipeServerBase(PipeSettings settings)
        {
            this.HostName = settings.HostName;
            this.PipeName = settings.PipeName;
            this.ConnectTimeout = settings.ConnectTimeout;
            this.ReceiveBufferSize = settings.ReceiveBufferSize;
            this.SendBufferSize = settings.SendBufferSize;
            this.PipeDirection = settings.PipeDirection;
            this.PipeOptions = settings.PipeOptions;
            this.VerifyPipe = settings.VerifyPipe;
            this.MaxAllowedServerInstances = settings.MaxAllowedServerInstances;
            this.MaxServerConnections = settings.MaxServerConnections;

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
                if (IsAsync)
                    servers[i] = new Thread(RunAsync);
                else
                    servers[i] = new Thread(Run);
                servers[i].IsBackground = true;
                servers[i].Start();
            }
            OnLoad();

            Log.Info("Waiting for client connection...\n");

        }
        /// <summary>
        /// On load.
        /// </summary>
        protected virtual void OnLoad()
        {

        }
        /// <summary>
        /// On start.
        /// </summary>
        protected virtual void OnStart()
        {

        }
        /// <summary>
        /// On stop.
        /// </summary>
        protected virtual void OnStop()
        {

        }
        /// <summary>
        /// Start pipe server listner.
        /// </summary>
        /// <param name="isAsync"></param>
        public void Start(bool isAsync = false)
        {
            IsAsync = isAsync;
            Listen = true;
            Init();
            OnStart();
        }
        /// <summary>
        /// Stop pipe server listner.
        /// </summary>
        public void Stop()
        {
            Listen = false;
            OnStop();
        }

        #endregion

        #region Read/Write
        /// <summary>
        /// Read request
        /// </summary>
        /// <param name="pipeServer"></param>
        /// <returns></returns>
        protected abstract QueueItem ReadRequest(NamedPipeServerStream pipeServer);


        // protected abstract void WriteResponse(NamedPipeServerStream pipeServer, NetStream bResponse, TRequest message);

        /// <summary>
        /// Write response
        /// </summary>
        /// <param name="pipeServer"></param>
        /// <param name="bResponse"></param>
        protected virtual void WriteResponse(NamedPipeServerStream pipeServer, NetStream bResponse)
        {

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
        protected abstract TransStream ExecRequset(QueueItem request);

        /// <summary>
        /// Occured when client is connected.
        /// </summary>
        protected virtual void OnClientConnected()
        {

        }

        #endregion

        #region Run

        NamedPipeServerStream CreatePipeSecurity()
        {

            // Prepare the security attributes (the pipeSecurity parameter in 
            // the constructor of NamedPipeServerStream) for the pipe. 
            PipeSecurity pipeSecurity = null;
            pipeSecurity = CreateSystemIOPipeSecurity();


            // Create the named pipe.
            return new NamedPipeServerStream(
                PipeName,                                           // The unique pipe name.
                PipeDirection,                                      // The pipe is duplex
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Message,                       // Message-based communication
                PipeOptions,                                        // No additional parameters
                ReceiveBufferSize,                                       // Input buffer size
                SendBufferSize,                                      // Output buffer size
                pipeSecurity,                                       // Pipe security attributes
                HandleInheritability.None                          // Not inheritable
                //PipeAccessRights.FullControl
                );
        }

        NamedPipeServerStream CreatePipeAccessControl()
        {
            // Fix up the DACL on the pipe before opening the listener instance
            // This won't disturb the SACL containing the mandatory integrity label
            NamedPipeServerStream pipeServer = new NamedPipeServerStream(
                        PipeName,                                           // The unique pipe name.
                        PipeDirection,                                      // The pipe is duplex
                        NamedPipeServerStream.MaxAllowedServerInstances,    // MaxAllowedServerInstances
                        PipeTransmissionMode.Message,                       // Byte|Message-based communication
                        PipeOptions.None,                                   // No additional parameters
                        ReceiveBufferSize,                                       // Input buffer size
                        SendBufferSize,                                      // Output buffer size
                        null,                                               // Pipe security attributes
                        HandleInheritability.None,                          // Not inheritable
                        PipeAccessRights.ChangePermissions);

            PipeSecurity ps = pipeServer.GetAccessControl();

            PipeAccessRule aceClients = new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), // or some other group defining the allowed clients
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow);
            PipeAccessRule aceOwner = new PipeAccessRule(
                WindowsIdentity.GetCurrent().Owner,
                PipeAccessRights.FullControl,
                AccessControlType.Allow);

            ps.AddAccessRule(aceClients);
            ps.AddAccessRule(aceOwner);

            pipeServer.SetAccessControl(ps);

            return pipeServer;
        }

        /// <summary>
        /// Use the pipe classes in the System.IO.Pipes namespace to create the 
        /// named pipe. This solution is recommended.
        /// </summary>
        private void Run()
        {
            NamedPipeServerStream pipeServer = null;
            QueueItem message = null;
            bool connected = false;
            //const string ResponseMessage = "Default response from server\0";
            Console.WriteLine("{0} Pipe server start listen Thread<{1}>", PipeName, Thread.CurrentThread.ManagedThreadId);

            while (Listen)
            {


                try
                {

                    pipeServer = CreatePipeAccessControl();

                    // Wait for the client to connect.
                    //Netlog.Info("Waiting for the client's connection...");
                    pipeServer.WaitForConnection();
                    connected = true;
                    //Netlog.Info("Client is connected.");

                    OnClientConnected();

                    message = ReadRequest(pipeServer);

                    NetStream res = ExecRequset(message).GetStream();

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
                    Log.Exception("The pipe server throws the error: ", ex, true);
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
                        message.Dispose();
                        message = null;
                    }
                }
            }
            Console.WriteLine("{0} Pipe server stope listen Thread<{1}>", PipeName, Thread.CurrentThread.ManagedThreadId);

        }



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

                    pipeServerAsync = CreatePipeAccessControl();

                    // Wait for the client to connect.
                    AsyncCallback myCallback = new AsyncCallback(WaitForConnectionAsyncCallback);
                    IAsyncResult asyncResult = pipeServerAsync.BeginWaitForConnection(myCallback, pipeServerAsync);

                    while (!asyncResult.IsCompleted)
                    {
                        Thread.Sleep(100);
                    }

                    connected = true;

                    OnClientConnected();

                    //pipeServer.Flush();

                    // Flush the pipe to allow the client to read the pipe's contents 
                    // before disconnecting. Then disconnect the client's connection.
                    pipeServerAsync.WaitForPipeDrain();
                    pipeServerAsync.Disconnect();
                    connected = false;
                }
                catch (Exception ex)
                {
                    Log.Exception("The pipe sync server throws the error: ", ex, true);
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


        private void WaitForConnectionAsyncCallback(IAsyncResult result)
        {
            QueueItem message = null;
            try
            {
                NamedPipeServerStream pipeServerAsync = (NamedPipeServerStream)result.AsyncState;

                pipeServerAsync.EndWaitForConnection(result);

                message = ReadRequest(pipeServerAsync);

                NetStream res = ExecRequset(message).GetStream();

                WriteResponse(pipeServerAsync, res);
            }
            catch (OperationCanceledException oex)
            {
                Log.Exception("Pipe server error, The pipe was canceled: ", oex);
            }
            catch (Exception ex)
            {
                Log.Exception("Pipe server error: ", ex, true);
            }
            finally
            {
                if (message != null)
                {
                    message.Dispose();
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
        // <see cref="http://msdn.microsoft.com/en-us/library/aa365600(VS.85).aspx"/>
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
