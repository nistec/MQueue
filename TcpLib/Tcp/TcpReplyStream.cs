using System;
using System.Collections.Generic;
using System.Text;
using Nistec.Net;
using Nistec.IO;
using System.IO;
using Nistec.Runtime;

namespace Nistec.Messaging.Tcp
{

    public enum TcpReplyType : byte
    {
        Text = 100,
        Stream = 101
    }

    /// <summary>
    /// This class represent s TcpMessage server reply-stream.
    /// </summary>
    public class TcpReplyStream : ISerialEntity,IMessage
    {
        public const string MsgReplyEnd = "END\r\n";
        public const string MsgReplyBegin = "RPLY";


        private int    m_ReplyCode  = 0;
        private TcpReplyType m_ReplyType = TcpReplyType.Text;
        private string m_Text = null;
        private NetStream m_ReplyStream = null;

        public NetStream BodyStream
        {
            get { return m_ReplyStream; }
        }
        
        private bool   m_IsLastLine = true;




        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="replyCode">TcpMessage server reply code.</param>
        /// <param name="text">TcpMessage server reply text.</param>
        /// <param name="isLastLine">Specifies if this line is last line in response.</param>
        public TcpReplyStream(int replyCode, string text, bool isLastLine=true)
        {
            if (text == null)
            {
                text = "";
            }
            m_ReplyType = TcpReplyType.Text;
            m_ReplyCode = replyCode;
            m_Text = text;
            m_ReplyStream = new NetStream(Encoding.UTF8.GetBytes(text));
            m_IsLastLine = isLastLine;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="replyCode">TcpMessage server reply code.</param>
        /// <param name="stream"></param>
        /// <param name="isLastLine">Specifies if this line is last line in response.</param>
        public TcpReplyStream(int replyCode, NetStream stream, bool isLastLine=true)
        {
            m_ReplyType = TcpReplyType.Stream;
            m_ReplyStream = stream;
            m_ReplyCode = replyCode;
            m_Text = null;
            m_IsLastLine = isLastLine;
        }

        internal TcpReplyStream(NetStream stream)
        {
            EntityRead(stream,null);
        }

        #region  ISerialEntity

        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteFixedString(TcpReplyStream.MsgReplyBegin, 4);
            streamer.WriteValue((int)ReplyCode);
            streamer.WriteValue((byte)m_ReplyType);
            if (m_ReplyType == TcpReplyType.Stream)
                streamer.WriteValue(m_ReplyStream);
            else
                streamer.WriteString(m_Text);

            streamer.WriteFixedString(TcpReplyStream.MsgReplyEnd, 5);
            streamer.Flush();
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            string mmsg = streamer.ReadFixedString();
            if (mmsg != TcpReplyStream.MsgReplyBegin)
            {
                throw new Exception("Incorrect message format");
            }

            m_ReplyCode = streamer.ReadValue<int>();
            m_ReplyType = (TcpReplyType)streamer.ReadValue<byte>();
            if (m_ReplyType == TcpReplyType.Stream)
                m_ReplyStream = (NetStream)streamer.ReadValue();
            else
                m_Text = streamer.ReadString();

            string endmsg = streamer.ReadFixedString();
            if (endmsg != TcpReplyStream.MsgReplyEnd)
            {
                throw new Exception("Incorrect message format");
            }
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
        public static TcpReplyStream Parse(string line)
        {
            if (line == null)
            {
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

            return new TcpReplyStream(replyCode, text, isLastLine);
        }

        /// <summary>
        /// Parses TcpMessage reply-stream 
        /// </summary>
        /// <param name="stream">TcpMessage server reply-stream.</param>
        /// <returns>Returns parsed TcpMessage server reply-line.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>line</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when reply-line parsing fails.</exception>
        public static TcpReplyStream Parse(Stream stream)
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

            TcpReplyStream reply = new TcpReplyStream(NetStream.EnsureNetStream(stream));
 
            if (reply.ReplyCode <= 0)
            {
                throw new ParseException("Invalid TcpMessage server reply-stream '" + reply.ReplyCode.ToString() + "' reply-code.");
            }

            return reply;

        }


        #endregion


        #region override method ToString

        /// <summary>
        /// Returns this as TcpMessage server <b>reply-line</b>.
        /// </summary>
        /// <returns>Returns this as TcpMessage server <b>reply-line</b>.</returns>
        public override string ToString()
        {
            if (m_IsLastLine)
            {
                return m_ReplyCode.ToString() + " " + m_Text + MsgReplyEnd;
            }
            else
            {
                return m_ReplyCode.ToString() + "-" + m_Text + "\r\n";
            }
        }

        public byte[] GetBuffer()
        {
            using (NetStream stream = new NetStream())
            {
                EntityWrite(stream, null);
                return stream.ToArray();
            }
        }

        public string GetMessageContext()
        {
            NetStream stream = BodyStream;
            if (stream == null)
            {
                return null;
            }
            return stream.PeekString(1, 4);
        }

        public Message GetMessage()
        {
            NetStream stream = BodyStream;
            if (stream == null)
            {
                return null;
            }
            //string m = stream.PeekString(1, 4);
            //if (m == MessageContext.MMSG)
            //{
                return new Message(stream, null, MessageState.Receiving);
            //}
            //return null;
        }

        public Message GetResponse()
        {
            NetStream stream = BodyStream;
            if (stream == null)
            {
                return null;
            }
            //string m = stream.PeekString(1, 4);
            //if (m == MessageContext.QACK)
            //{
            //    return new Message(stream,null, MessageState.Receiving);
            //}
            //return null;

            return new Message(stream, null, MessageState.Receiving);
        }

        //public IMessage ParseMessageContext()
        //{
        //    NetStream stream = BodyStream;
        //    if (stream == null)
        //    {
        //        return null;
        //    }
        //    string m = stream.PeekString(1, 4);
        //    switch (m)
        //    {
        //        case MessageContext.MMSG:
        //            return new Message(stream, null);
        //        case MessageContext.QREQ:
        //            return new QueueRequest(stream);
        //        case MessageContext.QACK:
        //            return new QueueResponse(stream);
        //        default:
        //            throw new ArgumentException("Could not parse MessageContext, Incorrect stream formt");
        //    }
        //}

        //public Stream ToStream()
        //{


        //    var sumBytes = ToBuffer();
        //    return new NetStream(sumBytes);

        //}

        //public byte[] ToBuffer()
        //{
        //    return StreamBuffer.Combine(Encoding.UTF8.GetBytes(m_ReplyCode.ToString() + ((byte)m_ReplyType).ToString()),
        //        m_ReplyStream.ToArray(),
        //        Encoding.UTF8.GetBytes("LAST\r\n")
        //        );
        //}

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
            get{ return m_Text ?? ""; }
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
