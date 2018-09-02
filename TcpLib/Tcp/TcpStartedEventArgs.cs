using System;
using System.Collections.Generic;
using System.Text;

namespace Nistec.Messaging.Tcp
{
    /// <summary>
    /// This class provides data for <b cref="TcpMessage.Started">TcpMessage.Started</b> event.
    /// </summary>
    public class TcpStartedEventArgs : EventArgs
    {
        private TcpMessage m_Task = null;
        private TcpReplyStream m_Reply = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="message">Owner TcpMessage server message.</param>
        /// <param name="reply">TcpMessage server reply.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>message</b> or <b>reply</b> is null reference.</exception>
        public TcpStartedEventArgs(TcpMessage message, TcpReplyStream reply)
        {
            if(message == null){
                throw new ArgumentNullException("message");
            }
            if(reply == null){
                throw new ArgumentNullException("reply");
            }

            m_Task = message;
            m_Reply   = reply;
        }


        #region Properties impelemntation

        /// <summary>
        /// Gets owner TcpMessage message.
        /// </summary>
        public TcpMessage Task
        {
            get{ return m_Task; }
        }

        /// <summary>
        /// Gets or sets TcpMessage server reply.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null reference passed.</exception>
        public TcpReplyStream Reply
        {
            get{ return m_Reply; }

            set{
                if(value == null){
                    throw new ArgumentNullException("Reply");
                }

                m_Reply = value;
            }
        }

        #endregion
    }
}
