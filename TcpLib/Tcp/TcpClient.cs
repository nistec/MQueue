using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using Nistec.Net.IO;
using Nistec.Net.Tcp;
using Nistec.Net.Auth;
using Nistec.Net.Dns;
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Net;


namespace Nistec.Messaging.Tcp
{
    /// <summary>
    /// This class implements TcpMessage client. Defined in RFC 5321.
    /// </summary>
    /// <example>
    /// Simple way:
    /// <code>
    /// /*
    ///  To make this code to work, you need to import following namespaces:
    ///  using Nistec.Messaging.Net.TcpMessage.Client; 
    /// */
    /// 
    /// // You can send any valid TcpMessage message here, from disk,memory, ... or
    /// // you can use Nistec.Messaging.Net.Mail classes to compose valid TcpMessage mail message.
    /// 
    /// // TcpClient.QuickSendSmartHost(...
    /// or
    /// // TcpClient.QuickSend(...
    /// </code>
    /// 
    /// Advanced way:
    /// <code> 
    /// /*
    ///  To make this code to work, you need to import following namespaces:
    ///  using Nistec.Messaging.Net.TcpMessage.Client; 
    /// */
    /// 
    /// using(TcpClient smtp = new TcpClient()){      
    ///     // You can use Dns_Client.GetEmailHosts(... to get target recipient TcpMessage hosts for Connect method.
    ///		smtp.Connect("hostName",WellKnownPorts.TcpMessage); 
    ///		smtp.EhloHelo("mail.domain.com");
    ///     // Authenticate if target server requires.
    ///     // smtp.Auth(smtp.AuthGetStrongestMethod("user","password"));
    ///     smtp.MailFrom("sender@domain.com");
    ///     // Repeat this for all recipients.
    ///     smtp.RcptTo("to@domain.com");
    /// 
    ///     // Send message to server.
    ///     // You can send any valid TcpMessage message here, from disk,memory, ... or
    ///     // you can use Nistec.Messaging.Net.Mail classes to compose valid TcpMessage mail message.
    ///     // smtp.SendMessage(.... .
    ///     
    ///     smtp.Disconnect();
    ///	}
    /// </code>
    /// </example>
    public class TcpClient : Nistec.Net.Tcp.TcpClient
    {
        private string m_LocalHostName = null;
        private string m_RemoteHostName = null;
        //private string m_From = null;
        //private List<string> m_Recipients = null;
        private GenericIdentity m_AuthdUserIdentity = null;

        public TcpReplyStream ReplyStream { get; internal set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TcpClient()
        {
        }


        //mcontrol
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="host"></param>
        /// <param name="enablePool"></param>
        public TcpClient(string host, bool enablePool)
            : base(host, enablePool)
        {

        }

        #region override method Dispose

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion
        
        #region override method Disconnect

        /// <summary>
        /// Closes connection to TcpMessage server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TcpMessage client is not connected.</exception>
        public override void Disconnect()
        {
            Disconnect(true);
        }

        /// <summary>
        /// Closes connection to TcpMessage server.
        /// </summary>
        /// <param name="sendQuit">If true QUIT command is sent to TcpMessage server.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TcpMessage client is not connected.</exception>
        public void Disconnect(bool sendQuit)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("TcpMessage client is not connected.");
            }

            try
            {
                if (sendQuit)
                {
                    // Send QUIT command to server.                
                    WriteLine("QUIT");

                    // Read QUIT response.
                    ReadLine();
                }
            }
            catch
            {
            }

            m_LocalHostName = null;
            m_RemoteHostName = null;
           
            //m_From = null;
            //m_Recipients = null;
            m_AuthdUserIdentity = null;

            try
            {
                base.Disconnect();
            }
            catch
            {
            }
        }

        #endregion

        #region method Auth

        /// <summary>
        /// Gets strongest authentication method which we can support from TcpMessage server.
        /// Preference order DIGEST-MD5 -> CRAM-MD5 -> LOGIN -> PLAIN.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="password">User password.</param>
        /// <returns>Returns authentication method.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TcpMessage client is not connected .</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>userName</b> or <b>password</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="NotSupportedException">Is raised when TcpMessage server won't support authentication or we 
        /// don't support any of the server authentication mechanisms.</exception>
        public AuthSaslClient AuthGetStrongestMethod(string userName, string password)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            List<string> authMethods = new List<string>(this.SaslAuthMethods);
            if (authMethods.Count == 0)
            {
                throw new NotSupportedException("TcpMessage server does not support authentication.");
            }
            else if (authMethods.Contains("DIGEST-MD5"))
            {
                return new AuthSaslClientDigestMd5("TcpMessage", this.RemoteEndPoint.Address.ToString(), userName, password);
            }
            else if (authMethods.Contains("CRAM-MD5"))
            {
                return new AuthSaslClientCramMd5(userName, password);
            }
            else if (authMethods.Contains("LOGIN"))
            {
                return new AuthSaslClientLogin(userName, password);
            }
            else if (authMethods.Contains("PLAIN"))
            {
                return new AuthSaslClientPlain(userName, password);
            }
            else
            {
                throw new NotSupportedException("We don't support any of the TcpMessage server authentication methods.");
            }
        }

        

        /// <summary>
        /// Sends AUTH command to TcpMessage server.
        /// </summary>
        /// <param name="sasl">SASL authentication. You can use method <see cref="AuthGetStrongestMethod"/> to get strongest supported authentication.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TcpMessage client is not connected or is already authenticated.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public void Auth(AuthSaslClient sasl)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (this.IsAuthenticated)
            {
                throw new InvalidOperationException("Connection is already authenticated.");
            }
            if (sasl == null)
            {
                throw new ArgumentNullException("sasl");
            }

            ManualResetEvent wait = new ManualResetEvent(false);
            using (AuthAsyncOperation op = new AuthAsyncOperation(sasl))
            {
                op.CompletedAsync += delegate(object s1, EventArgs<AuthAsyncOperation> e1)
                {
                    wait.Set();
                };
                if (!this.AuthAsync(op))
                {
                    wait.Set();
                }
                wait.WaitOne();
                wait.Close();

                if (op.Error != null)
                {
                    throw op.Error;
                }
            }
        }

       

        #region class AuthAsyncOperation

        /// <summary>
        /// This class represents <see cref="TcpClient.AuthAsync"/> asynchronous operation.
        /// </summary>
        public class AuthAsyncOperation : IDisposable, IAsyncOperation
        {
            private object m_Lock = new object();
            private OperationState m_State = OperationState.Waiting;
            private Exception m_Exception = null;
            private TcpClient m_TcpClient = null;
            private AuthSaslClient m_SASL = null;
            private bool m_RiseCompleted = false;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="sasl">SASL authentication. You can use method <see cref="AuthGetStrongestMethod"/> to get strongest supported authentication.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>sasl</b> is null reference.</exception>
            public AuthAsyncOperation(AuthSaslClient sasl)
            {
                if (sasl == null)
                {
                    throw new ArgumentNullException("sasl");
                }

                m_SASL = sasl;
            }

            #region method Dispose

            /// <summary>
            /// Cleans up any resource being used.
            /// </summary>
            public void Dispose()
            {
                if (m_State == OperationState.Disposed)
                {
                    return;
                }
                SetState(OperationState.Disposed);

                m_Exception = null;
                m_TcpClient = null;

                this.CompletedAsync = null;
            }

            #endregion


            #region method Start

            /// <summary>
            /// Starts operation processing.
            /// </summary>
            /// <param name="owner">Owner TcpMessage client.</param>
            /// <returns>Returns true if asynchronous operation in progress or false if operation completed synchronously.</returns>
            /// <exception cref="ArgumentNullException">Is raised when <b>owner</b> is null reference.</exception>
            internal bool Start(TcpClient owner)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException("owner");
                }

                m_TcpClient = owner;

                SetState(OperationState.Active);

                try
                {
                    /* RFC 4954 4. The AUTH Command.

                        AUTH mechanism [initial-response]

                        Arguments:
                            mechanism: A string identifying a [SASL] authentication mechanism.

                            initial-response: An optional initial client response.  If
                            present, this response MUST be encoded as described in Section
                            4 of [BASE64] or contain a single character "=".
                    */

                    if (m_SASL.SupportsInitialResponse)
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes("AUTH " + m_SASL.Name + " " + Convert.ToBase64String(m_SASL.Continue(null)) + "\r\n");

                        // Log
                        m_TcpClient.LogAddWrite(buffer.Length, Encoding.UTF8.GetString(buffer).TrimEnd());

                        // Start command sending.
                        m_TcpClient.TcpStream.BeginWrite(buffer, 0, buffer.Length, this.AuthCommandSendingCompleted, null);
                    }
                    else
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes("AUTH " + m_SASL.Name + "\r\n");

                        // Log
                        m_TcpClient.LogAddWrite(buffer.Length, "AUTH " + m_SASL.Name);

                        // Start command sending.
                        m_TcpClient.TcpStream.BeginWrite(buffer, 0, buffer.Length, this.AuthCommandSendingCompleted, null);
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(OperationState.Completed);
                }

                // Set flag rise CompletedAsync event flag. The event is raised when async op completes.
                // If already completed sync, that flag has no effect.
                lock (m_Lock)
                {
                    m_RiseCompleted = true;

                    return m_State == OperationState.Active;
                }
            }

            #endregion


            #region method SetState

            /// <summary>
            /// Sets operation state.
            /// </summary>
            /// <param name="state">New state.</param>
            private void SetState(OperationState state)
            {
                if (m_State == OperationState.Disposed)
                {
                    return;
                }

                lock (m_Lock)
                {
                    m_State = state;

                    if (m_State == OperationState.Completed && m_RiseCompleted)
                    {
                        OnCompletedAsync();
                    }
                }
            }

            #endregion

            #region method AuthCommandSendingCompleted

            /// <summary>
            /// Is called when AUTH command sending has finished.
            /// </summary>
            /// <param name="ar">Asynchronous result.</param>
            private void AuthCommandSendingCompleted(IAsyncResult ar)
            {
                try
                {
                    m_TcpClient.TcpStream.EndWrite(ar);

                    // Read TcpMessage server response.
                    ReadResponseAsyncOperation readResponseOp = new ReadResponseAsyncOperation();
                    readResponseOp.CompletedAsync += delegate(object s, EventArgs<ReadResponseAsyncOperation> e)
                    {
                        AuthReadResponseCompleted(readResponseOp);
                    };
                    if (!m_TcpClient.ReadResponseAsync(readResponseOp))
                    {
                        AuthReadResponseCompleted(readResponseOp);
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(OperationState.Completed);
                }
            }

            #endregion

            #region method AuthReadResponseCompleted

            /// <summary>
            /// Is called when TcpMessage server response reading has completed.
            /// </summary>
            /// <param name="op">Asynchronous operation.</param>
            private void AuthReadResponseCompleted(ReadResponseAsyncOperation op)
            {
                try
                {
                    // Continue authenticating.
                    if (op.ReplyStream.ReplyCode == 334)
                    {
                        // 334 base64Data, we need to decode it.
                        byte[] serverResponse = Convert.FromBase64String(op.ReplyStream.Text);

                        byte[] clientResponse = m_SASL.Continue(serverResponse);

                        // We need just send SASL returned auth-response as base64.
                        byte[] buffer = Encoding.UTF8.GetBytes(Convert.ToBase64String(clientResponse) + "\r\n");

                        // Log
                        m_TcpClient.LogAddWrite(buffer.Length, Convert.ToBase64String(clientResponse));

                        // Start auth-data sending.
                        m_TcpClient.TcpStream.BeginWrite(buffer, 0, buffer.Length, this.AuthCommandSendingCompleted, null);
                    }
                    // Authentication suceeded.
                    else if (op.ReplyStream.ReplyCode == 235)
                    {
                        m_TcpClient.m_AuthdUserIdentity = new GenericIdentity(m_SASL.UserName, m_SASL.Name);

                        SetState(OperationState.Completed);
                    }
                    // Authentication rejected.
                    else
                    {
                        m_Exception = new TcpClientException(op.ReplyStream.Text);
                        SetState(OperationState.Completed);
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(OperationState.Completed);
                }
            }

            #endregion
            
            #region Properties implementation

            /// <summary>
            /// Gets asynchronous operation state.
            /// </summary>
            public OperationState State
            {
                get { return m_State; }
            }

            /// <summary>
            /// Gets error happened during operation. Returns null if no error.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
            /// <exception cref="InvalidOperationException">Is raised when this property is accessed other than <b>OperationState.Completed</b> state.</exception>
            public Exception Error
            {
                get
                {
                    if (m_State == OperationState.Disposed)
                    {
                        throw new ObjectDisposedException(this.GetType().Name);
                    }
                    if (m_State != OperationState.Completed)
                    {
                        throw new InvalidOperationException("Property 'Error' is accessible only in 'OperationState.Completed' state.");
                    }

                    return m_Exception;
                }
            }

            #endregion

            #region Events implementation

            /// <summary>
            /// Is called when asynchronous operation has completed.
            /// </summary>
            public event EventHandler<EventArgs<AuthAsyncOperation>> CompletedAsync = null;

            #region method OnCompletedAsync

            /// <summary>
            /// Raises <b>CompletedAsync</b> event.
            /// </summary>
            private void OnCompletedAsync()
            {
                if (this.CompletedAsync != null)
                {
                    this.CompletedAsync(this, new EventArgs<AuthAsyncOperation>(this));
                }
            }

            #endregion

            #endregion
        }

        #endregion

        /// <summary>
        /// Starts sending AUTH command to TcpMessage server.
        /// </summary>
        /// <param name="op">Asynchronous operation.</param>
        /// <returns>Returns true if aynchronous operation is pending (The <see cref="AuthAsyncOperation.CompletedAsync"/> event is raised upon completion of the operation).
        /// Returns false if operation completed synchronously.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TcpMessage client is not connected or connection is already authenticated.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
        public bool AuthAsync(AuthAsyncOperation op)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (this.IsAuthenticated)
            {
                throw new InvalidOperationException("Connection is already authenticated.");
            }
            if (op == null)
            {
                throw new ArgumentNullException("op");
            }
            if (op.State != OperationState.Waiting)
            {
                throw new ArgumentException("Invalid argument 'op' state, 'op' must be in 'OperationState.Waiting' state.", "op");
            }

            return op.Start(this);
        }

        #endregion
     
        #region method SendMessage

               /// <summary>
        /// Sends raw message to TcpMessage server.
        /// </summary>
        /// <param name="message">Message. Sending the current message.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TcpMessage client is not connected.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        /// <remarks>The stream must contain data in MIME format, other formats normally are rejected by TcpMessage server.</remarks>
        public TcpReplyStream SendMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("TcpClient.SendMessage message");
            }

            //if (message.Targets != null)
            //{
            //    Message mcopy = new Message(message.BodyStream, null);

            //    foreach (var t in message.Targets)
            //    {
            //        mcopy.Host = t.HostName;
            //        var ms = mcopy.Serialize();
            //        ms.Position = 0;
            //        SendMessage(ms);
            //    }
            //}


           return SendMessage(message.Serialize());
        }

        /// <summary>
        /// Sends raw message to TcpMessage server.
        /// </summary>
        /// <param name="stream">Message stream. Sending starts from stream current position.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TcpMessage client is not connected.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        /// <remarks>The stream must contain data in MIME format, other formats normally are rejected by TcpMessage server.</remarks>
        public TcpReplyStream SendMessage(Stream stream)
        {

            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            ReplyStream = null;

            ManualResetEvent wait = new ManualResetEvent(false);
            using (SendMessageAsyncOperation op = new SendMessageAsyncOperation(stream, true))
            {
                op.CompletedAsync += delegate(object s1, EventArgs<SendMessageAsyncOperation> e1)
                {
                    wait.Set();
                };
                if (!this.SendMessageAsync(op))
                {
                    wait.Set();
                }
                wait.WaitOne();
                wait.Close();

                if (op.Error != null)
                {
                    throw op.Error;
                }
            }

            return ReplyStream;
        }

             
        #region class SendMessageAsyncOperation

        /// <summary>
        /// This class represents <see cref="TcpClient.SendMessageAsync"/> asynchronous operation.
        /// </summary>
        public class SendMessageAsyncOperation : IDisposable, IAsyncOperation
        {
            private object m_Lock = new object();
            private OperationState m_State = OperationState.Waiting;
            private Exception m_Exception = null;
            private Stream m_Stream = null;
            private bool m_UseBdat = false;
            private TcpClient m_TcpClient = null;
            private byte[] m_BdatBuffer = null;
            private int m_BdatBytesInBuffer = 0;
            private byte[] m_BdatSendBuffer = null;
            private bool m_RiseCompleted = false;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="stream">Message stream. Message sending starts from <b>stream</b> current position and all stream data will be sent.</param>
            public SendMessageAsyncOperation(Stream stream, bool useBdatIfPossibe)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("stream");
                }

                m_Stream = stream;
                m_UseBdat = useBdatIfPossibe;
            }

            #region method Dispose

            /// <summary>
            /// Cleans up any resources being used.
            /// </summary>
            public void Dispose()
            {
                if (m_State == OperationState.Disposed)
                {
                    return;
                }
                SetState(OperationState.Disposed);

                m_Exception = null;
                m_Stream = null;
                m_TcpClient = null;
                m_BdatBuffer = null;
                m_BdatSendBuffer = null;

                this.CompletedAsync = null;
            }

            #endregion

            #region method Start

            /// <summary>
            /// Invoke operation processing.
            /// </summary>
            /// <param name="owner">Owner TcpMessage client.</param>
            /// <returns>Returns true if asynchronous operation in progress or false if operation completed synchronously.</returns>
            /// <exception cref="ArgumentNullException">Is raised when <b>owner</b> is null reference.</exception>
            internal bool Invoke(TcpClient owner)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException("owner");
                }

                m_TcpClient = owner;

                SetState(OperationState.Active);

                try
                {

                    // See if BDAT supported.
                    bool bdatSupported = true;
                                
                    // BDAT.
                    if (bdatSupported && m_UseBdat)
                    {
                        /* RFC 3030 2.
                            bdat-cmd   ::= "BDAT" SP chunk-size [ SP end-marker ] CR LF
                            chunk-size ::= 1*DIGIT
                            end-marker ::= "LAST"
                        */

                        m_BdatBuffer = new byte[64000];
                        m_BdatSendBuffer = new byte[64100]; // 100 bytes for "BDAT xxxxxx...CRLF"

                        // Start reading message data-block.
                        m_Stream.BeginRead(m_BdatBuffer, 0, m_BdatBuffer.Length, this.BdatChunkReadingCompleted, null);

                    }
                    else
                    {

                        /* RFC 5321 4.1.1.4.
                            The mail data are terminated by a line containing only a partial, that
                            is, the character sequence "<CRLF>.<CRLF>", where the first <CRLF> is
                            actually the terminator of the previous line.
                          
                            Examples:
                                C: DATA<CRLF>
                                S: 354 Start sending message, end with <crlf>.<crlf>.<CRLF>
                                C: send_message
                                C: .<CRLF>
                                S: 250 Ok<CRLF>
                        */

                        //byte[] buffer = Encoding.UTF8.GetBytes("DATA\r\n");

                        //// Log
                        //m_TcpClient.LogAddWrite(buffer.Length, "DATA");

                        //// Start command sending.
                        //m_TcpClient.TcpStream.BeginWrite(buffer, 0, buffer.Length, this.DataCommandSendingCompleted, null);
                    }
                   
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                    SetState(OperationState.Completed);
                }

                // Set flag rise CompletedAsync event flag. The event is raised when async op completes.
                // If already completed sync, that flag has no effect.
                lock (m_Lock)
                {
                    m_RiseCompleted = true;

                    return m_State == OperationState.Active;
                }
            }

            #endregion

            #region method SetState

            /// <summary>
            /// Sets operation state.
            /// </summary>
            /// <param name="state">New state.</param>
            private void SetState(OperationState state)
            {
                if (m_State == OperationState.Disposed)
                {
                    return;
                }

                lock (m_Lock)
                {
                    m_State = state;

                    if (m_State == OperationState.Completed && m_RiseCompleted)
                    {
                        OnCompletedAsync();
                    }
                }
            }

            #endregion

            #region method Bdat command

            /// <summary>
            /// Is called when message data block for BDAT reading has completed.
            /// </summary>
            /// <param name="ar">Asynchronous result.</param>
            private void BdatChunkReadingCompleted(IAsyncResult ar)
            {
                try
                {
                    m_BdatBytesInBuffer = m_Stream.EndRead(ar);

                    /* RFC 3030 2.
                        bdat-cmd   ::= "BDAT" SP chunk-size [ SP end-marker ] CR LF
                        chunk-size ::= 1*DIGIT
                        end-marker ::= "LAST"
                    */

                    // Send data chunk.
                    if (m_BdatBytesInBuffer > 0)
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes("BDAT " + m_BdatBytesInBuffer + "\r\n");

                        // Log
                        m_TcpClient.LogAddWrite(buffer.Length, "BDAT " + m_BdatBytesInBuffer);
                        m_TcpClient.LogAddWrite(m_BdatBytesInBuffer, "<BDAT data-chunk of " + m_BdatBytesInBuffer + " bytes>");

                        // Copy data to send buffer.(BDAT xxxCRLF<xxx-bytes>).
                        Array.Copy(buffer, m_BdatSendBuffer, buffer.Length);
                        Array.Copy(m_BdatBuffer, 0, m_BdatSendBuffer, buffer.Length, m_BdatBytesInBuffer);

                        // Start command sending.
                        m_TcpClient.TcpStream.BeginWrite(m_BdatSendBuffer, 0, buffer.Length + m_BdatBytesInBuffer, this.BdatCommandSendingCompleted, null);
                    }
                    // EOS, we readed all message data.
                    else
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes("BDAT 0 LAST\r\n");

                        // Log
                        m_TcpClient.LogAddWrite(buffer.Length, "BDAT 0 LAST");

                        // Start command sending.
                        m_TcpClient.TcpStream.BeginWrite(buffer, 0, buffer.Length, this.BdatCommandSendingCompleted, null);
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(OperationState.Completed);
                }
            }

          

            /// <summary>
            /// Is called when BDAT command sending has finished.
            /// </summary>
            /// <param name="ar">Asynchronous result.</param>
            private void BdatCommandSendingCompleted(IAsyncResult ar)
            {
                try
                {
                    m_TcpClient.TcpStream.EndWrite(ar);

                    // Read BDAT command response.
                    ReadResponseAsyncOperation readResponseOp = new ReadResponseAsyncOperation();
                    readResponseOp.CompletedAsync += delegate(object s, EventArgs<ReadResponseAsyncOperation> e)
                    {
                        BdatReadResponseCompleted(readResponseOp);
                    };
                    if (!m_TcpClient.ReadResponseAsync(readResponseOp))
                    {
                        BdatReadResponseCompleted(readResponseOp);
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(OperationState.Completed);
                }
            }

           

            /// <summary>
            /// Is called when TcpMessage server BDAT command response reading has completed.
            /// </summary>
            /// <param name="op">Asynchronous operation.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
            private void BdatReadResponseCompleted(ReadResponseAsyncOperation op)
            {
                if (op == null)
                {
                    throw new ArgumentNullException("op");
                }

                try
                {
                    if (op.Error != null)
                    {
                        m_Exception = op.Error;
                        m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                        SetState(OperationState.Completed);
                    }
                    else
                    {
                        m_TcpClient.ReplyStream = op.ReplyStream;

                        // BDAT succeeded.
                        if (op.ReplyStream.ReplyCode == 250)
                        {
                            // We have sent whole message, we are done.
                            if (m_BdatBytesInBuffer == 0)
                            {
                                SetState(OperationState.Completed);
                                return;
                            }
                            // Send next BDAT data-chunk.
                            else
                            {
                                // Start reading next message data-block.
                                m_Stream.BeginRead(m_BdatBuffer, 0, m_BdatBuffer.Length, this.BdatChunkReadingCompleted, null);
                            }
                        }
                        // BDAT failed.
                        else
                        {
                            m_Exception = new TcpClientException(op.ReplyStream);

                            SetState(OperationState.Completed);
                        }
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                    SetState(OperationState.Completed);
                }

                op.Dispose();
            }

            #endregion

            #region method Data command

            /// <summary>
            /// Is called when DATA command sending has finished.
            /// </summary>
            /// <param name="ar">Asynchronous result.</param>
            private void DataCommandSendingCompleted(IAsyncResult ar)
            {
                try
                {
                    m_TcpClient.TcpStream.EndWrite(ar);

                    // Read DATA command response.
                    ReadResponseAsyncOperation readResponseOp = new ReadResponseAsyncOperation();
                    readResponseOp.CompletedAsync += delegate(object s, EventArgs<ReadResponseAsyncOperation> e)
                    {
                        DataReadResponseCompleted(readResponseOp);
                    };
                    if (!m_TcpClient.ReadResponseAsync(readResponseOp))
                    {
                        DataReadResponseCompleted(readResponseOp);
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(OperationState.Completed);
                }
            }

            /// <summary>
            /// Is called when TcpMessage server DATA command initial response reading has completed.
            /// </summary>
            /// <param name="op">Asynchronous operation.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
            private void DataReadResponseCompleted(ReadResponseAsyncOperation op)
            {
                if (op == null)
                {
                    throw new ArgumentNullException("op");
                }

                try
                {
                    if (op.Error != null)
                    {
                        m_Exception = op.Error;
                        m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                        SetState(OperationState.Completed);
                    }
                    else
                    {
                        // DATA command succeeded.
                        if (op.ReplyStream.ReplyCode == 354)
                        {
                            // Start sending message.
                            ExtendedStream.WritePartialTerminatedAsyncOperation sendMsgOP = new ExtendedStream.WritePartialTerminatedAsyncOperation(m_Stream);
                            sendMsgOP.CompletedAsync += delegate(object s, EventArgs<ExtendedStream.WritePartialTerminatedAsyncOperation> e)
                            {
                                DataMsgSendingCompleted(sendMsgOP);
                            };
                            if (!m_TcpClient.TcpStream.WritePartialTerminatedAsync(sendMsgOP))
                            {
                                DataMsgSendingCompleted(sendMsgOP);
                            }
                        }
                        // DATA command failed.
                        else
                        {
                            m_Exception = new TcpClientException(op.ReplyStream);
                            SetState(OperationState.Completed);
                        }
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                    SetState(OperationState.Completed);
                }

                op.Dispose();
            }

            /// <summary>
            /// Is called when DATA command message sending has completed.
            /// </summary>
            /// <param name="op">Asynchronous operation.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
            private void DataMsgSendingCompleted(ExtendedStream.WritePartialTerminatedAsyncOperation op)
            {
                if (op == null)
                {
                    throw new ArgumentNullException("op");
                }

                try
                {
                    if (op.Error != null)
                    {
                        m_Exception = op.Error;
                        m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                        SetState(OperationState.Completed);
                    }
                    else
                    {
                        // Log
                        m_TcpClient.LogAddWrite(op.BytesWritten, "Sent message " + op.BytesWritten + " bytes.");

                        // Read DATA command final response.
                        ReadResponseAsyncOperation readResponseOp = new ReadResponseAsyncOperation();
                        readResponseOp.CompletedAsync += delegate(object s, EventArgs<ReadResponseAsyncOperation> e)
                        {
                            DataReadFinalResponseCompleted(readResponseOp);
                        };
                        if (!m_TcpClient.ReadResponseAsync(readResponseOp))
                        {
                            DataReadFinalResponseCompleted(readResponseOp);
                        }
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                    SetState(OperationState.Completed);
                }

                op.Dispose();
            }

  
            /// <summary>
            /// Is called when TcpMessage server DATA command final response reading has completed.
            /// </summary>
            /// <param name="op">Asynchronous operation.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
            private void DataReadFinalResponseCompleted(ReadResponseAsyncOperation op)
            {
                if (op == null)
                {
                    throw new ArgumentNullException("op");
                }

                try
                {
                    if (op.Error != null)
                    {
                        m_Exception = op.Error;
                        m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                        SetState(OperationState.Completed);
                    }
                    else
                    {
                        // DATA command failed, only 2xx response is success.
                        if (op.ReplyStream.ReplyCode < 200 || op.ReplyStream.ReplyCode > 299)
                        {
                            m_Exception = new TcpClientException(op.ReplyStream);
                        }

                        SetState(OperationState.Completed);
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                    SetState(OperationState.Completed);
                }

                op.Dispose();
            }

            #endregion

            #region Properties implementation

            /// <summary>
            /// Gets asynchronous operation state.
            /// </summary>
            public OperationState State
            {
                get { return m_State; }
            }

            /// <summary>
            /// Gets error happened during operation. Returns null if no error.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
            /// <exception cref="InvalidOperationException">Is raised when this property is accessed other than <b>OperationState.Completed</b> state.</exception>
            public Exception Error
            {
                get
                {
                    if (m_State == OperationState.Disposed)
                    {
                        throw new ObjectDisposedException(this.GetType().Name);
                    }
                    if (m_State != OperationState.Completed)
                    {
                        throw new InvalidOperationException("Property 'Error' is accessible only in 'OperationState.Completed' state.");
                    }

                    return m_Exception;
                }
            }

            #endregion

            #region Events implementation

            /// <summary>
            /// Is called when asynchronous operation has completed.
            /// </summary>
            public event EventHandler<EventArgs<SendMessageAsyncOperation>> CompletedAsync = null;

            #region method OnCompletedAsync

            /// <summary>
            /// Raises <b>CompletedAsync</b> event.
            /// </summary>
            private void OnCompletedAsync()
            {
                if (this.CompletedAsync != null)
                {
                    this.CompletedAsync(this, new EventArgs<SendMessageAsyncOperation>(this));
                }
            }

            #endregion

            #endregion
        }

        #endregion

        /// <summary>
        /// Starts sending message to TcpMessage server.
        /// </summary>
        /// <param name="op">Asynchronous operation.</param>
        /// <returns>Returns true if aynchronous operation is pending (The <see cref="SendMessageAsyncOperation.CompletedAsync"/> event is raised upon completion of the operation).
        /// Returns false if operation completed synchronously.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TcpMessage client is not connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
        public bool SendMessageAsync(SendMessageAsyncOperation op)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (op == null)
            {
                throw new ArgumentNullException("op");
            }
            if (op.State != OperationState.Waiting)
            {
                throw new ArgumentException("Invalid argument 'op' state, 'op' must be in 'OperationState.Waiting' state.", "op");
            }

            return op.Invoke(this);
        }

        #endregion

        #region method Rset

        /// <summary>
        /// Send RSET command to server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TcpMessage client is not connected.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public void Rset()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }

            ManualResetEvent wait = new ManualResetEvent(false);
            using (RsetAsyncOperation op = new RsetAsyncOperation())
            {
                op.CompletedAsync += delegate(object s1, EventArgs<RsetAsyncOperation> e1)
                {
                    wait.Set();
                };
                if (!this.RsetAsync(op))
                {
                    wait.Set();
                }
                wait.WaitOne();
                wait.Close();

                if (op.Error != null)
                {
                    throw op.Error;
                }
            }
        }

      
        #region class RsetAsyncOperation

        /// <summary>
        /// This class represents <see cref="TcpClient.RsetAsync"/> asynchronous operation.
        /// </summary>
        public class RsetAsyncOperation : IDisposable, IAsyncOperation
        {
            private object m_Lock = new object();
            private OperationState m_State = OperationState.Waiting;
            private Exception m_Exception = null;
            private TcpClient m_TcpClient = null;
            private bool m_RiseCompleted = false;

            /// <summary>
            /// Default constructor.
            /// </summary>
            public RsetAsyncOperation()
            {
            }

            #region method Dispose

            /// <summary>
            /// Cleans up any resources being used.
            /// </summary>
            public void Dispose()
            {
                if (m_State == OperationState.Disposed)
                {
                    return;
                }
                SetState(OperationState.Disposed);

                m_Exception = null;
                m_TcpClient = null;

                this.CompletedAsync = null;
            }

            #endregion


            #region method Start

            /// <summary>
            /// Starts operation processing.
            /// </summary>
            /// <param name="owner">Owner TcpMessage client.</param>
            /// <returns>Returns true if asynchronous operation in progress or false if operation completed synchronously.</returns>
            /// <exception cref="ArgumentNullException">Is raised when <b>owner</b> is null reference.</exception>
            internal bool Start(TcpClient owner)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException("owner");
                }

                m_TcpClient = owner;

                SetState(OperationState.Active);

                try
                {
                    /* RFC 5321 4.1.1.5.
                        rset = "REST" CRLF
                    */

                    byte[] buffer = Encoding.UTF8.GetBytes("RSET\r\n");

                    // Log
                    m_TcpClient.LogAddWrite(buffer.Length, "RSET");

                    // Start command sending.
                    m_TcpClient.TcpStream.BeginWrite(buffer, 0, buffer.Length, this.RsetCommandSendingCompleted, null);
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(OperationState.Completed);
                }

                // Set flag rise CompletedAsync event flag. The event is raised when async op completes.
                // If already completed sync, that flag has no effect.
                lock (m_Lock)
                {
                    m_RiseCompleted = true;

                    return m_State == OperationState.Active;
                }
            }

            #endregion


            #region method SetState

            /// <summary>
            /// Sets operation state.
            /// </summary>
            /// <param name="state">New state.</param>
            private void SetState(OperationState state)
            {
                if (m_State == OperationState.Disposed)
                {
                    return;
                }

                lock (m_Lock)
                {
                    m_State = state;

                    if (m_State == OperationState.Completed && m_RiseCompleted)
                    {
                        OnCompletedAsync();
                    }
                }
            }

            #endregion

            #region method RsetCommandSendingCompleted

            /// <summary>
            /// Is called when RSET command sending has finished.
            /// </summary>
            /// <param name="ar">Asynchronous result.</param>
            private void RsetCommandSendingCompleted(IAsyncResult ar)
            {
                try
                {
                    m_TcpClient.TcpStream.EndWrite(ar);

                    // Read TcpMessage server response.
                    ReadResponseAsyncOperation readResponseOp = new ReadResponseAsyncOperation();
                    readResponseOp.CompletedAsync += delegate(object s, EventArgs<ReadResponseAsyncOperation> e)
                    {
                        RsetReadResponseCompleted(readResponseOp);
                    };
                    if (!m_TcpClient.ReadResponseAsync(readResponseOp))
                    {
                        RsetReadResponseCompleted(readResponseOp);
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + x.Message, x);
                    SetState(OperationState.Completed);
                }
            }

            #endregion

            #region method RsetReadResponseCompleted

            /// <summary>
            /// Is called when TcpMessage server RSET command response reading has completed.
            /// </summary>
            /// <param name="op">Asynchronous operation.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
            private void RsetReadResponseCompleted(ReadResponseAsyncOperation op)
            {
                if (op == null)
                {
                    throw new ArgumentNullException("op");
                }

                try
                {
                    if (op.Error != null)
                    {
                        m_Exception = op.Error;
                        m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                    }
                    else
                    {
                        // RSET succeeded.
                        if (op.ReplyStream.ReplyCode == 250)
                        {
                            /* RFC 5321 4.1.1.9.
                                rset      = "RSET" CRLF
                                rset-resp = "250 OK" CRLF
                            */

                            // Do nothing.
                        }
                        // RSET failed.
                        else
                        {
                            m_Exception = new TcpClientException(op.ReplyStream);
                            m_TcpClient.LogAddException("Exception: " + m_Exception.Message, m_Exception);
                        }
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_TcpClient.LogAddException("Exception: " + x.Message, x);
                }

                SetState(OperationState.Completed);
            }

            #endregion


            #region Properties implementation

            /// <summary>
            /// Gets asynchronous operation state.
            /// </summary>
            public OperationState State
            {
                get { return m_State; }
            }

            /// <summary>
            /// Gets error happened during operation. Returns null if no error.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
            /// <exception cref="InvalidOperationException">Is raised when this property is accessed other than <b>OperationState.Completed</b> state.</exception>
            public Exception Error
            {
                get
                {
                    if (m_State == OperationState.Disposed)
                    {
                        throw new ObjectDisposedException(this.GetType().Name);
                    }
                    if (m_State != OperationState.Completed)
                    {
                        throw new InvalidOperationException("Property 'Error' is accessible only in 'OperationState.Completed' state.");
                    }

                    return m_Exception;
                }
            }

            #endregion

            #region Events implementation

            /// <summary>
            /// Is called when asynchronous operation has completed.
            /// </summary>
            public event EventHandler<EventArgs<RsetAsyncOperation>> CompletedAsync = null;

            #region method OnCompletedAsync

            /// <summary>
            /// Raises <b>CompletedAsync</b> event.
            /// </summary>
            private void OnCompletedAsync()
            {
                if (this.CompletedAsync != null)
                {
                    this.CompletedAsync(this, new EventArgs<RsetAsyncOperation>(this));
                }
            }

            #endregion

            #endregion
        }

        #endregion

        /// <summary>
        /// Starts sending RSET command to TcpMessage server.
        /// </summary>
        /// <param name="op">Asynchronous operation.</param>
        /// <returns>Returns true if aynchronous operation is pending (The <see cref="RsetAsyncOperation.CompletedAsync"/> event is raised upon completion of the operation).
        /// Returns false if operation completed synchronously.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when TcpMessage client is not connected.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
        public bool RsetAsync(RsetAsyncOperation op)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("You must connect first.");
            }
            if (op == null)
            {
                throw new ArgumentNullException("op");
            }
            if (op.State != OperationState.Waiting)
            {
                throw new ArgumentException("Invalid argument 'op' state, 'op' must be in 'OperationState.Waiting' state.", "op");
            }

            return op.Start(this);
        }

        #endregion

        #region override method OnConnected

        /// <summary>
        /// This method is called when Tcp client has sucessfully connected.
        /// </summary>
        /// <param name="callback">Callback to be called to complete connect operation.</param>
        protected override void OnConnected(CompleteConnectCallback callback)
        {
            // Read TcpMessage server greeting response.
            //ReadResponseAsyncOperation readGreetingOP = new ReadResponseAsyncOperation();
            //readGreetingOP.CompletedAsync += delegate(object s, EventArgs<ReadResponseAsyncOperation> e)
            //{
            //    ReadServerGreetingCompleted(readGreetingOP, callback);
            //};
            //if (!ReadResponseAsync(readGreetingOP))
            //{
            //    ReadServerGreetingCompleted(readGreetingOP, callback);
            //}
        }

        #endregion

        #region method ReadResponseAsync

        #region class ReadResponseAsyncOperation

        /// <summary>
        /// This class represents <see cref="TcpClient.ReadResponseAsync"/> asynchronous operation.
        /// </summary>
        private class ReadResponseAsyncOperation : IDisposable, IAsyncOperation
        {
            private OperationState m_State = OperationState.Waiting;
            private Exception m_Exception = null;
            private TcpClient m_TcpClient = null;
            //private List<TcpReplyLine> m_ReplyLines = null;
            private TcpReplyStream m_ReplyStream = null;

            /// <summary>
            /// Default constructor.
            /// </summary>
            public ReadResponseAsyncOperation()
            {
               // m_ReplyLines = new List<TcpReplyLine>();
            }

            #region method Dispose

            /// <summary>
            /// Cleans up any resource being used.
            /// </summary>
            public void Dispose()
            {
                if (m_State == OperationState.Disposed)
                {
                    return;
                }
                SetState(OperationState.Disposed);

                m_Exception = null;
                m_TcpClient = null;
                //m_ReplyLines = null;

                this.CompletedAsync = null;
            }

            #endregion


            #region method Start

            /// <summary>
            /// Starts operation processing.
            /// </summary>
            /// <param name="owner">Owner TcpMessage client.</param>
            /// <returns>Returns true if asynchronous operation in progress or false if operation completed synchronously.</returns>
            /// <exception cref="ArgumentNullException">Is raised when <b>owner</b> is null reference.</exception>
            internal bool Start(TcpClient owner)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException("owner");
                }

                m_TcpClient = owner;

                try
                {
                    ExtendedStream.ReadLineAsyncOperation op = new ExtendedStream.ReadLineAsyncOperation(new byte[8000], SizeExceededAction.SkipAndThrowException);
                    op.Completed += delegate(object s, EventArgs<ExtendedStream.ReadLineAsyncOperation> e)
                    {
                        try
                        {
                            // Response reading completed.
                            if (!ReadLineCompleted(op))
                            {
                                SetState(OperationState.Completed);
                                OnCompletedAsync();
                            }
                            // Continue response reading.
                            else
                            {
                                while (owner.TcpStream.ReadLine(op, true))
                                {
                                    // Response reading completed.
                                    if (!ReadLineCompleted(op))
                                    {
                                        SetState(OperationState.Completed);
                                        OnCompletedAsync();

                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception x)
                        {
                            m_Exception = x;
                            SetState(OperationState.Completed);
                            OnCompletedAsync();
                        }
                    };
                    while (owner.TcpStream.ReadLine(op, true))
                    {
                        // Response reading completed.
                        if (!ReadLineCompleted(op))
                        {
                            SetState(OperationState.Completed);

                            return false;
                        }
                    }

                    return true;
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    SetState(OperationState.Completed);

                    return false;
                }
            }

            #endregion


            #region method SetState

            /// <summary>
            /// Sets operation state.
            /// </summary>
            /// <param name="state">New state.</param>
            private void SetState(OperationState state)
            {
                m_State = state;
            }

            #endregion

            #region method ReadLineCompleted

            /// <summary>
            /// Is called when read line has completed.
            /// </summary>
            /// <param name="op">Asynchronous operation.</param>
            /// <returns>Returns true if multiline response has more response lines.</returns>
            /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
            private bool ReadLineCompleted(ExtendedStream.ReadLineAsyncOperation op)
            {
                if (op == null)
                {
                    throw new ArgumentNullException("op");
                }

                try
                {
                    // Line reading failed, we are done.
                    if (op.Error != null)
                    {
                        m_Exception = op.Error;
                    }
                    // Line reading succeeded.
                    else
                    {
                        // Log.
                        m_TcpClient.LogAddRead(op.BytesInBuffer, op.LineUtf8);

                        //TcpReplyLine replyLine = TcpReplyLine.Parse(op.LineUtf8);
                        //m_ReplyLines.Add(replyLine);

                        //return !replyLine.IsLastLine;

                        m_ReplyStream = new TcpReplyStream(new NetStream(op.Buffer));
                        return !m_ReplyStream.IsLastLine;
                    }
                }
                catch (Exception x)
                {
                    m_Exception = x;
                }

                return false;
            }

            #endregion


            #region Properties implementation

            /// <summary>
            /// Gets asynchronous operation state.
            /// </summary>
            public OperationState State
            {
                get { return m_State; }
            }

            /// <summary>
            /// Gets error happened during operation. Returns null if no error.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
            /// <exception cref="InvalidOperationException">Is raised when this property is accessed other than <b>OperationState.Completed</b> state.</exception>
            public Exception Error
            {
                get
                {
                    if (m_State == OperationState.Disposed)
                    {
                        throw new ObjectDisposedException(this.GetType().Name);
                    }
                    if (m_State != OperationState.Completed)
                    {
                        throw new InvalidOperationException("Property 'Error' is accessible only in 'OperationState.Completed' state.");
                    }

                    return m_Exception;
                }
            }

            ///// <summary>
            ///// Gets TcpMessage server reply-lines.
            ///// </summary>
            ///// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
            ///// <exception cref="InvalidOperationException">Is raised when this property is accessed other than <b>OperationState.Completed</b> state.</exception>
            //public TcpReplyLine[] ReplyLines
            //{
            //    get
            //    {
            //        if (m_State == OperationState.Disposed)
            //        {
            //            throw new ObjectDisposedException(this.GetType().Name);
            //        }
            //        if (m_State != OperationState.Completed)
            //        {
            //            throw new InvalidOperationException("Property 'ReplyLines' is accessible only in 'OperationState.Completed' state.");
            //        }
            //        if (m_Exception != null)
            //        {
            //            throw m_Exception;
            //        }

            //        return m_ReplyLines.ToArray();
            //    }
            //}

             /// <summary>
            /// Gets TcpMessage server reply-lines.
            /// </summary>
            /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
            /// <exception cref="InvalidOperationException">Is raised when this property is accessed other than <b>OperationState.Completed</b> state.</exception>
            public TcpReplyStream ReplyStream
            {
                get
                {
                    if (m_State == OperationState.Disposed)
                    {
                        throw new ObjectDisposedException(this.GetType().Name);
                    }
                    if (m_State != OperationState.Completed)
                    {
                        throw new InvalidOperationException("Property 'ReplyLines' is accessible only in 'OperationState.Completed' state.");
                    }
                    if (m_Exception != null)
                    {
                        throw m_Exception;
                    }

                    return m_ReplyStream;// Lines.ToArray();
                }
            }

            #endregion

            #region Events implementation

            /// <summary>
            /// Is called when asynchronous operation has completed.
            /// </summary>
            public event EventHandler<EventArgs<ReadResponseAsyncOperation>> CompletedAsync = null;

            #region method OnCompletedAsync

            /// <summary>
            /// Raises <b>CompletedAsync</b> event.
            /// </summary>
            private void OnCompletedAsync()
            {
                if (this.CompletedAsync != null)
                {
                    this.CompletedAsync(this, new EventArgs<ReadResponseAsyncOperation>(this));
                }
            }

            #endregion

            #endregion
        }

        #endregion

        /// <summary>
        /// Reads TcpMessage server single or multiline response.
        /// </summary>
        /// <param name="op">Asynchronous operation.</param>
        /// <returns>Returns true if aynchronous operation is pending (The <see cref="ReadResponseAsyncOperation.CompletedAsync"/> event is raised upon completion of the operation).
        /// Returns false if operation completed synchronously.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
        private bool ReadResponseAsync(ReadResponseAsyncOperation op)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if (op == null)
            {
                throw new ArgumentNullException("op");
            }
            if (op.State != OperationState.Waiting)
            {
                throw new ArgumentException("Invalid argument 'op' state, 'op' must be in 'OperationState.Waiting' state.", "op");
            }

            return op.Start(this);
        }
        
        #endregion
        
        #region static method QuickSend

        ///// <summary>
        ///// Sends specified mime message.
        ///// </summary>
        ///// <param name="message">Message to send.</param>
        ///// <exception cref="ArgumentNullException">Is raised when <b>message</b> is null.</exception>
        //[Obsolete("Use QuickSend(QueueMessage) instead")]
        //public static void QuickSend(Nistec.Messaging.Net.Mime.Mime message)
        //{
        //    if(message == null){
        //        throw new ArgumentNullException("message");
        //    }

        //    string from = "";
        //    if(message.MainEntity.From != null && message.MainEntity.From.Count > 0){
        //        from = ((MailboxAddress)message.MainEntity.From[0]).EmailAddress;
        //    }

        //    List<string> recipients = new List<string>();
        //    if(message.MainEntity.To != null){
        //        MailboxAddress[] addresses = message.MainEntity.To.Mailboxes;				
        //        foreach(MailboxAddress address in addresses){
        //            recipients.Add(address.EmailAddress);
        //        }
        //    }
        //    if(message.MainEntity.Cc != null){
        //        MailboxAddress[] addresses = message.MainEntity.Cc.Mailboxes;				
        //        foreach(MailboxAddress address in addresses){
        //            recipients.Add(address.EmailAddress);
        //        }
        //    }
        //    if(message.MainEntity.Bcc != null){
        //        MailboxAddress[] addresses = message.MainEntity.Bcc.Mailboxes;				
        //        foreach(MailboxAddress address in addresses){
        //            recipients.Add(address.EmailAddress);
        //        }

        //        // We must hide BCC
        //        message.MainEntity.Bcc.Clear();
        //    }

        //    foreach(string recipient in recipients){
        //        QuickSend(null,from,recipient,new MemoryStream(message.ToByteData()));
        //    }
        //}

        /// <summary>
        /// Sends specified request message.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>message</b> is null.</exception>
        public static void QuickSend(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if (message.HostAddress == null )
            {
                throw new ArgumentNullException("message.Host");
            }

            //if (message.Targets == null || message.Targets.Count == 0)
            //{
            //    throw new ArgumentNullException("message.Targets");
            //}

            //if (message.Targets != null)
            //{
            //    foreach (var t in message.Targets)
            //    {
            //        Message msg = new Message(message.BodyStream, null);
            //        msg.Host = t.HostName;
            //        var ms = msg.Serialize();
            //        ms.Position = 0;
            //        QuickSend(message.Sender, t.OriginalHostAddress, t.Port, ms);
            //    }
            //}
            var host = message.Host;
            QuickSend(message.Sender, host.HostAddress, host.Port, message.BodyStream);

        }

        /// <summary>
        /// Sends message directly to email domain. Domain email sever resolve order: MX recordds -> A reords if no MX.
        /// </summary>
        /// <param name="localHost">Host name which is reported to TcpMessage server.</param>
        /// <param name="targetHost">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>from</b>,<b>to</b> or <b>message</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public static void QuickSend(string localHost, string targetHost, int port, Stream message)
        {

            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            QuickSendSmartHost(localHost, targetHost, port, false, null, null, message);
        }

        /*
        /// <summary>
        /// Sends message directly to email domain. Domain email sever resolve order: MX recordds -> A reords if no MX.
        /// </summary>
        /// <param name="from">Sender email what is reported to TcpMessage server.</param>
        /// <param name="to">Recipient email.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>from</b>,<b>to</b> or <b>message</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public static void QuickSend(string from, string to, Stream message)
        {
            QuickSend(null, from, to, message);
        }

        /// <summary>
        /// Sends message directly to email domain. Domain email sever resolve order: MX recordds -> A reords if no MX.
        /// </summary>
        /// <param name="localHost">Host name which is reported to TcpMessage server.</param>
        /// <param name="from">Sender email what is reported to TcpMessage server.</param>
        /// <param name="to">Recipient email.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>from</b>,<b>to</b> or <b>message</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public static void QuickSend(string localHost, string from, string to, Stream message)
        {
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (from != "" )//&& !MQ_Utils.IsValidAddress(from))
            {
                throw new ArgumentException("Argument 'from' has invalid value.");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }
            if (to == "")
            {
                throw new ArgumentException("Argument 'to' value must be specified.");
            }
            //if (!MQ_Utils.IsValidAddress(to))
            //{
            //    throw new ArgumentException("Argument 'to' has invalid value.");
            //}
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            QuickSendSmartHost(localHost, DnsClient.Static.GetHostsAddress(to)[0].HostName, 25, false, from, new string[] { to }, message);
        }
        */
        #endregion

        #region static method QuickSendSmartHost

        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="ssl">Specifies if connected via SSL.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b> or <b>message</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public static void QuickSendSmartHost(Message message, bool ssl)
        {

            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (message.HostAddress == null)
            {
                throw new ArgumentNullException("message.Host");
            }

            //if (message.Targets == null || message.Targets.Count == 0)
            //{
            //    throw new ArgumentNullException("message.Targets");
            //}

            //NetStream stream = message.BodyStream;


            //foreach (var t in message.Targets)
            //{
            //    var ms = stream.Copy();
            //    ms.Position = 0;
            //    QuickSendSmartHost(null, t.OriginalHostAddress, t.Port, ssl, null, null, ms);
            //}

            var host = message.Host;
            QuickSendSmartHost(null, host.HostAddress, host.Port, ssl, null, null, message.BodyStream);
        }

        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="localHost">Host name which is reported to TcpMessage server.</param>
        /// <param name="targetHost">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="ssl">Specifies if connected via SSL.</param>
        /// <param name="userName">TcpMessage server user name. This value may be null, then authentication not used.</param>
        /// <param name="password">TcpMessage server password.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b>,<b>from</b>,<b>to</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public static void QuickSendSmartHost(string localHost, string targetHost, int port, bool ssl, string userName, string password, Stream message)
        {
            if (targetHost == null)
            {
                throw new ArgumentNullException("targetHost");
            }
            if (targetHost == "")
            {
                throw new ArgumentException("Argument 'host' value may not be empty.");
            }
            if (port < 1)
            {
                throw new ArgumentException("Argument 'port' value must be >= 1.");
            }


            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            using (TcpClient client = new TcpClient())
            {
                client.Connect(targetHost, port, ssl);
                if (!string.IsNullOrEmpty(userName))
                {
                    client.Auth(client.AuthGetStrongestMethod(userName, password));
                }
                client.SendMessage(message);
            }
        }

        /*
        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="ssl">Specifies if connected via SSL.</param>
        /// <param name="message">Mail message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b> or <b>message</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public static void QuickSendSmartHost(string host, int port, bool ssl, QueueMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            string from = "";
            if (message.Sender != null )//&& message.From.Count > 0)
            {
                from = message.Sender;//.HostAddress;
            }

            List<string> recipients = new List<string>();
            if (message.Targets != null)
            {
                foreach (var address in message.Targets)
                {
                    recipients.Add(address.HostAddress);
                }
            }
           
            foreach (string recipient in recipients)
            {
                NetStream ms = message.Serialize();//.GetEntityStream(true);
                ms.Position = 0;
                QuickSendSmartHost(null, host, port, ssl, null, null, from, new string[] { recipient }, ms);
            }
        }

        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="from">Sender email what is reported to TcpMessage server.</param>
        /// <param name="to">Recipients email addresses.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b>,<b>from</b>,<b>to</b> or <b>message</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public static void QuickSendSmartHost(string host, int port, string from, string[] to, Stream message)
        {
            QuickSendSmartHost(null, host, port, false, null, null, from, to, message);
        }

        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="ssl">Specifies if connected via SSL.</param>
        /// <param name="from">Sender email what is reported to TcpMessage server.</param>
        /// <param name="to">Recipients email addresses.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b>,<b>from</b>,<b>to</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public static void QuickSendSmartHost(string host, int port, bool ssl, string from, string[] to, Stream message)
        {
            QuickSendSmartHost(null, host, port, ssl, null, null, from, to, message);
        }

        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="localHost">Host name which is reported to TcpMessage server.</param>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="ssl">Specifies if connected via SSL.</param>
        /// <param name="from">Sender email what is reported to TcpMessage server.</param>
        /// <param name="to">Recipients email addresses.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b>,<b>from</b>,<b>to</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public static void QuickSendSmartHost(string localHost, string host, int port, bool ssl, string from, string[] to, Stream message)
        {
            QuickSendSmartHost(localHost, host, port, ssl, null, null, from, to, message);
        }

        /// <summary>
        /// Sends message by using specified smart host.
        /// </summary>
        /// <param name="localHost">Host name which is reported to TcpMessage server.</param>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="ssl">Specifies if connected via SSL.</param>
        /// <param name="userName">TcpMessage server user name. This value may be null, then authentication not used.</param>
        /// <param name="password">TcpMessage server password.</param>
        /// <param name="from">Sender email what is reported to TcpMessage server.</param>
        /// <param name="to">Recipients email addresses.</param>
        /// <param name="message">Raw message to send.</param>
        /// <exception cref="ArgumentNullException">Is raised when argument <b>host</b>,<b>from</b>,<b>to</b> or <b>stream</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the method arguments has invalid value.</exception>
        /// <exception cref="TcpClientException">Is raised when TcpMessage server returns error.</exception>
        public static void QuickSendSmartHost(string localHost, string host, int port, bool ssl, string userName, string password, string from, string[] to, Stream message)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            if (host == "")
            {
                throw new ArgumentException("Argument 'host' value may not be empty.");
            }
            if (port < 1)
            {
                throw new ArgumentException("Argument 'port' value must be >= 1.");
            }
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (from != "")// && !MQ_Utils.IsValidAddress(from))
            {
                throw new ArgumentException("Argument 'from' has invalid value.");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }
            if (to.Length == 0)
            {
                throw new ArgumentException("Argument 'to' must contain at least 1 recipient.");
            }
            //foreach (string t in to)
            //{
            //    if (!MQ_Utils.IsValidAddress(t))
            //    {
            //        throw new ArgumentException("Argument 'to' has invalid value '" + t + "'.");
            //    }
            //}
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            using (TcpClient smtp = new TcpClient())
            {
                smtp.Connect(host, port, ssl);
                if (!string.IsNullOrEmpty(userName))
                {
                    smtp.Auth(smtp.AuthGetStrongestMethod(userName, password));
                }
                smtp.SendMessage(message);
            }
        }
        */
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets host name which is reported to TcpMessage server. If value null, then local computer name is used.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and TcpMessage client is connected.</exception>
        public string LocalHostName
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_LocalHostName;
            }

            set
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if (this.IsConnected)
                {
                    throw new InvalidOperationException("Property LocalHostName is available only when TcpMessage client is not connected.");
                }

                m_LocalHostName = value;
            }
        }

        /// <summary>
        /// Gets TcpMessage server host name which it reported to us.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and TcpMessage client is not connected.</exception>
        public string RemoteHostName
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if (!this.IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_RemoteHostName;
            }
        }

  

        /// <summary>
        /// Gets TcpMessage server supported SASL authentication method.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and TcpMessage client is not connected.</exception>
        public string[] SaslAuthMethods
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if (!this.IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                // Search AUTH entry.
                //foreach (string feature in this.EsmtpFeatures)
                //{
                //    if (feature.ToUpper().StartsWith(MQ_ServiceExtensions.AUTH))
                //    {
                //        // Remove AUTH<SP> and split authentication methods.
                //        return feature.Substring(4).Trim().Split(' ');
                //    }
                //}

                return new string[0];
            }
        }

        /// <summary>
        /// Gets maximum message size in bytes what TcpMessage server accepts. Value null means not known.
        /// </summary>
        public long MaxAllowedMessageSize
        {
            get
            {
                try
                {
                    //foreach (string feature in this.EsmtpFeatures)
                    //{
                    //    if (feature.ToUpper().StartsWith(MQ_ServiceExtensions.SIZE))
                    //    {
                    //        return Convert.ToInt64(feature.Split(' ')[1]);
                    //    }
                    //}
                }
                catch
                {
                    // Never should reach here, skip errors here.
                }

                return 0;
            }
        }


        /// <summary>
        /// Gets task authenticated user identity, returns null if not authenticated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="InvalidOperationException">Is raised when this property is accessed and TcpMessage client is not connected.</exception>
        public override GenericIdentity AuthenticatedUserIdentity
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if (!this.IsConnected)
                {
                    throw new InvalidOperationException("You must connect first.");
                }

                return m_AuthdUserIdentity;
            }
        }

        #endregion

    }
}
