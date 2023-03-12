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

    public class QueueItem : MessageStream, IQueueItem, ICloneable, IDisposable, IQueueAck//, ISerialEntity
    {
        #region static
        public static QueueItem Create(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("QueueItem.Load.stream");
            }
            if (stream is NetStream)
            {
                stream.Position = 0;
            }
            var msg = new QueueItem();
            msg.EntityRead(stream, null);
            msg.Modified = DateTime.Now;
            return msg;
        }

        public static QueueItem Create(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("QueueItem.Load.XmlNode");
            }
            var msg = new QueueItem();
            msg.Load(node);
            msg.Modified = DateTime.Now;
            return msg;
        }

        public static QueueItem Create(DataRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("QueueItem.Load.DataRow");
            }
            var dic = Nistec.Data.DataUtil.DataRowToHashtable(row);
            var msg = new QueueItem();
            msg.Load(dic);
            msg.Modified = DateTime.Now;
            return msg;
        }

        public static QueueItem Create(IDictionary dic)
        {
            if (dic == null)
            {
                throw new ArgumentNullException("QueueItem.Load.IDictionary");
            }
            var msg = new QueueItem();
            msg.Load(dic);
            msg.Modified = DateTime.Now;
            return msg;
        }

        public static QueueItem Create(NameValueCollection nvc)
        {
            if (nvc == null)
            {
                throw new ArgumentNullException("QueueItem.Load.NameValueCollection");
            }
            var msg = new QueueItem();
            msg.Load(nvc);
            msg.Modified = DateTime.Now;
            return msg;
        }
        public static QueueItem Create(GenericRecord rcd)
        {
            if (rcd == null)
            {
                throw new ArgumentNullException("QueueItem.Load.GenericRecord");
            }
            var msg = new QueueItem();
            msg.Load(rcd);
            msg.Modified = DateTime.Now;
            return msg;
        }
        public static QueueItem Ack(MessageState state, QueueCmd cmd)
        {
            return new QueueItem()
            {
                QCommand = cmd,
                Label = state.ToString(),
                MessageState = state
            };
        }

        public static QueueItem Ack(MessageState state, QueueCmd cmd, Exception ex)
        {
            return new QueueItem()
            {
                QCommand = cmd,
                Label = ex == null ? state.ToString() : ex.Message,
                MessageState = state
            };
        }

        public static QueueItem Ack(MessageState state, QueueCmd cmd, string label, string identifier)
        {
            return new QueueItem()
            {
                QCommand = cmd,
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
                Host = item.Host,
                Identifier = identifier,
                Modified = DateTime.Now,
                Creation = item.Creation,
                Priority = item.Priority,
                MessageType = item.MessageType,
                Sender = item.Sender,
                TransformType = item.TransformType,
                Retry = (byte)retry,
                Label = label
            };
        }


        #endregion

        #region Load

        void ClearData()
        {

            BodyStream = null;
            TypeName = null;
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
                    QCommand = (QueueCmd)Types.ToByte(value, 0); break;
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
                case "Modified":
                    Modified = Types.ToDateTime(value); break;
                case "Duration":
                    Duration = Types.ToInt(value); break;
                //case "MessageId":
                //    MessageId = Types.ToInt(value); break;
                //case "TransformType":
                //    TransformType = (TransformTypes)Types.ToByte(value, 0); break;
                case "TransformType":
                    TransformType = (TransformType)Types.ToByte(value, 0); break;
                case "Expiration":
                    Expiration = Types.ToInt(value, 0); break;
                case "IsDuplex":
                    IsDuplex = Types.ToBool(value, false); break;
                case "Host":
                    Host = Types.NZ(value, ""); break;
                case "Label":
                    Label = Types.NZ(value, ""); break;
                case "Sender":
                    Sender = Types.NZ(value, ""); break;
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
                case "EncodingName":
                    EncodingName = Types.NZ(value, DefaultEncoding); break;
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

        public QueueItem Copy()
        {
            var copy = new QueueItem()
            {
                MessageState = this.MessageState,
                MessageType = this.MessageType,
                Command = this.Command,
                Priority = this.Priority,
                Identifier = this.Identifier,
                Retry = this.Retry,
                ArrivedTime = this.ArrivedTime,
                Creation = this.Creation,
                Modified = this.Modified,
                TransformType = this.TransformType,
                Host = this.Host,
                Label = this.Label,
                Sender = this.Sender,

                BodyStream = this.BodyStream.Copy(),
                TypeName = this.TypeName,
                EncodingName=this.EncodingName
                //Header = this.Header,
                //ItemBinary = this.ItemBinary
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
        public QueueItem()
        {
            Identifier = Ptr.NewIdentifier();
            Priority = Priority.Normal;
            Creation = DateTime.Now;
            Modified = DateTime.Now;
            ArrivedTime = Assists.NullDate;
            Version = QueueDefaults.CurrentVersion;
            IsDuplex = true;
            MessageType = MQTypes.Message;
            EncodingName = DefaultEncoding;
        }

        //public QueueItem(QueueCmd command, TransformTypes transformType, Priority priority, string destination, string json)
        //{
        //    MessageType = MQTypes.MessageRequest;
        //    Command = command;
        //    Priority = priority;
        //    TransformType = transformType;
        //    Host = destination;
        //    if (json != null)
        //        m_BodyStream = new NetStream(Encoding.UTF8.GetBytes(json));
        //    Creation = DateTime.Now;
        //    Modified = DateTime.Now;
        //    SetArrived();

        //    //ItemBinary = Encoding.UTF8.GetBytes(json);
        //    //m_BodyStream = new NetStream(Body);
        //}

        //public QueueItem(QueueItem message, byte[] body, Type type)
        //{

        //    Message msg = message.ToMessage();
        //    msg.SetBody(body,type);

        //    Command = message.Command;
        //    Priority = message.Priority;
        //    TransformType = message.TransformType;
        //    Label = message.Label;
        //    Host = message.Host;
        //    m_BodyStream = new NetStream(body);

        //    //NetStream ns = new NetStream();
        //    //msg.EntityWrite(ns,null);
        //    //ItemBinary = ns.ToArray();


        //    //MessageType = MQTypes.MessageRequest;
        //    //Command = message.Command;
        //    //Priority = message.Priority;
        //    //TransformType = message.TransformType;
        //    //Host = message.Host;
        //    //m_BodyStream = new NetStream(body);
        //    ////ArrivedTime = DateTime.Now;
        //    //SetArrived();
        //    //SetItemBinary();

        //    //NetStream ns = new NetStream();
        //    //this.EntityWrite(ns,null);

        //    //message.EntityWrite(ns);
        //    //m_BodyStream = ns;
        //    //ItemBinary = body;// Encoding.UTF8.GetBytes(body);
        //}

        public QueueItem(QueueRequest message)
        {
            Version = message.Version;
            MessageType = MQTypes.MessageRequest;
            QCommand = message.QCommand;
            Priority = message.Priority;
            TransformType = message.TransformType;
            Host = message.Host;
            Creation = message.Creation;
            Modified = DateTime.Now;
            ArrivedTime = Assists.NullDate;
            //m_BodyStream = null;
            BodyStream = null;
            EncodingName = message.EncodingName;
        }
               
        //public QueueItem(Message message)
        //{
        //    MessageType = MQTypes.Message;
        //    Command = message.Command;
        //    Priority = message.Priority;
        //    TransformType = message.TransformType;
        //    Label = message.Label;
        //    Host = message.Host;
        //    Creation = message.Creation;
        //    Modified = DateTime.Now;
        //    m_BodyStream = message.BodyStream;
        //    //ArrivedTime = DateTime.Now;
        //    SetArrived();

        //    NetStream ns = new NetStream();
        //    message.EntityWrite(ns,null);
        //    //m_BodyStream = ns;
        //    ItemBinary = ns.ToArray();
        //}

        //public QueueItem(QueueAck message)
        //{
        //    MessageType = MQTypes.Ack;
        //    Command = QueueCmd.Ack;
        //    //Priority = message.Priority;
        //    //TransformType = message.TransformType;
        //    Host = message.Host;
        //    Label = message.Label;
        //    Identifier = message.Identifier;
        //    MessageState = message.MessageState;
        //    Creation = message.Creation;

        //    //ArrivedTime = DateTime.Now;
        //    SetArrived();

        //    //NetStream ns = new NetStream();
        //    //message.EntityWrite(ns);
        //    ////m_BodyStream = ns;
        //    //ItemBinary = ns.ToArray();
        //}

        /// <summary>
        /// Initialize a new instance of MessageStream from stream using for <see cref="ISerialEntity"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        /// <param name="state"></param>
        internal QueueItem(Stream stream, IBinaryStreamer streamer, MessageState state)
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
        public QueueItem(Stream stream, IBinaryStreamer streamer)
        {
            EntityRead(stream, streamer);
        }

        /// <summary>
        /// Initialize a new instance of MessageStream from <see cref="SerializeInfo"/>.
        /// </summary>
        /// <param name="info"></param>
        public QueueItem(SerializeInfo info)
        {
            ReadContext(info);
        }

        public QueueItem(IPersistQueueItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("QueueItem.QueuePersistItem");
            }
            EntityRead(new NetStream(item.ItemBinary),null);

            //item.Header = Header;
            //if (item.Body != null)
            //    m_BodyStream = new NetStream(item.Body);
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
                Host = null;
                //_ItemBinary = null;
                //_Header = null;

                if (m_BodyStream != null)
                {
                    m_BodyStream.Dispose();
                    m_BodyStream = null;
                }
            }
            disposed = true;
        }
        */
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Host = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region property

        public int Version { get; set; }

        /// <summary>
        /// Get ItemId
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Get MessageState
        /// </summary>
        public MessageState MessageState { get; set; }


        QueueCmd _QCommand;
        /// <summary>
        /// Get Command
        /// </summary>
        public QueueCmd QCommand {
            get { return _QCommand; }
            set {
                _QCommand = value;
                Command = value.ToString();
            }
        }

        /// <summary>
        /// Get or Set message type.
        /// </summary>
        public MQTypes MessageType { get; set; }

        /// <summary>
        /// Get or set Priority
        /// </summary>
        public Priority Priority { get; set; }
        
        /// <summary>
        /// Get or set The message Host\Queue name.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Get or set The message Destination.
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// Get or set The Channel ID.
        /// </summary>
        public int ChannelId { get; set; }
        /// <summary>
        /// Get or set The AccountId.
        /// </summary>
        public int AccountId { get; set; }
        /// <summary>
        /// Get or set The MessageId.
        /// </summary>
        public int MessageId { get; set; }
        ///// <summary>
        ///// Get or set The BatchId.
        ///// </summary>
        //public int BatchId { get; set; }
        ///// <summary>
        ///// Get or set The OperationId.
        ///// </summary>
        //public int OperationId { get; set; }

        /// <summary>
        /// Get or set The Amount.
        /// </summary>
        public decimal Amount { get; set; }

        //NetStream m_BodyStream;
        ///// <summary>
        ///// Get or Set The message body stream.
        ///// </summary>
        //public NetStream BodyStream { get { return m_BodyStream; } }// set { m_BodyStream = value; } }

        //string _TypeName;
        ///// <summary>
        /////  Get The type name of body stream.
        ///// </summary>
        //public string TypeName
        //{
        //    get { return _TypeName; }
        //}

        /// <summary>
        /// Get Retry
        /// </summary>
        public byte Retry { get; set; }

        /// <summary>
        /// Get Creation time
        /// </summary>
        public DateTime Creation { get; internal set; }

        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        public DateTime ArrivedTime { get; internal set; }
    
        /// <summary>
        /// Get or Set Duration in milliseconds/Expiration in minutes
        /// </summary>
        public int Duration { get; internal set; }
   

        public bool IsExpired
        {
            get { return Expiration == 0 ? true : Creation.AddMinutes(Expiration) > DateTime.Now; }
        }

        #endregion

        #region ItemStream/Body
        
        //byte[] _ItemBinary;
        //public byte[] ItemBinary
        //{
        //    get
        //    {
        //        if (_ItemBinary == null)
        //        {
        //            _ItemBinary = ToStream().ToArray();
        //        }
        //        return _ItemBinary;
        //    }
        //}

        //public byte[] ItemBinary { get; set; }

        public byte[] Body { get { return BodyStream==null? null: BodyStream.ToArray(); } }


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

        //        streamer.WriteString(Host);

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
        /*
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
                    var header = MessageHeader.Get(value);

                    MessageState = header.MessageState;
                    MessageType = header.MessageType;
                    Command = header.Command;
                    Priority = header.Priority;
                    Identifier = header.Identifier;
                    Retry = header.Retry;
                    ArrivedTime = header.ArrivedTime;
                    Creation = header.Creation;
                    Modified = header.Modified;
                    Duration = header.Duration;
                    TransformType = header.TransformType;
                    Host = header.Host;
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
                Creation = this.Creation,
                Modified = this.Modified,
                Duration = this.Duration,
                TransformType = this.TransformType,
                Host = this.Host,
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

            //if (MessageType == MQTypes.MessageRequest)
            //{
            //    streamer.WriteValue((byte)MessageType);
            //    streamer.WriteValue((byte)Command);
            //    streamer.WriteValue((byte)Priority);
            //    streamer.WriteString(Identifier);//.WriteValue(ItemId);
            //    streamer.WriteValue(Creation);
            //    streamer.WriteValue((byte)TransformType);
            //    streamer.WriteString(Host);
            //    //streamer.WriteString(Sender);
            //    streamer.WriteValue(BodyStream);
            //}

            streamer.WriteValue(Version);

            streamer.WriteValue((byte)MessageState);
            streamer.WriteValue((byte)MessageType);
            streamer.WriteValue((byte)QCommand);
            streamer.WriteValue((byte)Priority);
            streamer.WriteString(Identifier);//.WriteValue(ItemId);
            streamer.WriteValue((byte)Retry);
            streamer.WriteValue(ArrivedTime);
            streamer.WriteValue(Creation);
            //streamer.WriteValue(Modified);
            streamer.WriteValue(Duration);
            //streamer.WriteValue((byte)TransformType);
            //streamer.WriteValue((byte)DuplexType);
            //streamer.WriteValue(Expiration);
            streamer.WriteString(Host);
            //streamer.WriteString(Label);
            //streamer.WriteString(Sender);
            streamer.WriteString(Destination);
            streamer.WriteValue(ChannelId);
            streamer.WriteValue(AccountId);
            streamer.WriteValue(MessageId);
            //streamer.WriteValue(BatchId);
            //streamer.WriteValue(OperationId);
            streamer.WriteValue(Amount);

        //streamer.WriteValue(BodyStream);
        //streamer.WriteString(TypeName);

        //MessageStream=======================================
            streamer.WriteString(Id);
            streamer.WriteValue(BodyStream);
            streamer.WriteString(TypeName);
            streamer.WriteValue((int)Formatter);
            streamer.WriteString(Label);
            streamer.WriteString(GroupId);
            streamer.WriteString(Command);
            streamer.WriteString(Sender);
            streamer.WriteValue((int)DuplexType);
            streamer.WriteValue(Expiration);
            streamer.WriteValue(Modified);
            streamer.WriteValue(Args);
            streamer.WriteValue((byte)TransformType);
            streamer.WriteString(EncodingName);
            streamer.Flush();
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
            
            MessageState = (MessageState)streamer.ReadValue<byte>();
            MessageType = (MQTypes)streamer.ReadValue<byte>();
            QCommand = (QueueCmd)streamer.ReadValue<byte>();
            Priority = (Priority)streamer.ReadValue<byte>();
            Identifier = streamer.ReadString();//.ReadValue<Guid>();
            Retry = streamer.ReadValue<byte>();
            ArrivedTime = streamer.ReadValue<DateTime>();
            Creation = streamer.ReadValue<DateTime>();
            //Modified = streamer.ReadValue<DateTime>();
            Duration = streamer.ReadValue<int>();
            //TransformType = (TransformType)streamer.ReadValue<byte>();
            //DuplexType = (DuplexTypes)streamer.ReadValue<byte>();
            //Expiration = streamer.ReadValue<int>();
            Host = streamer.ReadString();
            //Label = streamer.ReadString();
            //Sender = streamer.ReadString();
            Destination = streamer.ReadString();
            ChannelId = streamer.ReadValue<int>();
            AccountId = streamer.ReadValue<int>();
            MessageId = streamer.ReadValue<int>();
            //BatchId = streamer.ReadValue<int>();
            //OperationId = streamer.ReadValue<int>();
            Amount = streamer.ReadValue<decimal>();
            //BodyStream = (NetStream)streamer.ReadValue();
            //TypeName = streamer.ReadString();


            //MessageStream=======================================
            Id = streamer.ReadString();
            BodyStream = (NetStream)streamer.ReadValue();
            TypeName = streamer.ReadString();
            Formatter = (Formatters)streamer.ReadValue<int>();
            Label = streamer.ReadString();
            GroupId = streamer.ReadString();
            Command = streamer.ReadString();
            Sender = streamer.ReadString();
            DuplexType = (DuplexTypes)streamer.ReadValue<int>();
            Expiration = streamer.ReadValue<int>();
            Modified = streamer.ReadValue<DateTime>();
            Args = (NameValueArgs)streamer.ReadValue();
            TransformType = (TransformType)streamer.ReadValue<byte>();
            EncodingName = Types.NZorEmpty(streamer.ReadString(), DefaultEncoding);

            ////Topic = streamer.ReadString();
            //HeaderStream = (NetStream)streamer.ReadValue();


            //var ns = (NetStream)streamer.ReadValue();
            //Body = ns.ToArray();

            //ItemBinary = streamer.ReadValue<byte[]>(); 

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
        public override void WriteContext(ISerializerContext context)
        {
            SerializeInfo info = new SerializeInfo();

            info.Add("Version", Version);
            info.Add("MessageState", (byte)MessageState);
            info.Add("MessageType", (byte)MessageType);
            info.Add("Command", (byte)QCommand);
            info.Add("Priority", (byte)Priority);
            info.Add("Identifier", Identifier);
            info.Add("Retry", Retry);
            info.Add("ArrivedTime", ArrivedTime);
            info.Add("Creation", Creation);
            //info.Add("Modified", Modified);
            info.Add("Duration", Duration);
            //info.Add("TransformType", (byte)TransformType);
            //info.Add("DuplexType", (byte)DuplexType);
            //info.Add("Expiration", Expiration);

            info.Add("Host", Host);
            //info.Add("Label", Label);
            //info.Add("Sender", Sender);
            info.Add("Destination", Destination);
            info.Add("ChannelId", ChannelId);
            info.Add("AccountId", AccountId);
            info.Add("MessageId", MessageId);
            //info.Add("BatchId", BatchId);
            //info.Add("OperationId", OperationId);
            info.Add("Amount", Amount);

            //info.Add("BodyStream", BodyStream);
            //info.Add("TypeName", TypeName);

            //MessageStream=======================================
            info.Add("Id", Id);
            info.Add("BodyStream", BodyStream);
            info.Add("TypeName", TypeName);
            info.Add("Formatter", (int)Formatter);
            info.Add("Label", Label);
            info.Add("GroupId", GroupId);
            info.Add("Command", Command);
            info.Add("Sender", Sender);
            info.Add("DuplexType", (int)DuplexType);
            info.Add("Expiration", Expiration);
            info.Add("Modified", Modified);
            info.Add("Args", Args);
            info.Add("TransformType", (byte)TransformType);
            info.Add("EncodingName", EncodingName);


            //info.Add("ItemBinary", new NetStream(ItemBinary));

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
        public override void ReadContext(ISerializerContext context)
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

            Version = info.GetValue<int>("Version");
            MessageState = (MessageState)info.GetValue<byte>("MessageState");
            MessageType = (MQTypes)info.GetValue<byte>("MessageType");
            QCommand = (QueueCmd)info.GetValue<byte>("Command");
            Priority = (Priority)info.GetValue<byte>("Priority");
            Identifier = info.GetValue<string>("Identifier");
            Retry = info.GetValue<byte>("Retry");
            ArrivedTime = info.GetValue<DateTime>("ArrivedTime");
            Creation = info.GetValue<DateTime>("Creation");
            //Modified = info.GetValue<DateTime>("Modified");
            Duration = info.GetValue<int>("Duration");
            //TransformType = (TransformType)info.GetValue<byte>("TransformType");
            //DuplexType = (DuplexTypes)info.GetValue<byte>("DuplexType");
            //Expiration = info.GetValue<int>("Expiration");
            Host = info.GetValue<string>("Host");
            //Label = info.GetValue<string>("Label");
            //Sender = info.GetValue<string>("Sender");
            Destination = info.GetValue<string>("Destination");
            ChannelId = info.GetValue<int>("ChannelId");
            AccountId = info.GetValue<int>("AccountId");
            MessageId = info.GetValue<int>("MessageId");
            //BatchId = info.GetValue<int>("BatchId");
            //OperationId = info.GetValue<int>("OperationId");
            Amount = info.GetValue<decimal>("Amount");

            //BodyStream = (NetStream)info.GetValue("BodyStream");
            //TypeName = info.GetValue<string>("TypeName");


            //MessageStream=======================================
            Id = info.GetValue<string>("Id");
            BodyStream = (NetStream)info.GetValue("BodyStream");
            TypeName = info.GetValue<string>("TypeName");
            Formatter = (Formatters)info.GetValue<int>("Formatter");
            Label = info.GetValue<string>("Label");
            GroupId = info.GetValue<string>("GroupId");
            Command = info.GetValue<string>("Command");
            Sender = info.GetValue<string>("Sender");
            DuplexType = (DuplexTypes)info.GetValue<int>("DuplexType");
            Expiration = info.GetValue<int>("Expiration");
            Modified = info.GetValue<DateTime>("Modified");
            Args = (NameValueArgs)info.GetValue("Args");
            TransformType = (TransformType)info.GetValue<byte>("TransformType");
            EncodingName = Types.NZorEmpty(info.GetValue<string>("EncodingName"), DefaultEncoding); 

            //var ns = (NetStream)info.GetValue("ItemBinary");
            //ItemBinary = ns.ToArray();

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

        #region ISerialJson

        public override string EntityWrite(IJsonSerializer serializer, bool pretty = false)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Write, null);

            //object body = null;
            //if (BodyStream != null)
            //{
            //    body = BinarySerializer.ConvertFromStream(BodyStream);
            //}

            
            serializer.WriteToken("Version", Version);
            serializer.WriteToken("MessageState", (byte)MessageState);
            serializer.WriteToken("MessageType", (byte)MessageType);
            serializer.WriteToken("Command", (byte)QCommand);
            serializer.WriteToken("Priority", (byte)Priority);
            serializer.WriteToken("Identifier", Identifier);
            serializer.WriteToken("Retry", Retry);
            serializer.WriteToken("ArrivedTime", ArrivedTime);
            serializer.WriteToken("Creation", Creation);
            //serializer.WriteToken("Modified", Modified);
            serializer.WriteToken("Duration", Duration);
            //serializer.WriteToken("TransformType", (byte)TransformType);
            //serializer.WriteToken("DuplexType", (byte)DuplexType);
            //serializer.WriteToken("Expiration", Expiration);

            serializer.WriteToken("Host", Host);
            //serializer.WriteToken("Label", Label);
            //serializer.WriteToken("Sender", Sender);
            serializer.WriteToken("Destination", Destination);
            serializer.WriteToken("ChannelId", ChannelId);
            serializer.WriteToken("AccountId", AccountId);
            serializer.WriteToken("MessageId", MessageId);
            //serializer.WriteToken("BatchId", BatchId);
            //serializer.WriteToken("OperationId", OperationId);
            serializer.WriteToken("Amount", Amount);

            //serializer.WriteToken("BodyStream", BodyStream);
            //serializer.WriteToken("TypeName", TypeName);

            //MessageStream=======================================
            serializer.WriteToken("Id", Id);
            serializer.WriteToken("BodyStream", BodyStream == null ? null : BodyStream.ToBase64String());
            serializer.WriteToken("TypeName", TypeName);
            serializer.WriteToken("Formatter", Formatter);
            serializer.WriteToken("Label", Label, null);
            serializer.WriteToken("GroupId", GroupId, null);
            serializer.WriteToken("Command", Command);
            serializer.WriteToken("Sender", Sender);
            serializer.WriteToken("DuplexType", (int)DuplexType);
            serializer.WriteToken("Expiration", Expiration);
            serializer.WriteToken("Modified", Modified);
            serializer.WriteToken("Args", Args);
            serializer.WriteToken("TransformType", TransformType);
            serializer.WriteToken("EncodingName", EncodingName);

            //serializer.WriteToken("Body", body);

            return serializer.WriteOutput(pretty);
        }

        public override object EntityRead(string json, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, new JsonSettings() { IgnoreCaseOnDeserialize = true });

            //var queryParams = new Dictionary<string, string>(HtmlPage.Document.QueryString, StringComparer.InvariantCultureIgnoreCase);


            var dic = serializer.Read<Dictionary<string, object>>(json);

            if (dic != null)
            {



                Version = dic.Get<int>("Version");
                MessageState = (MessageState)dic.Get<byte>("MessageState");
                MessageType = (MQTypes)dic.Get<byte>("MessageType");
                QCommand = (QueueCmd)dic.Get<byte>("Command");
                Priority = (Priority)dic.Get<byte>("Priority");
                Identifier = dic.Get<string>("Identifier");
                Retry = dic.Get<byte>("Retry");
                ArrivedTime = dic.Get<DateTime>("ArrivedTime");
                Creation = dic.Get<DateTime>("Creation");
                //Modified = dic.Get<DateTime>("Modified");
                Duration = dic.Get<int>("Duration");
                //TransformType = (TransformType)dic.Get<byte>("TransformType");
                //DuplexType = (DuplexTypes)dic.Get<byte>("DuplexType");
                //Expiration = dic.Get<int>("Expiration");
                Host = dic.Get<string>("Host");
                //Label = dic.Get<string>("Label");
                //Sender = dic.Get<string>("Sender");
                Destination = dic.Get<string>("Destination");
                ChannelId = dic.Get<int>("ChannelId");
                AccountId = dic.Get<int>("AccountId");
                MessageId = dic.Get<int>("MessageId");
                //BatchId = dic.Get<int>("BatchId");
                //OperationId = dic.Get<int>("OperationId");
                Amount = dic.Get<decimal>("Amount");

                //BodyStream = (NetStream)dic.Get("BodyStream");
                //TypeName = dic.Get<string>("TypeName");

                //MessageStream=======================================
                Id = dic.Get<string>("Id");
                var body = dic.Get<string>("BodyStream");
                TypeName = dic.Get<string>("TypeName");
                Formatter = dic.GetEnum<Formatters>("Formatter", Formatters.Json);
                Label = dic.Get<string>("Label");
                GroupId = dic.Get<string>("GroupId");
                Command = dic.Get<string>("Command");
                Sender = dic.Get<string>("Sender");
                DuplexType = (DuplexTypes)dic.Get<int>("DuplexType");
                Expiration = dic.Get<int>("Expiration");
                Modified = dic.Get<DateTime>("Modified");
                Args = NameValueArgs.Convert((IDictionary<string, object>)dic.Get("Args"));// dic.Get<NameValueArgs>("Args");
                TransformType = (TransformType)dic.GetEnum<TransformType>("TransformType", TransformType.Object);
                EncodingName = Types.NZorEmpty(dic.Get<string>("EncodingName"), DefaultEncoding);  

                if (body != null && body.Length > 0)
                    BodyStream = NetStream.FromBase64String(body);
            }

            return this;
        }

        public override object EntityRead(NameValueCollection queryString, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, new JsonSettings() { IgnoreCaseOnDeserialize = true });

            if (queryString != null)
            {

                Version = queryString.Get<int>("Version");
                MessageState = (MessageState)queryString.Get<byte>("MessageState");
                MessageType = (MQTypes)queryString.Get<byte>("MessageType");
                QCommand = (QueueCmd)queryString.Get<byte>("Command");
                Priority = (Priority)queryString.Get<byte>("Priority");
                Identifier = queryString.Get<string>("Identifier");
                Retry = queryString.Get<byte>("Retry");
                ArrivedTime = queryString.Get<DateTime>("ArrivedTime");
                Creation = queryString.Get<DateTime>("Creation");
                //Modified = queryString.Get<DateTime>("Modified");
                Duration = queryString.Get<int>("Duration");
                //TransformType = (TransformType)queryString.Get<byte>("TransformType");
                //DuplexType = (DuplexTypes)queryString.Get<byte>("DuplexType");
                //Expiration = queryString.Get<int>("Expiration");
                Host = queryString.Get<string>("Host");
                //Label = queryString.Get<string>("Label");
                //Sender = queryString.Get<string>("Sender");
                Destination = queryString.Get<string>("Destination");
                ChannelId = queryString.Get<int>("ChannelId");
                AccountId = queryString.Get<int>("AccountId");
                MessageId = queryString.Get<int>("MessageId");
                //BatchId = queryString.Get<int>("BatchId");
                //OperationId = queryString.Get<int>("OperationId");
                Amount = queryString.Get<decimal>("Amount");

                //BodyStream = (NetStream)queryString.Get("BodyStream");
                //TypeName = queryString.Get<string>("TypeName");

                //MessageStream=======================================
                Id = queryString.Get<string>("Id");
                var body = queryString.Get<string>("BodyStream");
                TypeName = queryString.Get<string>("TypeName");
                Formatter = queryString.GetEnum<Formatters>("Formatter", Formatters.Json);
                Label = queryString.Get<string>("Label");
                GroupId = queryString.Get<string>("GroupId");
                Command = queryString.Get<string>("Command");
                Sender = queryString.Get<string>("Sender");
                DuplexType = (DuplexTypes)queryString.Get<int>("DuplexType");
                Expiration = queryString.Get<int>("Expiration");
                Modified = queryString.Get<DateTime>("Modified", DateTime.Now);
                var args = queryString.Get("Args");
                if (args != null)
                {
                    string[] nameValue = args.SplitTrim(':', ',', ';');
                    Args = NameValueArgs.Get(nameValue);
                }
                TransformType = (TransformType)queryString.GetEnum<TransformType>("TransformType", TransformType.Object);
                EncodingName = Types.NZorEmpty(queryString.Get<string>("EncodingName"), DefaultEncoding); 
                if (body != null && body.Length > 0)
                    BodyStream = NetStream.FromBase64String(body);


                //MessageStream=======================================
                //Command = queryString.Get<string>("Command".ToLower());
                //Sender = queryString.Get<string>("Sender".ToLower());
                //Id = queryString.Get<string>("Id".ToLower());
                //TypeName = queryString.Get<string>("TypeName".ToLower());
                //Label = queryString.Get<string>("Label".ToLower());
                //GroupId = queryString.Get<string>("GroupId".ToLower());
                //var body = queryString.Get<string>("BodyStream".ToLower());
                //Modified = queryString.Get<DateTime>("Modified".ToLower(), DateTime.Now);
                //Formatter = queryString.GetEnum<Formatters>("Formatter".ToLower(), Formatters.Json);
                ////IsDuplex = queryString.Get<bool>("IsDuplex".ToLower());
                //DuplexType = (DuplexTypes)queryString.Get<int>("DuplexType".ToLower());
                //Expiration = queryString.Get<int>("Expiration".ToLower());
                //var args = queryString.Get("Args".ToLower());
                //if (args != null)
                //{
                //    string[] nameValue = args.SplitTrim(':', ',', ';');
                //    Args = NameValueArgs.Get(nameValue);
                //}
                ////Args = NameValueArgs.Convert((IDictionary<string, object>)queryString.Get("Args".ToLower()));//queryString.Get<NameValueArgs>("Args".ToLower());
                //TransformType = (TransformType)queryString.GetEnum<TransformType>("TransformType".ToLower(), TransformType.Object);
                //if (body != null && body.Length > 0)
                //    BodyStream = NetStream.FromBase64String(body);
            }

            return this;
        }

        #endregion

        #region Converters

        public byte[] Serialize()
        {
            return BinarySerializer.SerializeToBytes(this);
        }
        public static QueueItem Deserialize(byte[] bytes)
        {
            return BinarySerializer.Deserialize<QueueItem>(bytes);
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
        public static QueueItem Deserialize(string json)
        {
            return JsonSerializer.Deserialize<QueueItem>(json);

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
        ///// Get body stream after set the position to first byte in buffer, This method is a part of <see cref="IQueueItem"/> implementation.
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
            this.SetState(state);
            TransStream stream = new TransStream(this);
            return stream;
        }

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

        /*
         /// <summary>
         /// Set the given value to body stream using <see cref="BinarySerializer"/>, This method is a part of <see cref="IMessageStream"/> implementation..
         /// </summary>
         /// <param name="value"></param>
         public void SetBody(object value)
         {

             if (value != null)
             {
                 TypeName = value.GetType().FullName;

                 NetStream ns = new NetStream();
                 var ser = new BinarySerializer();
                 ser.Serialize(ns, value);
                 ns.Position = 0;
                 BodyStream = ns;
             }
             else
             {
                 TypeName = typeof(object).FullName;
                 BodyStream = null;
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

                 NetStream ns = new NetStream();
                 var ser = new BinarySerializer();
                 ser.Serialize(ns, value);
                 ns.Position = 0;
                 BodyStream = ns;
             }
             else
             {
                 TypeName = typeof(string).FullName;
                 BodyStream = null;
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
             if (value != null)
             {
                 BodyStream = new NetStream(value);
             }
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
                 BodyStream = value;
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
          */

        #endregion

        #region ITransformMessage
        ///// <summary>
        ///// Get or Set The return type name.
        ///// </summary>
        //public TransformType TransformType { get; set; }

        ///// <summary>
        ///// Get or Set indicate wether the message is a duplex type.
        ///// </summary>
        //public bool IsDuplex { get; set; }


        ///// <summary>
        /////  Get or Set The message expiration.
        ///// </summary>
        //public int Expiration { get; set; }

        #endregion

        #region item properties

        public Ptr GetPtr()
        {
            return new Ptr(Identifier, 0, Host);
        }

        public Ptr GetPtr(string hotName)
        {
            return new Ptr(Identifier, 0, hotName);
        }

        //public double Duration()
        //{
        //    return SentTime.Subtract(ArrivedTime).TotalSeconds;
        //}

        internal void SetReceived(MessageState state)
        {
            DateTime now = DateTime.Now;
            DateTime recievTime = Modified;

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
            var d = now.Subtract(Modified).TotalMilliseconds;
            //var d = now.Subtract(Creation).TotalMilliseconds;
            d = Math.Min(d, int.MaxValue);
            Duration = (int)d;
        }

        internal Ptr SetArrivedPtr(string host)
        {
            try
            {
                DateTime now = DateTime.Now;
                this.Modified = now;
                this.ArrivedTime = now;
                this.MessageState = Messaging.MessageState.Arrived;
                this.Identifier = Ptr.NewIdentifier();

                Ptr ptr = new Ptr(this, host);
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
            this.Modified = DateTime.Now;
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
            MessageState,
            MessageType,
            Command,
            Priority,
            Identifier,
            Retry,
            ArrivedTime,
            Creation,
            Modified,
            Duration,
            TransformType,
            Host,
            Label,
            Sender,
            GetBytes(),
            TypeName
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

            return string.Format("Host:{0},Command:{1},MessageState:{2},Retry:{3},Creation:{4},Identifier:{5}",
            Host,
            Command,
            MessageState,
            Retry,
            Creation,
            Identifier

            //MessageType,
            //Priority,
            //ArrivedTime,
            //Modified
            //Expiration,
            //TransformType,
            //Label,
            //Sender,
            //TypeName
            );

            //return string.Format("MessageState:{0},MessageType:{1},Command:{2},Priority:{3},Identifier:{4},Retry:{5},ArrivedTime:{6},Creation:{7},Modified:{8},Expiration:{9},TransformType:{10},Host:{11},Label:{12},Sender:{13},TypeName:{14}", 
            //MessageState,
            //MessageType,
            //Command,
            //Priority,
            //Identifier,
            //Retry,
            //ArrivedTime,
            //Creation,
            //Modified,
            //Expiration,
            //TransformType,
            //Host,
            //Label,
            //Sender,
            //TypeName);


            //StringBuilder sb = new StringBuilder();
            //sb.Append("QueueItem Print:");
            //sb.AppendFormat("\r\n{0}", MessageState);
            //sb.AppendFormat("\r\n{0}", MessageType);
            //sb.AppendFormat("\r\n{0}", Command);
            //sb.AppendFormat("\r\n{0}", Priority);
            //sb.AppendFormat("\r\n{0}", Identifier);
            //sb.AppendFormat("\r\n{0}", Retry);
            //sb.AppendFormat("\r\n{0}", ArrivedTime);
            //sb.AppendFormat("\r\n{0}", Creation);
            //sb.AppendFormat("\r\n{0}", Modified);
            //sb.AppendFormat("\r\n{0}", Expiration);
            ////sb.AppendFormat("\r\n{0}", MessageId);
            //sb.AppendFormat("\r\n{0}", TransformType);
            //sb.AppendFormat("\r\n{0}", Host);
            //sb.AppendFormat("\r\n{0}", Sender);
            //sb.AppendFormat("\r\n{0}", Label);
            //return sb.ToString();
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
        #endregion

        #region File

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

     
    }


#if (false)
    public class QItemHeader
    {
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
        /// Get The message Host\Queue name.
        /// </summary>
        public string Host { get; set; }

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
            streamer.WriteValue(Modified);
            streamer.WriteValue(Expiration);
            streamer.WriteValue((byte)TransformType);
            streamer.WriteString(Host);
            streamer.WriteString(Sender);
            streamer.WriteString(Label);
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
            Modified = streamer.ReadValue<DateTime>();
            Expiration = streamer.ReadValue<int>();
            TransformType = (TransformTypes)streamer.ReadValue<byte>();
            Host = streamer.ReadString();
            Sender = streamer.ReadString();
            Label = streamer.ReadString();

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
            return ToStream().ToArray();
        }
    }


    public class QueueItem:IDisposable, ICloneable, IQueueItem
    {
    #region ICloneable
        public object Clone()
        {
            var copy = new QueueItem()
            {
                Modified = this.Modified,
                Retry = this.Retry,
                MessageState = this.MessageState,
                ItemId = this.ItemId,
                Header=this.Header,
                Body =this.Body
            };

            return copy;
        }
    #endregion

    #region properties
        /// <summary>
        /// Get or Set message type.
        /// </summary>
        public MQTypes MessageType { get; set; }
        /// <summary>
        /// Get ItemId
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// Get MessageState
        /// </summary>
        public MessageState MessageState { get; set; }
        /// <summary>
        /// Get Retry
        /// </summary>
        public byte Retry { get; set; }
        /// <summary>
        /// Get the last modified time.
        /// </summary>
        public DateTime Modified { get; set; }

        public byte[] Header { get; set; }

        public byte[] Body { get; set; }
    #endregion

    #region ctor
        /// <summary>
        /// Initialize a new instance of Message
        /// </summary>
        public QueueItem()
        {
            var header = new QItemHeader()
            {
                Priority = Priority.Normal
            };
        }

        public QueueItem(QueueCmd command, TransformTypes transformType, Priority priority, string destination, string json)
        {
            var header = new QItemHeader()
            {
                MessageType = MQTypes.MessageRequest,
                Command = command,
                Priority = priority,
                TransformType = transformType,
                Host = destination,
                ArrivedTime = DateTime.Now

            };
            Header = header.ToBinary();
            Body = Encoding.UTF8.GetBytes(json);
            //m_BodyStream = new NetStream(Body);
        }

        public QueueItem(MessageRequest message)
        {
            var header = new QItemHeader()
            {
                MessageType = MQTypes.MessageRequest,
                Command = message.Command,
                Priority = message.Priority,
                TransformType = message.TransformType,
                Host = message.Host,
                ArrivedTime = DateTime.Now
            };
            Header = header.ToBinary();
            NetStream ns = new NetStream();
            message.EntityWrite(ns);
            //m_BodyStream = ns;
            Body = ns.ToArray();
        }

        public QueueItem(Message message)
        {
            var header = new QItemHeader()
            {
                MessageType = MQTypes.Message,
                Command = message.Command,
                Priority = message.Priority,
                TransformType = message.TransformType,
                Host = message.Host,
                ArrivedTime = DateTime.Now
            };
            Header = header.ToBinary();
            NetStream ns = new NetStream();
            message.EntityWrite(ns);
            //m_BodyStream = ns;
            Body = ns.ToArray();
        }

        public QueueItem(QueueAck message)
        {
            var header = new QItemHeader()
            {
                MessageType = MQTypes.Ack,
                Command = QueueCmd.Ack,
                //Priority = message.Priority;
                //TransformType = message.TransformType;
                Host = message.Host,
                ArrivedTime = DateTime.Now
            };
            Header = header.ToBinary();
            NetStream ns = new NetStream();
            message.EntityWrite(ns);
            //m_BodyStream = ns;
            Body = ns.ToArray();
        }

        /// <summary>
        /// Initialize a new instance of MessageStream from stream using for <see cref="ISerialEntity"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public QueueItem(Stream stream, IBinaryStreamer streamer)
        {
            EntityRead(stream, streamer);
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
                Header = null;
                Body = null;

                //if (m_BodyStream != null)
                //{
                //    m_BodyStream.Dispose();
                //    m_BodyStream = null;
                //}
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

            streamer.WriteValue((byte)MessageState);
            streamer.WriteValue((byte)MessageType);
            streamer.WriteString(ItemId);//.WriteValue(ItemId);
            streamer.WriteValue(Retry);
            streamer.WriteValue(Modified);
            streamer.WriteValue(new NetStream(Header));
            streamer.WriteValue(new NetStream(Body));
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
            ItemId = streamer.ReadString();//.ReadValue<Guid>();
            Retry = streamer.ReadValue<byte>();
            Modified = streamer.ReadValue<DateTime>();

            var nsh = (NetStream)streamer.ReadValue();
            Header = nsh.ToArray();

            var nsb = (NetStream)streamer.ReadValue();
            Body = nsb.ToArray();
        }

    #endregion

    #region IMessageStream

        public string ToJson()
        {
            switch (MessageType)
            {
                case MQTypes.Ack:
                    QueueAck ack = new QueueAck(GetBodyStream());
                    return JsonSerializer.Serialize(ack);
                case MQTypes.Message:
                    Message message = new Message(GetBodyStream());
                    return JsonSerializer.Serialize(message);
                case MQTypes.MessageRequest:
                    MessageRequest mr = new MessageRequest(GetBodyStream());
                    return JsonSerializer.Serialize(mr);
                case MQTypes.Json:
                    var stream = GetBodyStream();
                    return Encoding.UTF8.GetString(stream.ToArray());
            }
            return null;
        }

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
            return new NetStream(Body);

            //if (BodyStream == null)
            //    return null;
            //if (BodyStream.Position > 0)
            //    BodyStream.Position = 0;
            //return BodyStream;
        }

        /// <summary>
        /// Get Header stream after set the position to first byte in buffer.
        /// </summary>
        /// <returns></returns>
        public NetStream GetHeaderStream()
        {
            return new NetStream(Header);
        }

        public QItemHeader GetHeader()
        {
            var header = new QItemHeader();
            header.EntityWrite(GetHeaderStream(), null);
            return header;
        }
        public Message GetBody()
        {
            var msg = new Message();
            msg.EntityWrite(GetBodyStream(), null);
            return msg;
        }

    #endregion
    }
#endif

}
