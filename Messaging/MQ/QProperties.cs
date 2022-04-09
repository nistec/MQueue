using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Messaging;

using Nistec.Threading;
using Nistec.Data.SqlClient;
using Nistec.Data;
using System.Data;
using System.Data.SqlClient;
using Nistec.Runtime;
using System.Security.Permissions;
using System.Collections;
using System.Security.AccessControl;
using Microsoft.Win32;
using System.Xml;
using Nistec.Xml;
using Nistec.Generic;
using System.IO;
using Nistec.Serialization;
using Nistec.IO;


namespace Nistec.Messaging
{

    public interface IQProperties //: ISerialEntity
    {

        string ServerPath { get; }
        string QueueName { get; }
        bool IsTrans { get; }
        bool IsTopic { get; }
        byte MaxRetry { get; }
        CoverMode Mode { get; }
        string TargetPath { get; }
        int ConnectTimeout { get; }

        bool ReloadOnStart { get; }

        //Persist properties
        /// <summary>
        /// Use commit mode.
        /// Default is OnDisk;
        /// </summary>
        PersistCommitMode CommitMode { get; }

        QueueHost GetRoutHost();
        string Print();
       
        bool IsDbQueue { get; }
        bool IsFileQueue { get; }
        bool IsPersistent { get; }
    }

    /// <summary>
    /// AsyncQueueThread
    /// </summary>
    [Serializable]
    [SecurityPermission(SecurityAction.Assert)]
    public class QProperties : IQProperties, ISerialEntity
    {

        #region properties

        public const byte DefaultMaxRetry = 5;

        public string ServerPath { get; set; }
        public string QueueName { get; set; }
        public bool IsTrans { get; set; }
        public byte MaxRetry{ get; set; }
        public CoverMode Mode { get; set; }
        public string TargetPath { get; set; }
        public int ConnectTimeout { get; set; }
        public bool IsTopic { get; set; }

        //public QCover Cover { get; set; }

        public bool ReloadOnStart{ get; set; }

        //Persist properties
        /// <summary>
        /// Use commit mode.
        /// Default is OnDisk;
        /// </summary>
        public PersistCommitMode CommitMode { get; set; }


        public string Print()
        {
            return string.Format("QueueName: {0}, ServerPath: {1}, IsTrans: {2}, Mode: {3}, ConnectTimeout: {4}, TargetPath:{5}",
                QueueName, ServerPath, IsTrans, Mode, ConnectTimeout, TargetPath
                );
        }

        public bool IsPersistent
        {
            get
            {
                return Mode == CoverMode.Persistent;
            }
        }

        public bool IsDbQueue
        {
            get
            {
                return Mode == CoverMode.Db;
            }
          }
        public bool IsFileQueue
        {
            get
            {
                return Mode == CoverMode.FileStream;
            }
        }
        internal bool IsValid()
        {
            return IsValid(Encryption.Enlock());
        }

        /// <summary>
        /// Get if is valid queue properties if not throw exception
        /// </summary>
        /// <returns></returns>
        public bool IsValid(string lockKey)
        {

            if (!Encryption.Delock(lockKey))
            {
                throw new ArgumentNullException("Invalid Lock Key");
            }
            if (QueueName == null)
            {
                throw new ArgumentNullException("QueueName");
            }
            if (QueueName.Length == 0)
            {
                throw new ArgumentException("InvalidParameter", "QueueName");
            }
            if (IsDbQueue)
            {
                throw new Exception("Not supported yet");
                //if (string.IsNullOrEmpty(connectionString))
                //{
                //    throw new ArgumentException("Invalid Connection");
                //}
            }
            if(Mode== CoverMode.Rout && TargetPath == null || TargetPath == "")
            {
                throw new Exception("Invalid Cover Path for Rout mod");
            }
            return true;
        }
        #endregion

        #region ctor

        internal QProperties()
        {
            ServerPath = "localhost";
            Mode = CoverMode.Memory;
            IsTrans = false;
            MaxRetry = DefaultMaxRetry;
            ReloadOnStart = false;
            IsTopic = false;
            TargetPath = null;
            CommitMode = PersistCommitMode.None;
        }

        /// <summary>
        /// QProperties ctor
        /// </summary>
        /// <param name="queueName"></param>
        public QProperties(string queueName)
        {
            ServerPath = "localhost";
            QueueName = queueName;
            Mode = CoverMode.Memory;
            IsTrans = false;
            MaxRetry = DefaultMaxRetry;
            ReloadOnStart = false;
            IsTopic = false;
            TargetPath = null;
            CommitMode = PersistCommitMode.None;
        }

        /// <summary>
        /// QProperties ctor
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="isTrans"></param>
        /// <param name="mode"></param>
        public QProperties(string queueName, bool isTrans, CoverMode mode)
        {
            ServerPath = "localhost";
            QueueName = queueName;
            Mode = mode;
            IsTrans = isTrans;
            MaxRetry = DefaultMaxRetry;
            ReloadOnStart = false;
            IsTopic = false;
            TargetPath = null;
            CommitMode = PersistCommitMode.None;
        }

        /// <summary>
        /// QProperties ctor
        /// </summary>
        /// <param name="node"></param>
        public QProperties(XmlNode node)
        {
            //XmlNode node = xml.SelectSingleNode("//queueSettings");
            if (node == null)
            {
                throw new ArgumentException("Inavlid Xml Root, 'queueSettings' ");
            }
            
            XmlParser parser = new XmlParser(node.OuterXml);

            QueueName = parser.GetAttributeValue(node, "name", true);
            ServerPath = parser.GetAttributeValue(node, "ServerPath", "value", "localhost");
            Mode = (CoverMode)(int)parser.GetAttributeValue(node, "CoverMode", "value", (int)CoverMode.Memory);
            IsTrans = Types.ToBool(parser.GetAttributeValue(node, "IsTrans", "value", "false"), false);
            MaxRetry = (byte)parser.GetAttributeValue(node, "MaxRetry", "value", (int)DefaultMaxRetry);
            IsTopic = Types.ToBool(parser.GetAttributeValue(node, "IsTopic", "value", "false"), false);
            TargetPath = parser.GetAttributeValue(node, "TargetPath", "value", null);
            CommitMode = (PersistCommitMode)(int)parser.GetAttributeValue(node, "CommitMode", "value", (int)PersistCommitMode.None);
        }

        public QProperties(NetStream stream)
        {
            //XmlNode node = xml.SelectSingleNode("//queueSettings");
            if (stream == null)
            {
                throw new ArgumentException("Inavlid stream, 'queueSettings' ");
            }

            EntityRead(stream, null);

        }

        #endregion

        #region methods

        public QueueHost GetRoutHost()
        {

            if(Mode== CoverMode.Rout)
            {
                if(TargetPath== null || TargetPath == "")
                {
                    return null;
                }
                return QueueHost.Parse(TargetPath);
            }
            return null;
        }

        public IDictionary ToDictionary()
        {
            IDictionary prop = new Hashtable();
            prop["ServerPath"] = ServerPath;
            prop["QueueName"] = QueueName;
            prop["Mode"] =(int) Mode;
            prop["IsTrans"] = IsTrans;
            prop["MaxRetry"] = MaxRetry;
            prop["ReloadOnStart"] = ReloadOnStart;
            prop["TargetPath"] = TargetPath;
            prop["IsTopic"] = IsTopic;
            prop["CommitMode"] = (int)CommitMode;
            return prop;
        }

       public GenericNameValue ToArgs()
       {
           GenericNameValue prop = new GenericNameValue();
           prop["ServerPath"] = ServerPath;
           prop["QueueName"] = QueueName;
           prop["Mode"] = ((int)Mode).ToString();
           prop["IsTrans"] = IsTrans.ToString();
           prop["MaxRetry"] = MaxRetry.ToString();
           prop["ReloadOnStart"] = ReloadOnStart.ToString();
            prop["TargetPath"] = TargetPath;
            prop["IsTopic"] = IsTopic.ToString();
            prop["CommitMode"] = ((int)CommitMode).ToString();
            return prop;
       }


        public static QProperties Create(IDictionary prop)
        {

            QProperties mqp = new QProperties(Types.NZ(prop["QueueName"], "MQueue"));
            mqp.ServerPath = Types.NZ(prop["ServerPath"], "localhost");
            mqp.QueueName = Types.NZ(prop["QueueName"], "MQueue");
            mqp.Mode = (CoverMode)Types.ToInt(prop["Mode"], (int)CoverMode.Memory);
            mqp.IsTrans = Types.ToBool(prop["IsTrans"], false);
            mqp.MaxRetry = (byte)Types.ToInt(prop["MaxRetry"], DefaultMaxRetry);
            mqp.ReloadOnStart = Types.ToBool(prop["ReloadOnStart"], false);
            mqp.TargetPath = Types.NZ(prop["TargetPath"], null);
            mqp.IsTopic = Types.ToBool(prop["IsTopic"], false);
            mqp.CommitMode = (PersistCommitMode)Types.ToInt(prop["CommitMode"], (int)PersistCommitMode.None);
            return mqp;
        }

        public static QProperties Get(GenericNameValue gnv)
        {
            QProperties mqp = new QProperties()
            {
                QueueName = gnv.Get("QueueName"),
                ServerPath = gnv.Get("ServerPath", "localhost"),
                Mode = (CoverMode)gnv.Get<byte>("Mode", (byte)CoverMode.Memory),
                IsTrans = gnv.Get<bool>("IsTrans", false),
                MaxRetry = (byte)gnv.Get<byte>("MaxRetry", DefaultMaxRetry),
                ReloadOnStart = gnv.Get<bool>("ReloadOnStart", false),
                TargetPath = gnv.Get("TargetPath", null),
                IsTopic = gnv.Get<bool>("IsTopic", false),
                CommitMode = (PersistCommitMode)gnv.Get<byte>("CommitMode", (byte)PersistCommitMode.None),
            };
            return mqp;
        }
        public static QProperties ByCommaPipe(string cp)
        {
            var prop=  KeyValueUtil.ParseCommaPipe(cp);

            QProperties mqp = new QProperties()
            {
                ServerPath = Types.NZ(prop["ServerPath"], "localhost"),
                QueueName = Types.NZ(prop["QueueName"], "MQueue"),
                Mode = EnumExtension.Parse<CoverMode>(prop["Mode"],CoverMode.Memory),
                IsTrans = Types.ToBool(prop["IsTrans"], false),
                MaxRetry = (byte)Types.ToInt(prop["MaxRetry"], DefaultMaxRetry),
                ReloadOnStart = Types.ToBool(prop["ReloadOnStart"], false),
                TargetPath = Types.NZ(prop["TargetPath"], null),
                IsTopic = Types.ToBool(prop["IsTopic"], false),
                CommitMode = EnumExtension.Parse<PersistCommitMode>(prop["CommitMode"], PersistCommitMode.None),
            };
            return mqp;
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

            streamer.WriteFixedString(MessageContext.QPRO, 4);
            streamer.WriteValue((byte)Mode);
            streamer.WriteValue(IsTrans);
            streamer.WriteValue(MaxRetry);
            streamer.WriteValue(QueueName);
            streamer.WriteValue(ServerPath);
            //streamer.WriteValue(Cover);
            streamer.WriteValue(ConnectTimeout);
            streamer.WriteValue(TargetPath);
            streamer.WriteValue(IsTopic);
            streamer.WriteValue((byte)CommitMode);
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
            if (mmsg != MessageContext.QPRO)
            {
                throw new Exception("Incorrect message format");
            }

            Mode = (CoverMode)streamer.ReadValue<byte>();
            IsTrans = streamer.ReadValue<bool>();
            MaxRetry = streamer.ReadValue<byte>();
            QueueName = streamer.ReadString();
            ServerPath = streamer.ReadString();
            //Cover = streamer.ReadValue<QCover>();
            ConnectTimeout = streamer.ReadValue<int>();
            TargetPath = streamer.ReadString();
            IsTopic = streamer.ReadValue<bool>();
            CommitMode = (PersistCommitMode) streamer.ReadValue<byte>();
        }

       
        #endregion

    }
 
}


