using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.IO;
using System.IO;
using Nistec.Runtime;
using Nistec.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Nistec.Serialization;
using Nistec.Logging;

namespace Nistec.Messaging
{
    /// <summary>
    /// Represent an item stream in queue.
    /// </summary>
    public sealed class QueueItem : IDisposable//IQueueItem
    {
        
        #region ctor

        /// <summary>
        /// Initialize a new instance of QueueItem.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static QueueItem MessageArraived(NetStream stream)
        {
            return new QueueItem(stream, true);
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
        //        return new QueueItem((NetStream)stream, false);
        //    }
        //    else
        //    {
        //        return new QueueItem(NetStream.CopyStream(stream), false);
        //    }
        //}

        //public static QueueItem Create(Message message, QueueCmd command)
        //{
        //    if (message == null)
        //    {
        //        throw new ArgumentNullException("QueueItem.ctor stream");
        //    }
        //    bool isArrived = false;
        //    message.Command = command;
        //    if (message.Command == QueueCmd.Enqueue)
        //    {
        //        //SetArrived();
        //        isArrived = true;
        //        message.ArrivedTime = DateTime.Now;
        //        message.MessageState = Messaging.MessageState.Arrived;
        //        message.ItemId = UUID.NewUuid();

        //    }
        //    message.Modified = DateTime.Now;
        //    NetStream stream = new NetStream();
        //    message.EntityWrite(stream, null);
        //    stream.Position = 0;
        //    return new QueueItem(stream, isArrived);

        //}

        private QueueItem()
        {

        }

        /// <summary>
        /// Initialize a new instance of QueueItem
        /// </summary>
        /// <param name="stream"></param>
        private QueueItem(Message message, QueueCmd command)
        {
            if (message == null)
            {
                throw new ArgumentNullException("QueueItem.ctor message");
            }
            
            message.Command = command;
            if (message.Command == QueueCmd.Enqueue)
            {
                message.ArrivedTime = DateTime.Now;
                message.MessageState = Messaging.MessageState.Arrived;
                message.ItemId = UUID.NewUuid();
            }

            message.Modified = DateTime.Now;
            NetStream stream = new NetStream();
            message.EntityWrite(stream, null);
            stream.Position = 0;
            m_stream = stream.Copy();
            ValidateStream();

            Body = stream.ToArray();
            MessageState = message.MessageState;
            Command = message.Command;
            //Priority = message.Priority;
            ItemId = message.ItemId;
            Retry = message.Retry;
            ArrivedTime = message.ArrivedTime;
            //SentTime = message.SentTime;
            Modified = message.Modified;
            Expiration = message.Expiration;
            TransformType = message.TransformType;
            QueueName = message.Destination;
            Sender = message.Sender;

            
        }

        /// <summary>
        /// Initialize a new instance of QueueItem
        /// </summary>
        /// <param name="stream"></param>
        private QueueItem(NetStream stream, bool isArraived)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("QueueItem.ctor stream");
            }
            m_stream = stream.Copy();
            ValidateStream();
            if (isArraived)
            {
                SetArrived();
            }
            Read();
            
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

                //Label = null;
                Sender = null;
                QueueName = null;
                //Notify = null;
                if (HeaderStream != null)
                {
                    HeaderStream.Dispose();
                    HeaderStream = null;
                }
                if (m_stream != null)
                {
                    m_stream.Dispose();
                    m_stream = null;
                }
            }
            disposed = true;
        }
        #endregion

        
        #region message property

        /// <summary>
        /// Get MessageState
        /// </summary>
        public MessageState MessageState { get; internal set; }

        ///// <summary>
        ///// Get Command
        ///// </summary>
        //public QueueCmd Command { get; internal set; }
        ///// <summary>
        ///// Get or Set transformation type.
        ///// </summary>
        //public TransformTypes TransformType { get; set; }

        /// <summary>
        /// Get ItemId
        /// </summary>
        public Guid ItemId { get; internal set; }
        
        ///// <summary>
        ///// Get Priority
        ///// </summary>
        //public Priority Priority { get; set; }

        /// <summary>
        /// Get Retry
        /// </summary>
        public byte Retry { get; internal set; }

        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        public DateTime ArrivedTime { get; internal set; }
        
        ///// <summary>
        ///// Get SentTime
        ///// </summary>
        //public DateTime SentTime { get; internal set; }

        /// <summary>
        /// Get or Set timeout in seconds
        /// </summary>
        public int Expiration { get; set; }

        /// <summary>
        /// Get the last modified time.
        /// </summary>
        public DateTime Modified { get; internal set; }

        /// <summary>
        /// Get The message Destination\Queue name.
        /// </summary>
        public string QueueName { get; internal set; }

        ///// <summary>
        ///// Get MessageId
        ///// </summary>
        //public int MessageId { get; set; }

        ///// <summary>
        ///// Get or Set The message Label.
        ///// </summary>
        //public string Label { get; set; }

        /// <summary>
        /// Get or Set The message Sender.
        /// </summary>
        public string Sender { get; internal set; }

        public byte[] Body { get; internal set; }

        #endregion

        #region item properties


        public Ptr GetPtr()
        {
            return new Ptr(Identifier, 0, QueueName);
        }

        /// <summary>
        /// Get indicate wether the item is timeout 
        /// </summary>
        public bool IsTimeOut
        {
            get { return TimeSpan.FromSeconds(Expiration) < DateTime.Now.Subtract(ArrivedTime); }
        }

        /// <summary>
        /// Get UniqueId
        /// </summary>
        public long UniqueId
        {
            get { return ItemId.UniqueId(); }
        }

        public string Identifier
        {
            get { return Assists.GetIdentifier(ItemId, Priority.Normal); }
        }

        //public string Filename
        //{
        //    get { return Assists.GetFilename(Identifier); }
        //}

        //public string FolderId
        //{
        //    get
        //    {
        //        return Assists.GetFolderId(Identifier);//Modified, Priority);
        //    }
        //}

        #endregion

        #region Header Stream

        //internal NetStream HeaderStream { get; set; }

        //GenericNameValue _Header;

        ///// <summary>
        ///// Deserialize header stream to object.
        ///// </summary>
        ///// <returns></returns>
        //public GenericNameValue GetHeader()
        //{
        //    if (HeaderStream == null)
        //        return null;
        //    if (_Header == null)
        //    {
        //        HeaderStream.Position = 0;
        //        var ser = new BinarySerializer();
        //        _Header= ser.Deserialize<GenericNameValue>(HeaderStream, true);
        //    }
        //    return _Header;
        //}


        #endregion

        #region message stream



        NetStream m_stream;
        /// <summary>
        /// Get the current body stream.
        /// </summary>
        public NetStream BodyStream
        {
            get { return m_stream; }
        }

        public Message GetMessage()
        {
            if (BodyStream == null)
            {
                return null;
            }
            return Message.Create(BodyStream);//( (Message)Message.Deserialize(BodyStream);
        }

        const int offset = 0;//9;

        void Read()
        {
            //first byte is SerialContextType

            try
            {
                //0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 
                //t b t b t b t b------------------ItemId------------------b  t  b  t  b-----Arrived

                //31 32 33 34 35 36 37 38 39 40 41 42 43 44 45 46 47 48 49 50 51 52 53 54 55 56 57 58 59
                //-------b  t  b----------Sent------b  t  b------Modified------b  t  b--------b  t  b---

                //60 61 62 63 64 65 66 67 68 69 70
                //---b  t  b   t b--count--b


                //streamer.WriteValue((byte)MessageState);
                //streamer.WriteValue((byte)Command);
                //streamer.WriteValue((byte)Priority);
                //streamer.WriteValue(ItemId);
                //streamer.WriteValue(Retry);
                //streamer.WriteValue(ArrivedTime);
                //streamer.WriteValue(SentTime);
                //streamer.WriteValue(Modified);
                //streamer.WriteValue(Expiration);
                //streamer.WriteValue(MessageId);
                //streamer.WriteValue((byte)TransformType);
                //streamer.WriteString(Destination);

                m_stream.Position = 0;
                using (BinaryStreamer streamer = new BinaryStreamer(m_stream,true))
                {
                    MessageState = (MessageState)streamer.ReadValue<byte>();
                    Command = (QueueCmd)streamer.ReadValue<byte>();
                    //Priority = (Priority)streamer.ReadValue<byte>();
                    ItemId = streamer.ReadValue<Guid>();
                    Retry = streamer.ReadValue<byte>();
                    ArrivedTime = streamer.ReadValue<DateTime>();
                    //SentTime = streamer.ReadValue<DateTime>();
                    Modified = streamer.ReadValue<DateTime>();
                    Expiration = streamer.ReadValue<int>();
                    //MessageId = 
                    streamer.ReadValue<int>();
                    TransformType = (TransformTypes)streamer.ReadValue<byte>();
                    QueueName = streamer.ReadString();
                }
                 m_stream.Position = 0;


                //m_stream.Position = 0;
                //MessageState = (MessageState)m_stream.PeekByte(offset + 1);
                //Command = (QueueCmd)m_stream.PeekByte(offset + 3);
                //Priority = (Priority)m_stream.PeekByte(offset + 5);
                //ItemId = new Guid(m_stream.PeekBytes(offset + 7, 16));
                //Retry = (byte)m_stream.PeekByte(offset + 24);
                //ArrivedTime = (DateTime)m_stream.PeekDateTime(offset + 26);
                ////SentTime = (DateTime)m_stream.PeekDateTime(offset + 35);
                //Modified = (DateTime)m_stream.PeekDateTime(offset + 44);
                //Expiration = m_stream.PeekInt32(offset + 53);
                ////MessageId = m_stream.PeekInt32(offset + 58);
                //TransformType = (TransformTypes)m_stream.PeekByte(offset + 63);
                //int counter = 0;
                //m_stream.PeekString(offset + 65, out counter);


            }
            catch (Exception ex)
            {
                throw new MessageException(Messaging.MessageState.StreamReadWriteError, "QueueItem error: " + ex.Message);
            }

        }

        internal void ValidateStream()
        {
            //string mmsg = m_stream.PeekString(4, 4);
            //if (mmsg != "MMSG")
            //{
            //    throw new Exception("Incorrect message format");
            //}
        }

        internal void SetArrived()
        {
            try
            {
                this.Modified = DateTime.Now;
                this.ArrivedTime = DateTime.Now;
                this.MessageState = Messaging.MessageState.Arrived;
                this.ItemId = UUID.NewUuid();

                m_stream.Replace((byte)MessageState, offset + 1);
                m_stream.Replace(ItemId.ToByteArray(), offset + 7, 16);
                m_stream.Replace(ArrivedTime.Ticks, offset + 26);
                m_stream.Replace(Modified.Ticks, offset + 44);

            }
            catch (Exception ex)
            {
                throw new MessageException(Messaging.MessageState.StreamReadWriteError, "QueueItem SetArrived error: " + ex.Message);
            }
        }

        internal void SetReceiving()
        {
            try
            {
                this.Modified = DateTime.Now;
                //this.SentTime = DateTime.Now;
                this.MessageState = Messaging.MessageState.Receiving;

                m_stream.Replace((byte)MessageState, offset + 1);
                //m_stream.Replace(SentTime.Ticks, offset + 35);
                m_stream.Replace(Modified.Ticks, offset + 44);
            }
            catch (Exception ex)
            {
                throw new MessageException(Messaging.MessageState.StreamReadWriteError, "QueueItem SetReceiving error: " + ex.Message);
            }
        }
        public void SetState(MessageState state)
        {
            try
            {
                this.Modified = DateTime.Now;
                this.MessageState = state;

                m_stream.Replace((byte)MessageState, offset + 1);
                m_stream.Replace(Modified.Ticks, offset + 44);
            }
            catch (Exception ex)
            {
                throw new MessageException(Messaging.MessageState.StreamReadWriteError, "QueueItem SetState error: " + ex.Message);
            }
        }

        public void DoRetry()
        {
            Retry++;
            this.Modified = DateTime.Now;
            m_stream.Replace(Retry, offset + 24);
            m_stream.Replace(Modified.Ticks, offset + 44);
        }

        #endregion

        #region util

        /// <summary>
        /// Get a copy of <see cref="QueueItem"/> as <see cref="IQueueItem"/>
        /// </summary>
        /// <returns></returns>
        public Message Copy()
        {
            if (BodyStream == null)
            {
                return null;
            }
            return new Message(BodyStream.Copy(), null, MessageState);
        }
       
        /// <summary>
        /// Get the item id of current item.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Guid GetItemId(NetStream stream)
        {
            byte[] b = stream.PeekBytes(7, 16);

            return new Guid(b);
        }
        
 
        #endregion

        public Ptr GetPtr(string hotName)
        {
            return new Ptr(Identifier, 0, hotName);
        }
        
        //public double Duration()
        //{
        //    return SentTime.Subtract(ArrivedTime).TotalSeconds;
        //}


            //dt.Columns.Add("Host", typeof(string));
            //dt.Columns.Add("MessageState", typeof(Int16));
            //dt.Columns.Add("Command", typeof(Int16));
            //dt.Columns.Add("Priority", typeof(Int16));
            //dt.Columns.Add("UniqueId", typeof(long));
            //dt.Columns.Add("Retry", typeof(Int16));
            //dt.Columns.Add("ArrivedTime", typeof(DateTime));
            //dt.Columns.Add("SentTime", typeof(DateTime));
            //dt.Columns.Add("Modified", typeof(DateTime));
            //dt.Columns.Add("Expiration", typeof(Int32));
            //dt.Columns.Add("MessageId", typeof(Int16));
            //dt.Columns.Add("BodyStream", typeof(string));

        internal object[] ItemArray()
        {

            return new object[]{  QueueName, 
            MessageState,
            Command,
            //Priority,
            UniqueId,
            Retry,
            ArrivedTime,
            //SentTime,
            Modified,
            Expiration,
            //MessageId,
            BodyStream
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
            sb.AppendFormat("\r\n{0}", Command);
            //sb.AppendFormat("\r\n{0}", Priority);
            sb.AppendFormat("\r\n{0}", ItemId);
            sb.AppendFormat("\r\n{0}", Retry);
            sb.AppendFormat("\r\n{0}", ArrivedTime);
            //sb.AppendFormat("\r\n{0}", SentTime);
            sb.AppendFormat("\r\n{0}", Modified);
            sb.AppendFormat("\r\n{0}", Expiration);
            //sb.AppendFormat("\r\n{0}", MessageId);
            sb.AppendFormat("\r\n{0}", TransformType);
            sb.AppendFormat("\r\n{0}", QueueName);
            //sb.AppendFormat("\r\n{0}", Label);
            sb.AppendFormat("\r\n{0}", Sender);
            return sb.ToString();
        }
 
    }
}
