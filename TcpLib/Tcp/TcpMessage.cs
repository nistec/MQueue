using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Security.Principal;

using Nistec.Net.IO;
using Nistec.Net.Tcp;
using Nistec.Net.Auth;
using Nistec.Net;

namespace Nistec.Messaging.Tcp
{
    /// <summary>
    /// This class implements TcpMessage message. Defined RFC 5321.
    /// </summary>
    public class TcpMessage : TcpServerTask
    {
        private Dictionary<string, AuthSaslServer> m_Authentications = null;
        private int m_BadCommands = 0;
        private int m_Transactions = 0;
        private bool m_TaskRejected = false;
        private GenericIdentity m_User = null;
        private Stream m_MessageStream = null;
        private int m_BDatReadedCount = 0;

        //private int m_MaxTransactions = 10;
        private int m_MaxMessageSize = 10000000;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TcpMessage()
        {
            m_Authentications = new Dictionary<string, AuthSaslServer>(StringComparer.CurrentCultureIgnoreCase);
            //m_To = new Dictionary<string, QueueHost>();
        }

        #region method Dispose

        /// <summary>
        /// Cleans up any resource being used.
        /// </summary>
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            base.Dispose();

            m_Authentications = null;
            m_User = null;
            if (m_MessageStream != null)
            {
                m_MessageStream.Dispose();
                m_MessageStream = null;
            }
        }

        #endregion

        #region override method

        /// <summary>
        /// Starts message processing.
        /// </summary>
        protected override void StartTask()
        {
            base.StartTask();

            /* RFC 5321 3.1.
                The TcpMessage protocol allows a server to formally reject a mail message
                while still allowing the initial connection as follows: a 554
                response MAY be given in the initial connection opening message
                instead of the 220.  A server taking this approach MUST still wait
                for the client to send a QUIT (see Section 4.1.1.10) before closing
                the connection and SHOULD respond to any intervening commands with
                "503 bad sequence of commands".  Since an attempt to make an TcpMessage
                connection to such a system is probably in error, a server returning
                a 554 response on connection opening SHOULD provide enough
                information in the reply text to facilitate debugging of the sending
                system.
            */

            try
            {
                TcpReplyStream reply = null;

                //if (string.IsNullOrEmpty(this.Server.GreetingText))
                //{
                //    reply = new TcpReply(220, "<" + NetUtils.GetLocalHostName(this.LocalHostName) + "> Simple Mail Transfer Service Ready.");
                //}
                //else
                //{
                //    reply = new TcpReply(220, this.Server.GreetingText);
                //}

                reply = new TcpReplyStream(220, "");
                reply = OnStarted(reply);

                WriteLine(reply.ToString());

                // Setup rejected flag, so we respond "503 bad sequence of commands" any command except QUIT.
                if (reply.ReplyCode >= 300)
                {
                    m_TaskRejected = true;
                }

                BeginReadCmd();
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

     
        /// <summary>
        /// Is called when message has processing error.
        /// </summary>
        /// <param name="x">Exception happened.</param>
        protected override void OnError(Exception x)
        {
            if (this.IsDisposed)
            {
                return;
            }
            if (x == null)
            {
                return;
            }

            /* Error handling:
                IO and Socket exceptions are permanent, so we must end message.
            */

            try
            {
                LogAddText("Exception: " + x.Message);

                // Permanent error.
                if (x is IOException || x is SocketException)
                {
                    Dispose();
                }
                // xx error, may be temporary.
                else
                {
                    // Raise TcpServer.Error event.
                    base.OnError(x);

                    // Try to send "500 Internal server error."
                    try
                    {
                        WriteLine(TcpMessageCode.InternalServerError.ToString()+" Internal server error.");
                    }
                    catch
                    {
                        // Error is permanent.
                        Dispose();
                    }
                }
            }
            catch
            {
            }
        }

     

        /// <summary>
        /// This method is called when specified message times out.
        /// </summary>
        /// <remarks>
        /// This method allows inhereted classes to report error message to connected client.
        /// Task will be disconnected after this method completes.
        /// </remarks>
        protected override void OnTimeout()
        {
            try
            {
                if (m_MessageStream != null)
                {
                    OnMessageStoringCanceled();
                }

                WriteLine(TcpMessageCode.IdleTimeoutClosingConnection.ToString() + " Idle timeout, closing connection.");
            }
            catch
            {
                // Skip errors.
            }
        }

        #endregion
        
        #region method ReadCmd

        /// <summary>
        /// Starts reading incoming command from the connected client.
        /// </summary>
        private void BeginReadCmd()
        {
            if (this.IsDisposed)
            {
                return;
            }

            try
            {
                ExtendedStream.ReadLineAsyncOperation readLineOp = new ExtendedStream.ReadLineAsyncOperation(new byte[32000], SizeExceededAction.SkipAndThrowException);
                // This event is raised only if read partial-terminated opeartion completes asynchronously.
                readLineOp.Completed += new EventHandler<EventArgs<ExtendedStream.ReadLineAsyncOperation>>(delegate(object sender, EventArgs<ExtendedStream.ReadLineAsyncOperation> e)
                {
                    if (ProcessCmd(readLineOp))
                    {
                        BeginReadCmd();
                    }
                });
                // Process incoming commands while, command reading completes synchronously.
                while (this.TcpStream.ReadLine(readLineOp, true))
                {
                    if (!ProcessCmd(readLineOp))
                    {
                        break;
                    }
                }
            }
            catch (Exception x)
            {
                OnError(x);
            }
        }

       

        /// <summary>
        /// Completes command reading operation.
        /// </summary>
        /// <param name="op">Operation.</param>
        /// <returns>Returns true if server should start reading next command.</returns>
        private bool ProcessCmd(ExtendedStream.ReadLineAsyncOperation op)
        {
            bool readNextCommand = true;

            try
            {
                // We are already disposed.
                if (this.IsDisposed)
                {
                    return false;
                }
                // Check errors.
                if (op.Error != null)
                {
                    OnError(op.Error);
                }
                // Remote host shut-down(Socket.ShutDown) socket.
                if (op.BytesInBuffer == 0)
                {
                    LogAddText("The remote host '" + this.RemoteEndPoint.ToString() + "' shut down socket.");
                    Dispose();

                    return false;
                }

                // Log.
                //if (this.Server.Logger != null)
                //{
                //    this.Server.Logger.AddRead(this.ID, this.AuthenticatedUserIdentity, op.BytesInBuffer, op.LineUtf8, this.LocalEndPoint, this.RemoteEndPoint);
                //}

                string[] cmd_args = Encoding.UTF8.GetString(op.Buffer, 0, op.LineBytesInBuffer).Split(new char[] { ' ' }, 2);
                string cmd = cmd_args[0].ToUpperInvariant();
                string args = cmd_args.Length == 2 ? cmd_args[1] : "";

                if (cmd == "BDAT")
                {
                    readNextCommand = BDAT(args);
                }
                else
                {
                    WriteLine(TcpMessageCode.ErrorCommand.ToString() + " Error: command '" + cmd + "' not recognized.");
                }

                //DATA_Begin();

                /*
                string[] cmd_args = Encoding.UTF8.GetString(op.Buffer, 0, op.LineBytesInBuffer).Split(new char[] { ' ' }, 2);
                string cmd = cmd_args[0].ToUpperInvariant();
                string args = cmd_args.Length == 2 ? cmd_args[1] : "";

                if (cmd == "AUTH")
                {
                    AUTH(args);
                }
                else if (cmd == "DATA")
                {
                    DATA(args);

                    //Cmd_DATA cmdData = new Cmd_DATA();
                    //cmdData.CompletedAsync += delegate(object sender, EventArgs<TcpTask.Cmd_DATA> e)
                    //{
                    //    if (op.Error != null)
                    //    {
                    //        OnError(op.Error);
                    //    }

                    //    cmdData.Dispose();
                    //    BeginReadCmd();
                    //};
                    //if (!cmdData.Start(this, args))
                    //{
                    //    if (op.Error != null)
                    //    {
                    //        OnError(op.Error);
                    //    }

                    //    cmdData.Dispose();
                    //}
                    //else
                    //{
                    //    readNextCommand = false;
                    //}
                }
                else if (cmd == "RSET")
                {
                    RSET(args);
                }
                else if (cmd == "QUIT")
                {
                    QUIT(args);
                    readNextCommand = false;
                }
                else
                {
                    m_BadCommands++;

                    // Maximum allowed bad commands exceeded.
                    if (this.Server.MaxBadCommands != 0 && m_BadCommands > this.Server.MaxBadCommands)
                    {
                        WriteLine("421 Too many bad commands, closing transmission channel.");
                        Disconnect();
                        return false;
                    }

                    WriteLine("502 Error: command '" + cmd + "' not recognized.");
                }
              */ 
            }
            catch (Exception x)
            {
                OnError(x);
            }

            return readNextCommand;
        }

        #endregion
        //
        #region method ReadCommandAsync

        #region class ReadCommandAsyncOperation

        /// <summary>
        /// 
        /// </summary>
        private class ReadCommandAsyncOperation
        {
            /// <summary>
            /// Default constructor.
            /// </summary>
            public ReadCommandAsyncOperation()
            {
            }


            #region Properties implementation

            #endregion
        }

        #endregion

        /// <summary>
        /// Reads next TcpMessage command.
        /// </summary>
        /// <param name="op">Asynchronous operation.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
        private void ReadCommandAsync(ReadCommandAsyncOperation op)
        {
            if (op == null)
            {
                throw new ArgumentNullException("op");
            }

            // ReadCommandCompleted
        }

        #endregion
        //
        #region method ReadCommandCompleted

        /// <summary>
        /// Is called when TcpMessage command reading has completed.
        /// </summary>
        /// <param name="op">Asynchronous operation.</param>
        private void ReadCommandCompleted(ReadCommandAsyncOperation op)
        {
            if (this.IsDisposed)
            {
                return;
            }
            if (op == null)
            {
                // TODO: Log somewhere, don't raise exception.
            }

            // TODO:
        }

        #endregion

        #region method SendResponseAsync

        #region class SendResponseAsyncOperation

        /// <summary>
        /// This class represents <see cref="TcpTask.SendResponseAsync"/> asynchronous operation.
        /// </summary>
        private class SendResponseAsyncOperation : IDisposable, IAsyncOperation
        {
            private object m_Lock = new object();
            private OperationState m_State = OperationState.Waiting;
            private Exception m_Exception = null;
            private TcpReplyStream m_ReplyStream = null;
            private TcpMessage m_Task = null;
            private bool m_RiseCompleted = false;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="reply">TcpMessage server reply line.</param>
            /// <exception cref="ArgumentNullException">Is raised when <b>reply</b> is null reference.</exception>
            public SendResponseAsyncOperation(TcpReplyStream reply)
            {
                if (reply == null)
                {
                    throw new ArgumentNullException("reply");
                }

                m_ReplyStream = reply;
            }

            ///// <summary>
            ///// Default constructor.
            ///// </summary>
            ///// <param name="replyStream">TcpMessage server reply lines.</param>
            ///// <exception cref="ArgumentNullException">Is raised when <b>replyLines</b> is null reference.</exception>
            ///// <exception cref="ArgumentException">Is raised when any of the arguments has invalid values.</exception>
            //public SendResponseAsyncOperation(TcpReplyStream replyStream)
            //{
            //    if (replyStream == null)
            //    {
            //        throw new ArgumentNullException("replyStream");
            //    }
            //    //if (replyLines.Length < 1)
            //    //{
            //    //    throw new ArgumentException("Argument 'replyLines' must contain at least 1 item.", "replyLines");
            //    //}

            //    m_ReplyStream = replyStream;
            //}

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
                m_ReplyStream = null;
                m_Task = null;

                this.CompletedAsync = null;
            }

            #endregion


            #region method Start

            /// <summary>
            /// Starts operation processing.
            /// </summary>
            /// <param name="owner">Owner TcpMessage message.</param>
            /// <returns>Returns true if asynchronous operation in progress or false if operation completed synchronously.</returns>
            /// <exception cref="ArgumentNullException">Is raised when <b>owner</b> is null reference.</exception>
            public bool Start(TcpMessage owner)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException("owner");
                }

                m_Task = owner;

                SetState(OperationState.Active);

                try
                {
                    // Build TcpMessage response.
                    StringBuilder response = new StringBuilder();
                    //foreach (TcpReplyLine replyLine in m_ReplyLines)
                    //{
                    //    response.Append(replyLine.ToString());
                    //}

                    //byte[] buffer = Encoding.UTF8.GetBytes(response.ToString());


                    byte[] buffer = m_ReplyStream.GetBuffer();

                    // Log
                    m_Task.LogAddWrite(buffer.Length, response.ToString());

                    // Start response sending.
                    m_Task.TcpStream.BeginWrite(buffer, 0, buffer.Length, this.ResponseSendingCompleted, null);

                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_Task.LogAddException("Exception: " + m_Exception.Message, m_Exception);
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

            #region method ResponseSendingCompleted

            /// <summary>
            /// Is called when response sending has finished.
            /// </summary>
            /// <param name="ar">Asynchronous result.</param>
            private void ResponseSendingCompleted(IAsyncResult ar)
            {
                try
                {
                    m_Task.TcpStream.EndWrite(ar);
                }
                catch (Exception x)
                {
                    m_Exception = x;
                    m_Task.LogAddException("Exception: " + m_Exception.Message, m_Exception);
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
            public event EventHandler<EventArgs<SendResponseAsyncOperation>> CompletedAsync = null;

            #region method OnCompletedAsync

            /// <summary>
            /// Raises <b>CompletedAsync</b> event.
            /// </summary>
            private void OnCompletedAsync()
            {
                if (this.CompletedAsync != null)
                {
                    this.CompletedAsync(this, new EventArgs<SendResponseAsyncOperation>(this));
                }
            }

            #endregion

            #endregion
        }

        #endregion

        /// <summary>
        /// Sends TcpMessage server response.
        /// </summary>
        /// <param name="op">Asynchronous operation.</param>
        /// <returns>Returns true if aynchronous operation is pending (The <see cref="SendResponseAsyncOperation.CompletedAsync"/> event is raised upon completion of the operation).
        /// Returns false if operation completed synchronously.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>op</b> is null reference.</exception>
        private bool SendResponseAsync(SendResponseAsyncOperation op)
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
        
        #region method DATA
        //private bool DATA(string cmdText)
        //{
        //    return false;
        //}

        private bool DATA_Begin()
        {
            // RFC 5321 3.1.
            if (m_TaskRejected)
            {
                WriteLine(TcpMessageCode.BadSequenceCommands.ToString() + " bad sequence of commands: Task rejected.");
                return true;
            }
            //// RFC 5321 4.1.4.
            //if (string.IsNullOrEmpty(m_EhloHost))
            //{
            //    WriteLine("503 Bad sequence of commands: send EHLO/HELO first.");
            //    return true;
            //}
            //// RFC 5321 4.1.4.
            //if (m_From == null)
            //{
            //    WriteLine("503 Bad sequence of commands: send 'MAIL FROM:' first.");
            //    return true;
            //}
            //// RFC 5321 4.1.4.
            //if (m_To.Count == 0)
            //{
            //    WriteLine("503 Bad sequence of commands: send 'RCPT TO:' first.");
            //    return true;
            //}
            // RFC 3030 BDAT.
            //if (m_MessageStream != null)
            //{
            //    WriteLine("503 Bad sequence of commands: DATA and BDAT commands cannot be used in the same transaction.");
            //    return true;
            //}

            /* RFC 5321 4.1.1.4.
                The receiver normally sends a 354 response to DATA, and then treats
                the lines (strings ending in <CRLF> sequences, as described in
                Section 2.3.7) following the command as mail data from the sender.
                This command causes the mail data to be appended to the mail data
                buffer.  The mail data may contain any of the 128 ASCII character
                codes, although experience has indicated that use of control
                characters other than SP, HT, CR, and LF may cause problems and
                SHOULD be avoided when possible.
             
                The custom of accepting lines ending only in <LF>, as a concession to
                non-conforming behavior on the part of some UNIX systems, has proven
                to cause more interoperability problems than it solves, and TcpMessage
                server systems MUST NOT do this, even in the name of improved
                robustness.  In particular, the sequence "<LF>.<LF>" (bare line
                feeds, without carriage returns) MUST NOT be treated as equivalent to
                <CRLF>.<CRLF> as the end of mail data indication.
             
                Receipt of the end of mail data indication requires the server to
                process the stored mail transaction information.  This processing
                consumes the information in the reverse-path buffer, the forward-path
                buffer, and the mail data buffer, and on the completion of this
                command these buffers are cleared.  If the processing is successful,
                the receiver MUST send an OK reply.  If the processing fails, the
                receiver MUST send a failure reply.  The TcpMessage model does not allow
                for partial failures at this point: either the message is accepted by
                the server for delivery and a positive response is returned or it is
                not accepted and a failure reply is returned.  In sending a positive
                "250 OK" completion reply to the end of data indication, the receiver
                takes full responsibility for the message (see Section 6.1).  Errors
                that are diagnosed subsequently MUST be reported in a mail message,
                as discussed in Section 4.4.

                When the TcpMessage server accepts a message either for relaying or for
                final delivery, it inserts a trace record (also referred to
                interchangeably as a "time stamp line" or "Received" line) at the top
                of the mail data.  This trace record indicates the identity of the
                host that sent the message, the identity of the host that received
                the message (and is inserting this time stamp), and the date and time
                the message was received.  Relayed messages will have multiple time
                stamp lines.  Details for formation of these lines, including their
                syntax, is specified in Section 4.4.
            */

            DateTime startTime = DateTime.Now;

            m_MessageStream = OnGetMessageStream();
            if (m_MessageStream == null)
            {
                m_MessageStream = new MemoryFileStream(32000);
            }
            // RFC 5321.4.4 trace info.
            byte[] recevived = CreateReceivedHeader();
            m_MessageStream.Write(recevived, 0, recevived.Length);

            WriteLine(TcpMessageCode.StartMessageInput.ToString() + " Start message input; end with <CRLF>.<CRLF>");

            // Create asynchronous read partial-terminated opeartion.
            ExtendedStream.ReadPartialTerminatedAsyncOperation readPartialTermOp = new ExtendedStream.ReadPartialTerminatedAsyncOperation(
                m_MessageStream,
                m_MaxMessageSize,
                SizeExceededAction.SkipAndThrowException
            );

            // This event is raised only if read partial-terminated opeartion completes asynchronously.
            readPartialTermOp.Completed += new EventHandler<EventArgs<ExtendedStream.ReadPartialTerminatedAsyncOperation>>(delegate(object sender, EventArgs<ExtendedStream.ReadPartialTerminatedAsyncOperation> e)
            {
                DATA_End(startTime, readPartialTermOp);
            });

            // Read partial-terminated completed synchronously.
            if (this.TcpStream.ReadPartialTerminated(readPartialTermOp, true))
            {
                DATA_End(startTime, readPartialTermOp);

                return true;
            }
            // Read partial-terminated completed asynchronously, Completed event will be raised once operation completes.
            // else{

            return false;
        }

        /// <summary>
        /// Completes DATA command.
        /// </summary>
        /// <param name="startTime">Time DATA command started.</param>
        /// <param name="op">Read partial-terminated opeartion.</param>
        private void DATA_End(DateTime startTime, ExtendedStream.ReadPartialTerminatedAsyncOperation op)
        {
            try
            {
                if (op.Error != null)
                {
                    if (op.Error is LineExceededException)
                    {
                        WriteLine(TcpMessageCode.InternalServerError.ToString() + " Line too long.");
                    }
                    else if (op.Error is SizeExceededException)
                    {
                        WriteLine(TcpMessageCode.MessageSizeExceeds.ToString() + " Too much message data.");
                    }
                    else
                    {
                        OnError(op.Error);
                    }

                    OnMessageStoringCanceled();
                }
                else
                {
                    TcpReplyStream reply = new TcpReplyStream((int)TcpMessageCode.Ok, "DATA completed in " + (DateTime.Now - startTime).TotalSeconds.ToString("f2") + " seconds.");

                    reply = OnMessageStoringCompleted(reply);

                    WriteLine(reply.ToString());
                }
            }
            catch (Exception x)
            {
                OnError(x);
            }

            Reset();
            BeginReadCmd();
        }

        #endregion

        #region method BDAT

        private bool BDAT(string cmdText)
        {
            // RFC 5321 3.1.
            if (m_TaskRejected)
            {
                WriteLine(TcpMessageCode.BadSequenceCommands.ToString() + " bad sequence of commands: Session rejected.");
                return true;
            }
            //// RFC 5321 4.1.4.
            //if (string.IsNullOrEmpty(m_EhloHost))
            //{
            //    WriteLine("503 Bad sequence of commands: send EHLO/HELO first.");
            //    return true;
            //}
            //// RFC 5321 4.1.4.
            //if (m_From == null)
            //{
            //    WriteLine("503 Bad sequence of commands: send 'MAIL FROM:' first.");
            //    return true;
            //}
            //// RFC 5321 4.1.4.
            //if (m_To.Count == 0)
            //{
            //    WriteLine("503 Bad sequence of commands: send 'RCPT TO:' first.");
            //    return true;
            //}

            /* RFC 3030 2
				The BDAT verb takes two arguments.The first argument indicates the length, 
                in octets, of the binary data chunk. The second optional argument indicates 
                that the data chunk	is the last.
				
				The message data is sent immediately after the trailing <CR>
				<LF> of the BDAT command line.  Once the receiver-Smtp receives the
				specified number of octets, it will return a 250 reply code.

				The optional LAST parameter on the BDAT command indicates that this
				is the last chunk of message data to be sent.  The last BDAT command
				MAY have a byte-count of zero indicating there is no additional data
				to be sent.  Any BDAT command sent after the BDAT LAST is illegal and
				MUST be replied to with a 503 "Bad sequence of commands" reply code.
				The state resulting from this error is indeterminate.  A RSET command
				MUST be sent to clear the transaction before continuing.
				
				A 250 response MUST be sent to each successful BDAT data block within
				a mail transaction.

				bdat-cmd   ::= "BDAT" SP chunk-size [ SP end-marker ] CR LF
				chunk-size ::= 1*DIGIT
				end-marker ::= "LAST"
			*/

            DateTime startTime = DateTime.Now;

            int chunkSize = 0;
            bool last = false;
            string[] args = cmdText.Split(' ');
            if (cmdText == string.Empty || args.Length > 2)
            {
                WriteLine(TcpMessageCode.SyntaxError.ToString() + " Syntax error, syntax: \"BDAT\" SP chunk-size [SP \"LAST\"] CRLF");
                return true;
            }
            if (!int.TryParse(args[0], out chunkSize))
            {
                WriteLine(TcpMessageCode.SyntaxError.ToString() + " Syntax error(chunk-size must be integer), syntax: \"BDAT\" SP chunk-size [SP \"LAST\"] CRLF");
                return true;
            }
            if (args.Length == 2)
            {
                if (args[1].ToUpperInvariant() != "LAST")
                {
                    WriteLine(TcpMessageCode.SyntaxError.ToString() + " Syntax error, syntax: \"BDAT\" SP chunk-size [SP \"LAST\"] CRLF");
                    return true;
                }
                last = true;
            }

            // First BDAT block in transaction.
            if (m_MessageStream == null)
            {
                m_MessageStream = OnGetMessageStream();
                if (m_MessageStream == null)
                {
                    m_MessageStream = new MemoryFileStream(32000);
                }
                // RFC 5321.4.4 trace info.
                byte[] recevived = CreateReceivedHeader();
                m_MessageStream.Write(recevived, 0, recevived.Length);
            }

            Stream storeStream = m_MessageStream;
            // Maximum allowed message size exceeded.
            if ((m_BDatReadedCount + chunkSize) > m_MaxMessageSize)
            {
                storeStream = new DroperStream();
            }

            // Read data block.
            this.TcpStream.BeginReadFixedCount(
                storeStream,
                chunkSize,
                new AsyncCallback(delegate(IAsyncResult ar)
            {
                try
                {
                    this.TcpStream.EndReadFixedCount(ar);

                    m_BDatReadedCount += chunkSize;

                    // Maximum allowed message size exceeded.
                    if (m_BDatReadedCount > m_MaxMessageSize)
                    {
                        WriteLine(TcpMessageCode.MessageSizeExceeds.ToString() + " Too much message data.");

                        OnMessageStoringCanceled();
                    }
                    else
                    {
                        TcpReplyStream reply = new TcpReplyStream((int)TcpMessageCode.Ok, chunkSize + " bytes received in " + (DateTime.Now - startTime).TotalSeconds.ToString("f2") + " seconds.");

                        if (last)
                        {
                            reply = OnMessageStoringCompleted(reply);
                        }

                        WriteLine(reply.ToString());
                    }

                    if (last)
                    {
                        // Accoring RFC 3030, client should send RSET and we must wait it and reject transaction commands.
                        // If we reset internally, then all works as specified. 
                        Reset();
                    }
                }
                catch (Exception x)
                {
                    OnError(x);
                }

                BeginReadCmd();
            }),
                null
            );

            return false;
        }

        #endregion
                
        #region methods

        /// <summary>
        /// Does reset as specified in RFC 5321.
        /// </summary>
        private void Reset()
        {
            if (this.IsDisposed)
            {
                return;
            }

            m_MessageStream = null;
            //m_BDatReadedCount = 0;
        }


        /// <summary>
        /// Creates "Received:" header field. For more info see RFC 5321.4.4.
        /// </summary>
        /// <returns>Returns "Received:" header field.</returns>
        private byte[] CreateReceivedHeader()
        {
            /* 5321 4.4. Trace Information.
                When an TcpMessage server receives a message for delivery or further
                processing, it MUST insert trace ("time stamp" or "Received")
                information at the beginning of the message content, as discussed in
                Section 4.1.1.4.

               RFC 4954.7. Additional Requirements on Servers.
                As described in Section 4.4 of [TcpMessage], an TcpMessage server that receives a
                message for delivery or further processing MUST insert the
                "Received:" header field at the beginning of the message content.
                This document places additional requirements on the content of a
                generated "Received:" header field.  Upon successful authentication,
                a server SHOULD use the "ESMTPA" or the "ESMTPSA" [TcpMessage-TT] (when
                appropriate) keyword in the "with" clause of the Received header
                field.
               
               http://www.iana.org/assignments/mail-parameters
                ESMTP                TcpMessage with Service Extensions               [RFC5321]
                ESMTPA               ESMTP with TcpMessage AUTH                       [RFC3848]
                ESMTPS               ESMTP with STARTTLS                        [RFC3848]
                ESMTPSA              ESMTP with both STARTTLS and TcpMessage AUTH     [RFC3848]
            */

            //Nistec.Messaging.Net.Mail.MailHeaderReceived received = new Nistec.Messaging.Net.Mail.MailHeaderReceived(this.EhloHost, NetUtils.GetLocalHostName(this.LocalHostName), DateTime.Now);
            //received.From_TcpInfo = new Nistec.Messaging.Net.Mail.MailTcpInfo(this.RemoteEndPoint.Address, null);
            //received.Via = "Tcp";
            //if (!this.IsAuthenticated && !this.IsSecureConnection)
            //{
            //    received.With = "ESMTP";
            //}
            //else if (this.IsAuthenticated && !this.IsSecureConnection)
            //{
            //    received.With = "ESMTPA";
            //}
            //else if (!this.IsAuthenticated && this.IsSecureConnection)
            //{
            //    received.With = "ESMTPS";
            //}
            //else if (this.IsAuthenticated && this.IsSecureConnection)
            //{
            //    received.With = "ESMTPSA";
            //}

            //return Encoding.UTF8.GetBytes(received.ToString());

            return Encoding.UTF8.GetBytes(""); ;
        }

 
        /// <summary>
        /// Sends and logs specified line to connected host.
        /// </summary>
        /// <param name="line">Line to send.</param>
        private void WriteLine(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            int countWritten = this.TcpStream.WriteLine(line);

            // Log.
            //if (this.Server.Logger != null)
            //{
            //    this.Server.Logger.AddWrite(this.ID, this.AuthenticatedUserIdentity, countWritten, line, this.LocalEndPoint, this.RemoteEndPoint);
            //}
        }

        /// <summary>
        /// Sends and logs specified line to connected host.
        /// </summary>
        /// <param name="line">Line to send.</param>
        private void WriteReply(TcpReplyStream reply)
        {
            if (reply == null)
            {
                throw new ArgumentNullException("reply");
            }

            var buffer = reply.GetBuffer();

            this.TcpStream.Write(buffer, 0, buffer.Length);

            // Log.
            //if (this.Server.Logger != null)
            //{
            //    this.Server.Logger.AddWrite(this.ID, this.AuthenticatedUserIdentity, countWritten, reply.ReplyCode.ToString(), this.LocalEndPoint, this.RemoteEndPoint);
            //}
        }

        #endregion

        #region mehtod Log

        /// <summary>
        /// Logs read operation.
        /// </summary>
        /// <param name="size">Number of bytes readed.</param>
        /// <param name="text">Log text.</param>
        public void LogAddRead(long size, string text)
        {
            //try
            //{
            //    if (this.Server.Logger != null)
            //    {
            //        this.Server.Logger.AddRead(
            //            this.ID,
            //            this.AuthenticatedUserIdentity,
            //            size,
            //            text,
            //            this.LocalEndPoint,
            //            this.RemoteEndPoint
            //        );
            //    }
            //}
            //catch
            //{
            //    // We skip all logging errors, normally there shouldn't be any.
            //}
        }

   
        /// <summary>
        /// Logs write operation.
        /// </summary>
        /// <param name="size">Number of bytes written.</param>
        /// <param name="text">Log text.</param>
        public void LogAddWrite(long size, string text)
        {
            //try
            //{
            //    if (this.Server.Logger != null)
            //    {
            //        this.Server.Logger.AddWrite(
            //            this.ID,
            //            this.AuthenticatedUserIdentity,
            //            size,
            //            text,
            //            this.LocalEndPoint,
            //            this.RemoteEndPoint
            //        );
            //    }
            //}
            //catch
            //{
            //    // We skip all logging errors, normally there shouldn't be any.
            //}
        }

      
        /// <summary>
        /// Logs free text entry.
        /// </summary>
        /// <param name="text">Log text.</param>
        public void LogAddText(string text)
        {
            //try
            //{
            //    if (this.Server.Logger != null)
            //    {
            //        this.Server.Logger.AddText(
            //            this.IsConnected ? this.ID : "",
            //            this.IsConnected ? this.AuthenticatedUserIdentity : null,
            //            text,
            //            this.IsConnected ? this.LocalEndPoint : null,
            //            this.IsConnected ? this.RemoteEndPoint : null
            //        );
            //    }
            //}
            //catch
            //{
            //    // We skip all logging errors, normally there shouldn't be any.
            //}
        }

 
        /// <summary>
        /// Logs exception.
        /// </summary>
        /// <param name="text">Log text.</param>
        /// <param name="x">Exception happened.</param>
        public void LogAddException(string text, Exception x)
        {
            //try
            //{
            //    if (this.Server.Logger != null)
            //    {
            //        this.Server.Logger.AddException(
            //            this.IsConnected ? this.ID : "",
            //            this.IsConnected ? this.AuthenticatedUserIdentity : null,
            //            text,
            //            this.IsConnected ? this.LocalEndPoint : null,
            //            this.IsConnected ? this.RemoteEndPoint : null,
            //            x
            //        );
            //    }
            //}
            //catch
            //{
            //    // We skip all logging errors, normally there shouldn't be any.
            //}
        }

        #endregion

        #region Properties implementation

        ///// <summary>
        ///// Gets message owner TcpMessage server.
        ///// </summary>
        ///// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        //public new TcpListener Server
        //{
        //    get
        //    {
        //        if (this.IsDisposed)
        //        {
        //            throw new ObjectDisposedException(this.GetType().Name);
        //        }

        //        return (TcpListener)base.Server;
        //    }
        //}

        /// <summary>
        /// Gets supported SASL authentication methods collection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public Dictionary<string, AuthSaslServer> Authentications
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_Authentications;
            }
        }

        /// <summary>
        /// Gets number of bad commands happened on TcpMessage message.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public int BadCommands
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_BadCommands;
            }
        }

        /// <summary>
        /// Gets number of mail transactions processed by this TcpMessage message.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public int Transactions
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_Transactions;
            }
        }

        ///// <summary>
        ///// Gets client reported EHLO host name. Returns null if EHLO/HELO is not issued yet.
        ///// </summary>
        ///// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        //public string EhloHost
        //{
        //    get
        //    {
        //        if (this.IsDisposed)
        //        {
        //            throw new ObjectDisposedException(this.GetType().Name);
        //        }

        //        return m_EhloHost;
        //    }
        //}

        /// <summary>
        /// Gets authenticated user identity or null if user has not authenticated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public override GenericIdentity AuthenticatedUserIdentity
        {
            get
            {
                if (this.IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_User;
            }
        }

        ///// <summary>
        ///// Gets MAIL FROM: value. Returns null if MAIL FROM: is not issued yet.
        ///// </summary>
        ///// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        //public QueueHost From
        //{
        //    get
        //    {
        //        if (this.IsDisposed)
        //        {
        //            throw new ObjectDisposedException(this.GetType().Name);
        //        }

        //        return m_From;
        //    }
        //}

        ///// <summary>
        ///// Gets RCPT TO: values. Returns null if RCPT TO: is not issued yet.
        ///// </summary>
        ///// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        //public QueueHost[] To
        //{
        //    get
        //    {
        //        if (this.IsDisposed)
        //        {
        //            throw new ObjectDisposedException(this.GetType().Name);
        //        }

        //        lock (m_To)
        //        {
        //            QueueHost[] retVal = new QueueHost[m_To.Count];
        //            m_To.Values.CopyTo(retVal, 0);

        //            return retVal;
        //        }
        //    }
        //}

        #endregion

        #region Events implementation

        /// <summary>
        /// Is raised when message has started processing and needs to send 220 greeting or 554 error resposne to the connected client.
        /// </summary>
        public event EventHandler<TcpStartedEventArgs> Started = null;

        #region method OnStarted

        /// <summary>
        /// Raises <b>Started</b> event.
        /// </summary>
        /// <param name="reply">Default TcpMessage server reply.</param>
        /// <returns>Returns TcpMessage server reply what must be sent to the connected client.</returns>
        private TcpReplyStream OnStarted(TcpReplyStream reply)
        {
            if (this.Started != null)
            {
                TcpStartedEventArgs eArgs = new TcpStartedEventArgs(this, reply);
                this.Started(this, eArgs);

                return eArgs.Reply;
            }

            return reply;
        }

        #endregion

       

        /// <summary>
        /// Is raised when TcpMessage server needs to get stream where to store incoming message.
        /// </summary>
        public event EventHandler<TcpMessageEventArgs> GetMessageStream = null;

        #region method OnGetMessageStream

        /// <summary>
        /// Raises <b>GetMessageStream</b> event.
        /// </summary>
        /// <returns>Returns message store stream.</returns>
        private Stream OnGetMessageStream()
        {
            if (this.GetMessageStream != null)
            {
                TcpMessageEventArgs eArgs = new TcpMessageEventArgs(this);
                this.GetMessageStream(this, eArgs);

                return eArgs.Stream;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Is raised when TcpMessage server has canceled message storing.
        /// </summary>
        /// <remarks>This can happen on 2 cases: on message timeout and if between BDAT chunks RSET issued.</remarks>
        public event EventHandler MessageStoringCanceled = null;

        #region method OnMessageStoringCanceled

        /// <summary>
        /// Raises <b>MessageStoringCanceled</b> event.
        /// </summary>
        private void OnMessageStoringCanceled()
        {
            if (this.MessageStoringCanceled != null)
            {
                this.MessageStoringCanceled(this, new EventArgs());
            }
        }

        #endregion

        /// <summary>
        /// Is raised when TcpMessage server has completed message storing.
        /// </summary>
        public event EventHandler<TcpMessageCompletedEventArgs> MessageStoringCompleted = null;

        #region method OnMessageStoringCompleted

        /// <summary>
        /// Raises <b>MessageStoringCompleted</b> event.
        /// </summary>
        /// <param name="reply">Default TcpMessage server reply.</param>
        /// <returns>Returns TcpMessage server reply what must be sent to the connected client.</returns>
        private TcpReplyStream OnMessageStoringCompleted(TcpReplyStream reply)
        {
            //OnTaskCompleted();

            if (this.MessageStoringCompleted != null)
            {
                TcpMessageCompletedEventArgs eArgs = new TcpMessageCompletedEventArgs(this, m_MessageStream, reply);
                this.MessageStoringCompleted(this, eArgs);

                return eArgs.Reply;
            }

            return reply;
        }

        #endregion

        #endregion

    }
}
