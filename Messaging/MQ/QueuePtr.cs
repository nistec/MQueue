using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Generic;
//using Nistec.Messaging.Server;
using System.IO;
using Nistec.Runtime;
using Nistec.IO;
using Nistec.Messaging.Io;
using Nistec.Serialization;

namespace Nistec.Messaging
{


    //public struct Ptr
    //{
    //    Guid ItemId{get;set;}
    //    string Source { get; set; }
    //}
    public struct Ptr
    {
        string m_Identifier;
        DateTime m_ArrivedTime;
        int m_TimeOut;
        string m_Host;
        //string m_Location;
        PtrState m_State;
        int m_Retry;

        public static string NewIdentifier()
        {
            return UUID.NewUuid().ToString();
        }

        //public Ptr(string identifier, int timeout)
        //{
        //    m_Identifier = identifier;
        //    m_ArrivedTime = DateTime.Now;
        //    m_TimeOut = timeout;
        //    m_Host = null;
        //    //m_Location = null;
        //    m_State = 0;
        //    m_Retry = 0;
        //}

        public Ptr(string identifier, int timeout, string hostName)//, string rootPath)
        {
            m_Identifier = identifier;
            m_ArrivedTime = DateTime.Now;
            m_TimeOut = timeout;
            m_Host = hostName;
            //m_Location = GetPtrLocation(rootPath, identifier); 
            m_State = 0;
            m_Retry = 0;
        }

        public Ptr(IQueueItem item, string hostName)
        {
            m_Identifier = item.Identifier;
            m_ArrivedTime = item.ArrivedTime;
            m_TimeOut = 0;// item.Expiration;
            m_Host = hostName;
            //m_Location = GetPtrLocation(item, host);
            m_State = 0;
            m_Retry = 0;
        }

       

        //public Ptr(Message item, string hostName)
        //{
        //    m_Identifier = item.Identifier;
        //    m_ArrivedTime = item.ArrivedTime;
        //    m_TimeOut = item.Expiration;
        //    m_Host = hostName;
        //    //m_Location = GetPtrLocation(item, host);
        //    m_State = 0;
        //    m_Retry = 0;
        //}

        public Ptr(Ptr item, PtrState state)
        {
            m_Identifier = item.Identifier;
            m_ArrivedTime = item.ArrivedTime;
            m_TimeOut = item.TimeOut;
            m_Host = item.Host;
            //m_Location = GetPtrLocation(item, host);
            m_State = state;
            m_Retry = item.Retry;
        }

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Identifier); }
        }

        public static Ptr Empty
        {
            get { return new  Ptr(); }
        }

        public static Ptr Get(IQueueItem item, PtrState state)
        {
            return new Ptr()
            {
                m_Identifier = item.Identifier,
                m_ArrivedTime = item.ArrivedTime,
                m_TimeOut = 0,// item.Expiration,
                m_Host = item.Host,
                m_State = state,
                m_Retry = 0
            };
        }

        public static Ptr Get(string identifier, PtrState state)
        {
            return new Ptr()
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
        public string Identifier { get { return m_Identifier; } }
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
            streamer.WriteString(Identifier);
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
            m_Identifier = streamer.ReadString();
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
            string filename = Assists.GetInfoFilename(rootPath, m_Host, Identifier);
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
        public static Ptr ReadFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return Ptr.Empty;
            }
            var ptr = Ptr.Empty;
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
            if (obj is Ptr)
            {
                return ((Ptr)obj).Identifier == Identifier;
            }
            return false;
        }

        public bool Equals(Ptr ptr)
        {
            return ptr.Identifier == Identifier;
        }

        public bool Equals(string identifier)
        {
            return identifier == Identifier;
        }


        //internal static string GetPtrLocation(string host, string priority, string folderId, long uniqueId)
        //{
        //    string hostPath = host;// Path.Combine(rootPath, host);
        //    string ppath = Path.Combine(hostPath, priority);
        //    string fpath = Path.Combine(ppath, folderId);
        //    return string.Format("{0}\\{1}{2}", fpath, uniqueId, QueueDefaults.QueueItemExt);
        //}

        //internal static string GetPtrLocation(string path, Guid itemId)
        //{
        //    return string.Format("{0}\\{1}{2}", path, itemId.UniqueId(), QueueDefaults.QueueItemExt);
        //}

        //internal static string GetPtrLocation(string host, string folderId, long uniqueId)
        //{
        //    string ppath = host;// Path.Combine(rootPath, host);
        //    string fpath = Path.Combine(ppath, folderId);
        //    return string.Format("{0}\\{1}{2}", fpath, uniqueId, QueueDefaults.QueueItemExt);
        //}

        //internal static string GetPtrLocation(string host, string folderId, long uniqueId, Priority priority)
        //{
        //    string ppath = host;// Path.Combine(rootPath, host);
        //    string fpath = Path.Combine(ppath, folderId);
        //    return string.Format("{0}\\{1}", fpath, Assists.GetFilename(uniqueId, priority));
        //}

        //public static string GetPtrLocation(string host, string identifier)
        //{
        //    string folderId = IoAssists.GetFolderId(identifier);
        //    return GetPtrLocation(host, folderId, identifier);
        //}

        //internal static string GetPtrLocation(string host, string folderId, string identifier)
        //{
        //    string fpath = Path.Combine(host, folderId);
        //    return string.Format("{0}\\{1}", fpath, Assists.GetFilename(identifier));
        //}

        //internal static string GetPtrLocation(IQueueItem item, string host)
        //{
        //    return GetPtrLocation(host, item.FolderId, item.Identifier);
        //}

        //internal static string GetPtrLocation(Message item, string host)
        //{
        //    return GetPtrLocation(host, item.FolderId, item.Identifier);
        //}

    }
}
