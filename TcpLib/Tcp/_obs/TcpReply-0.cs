using System;
using System.Collections.Generic;
using System.Text;

namespace Nistec.Messaging.Tcp
{
    /// <summary>
    /// This class implements TcpMessage server reply.
    /// </summary>
    public class TcpReply
    {
        private int m_ReplyCode = 0;
        private string[] m_ReplyLines = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="replyCode">TcpMessage server reply code.</param>
        /// <param name="replyLine">TcpMessage server reply line.</param>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>replyLine</b> is null reference.</exception>
        public TcpReply(int replyCode, string replyLine)
            : this(replyCode, new string[] { replyLine })
        {
            if (replyLine == null)
            {
                throw new ArgumentNullException("replyLine");
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="replyCode">TcpMessage server reply code.</param>
        /// <param name="replyLines">TcpMessage server reply line(s).</param>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>replyLines</b> is null reference.</exception>
        public TcpReply(int replyCode, string[] replyLines)
        {
            if (replyCode < 200 || replyCode > 599)
            {
                throw new ArgumentException("Argument 'replyCode' value must be >= 200 and <= 599.", "replyCode");
            }
            if (replyLines == null)
            {
                throw new ArgumentNullException("replyLines");
            }
            if (replyLines.Length == 0)
            {
                throw new ArgumentException("Argument 'replyLines' must conatin at least one line.", "replyLines");
            }

            m_ReplyCode = replyCode;
            m_ReplyLines = replyLines;
        }


        #region method override ToString

        /// <summary>
        /// Returns TcpMessage server reply as string.
        /// </summary>
        /// <returns>Returns TcpMessage server reply as string.</returns>
        public override string ToString()
        {
            StringBuilder retVal = new StringBuilder();
            for (int i = 0; i < m_ReplyLines.Length; i++)
            {
                // Last line.
                if (i == (m_ReplyLines.Length - 1))
                {
                    retVal.Append(m_ReplyCode + " " + m_ReplyLines[i] + "\r\n");
                }
                else
                {
                    retVal.Append(m_ReplyCode + "-" + m_ReplyLines[i] + "\r\n");
                }
            }

            return retVal.ToString();
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets TcpMessage server reply code.
        /// </summary>
        public int ReplyCode
        {
            get { return m_ReplyCode; }
        }

        /// <summary>
        /// Gets TcpMessage server reply lines.
        /// </summary>
        public string[] ReplyLines
        {
            get { return m_ReplyLines; }
        }

        #endregion
    }
}
