using Nistec.Channels.Http;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Nistec.Messaging
{

    public class Message : IQueueItem, IQueueAck, ICloneable, IDisposable//, ISerialEntity
    {
        #region static

        public static QueueItem Ack(MessageState state, QueueCmd cmd)
        {
            return new QueueItem()
            {
                Command = cmd,
                Label = state.ToString(),
                MessageState = state
            };
        }

        public static QueueItem Ack(MessageState state, QueueCmd cmd, Exception ex)
        {
            return new QueueItem()
            {
                Command = cmd,
                Label = ex == null ? state.ToString() : ex.Message,
                MessageState = state
            };
        }

        public static QueueItem Ack(MessageState state, QueueCmd cmd, string label, string identifier)
        {
            return new QueueItem()
            {
                Command = cmd,
                Label = label,
                Identifier = identifier,
                MessageState = state
            };
        }

        public static QueueItem Ack(QueueItem item, MessageState state, int retry, string label, string identifier)
        {
            return new QueueItem()
            {
                ArrivedTime = DateTime.Now,
                MessageState = state,
                Command = item.Command,
                Destination = item.Destination,
                Identifier = identifier,
                Modified = DateTime.Now,
                Priority = item.Priority,
                MessageType = item.MessageType,
                Sender = item.Sender,
                TransformType = item.TransformType,
                Retry =(byte) retry,
                Label=label
            };
        }


        ///// <summary>
        ///// Create a new instance of QueueItem.
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <returns></returns>
        //public static QueueItem Create(Stream stream)
        //{
        //    if (stream == null)
        //    {
        //        throw new ArgumentNullException("Message.Load.stream");
        //    }

        //    if (stream is NetStream)
        //    {
        //        stream.Position = 0;
        //        return new QueueItem((NetStream)stream, null);
        //    }
        //    else
        //    {
        //        return new QueueItem(NetStream.CopyStream(stream), null);
        //    }
        //}

        #endregion

        #region ICloneable

        public IQueueItem Copy()
        {
            var copy = new Message()
            {
                MessageState = this.MessageState,
                MessageType = this.MessageType,
                Command = this.Command,
                Priority = this.Priority,
                Identifier = this.Identifier,
                Retry = this.Retry,
                ArrivedTime = this.ArrivedTime,
                Modified = this.Modified,
                TransformType = this.TransformType,
                Label = this.Label,
                Destination = this.Destination,
                m_BodyStream = this.BodyStream.Copy(),
                //Header = this.Header,
                ItemBinary = this.ItemBinary
            };

            return copy;
        }

        public object Clone()
        {
            return Copy();
        }
        #endregion

        #region ctor
        /// <summary>
        /// Initialize a new instance of Message
        /// </summary>
        public Message()
        {
            Priority = Priority.Normal;
        }
       
        public Message(QueueCmd command, TransformTypes transformType, Priority priority, string destination, string json)
        {
            MessageType = MQTypes.MessageRequest;
            Command = command;
            Priority = priority;
            TransformType = transformType;
            Destination = destination;
            if(json!=null)
            m_BodyStream = new NetStream(Encoding.UTF8.GetBytes(json)); 
            //ArrivedTime = DateTime.Now;
            SetArrived();

            ItemBinary = Encoding.UTF8.GetBytes(json);
            //m_BodyStream = new NetStream(Body);
        }

        public Message(QueueItem message, byte[] body, Type type)
        {

            Message msg = message.ToMessage();
            msg.SetBody(body,type);

            Command = message.Command;
            Priority = message.Priority;
            TransformType = message.TransformType;
            Label = message.Label;
            Destination = message.Destination;
            m_BodyStream = new NetStream(body);

            NetStream ns = new NetStream();
            msg.EntityWrite(ns,null);
            ItemBinary = ns.ToArray();


            //MessageType = MQTypes.MessageRequest;
            //Command = message.Command;
            //Priority = message.Priority;
            //TransformType = message.TransformType;
            //Destination = message.Destination;
            //m_BodyStream = new NetStream(body);
            ////ArrivedTime = DateTime.Now;
            //SetArrived();
            //SetItemBinary();

            //NetStream ns = new NetStream();
            //this.EntityWrite(ns,null);

            //message.EntityWrite(ns);
            //m_BodyStream = ns;
            //ItemBinary = body;// Encoding.UTF8.GetBytes(body);
        }

        public Message(MessageRequest message)
        {
            MessageType = MQTypes.MessageRequest;
            Command = message.Command;
            Priority = message.Priority;
            TransformType = message.TransformType;
            Destination = message.Destination;
            m_BodyStream = null;
            //ArrivedTime = DateTime.Now;
            SetArrived();
            NetStream ns = new NetStream();
            message.EntityWrite(ns);
            //m_BodyStream = ns;
            ItemBinary = ns.ToArray();
        }

        public Message(Message message)
        {
            MessageType = MQTypes.Message;
            Command = message.Command;
            Priority = message.Priority;
            TransformType = message.TransformType;
            Label = message.Label;
            Destination = message.Destination;
            m_BodyStream = message.BodyStream;
            //ArrivedTime = DateTime.Now;
            SetArrived();

            NetStream ns = new NetStream();
            message.EntityWrite(ns,null);
            //m_BodyStream = ns;
            ItemBinary = ns.ToArray();
        }

        public Message(QueueAck message)
        {
            MessageType = MQTypes.Ack;
            Command = QueueCmd.Ack;
            //Priority = message.Priority;
            //TransformType = message.TransformType;
            Destination = message.Destination;
            Label = message.Label;

            //ArrivedTime = DateTime.Now;
            SetArrived();
            NetStream ns = new NetStream();
            message.EntityWrite(ns);
            //m_BodyStream = ns;
            ItemBinary = ns.ToArray();
        }

        /// <summary>
        /// Initialize a new instance of MessageStream from stream using for <see cref="ISerialEntity"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public Message(Stream stream, IBinaryStreamer streamer)
        {
            EntityRead(stream, streamer);
        }

        /// <summary>
        /// Initialize a new instance of MessageStream from <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="info"></param>
        public Message(SerializeInfo info)
        {
            ReadContext(info);
        }

        internal Message(NetStream stream)
        {
            EntityRead(stream, null);
        }

        //public QueueItem(byte[] body)
        //{
        //    if(body==null)
        //    {
        //        throw new ArgumentNullException("QueueItem.body");
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
                Destination = null;
                ItemBinary = null;

                if (m_BodyStream != null)
                {
                    m_BodyStream.Dispose();
                    m_BodyStream = null;
                }
            }
            disposed = true;
        }
        #endregion

        #region property

        public int Version { get { return 4022; } }

        /// <summary>
        /// Get ItemId
        /// </summary>
        public string Identifier { get; set; }


        /// <summary>
        /// Get MessageState
        /// </summary>
        public MessageState MessageState { get; set; }

        /// <summary>
        /// Get Command
        /// </summary>
        public QueueCmd Command { get; set; }

        /// <summary>
        /// Get or Set transformation type.
        /// </summary>
        public TransformTypes TransformType { get; set; }

        /// <summary>
        /// Get or Set message type.
        /// </summary>
        public MQTypes MessageType { get; set; }

        /// <summary>
        /// Get Priority
        /// </summary>
        public Priority Priority { get; set; }
        
        /// <summary>
        /// Get The message Destination\Queue name.
        /// </summary>
        public string Destination { get; set; }

        NetStream m_BodyStream;
        /// <summary>
        /// Get or Set The message body stream.
        /// </summary>
        public NetStream BodyStream { get { return m_BodyStream; } }

        string _TypeName;
        /// <summary>
        ///  Get The type name of body stream.
        /// </summary>
        public string TypeName
        {
            get { return _TypeName; }
        }

        /// <summary>
        /// Get Retry
        /// </summary>
        public byte Retry { get; set; }

        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        public DateTime ArrivedTime { get; set; }
 
        /// <summary>
        /// Get the last modified time.
        /// </summary>
        public DateTime Modified { get; set; }
        /// <summary>
        /// Get or Set Expiration in minutes
        /// </summary>
        public int Expiration { get; set; }
        /// <summary>
        /// Get or Set The message Sender.
        /// </summary>
        public string Sender { get; internal set; }

        /// <summary>
        /// Get or Set The message Label.
        /// </summary>
        public string Label { get; set; }


        #endregion

        #region ItemStream/Body

        public byte[] ItemBinary { get; set; }

        public byte[] Body { get { return ItemBinary; } }


        //protected void SetItemBinary()
        //{
        //    using (NetStream stream = new NetStream())
        //    {
        //        IBinaryStreamer streamer = new BinaryStreamer(stream);

        //        streamer.WriteValue((byte)MessageState);
        //        streamer.WriteValue((byte)MessageType);
        //        streamer.WriteValue((byte)Command);
        //        streamer.WriteValue((byte)Priority);
        //        streamer.WriteString(Identifier);//.WriteValue(ItemId);
        //        streamer.WriteValue(Retry);
        //        streamer.WriteValue(ArrivedTime);
        //        //streamer.WriteValue(SentTime);
        //        streamer.WriteValue(Modified);
        //        streamer.WriteValue(Expiration);
        //        //streamer.WriteValue(MessageId);
        //        streamer.WriteValue((byte)TransformType);

        //        streamer.WriteString(Destination);

        //        streamer.WriteValue(BodyStream);

        //        //streamer.WriteString(Label);
        //        //streamer.WriteString(Sender);
        //        //streamer.WriteString(Topic);
        //        //streamer.WriteValue(HeaderStream);

        //        //streamer.WriteValue(new NetStream(Body));
        //        //streamer.WriteValue(ItemBinary);

        //        //if (Command != QueueCmd.Ack)
        //        //{
        //        //    streamer.WriteValue((int)Formatter);
        //        //    streamer.WriteValue(BodyStream);
        //        //    streamer.WriteString(TypeName);
        //        //    streamer.WriteValue(Segments);
        //        //    streamer.WriteString(Notify);
        //        //}

        //        streamer.Flush();

        //        ItemBinary = stream.ToArray();
        //    }
        //}

        #endregion

        #region Header

        byte[] _Header;
        public byte[] Header
        {
            get
            {
                if(_Header==null)
                {
                    SetHeader();
                }
                return _Header;
            }
            set {

                if (value != null)
                {
                    var header = new MessageHeader();
                    header.EntityWrite(new NetStream(value), null);

                    MessageState = header.MessageState;
                    MessageType = header.MessageType;
                    Command = header.Command;
                    Priority = header.Priority;
                    Identifier = header.Identifier;
                    Retry = header.Retry;
                    ArrivedTime = header.ArrivedTime;
                    Modified = header.Modified;
                    Expiration = header.Expiration;
                    TransformType = header.TransformType;
                    Destination = header.Destination;
                    Sender = header.Sender;
                    Label = header.Label;
                }
                _Header = value;
            }
        }

        internal void SetHeader()
        {
            var header = new MessageHeader()
            {
                MessageState = this.MessageState,
                MessageType = this.MessageType,
                Command = this.Command,
                Priority = this.Priority,
                Identifier = this.Identifier,
                Retry = this.Retry,
                ArrivedTime = this.ArrivedTime,
                Modified = this.Modified,
                Expiration=this.Expiration,
                TransformType = this.TransformType,
                Destination = this.Destination,
                Sender = this.Sender,
                Label = this.Label,
            };
            _Header = header.ToBinary();
        }

        /// <summary>
        /// Get Header stream after set the position to first byte in buffer.
        /// </summary>
        /// <returns></returns>
        public NetStream GetHeaderStream()
        {
            return new NetStream(Header);
        }

        public MessageHeader GetHeader()
        {
            var header = new MessageHeader();
            header.EntityWrite(GetHeaderStream(), null);
            return header;
        }

        #endregion

        #region IBodyFormatter extend

        /// <summary>
        /// Set the given byte array to body stream.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="typeName"></param>
        public void SetBody(byte[] value, string typeName)
        {
            _TypeName = (!string.IsNullOrEmpty(typeName)) ? typeName : typeof(object).FullName;
            if (value != null)
            {
                m_BodyStream = new NetStream(value);
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
            _TypeName = (!string.IsNullOrEmpty(typeName)) ? typeName : typeof(object).FullName;
            if (ns != null)
            {
                if (copy)
                    ns.CopyTo(m_BodyStream);
                else
                    m_BodyStream = ns;
            }
        }
        /// <summary>
        /// Set the given stream to body stream.
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="typeName"></param>
        /// <param name="copy"></param>
        internal void SetBodyInternal(object value, string typeName, bool copy = true)
        {
            if (value == null)
                m_BodyStream = null;
            else if (value is NetStream)
            {
                _TypeName = (!string.IsNullOrEmpty(typeName)) ? typeName : typeof(object).FullName;
                if (copy)
                    ((NetStream)value).CopyTo(BodyStream);
                else
                    m_BodyStream = (NetStream)value;
            }
            else
            {
                SetBody(value);
            }
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

            streamer.WriteValue((byte)MessageState);
            streamer.WriteValue((byte)MessageType);
            streamer.WriteValue((byte)Command);
            streamer.WriteValue((byte)Priority);
            streamer.WriteString(Identifier);//.WriteValue(ItemId);
            streamer.WriteValue(Retry);
            streamer.WriteValue(ArrivedTime);
            //streamer.WriteValue(SentTime);
            streamer.WriteValue(Modified);
            streamer.WriteValue(Expiration);
            //streamer.WriteValue(MessageId);
            streamer.WriteValue((byte)TransformType);

            streamer.WriteString(Destination);

            streamer.WriteValue(BodyStream);

            //streamer.WriteString(Label);
            //streamer.WriteString(Sender);
            //streamer.WriteString(Topic);
            //streamer.WriteValue(HeaderStream);

            //streamer.WriteValue(new NetStream(Body));
            streamer.WriteValue(ItemBinary);

            //if (Command != QueueCmd.Ack)
            //{
            //    streamer.WriteValue((int)Formatter);
            //    streamer.WriteValue(BodyStream);
            //    streamer.WriteString(TypeName);
            //    streamer.WriteValue(Segments);
            //    streamer.WriteString(Notify);
            //}

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

            MessageState = (MessageState)streamer.ReadValue<byte>();
            MessageType = (MQTypes)streamer.ReadValue<byte>();
            Command = (QueueCmd)streamer.ReadValue<byte>();
            Priority = (Priority)streamer.ReadValue<byte>();
            Identifier = streamer.ReadString();//.ReadValue<Guid>();
            Retry = streamer.ReadValue<byte>();
            ArrivedTime = streamer.ReadValue<DateTime>();
            //SentTime = streamer.ReadValue<DateTime>();
            Modified = streamer.ReadValue<DateTime>();
            Expiration = streamer.ReadValue<int>();
            //MessageId = streamer.ReadValue<int>();
            TransformType = (TransformTypes)streamer.ReadValue<byte>();

            Destination = streamer.ReadString();
            //Label = streamer.ReadString();
            //Sender = streamer.ReadString();
            ////Topic = streamer.ReadString();
            //HeaderStream = (NetStream)streamer.ReadValue();

            m_BodyStream = (NetStream)streamer.ReadValue();

            //var ns = (NetStream)streamer.ReadValue();
            //Body = ns.ToArray();

            ItemBinary = streamer.ReadValue<byte[]>(); 

            //var map = streamer.GetMapper();
            //Console.WriteLine(map);
            //if (Command != QueueCmd.Ack)
            //{
            //    Formatter = (Formatters)streamer.ReadValue<int>();
            //    m_BodyStream = (NetStream)streamer.ReadValue();
            //    _TypeName = streamer.ReadString();
            //    Segments = streamer.ReadValue<byte>();

            //    //HostType = streamer.ReadValue<byte>();
            //    Notify = streamer.ReadString();

            //    //Command = streamer.ReadString();
            //    //IsDuplex = streamer.ReadValue<bool>();
            //    //HeaderStream = (GenericNameValue)streamer.ReadValue();
            //}
        }

        /// <summary>
        /// Write the current object include the body and properties to <see cref="ISerializerContext"/> using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        public void WriteContext(ISerializerContext context)
        {
            SerializeInfo info = new SerializeInfo();

            info.Add("MessageState", (byte)MessageState);
            info.Add("MessageType", (byte)MessageType);
            info.Add("Command", (byte)Command);
            info.Add("Priority", (byte)Priority);
            info.Add("Identifier", Identifier);
            info.Add("Retry", Retry);
            info.Add("ArrivedTime", ArrivedTime);
            //info.Add("SentTime", SentTime);
            info.Add("Modified", Modified);
            info.Add("Expiration", Expiration);
            //info.Add("MessageId", MessageId);
            info.Add("TransformType", (byte)TransformType);
            info.Add("Destination", Destination);
            //info.Add("Label", Label);
            //info.Add("Sender", Sender);
            ////info.Add("Topic", Topic);
            //info.Add("Headers", HeaderStream);
            info.Add("BodyStream", BodyStream);
            info.Add("ItemBinary", new NetStream(ItemBinary));

            //if (Command != QueueCmd.Ack)
            //{
            //    info.Add("Formatter", (int)Formatter);
            //    info.Add("BodyStream", BodyStream);
            //    info.Add("TypeName", TypeName);
            //    info.Add("Segments", Segments);

            //    //info.Add("HostType", HostType);
            //    info.Add("Notify", Notify);

            //    //info.Add("Command", Command);
            //    //info.Add("IsDuplex", IsDuplex);
            //    //info.Add("Headers", GetHeaders());
            //}
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

            MessageState = (MessageState)info.GetValue<byte>("MessageState");
            MessageType = (MQTypes)info.GetValue<byte>("MessageType");
            Command = (QueueCmd)info.GetValue<byte>("Command");
            Priority = (Priority)info.GetValue<byte>("Priority");
            Identifier = info.GetValue<string>("Identifier");
            Retry = info.GetValue<byte>("Retry");
            ArrivedTime = info.GetValue<DateTime>("ArrivedTime");
            //SentTime = info.GetValue<DateTime>("SentTime");
            Modified = info.GetValue<DateTime>("Modified");
            Expiration = info.GetValue<int>("Expiration");
            //MessageId = info.GetValue<int>("MessageId");
            TransformType = (TransformTypes)info.GetValue<byte>("TransformType");
            Destination = info.GetValue<string>("Destination");
            //Label = info.GetValue<string>("Label");
            //Sender = info.GetValue<string>("Sender");
            ////Topic = info.GetValue<string>("Topic");
            //HeaderStream = (NetStream)info.GetValue("HeaderStream");

            m_BodyStream = (NetStream)info.GetValue("BodyStream");

            var ns = (NetStream)info.GetValue("ItemBinary");
            ItemBinary = ns.ToArray();

            //if (Command != QueueCmd.Ack)
            //{
            //    Formatter = (Formatters)info.GetValue<int>("Formatter");
            //    m_BodyStream = (NetStream)info.GetValue("BodyStream");
            //    _TypeName = info.GetValue<string>("TypeName");
            //    Segments = info.GetValue<byte>("Segments");

            //    //HostType = info.GetValue<byte>("HostType");
            //    Notify = info.GetValue<string>("Notify");

            //    //Command = info.GetValue<string>("Command");
            //    //IsDuplex = info.GetValue<bool>("IsDuplex");
            //    //_Headers = (GenericNameValue)info.GetValue("Headers");
            //}

        }
        #endregion

        #region Converters

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);

            //switch (MessageType)
            //{
            //    case MQTypes.Ack:
            //        QueueAck ack = new QueueAck(GetMessageStream());
            //        return JsonSerializer.Serialize(ack);
            //    case MQTypes.Message:
            //        Message message = new Message(GetMessageStream());
            //        return JsonSerializer.Serialize(message);
            //    case MQTypes.MessageRequest:
            //        MessageRequest mr = new MessageRequest(GetMessageStream());
            //        return JsonSerializer.Serialize(mr);
            //    case MQTypes.Json:
            //        var stream = GetMessageStream();
            //        return Encoding.UTF8.GetString(stream.ToArray());
            //}
            //return null;
        }

        public IQueueAck ToAck()
        {
            return this;
            //return new QueueAck()
            //{
            //    ArrivedTime = ArrivedTime,
            //    Count = 0,
            //    Destination = Destination,
            //    Identifier = Identifier,
            //    Label = Label,
            //    MessageState = this.MessageState
            //};
        }

        public Message ToMessage()
        {
            //return new Message()
            //{
            //    MessageState = this.MessageState,
            //    MessageType = this.MessageType,
            //    Command = this.Command,
            //    Priority = this.Priority,
            //    Identifier = this.Identifier,
            //    Retry = this.Retry,
            //    ArrivedTime = this.ArrivedTime,
            //    Modified = this.Modified,
            //    TransformType = this.TransformType,
            //    Label = this.Label,
            //    Destination = this.Destination,
            //    m_BodyStream = this.BodyStream.Copy(),
            //    //Header = this.Header,
            //    ItemBinary = this.ItemBinary
            //}

            return this;// new Message(GetBodyStream());
        }

        /// <summary>
        /// Get body stream after set the position to first byte in buffer, This method is a part of <see cref="IQueueItem"/> implementation.
        /// </summary>
        /// <returns></returns>
        public NetStream GetMessageStream()
        {
            if (ItemBinary == null)
                return null;
            return new NetStream(ItemBinary);
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
        //    return new NetStream(ItemBinary);

        //    //if (BodyStream == null)
        //    //    return null;
        //    //if (BodyStream.Position > 0)
        //    //    BodyStream.Position = 0;
        //    //return BodyStream;
        //}


        #endregion

        #region IMessageStream

        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }

        /// <summary>
        /// Get body stream after set the position to first byte in buffer, This method is a part of <see cref="IMessageStream"/> implementation.
        /// </summary>
        /// <returns></returns>
        public NetStream GetBodyStream()
        {
            if (BodyStream == null)
                return null;
            if (BodyStream.Position > 0)
                BodyStream.Position = 0;
            return BodyStream;
        }
        /// <summary>
        /// Set the given value to body stream using <see cref="BinarySerializer"/>, This method is a part of <see cref="IMessageStream"/> implementation..
        /// </summary>
        /// <param name="value"></param>
        public void SetBody(object value)
        {

            if (value != null)
            {
                _TypeName = value.GetType().FullName;

                NetStream ns = new NetStream();
                var ser = new BinarySerializer();
                ser.Serialize(ns, value);
                ns.Position = 0;
                m_BodyStream = ns;
            }
            else
            {
                _TypeName = typeof(object).FullName;
                m_BodyStream = null;
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
                _TypeName = value.GetType().FullName;

                NetStream ns = new NetStream();
                var ser = new BinarySerializer();
                ser.Serialize(ns, value);
                ns.Position = 0;
                m_BodyStream = ns;
            }
            else
            {
                _TypeName = typeof(string).FullName;
                m_BodyStream = null;
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
            _TypeName = (type != null) ? type.FullName : typeof(object).FullName;
            if (value != null)
            {
                m_BodyStream = new NetStream(value);
            }
        }
        /// <summary>
        /// Deserialize body stream to object, This method is a part of <see cref="IMessageStream"/> implementation.
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
        /// <summary>
        ///  Deserialize body stream to generic object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetBody<T>()
        {
            return GenericTypes.Cast<T>(GetBody());
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
            return new Ptr(Identifier, 0, Destination);
        }

        public Ptr GetPtr(string hotName)
        {
            return new Ptr(Identifier, 0, hotName);
        }

        //public double Duration()
        //{
        //    return SentTime.Subtract(ArrivedTime).TotalSeconds;
        //}

        internal void SetArrived()
        {
            try
            {
                this.Modified = DateTime.Now;
                this.ArrivedTime = DateTime.Now;
                this.MessageState = Messaging.MessageState.Arrived;
                this.Identifier = UUID.NewUuid().ToString();

                SetHeader();

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

        internal void SetReceiving()
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

        public void DoRetry()
        {
            Retry++;
            this.Modified = DateTime.Now;
            SetHeader();
            //m_stream.Replace(Retry, offset + 24);
            //m_stream.Replace(Modified.Ticks, offset + 44);
        }
        #endregion

        #region util

        internal object[] ItemArray()
        {

            return new object[]{  MessageState,
            MessageType,
            Command,
            Priority,
            Identifier,
            Retry,
            ArrivedTime,
            Modified,
            Expiration,
            TransformType,
            Destination,
            Sender,
            Label,
            ItemBinary
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
            StringBuilder sb = new StringBuilder();
            sb.Append("QueueItem Print:");
            sb.AppendFormat("\r\n{0}", MessageState);
            sb.AppendFormat("\r\n{0}", MessageType);
            sb.AppendFormat("\r\n{0}", Command);
            sb.AppendFormat("\r\n{0}", Priority);
            sb.AppendFormat("\r\n{0}", Identifier);
            sb.AppendFormat("\r\n{0}", Retry);
            sb.AppendFormat("\r\n{0}", ArrivedTime);
            //sb.AppendFormat("\r\n{0}", SentTime);
            sb.AppendFormat("\r\n{0}", Modified);
            sb.AppendFormat("\r\n{0}", Expiration);
            //sb.AppendFormat("\r\n{0}", MessageId);
            sb.AppendFormat("\r\n{0}", TransformType);
            sb.AppendFormat("\r\n{0}", Destination);
            sb.AppendFormat("\r\n{0}", Sender);
            sb.AppendFormat("\r\n{0}", Label);
            return sb.ToString();
        }

        ///// <summary>
        ///// Get a copy of <see cref="QueueItemStream"/> as <see cref="IQueueItem"/>
        ///// </summary>
        ///// <returns></returns>
        //public Message GetMessage()
        //{
        //    if (ItemBinary == null)
        //        return null;
        //    var stream = new NetStream(ItemBinary);
        //    //var stream = GetItemStream();
        //    if (stream == null)
        //    {
        //        return null;
        //    }
        //    return new Message(stream.Copy(), null, MessageState);
        //}

        ///// <summary>
        ///// Get the item id of current item.
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <returns></returns>
        //public static Guid GetItemId(NetStream stream)
        //{
        //    byte[] b = stream.PeekBytes(7, 16);

        //    return new Guid(b);
        //}

        /// <summary>
        /// Get an instance of <see cref="QueueItemStream"/> from file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static QueueItem ReadFile(string filename)
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
            return new QueueItem(netStream, null);
        }

        /// <summary>
        /// Get an instance of <see cref="QueueItemStream"/> from file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static ReadFileState ReadFile(string filename, out IQueueItem item)
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
                QueueItem qitem = new QueueItem(netStream, null);

                item = qitem as IQueueItem;

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


    }


}
