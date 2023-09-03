using Nistec.Channels;
using Nistec.Channels.Http;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Nistec.Messaging
{
    public class MessageQueue : IMessageQueue, ISerialEntity, ISerialJson, ICloneable, IDisposable//, IQueueAck//, ISerialEntity
    {
        #region static
        public static QueueMessage Create(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("QueueMessage.Load.stream");
            }
            if (stream is NetStream)
            {
                stream.Position = 0;
            }
            var msg = new QueueMessage();
            msg.EntityRead(stream, null);
            msg.Creation = DateTime.Now;
            return msg;
        }

        public static QueueMessage Create(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("QueueMessage.Load.XmlNode");
            }
            var msg = new QueueMessage();
            msg.Load(node);
            msg.Creation = DateTime.Now;
            return msg;
        }

        public static QueueMessage Create(DataRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("QueueMessage.Load.DataRow");
            }
            var dic = Nistec.Data.DataUtil.DataRowToHashtable(row);
            var msg = new QueueMessage();
            msg.Load(dic);
            msg.Creation = DateTime.Now;
            return msg;
        }

        public static QueueMessage Create(IDictionary dic)
        {
            if (dic == null)
            {
                throw new ArgumentNullException("QueueMessage.Load.IDictionary");
            }
            var msg = new QueueMessage();
            msg.Load(dic);
            msg.Creation = DateTime.Now;
            return msg;
        }

        public static QueueMessage Create(NameValueCollection nvc)
        {
            if (nvc == null)
            {
                throw new ArgumentNullException("QueueMessage.Load.NameValueCollection");
            }
            var msg = new QueueMessage();
            msg.Load(nvc);
            msg.Creation = DateTime.Now;
            return msg;
        }
        public static QueueMessage Create(GenericRecord rcd)
        {
            if (rcd == null)
            {
                throw new ArgumentNullException("QueueMessage.Load.GenericRecord");
            }
            var msg = new QueueMessage();
            msg.Load(rcd);
            msg.Creation = DateTime.Now;
            return msg;
        }
        public static QueueMessage Ack(MessageState state, QueueCmd cmd)
        {
            return new QueueMessage()
            {
                QCommand = cmd,
                Label = state.ToString(),
                MessageState = state
            };
        }

        public static QueueMessage Ack(MessageState state, QueueCmd cmd, Exception ex)
        {
            return new QueueMessage()
            {
                QCommand = cmd,
                Label = ex == null ? state.ToString() : ex.Message,
                MessageState = state
            };
        }

        public static QueueMessage Ack(MessageState state, QueueCmd cmd, string label, string identifier)
        {
            return new QueueMessage(identifier)
            {
                QCommand = cmd,
                Label = label,
                //Identifier = identifier,
                MessageState = state
            };
        }

        public static QueueMessage Ack(QueueMessage item, MessageState state, int retry, string label, string identifier)
        {
            return new QueueMessage(identifier)
            {
                ArrivedTime = DateTime.Now,
                MessageState = state,
                Command = item.Command,
                Host = item.Host,
                //Identifier = identifier,
                Creation = DateTime.Now,
                //Creation = item.Creation,
                Priority = item.Priority,
                MessageType = item.MessageType,
                Source = item.Source,
                TransformType = item.TransformType,
                Retry = (byte)retry,
                Label = label
            };
        }


        #endregion

        #region Load

        void ClearData()
        {

            Body = null;
            TypeName = null;
        }

        /// <summary>
        /// Serializes the message body using the specified System.Xml.XmlWriter.
        /// </summary>
        /// <param name="wr"></param>
        public void WriteBody(XmlTextWriter wr)
        {

            object obj = ReadBody();
            if (obj != null)
            {
                wr.WriteStartElement("Body");
                wr.WriteRaw("\r\n");
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(wr, obj);
                wr.WriteEndElement();
            }
            else
            {
                wr.WriteRaw("<Body/>");
            }
            wr.WriteRaw("\r\n");
        }


        /// <summary>
        /// Serializes the message body using the specified System.Xml.XmlWriter.
        /// </summary>
        /// <param name="wr"></param>
        public void MessageRead(XmlTextReader xr)
        {
            while (xr.Read())
            {
                if (xr.IsStartElement())
                {

                    switch (xr.Name)
                    {
                        case "Body":
                        case "BodyStream":
                            ReadBody(xr.ReadSubtree()); break;
                        case "Header":
                        case "Headers":
                            ReadHeaders(xr.ReadSubtree()); break;
                        default:
                            SetInnerValue(xr.Name, xr.ReadString()); break;
                    }

                }
            }
        }

        void ReadBody(XmlReader xr)
        {
            while (xr.Read())
            {
                if (xr.IsStartElement())
                {
                    //Headers.Add(xr.Name, xr.Value);
                }
            }
        }

        void ReadHeaders(XmlReader xr)
        {
            while (xr.Read())
            {
                if (xr.IsStartElement())
                {
                    //Headers.Add(xr.Name, xr.Value);
                }
            }
        }
        internal void Load(NameValueCollection nvc)
        {
            if (nvc == null)
            {
                throw new ArgumentNullException("Message.Load.NameValueCollection");
            }
            ClearData();

            foreach (string Key in nvc.Keys)
            {
                SetInnerValue(Key, nvc.Get(Key));
            }
        }

        internal void Load(IDictionary dic)
        {
            if (dic == null)
            {
                throw new ArgumentNullException("Message.Load.IDictionary");
            }
            ClearData();

            foreach (DictionaryEntry n in dic)
            {
                SetInnerValue(n.Key.ToString(), n.Value);
            }
        }

        internal void Load(XmlNode node)
        {

            try
            {
                if (node == null)
                {
                    throw new ArgumentNullException("Message.Load.XmlNode");
                }
                ClearData();

                XmlNodeList list = node.ChildNodes;
                if (list == null)
                    return;

                foreach (XmlNode n in list)
                {
                    SetValue(n);
                }


            }
            catch (Exception ex)
            {
                throw new Exception("LoadXml node error " + ex.Message);
            }

        }


        void SetValue(XmlNode node)
        {

            if (node.HasChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    switch (node.Name)
                    {
                        case "BodyStream":
                            //SetBody(value); 
                            break;
                        case "HeaderStream":
                            //_Headers = (GenericNameValue)value; 
                            break;
                    }
                }
            }
            else
            {
                SetInnerValue(node.Name, node.InnerText);
            }
        }
        
        /// <summary>
        /// Serializes the message body using the specified System.Xml.XmlWriter.
        /// </summary>
        /// <param name="wr"></param>
        void SetInnerValue(string name, object value)
        {

            switch (name)
            {
                case "MessageState":
                    MessageState = (MessageState)Types.ToByte(value, 0); break;
                case "Command":
                    Header.QCommand = (QueueCmd)Types.ToByte(value, 0); break;
                case "Priority":
                    Priority = (Priority)Types.ToByte(value, 2); break;
                //case "ItemId":
                //    ItemId = Types.ToGuid(value); break;
                case "Retry":
                    Retry = Types.ToByte(value, 0); break;
                case "ArrivedTime":
                    ArrivedTime = Types.ToDateTime(value); break;
                //case "SentTime":
                //    SentTime = Types.ToDateTime(value); break;
                //case "Modified":
                //    Modified = Types.ToDateTime(value); break;
                case "Duration":
                    Duration = Types.ToInt(value); break;
                //case "MessageId":
                //    MessageId = Types.ToInt(value); break;
                //case "TransformType":
                //    TransformType = (TransformTypes)Types.ToByte(value, 0); break;
                case "TransformType":
                    Header.TransformType = (TransformType)Types.ToByte(value, 0); break;
                case "Expiration":
                    Header.Expiration = Types.ToInt(value, 0); break;
                case "IsDuplex":
                    Header.IsDuplex = Types.ToBool(value, false); break;
                case "Host":
                    Header.Host = Types.NZ(value, ""); break;
                case "Label":
                    Label = Types.NZ(value, ""); break;
                case "Sender":
                    Source = Types.NZ(value, ""); break;
                //case "Topic":
                //    Topic = Types.NZ(value, ""); break;
                //case "HeaderStream":
                //    SetHeader((GenericNameValue)value);
                //    break;
                //case "Formatter":
                //    Formatter = (Formatters)Types.ToInt(value); break;
                //case "BodyStream":
                //    SetBodyInternal(value, _TypeName);
                //    break;
                case "TypeName":
                    TypeName = Types.NZ(value, ""); break;
                //case "EncodingName":
                //    EncodingName = Types.NZ(value, DefaultEncoding); break;
                    //case "Segments":
                    //    Segments = Types.ToByte(value, 0); break;
                    //case "HostType":
                    //    HostType = Types.ToByte(value, 0); break;
                    //case "Notify":
                    //    Notify = Types.NZ(value, ""); break;
                    //case "Command":
                    //    Command = Types.NZ(value,""); break;
                    //case "IsDuplex":
                    //    IsDuplex = Types.ToBool(value, false); break;
                    //case "Headers":
                    // _Headers = (GenericNameValue)value; 
                    //break;
            }
            
        }

        #endregion

        #region ICloneable

        public MessageQueue Copy()
        {
            //var copy = new MessageQueue()
            //{
            //    Header=new MessageHeader(Header),
            //    MessageState = this.MessageState,
            //    MessageType = this.MessageType,
            //    Command = this.Command,
            //    Priority = this.Priority,
            //    //Identifier = this.Identifier,
            //    Retry = this.Retry,
            //    ArrivedTime = this.ArrivedTime,
            //    Creation = this.Creation,
            //    Modified = this.Modified,
            //    TransformType = this.TransformType,
            //    Host = this.Host,
            //    Label = this.Label,
            //    Sender = this.Sender,

            //    BodyStream = this.BodyStream.Copy(),
            //    TypeName = this.TypeName,
            //    EncodingName=this.EncodingName,

            //    //Identifier = this.Identifier,
            //    Formatter = this.Formatter,
            //    CustomId = this.CustomId,
            //    SessionId = this.SessionId,
            //    DuplexType = this.DuplexType,
            //    Expiration = this.Expiration,
            //    Args = this.Args,
            //    IArgs = this.IArgs,

            //    //Header = this.Header,
            //    //ItemBinary = this.ItemBinary
            //};

            return new MessageQueue(this);
        }

        public object Clone()
        {
            return Copy();
        }
        #endregion

        #region ctor

        private MessageQueue(string identifier)
        {
            Header = new MessageHeader(identifier);
            Priority = Priority.Normal;
            ArrivedTime = Assists.NullDate;
        }

        /// <summary>
        /// Initialize a new instance of Message
        /// </summary>
        public MessageQueue() : this((string)null)
        {
        }

        public MessageQueue(Ptr ptr) : this(ptr.Identifier)
        {
        }
        public MessageQueue(QueueRequest message)
        {
            Header = new MessageHeader()
            {
                Version = message.Version,
                MessageType = MQTypes.MessageRequest,
                QCommand = message.QCommand,
                TransformType = message.TransformType,
                Host = message.Host,
                Creation = message.Creation,
                //Modified = DateTime.Now,
                //m_BodyStream = null;
                //BodyStream = null,
                //EncodingName = message.EncodingName,
                //_IArgs = new NameValueArgs<int>()
            };
            Priority = message.Priority;
            ArrivedTime = Assists.NullDate;
 
        }
        internal MessageQueue(MessageQueue m)
        {
            Header = new MessageHeader(m.Header);
            Source = Source;
            Label = Label;
            Retry = Retry;
            ArrivedTime = ArrivedTime;
            Priority = Priority;
            MessageState = MessageState;
            Duration = Duration;
            Body = Body;
        }


        /// <summary>
        /// Initialize a new instance of MessageStream from stream using for <see cref="ISerialEntity"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        /// <param name="state"></param>
        internal MessageQueue(Stream stream, IBinaryStreamer streamer, MessageState state):this()
        {
            EntityRead(stream, streamer);
            //Modified = DateTime.Now;
            MessageState = state;
        }

        /// <summary>
        /// Initialize a new instance of MessageStream from stream using for <see cref="ISerialEntity"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public MessageQueue(Stream stream, IBinaryStreamer streamer) : this()
        {
            EntityRead(stream, streamer);
        }

        /// <summary>
        /// Initialize a new instance of MessageStream from <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="info"></param>
        public MessageQueue(SerializeInfo info) : this()
        {
            ReadContext(info);
        }

        public MessageQueue(IPersistQueueItem item) : this()
        {
            if (item == null)
            {
                throw new ArgumentNullException("QueueMessage.QueuePersistItem");
            }
            EntityRead(new NetStream(item.ItemBinary),null);

            //item.Header = Header;
            //if (item.Body != null)
            //    m_BodyStream = new NetStream(item.Body);
        }


        //public QueueMessage(byte[] body)
        //{
        //    if(body==null)
        //    {
        //        throw new ArgumentNullException("QueueMessage.body");
        //    }
        //    EntityRead(new NetStream(body), null);
        //}
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
                Header = null;
                Body = null;
                //_Header = null;

                //if (m_BodyStream != null)
                //{
                //    m_BodyStream.Dispose();
                //    m_BodyStream = null;
                //}
            }
            disposed = true;
        }
        
        //protected override void Dispose(bool disposing)
        //{
        //    if (!IsDisposed)
        //    {
        //        Host = null;
        //    }

        //    base.Dispose(disposing);
        //}

        #endregion

        #region property

        public MessageHeader Header { get; private set; }
 
        /// <summary>
        /// Get or Set The message Source.
        /// </summary>
        public string Source { get; internal set; }
        /// <summary>
        /// Get or Set The message Label.
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Get Retry
        /// </summary>
        public byte Retry { get; set; }
        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        public DateTime ArrivedTime { get; set; }
        /// <summary>
        /// Get Priority
        /// </summary>
        public Priority Priority { get; set; }
        /// <summary>
        /// Get MessageState
        /// </summary>
        public MessageState MessageState { get; set; }
        /// <summary>
        /// Get or Set Duration in milliseconds/Expiration in minutes
        /// </summary>
        public int Duration { get; internal set; }


        public bool IsExpired
        {
            get { return Header.IsExpired; }
        }

        #endregion

        #region Body
        public string TypeName { get; set; }

        //public TransType TransType { get; set; }
        //public int State { get; set; }
        //public object Value { get { return _Value; } }
        public byte[] Body { get; set; }
              

        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }

        //public string ToJson(bool pretty = false)
        //{
        //    return GenericKeyValue.Create("TransType", TransType, "State", State, "TypeName", TypeName, "Body", ReadBody()).ToJson(pretty);
        //}

        public virtual object ReadBody()
        {
            if (Body == null)
                return null;
            //BodyStream.Position = 0;
            var ser = new BinarySerializer();
            using (var stream = new NetStream(Body))
            {
                return ser.Deserialize(new NetStream(Body));
            }
            //var ser = new BinarySerializer();
            //return ser.Deserialize(new NetStream(BodyStream));
        }

        public T ReadBody<T>()
        {
            return GenericTypes.Cast<T>(ReadBody(), true);
        }

        public string BodyToJson(bool pretty = false)
        {
            return JsonSerializer.Serialize(ReadBody(), pretty);
        }

        #endregion

        #region  ISerialEntity


        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public virtual void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
   
            streamer.WriteValue(Header);
            streamer.WriteString(Source);
            streamer.WriteString(Label);
            streamer.WriteValue((byte)Retry);
            streamer.WriteValue(ArrivedTime);
            streamer.WriteValue((byte)Priority);
            streamer.WriteValue((byte)MessageState);
            streamer.WriteValue((int)Duration);
            streamer.WriteValue(Body);
            streamer.Flush();
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public virtual void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            Header = streamer.ReadValue<MessageHeader>();
            Source = streamer.ReadString();
            Label = streamer.ReadString();
            Retry = streamer.ReadValue<byte>();
            ArrivedTime =streamer.ReadValue<DateTime>();
            MessageState = (MessageState)streamer.ReadValue<byte>();
            Duration = streamer.ReadValue<int>();
            Body = streamer.ReadValue<byte[]>();
        }

        /// <summary>
        /// Write the current object include the body and properties to <see cref="ISerializerContext"/> using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="info"></param>
        public virtual void WriteContext(ISerializerContext context, SerializeInfo info = null)
        {
            if(info==null)
            info = new SerializeInfo();

            info.Add("Header", Header.ToBase64());
            info.Add("Source", Source);
            info.Add("Label", Label);
            info.Add("Retry", Retry);
            info.Add("ArrivedTime", ArrivedTime);
            info.Add("Priority", (byte)Priority);
            info.Add("MessageState", (byte)MessageState);
            info.Add("Duration", (int)Duration);
            info.Add("Body", BinarySerializer.ToBase64(Body));
        }

        /// <summary>
        /// Read <see cref="ISerializerContext"/> context to the current object include the body and properties using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        public virtual void ReadContext(ISerializerContext context, SerializeInfo info = null)
        {
            if (info == null)
                info = context.ReadSerializeInfo();
            ReadContext(info);
        }

        /// <summary>
        /// Read <see cref="SerializeInfo"/> context to the current object include the body and properties using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        void ReadContext(SerializeInfo info)
        {

            Header = MessageHeader.FromBase64(info.GetValue<string>("Header"));
            Source = info.GetValue<string>("Source");
            Label = info.GetValue<string>("Label");
            Retry = info.GetValue<byte>("Retry");
            ArrivedTime = info.GetValue<DateTime>("ArrivedTime");
            Priority = (Priority)info.GetValue<byte>("Priority");
            MessageState = (MessageState)info.GetValue<byte>("MessageState");
            Duration = info.GetValue<int>("Duration");
            Body = BinarySerializer.FromBase64(info.GetValue<string>("Body"));
        }
        #endregion

        #region ISerialJson

        public virtual string EntityWrite(IJsonSerializer serializer, bool pretty = false)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Write, null);

            //object body = null;
            //if (BodyStream != null)
            //{
            //    body = BinarySerializer.ConvertFromStream(BodyStream);
            //}

            serializer.WriteToken("Header", Header);
            serializer.WriteToken("Source", Source);
            serializer.WriteToken("Label", Label);
            serializer.WriteToken("Retry", Retry);
            serializer.WriteToken("ArrivedTime", ArrivedTime);
            serializer.WriteToken("Priority", (byte)Priority);
            serializer.WriteToken("MessageState", (byte)MessageState);
            serializer.WriteToken("Duration", (int)Duration);
            serializer.WriteToken("Body", BinarySerializer.ToBase64(Body));
            return serializer.WriteOutput(pretty);

        }

        public virtual object EntityRead(string json, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, new JsonSettings() { IgnoreCaseOnDeserialize = true });

            var JsonReader = serializer.Read<Dictionary<string, object>>(json);
            if(JsonReader!= null)
            {

                Header=JsonReader.Get<MessageHeader>("Header");
                Source=JsonReader.Get<string>("Source");
                Label=JsonReader.Get<string>("Label");
                Retry=JsonReader.Get<byte>("Retry");
                ArrivedTime=JsonReader.Get<DateTime>("ArrivedTime");
                Priority= (Priority)JsonReader.Get<byte>("Priority");
                MessageState= (MessageState)JsonReader.Get<byte>("MessageState");
                Duration=JsonReader.Get<int> ("Duration");
                Body= BinarySerializer.FromBase64(JsonReader.Get<string>("Body"));
                
            }
            return this;
        }

        public virtual object EntityRead(NameValueCollection queryString, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, new JsonSettings() { IgnoreCaseOnDeserialize = true });

            if (queryString != null)
            {
                Header = queryString.Get<MessageHeader>("Header");
                Source = queryString.Get<string>("Source");
                Label = queryString.Get<string>("Label");
                Retry = queryString.Get<byte>("Retry");
                ArrivedTime = queryString.Get<DateTime>("ArrivedTime");
                Priority = (Priority)queryString.Get<byte>("Priority");
                MessageState = (MessageState)queryString.Get<byte>("MessageState");
                Duration = queryString.Get<int>("Duration");
                Body = BinarySerializer.FromBase64(queryString.Get<string>("Body"));
            }

            return this;
        }

        #endregion

        #region Converters

        public byte[] Serialize()
        {
            return BinarySerializer.SerializeToBytes(this);
        }
        public static QueueMessage Deserialize(byte[] bytes)
        {
            return BinarySerializer.Deserialize<QueueMessage>(bytes);
        }

        //public PersistItem ToPersistItem()
        //{
        //    return new PersistItem()
        //    {
        //        ArrivedTime = this.ArrivedTime,
        //        Expiration = this.Expiration,
        //        Identifier = this.Identifier,
        //        MessageState = this.MessageState,
        //        Retry = Retry,
        //        Header = Header,
        //        Body = (BodyStream != null) ? BodyStream.ToArray() : null

        //    };
        //}
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);

            //switch (MessageType)
            //{
            //    case MQTypes.Ack:
            //        QueueAck ack = new QueueAck(ToStream());// GetMessageStream());
            //        return JsonSerializer.Serialize(ack);
            //    case MQTypes.Message:
            //        //Message message = new Message(GetMessageStream());
            //        //return JsonSerializer.Serialize(message);
            //        return JsonSerializer.Serialize(this);
            //    case MQTypes.MessageRequest:
            //        QueueRequest mr = new QueueRequest(ToStream());// GetMessageStream());
            //        return JsonSerializer.Serialize(mr);
            //    case MQTypes.Json:
            //        //var stream = (ToStream();// GetMessageStream);
            //        //return Encoding.UTF8.GetString(stream.ToArray());
            //        return JsonSerializer.Serialize(this);
            //}
            //return null;
        }
        public static QueueMessage Deserialize(string json)
        {
            return JsonSerializer.Deserialize<QueueMessage>(json);

        }

        //public IQueueAck ToAck()
        //{
        //    return this;
        //    //return new QueueAck()
        //    //{
        //    //    ArrivedTime = ArrivedTime,
        //    //    Count = 0,
        //    //    Host = Host,
        //    //    Identifier = Identifier,
        //    //    Label = Label,
        //    //    MessageState = this.MessageState
        //    //};
        //}

        //public Message ToMessage()
        //{
        //    //return new Message()
        //    //{
        //    //    MessageState = this.MessageState,
        //    //    MessageType = this.MessageType,
        //    //    Command = this.Command,
        //    //    Priority = this.Priority,
        //    //    Identifier = this.Identifier,
        //    //    Retry = this.Retry,
        //    //    ArrivedTime = this.ArrivedTime,
        //    //    Modified = this.Modified,
        //    //    TransformType = this.TransformType,
        //    //    Label = this.Label,
        //    //    Host = this.Host,
        //    //    m_BodyStream = this.BodyStream.Copy(),
        //    //    //Header = this.Header,
        //    //    ItemBinary = this.ItemBinary
        //    //}

        //    return GetMessage();// new Message(GetBodyStream());
        //}

        ///// <summary>
        ///// Get body stream after set the position to first byte in buffer, This method is a part of <see cref="IQueueMessage"/> implementation.
        ///// </summary>
        ///// <returns></returns>
        //public NetStream GetMessageStream()
        //{
        //    if (ItemBinary == null)
        //        return null;
        //    return new NetStream(ItemBinary);
        //}

        //public NetStream ToStream()
        //{
        //    NetStream stream = new NetStream();
        //    EntityWrite(stream, null);
        //    return stream;
        //}

        ///// <summary>
        ///// Get body stream after set the position to first byte in buffer, This method is a part of <see cref="IMessageStream"/> implementation.
        ///// </summary>
        ///// <returns></returns>
        //public NetStream GetBodyStream()
        //{
        //    return new NetStream(ItemBinary);

        //    //if (BodyStream == null)
        //    //    return null;
        //    //if (BodyStream.Position > 0)
        //    //    BodyStream.Position = 0;
        //    //return BodyStream;
        //}


        #endregion

        #region IMessageStream

        public TransStream ToTransStream()
        {
            TransStream stream = new TransStream(this);
            return stream;
        }

        public TransStream ToTransStream(MessageState state)
        {
            MessageState = state;// this.SetState(state);
            TransStream stream = new TransStream(this);
            return stream;
        }

        //public NetStream ToStream()
        //{
        //    NetStream stream = new NetStream();
        //    EntityWrite(stream, null);
        //    return stream;
        //}

        ///// <summary>
        ///// Get body stream after set the position to first byte in buffer, This method is a part of <see cref="IMessageStream"/> implementation.
        ///// </summary>
        ///// <returns></returns>
        //public NetStream GetBodyStream()
        //{
        //    if (BodyStream == null)
        //        return null;
        //    if (BodyStream.Position > 0)
        //        BodyStream.Position = 0;
        //    return BodyStream;
        //}

        ///// <summary>
        ///// Deserialize body stream to object, This method is a part of <see cref="IMessageStream"/> implementation.
        ///// </summary>
        ///// <returns></returns>
        //public object GetBody()
        //{
        //    if (Body == null)
        //        return null;
        //    BodyStream.Position = 0;
        //    var ser = new BinarySerializer();
        //    return ser.Deserialize(BodyStream, true);
        //}
        ///// <summary>
        /////  Deserialize body stream to generic object.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public T GetBody<T>()
        //{
        //    return GenericTypes.Cast<T>(GetBody());
        //}

        
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
                    //BodyStream = ns;
                    Body = ns.ToArray();
                }
             }
             else
             {
                 TypeName = typeof(object).FullName;
                 Body = null;
             }
         }
         /// <summary>
         /// Set the given value to body stream using <see cref="BinarySerializer"/>, This method is a part of <see cref="IMessageStream"/> implementation..
         /// </summary>
         /// <param name="value"></param>
         public void SetBodyText(string value)
         {

             if (value != null)
             {
                 TypeName = value.GetType().FullName;

                using (NetStream ns = new NetStream())
                {
                    var ser = new BinarySerializer();
                    ser.Serialize(ns, value);
                    ns.Position = 0;
                    //BodyStream = ns;
                    Body = ns.ToArray();
                }
             }
             else
             {
                 TypeName = typeof(string).FullName;
                 Body = null;
             }
         }

         ///// <summary>
         ///// Set the given array of values to body stream using <see cref="BinarySerializer"/>, This method is a part of <see cref="IMessageStream"/> implementation..
         ///// </summary>
         ///// <param name="value"></param>
         //public void SetBodyWithSegments(object[] value)
         //{

         //    if (value != null)
         //    {
         //        _TypeName = value.GetType().FullName;
         //        Segments = (byte)value.Length;
         //        NetStream ns = new NetStream();
         //        var ser = new BinarySerializer();
         //        ser.Serialize(ns, value, typeof(object[]));
         //        ns.Position = 0;
         //        m_BodyStream = ns;
         //    }
         //    else
         //    {
         //        _TypeName = typeof(object).FullName;
         //        m_BodyStream = null;
         //    }
         //}

         /// <summary>
         /// Set the given byte array to body stream using <see cref="NetStream"/>, This method is a part of <see cref="IMessageStream"/> implementation
         /// </summary>
         /// <param name="value"></param>
         /// <param name="type"></param>
         public void SetBody(byte[] value, Type type)
         {
             TypeName = (type != null) ? type.FullName : typeof(object).FullName;
             Body = value;
             //if (value != null)
             //{
             //    BodyStream = new NetStream(value);
             //}
         }

         /// <summary>
         /// Set the given byte array to body stream using <see cref="NetStream"/>, This method is a part of <see cref="IMessageStream"/> implementation
         /// </summary>
         /// <param name="value"></param>
         /// <param name="type"></param>
         public void SetBody(NetStream value, Type type)
         {
             TypeName = (type != null) ? type.FullName : typeof(object).FullName;
             if (value != null)
             {
                 Body = value.ToArray();
             }
         }

         /// <summary>
         /// Read stream to object.
         /// </summary>
         /// <param name="type"></param>
         /// <param name="stream"></param>
         /// <returns></returns>
         public static object ReadBodyStream(Type type, Stream stream)
         {
             if (stream == null)
             {
                 throw new ArgumentNullException("ReadBodyStream.stream");
             }
             if (type == null)
             {
                 throw new ArgumentNullException("ReadBodyStream.type");
             }

             BinarySerializer reader = new BinarySerializer();
             return reader.Deserialize(stream);
         }
         /// <summary>
         /// Write object to stream
         /// </summary>
         /// <param name="entity"></param>
         /// <param name="stream"></param>
         public static void WriteBodyStream(object entity, Stream stream)
         {
             if (stream == null)
             {
                 throw new ArgumentNullException("WriteBodyStream.stream");
             }
             if (entity == null)
             {
                 throw new ArgumentNullException("WriteBodyStream.entity");
             }

             BinarySerializer writer = new BinarySerializer();
             writer.Serialize(stream, entity);
             //writer.Flush();
         }
          

        #endregion

        #region item properties

        public Ptr GetPtr()
        {
            return new Ptr(Header.Identifier, 0, Header.Host);
        }

        public Ptr GetPtr(string hotName)
        {
            return new Ptr(Header.Identifier, 0, hotName);
        }

        //public double Duration()
        //{
        //    return SentTime.Subtract(ArrivedTime).TotalSeconds;
        //}

        internal void SetReceived(MessageState state)
        {
            DateTime now = DateTime.Now;
            DateTime recievTime = ArrivedTime;

            switch (state)
            {
                case MessageState.Receiving:
                case MessageState.Received:
                case MessageState.Peeking:
                case MessageState.Peeked:
                case MessageState.Arrived:
                    MessageState = state;
                    break;
            }
            //if (state != MessageState.None)
            //    MessageState = state;// MessageState.Received;

            ArrivedTime = now;
            var d = now.Subtract(recievTime).TotalMilliseconds;
            d = Math.Min(d, int.MaxValue);
            Duration = (int)d;
        }
        internal void SetArrived()
        {
            DateTime now = DateTime.Now;
            ArrivedTime = now;
            var d = now.Subtract(Header.Creation).TotalMilliseconds;
            //var d = now.Subtract(Creation).TotalMilliseconds;
            d = Math.Min(d, int.MaxValue);
            Duration = (int)d;
        }

        internal Ptr SetArrivedPtr(string host)
        {
            try
            {
                DateTime now = DateTime.Now;
                //this.Modified = now;
                this.ArrivedTime = now;
                this.MessageState = Messaging.MessageState.Arrived;
                //this.Identifier = Ptr.NewIdentifier();

                Ptr ptr = new Ptr(Header.Identifier,0, host);
                return ptr;

                //_Header = null;

                //SetHeader();

                //m_stream.Replace((byte)MessageState, offset + 1);
                //m_stream.Replace(ItemId.ToByteArray(), offset + 7, 16);
                //m_stream.Replace(ArrivedTime.Ticks, offset + 26);
                //m_stream.Replace(Modified.Ticks, offset + 44);

            }
            catch (Exception ex)
            {
                throw new MessageException(Messaging.MessageState.StreamReadWriteError, "QueueItemStream SetArrived error: " + ex.Message);
            }
        }
        internal void SetRetryInternal()
        {
            Retry++;
            //this.Modified = DateTime.Now;
            //_Header = null;
            //m_stream.Replace(Retry, offset + 24);
            //m_stream.Replace(Modified.Ticks, offset + 44);
        }
        /*
        public void SetReceiving()
        {
            try
            {
                this.Modified = DateTime.Now;
                //this.SentTime = DateTime.Now;
                this.MessageState = Messaging.MessageState.Receiving;
                SetHeader();
                //m_stream.Replace((byte)MessageState, offset + 1);
                //m_stream.Replace(SentTime.Ticks, offset + 35);
                //m_stream.Replace(Modified.Ticks, offset + 44);
            }
            catch (Exception ex)
            {
                throw new MessageException(Messaging.MessageState.StreamReadWriteError, "QueueItemStream SetReceiving error: " + ex.Message);
            }
        }
        public void SetState(MessageState state)
        {
            try
            {
                this.Modified = DateTime.Now;
                this.MessageState = state;
                SetHeader();
                //m_stream.Replace((byte)MessageState, offset + 1);
                //m_stream.Replace(Modified.Ticks, offset + 44);
            }
            catch (Exception ex)
            {
                throw new MessageException(Messaging.MessageState.StreamReadWriteError, "QueueItemStream SetState error: " + ex.Message);
            }
        }

        
        */
        #endregion

        #region util

        internal object[] ItemArray()
        {

            return new object[] {
            Header,
            Source,
            Label,
            Retry,
            ArrivedTime,
            Priority,
            MessageState,
            Duration,
            Body
            };
        }

        public void BeginTransScop()
        {
            //string filename = GetFilename(hostPath);
            //BodyStream.SaveToFile(filename);
            //return filename;
        }

        public void EndTransScop(ItemState state)
        {

        }

        public string Print()
        {
            return Header.Print();
        }
        #endregion

        #region File

        /// <summary>
        /// Get an instance of <see cref="QueueItemStream"/> from file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static QueueMessage ReadFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return null;
            }

            NetStream netStream = new NetStream();
            using (Stream input = File.OpenRead(filename))
            {
                input.CopyTo(netStream);
            }
            netStream.Position = 0;
            return new QueueMessage(netStream, null);
        }

        /// <summary>
        /// Get an instance of <see cref="QueueItemStream"/> from file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static ReadFileState ReadFile(string filename, out IQueueMessage item)
        {
            if (!File.Exists(filename))
            {
                item = null;
                return ReadFileState.NotExists;
            }

            try
            {
                NetStream netStream = new NetStream();
                using (Stream input = File.OpenRead(filename))
                {
                    input.CopyTo(netStream);
                }
                netStream.Position = 0;
                QueueMessage qitem = new QueueMessage(netStream, null);

                item = qitem as IQueueMessage;

                return ReadFileState.Completed;
            }
            catch (IOException ioex)
            {
                //Netlog.Exception("ReadFile IOException ", ioex);
                item = null;
                return ReadFileState.IOException;
            }
            catch (Exception ex)
            {
                //Netlog.Exception("ReadFile Exception ", ex);
                item = null;
                return ReadFileState.Exception;
            }
        }
        public void SaveToFile(string filename)
        {
            var stream = ToStream();
            if (stream == null)
            {
                throw new Exception("Invalid BodyStream , Can't save body stream to file,");
            }
            //string filename = GetPtrLocation(location);

            //stream.GetStream().Copy().SaveToFile(filename);
            stream.Copy().SaveToFile(filename);

            //BodyStream.Position = 0;
            //return filename;
        }


        //public string SaveToFile(string location)
        //{
        //    if (BodyStream == null)
        //    {
        //        throw new Exception("Invalid BodyStream , Can't save body stream to file,");
        //    }
        //    string filename = GetPtrLocation(location);
        //    BodyStream.SaveToFile(filename);
        //    return filename;
        //}

        //internal string GetPtrLocation(string host)
        //{
        //    return Ptr.GetPtrLocation(host, FolderId, Identifier);
        //}

        #endregion

        #region extended properties
        public string Filename
        {
            get { return Assists.GetFilename(Header.Identifier); }
        }

        public string FolderId
        {
            get
            {
                return Assists.GetFolderId(Header.Identifier);//Modified, Priority);
            }
        }

        #endregion
    
    }

}
