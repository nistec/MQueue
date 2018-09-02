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

namespace Nistec.Messaging
{

 

    /// <summary>
    /// Represent a message stream for network communication like namedPipe or Tcp.
    /// This message can serialize/desrialize fast and easly using the <see cref="BinaryStreamer"/>
    /// </summary>
    [Serializable]
    public sealed class Message : IMessage,  ISerialEntity, /*IQueueAck,*/ IDisposable
    {

  
        #region static
        ///// <summary>
        ///// Deserialize stream to message.
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <returns></returns>
        //public static Message Deserialize(NetStream stream)
        //{
        //    return new BinarySerializer().Deserialize<Message>(stream);
        //}

        ///// <summary>
        ///// Serialize message as stream.
        ///// </summary>
        ///// <returns></returns>
        //public NetStream Serialize()
        //{
        //    return Serialize(true);
        //    //return new BinarySerializer().Serialize(this);
        //}

        
        /// <summary>
        /// Get the default formatter.
        /// </summary>
        public static Formatters DefaultFormatter { get { return Formatters.BinarySerializer; } }


        public static Message Ack(MessageState state, Exception ex)
        {
            return new Message()
            {
                Command = QueueCmd.Ack,
                Label = ex == null ? state.ToString() : ex.Message,
                MessageState = state
            };
        }

        public static Message Ack(MessageState state, string label)
        {
            return new Message()
            {
                Command = QueueCmd.Ack,
                Label = label,
                MessageState = state
            };
        }


        public static Message Create(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("Message.Load.stream");
            }
            if (stream is NetStream)
            {
                stream.Position = 0;
            }
            var msg = new Message();
            msg.EntityRead(stream, null);
            msg.Modified = DateTime.Now;
            return msg;
        }

        public static Message Create(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("Message.Load.XmlNode");
            }
            var msg = new Message();
            msg.Load(node);
            msg.Modified = DateTime.Now;
            return msg;
        }

        public static Message Create(DataRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("Message.Load.DataRow");
            }
            var dic = Nistec.Data.DataUtil.DataRowToHashtable(row);
            var msg = new Message();
            msg.Load(dic);
            msg.Modified = DateTime.Now;
            return msg;
        }

        public static Message Create(IDictionary dic)
        {
            if (dic == null)
            {
                throw new ArgumentNullException("Message.Load.IDictionary");
            }
            var msg = new Message();
            msg.Load(dic);
            msg.Modified = DateTime.Now;
            return msg;
        }

        public static Message Create(GenericRecord rcd)
        {
            if (rcd == null)
            {
                throw new ArgumentNullException("Message.Load.GenericRecord");
            }
            var msg = new Message();
            msg.Load(rcd);
            msg.Modified = DateTime.Now;
            return msg;
        }

        #endregion

        #region property

        public int Version { get { return 4022; } }

        /// <summary>
        /// Get MessageState
        /// </summary>
        public MessageState MessageState { get; internal set; }

        /// <summary>
        /// Get Command
        /// </summary>
        public QueueCmd Command { get; internal set; }

        /// <summary>
        /// Get or Set transformation type.
        /// </summary>
        public TransformTypes TransformType { get; set; }

        /// <summary>
        /// Get ItemId
        /// </summary>
        public Guid ItemId { get; internal set; }

        /// <summary>
        /// Get MessageId
        /// </summary>
        public int MessageId { get; set; }
        /// <summary>
        /// Get Priority
        /// </summary>
        public Priority Priority { get; set; }

        /// <summary>
        /// Get Retry
        /// </summary>
        public byte Retry { get; internal set; }

        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        public DateTime ArrivedTime { get; internal set; }
        /// <summary>
        /// Get SentTime
        /// </summary>
        public DateTime SentTime { get; internal set; }

        /// <summary>
        /// Get or Set timeout in minutes
        /// </summary>
        public int Expiration { get; set; }

        /// <summary>
        /// Get the last modified time.
        /// </summary>
        public DateTime Modified { get; internal set; }

        /// <summary>
        /// Get The message Destination\Queue name.
        /// </summary>
        public string Destination { get; internal set; }

        ///// <summary>
        ///// Get or Set The message Topic.
        ///// </summary>
        //public string Topic { get; set; }

        /// <summary>
        /// Get or Set The message Label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Get or Set The message Sender.
        /// </summary>
        public string Sender { get; internal set; }
        
        ///// <summary>
        ///// Gets or sets the content of the message.
        ///// An object that specifies the message contents. The object can be a string,
        ///// a date, a currency, a number, an array of bytes, or any managed object.
        ///// </summary>
        //public object Body { get; set; }

        /// <summary>
        /// Get or Set The Segments in body.
        /// </summary>
        public byte Segments { get; set; }
        /// <summary>
        /// Get or Set The serializer formatter.
        /// </summary>
        public Formatters Formatter { get; set; }

        string _TypeName;
        /// <summary>
        ///  Get The type name of body stream.
        /// </summary>
        public string TypeName
        {
            get { return _TypeName; }
        }
        NetStream m_BodyStream;
        /// <summary>
        /// Get or Set The message body stream.
        /// </summary>
        public NetStream BodyStream { get { return m_BodyStream; } }
        /// <summary>
        /// Get or Set Notify
        /// </summary>
        public string Notify { get; set; }
       

        #endregion

        #region extended properties

        public Ptr GetPtr()
        {
            return new Ptr(Identifier, 0, Destination);
        }
        public string Identifier
        {
            get { return Assists.GetIdentifier(ItemId, Priority); }
        }
        public string Filename
        {
            get { return Assists.GetFilename(Identifier); }
        }

        public string FolderId
        {
            get
            {
                return Assists.GetFolderId(Identifier);//Modified, Priority);
            }
        }

        #endregion

        #region MessageState settings
        internal void SetMessageState(MessageState state)
        {
            //System.Messaging.Message;
            //System.Messaging.MessageQueue;


            MessageState = state;
            Modified = DateTime.Now;
        }

        internal void SetReceived(QueueCmd type)
        {
            switch (type)
            {
                case QueueCmd.Peek:
                case QueueCmd.PeekItem:
                case QueueCmd.PeekPriority:
                    SetMessageState(MessageState.Peeked);
                    break;
                case QueueCmd.Dequeue:
                case QueueCmd.DequeueItem:
                case QueueCmd.DequeuePriority:
                    SetMessageState(MessageState.Received);
                    break;
            }
        }

        internal void SetArraived()
        {
            ItemId = UUID.NewUuid();
            MessageState = MessageState.Sending;
            Modified = DateTime.Now;
        }
        
        #endregion

        #region Header Stream

        internal NetStream HeaderStream { get; set; }

        /// <summary>
        /// Get header stream after set the position to first byte in buffer.
        /// </summary>
        /// <returns></returns>
        public NetStream GetHeaderStream()
        {
            if (HeaderStream == null)
                return null;
            if (HeaderStream.Position > 0)
                HeaderStream.Position = 0;
            return HeaderStream;
        }

        /// <summary>
        /// Set the given value to header stream using <see cref="BinarySerializer"/>.
        /// </summary>
        /// <param name="value"></param>
        public void SetHeader(GenericNameValue header)
        {

            if (header != null)
            {
                NetStream ns = new NetStream();
                var ser = new BinarySerializer();
                ser.Serialize(ns, header);
                ns.Position = 0;
                HeaderStream = ns;
            }
            else
            {
                HeaderStream = null;
            }
        }
        /// <summary>
        /// Deserialize header stream to object.
        /// </summary>
        /// <returns></returns>
        public GenericNameValue GetHeader()
        {
            if (HeaderStream == null)
                return null;
            HeaderStream.Position = 0;
            var ser = new BinarySerializer();
            return ser.Deserialize<GenericNameValue>(HeaderStream, true);
        }


        #endregion
 
        #region ctor
        /// <summary>
        /// Initialize a new instance of Message
        /// </summary>
        public Message()
        {
            MessageState = 0;
            ItemId = UUID.NewUuid();
            Modified = DateTime.Now;
            ArrivedTime = DateTime.Now;
            SentTime = DateTime.Now.AddMinutes(Expiration);
            //Command = 0;
            Priority = Messaging.Priority.Normal;
        }
        /// <summary>
        /// Initialize a new instance of Message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="destination"></param>
        /// <param name="body"></param>
        public Message(string sender, string destination, object body)
            : this()
        {
            Sender = sender;
            Destination = destination;
            SetBody(body);
        }

        /// <summary>
        /// Initialize a new instance of MessageStream from stream using for <see cref="ISerialEntity"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        /// <param name="state"></param>
        public Message(Stream stream, IBinaryStreamer streamer,MessageState state)
        {
            EntityRead(stream, streamer);
            Modified = DateTime.Now;
            MessageState = state;
        }

        /// <summary>
        /// Initialize a new instance of MessageStream from stream using for <see cref="ISerialEntity"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public Message(Stream stream, IBinaryStreamer streamer)
        {
            EntityRead(stream, streamer);
            Modified = DateTime.Now;
        }

        /// <summary>
        /// Initialize a new instance of MessageStream from <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="info"></param>
        public Message(SerializeInfo info, MessageState state)
        {
            ReadContext(info);
            Modified = DateTime.Now;
            MessageState = state;
        }

         public Message(MessageState state, string label)
            : this()
        {

            this.Command = QueueCmd.Ack;
            this.Label = label;
            this.MessageState = state;
        }
         public Message(MessageState state, string label,QueueCmd cmd)
             : this()
         {

             this.Command = cmd;
             this.Label = label;
             this.MessageState = state;
         }

        internal Message(NetStream stream)
        {
            EntityRead(stream, null);
        }
        internal Message(QueueItem item)
        {
            MessageState = this.MessageState;
            //MessageType = this.MessageType;
            Command = this.Command;
            Priority = this.Priority;
            ItemId = new Guid(this.Identifier);
            Retry = this.Retry;
            ArrivedTime = this.ArrivedTime;
            Modified = this.Modified;
            TransformType = this.TransformType;
            Label = this.Label;
            Destination = this.Destination;
            m_BodyStream = this.BodyStream.Copy();
        }
        void ClearData()
        {
            HeaderStream = null;
            m_BodyStream = null;
            _TypeName = null;
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
                
                Label = null;
                _TypeName = null;
                Sender = null;
                Destination = null;
                Notify = null;
                if (HeaderStream != null)
                {
                    HeaderStream.Dispose();
                    HeaderStream = null;
                }
                if (m_BodyStream != null)
                {
                    m_BodyStream.Dispose();
                    m_BodyStream = null;
                }
            }
            disposed = true;
        }
        #endregion

        #region EntityStreamState

        public static NetStream CreateStreamState(EntityStreamState state)
        {
            if (state == EntityStreamState.None)
            {
                throw new ArgumentException("EntityStreamState.None is not valid");
            }
            return new NetStream(new byte[] { (byte)state });
        }

        public static bool IsStreamHasState(NetStream stream)
        {
            return stream.Length == 1 && stream.PeekByte(0) > 200;
        }

        public static EntityStreamState GetStreamState(NetStream stream)
        {
            if (stream == null || stream.Length == 0)
                return EntityStreamState.None;
            if (stream.Length == 1 && stream.PeekByte(0) > 200)
                return (EntityStreamState)stream.PeekByte(0);
            return EntityStreamState.None;
        }

        public bool IsStreamHasState()
        {
            if (BodyStream == null || BodyStream.Length == 0)
                return false;
            return BodyStream.Length == 1 && BodyStream.PeekByte(0) > 200;
        }

        public void SetEntityStreamState(EntityStreamState state)
        {
            if (state == EntityStreamState.None)
            {
                throw new ArgumentException("EntityStreamState.None is not valid");
            }
            m_BodyStream = new NetStream(new byte[] { (byte)state });
        }

        #endregion

        #region methods

        

        int GetSize()
        {
            if (BodyStream == null)
                return 0;// GetInternalSize();
            return BodyStream.iLength;
        }

        /// <summary>
        /// Get Body Size in bytes
        /// </summary>
        public int Size
        {
            get { return GetSize(); }
        }


        /// <summary>
        /// Get indicate wether the item is empty 
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return m_BodyStream == null || BodyStream.Length == 0;
            }
        }

        /// <summary>
        /// Get Type of body
        /// </summary>
        public Type BodyType
        {
            get
            {
                return  SerializeTools.GetQualifiedType(TypeName);
            }
        }
        /// <summary>
        /// Get indicate wether the current body type is a known object type.
        /// </summary>
        public bool IsKnownType
        {
            get
            {
                return !string.IsNullOrEmpty(TypeName) && BodyType != null && !typeof(object).Equals(BodyType);
            }
        }

        
        #endregion

        #region Headers
        ///// <summary>
        ///// Create arguments helper.
        ///// </summary>
        ///// <param name="keyValues"></param>
        ///// <returns></returns>
        //public static GenericNameValue CreateHeaders(params string[] keyValues)
        //{
        //    if (keyValues == null)
        //        return null;
        //    GenericNameValue args = new GenericNameValue(keyValues);
        //    return args;
        //}
        ///// <summary>
        ///// Get or create a collection of arguments.
        ///// </summary>
        ///// <returns></returns>
        //public GenericNameValue GetHeaders()
        //{
        //    if (Headers == null)
        //        return new GenericNameValue();
        //    return Headers;
        //}
        #endregion

        #region Convert
        /// <summary>
        /// Convert body to base 64 string.
        /// </summary>
        /// <returns></returns>
        public string ConvertToBase64()
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

            //streamer.WriteFixedString(MessageContext.MMSG, 4);



            streamer.WriteValue((byte)MessageState);
            streamer.WriteValue((byte)Command);
            streamer.WriteValue((byte)Priority);
            streamer.WriteValue(ItemId);
            streamer.WriteValue(Retry);
            streamer.WriteValue(ArrivedTime);
            streamer.WriteValue(SentTime);
            streamer.WriteValue(Modified);
            streamer.WriteValue(Expiration);
            streamer.WriteValue(MessageId);
            streamer.WriteValue((byte)TransformType);

            streamer.WriteString(Destination);
            streamer.WriteString(Label);
            streamer.WriteString(Sender);
            //streamer.WriteString(Topic);
            streamer.WriteValue(HeaderStream);

            if (Command != QueueCmd.Ack)
            {
                streamer.WriteValue((int)Formatter);
                streamer.WriteValue(BodyStream);
                streamer.WriteString(TypeName);
                streamer.WriteValue(Segments);
                streamer.WriteString(Notify);
            }
            
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

            //string mmsg=  streamer.ReadFixedString();
            //if (mmsg != MessageContext.MMSG)
            //{
            //    throw new Exception("Incorrect message format");
            //}
            //streamer.MapperBegin();
            MessageState = (MessageState)streamer.ReadValue<byte>();
            Command = (QueueCmd)streamer.ReadValue<byte>();
            Priority = (Priority)streamer.ReadValue<byte>();
            ItemId = streamer.ReadValue<Guid>();
            Retry = streamer.ReadValue<byte>();
            ArrivedTime = streamer.ReadValue<DateTime>();
            SentTime = streamer.ReadValue<DateTime>();
            Modified = streamer.ReadValue<DateTime>();
            Expiration = streamer.ReadValue<int>();
            MessageId = streamer.ReadValue<int>();
            TransformType = (TransformTypes)streamer.ReadValue<byte>();

            Destination = streamer.ReadString();
            Label = streamer.ReadString();
            Sender = streamer.ReadString();
            //Topic = streamer.ReadString();
            HeaderStream = (NetStream)streamer.ReadValue();

            //var map = streamer.GetMapper();
            //Console.WriteLine(map);
            if (Command != QueueCmd.Ack)
            {
                Formatter = (Formatters)streamer.ReadValue<int>();
                m_BodyStream = (NetStream)streamer.ReadValue();
                _TypeName = streamer.ReadString();
                Segments = streamer.ReadValue<byte>();

                //HostType = streamer.ReadValue<byte>();
                Notify = streamer.ReadString();

                //Command = streamer.ReadString();
                //IsDuplex = streamer.ReadValue<bool>();
                //HeaderStream = (GenericNameValue)streamer.ReadValue();
            }
        }

        /// <summary>
        /// Write the current object include the body and properties to <see cref="ISerializerContext"/> using <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="context"></param>
        public void WriteContext(ISerializerContext context)
        {
            SerializeInfo info = new SerializeInfo();

            info.Add("MessageState", (byte)MessageState);
            info.Add("Command", (byte)Command);
            info.Add("Priority", (byte)Priority);
            info.Add("ItemId", ItemId);
            info.Add("Retry", Retry);
            info.Add("ArrivedTime", ArrivedTime);
            info.Add("SentTime", SentTime);
            info.Add("Modified", Modified);
            info.Add("Expiration", Expiration);
            info.Add("MessageId", MessageId);
            info.Add("TransformType", (byte)TransformType);
            info.Add("Destination", Destination);
            info.Add("Label", Label);
            info.Add("Sender", Sender);
            //info.Add("Topic", Topic);
            info.Add("Headers", HeaderStream);

            if (Command != QueueCmd.Ack)
            {
                info.Add("Formatter", (int)Formatter);
                info.Add("BodyStream", BodyStream);
                info.Add("TypeName", TypeName);
                info.Add("Segments", Segments);

                //info.Add("HostType", HostType);
                info.Add("Notify", Notify);

                //info.Add("Command", Command);
                //info.Add("IsDuplex", IsDuplex);
                //info.Add("Headers", GetHeaders());
            }
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

            MessageState =(MessageState) info.GetValue<byte>("MessageState");
            Command = (QueueCmd)info.GetValue<byte>("Command");
            Priority = (Priority)info.GetValue<byte>("Priority");
            ItemId = info.GetValue<Guid>("ItemId");
            Retry = info.GetValue<byte>("Retry");
            ArrivedTime = info.GetValue<DateTime>("ArrivedTime");
            SentTime = info.GetValue<DateTime>("SentTime");
            Modified = info.GetValue<DateTime>("Modified");
            Expiration = info.GetValue<int>("Expiration");
            MessageId = info.GetValue<int>("MessageId");
            TransformType = (TransformTypes)info.GetValue<byte>("TransformType");
            Destination = info.GetValue<string>("Destination");
            Label = info.GetValue<string>("Label");
            Sender = info.GetValue<string>("Sender");
            //Topic = info.GetValue<string>("Topic");
            HeaderStream = (NetStream)info.GetValue("HeaderStream");

            if (Command != QueueCmd.Ack)
            {
                Formatter = (Formatters)info.GetValue<int>("Formatter");
                m_BodyStream = (NetStream)info.GetValue("BodyStream");
                _TypeName = info.GetValue<string>("TypeName");
                Segments = info.GetValue<byte>("Segments");

                //HostType = info.GetValue<byte>("HostType");
                Notify = info.GetValue<string>("Notify");

                //Command = info.GetValue<string>("Command");
                //IsDuplex = info.GetValue<bool>("IsDuplex");
                //_Headers = (GenericNameValue)info.GetValue("Headers");
            }

        }
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

        /// <summary>
        /// Set the given array of values to body stream using <see cref="BinarySerializer"/>, This method is a part of <see cref="IMessageStream"/> implementation..
        /// </summary>
        /// <param name="value"></param>
        public void SetBodyWithSegments(object[] value)
        {

            if (value != null)
            {
                _TypeName = value.GetType().FullName;
                Segments = (byte)value.Length;
                NetStream ns = new NetStream();
                var ser = new BinarySerializer();
                ser.Serialize(ns, value, typeof(object[]));
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
            return ser.Deserialize(BodyStream,true);
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
            writer.Serialize(stream,entity);
            //writer.Flush();
        }

        #endregion

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


       

        /// <summary>
        /// Write the message include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void MessageWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteValue((byte)SerialContextType.SerialEntityType);
            EntityWrite(stream,streamer);
        }



        /// <summary>
        /// Write the message include the body and properties to <see cref="XmlTextWriter"/>.
        /// </summary>
        /// <param name="context"></param>
        public void MessageWrite(XmlTextWriter wr)
        {

            // Write table start
            wr.WriteStartElement("Message");
            wr.WriteRaw("\r\n");
                       
            WriteInfo(wr);
            WriteHeader(wr);
            WriteBody(wr);
                        
            wr.WriteEndElement();
            wr.Flush();

        }

               
        void Write(XmlTextWriter wr, string name, object value)
        {

            wr.WriteRaw("\t");
            wr.WriteStartElement(name);
            wr.WriteValue(value.ToString());
            wr.WriteEndElement();
            wr.WriteRaw("\r\n");
        }


        /// <summary>
        /// Serializes the message body using the specified System.Xml.XmlWriter.
        /// </summary>
        /// <param name="wr"></param>
        public void WriteHeader(XmlTextWriter wr)
        {
            if (HeaderStream  != null)// && _Headers.Count > 0)
            {
                var header=GetHeader();
                if (header.Count > 0)
                {
                    wr.WriteStartElement("Header");
                    wr.WriteRaw("\r\n");

                    foreach (var h in header)
                    {
                        Write(wr, h.Key, h.Value);
                    }

                    // Write table end
                    wr.WriteEndElement();
                }
            }
            else
            {
                wr.WriteRaw("<Header/>");
            }
            wr.WriteRaw("\r\n");
        }

        /// <summary>
        /// Serializes the message body using the specified System.Xml.XmlWriter.
        /// </summary>
        /// <param name="wr"></param>
        public void WriteInfo(XmlTextWriter wr)
        {
            wr.WriteStartElement("Info");
            wr.WriteRaw("\r\n");

            Write(wr, "MessageState", (byte)MessageState);
            Write(wr, "Command", (byte)Command);
            Write(wr, "Priority", (byte)Priority);
            Write(wr, "ItemId", ItemId);
            Write(wr, "Retry", Retry);
            Write(wr, "ArrivedTime", ArrivedTime);
            Write(wr, "SentTime", SentTime);
            Write(wr, "Modified", Modified);
            Write(wr, "Expiration", Expiration);
            Write(wr, "MessageId", MessageId);
            Write(wr, "TransformType", TransformType);

            Write(wr, "Destination", Destination);
            Write(wr, "Label", Label);
            Write(wr, "Sender", Sender);
            //Write(wr, "Topic", Topic);

            Write(wr, "Formatter", (int)Formatter);
            //Write(wr, "BodyStream", BodyStream);
            Write(wr, "TypeName", TypeName);
            Write(wr, "Segments", Segments);

            //Write(wr, "HostType", HostType);
            Write(wr, "Notify", Notify);

            //Write(wr, "Command", Command);
            //Write(wr, "IsDuplex", IsDuplex);
            //Write(wr, "Headers", _Headers);


            // Write table end
            wr.WriteEndElement();
            wr.WriteRaw("\r\n");
        }
        /// <summary>
        /// Serializes the message body using the specified System.Xml.XmlWriter.
        /// </summary>
        /// <param name="wr"></param>
        public void WriteBody(XmlTextWriter wr)
        {

            object obj = GetBody();
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
                        case "BodyStream":
                            ReadBody(xr.ReadSubtree()); break;
                        case "Headers":
                            ReadHeaders(xr.ReadSubtree()); break;
                        default:
                            SetValue(xr.Name, xr.ReadString());break;
                    }

                    /*
                    switch (xr.Name)
                    {
                        case "MessageState":
                             _MessageState = Types.ToByte(xr.ReadString(),0);break;
                        case "Command":
                            Command = (Command)Types.ToByte(xr.ReadString(),0); break;
                        case "Priority":
                            Priority = (Priority)Types.ToInt(xr.ReadString(), 0); break;
                        case "ItemId":
                            _ItemId =  Types.ToGuid(xr.ReadString()); break;
                        case "Retry":
                            Retry = Types.ToInt(xr.ReadString(), 0); break;
                        case "ArrivedTime":
                            ArrivedTime = Types.ToDateTime(xr.ReadString()); break;
                        case "SentTime":
                            SentTime = Types.ToDateTime(xr.ReadString()); break;
                        case "Modified":
                            Modified = Types.ToDateTime(xr.ReadString()); break;
                        case "Expiration":
                            Expiration = Types.ToInt(xr.ReadString()); break;
                        case "MessageId":
                            MessageId = Types.ToInt(xr.ReadString()); break;

                        case "Formatter":
                            Formatter = (Formatters)Types.ToInt(xr.ReadString()); break;
                        case "BodyStream":
                            ReadBody(xr.ReadSubtree());break;
                        case "TypeName":
                            _TypeName = xr.ReadString(); break;
                        case "Segments":
                            Segments = Types.ToByte(xr.ReadString(), 0); break;

                        case "Sender":
                            Sender = xr.ReadString(); break;
                        case "Destination":
                            Destination = xr.ReadString(); break;
                        case "DestinationType":
                            DestinationType = Types.ToByte(xr.ReadString(),0); break;
                        case "Notify":
                            Notify = xr.ReadString(); break;

                        case "Label":
                            Label = xr.ReadString(); break;
                        case "Command":
                            Command = xr.ReadString(); break;
                        case "IsDuplex":
                            IsDuplex = Types.ToBool(xr.ReadString(),false); break;
                        case "Headers":
                            ReadHeaders(xr.ReadSubtree());break;
                            //_Headers = (GenericNameValue)xr.ReadValue(); break;
                        //id = reader.ReadString();
                    }
                    */
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


        internal void Load(IDictionary dic)
        {
            if (dic == null)
            {
                throw new ArgumentNullException("Message.Load.IDictionary");
            }
            ClearData();

            foreach (DictionaryEntry n in dic)
            {
                SetValue(n.Key.ToString(),n.Value);
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
                SetValue(node.Name, node.InnerText);
            }
        }

        /// <summary>
        /// Serializes the message body using the specified System.Xml.XmlWriter.
        /// </summary>
        /// <param name="wr"></param>
        void SetValue(string name, object value)
        {

            switch (name)
            {
                case "MessageState":
                    MessageState =(MessageState) Types.ToByte(value, 0); break;
                case "Command":
                    Command = (QueueCmd)Types.ToByte(value, 0); break;
                case "Priority":
                    Priority = (Priority)Types.ToByte(value, 2); break;
                case "ItemId":
                    ItemId = Types.ToGuid(value); break;
                case "Retry":
                    Retry = Types.ToByte(value, 0); break;
                case "ArrivedTime":
                    ArrivedTime = Types.ToDateTime(value); break;
                case "SentTime":
                    SentTime = Types.ToDateTime(value); break;
                case "Modified":
                    Modified = Types.ToDateTime(value); break;
                case "Expiration":
                    Expiration = Types.ToInt(value); break;
                case "MessageId":
                    MessageId = Types.ToInt(value); break;
                case "TransformType":
                    TransformType =(TransformTypes) Types.ToByte(value,0); break;
                case "Destination":
                    Destination = Types.NZ(value, ""); break;
                case "Label":
                    Label = Types.NZ(value, ""); break;
                case "Sender":
                    Sender = Types.NZ(value, ""); break;
                //case "Topic":
                //    Topic = Types.NZ(value, ""); break;
                case "HeaderStream":
                    SetHeader((GenericNameValue)value);
                    break;

                case "Formatter":
                    Formatter = (Formatters)Types.ToInt(value); break;
                case "BodyStream":
                    SetBodyInternal(value,_TypeName); 
                    break;
                case "TypeName":
                    _TypeName = Types.NZ(value,""); break;
                case "Segments":
                    Segments = Types.ToByte(value, 0); break;
                //case "HostType":
                //    HostType = Types.ToByte(value, 0); break;
                case "Notify":
                    Notify = Types.NZ(value,""); break;
                //case "Command":
                //    Command = Types.NZ(value,""); break;
                //case "IsDuplex":
                //    IsDuplex = Types.ToBool(value, false); break;
                //case "Headers":
                // _Headers = (GenericNameValue)value; 
                 //break;
            }
        }


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

        #endregion

        #region ExecuteTask
        /// <summary>
        /// Execute async task request and return the response as<see cref="NetStream"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public NetStream AsyncTask(Func<IMessageStream> action)
        {
            using (Task<IMessageStream> task = Task.Factory.StartNew<IMessageStream>(action))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result == null)
                        return null;
                    return task.Result.BodyStream;
                }
            }
            return null;
        }
        /// <summary>
        /// Execute async task request and return the response as<see cref="NetStream"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="formatter"></param>
        /// <returns></returns>
        public NetStream AsyncTask(Func<ISerialEntity> action, Formatters formatter)
        {
            using (Task<ISerialEntity> task = Task.Factory.StartNew<ISerialEntity>(action))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result == null)
                        return null;
                    if (formatter == Formatters.BinaryFormatter)
                    {
                        NetStream ns = new NetStream();
                        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ser = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        ser.Serialize(ns, task.Result);
                        return ns;
                    }

                    task.Result.Serialize();//.GetEntityStream(true);
                }
            }
            return null;
        }
        /// <summary>
        /// Execute async one way task request.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="stream"></param>
        public void AsyncTask(Func<ISerialEntity> action, Stream stream)
        {
            using (Task<ISerialEntity> task = Task.Factory.StartNew<ISerialEntity>(action))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result == null)
                        return;
                    task.Result.EntityWrite(stream,null);//..GetEntityStream();
                }
            }
            //return null;
        }

        /// <summary>
        /// Execute async task request and return the response as<see cref="NetStream"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public NetStream AsyncTask(Func<object> action)
        {
            using (Task<object> task = Task.Factory.StartNew<object>(action))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result == null)
                        return null;
                    var ns = new NetStream();
                    var ser = new BinarySerializer();
                    ser.Serialize(ns, task.Result);
                    return ns;
                }
            }
            return null;
        }

        public void AsyncTask(Action action)
        {
            using (Task task = Task.Factory.StartNew(action))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                }
            }
        }


        public NetStream AsyncTask(Func<byte[]> action)
        {
            using (Task<byte[]> task = Task.Factory.StartNew<byte[]>(action))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result == null)
                        return null;
                    return new NetStream(task.Result);
                }
            }
            return null;
        }
        #endregion

        public void DoRetry()
        {
            Retry++;
        }

        
        public Message Copy()
        {
            return Copy(true, true);
        }
        public Message Copy(bool isNew,bool copyBody, string destination = null)
        {
            Message msg = new Message()
            {
                MessageState = MessageState,
                Command = Command,
                Priority = Priority,
                ItemId =isNew? Guid.NewGuid(): ItemId,
                Retry = Retry,
                ArrivedTime = ArrivedTime,
                SentTime = SentTime,
                Modified = Modified,
                Expiration = Expiration,
                MessageId = MessageId,
                TransformType = TransformType,
                Destination = destination ?? Destination,
                Label = Label,
                Sender = Sender,
                //Topic = Topic,
                HeaderStream =HeaderStream,
                Formatter = Formatter,

                Segments = Segments,

                //HostType = HostType,
                Notify = Notify,

                //Command = Command,
                //IsDuplex = IsDuplex,
                //_Headers = Headers
            };

            if (copyBody)
            {
                msg.m_BodyStream = BodyStream;
                msg._TypeName = TypeName;
            }

            return msg;

        }

        #region chunk methods

        public void Validate()
        {
            if (BodyStream == null)
                throw new MessageException(MessageState.InvalidMessageBody, "Invalid Body");
            if (Destination == null)
                throw new MessageException(MessageState.InvalidMessageHost, "Invalid Host");
        }

        /*
        public void SetDestination(params string[] dests)
        {
            if (dests == null)
            {
                throw new ArgumentNullException("dests");
            }
            Destination = dests.JoinTrim("|");
        }

        internal string[] GetHostNames()
        {
            if (Destination == null)
                return null;
            return Destination.SplitTrim('|');
        }
        */

        internal object[] SplitBody()
        {
            if (BodyStream == null)
                throw new MessageException(MessageState.InvalidMessageBody, "Invalid Body");

            var ser = new BinarySerializer();
            object val = ser.Deserialize(BodyStream);
            if (val == null || !(val is object[]))
            {
                throw new Exception("Body items is null");
            }

            object[] values = (object[])val;

            if (values == null)
            {
                throw new Exception("Body items is null");
            }
            if (values.Length != Segments)
            {
                throw new Exception("Segments dos not match the Body items");
            }
            return values;
        }

        public Message[] SplitMessages()
        {
            if (BodyStream == null)
                throw new MessageException(MessageState.InvalidMessageBody, "Invalid Body");

            if (Segments > 1)
            {

                object[] values = SplitBody();

                List<Message> list = new List<Message>();
                for (int i = 0; i < values.Length; i++)
                {
                    Message msg = this.Copy(true, false);
                    msg.Command = QueueCmd.QueueItem;
                    msg.SetBody(values[i]);
                    list.Add(msg);
                }
                return list.ToArray();
            }
            else
            {
                return new Message[] { this };
            }
        }

        //public QueueItemStream[] SplitQueueItems()
        //{
        //    if (BodyStream == null)
        //        throw new MessageException(MessageState.InvalidMessageBody, "Invalid Body");

        //    if (Segments > 1)
        //    {
        //        object[] values = SplitBody();

        //        List<QueueItemStream> list = new List<QueueItemStream>();
        //        for (int i = 0; i < values.Length; i++)
        //        {
        //            Message msg = this.Copy(true,false);
        //            msg.Command = QueueCmd.QueueItem;
        //            msg.SetBody(values[i]);
        //            list.Add(new QueueItemStream(msg, QueueCmd.SplitToQueue));
        //        }
        //        return list.ToArray();
        //    }
        //    else
        //    {
        //        return new QueueItemStream[] { new QueueItemStream(this,QueueCmd.SplitToQueue) };
        //    }
        //}


        public string Print()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("QueueItem Print:");
            sb.AppendFormat("\r\n{0}", MessageState);
            sb.AppendFormat("\r\n{0}", Command);
            sb.AppendFormat("\r\n{0}", Priority);
            sb.AppendFormat("\r\n{0}", ItemId);
            sb.AppendFormat("\r\n{0}", Retry);
            sb.AppendFormat("\r\n{0}", ArrivedTime);
            sb.AppendFormat("\r\n{0}", SentTime);
            sb.AppendFormat("\r\n{0}", Modified);
            sb.AppendFormat("\r\n{0}", Expiration);
            sb.AppendFormat("\r\n{0}", MessageId);
            sb.AppendFormat("\r\n{0}", TransformType);
            sb.AppendFormat("\r\n{0}", Destination);
            sb.AppendFormat("\r\n{0}", Label);
            sb.AppendFormat("\r\n{0}", Sender);
            return sb.ToString();
        }


        #endregion


        /// <summary>
        /// Get an instance of <see cref="QueueItemStream"/> from file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Message ReadFile(string filename)
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
            return new Message(netStream, null);
        }

        /// <summary>
        /// Get an instance of <see cref="QueueItemStream"/> from file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static ReadFileState ReadFile(string filename, out Message item)
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
                item = new Message(netStream, null);

                //item = qitem as IQueueItem;

                return ReadFileState.Completed;
            }
            catch (IOException ioex)
            {
                //Netlog.Exception("ReadFile IOException ", ioex);
                string err = ioex.Message;
                item = null;
                return ReadFileState.IOException;
            }
            catch (Exception ex)
            {
                //Netlog.Exception("ReadFile Exception ", ex);
                string err = ex.Message;
                item = null;
                return ReadFileState.Exception;
            }
        }
        public void SaveToFile(string filename)
        {
            if (BodyStream == null)
            {
                throw new Exception("Invalid BodyStream , Can't save body stream to file,");
            }
            var stream = ToStream();

            //string filename = GetPtrLocation(location);

            stream.Copy().SaveToFile(filename);
            //BodyStream.Position = 0;
            //return filename;
        }

    }
}
