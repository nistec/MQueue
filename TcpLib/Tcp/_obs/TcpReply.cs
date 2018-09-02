using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Nistec.IO;
using Nistec.Net.IO;
using Nistec.Net;

namespace Nistec.Messaging.Tcp
{
    /// <summary>
    /// This class implements TcpMessage server reply.
    /// </summary>
    public class TcpReply
    {
        private int m_ReplyCode = 0;
        private NetStream m_ReplyStream = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="replyCode">TcpMessage server reply code.</param>
        /// <param name="replyLine">TcpMessage server reply line.</param>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>replyLine</b> is null reference.</exception>
        public TcpReply(int replyCode, string replyLine)
            : this(replyCode, new NetStream(UTF8Encoding.UTF8.GetBytes("|TEXT|" + replyLine)))
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
        /// <param name="replyStream">TcpMessage server reply stream.</param>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>replyLines</b> is null reference.</exception>
        public TcpReply(int replyCode, NetStream replyStream)
        {
            if (replyCode < 200 || replyCode > 599)
            {
                throw new ArgumentException("Argument 'replyCode' value must be >= 200 and <= 599.", "replyCode");
            }
            if (replyStream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            m_ReplyCode = replyCode;
            m_ReplyStream = replyStream;
        }


        #region method override ToString

        /// <summary>
        /// Returns TcpMessage server reply as string.
        /// </summary>
        /// <returns>Returns TcpMessage server reply code as string.</returns>
        public override string ToString()
        {
             return ReplyCode.ToString();
        }

        public Stream ToStream()
        {
           var sumBytes = StreamBuffer.Combine(Encoding.UTF8.GetBytes(m_ReplyCode.ToString()), m_ReplyStream.ToArray());
           return new NetStream(sumBytes);
        }

        #endregion

        #region static method Parse

        /// <summary>
        /// Parses TcpMessage reply-line from 
        /// </summary>
        /// <param name="line">TcpMessage server reply-line.</param>
        /// <returns>Returns parsed TcpMessage server reply-line.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>line</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when reply-line parsing fails.</exception>
        public static TcpReply Parse(NetStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            /* RFC 5321 4.2.
                Reply-line     = *( Reply-code "-" [ textstring ] CRLF )
                                 Reply-code [ SP textstring ] CRLF
             
                Since, in violation of this specification, the text is sometimes not sent, clients that do not
                receive it SHOULD be prepared to process the code alone (with or without a trailing space character).
            */

           

            if (stream.Length < 3)
            {
                throw new ParseException("Invalid TcpMessage server reply.");
            }
            int replyCode = stream.PeekInt32(0);
            if (replyCode<=0)
            {
                throw new ParseException("Invalid TcpMessage server reply '" + replyCode + "' reply-code.");
            }

            string msgtype = stream.PeekString(4, 6);

            var bytes = stream.PeekBytes(7, stream.iLength-9);


            bool isLastLine = true;
            if (line.Length > 3)
            {
                isLastLine = (line[3] == ' ');
            }

            string text = "";
            if (line.Length > 5)
            {
                text = line.Substring(4);
            }

            return new TcpReply(replyCode, text, isLastLine);
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
        /// Gets TcpMessage server reply stream.
        /// </summary>
        public Stream ReplyStream
        {
            get { return m_ReplyStream; }
        }

        #endregion
    }
}
