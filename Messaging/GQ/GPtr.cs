using Nistec.Generic;
using Nistec.IO;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nistec.Messaging
{

    public struct GPtr : ISerialEntity
    {
        Guid m_Identifier;
        DateTime m_ArrivedTime;
        int m_TimeOut;
        string m_Host;
        //string m_Location;
        PtrState m_State;
        int m_Retry;

        public static Guid NewIdentifier()
        {
            return UUID.NewUuid();
        }
        public static Guid NewIdentifier(Guid itemId)
        {
            return itemId;//.ToString();
        }

        public GPtr(Guid identifier, string hostName, int timeout = 0)//, string rootPath)
        {
            m_Identifier = identifier;
            m_ArrivedTime = DateTime.Now;
            m_TimeOut = timeout;
            m_Host = hostName;
            //m_Location = GetPtrLocation(rootPath, identifier); 
            m_State = 0;
            m_Retry = 0;
            //MessageType = MQTypes.Message;
        }

        public GPtr(GPtr item, PtrState state)
        {
            m_Identifier = item.Identifier;
            m_ArrivedTime = item.ArrivedTime;
            m_TimeOut = item.TimeOut;
            m_Host = item.Host;
            //m_Location = GetPtrLocation(item, host);
            m_State = state;
            m_Retry = item.Retry;
            //MessageType = item.MessageType;
        }
        public GPtr(byte[] bytes)
        {

            using (var stream = new NetStream(bytes))
            {

                using (var streamer = new BinaryStreamer(stream))
                {

                    string mmsg = streamer.ReadFixedString();
                    if (mmsg != MessageContext.QPTR)
                    {
                        throw new Exception("Incorrect message format");
                    }
                    m_State = (PtrState)streamer.ReadValue<byte>();
                    m_Retry = streamer.ReadValue<int>();
                    m_Identifier = streamer.ReadValue<Guid>();
                    m_Host = streamer.ReadString();
                    m_ArrivedTime = streamer.ReadValue<DateTime>();
                    m_TimeOut = streamer.ReadValue<int>();
                }
                //EntityRead(stream, null);
            }
        }

        public void Dispose()
        {
            m_Identifier = Guid.Empty;
            m_Host = null;
        }

        public bool IsEmpty
        {
            get { return Identifier.IsEmpty(); }
        }

        public static GPtr Empty
        {
            get { return new GPtr(); }
        }

        public static GPtr Get(Guid identifier, PtrState state)
        {
            return new GPtr()
            {
                m_ArrivedTime = DateTime.Now,
                m_Identifier = identifier,
                m_State = state
            };
        }

        #region property

        /// <summary>
        /// Get Identifier
        /// </summary>
        public Guid Identifier { get { return m_Identifier; } }
        /// <summary>
        /// Get or Set the item host.
        /// </summary>
        public string Host { get { return m_Host; } set { m_Host = value; } }
        ///// <summary>
        ///// Get or Set the item location.
        ///// </summary>
        //public string Location { get { return m_Location; } set { m_Location = value; } }

        /// <summary>
        /// Get Item state
        /// </summary>
        public PtrState State { get { return m_State; } }
        /// <summary>
        /// Get Item Retry
        /// </summary>
        public int Retry { get { return m_Retry; } }
        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        public DateTime ArrivedTime { get { return m_ArrivedTime; } }
        /// <summary>
        /// Get or Set timeout in seconds
        /// </summary>
        public int TimeOut { get { return m_TimeOut; } }
        /// <summary>
        /// Get indicate wether the item is timeout 
        /// </summary>
        public bool IsTimeOut
        {
            get { return TimeOut == 0 ? false : TimeSpan.FromSeconds(TimeOut) < DateTime.Now.Subtract(ArrivedTime); }
        }

        //public Guid ItemId { get { return new Guid(Identifier); } }

        ///// <summary>
        ///// Get UniqueId
        ///// </summary>
        //public long UniqueId
        //{
        //    get { return Assists.GetUniqueId(Identifier); }
        //}

        //public string Filename
        //{
        //    get { return string.Format("{0}.info", UniqueId); }
        //}

        /// <summary>
        /// Get Item state
        /// </summary>
        public MessageState MessageState { get { return (MessageState)m_State; } }

        /// <summary>
        /// Get or Set message type.
        /// </summary>
        //public MQTypes MessageType { get; set; }

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

            streamer.WriteFixedString(MessageContext.QPTR, 4);
            streamer.WriteValue((byte)State);
            streamer.WriteValue((int)Retry);
            streamer.WriteValue(Identifier);
            streamer.WriteString(Host);
            streamer.WriteValue(ArrivedTime);
            streamer.WriteValue(TimeOut);
            //streamer.WriteString(Location);
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
            if (mmsg != MessageContext.QPTR)
            {
                throw new Exception("Incorrect message format");
            }

            m_State = (PtrState)streamer.ReadValue<byte>();
            m_Retry = streamer.ReadValue<int>();
            m_Identifier = streamer.ReadValue<Guid>();
            m_Host = streamer.ReadString();
            m_ArrivedTime = streamer.ReadValue<DateTime>();
            m_TimeOut = streamer.ReadValue<int>();
            //m_Location = streamer.ReadString();

        }


        #endregion

        public void DoRetry()
        {
            m_Retry++;
            //this.Modified = DateTime.Now;
        }

        public void SaveToFile(string rootPath)
        {
            string filename = Assists.GetInfoFilename(rootPath, m_Host, Identifier.ToString());
            using (NetStream stream = new NetStream())
            {
                EntityWrite(stream, null);
                stream.SaveToFile(filename);
            }
        }

        /// <summary>
        /// Get an instance of <see cref="QueueItemStream"/> from file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static GPtr ReadFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return GPtr.Empty;
            }
            var ptr = GPtr.Empty;
            using (NetStream memoryStream = new NetStream())
            {
                using (Stream input = File.OpenRead(filename))
                {
                    input.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                ptr.EntityRead(memoryStream, null);
            }
            return ptr;
        }

        //public static string CreateFileId(long uniqeId)
        //{
        //    QueueFormatter.GetFilename(QueueSettings.RootPath,"",
        //    return null;
        //}

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Identifier.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is GPtr)
            {
                return ((GPtr)obj).Identifier == Identifier;
            }
            return false;
        }

        public bool Equals(GPtr ptr)
        {
            return ptr.Identifier == Identifier;
        }

        public bool Equals(Guid identifier)
        {
            return identifier == Identifier;
        }
        public GPtr Copy()
        {
            return Deserialize(Serialize());
        }

        public byte[] Serialize()
        {
            using (var stream = new NetStream())
            {
                using (var streamer = new BinaryStreamer(stream))
                {
                    EntityWrite(stream, streamer);
                }
                return stream.ToArray();
            }
            //return BinarySerializer.SerializeToBytes(this);
        }
        public static GPtr Deserialize(byte[] bytes)
        {
            return new GPtr(bytes);
            //return BinarySerializer.Deserialize<GPtr>(bytes);
        }

    }
}
