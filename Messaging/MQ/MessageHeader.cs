using Nistec.Channels;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Messaging
{
    public class QHeader //: TransformHeader
    {

        public QHeader() { }

        internal QHeader(NameValueCollection QueryString) 
        {
            //Version = QueryString.Get<int>("Version");
            //var MessageState = (MessageState)QueryString.Get<byte>("MessageState");
            MessageType = EnumExtension.ParseOrCast<MQTypes>(QueryString.Get("MessageType"), MQTypes.MessageRequest);
            QCommand = EnumExtension.ParseOrCast<QueueCmd>(QueryString.Get("QCommand"), QueueCmd.None);
            TransformType = EnumExtension.ParseOrCast<TransformType>(QueryString.Get("TransformType"), TransformType.None);
            DuplexType = EnumExtension.ParseOrCast<DuplexTypes>(QueryString.Get("DuplexType"), DuplexTypes.Respond);
            //mqh-Expiration = QueryString.Get<int>("Expiration");
            Host = QueryString.Get("Host");
        }

        /// <summary>
        /// Get Command
        /// </summary>
        public QueueCmd QCommand { get; set; }
        /// <summary>
        /// Get or Set message type.
        /// </summary>
        public MQTypes MessageType { get; set; }
        /// <summary>
        /// Get The message Destination\Queue name.
        /// </summary>
        public string Host { get; set; }
        //public int Version { get; set; }
               

        #region ITransformMessage

        /// <summary>
        /// Get indicate wether the message is a duplex type.
        /// </summary>
        public bool IsDuplex { get { return DuplexType != DuplexTypes.None; } }
    
        public virtual DuplexTypes DuplexType { get; set; }
        public virtual TransformType TransformType { get; set; }

        #endregion
    }

    public class MessageHeader : ISerialEntity
    {


        internal MessageHeader(string identifier)
        {
            Creation = DateTime.Now;
            Identifier = Types.NZorEmpty(identifier, Ptr.NewIdentifier());
            IsDuplex = true;
            MessageType = MQTypes.Message;
            Version = 4022;
            //EncodingName = DefaultEncoding;
        }
        public MessageHeader() : this((string)null)
        {
        }
        public MessageHeader(Stream stream)
        {
            EntityRead(stream, null);
        }
        internal MessageHeader(MessageHeader h) : this(h.Identifier)
        {
            //MessageState = (MessageState)streamer.ReadValue<byte>();
            MessageType = h.MessageType;
            QCommand = h.QCommand;
            // Command = (QueueCmd)streamer.ReadValue<byte>();
            //Priority = (Priority)streamer.ReadValue<byte>();
            //Identifier = h.Identifier;//.ReadValue<Guid>();
            //Retry = streamer.ReadValue<byte>();
            //ArrivedTime = streamer.ReadValue<DateTime>();
            Creation = h.Creation;
            //Modified = streamer.ReadValue<DateTime>();
            //Duration = streamer.ReadValue<int>();
            TransformType = h.TransformType;
            Host = h.Host;
            //Sender = streamer.ReadString();
            //Label = streamer.ReadString();
            CustomId = h.CustomId;
            SessionId = h.SessionId;
            Expiration = h.Expiration;
        }

        #region property

        //public int Version { get { return 4022; } }
        public int Version { get; internal set; }
        /// <summary>
        /// Get ItemId
        /// </summary>
        public string Identifier { get; private set; }


        ///// <summary>
        ///// Get MessageState
        ///// </summary>
        //public MessageState MessageState { get; set; }

        /// <summary>
        /// Get Command
        /// </summary>
        public QueueCmd QCommand { get; set; }

        ///// <summary>
        ///// Get or Set transformation type.
        ///// </summary>
        //public TransformTypes TransformType { get; set; }

        /// <summary>
        /// Get or Set message type.
        /// </summary>
        public MQTypes MessageType { get; set; }

        ///// <summary>
        ///// Get Priority
        ///// </summary>
        //public Priority Priority { get; set; }

        /// <summary>
        /// Get The message Destination\Queue name.
        /// </summary>
        public string Host { get; set; }

        ///// <summary>
        ///// Get Retry
        ///// </summary>
        //public byte Retry { get; set; }

        ///// <summary>
        ///// Get ArrivedTime
        ///// </summary>
        //public DateTime ArrivedTime { get; set; }
        /// <summary>
        /// Get Creation Time
        /// </summary>
        public DateTime Creation { get; set; }
        ///// <summary>
        ///// Get the last modified time.
        ///// </summary>
        //public DateTime Modified { get; set; }

        ///// <summary>
        ///// Get or Set Duration in seconds
        ///// </summary>
        //public int Duration { get; set; }
        ///// <summary>
        ///// Get or Set Expiration in minutes
        ///// </summary>
        //public int Expiration { get; set; }        
        ///// <summary>
        ///// Get or Set The message Sender.
        ///// </summary>
        //public string Sender { get; internal set; }

        ///// <summary>
        ///// Get or Set The message Label.
        ///// </summary>
        //public string Label { get; set; }
        /// <summary>
        /// Get or Set The message CustomId.
        /// </summary>
        public string CustomId { get; set; }
        /// <summary>
        /// Get or Set The message SessionId.
        /// </summary>
        public string SessionId { get; set; }

        public bool IsExpired
        {
            get { return Expiration == 0 ? true : Creation.AddMinutes(Expiration) > DateTime.Now; }
        }
        #endregion

        #region ITransformMessage

        /// <summary>
        /// Get indicate wether the message is a duplex type.
        /// </summary>
        bool _IsDuplex;
        public bool IsDuplex
        {
            get { return _IsDuplex; }
            set
            {
                _IsDuplex = value;
                if (!value)
                    _DuplexType = DuplexTypes.None;
                else if (_DuplexType == DuplexTypes.None)
                    _DuplexType = DuplexTypes.Respond;
            }
        }

        /// <summary>
        /// Get or Set DuplexType.
        /// </summary>
        DuplexTypes _DuplexType;
        public DuplexTypes DuplexType
        {
            get { return _DuplexType; }
            set
            {
                _DuplexType = value;
                _IsDuplex = (_DuplexType != DuplexTypes.None);
            }
        }

        /// <summary>
        ///  Get or Set The message expiration int minutes.
        /// </summary>
        public int Expiration { get; set; }

        public virtual TransformType TransformType { get; set; }

        //public StringFormatType FormatType { get { return (StringFormatType)(int)TransformType; } }
        #endregion

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

            //streamer.WriteValue((byte)MessageState);
            streamer.WriteValue((byte)MessageType);
            streamer.WriteValue((byte)QCommand);
            //streamer.WriteValue((byte)Priority);
            streamer.WriteString(Identifier);//.WriteValue(ItemId);
            //streamer.WriteValue(Retry);
            //streamer.WriteValue(ArrivedTime);
            streamer.WriteValue(Creation);
            //streamer.WriteValue(Modified);
            //streamer.WriteValue(Duration);
            streamer.WriteValue((byte)TransformType);
            streamer.WriteString(Host);
            //streamer.WriteString(Sender);
            //streamer.WriteString(Label);
            streamer.WriteString(CustomId);
            streamer.WriteString(SessionId);
            streamer.WriteValue(Expiration);
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

            //MessageState = (MessageState)streamer.ReadValue<byte>();
            MessageType = (MQTypes)streamer.ReadValue<byte>();
            QCommand = (QueueCmd)streamer.ReadValue<byte>();
            //Priority = (Priority)streamer.ReadValue<byte>();
            Identifier = streamer.ReadString();//.ReadValue<Guid>();
            //Retry = streamer.ReadValue<byte>();
            //ArrivedTime = streamer.ReadValue<DateTime>();
            Creation = streamer.ReadValue<DateTime>();
            //Modified = streamer.ReadValue<DateTime>();
            //Duration = streamer.ReadValue<int>();
            TransformType = (TransformType)streamer.ReadValue<byte>();
            Host = streamer.ReadString();
            //Sender = streamer.ReadString();
            //Label = streamer.ReadString();
            CustomId = streamer.ReadString();
            SessionId = streamer.ReadString();
            Expiration = streamer.ReadValue<int>();

        }

        #endregion

        public string Print()
        {

            return string.Format("Host:{0},Command:{1},MessageType:{2},SessionId:{3},Creation:{4},Identifier:{5}",
            Host,
            QCommand.ToString(),
            MessageType.ToString(),
            //Retry,
            SessionId,
            Creation,
            Identifier);
        }

        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }
        public byte[] ToBinary()
        {
            return ToStream().ToArray();
        }

        public string ToBase64()
        {
            return BinarySerializer.ToBase64(ToBinary());
        }
        public static MessageHeader FromBase64(string base64String)
        {
            return FromBinary(BinarySerializer.FromBase64(base64String));
        }

        internal static MessageHeader FromBinary(byte[] value)
        {
            return new MessageHeader(new NetStream(value));
        }
    }
}
