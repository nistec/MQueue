using System;
using System.Collections.Generic;
using System.Text;

using Nistec.Net.Tcp;
using Nistec.Net.Auth;
using System.IO;
using Nistec.Generic;
using Nistec.Messaging.Adapters;
using Nistec.Messaging.Remote;

namespace Nistec.Messaging.Tcp
{
    /// <summary>
    /// This class implements Tcp server.
    /// </summary>
    public class TcpListener : Nistec.Net.Tcp.TcpServer<TcpMessage>
    {
        private int          m_MaxBadCommands     = 30;
        private int          m_MaxTransactions    = 10;
        private int          m_MaxMessageSize     = 10000000;
 
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TcpListener()
        {

        }

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

                Message msg = new Message(e.Stream,null);

                QueueClient client=new QueueClient(msg.Host);

                TcpAdapter adapter=new TcpAdapter(new AdapterProperties(msg.Host,null);
                switch(msg.Command)
                {
                    case "Enqueue":
                        var ack=client.Send(msg);
                        e.Reply = new TcpReply(200, ack..Serialize());
                        break;
                    case "":
                        var m= client.Receive();
                        e.Reply = new TcpReply(200, msg.Serialize());
                        break;
                }


                OnMessageCompleted(new GenericEventArgs<Message>(msg));
            }
            catch (Exception x)
            {

                OnError(x);

                e.Reply = new TcpReply(552, "Requested action aborted: Internal server error.");
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
           if(MessageCompleted!=null)
               MessageCompleted(this,e);
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
            get{
                if(this.IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_MaxBadCommands; 
            }

            set{
                if(this.IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if(value < 0){
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
            get{
                if(this.IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_MaxTransactions; 
            }

            set{
                if(this.IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                if(value < 0){
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
            get{
                if(this.IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_MaxMessageSize; 
            }

            set{
                if(this.IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                if(value < 500){
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
