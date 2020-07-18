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

namespace Nistec.Messaging
{

 

    /// <summary>
    /// Represent a message stream for network communication like namedPipe or Tcp.
    /// This message can serialize/desrialize fast and easly using the <see cref="BinaryStreamer"/>
    /// </summary>
    [Serializable]
    public sealed class QueueRequest : MessageStream,  IQueueMessage, IDisposable
    {
        #region static
   
        
        /// <summary>
        /// Get the default formatter.
        /// </summary>
        public static Formatters DefaultFormatter { get { return Formatters.BinarySerializer; } }

        #endregion

        #region property

        public int Version { get; internal set; }

        /// <summary>
        /// Get Creation Time
        /// </summary>
        public DateTime Creation { get; internal set; }
        /// <summary>
        /// Get Command
        /// </summary>
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
                Command = value.ToString();
            }
        }

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
        /// Get Priority
        /// </summary>
        public Priority Priority { get; set; }

        /// <summary>
        /// Get The message Host\Queue name.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Get ItemId
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Get or Set message type.
        /// </summary>
        public MQTypes MessageType { get { return MQTypes.MessageRequest; } }

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
            //DuplexType = DuplexTypes.None;
            //EnsureDuplex();
        }

        public QueueRequest(Stream stream)
        {
            EntityRead(stream,null);
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


            //streamer.WriteValue(ToBinary());

            streamer.WriteValue(Version);
            streamer.WriteValue((byte)MessageState.Sending);
            streamer.WriteValue((byte)MessageType);
            streamer.WriteValue((byte)QCommand);
            streamer.WriteValue((byte)Priority);
            streamer.WriteString(Identifier);//.WriteValue(ItemId);
            streamer.WriteValue((byte)0);//Retry
            streamer.WriteValue(Modified);// ArrivedTime);
            streamer.WriteValue(Creation);
            streamer.WriteValue(Modified);
            streamer.WriteValue((int)0);// Duration);
            streamer.WriteValue((byte)TransformType);
            streamer.WriteValue((byte)DuplexType);
            streamer.WriteValue(Expiration);
            streamer.WriteString(Host);
            streamer.WriteString(Label);
            streamer.WriteString(Sender);
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
        public override void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);


            Version = streamer.ReadValue<int>();
            var MessageState = (MessageState)streamer.ReadValue<byte>();
            var  MessageType = (MQTypes)streamer.ReadValue<byte>();
            QCommand = (QueueCmd)streamer.ReadValue<byte>();
            Priority = (Priority)streamer.ReadValue<byte>();
            Identifier = streamer.ReadString();//.ReadValue<Guid>();
            var Retry = streamer.ReadValue<byte>();
            var  ArrivedTime = streamer.ReadValue<DateTime>();
            Creation = streamer.ReadValue<DateTime>();
            Modified = streamer.ReadValue<DateTime>();
            var Duration = streamer.ReadValue<int>();

            TransformType = (TransformType)streamer.ReadValue<byte>();
            DuplexType = (DuplexTypes)streamer.ReadValue<byte>();
            Expiration = streamer.ReadValue<int>();
            Host = streamer.ReadString();
            Label = streamer.ReadString();
            Sender = streamer.ReadString();
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

        public byte[] ToBinary() {

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
                    strmer.WriteValue(Modified);// ArrivedTime);
                    strmer.WriteValue(Creation);
                    strmer.WriteValue(Modified);
                    strmer.WriteValue((int)0);// Duration);
                    strmer.WriteValue((byte)TransformType);
                    strmer.WriteValue((byte)DuplexType);
                    strmer.WriteValue(Expiration);
                    strmer.WriteString(Host);
                    strmer.WriteString(Label);
                    strmer.WriteString(Sender);
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
}
