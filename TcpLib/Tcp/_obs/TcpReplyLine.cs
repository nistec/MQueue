using System;
using System.Collections.Generic;
using System.Text;
using Nistec.Net;

namespace Nistec.Messaging.Tcp
{
    /// <summary>
    /// This class represent s TcpMessage server reply-line. Defined in RFC 5321 4.2.
    /// </summary>
    public class TcpReplyLine
    {
        private int    m_ReplyCode  = 0;
        private string m_Text       = null;
        private bool   m_IsLastLine = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="replyCode">TcpMessage server reply code.</param>
        /// <param name="text">TcpMessage server reply text.</param>
        /// <param name="isLastLine">Specifies if this line is last line in response.</param>
        public TcpReplyLine(int replyCode,string text,bool isLastLine)
        {
            if(text == null){
                text = "";
            }

            m_ReplyCode  = replyCode;
            m_Text       = text;
            m_IsLastLine = isLastLine;
        }


        #region static method Parse

        /// <summary>
        /// Parses TcpMessage reply-line from 
        /// </summary>
        /// <param name="line">TcpMessage server reply-line.</param>
        /// <returns>Returns parsed TcpMessage server reply-line.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>line</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when reply-line parsing fails.</exception>
        public static TcpReplyLine Parse(string line)
        {
            if(line == null){
                throw new ArgumentNullException("line");
            }

            /* RFC 5321 4.2.
                Reply-line     = *( Reply-code "-" [ textstring ] CRLF )
                                 Reply-code [ SP textstring ] CRLF
             
                Since, in violation of this specification, the text is sometimes not sent, clients that do not
                receive it SHOULD be prepared to process the code alone (with or without a trailing space character).
            */

            if (line.Length < 3)
            {
                throw new ParseException("Invalid TcpMessage server reply-line '" + line + "'.");
            }

            int replyCode = 0;
            if (!int.TryParse(line.Substring(0, 3), out replyCode))
            {
                throw new ParseException("Invalid TcpMessage server reply-line '" + line + "' reply-code.");
            }
            
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

            return new TcpReplyLine(replyCode,text,isLastLine);
        }

        #endregion


        #region override method ToString

        /// <summary>
        /// Returns this as TcpMessage server <b>reply-line</b>.
        /// </summary>
        /// <returns>Returns this as TcpMessage server <b>reply-line</b>.</returns>
        public override string ToString()
        {
            if(m_IsLastLine){
                return m_ReplyCode.ToString() + " " + m_Text + "\r\n";
            }
            else{
                return m_ReplyCode.ToString() + "-" + m_Text + "\r\n";
            }
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets TcpMessage server reply code.
        /// </summary>
        public int ReplyCode
        {
            get{ return m_ReplyCode; }
        }

        /// <summary>
        /// Gets TcpMessage server relpy text.
        /// </summary>
        public string Text
        {
            get{ return m_Text; }
        }

        /// <summary>
        /// Gets if this is last reply line.
        /// </summary>
        public bool IsLastLine
        {
            get{ return m_IsLastLine; }
        }

        #endregion

  
    }
}
