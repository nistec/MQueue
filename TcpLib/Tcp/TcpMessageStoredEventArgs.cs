using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Nistec.Messaging.Tcp
{
    /// <summary>
    /// This class provided data for <b cref="TcpMessage.MessageStoringCompleted">MqTask.MessageStoringCompleted</b> event.
    /// </summary>
    public class TcpMessageCompletedEventArgs : EventArgs
    {
        private TcpMessage m_Task = null;
        private Stream       m_Stream  = null;
        private TcpReplyStream m_Reply = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="message">Owner TcpMessage server message.</param>
        /// <param name="stream">Message stream.</param>
        /// <param name="reply">TcpMessage server reply.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>message</b>, <b>stream</b> or <b>reply</b> is null reference.</exception>
        public TcpMessageCompletedEventArgs(TcpMessage message, Stream stream, TcpReplyStream reply)
        {
            if(message == null){
                throw new ArgumentNullException("message");
            }
            if(stream == null){
                throw new ArgumentNullException("stream");
            }
            if(reply == null){
                throw new ArgumentNullException("reply");
            }

            m_Task = message;
            m_Stream  = stream;
            m_Reply   = reply;
        }


        #region Properties implementation

        /// <summary>
        /// Gets owner TcpMessage message.
        /// </summary>
        public TcpMessage Task
        {
            get{ return m_Task; }
        }

        /// <summary>
        /// Gets message stream where message has stored.
        /// </summary>
        public Stream Stream
        {
            get{ return m_Stream; }
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
