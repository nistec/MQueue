using System;
using System.Collections.Generic;
using System.Text;

namespace Nistec.Messaging.Tcp
{
    /// <summary>
    /// TcpMessage client exception.
    /// </summary>
    public class TcpClientException : Exception
    {
        private TcpReplyStream m_ReplyStream = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="responseLine">TcpMessage server response line.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>responseLine</b> is null.</exception>
        public TcpClientException(string responseLine) : base(responseLine.TrimEnd())
        {
            if(responseLine == null){
                throw new ArgumentNullException("responseLine");
            }

            m_ReplyStream = TcpReplyStream.Parse(responseLine);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="replyStream">TcpMessage server error reply lines.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>replyLines</b> is null reference.</exception>
        public TcpClientException(TcpReplyStream replyStream)
            : base(replyStream.Text.TrimEnd())
        {
            if (replyStream == null)
            {
                throw new ArgumentNullException("replyStream");
            }

            m_ReplyStream = replyStream;            
        }


        #region Properties 
        /*
        /// <summary>
        /// Gets TcpMessage status code.
        /// </summary>
        [Obsolete("Use property 'ReplyLines' insead.")]
        public int StatusCode
        {
            get{ return m_ReplyLines[0].ReplyCode; }
        }

        /// <summary>
        /// Gets TcpMessage server response text after status code.
        /// </summary>
        [Obsolete("Use property 'ReplyLines' insead.")]
        public string ResponseText
        {
            get{ return m_ReplyLines[0].Text; }
        }
        */
        /// <summary>
        /// Gets TcpMessage server error reply lines.
        /// </summary>
        public TcpReplyStream ReplyStream
        {
            get { return m_ReplyStream; }
        }

        /// <summary>
        /// Gets if it is permanent TcpMessage(5xx) error.
        /// </summary>
        public bool IsPermanentError
        {
            get{
                if (m_ReplyStream.ReplyCode >= 500 && m_ReplyStream.ReplyCode <= 599)
                {
                    return true;
                }
                else{
                    return false;
                }
            }
        }

        #endregion

        #region Nistec
        /// <summary>
        /// StatusCode
        /// </summary>
        public int StatusCode
        {
            get
            {
                if (m_ReplyStream != null)// && m_ReplyStream.Length > 0)
                    return m_ReplyStream.ReplyCode;
                return 0;
            }
        }
        /// <summary>
        /// ResponseText
        /// </summary>
        public string ResponseText
        {
             get
            {
                if (m_ReplyStream != null)// && m_ReplyLines.Length > 0)
                    return m_ReplyStream.Text;
                return null;
            }
        }

        #endregion
    }
}
