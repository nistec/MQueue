using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Nistec.Data.SqlClient;
using Nistec.Data;
using System.Data;
using System.ComponentModel;

using Nistec.Runtime;
using Nistec.Xml;
using System.IO;
using Nistec.Generic;
using Nistec.Serialization;

namespace Nistec.Legacy
{

    #region DB structer
    /*
3	ID	int	4	0
0	ItemID	varchar	50	0
0	QueueName	varchar	50	0
0	Subject	varchar	50	0
0	Body	image	16	0
0	MessageId	int	4	0
0	ArrivedTime	datetime	8	0
0	Status	tinyint	1	0
0	Sender	nvarchar	50	0
0	Destination	nvarchar	50	0
0	SentTime	datetime	8	0
0	SenderId	int	4	0
0	OperationId	int	4	0
0	Notify	varchar	250	1
*/
    #endregion

    #region interface 
    /// <summary>
    /// IQueueItem
    /// </summary>
    public interface IQueueItem : IDisposable//,System.Runtime.Remoting.Messaging.IMessage
    {
       
        /// <summary>
        /// Get ItemId
        /// </summary>
        Guid ItemId { get; }
        /// <summary>
        /// Get MessageId
        /// </summary>
        int MessageId { get; set; }
        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        DateTime ArrivedTime { get; }
        /// <summary>
        /// Get SentTime
        /// </summary>
        DateTime SentTime { get; }
        /// <summary>
        /// Get Priority
        /// </summary>
        Nistec.Messaging.Priority Priority { get; }

        /// <summary>
        /// Get or Set Body
        /// </summary>
        object Body { get; set; }
        /// <summary>
        /// Get or Set Subject
        /// </summary>
        string Subject { get; set; }
        /// <summary>
        /// Get or Set Label
        /// </summary>
        string Label { get; set; }
        /// <summary>
        /// Get or Set Sender
        /// </summary>
        string Sender { get; set; }
        /// <summary>
        /// Get or Set Destination
        /// </summary>
        string Destination { get; set; }
        /// <summary>
        /// Get or Set Status
        /// </summary>
        Nistec.Messaging.ItemState Status { get; set; }
        /// <summary>
        /// Get or Set SenderId
        /// </summary>
        int SenderId { get; set; }
        /// <summary>
        /// Get or Set OperationId
        /// </summary>
        int OperationId { get; set; }
        /// <summary>
        /// Get Has Attach
        /// </summary>
        bool HasAttach { get; }
        /// <summary>
        /// Get AttachItems List
        /// </summary>
        IList<QueueAttachItem> AttachItems { get; }
        /// <summary>
        /// Get Retry
        /// </summary>
        int Retry { get; }
        /// <summary>
        /// Get or Set AppSpecific
        /// </summary>
        int AppSpecific { get; set; }
        /// <summary>
        /// Get or Set TransactionId
        /// </summary>
        string TransactionId { get; set; }
        /// <summary>
        /// Get or Set Server
        /// </summary>
        int Server { get; set; }
        /// <summary>
        /// Get or Set TimeOut in seconds
        /// </summary>
        int TimeOut { get; set; }

        /// <summary>
        /// Get or Set Identifer
        /// </summary>
        int Identifer { get; set; }
        ///// <summary>
        ///// Get or Set AttachStream
        ///// </summary>
        //string AttachStream { get; set; }
        /// <summary>
        /// Get or Set Notify
        /// </summary>
        string Notify { get; set; }
        /// <summary>
        /// Get or Set Price
        /// </summary>
        decimal Price { get; set; }
        /// <summary>
        /// Get or Set Segments
        /// </summary>
        int Segments { get; set; }
        /// <summary>
        /// Get or Set ClientContext
        /// </summary>
        string ClientContext { get; set; }

        /// <summary>
        /// Serialize QueueItem to string
        /// </summary>
        string ToString();
        /// <summary>
        /// Serialize QueueItem to xml
        /// </summary>
        string Serialize();
        /// <summary>
        /// AddAttachment
        /// </summary>
        /// <param name="item"></param>
        void AddAttachment(QueueAttachItem item);
        /// <summary>
        /// AddAttachment
        /// </summary>
        /// <param name="items"></param>
        void AddAttachment(QueueAttachItem[] items);
        /// <summary>
        /// AddAttachment
        /// </summary>
        /// <param name="attachStream"></param>
        /// <param name="attachPath"></param>
        void AddAttachment(string attachStream, string attachPath);
        /// <summary>
        /// Get Copy of Queue item
        /// </summary>
        /// <returns></returns>
        QueueItem Copy();

        /// <summary>
        /// Get indicate wether the item is timeout 
        /// </summary>
        bool IsTimeOut { get; }
        ///// <summary>
        ///// When start receive item from queue
        ///// </summary>
        ///// <param name="timeout"></param>
        //void BeginReceive(TimeSpan timeout);
        ///// <summary>
        ///// Get indicate wether the item is timeout 
        ///// </summary>
        //bool IsTimeOutReceive { get; }

        IDictionary ToDictionary();

        void Save(string path);
        void Delete(string path);
        double Duration();
    }

    #endregion

    public struct QueueItemPtr
    {
        public QueueItem Item;
    }

    //public class MessageItem :QueueItem, System.Runtime.Remoting.Messaging.IMessage
    //{
    //    public IDictionary Properties
    //    {
    //        get { return this.ToDictionary(); }
    //    }
    //}

    [Serializable]
    [ComVisible(true)]
    public sealed class QueueItem : IQueueItem, IDisposable
    {
        #region memebers

        //public const int DefaultTimeOut = 86400;
        public static readonly TimeSpan DefaultTimeOut = TimeSpan.FromMinutes(60);//4294967295);
        public static readonly int DefaultTimeOutInSecond = 3600;

        private Guid itemId;
        private int messageId;
        
        private object body;
        private string subject;
        private string sender;
        private string destination;
        private Nistec.Messaging.ItemState status;
        private int senderId;
        private int operationId;
        private string notify;
        private decimal price;
        private int identifer;
        private bool hasAttach;
        //private string attachStream;

        private int timeOut;

        internal Nistec.Messaging.Priority priority;
        internal DateTime arrivedTime;
        internal DateTime sentTime;
        internal int retry;
        private string transactionId;
        private int server;
        private int appSpecific;
        private string label;
        private int segments;
        private string clientContext;
        private List<QueueAttachItem> attachItems;

        #endregion

        #region locking

        private int m_Lock;

        internal void LOCK()
        {
            Interlocked.Exchange(ref m_Lock, 1);
        }
        internal void UNLOCK()
        {
            Interlocked.Exchange(ref m_Lock, 0);
        }
        internal int ISLOCK()
        {
            return m_Lock;
        }
        #endregion

        #region property

        /// <summary>
        /// Get ItemId
        /// </summary>
        public Guid ItemId { get { return itemId; } set { itemId = value; } }
        /// <summary>
        /// Get MessageId
        /// </summary>
        public int MessageId { get { return messageId; } set { messageId = value; } }
        /// <summary>
        /// Get Priority
        /// </summary>
        public Nistec.Messaging.Priority Priority { get { return priority; } set { priority = value; } }
        /// <summary>
        /// Get Retry
        /// </summary>
        public int Retry { get { return retry; } set { retry = value; } }
 
        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        public DateTime ArrivedTime { get { return arrivedTime; } set { arrivedTime = value; } }
        /// <summary>
        /// Get SentTime
        /// </summary>
        public DateTime SentTime { get { return sentTime; } set { sentTime = value; } }
 
        /// <summary>
        /// Get or Set Body
        /// </summary>
        public object Body { get { return body; } set { body = value; } }
        /// <summary>
        /// Get or Set Subject
        /// </summary>
        public string Subject { get { return subject; } set { subject = value; } }
        /// <summary>
        /// Get or Set Sender
        /// </summary>
        public string Sender { get { return sender; } set { sender = value; } }
        /// <summary>
        /// Get or Set Destination
        /// </summary>
        public string Destination { get { return destination; } set { destination = value; } }
        /// <summary>
        /// Get or Set Status
        /// </summary>
        public Nistec.Messaging.ItemState Status { get { return status; } set { status = value; } }
        /// <summary>
        /// Get or Set SenderId
        /// </summary>
        public int SenderId { get { return senderId; } set { senderId = value; } }
        /// <summary>
        /// Get or Set OperationId
        /// </summary>
        public int OperationId { get { return operationId; } set { operationId = value; } }
        /// <summary>
        /// Get  HasAttach
        /// </summary>
        public bool HasAttach { get { return hasAttach; } set { hasAttach = value; } }
        /// <summary>
        /// Get or Set Notify
        /// </summary>
        public string Notify { get { return notify; } set { notify = value; } }
        /// <summary>
        /// Get or Set Price
        /// </summary>
        public decimal Price { get { return price; } set { price = value; } }

        /// <summary>
        /// Get or Set Identifer
        /// </summary>
        public int Identifer { get { return identifer; } set { identifer = value; } }
         /// <summary>
        /// Get or Set Label
        /// </summary>
        public string Label { get { return label; } set { label = value; } }
        /// <summary>
        /// Get or Set TransactionId
        /// </summary>
        public string TransactionId { get { return transactionId; } set { transactionId = value; } }
        /// <summary>
        /// Get or Set AppSpecific
        /// </summary>
        public int AppSpecific { get { return appSpecific; } set { appSpecific = value; } }
        /// <summary>
        /// Get or Set Segments
        /// </summary>
        public int Segments { get { return segments; } set { segments = value; } }
        /// <summary>
        /// Get or Set ClientContext
        /// </summary>
        public string ClientContext { get { return clientContext; } set { clientContext = value; } }
        /// <summary>
        /// Get or Set Server
        /// </summary>
        public int Server { get { return server; } set { server = value; } }
        /// <summary>
        /// Get or Set timeout in seconds
        /// </summary>
        public int TimeOut { get { return timeOut; } set { timeOut = value; } }

        /// <summary>
        /// Get indicate wether the item is timeout 
        /// </summary>
        public bool IsTimeOut
        {
            get {return TimeSpan.FromSeconds(timeOut) < DateTime.Now.Subtract(arrivedTime); }
        }
        #endregion

        #region attachments

        /// <summary>
        /// Get AttachItems Read only List
        /// </summary>
        [ReadOnly (true)]
        public IList<QueueAttachItem>  AttachItems
        {
            get 
            {
                if (attachItems == null)
                {
                    attachItems = new List<QueueAttachItem>();
                }
                return attachItems.AsReadOnly(); 
            }
        }

 
        /// <summary>
        /// AddAttachment
        /// </summary>
        /// <param name="attachStream"></param>
        /// <param name="attachPath"></param>
        public void AddAttachment(string attachStream,string attachPath)
        {
            if (attachItems == null)
            {
                attachItems = new List<QueueAttachItem>();
            }
 
            attachItems.Add(new QueueAttachItem(this.ItemId,this.MessageId,attachStream,attachPath));
            hasAttach = attachItems.Count > 0;
        }

        /// <summary>
        /// AddAttachment
        /// </summary>
        /// <param name="item"></param>
        public void AddAttachment(QueueAttachItem item)
        {
            if (attachItems == null)
            {
                attachItems = new List<QueueAttachItem>();
            }
            attachItems.Add(item);
            hasAttach = attachItems.Count>0;
        }
        /// <summary>
        /// AddAttachment
        /// </summary>
        /// <param name="items"></param>
        public void AddAttachment(QueueAttachItem[] items)
        {
            if (attachItems == null)
            {
                attachItems = new List<QueueAttachItem>();
            }
            attachItems.AddRange(items);
            hasAttach = attachItems.Count > 0;
        }

        /// <summary>
        /// AddAttachment
        /// </summary>
        /// <param name="drAttach"></param>
        internal void AddAttachment(DataRow[] drAttach)
        {
            if (drAttach != null)
            {
                if (attachItems != null)
                {
                    attachItems.Clear();
                    attachItems = null;
                }
                attachItems = new List<QueueAttachItem>();
                foreach (DataRow d in drAttach)
                {
                    attachItems.Add(new QueueAttachItem(d));
                }
                hasAttach = attachItems.Count > 0;
            }
        }
        #endregion

        #region methods

        //private DateTime startReceive;
        //private TimeSpan receiveTimeout;
        //public void BeginReceive(TimeSpan timeout)
        //{
        //    startReceive = DateTime.Now;
        //    receiveTimeout = timeOut;
        //}
        ///// <summary>
        ///// Get indicate wether the item is timeout 
        ///// </summary>
        //public bool IsTimeOutReceive
        //{
        //    get 
        //    {
        //        if (receiveTimeout == TimeSpan.Zero)
        //            return IsTimeOut;
        //        return receiveTimeout < DateTime.Now.Subtract(startReceive); 
        //    }
        //}

        /// <summary>
        ///  Get Copy of Queue item
        /// </summary>
        /// <returns></returns>
        public QueueItem Copy()
        {
            //string xml=this.Serialize();
            //return QueueItem.Deserialize(xml);

            QueueItem item = new QueueItem();
            item.appSpecific = this.appSpecific;
            item.arrivedTime = this.arrivedTime;
            item.attachItems = this.attachItems;
            item.body = this.body;
            item.clientContext = this.clientContext;
            item.destination = this.destination;
            item.hasAttach = this.hasAttach;
            item.identifer = this.identifer;
            item.m_Lock = this.m_Lock;
            item.itemId = this.itemId;
            item.label = this.label;
            item.messageId = this.messageId;
            item.operationId = this.operationId;
            item.price = this.price;
            item.priority = this.priority;
            item.notify = this.notify;
            item.retry = this.retry;
            item.segments = this.segments;
            item.sender = this.sender;
            item.senderId = this.senderId;
            item.sentTime = this.sentTime;
            item.server = this.server;
            item.status = this.status;
            item.subject = this.subject;
            item.timeOut = this.timeOut;
            item.transactionId = this.transactionId;
            return item;

        }
        /// <summary>
        /// Serialize QueueItem to xml string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return XmlUtil.Serialize(this);// ObjectToXml.Instance.Serialize(this);
        }

        internal void DoRetry()
        {
            retry++;
            itemId = Guid.NewGuid();
        }

        internal void SetRetry()
        {
            retry++;
        }

        #endregion

        #region ctor

        /// <summary>
        /// QueueItem ctor
        /// </summary>
        /// <param name="priority"></param>
        public QueueItem(/*int messageId,*/ Nistec.Messaging.Priority priority)
            : this(priority, DefaultTimeOut)
        {
        }

        /// <summary>
        /// QueueItem ctor with time out
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="timeout"></param>
        public QueueItem(/*Guid itemId, int messageId,*/ Nistec.Messaging.Priority priority, TimeSpan timeout)
        {
            this.itemId = UUID.NewUuid();
            this.priority = priority;

            this.arrivedTime = DateTime.Now;
            this.sentTime = DateTime.Now;
            this.timeOut =(int) timeout.TotalSeconds;// DateTime.Now.AddSeconds((timeout <= 0) ? DefaultTimeOut : timeout);
        }

        /// <summary>
        /// QueueItem ctor with time out
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="priority"></param>
        public QueueItem(Guid itemId, Nistec.Messaging.Priority priority)
        {
            this.itemId = itemId;
            this.priority = priority;

            this.arrivedTime = DateTime.Now;
            this.sentTime = DateTime.Now;
            this.timeOut = (int)DefaultTimeOut.TotalSeconds;// DateTime.Now.AddSeconds((timeout <= 0) ? DefaultTimeOut : timeout);
        }

        /// <summary>
        /// QueueItem ctor with time out
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="priority"></param>
        /// <param name="timeout"></param>
        public QueueItem(Guid itemId, Nistec.Messaging.Priority priority, TimeSpan timeout)
        {
            this.itemId = itemId;
            this.priority = priority;

            this.arrivedTime = DateTime.Now;
            this.sentTime = DateTime.Now;
            this.timeOut = (int)timeout.TotalSeconds;// DateTime.Now.AddSeconds((timeout <= 0) ? DefaultTimeOut : timeout);
        }

        /// <summary>
        /// QueueItem
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="messageId"></param>
        public QueueItem()
            : this(Nistec.Messaging.Priority.Normal, DefaultTimeOut)//Guid itemId, int messageId)
        {
       }

  

 
        /// <summary>
        /// QueueItem ctor
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="serializeBody">should serialize body to base 64 string</param>
        public QueueItem(DataRow dr, bool serializeBody)
        {
            if (dr == null)
                return ;
            this.itemId = UUID.NewUuid();//= new Guid(dr["ItemID"].ToString());
            this.subject = dr["Subject"].ToString();
            this.messageId = Types.ToInt(dr["MessageId"], 0);
            this.priority =(Nistec.Messaging.Priority) Types.ToInt(dr["Priority"], 0);
            int itimeOut = Types.ToInt(dr["TimeOut"], DefaultTimeOutInSecond);
            this.timeOut = (itimeOut <= 0) ? DefaultTimeOutInSecond : itimeOut;
            //this.timeOut = TimeSpan.FromSeconds((double)itimeOut);//DateTime.Now.AddSeconds(DefaultTimeOut));

            object drBody=dr["Body"];
            if (serializeBody && drBody != null)
            {
                try
                {
                    this.body = BinarySerializer.DeserializeFromBase64(dr["Body"].ToString());
                }
                catch
                {
                    this.body = drBody;
                }
            }
            else
            {
                this.body = drBody;
            }
            this.status = (Nistec.Messaging.ItemState)Types.ToInt(dr["Status"], 0);
            this.sender = Types.NZ(dr["Sender"],"");
            this.destination = dr["Destination"].ToString();
            this.senderId = Types.ToInt(dr["SenderId"], 0);
            this.operationId = Types.ToInt(dr["OperationId"], 0);
            this.identifer = Types.ToInt(dr["Identifer"], 0);
            this.hasAttach = Types.ToBool(dr["HasAttach"], false);
            this.price = Types.ToDecimal(dr["Price"], 0);
            this.retry = Types.ToInt(dr["Retry"], 0);
            this.appSpecific = Types.ToInt(dr["AppSpecific"], 0);
            this.segments = Types.ToInt(dr["Segments"], 0);
            this.server = Types.ToInt(dr["Server"], 0);
            this.clientContext = Types.NZ(dr["ClientContext"], "");
            this.label = Types.NZ(dr["Label"], "");
            this.notify = Types.NZ(dr["Notify"], "");
            this.transactionId = Types.NZ(dr["TransactionId"], "");

            object o = null;
            //o = dr["ArrivedTime"];
            //if (o != null && o != DBNull.Value)
            //    this.arrivedTime = Types.ToDateTime(o.ToString(), DateTime.Now);
            
            this.arrivedTime = DateTime.Now;

            o = dr["SentTime"];
            if (o != null && o != DBNull.Value)
                this.sentTime = Types.ToDateTime(o.ToString(), DateTime.Now);


            //o = dr["AttachStream"];
            //if (o != null && o != DBNull.Value)
            //{
            //    this.attachStream =  (System.IO.Stream)o;
            //}

 
        }


        /// <summary>
        /// QueueItem ctor
        /// </summary>
        /// <param name="dr">IDictionary</param>
        public QueueItem(IDictionary dr)
        {
            if (dr == null)
                return;
            this.itemId = (Guid)dr["ItemID"];
            this.subject = (string)dr["Subject"];
            this.messageId = (int)dr["MessageId"];
            this.priority = (Nistec.Messaging.Priority)(int)dr["Priority"];
            this.timeOut = (int)dr["TimeOut"];
            this.body = dr["Body"];
            this.status = (Nistec.Messaging.ItemState)(int)dr["Status"];
            this.sender = (string)dr["Sender"];
            this.destination = (string)dr["Destination"];
            this.senderId = (int)dr["SenderId"];
            this.operationId = (int)dr["OperationId"];
            this.identifer = (int)dr["Identifer"];
            this.hasAttach = (bool)dr["HasAttach"];
            this.price = (decimal)dr["Price"];
            this.retry = (int)dr["Retry"];
            this.appSpecific = (int)dr["AppSpecific"];
            this.segments = (int)dr["Segments"];
            this.server = (int)dr["Server"];
            this.clientContext = (string)dr["ClientContext"];
            this.label = (string)dr["Label"];
            this.notify = (string)dr["Notify"];
            this.transactionId = (string)dr["TransactionId"];
            this.arrivedTime = (DateTime)dr["ArrivedTime"];
            this.sentTime = (DateTime)dr["SentTime"];
        }

        //~QueueItem()
        //{
        //    Dispose(false);
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (attachItems != null)
                {
                    foreach (QueueAttachItem itm in attachItems)
                    {
                        itm.Dispose();
                    }
                    //attachItems.Clear();
                    attachItems = null;
                }
                body = null;
                subject = null;
                sender = null;
                destination = null;
                notify = null;
                transactionId = null;
                label = null;

                this.disposed = true;
            }
 
        }

       
        #endregion

        #region util

        internal void SetSentTime()
        {
            if (sentTime <= arrivedTime)
            {
                this.sentTime = DateTime.Now;
            }
        }

        public double Duration()
        {
          return  SentTime.Subtract(ArrivedTime).TotalSeconds;
        }

        /// <summary>
        /// Deserialize queue item from base64 string
        /// </summary>
        public static QueueItem Deserialize(string serItem)
        {
            object o = BinarySerializer.DeserializeFromBase64(serItem);//.DeserializeFromXml(xml, typeof(QueueItem));
            if (o == null)
                return null;
            if (!(o.GetType().IsAssignableFrom(typeof(QueueItem))))
            {
                throw new Exception("Type not valid");
            }
            return (QueueItem)o;
        }

        /// <summary>
        /// Deserialize queue item from byte array
        /// </summary>
        public static QueueItem Deserialize(byte[] bytes)
        {
            object o = BinarySerializer.Deserialize(bytes);//.DeserializeFromBytes(bytes);//.DeserializeFromXml(xml, typeof(QueueItem));
            if (o == null)
                return null;
            if (!(o.GetType().IsAssignableFrom(typeof(QueueItem))))
            {
                throw new Exception("Type not valid");
            }
            return (QueueItem)o;
        }

        public static QueueItem ReadFile(string filename)
        {
            if (!File.Exists(filename))
            {
                return null;
            }
            return  Deserialize(File.ReadAllBytes(filename));

        }

        /// <summary>
        /// Serialize Queue Item
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return BinarySerializer.SerializeToBase64(this);//.SerializeToXml(this,this.GetType());
        }

        /// <summary>
        /// Serialize Queue Item To ByteArray
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return BinarySerializer.SerializeToBytes(this);
        }

        /// <summary>
        /// Stores Queue Item it's child entities to specified stream.
        /// </summary>
        /// <param name="storeStream">Stream where to store mime entity.</param>
        public void ToStream(System.IO.Stream storeStream)
        {
            byte[] data = ToByteArray();
            storeStream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Get Queue Item as Stream
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
           return new MemoryStream(ToByteArray());
        }

        public string ToFileName(string path)
        {
            return IoHelper.PathFix(path + "\\" + itemId.ToString() + ".mcq");
        }

        public void Save(string path)
        {
            string filename = ToFileName(path);
            File.WriteAllBytes(filename, ToByteArray());

            //using (FileStream fs = File.Create(filename))
            //{
            //    fs.BeginWrite
            //    ToStream(fs);
            //    SysUtil.StreamCopy(message, fs);

            //    // Create message info file for the specified relay message.
            //    RelayMessageInfo messageInfo = new RelayMessageInfo(sender, to, date, false, targetHost);

            //    File.WriteAllBytes(filename, ToByteArray());
            //}
        }

        public void Save(string path, FileAttributes attribute)
        {
            string filename = ToFileName(path);
            File.WriteAllBytes(filename, ToByteArray());
            File.SetAttributes(filename, attribute);

            //using (FileStream fs = File.Create(filename))
            //{
            //    fs.BeginWrite
            //    ToStream(fs);
            //    SysUtil.StreamCopy(message, fs);

            //    // Create message info file for the specified relay message.
            //    RelayMessageInfo messageInfo = new RelayMessageInfo(sender, to, date, false, targetHost);

            //    File.WriteAllBytes(filename, ToByteArray());
            //}
        }
        public void Delete(string path)
        {
            IoHelper.DeleteFile(ToFileName(path));
        }

        public DataRow ToDataRow()
        {
            //System.Runtime.Serialization.ISerializable;
            //System.Xml.Serialization.IXmlSerializable
            DataRow dr = SQLCMD.GetQueueItemRowSchema();
            FillDataRow(dr);
            return dr;
        }

        internal void FillDataRow(DataRow dr)
        {

            dr["ItemID"] = itemId;
            dr["Status"] = this.status;
            dr["MessageId"] = this.messageId;
            dr["Priority"] = this.priority;
            dr["Retry"] = this.retry;
            dr["ArrivedTime"] = this.arrivedTime;
            dr["SentTime"] = this.sentTime;
            dr["Body"] = Types.NZ(this.body, "");
            dr["Subject"] = Types.NZ(this.subject, "");
            dr["Sender"] = Types.NZ(this.sender, "");
            dr["Destination"] = Types.NZ(this.destination, "");
            dr["SenderId"] = this.senderId;
            dr["OperationId"] = this.operationId;
            dr["HasAttach"] = this.hasAttach;
            dr["Notify"] = Types.NZ(this.notify, "");
            dr["Price"] = this.price;
            dr["Identifer"] = this.identifer;
            dr["Label"] = Types.NZ(this.label, "");
            dr["TransactionId"] = Types.NZ(this.transactionId, "");
            dr["AppSpecific"] = this.appSpecific;
            dr["Segments"] = this.segments;
            dr["ClientContext"] = Types.NZ(this.clientContext, "");
            dr["Server"] = this.server;
            dr["TimeOut"] = this.timeOut;
        }


        //public IDictionary Properties
        //{
        //    get { return this.ToDictionary(); }
        //}

        public IDictionary ToDictionary()
        {
            IDictionary dr = new Hashtable();
            dr["ItemID"] = itemId;
            dr["Status"] = (int)this.status;
            dr["MessageId"] = this.messageId;
            dr["Priority"] = (int)this.priority;
            dr["Retry"] = this.retry;
            dr["ArrivedTime"] = this.arrivedTime;
            dr["SentTime"] = this.sentTime;
            dr["Body"] = this.body;
            dr["Subject"] = this.subject;
            dr["Sender"] = this.sender;
            dr["Destination"] = this.destination;
            dr["SenderId"] = this.senderId;
            dr["OperationId"] = this.operationId;
            dr["HasAttach"] = this.hasAttach;
            dr["Notify"] = this.notify;
            dr["Price"] = this.price;
            dr["Identifer"] = this.identifer;
            dr["Label"] = this.label;
            dr["TransactionId"] = this.transactionId;
            dr["AppSpecific"] = this.appSpecific;
            dr["Segments"] = this.segments;
            dr["ClientContext"] = this.clientContext;
            dr["Server"] = this.server;
            dr["TimeOut"] = this.timeOut;
            return dr;
        }
        #endregion

    }

    [Serializable]
    public class QueueAttachItem : IDisposable
    {
        #region memebers

        private Guid attachId;
        private int msgId;
        private string attachStream;
        private string attachPath;

        #endregion

        #region property

        /// <summary>
        /// Get ItemId
        /// </summary>
        public Guid AttachId { get { return attachId; } }
        /// <summary>
        /// Get MessageId
        /// </summary>
        public int MsgId { get { return msgId; } }
        /// <summary>
        /// Get or Set AttachStream
        /// </summary>
        public string AttachStream
        {
            get { return attachStream; }
            set
            {
                attachStream = value;
            }
        }
        /// <summary>
        /// Get or Set AttachPath
        /// </summary>
        public string AttachPath
        {
            get { return attachPath; }
            set
            {
                attachPath = value;
            }
        }

        #endregion

        /// <summary>
        /// Serialize QueueItem to xml string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return XmlUtil.Serialize(this);// ObjectToXml.Instance.Serialize(this);
        }

        /// <summary>
        /// Serialize QueueItem to xml string
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return BinarySerializer.SerializeToBase64(this);// XmlUtil.Serialize(this);// ObjectToXml.Instance.Serialize(this);
        }
        /// <summary>
        /// Serialize Queue Item To ByteArray
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return BinarySerializer.SerializeToBytes(this);
        }
        /// <summary>
        /// Deserialize queue item from base64 string
        /// </summary>
        public static QueueAttachItem Deserialize(string serItem)
        {
            object o = BinarySerializer.DeserializeFromBase64(serItem);
            if (o == null)
                return null;
            if (!(o.GetType().IsAssignableFrom(typeof(QueueItem))))
            {
                throw new Exception("Type not valid");
            }
            return (QueueAttachItem)o;
        }

        /// <summary>
        /// Deserialize queue item from byte array
        /// </summary>
        public static QueueAttachItem Deserialize(byte[] bytes)
        {
            object o = BinarySerializer.Deserialize(bytes);//.DeserializeFromBytes(bytes);
            if (o == null)
                return null;
            if (!(o.GetType().IsAssignableFrom(typeof(QueueAttachItem))))
            {
                throw new Exception("Type not valid");
            }
            return (QueueAttachItem)o;
        }


        #region ctor

        /// <summary>
        /// QueueAttachItem ctor
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="messageId"></param>
        public QueueAttachItem(Guid itemId, int messageId)
        {
            this.attachId = itemId;
            this.msgId = messageId;
        }

         public QueueAttachItem(Guid itemId, int messageId,string stream,string link)
        {
            this.attachId = itemId;
            this.msgId = messageId;
            this.attachStream = stream;
            if (!string.IsNullOrEmpty(link))
            {
                this.attachPath = link;// new Uri(link);
            }
        }

         /// <summary>
        /// QueueItem ctor
        /// </summary>
        /// <param name="dr"></param>
        public QueueAttachItem(DataRow dr)
        {
            if (dr == null)
                return ;
            this.attachId = new Guid(dr["AttachId"].ToString());
            this.msgId = Types.ToInt(dr["MsgId"], 0);
            object o = null;
            o=dr["AttachStream"];
            if (o != null && o != DBNull.Value)
            {
                this.attachStream = o.ToString();
            }
            o = dr["AttachPath"];
            if (o != null && o != DBNull.Value)
            {
                this.attachPath = o.ToString();//new Uri(o.ToString());
            }
        }

        public void Dispose()
        {
            attachStream = null;
            GC.SuppressFinalize(this);
        }

        //private bool disposed = false;


        //private void Dispose(bool disposing)
        //{
        //    if (!this.disposed)
        //    {
        //        if (disposing)
        //        {
        //            attachStream = null;
        //        }

        //        // Call the appropriate methods to clean up 
        //        // unmanaged resources here.
        //        // If disposing is false, 
        //        // only the following code is executed.
        //        //CloseHandle(handle);
        //        //handle = IntPtr.Zero;
        //    }
        //    disposed = true;
        //}

        // Use interop to call the method necessary  
        // to clean up the unmanaged resource.
        //[System.Runtime.InteropServices.DllImport("Kernel32")]
        //private extern static Boolean CloseHandle(IntPtr handle);

        #endregion

        #region util

        //public DataRow ToDataRow()
        //{
        //    DataRow dr = SQLCMD.GetQueueAttachItemRowSchema();
        //    FillDataRow(dr);
        //    return dr;
        //}

        internal void FillDataRow(DataRow dr)
        {
            dr["AttachId"] = attachId;
            dr["MsgId"] = this.msgId;
            dr["AttachStream"] = this.attachStream;
            dr["AttachPath"] = this.attachPath;
  
        }



        #endregion

    }

    /// <summary>
    /// This class holds Relay_Queue queued item.
    /// </summary>
    public class Relay_QueueItem
    {
        private string m_Queue = null;
        private string m_From = "";
        private string m_To = "";
        private string m_MessageID = "";
        private Stream m_MessageStream = null;
        private object m_Tag = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="queue">Item owner queue.</param>
        /// <param name="from">Sender address.</param>
        /// <param name="to">Target recipient address.</param>
        /// <param name="messageID">Message ID.</param>
        /// <param name="message">Raw mime message. Message reading starts from current position.</param>
        /// <param name="tag">User data.</param>
        internal Relay_QueueItem(string queue, string from, string to, string messageID, Stream message, object tag)
        {
            m_Queue = queue;
            m_From = from;
            m_To = to;
            m_MessageID = messageID;
            m_MessageStream = message;
            m_Tag = tag;
        }


        #region Properties Implementation

        /// <summary>
        /// Gets this relay item owner queue.
        /// </summary>
        public string Queue
        {
            get { return m_Queue; }
        }

        /// <summary>
        /// Gets from address.
        /// </summary>
        public string From
        {
            get { return m_From; }
        }

        /// <summary>
        /// Gets target recipient.
        /// </summary>
        public string To
        {
            get { return m_To; }
        }

        /// <summary>
        /// Gets message ID which is being relayed now.
        /// </summary>
        public string MessageID
        {
            get { return m_MessageID; }
        }

        /// <summary>
        /// Gets raw mime message which must be relayed.
        /// </summary>
        public Stream MessageStream
        {
            get { return m_MessageStream; }
        }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag
        {
            get { return m_Tag; }

            set { m_Tag = value; }
        }

        #endregion

    }
}
