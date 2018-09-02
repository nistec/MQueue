using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Nistec.Messaging.Tcp
{
    /// <summary>
    /// This class provided data for <b cref="TcpMessage.GetMessageStream">TcpMessage.GetMessageStream</b> event.
    /// </summary>
    public class TcpMessageEventArgs : EventArgs
    {
        private TcpMessage m_Task = null;
        private Stream       m_Stream  = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="message">Owner TcpMessage server message.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>message</b> is null reference.</exception>
        public TcpMessageEventArgs(TcpMessage message)
        {
            if(message == null){
                throw new ArgumentNullException("message");
            }

            m_Task = message;
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
        /// Gets or stes stream where to store incoming message.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null reference is passed.</exception>
        public Stream Stream
        {
            get{ return m_Stream; }

            set{ 
                if(value == null){
                    throw new ArgumentNullException("Stream");
                }

                m_Stream = value; 
            }
        }

        #endregion
    }
}
