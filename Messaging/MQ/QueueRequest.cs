using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using Nistec.Data.Entities;
using Nistec.Messaging.Remote;
using Nistec.Serialization;
using Nistec.Channels;
using System.Collections.Specialized;

namespace Nistec.Messaging
{

    /// <summary>
    /// Represent a message stream for network communication like namedPipe or Tcp.
    /// This message can serialize/desrialize fast and easly using the <see cref="BinaryStreamer"/>
    /// </summary>
    [Serializable]
    public sealed class QueueRequest : MessageStream, IQueueRequest, IDisposable
    {
        #region static


        ///// <summary>
        ///// Get the default formatter.
        ///// </summary>
        //public static Formatters DefaultFormatter { get { return Formatters.BinarySerializer; } }

        #endregion

        #region property
        //public QHeader Header { get; private set; }

        public int Version { get; internal set; }

        ///// <summary>
        ///// Get Creation Time
        ///// </summary>
        //public DateTime Creation { get; internal set; }
        ///// <summary>
        ///// Get Command
        ///// </summary>
        //public QueueCmd QCommand { get; internal set; }

        //QueueCmd _QCommand;
        ///// <summary>
        ///// Get Command
        ///// </summary>
        //public QueueCmd QCommand
        //{
        //    get { return _QCommand; }
        //    set
        //    {
        //        _QCommand = value;
        //        Command = value.ToString();
        //    }
        //}

        ///// <summary>
        ///// Get or Set transformation type.
        ///// </summary>
        //public TransformTypes TransformType { get; set; }


        /// <summary>
        /// Get or Set transformation type.
        /// </summary>
        //public TransformType TransformType { get; set; }
        ///// <summary>
        ///// Get or Set indicate wether the message is a duplex type.
        ///// </summary>
        //public bool IsDuplex { get; set; }
        ///// <summary>
        /////  Get or Set The message expiration.
        ///// </summary>
        //public int Expiration { get; set; }


        /// <summary>
        /// Get Command
        /// </summary>
        public QueueCmd QCommand
        {
            get { return Command == null ? QueueCmd.None : EnumExtension.Parse<QueueCmd>(Command); }
        }

        /// <summary>
        /// Get Priority
        /// </summary>
        public Priority Priority { get; set; }

        /// <summary>
        /// Get The message Host\Queue name.
        /// </summary>
        public string Host { get; set; }

        ///// <summary>
        ///// Get ItemId
        ///// </summary>
        //public string Identifier { get; set; }

        ///// <summary>
        ///// Get or Set message type.
        ///// </summary>
        //public MQTypes MessageType { get { return MQTypes.MessageRequest; } }

        //public NetStream BodyStream { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        //public TransStream ToStream()
        //{
        //    TransStream stream = new TransStream(this);
        //    //EntityWrite(stream, null);
        //    return stream;
        //}

        /// <summary>
        /// Deserialize body stream to object.
        /// </summary>
        /// <returns></returns>
        public object GetBody()
        {
            if (_Body == null)
                return null;
            //BodyStream.Position = 0;
            using (var stream = BodyStream())
            {
                var ser = new BinarySerializer();
                return ser.Deserialize(stream, true);
            }
        }

        public string Print()
        {
            return string.Format("Command:{0},Priority:{1},Identifier:{2},Creation:{3},TransformType:{4},Host:{5}",
            //MessageType,
            Command,
            Priority,
            Identifier,
            Creation,
            TransformType,
            Host);
        }
        ///// <summary>
        ///// Get or Set The serializer formatter.
        ///// </summary>
        //public Formatters Formatter { get; set; }

        ///// <summary>
        ///// Get or Set QueueName
        ///// </summary>
        //public string QueueName { get; set; }

        ///// <summary>
        ///// Get or Set HostName
        ///// </summary>
        //public string HostName { get; set; }
        ///// <summary>
        ///// Get or Set ServerName
        ///// </summary>
        //public string ServerName { get; set; }

        ///// <summary>
        ///// Get or Set Port
        ///// </summary>
        //public int Port { get; set; }
        ///// <summary>
        ///// Get or Set Protocol
        ///// </summary>
        //public NetProtocol Protocol { get; set; }
        ///// <summary>
        ///// Get or Set HostAddress
        ///// </summary>
        //public string HostAddress { get; set; }

        //string _OriginalHostAddress;
        //public string OriginalHostAddress
        //{
        //    get { return _OriginalHostAddress; }
        //}




        #endregion

        #region ITransformMessage
        /*
        /// <summary>
        /// Get or Set DuplexTypes
        /// </summary>
        public DuplexTypes DuplexType { get; set; }
        /// <summary>
        /// Get or Set TransformType
        /// </summary>
        public TransformType TransformType { get; set; }
        */
        #endregion

        #region ctor

        public QueueRequest() : base()
        {
            //Creation = DateTime.Now;
            Version = QueueDefaults.CurrentVersion;
            TransformType = TransformType.Object;
            //Identifier = Ptr.NewIdentifier();
            //DuplexType = DuplexTypes.None;
            //EnsureDuplex();
        }
        
        public QueueRequest(QueueCmd cmd) : this()
        {
            Command = cmd.ToString();
        }
        public QueueRequest(byte[] body, Type type) : this()
        {
            SetBody(body, type);
        }
        public QueueRequest(NetStream body, Type type) : this()
        {
            SetBody(body, type);
        }

        public QueueRequest(Stream stream)
        {
            EntityRead(stream, null);
        }

        public QueueRequest(NameValueCollection QueryString) : base(QueryString.Get("Identifier"))
        {
            //Header = new QHeader(QueryString);
            Version = QueryString.Get<int>("Version");
            //var MessageState = (MessageState)QueryString.Get<byte>("MessageState");
            Priority = EnumExtension.ParseOrCast<Priority>(QueryString.Get("Priority"), Priority.Normal);// (Priority)QueryString.Get<byte>("Priority");
            //Identifier = QueryString.Get("Identifier");//.ReadValue<Guid>();
            //var Retry = QueryString.Get<byte>("Retry");
            //var ArrivedTime = QueryString.Get<DateTime>("ArrivedTime");
            Creation = DateTime.Now;// QueryString.Get<DateTime>("Creation");
            //Modified = DateTime.Now;//QueryString.Get<DateTime>("Modified");
            //var Duration = QueryString.Get<int>("Duration");

            Expiration = QueryString.Get<int>("Expiration");
            Label = QueryString.Get("Label");
            Source = QueryString.Get("Source");
            //mqh-Sender = QueryString.Get("Sender");
            //BodyStream = (NetStream)Get.ReadValue();
            SetBody(QueryString.Get("Body"));
            TypeName = typeof(string).GetType().FullName;// QueryString.Get("TypeName");

        }

        #endregion

        #region Dispose
        /*
        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        bool disposed = false;
        /// <summary>
        /// Get indicate wether the current instance is Disposed.
        /// </summary>
        internal bool IsDisposed
        {
            get { return disposed; }
        }
        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        internal void Dispose(bool disposing)
        {
            if (!disposed)
            {
                //Command = null;
                
                Host = null;
            }
            disposed = true;
        }
        */
        #endregion

        #region  ISerialEntity


        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public override void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteValue(Version);
            //mqh-streamer.WriteValue((byte)MessageType);
            //mqh-streamer.WriteValue((byte)QCommand);
            streamer.WriteValue((byte)Priority);
            streamer.WriteString(Host);
            base.EntityWrite(stream, streamer);


            //streamer.WriteValue(ToBinary());
            /*
            streamer.WriteValue(Version);
            streamer.WriteValue((byte)MessageState.Sending);
            streamer.WriteValue((byte)MessageType);
            streamer.WriteValue((byte)QCommand);
            streamer.WriteValue((byte)Priority);
            streamer.WriteString(Identifier);//.WriteValue(ItemId);
            //mqh-streamer.WriteValue((byte)0);//Retry
            //mqh-streamer.WriteValue(Modified);// ArrivedTime);
            streamer.WriteValue(Creation);
            //mqh-streamer.WriteValue(Modified);
            //mqh-streamer.WriteValue((int)0);// Duration);
            streamer.WriteValue((byte)TransformType);
            streamer.WriteValue((byte)DuplexType);
            //mqh-streamer.WriteValue(Expiration);
            streamer.WriteString(Host);
            streamer.WriteString(Label);
            streamer.WriteString(Source);
            streamer.WriteValue(BodyStream);
            streamer.WriteString(TypeName);
            */

            /*
            streamer.WriteValue(Version);
            streamer.WriteValue((byte)MessageType);
            streamer.WriteValue((byte)QCommand);
            streamer.WriteValue((byte)Priority);
            streamer.WriteString(Identifier);//.WriteValue(ItemId);
            streamer.WriteValue(Creation);
            streamer.WriteValue((byte)TransformType);
            streamer.WriteValue((byte)DuplexType);
            streamer.WriteValue(Expiration);

            streamer.WriteString(Host);
            //streamer.WriteString(Sender);

            streamer.WriteValue(BodyStream);
            //streamer.WriteString(TypeName);
            */
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public override void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            Version = streamer.ReadValue<int>();
            //mqh-MessageType = (MQTypes)streamer.ReadValue<byte>();
            //mqh-QCommand = (QueueCmd)streamer.ReadValue<byte>();
            Priority = (Priority)streamer.ReadValue<byte>();
            Host = streamer.ReadString();
            base.EntityRead(stream, streamer);

            /*
            Version = streamer.ReadValue<int>();
            var MessageState = (MessageState)streamer.ReadValue<byte>();
            var MessageType = (MQTypes)streamer.ReadValue<byte>();
            QCommand = (QueueCmd)streamer.ReadValue<byte>();
            Priority = (Priority)streamer.ReadValue<byte>();
            Identifier = streamer.ReadString();//.ReadValue<Guid>();
            //mqh-var Retry = streamer.ReadValue<byte>();
            //mqh-var ArrivedTime = streamer.ReadValue<DateTime>();
            Creation = streamer.ReadValue<DateTime>();
            //mqh-Modified = streamer.ReadValue<DateTime>();
            //mqh-var Duration = streamer.ReadValue<int>();

            TransformType = (TransformType)streamer.ReadValue<byte>();
            DuplexType = (DuplexTypes)streamer.ReadValue<byte>();
            //mqh-Expiration = streamer.ReadValue<int>();
            Host = streamer.ReadString();
            Label = streamer.ReadString();
            Source = streamer.ReadString();
            BodyStream = (NetStream)streamer.ReadValue();
            TypeName = streamer.ReadString();
            */

            /*
            Version = streamer.ReadValue<int>();
            var MessageType = (MQTypes)streamer.ReadValue<byte>();
            QCommand = (QueueCmd)streamer.ReadValue<byte>();
            Priority = (Priority)streamer.ReadValue<byte>();
            Identifier = streamer.ReadString();//.ReadValue<Guid>();
            Creation = streamer.ReadValue<DateTime>();
            //TransformType = (TransformTypes)streamer.ReadValue<byte>();
            TransformType = (TransformType)streamer.ReadValue<byte>();
            DuplexType = (DuplexTypes)streamer.ReadValue<byte>();
            Expiration = streamer.ReadValue<int>();

            Host = streamer.ReadString();
            //Sender = streamer.ReadString();
            BodyStream = (NetStream)streamer.ReadValue();
            //_TypeName = streamer.ReadString();
            */
        }


        /*
        /// <summary>
        /// Write the current object include the body and properties to <see cref="ISerializerContext"/> using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        public void WriteContext(ISerializerContext context)
        {
            SerializeInfo info = new SerializeInfo();

            info.Add("Command", (byte)Command);
            info.Add("Priority", (byte)Priority);
            info.Add("Host", Host);
            info.Add("TransformType", (byte)TransformType);
            info.Add("Creation", Creation);
            context.WriteSerializeInfo(info);
        }

        
         /// <summary>
        /// Read <see cref="ISerializerContext"/> context to the current object include the body and properties using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        public void ReadContext(ISerializerContext context)
        {
            SerializeInfo info = context.ReadSerializeInfo();
            ReadContext(info);
        }

        /// <summary>
        /// Read <see cref="SerializeInfo"/> context to the current object include the body and properties using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        void ReadContext(SerializeInfo info)
        {

            Command = (QueueCmd)info.GetValue<byte>("Command");
            Priority = (Priority)info.GetValue<byte>("Priority");
            Host = info.GetValue<string>("Host");
            TransformType =(TransformTypes) info.GetValue<byte>("TransformType");
            Creation = info.GetValue<DateTime>("Creation");
        }
        */
        #endregion

        #region Convert

        /// <summary>
        /// Deserialize body stream to object, This method is a part of <see cref="IMessageStream"/> implementation.
        /// </summary>
        /// <returns></returns>
        public object DecodeBody()
        {
            if (_Body == null)
                return null;
            //BodyStream.Position = 0;
            using (var stream = BodyStream())
            {
                var ser = new BinarySerializer();
                return ser.Deserialize(stream, true);
            }
        }
        /// <summary>
        ///  Deserialize body stream to generic object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T DecodeBody<T>()
        {
            return GenericTypes.Cast<T>(DecodeBody(), true);
        }

        /// <summary>
        /// Convert body to string.
        /// </summary>
        /// <returns></returns>
        public string BodyToString()
        {
            if (_Body == null)
                return null;
            var body = DecodeBody();
            if (body == null)
                return null;
            return body.ToString();
            //return System.Text.Encoding.GetEncoding(Types.NZorEmpty(EncodingName, DefaultEncoding)).GetString(BodyStream.ToArray());
        }

        /// <summary>
        /// Convert body to json string.
        /// </summary>
        /// <returns></returns>
        public string BodyToJson<T>()
        {
            if (_Body == null)
                return null;
            T body = DecodeBody<T>();
            return JsonSerializer.Serialize(body);
        }

        /// <summary>
        /// Convert body to base 64 string.
        /// </summary>
        /// <returns></returns>
        public string BodyToBase64()
        {
            if (_Body == null)
                return null;
            return BinarySerializer.ToBase64(this._Body);
        }
        /// <summary>
        /// Convert from base 64 string to generic object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static T ConvertFromBase64<T>(string base64)
        {
            return BinarySerializer.DeserializeFromBase64<T>(base64);
        }

        #endregion

        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }

        public byte[] ToBinary()
        {

            byte[] binary = null;

            using (NetStream s = new NetStream())
            {
                using (var strmer = new BinaryStreamer(s))
                {
                    strmer.WriteValue(Version);
                    //mqh-streamer.WriteValue((byte)MessageType);
                    //mqh-streamer.WriteValue((byte)QCommand);
                    strmer.WriteValue((byte)Priority);
                    strmer.WriteString(Host);
                    base.EntityWrite(s, strmer);

                    //strmer.WriteValue(Version);
                    //strmer.WriteValue((byte)MessageType);
                    //strmer.WriteValue((byte)QCommand);
                    //strmer.WriteValue((byte)Priority);
                    //strmer.WriteString(Identifier);//.WriteValue(ItemId);
                    //strmer.WriteValue(Creation);
                    //strmer.WriteValue((byte)TransformType);
                    //strmer.WriteValue((byte)DuplexType);
                    //strmer.WriteValue(Expiration);
                    //strmer.WriteString(Host);
                    ////strmer.WriteString(Sender);
                    //strmer.WriteValue(BodyStream);
                    ////strmer.WriteString(TypeName);

                    /*
                    strmer.WriteValue(Version);
                    strmer.WriteValue((byte)MessageState.Sending);
                    strmer.WriteValue((byte)MessageType);
                    strmer.WriteValue((byte)QCommand);
                    strmer.WriteValue((byte)Priority);
                    strmer.WriteString(Identifier);//.WriteValue(ItemId);
                    //mqh-strmer.WriteValue((byte)0);//Retry
                    //mqh-strmer.WriteValue(Modified);// ArrivedTime);
                    strmer.WriteValue(Creation);
                    //mqh-strmer.WriteValue(Modified);
                    //mqh-strmer.WriteValue((int)0);// Duration);
                    strmer.WriteValue((byte)TransformType);
                    strmer.WriteValue((byte)DuplexType);
                    //mqh-strmer.WriteValue(Expiration);
                    strmer.WriteString(Host);
                    strmer.WriteString(Label);
                    strmer.WriteString(Source);
                    strmer.WriteValue(BodyStream);
                    strmer.WriteString(TypeName);
                    */

                    binary = s.ToArray();
                }
            }
            return binary;
        }

        public TransStream ToTransStream()
        {
            TransStream stream = new TransStream(this);
            return stream;
        }

        public Ptr GetPtr()
        {
            return new Ptr(Identifier, Host);
        }

        public Ptr GetPtr(string hotName)
        {
            return new Ptr(Identifier, hotName);
        }
    }


#if (false)
    /// <summary>
    /// Represent a message stream for network communication like namedPipe or Tcp.
    /// This message can serialize/desrialize fast and easly using the <see cref="BinaryStreamer"/>
    /// </summary>
    [Serializable]
    public sealed class QueueRequest : IQueueRequest, IDisposable
    {
    

    #region properties MessgaeStream
        /// <summary>
        /// Get the default formatter.
        /// </summary>
        public static Formatters DefaultFormatter { get { return Formatters.BinarySerializer; } }
        /// <summary>
        /// DefaultEncoding utf-8
        /// </summary>
        public const string DefaultEncoding = "utf-8";
        ///// <summary>
        ///// Get or Set The message Id.
        ///// </summary>
        //public Guid ItemId { get; protected set; }
        /// <summary>
        /// Get or Set The message Id.
        /// </summary>
        public string Identifier { get; private set; }
        /// <summary>
        /// Get or Set The message body stream.
        /// </summary>
        //NetStream _BodyStream;
        public NetStream BodyStream { get; set; }// { protected get { return _BodyStream == null ? null : _BodyStream.Ready(); } public set { _BodyStream = value; } }
        /// <summary>
        ///  Get or Set The type name of body stream.
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// Get or Set The serializer formatter.
        /// </summary>
        public Formatters Formatter { get; set; }
        /// <summary>
        /// Get or Set The message detail.
        /// </summary>
        public string Label { get; set; }
  
        /// <summary>
        /// Get or Set who send the message.
        /// </summary>
        public string Source { get; set; }
 
        /// <summary>
        /// Get or Set The message CustomId.
        /// </summary>
        public string CustomId { get; set; }
        /// <summary>
        /// Get or Set The message SessionId.
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// Get or set The message encoding, Default=utf-8.
        /// </summary>
        public string EncodingName { get; set; }

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
                    _DuplexType = DuplexTypes.WaitOne;
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

        public TransformType TransformType { get; set; }

        //public StringFormatType FormatType { get { return (StringFormatType)(int)TransformType; } }
    #endregion

    #region IBodyFormatter extend

        /// <summary>
        /// Set the given value to body stream using <see cref="BinarySerializer"/>, This method is a part of <see cref="IMessageStream"/> implementation..
        /// </summary>
        /// <param name="value"></param>
        public void SetBody(object value)
        {

            if (value != null)
            {
                TypeName = value.GetType().FullName;

                using (NetStream ns = new NetStream())
                {
                    var ser = new BinarySerializer();
                    ser.Serialize(ns, value);
                    ns.Position = 0;
                    BodyStream = ns;
                }
            }
            else
            {
                TypeName = typeof(object).FullName;
                BodyStream = null;
            }
        }

        /// <summary>
        /// Set the given byte array to body stream.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="typeName"></param>
        public void SetBody(byte[] value, string typeName)
        {
            TypeName = (!string.IsNullOrEmpty(typeName)) ? typeName : typeof(object).FullName;
            if (value != null)
            {
                BodyStream = new NetStream(value);
            }
        }
        /// <summary>
        /// Set the given stream to body stream.
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="typeName"></param>
        /// <param name="copy"></param>
        public void SetBody(NetStream ns, string typeName, bool copy = true)
        {
            TypeName = (!string.IsNullOrEmpty(typeName)) ? typeName : typeof(object).FullName;
            if (ns != null)
            {
                if (copy)
                    ns.CopyTo(BodyStream);
                else
                    BodyStream = ns;
            }
        }

    #endregion

    #region property MessgaeStream

        public int Version { get; internal set; }

        /// <summary>
        /// Get Creation Time
        /// </summary>
        public DateTime Creation { get; internal set; }
        ///// <summary>
        ///// Get Command
        ///// </summary>
        //public QueueCmd QCommand { get; internal set; }

        QueueCmd _QCommand;
        /// <summary>
        /// Get Command
        /// </summary>
        public QueueCmd QCommand
        {
            get { return _QCommand; }
            set
            {
                _QCommand = value;
                //Command = value.ToString();
            }
        }

        /// <summary>
        /// Get Priority
        /// </summary>
        public Priority Priority { get; set; }

        /// <summary>
        /// Get The message Host\Queue name.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Get or Set message type.
        /// </summary>
        public MQTypes MessageType { get { return MQTypes.MessageRequest; } }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }

        /// <summary>
        /// Deserialize body stream to object.
        /// </summary>
        /// <returns></returns>
        public object GetBody()
        {
            if (BodyStream == null)
                return null;
            BodyStream.Position = 0;
            var ser = new BinarySerializer();
            return ser.Deserialize(BodyStream, true);
        }

        public string Print()
        {
            return string.Format("MessageType:{0},Command:{1},Priority:{2},Identifier:{3},Creation:{4},TransformType:{5},Host:{6}",
            MessageType,
            QCommand,
            Priority,
            Identifier,
            Creation,
            TransformType,
            Host);
        }
        ///// <summary>
        ///// Get or Set The serializer formatter.
        ///// </summary>
        //public Formatters Formatter { get; set; }

        ///// <summary>
        ///// Get or Set QueueName
        ///// </summary>
        //public string QueueName { get; set; }

        ///// <summary>
        ///// Get or Set HostName
        ///// </summary>
        //public string HostName { get; set; }
        ///// <summary>
        ///// Get or Set ServerName
        ///// </summary>
        //public string ServerName { get; set; }

        ///// <summary>
        ///// Get or Set Port
        ///// </summary>
        //public int Port { get; set; }
        ///// <summary>
        ///// Get or Set Protocol
        ///// </summary>
        //public NetProtocol Protocol { get; set; }
        ///// <summary>
        ///// Get or Set HostAddress
        ///// </summary>
        //public string HostAddress { get; set; }

        //string _OriginalHostAddress;
        //public string OriginalHostAddress
        //{
        //    get { return _OriginalHostAddress; }
        //}




    #endregion

    #region ctor

        public QueueRequest()
        {
            Creation = DateTime.Now;
            Version = QueueDefaults.CurrentVersion;
            TransformType = TransformType.Object;
            Identifier = Ptr.NewIdentifier();
            //DuplexType = DuplexTypes.None;
            //EnsureDuplex();
        }

        public QueueRequest(Stream stream)
        {
            EntityRead(stream, null);
        }

        public QueueRequest(NameValueCollection QueryString)
        {
            Version = QueryString.Get<int>("Version");
            //var MessageState = (MessageState)QueryString.Get<byte>("MessageState");
            //var MessageType = (MQTypes)QueryString.Get<byte>("MessageType");
            QCommand = EnumExtension.ParseOrCast<QueueCmd>(QueryString.Get("QCommand"), QueueCmd.None);// (QueueCmd)QueryString.Get<byte>("QCommand");
            Priority = EnumExtension.ParseOrCast<Priority>(QueryString.Get("Priority"), Priority.Normal);// (Priority)QueryString.Get<byte>("Priority");
            Identifier = QueryString.Get("Identifier");//.ReadValue<Guid>();
            //var Retry = QueryString.Get<byte>("Retry");
            //var ArrivedTime = QueryString.Get<DateTime>("ArrivedTime");
            Creation = DateTime.Now;// QueryString.Get<DateTime>("Creation");
            //Modified = DateTime.Now;//QueryString.Get<DateTime>("Modified");
            //var Duration = QueryString.Get<int>("Duration");

            TransformType = EnumExtension.ParseOrCast<TransformType>(QueryString.Get("TransformType"), TransformType.None);// (TransformType)QueryString.Get<byte>("TransformType");
            DuplexType = EnumExtension.ParseOrCast<DuplexTypes>(QueryString.Get("DuplexType"), DuplexTypes.WaitOne);// (DuplexTypes)QueryString.Get<byte>("DuplexType");
            Expiration = QueryString.Get<int>("Expiration");
            Host = QueryString.Get("Host");
            Label = QueryString.Get("Label");
            Source = QueryString.Get("Source");
            //BodyStream = (NetStream)Get.ReadValue();
            SetBody(QueryString.Get("Body"));
            TypeName = typeof(string).GetType().FullName;// QueryString.Get("TypeName");

        }

    #endregion

    #region Dispose
        
        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        bool disposed = false;
        /// <summary>
        /// Get indicate wether the current instance is Disposed.
        /// </summary>
        internal bool IsDisposed
        {
            get { return disposed; }
        }
        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        internal void Dispose(bool disposing)
        {
            if (!disposed)
            {
                //Command = null;
                
                Host = null;
            }
            disposed = true;
        }
        
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


            //streamer.WriteValue(ToBinary());

            streamer.WriteValue(Version);
            streamer.WriteValue((byte)MessageState.Sending);
            streamer.WriteValue((byte)MessageType);
            streamer.WriteValue((byte)QCommand);
            streamer.WriteValue((byte)Priority);
            streamer.WriteString(Identifier);//.WriteValue(ItemId);
            //streamer.WriteValue((byte)0);//Retry
            //streamer.WriteValue(ArrivedTime);// ArrivedTime);
            streamer.WriteValue(Creation);
            //streamer.WriteValue(Modified);
            //streamer.WriteValue((int)0);// Duration);
            streamer.WriteValue((byte)TransformType);
            streamer.WriteValue((byte)DuplexType);
            streamer.WriteValue(Expiration);
            streamer.WriteString(Host);
            streamer.WriteString(Label);
            streamer.WriteString(Source);
            streamer.WriteValue(BodyStream);
            streamer.WriteString(TypeName);


            /*
            streamer.WriteValue(Version);
            streamer.WriteValue((byte)MessageType);
            streamer.WriteValue((byte)QCommand);
            streamer.WriteValue((byte)Priority);
            streamer.WriteString(Identifier);//.WriteValue(ItemId);
            streamer.WriteValue(Creation);
            streamer.WriteValue((byte)TransformType);
            streamer.WriteValue((byte)DuplexType);
            streamer.WriteValue(Expiration);

            streamer.WriteString(Host);
            //streamer.WriteString(Sender);

            streamer.WriteValue(BodyStream);
            //streamer.WriteString(TypeName);
            */
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


            Version = streamer.ReadValue<int>();
            var MessageState = (MessageState)streamer.ReadValue<byte>();
            var MessageType = (MQTypes)streamer.ReadValue<byte>();
            QCommand = (QueueCmd)streamer.ReadValue<byte>();
            Priority = (Priority)streamer.ReadValue<byte>();
            Identifier = streamer.ReadString();//.ReadValue<Guid>();
            //var Retry = streamer.ReadValue<byte>();
            //var ArrivedTime = streamer.ReadValue<DateTime>();
            Creation = streamer.ReadValue<DateTime>();
            //Modified = streamer.ReadValue<DateTime>();
            //var Duration = streamer.ReadValue<int>();

            TransformType = (TransformType)streamer.ReadValue<byte>();
            DuplexType = (DuplexTypes)streamer.ReadValue<byte>();
            Expiration = streamer.ReadValue<int>();
            Host = streamer.ReadString();
            Label = streamer.ReadString();
            Source = streamer.ReadString();
            BodyStream = (NetStream)streamer.ReadValue();
            TypeName = streamer.ReadString();


            /*
            Version = streamer.ReadValue<int>();
            var MessageType = (MQTypes)streamer.ReadValue<byte>();
            QCommand = (QueueCmd)streamer.ReadValue<byte>();
            Priority = (Priority)streamer.ReadValue<byte>();
            Identifier = streamer.ReadString();//.ReadValue<Guid>();
            Creation = streamer.ReadValue<DateTime>();
            //TransformType = (TransformTypes)streamer.ReadValue<byte>();
            TransformType = (TransformType)streamer.ReadValue<byte>();
            DuplexType = (DuplexTypes)streamer.ReadValue<byte>();
            Expiration = streamer.ReadValue<int>();

            Host = streamer.ReadString();
            //Sender = streamer.ReadString();
            BodyStream = (NetStream)streamer.ReadValue();
            //_TypeName = streamer.ReadString();
            */
        }


        /*
        /// <summary>
        /// Write the current object include the body and properties to <see cref="ISerializerContext"/> using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        public void WriteContext(ISerializerContext context)
        {
            SerializeInfo info = new SerializeInfo();

            info.Add("Command", (byte)Command);
            info.Add("Priority", (byte)Priority);
            info.Add("Host", Host);
            info.Add("TransformType", (byte)TransformType);
            info.Add("Creation", Creation);
            context.WriteSerializeInfo(info);
        }

        
         /// <summary>
        /// Read <see cref="ISerializerContext"/> context to the current object include the body and properties using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        public void ReadContext(ISerializerContext context)
        {
            SerializeInfo info = context.ReadSerializeInfo();
            ReadContext(info);
        }

        /// <summary>
        /// Read <see cref="SerializeInfo"/> context to the current object include the body and properties using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        void ReadContext(SerializeInfo info)
        {

            Command = (QueueCmd)info.GetValue<byte>("Command");
            Priority = (Priority)info.GetValue<byte>("Priority");
            Host = info.GetValue<string>("Host");
            TransformType =(TransformTypes) info.GetValue<byte>("TransformType");
            Creation = info.GetValue<DateTime>("Creation");
        }
        */
    #endregion

    #region Convert

        /// <summary>
        /// Deserialize body stream to object, This method is a part of <see cref="IMessageStream"/> implementation.
        /// </summary>
        /// <returns></returns>
        public object DecodeBody()
        {
            if (BodyStream == null)
                return null;
            //BodyStream.Position = 0;
            var ser = new BinarySerializer();
            return ser.Deserialize(BodyStream);
        }
        /// <summary>
        ///  Deserialize body stream to generic object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T DecodeBody<T>()
        {
            return GenericTypes.Cast<T>(DecodeBody(), true);
        }

        /// <summary>
        /// Convert body to string.
        /// </summary>
        /// <returns></returns>
        public string BodyToString()
        {
            if (BodyStream == null)
                return null;
            var body = DecodeBody();
            if (body == null)
                return null;
            return body.ToString();
            //return System.Text.Encoding.GetEncoding(Types.NZorEmpty(EncodingName, DefaultEncoding)).GetString(BodyStream.ToArray());
        }

        /// <summary>
        /// Convert body to json string.
        /// </summary>
        /// <returns></returns>
        public string BodyToJson<T>()
        {
            if (BodyStream == null)
                return null;
            T body = DecodeBody<T>();
            return JsonSerializer.Serialize(body);
        }

        /// <summary>
        /// Convert body to base 64 string.
        /// </summary>
        /// <returns></returns>
        public string BodyToBase64()
        {
            if (BodyStream == null)
                return null;
            return BinarySerializer.SerializeToBase64(this.BodyStream.ToArray());
        }
        /// <summary>
        /// Convert from base 64 string to generic object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static T ConvertFromBase64<T>(string base64)
        {
            return BinarySerializer.DeserializeFromBase64<T>(base64);
        }

    #endregion

        public byte[] ToBinary()
        {

            byte[] binary = null;

            using (NetStream s = new NetStream())
            {
                using (var strmer = new BinaryStreamer(s))
                {
                    //strmer.WriteValue(Version);
                    //strmer.WriteValue((byte)MessageType);
                    //strmer.WriteValue((byte)QCommand);
                    //strmer.WriteValue((byte)Priority);
                    //strmer.WriteString(Identifier);//.WriteValue(ItemId);
                    //strmer.WriteValue(Creation);
                    //strmer.WriteValue((byte)TransformType);
                    //strmer.WriteValue((byte)DuplexType);
                    //strmer.WriteValue(Expiration);
                    //strmer.WriteString(Host);
                    ////strmer.WriteString(Sender);
                    //strmer.WriteValue(BodyStream);
                    ////strmer.WriteString(TypeName);

                    strmer.WriteValue(Version);
                    strmer.WriteValue((byte)MessageState.Sending);
                    strmer.WriteValue((byte)MessageType);
                    strmer.WriteValue((byte)QCommand);
                    strmer.WriteValue((byte)Priority);
                    strmer.WriteString(Identifier);//.WriteValue(ItemId);
                    strmer.WriteValue((byte)0);//Retry
                    //strmer.WriteValue(Modified);// ArrivedTime);
                    strmer.WriteValue(Creation);
                    //strmer.WriteValue(Modified);
                    //strmer.WriteValue((int)0);// Duration);
                    strmer.WriteValue((byte)TransformType);
                    strmer.WriteValue((byte)DuplexType);
                    strmer.WriteValue(Expiration);
                    strmer.WriteString(Host);
                    strmer.WriteString(Label);
                    strmer.WriteString(Source);
                    strmer.WriteValue(BodyStream);
                    strmer.WriteString(TypeName);


                    binary = s.ToArray();
                }
            }
            return binary;
        }

        public TransStream ToTransStream()
        {
            TransStream stream = new TransStream(this);
            return stream;
        }

        public Ptr GetPtr()
        {
            return new Ptr(Identifier, 0, Host);
        }

        public Ptr GetPtr(string hotName)
        {
            return new Ptr(Identifier, 0, hotName);
        }
    }



#endif
}
