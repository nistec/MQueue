using System;
using System.Collections.Generic;
using System.Text;

using Nistec.Net.Tcp;
using Nistec.Net.Auth;
using System.IO;
using Nistec.Generic;
using Nistec.Messaging.Adapters;
using Nistec.Messaging.Remote;
using Nistec.Messaging.Server;
using Nistec.Messaging.Tcp;
using Nistec.Net;
using System.Net;

namespace Nistec.Messaging.Server
{
    /// <summary>
    /// This class implements Tcp server.
    /// </summary>
    public class TcpServer : Nistec.Net.Tcp.TcpServer<TcpMessage>
    {
        private int m_MaxBadCommands = 30;
        private int m_MaxTransactions = 10;
        private int m_MaxMessageSize = 10000000;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TcpServer()
        {
            //Load();
        }

       
        //void Load()
        //{
        //    this.LoadSettings(HostAddressTypes.tcp);
        //    if (Adapters.Count > 0)
        //    {
        //        List<IPBindInfo> binds = new List<IPBindInfo>();

        //        foreach (var adapter in Adapters)
        //        {
        //            IPBindInfo bindinfo = new IPBindInfo(adapter.Source.OriginalHostAddress, NetworkProtocol.Tcp, IPAddress.Any, adapter.Source.Port);
        //            binds.Add(bindinfo);
        //        }
        //        var first = Adapters[0];
        //        Bindings = binds.ToArray();
        //        MaxConnections = first.WorkerCount;
        //        MaxConnectionsPerIP = first.MaxItemsPerSession;
        //        TaskIdleTimeout = first.ConnectTimeout;
        //    }
        //}

        internal void Load(AdapterProperties[] adapters)
        {
            if (adapters != null && adapters.Length > 0)
            {
                List<IPBindInfo> binds = new List<IPBindInfo>();

                foreach (var adapter in Adapters)
                {
                    IPBindInfo bindinfo = new IPBindInfo(adapter.Source.HostAddress, NetworkProtocol.Tcp, IPAddress.Any, adapter.Source.Port);
                    binds.Add(bindinfo);
                }
                var first = Adapters[0];
                Bindings = binds.ToArray();
                MaxConnections = first.WorkerCount;
                MaxConnectionsPerIP = first.MaxItemsPerSession;
                TaskIdleTimeout = first.ConnectTimeout;
            }
        }

        //internal void LoadSettings(AdapterProperties adapter)
        //{
        //    if (adapter != null)
        //    {
        //        MaxConnections = adapter.WorkerCount;
        //        MaxConnectionsPerIP = adapter.MaxItemsPerSession;
        //        TaskIdleTimeout = adapter.ConnectTimeout;
        //    }
        //}

        #region properties

        List<AdapterProperties> _Adapters;
        /// <summary>
        /// Get QueueHostList.
        /// </summary>
        public List<AdapterProperties> Adapters
        {
            get
            {
                if (_Adapters == null)
                {
                    _Adapters = new List<AdapterProperties>();
                }
                return _Adapters;
            }
        }


        //bool _isalive = false;
        ///// <summary>
        ///// Get indicating whether the queue listener ia alive.
        ///// </summary>
        //public bool IsAlive
        //{
        //    get { return _isalive; }
        //}
        #endregion

        #region start/stop

        //FolderListener m_listener;

        ///// <summary>
        ///// Start the queue listener.
        ///// </summary>
        //public void Start()
        //{
        //    if (_isalive)
        //        return;

        //    this.LoadSettings();

        //    if (Adapters.Count > 0)
        //    {
        //        m_listener = new FolderListener(Adapters.ToArray());

        //        m_listener.Start();

        //        _isalive = true;

        //    }
        //    // Start queue listener...
        //    Console.WriteLine("FolderServer started...");
        //}

        ///// <summary>
        ///// Stop the queue listener.
        ///// </summary>
        //public void Stop(bool waitForWorkers)
        //{
        //    if (m_listener == null)
        //        return;

        //    m_listener.Stop(true);

        //    _isalive = false;
        //}

        #endregion

        protected override void OnTaskCreated(TcpMessage message)
        {
            base.OnTaskCreated(message);

            message.GetMessageStream += new EventHandler<TcpMessageEventArgs>(task_GetMessageStream);
            message.MessageStoringCanceled += new EventHandler(task_MessageStoringCanceled);
            message.MessageStoringCompleted += new EventHandler<TcpMessageCompletedEventArgs>(task_MessageStoringCompleted);


        }

        void task_MessageStoringCompleted(object sender, TcpMessageCompletedEventArgs e)
        {
            try
            {
                e.Stream.Position = 0;
                //ProcessAndStoreMessage(e.Session.From.ENVID, e.Session.From.Mailbox, e.Session.From.RET, e.Session.To, e.Stream, e);

                QueueItemStream item = QueueItemStream.Create(e.Stream);
                var stream = AgentManager.Queue.ExecRequset(item);

                //Message message = Message.Create(e.Stream);
                //var stream = AgentManager.Queue.ExecRequset(message);


                e.Reply = new TcpReplyStream(200, stream);

                
                //OnMessageCompleted(new GenericEventArgs<Message>(msg));
            }
            catch (Exception x)
            {

                OnError(x);

                e.Reply = new TcpReplyStream(552, "Requested action aborted: Internal server error.");
            }
            finally
            {
                // Close file. .NET will delete that file we use FileOptions.DeleteOnClose.
                if (e.Stream != null)
                {
                    ((FileStream)e.Stream).Dispose();
                }
            }
        }

        void task_MessageStoringCanceled(object sender, EventArgs e)
        {
            try
            {
                // Close file. .NET will delete that file we use FileOptions.DeleteOnClose.
                ((IDisposable)((TcpMessage)sender).Tags["MessageStream"]).Dispose();
            }
            catch
            {
                // We don't care about errors here.
            }
        }

        void task_GetMessageStream(object sender, TcpMessageEventArgs e)
        {
            //if (!Directory.Exists(m_MailStorePath + "IncomingSMTP"))
            //{
            //    Directory.CreateDirectory(m_MailStorePath + "IncomingSMTP");
            //}

            //e.Stream = new FileStream(API_Utlis.PathFix(m_MailStorePath + "IncomingSMTP\\" + Guid.NewGuid().ToString().Replace("-", "") + ".eml"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 32000, FileOptions.DeleteOnClose);
            //e.Task.Tags["MessageStream"] = e.Stream;
        }

        public event EventHandler<GenericEventArgs<Message>> MessageCompleted = null;

        protected virtual void OnMessageCompleted(GenericEventArgs<Message> e)
        {
            if (MessageCompleted != null)
                MessageCompleted(this, e);
        }

        protected virtual void OnError(Exception ex)
        {

        }

        // TODO:

        //public override Dispose


        #region override method OnMaxConnectionsExceeded

        /// <summary>
        /// Is called when new incoming message and server maximum allowed connections exceeded.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <remarks>This method allows inhereted classes to report error message to connected client.
        /// Task will be disconnected after this method completes.
        /// </remarks>
        protected override void OnMaxConnectionsExceeded(TcpMessage message)
        {
            message.TcpStream.WriteLine("421 Client host rejected: too many connections, please try again later.");
        }

        #endregion

        #region override method OnMaxConnectionsPerIPExceeded

        /// <summary>
        /// Is called when new incoming message and server maximum allowed connections per connected IP exceeded.
        /// </summary>
        /// <param name="message">Incoming message.</param>
        /// <remarks>This method allows inhereted classes to report error message to connected client.
        /// Task will be disconnected after this method completes.
        /// </remarks>
        protected override void OnMaxConnectionsPerIPExceeded(TcpMessage message)
        {
            message.TcpStream.WriteLine("421 Client host rejected: too many connections from your IP(" + message.RemoteEndPoint.Address + "), please try again later.");
        }

        #endregion

        #region Properties implementation


        /// <summary>
        /// Gets or sets how many bad commands message can have before it's terminated. Value 0 means unlimited.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid value is passed.</exception>
        public int MaxBadCommands
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_MaxBadCommands;
            }

            set
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if (value < 0)
                {
                    throw new ArgumentException("Property 'MaxBadCommands' value must be >= 0.");
                }

                m_MaxBadCommands = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum mail transactions per message. Value 0 means unlimited.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid value is passed.</exception>
        public int MaxTransactions
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_MaxTransactions;
            }

            set
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if (value < 0)
                {
                    throw new ArgumentException("Property 'MaxTransactions' value must be >= 0.");
                }

                m_MaxTransactions = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum message size in bytes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid value is passed.</exception>
        public int MaxMessageSize
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_MaxMessageSize;
            }

            set
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                if (value < 500)
                {
                    throw new ArgumentException("Property 'MaxMessageSize' value must be >= 500.");
                }

                m_MaxMessageSize = value;
            }
        }


        #endregion

        #region Events implementation



        #endregion


    }
}
