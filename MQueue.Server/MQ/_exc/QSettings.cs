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
using Nistec.Data.Sqlite;
using System.Data.SQLite;

namespace Nistec.Messaging
{


    /// <summary>
    /// AsyncQueueThread
    /// </summary>
    [Serializable]
    [SecurityPermission(SecurityAction.Assert)]
    public class QSettings : ISerialEntity
    {

        #region properties

        public const byte DefaultMaxRetry = 5;

        public string ServerPath { get; set; }
        public string QueueName { get; set; }
        public bool IsTrans { get; set; }
        public byte MaxRetry{ get; set; }
        public CoverMode Mode { get; set; }
        public string CoverPath { get; set; }
        public int ConnectTimeout { get; set; }
        public bool ReloadOnStart{ get; set; }

        //Persist
        public CommitMode CommitMode { get; set; }
        public SynchronizationModes SyncMode { get; set; }
        public SQLiteJournalModeEnum JournalMode { get; set; }
        public int PageSize { get; set; }
        public int CacheSize { get; set; }

        public string Print()
        {
            return string.Format("QueueName: {0}, ServerPath: {1}, IsTrans: {2}, Mode: {3}, ConnectTimeout: {4}, CoverPath:{5}",
                QueueName, ServerPath, IsTrans, Mode, ConnectTimeout, CoverPath
                );
        }
        
      
        /// <summary>
        /// Get if is valid queue properties if not throw exception
        /// </summary>
        /// <returns></returns>
        public void Validate()
        {

            if (string.IsNullOrWhiteSpace(QueueName))
            {
                throw new ArgumentNullException("QueueName");
            }

            if (PageSize < 1024)
                PageSize = 1024;
            else if (PageSize > 65536)
                PageSize = 65536;

            if (CacheSize < 1000)
                CacheSize = 10000;

        }
        #endregion

        #region ctor

        internal QSettings()
        {
            ServerPath = "localhost";
            Mode = CoverMode.Memory;
            IsTrans = false;
            MaxRetry = DefaultMaxRetry;
            ReloadOnStart = false;
        }

        /// <summary>
        /// QProperties ctor
        /// </summary>
        /// <param name="queueName"></param>
        public QSettings(string queueName)
        {
            ServerPath = "localhost";
            QueueName = queueName;
            Mode = CoverMode.Memory;
            IsTrans = false;
            MaxRetry = DefaultMaxRetry;
            ReloadOnStart = false;
        }

        /// <summary>
        /// QProperties ctor
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="isTrans"></param>
        /// <param name="mode"></param>
        public QSettings(string queueName, bool isTrans, CoverMode mode)
        {
            ServerPath = "localhost";
            QueueName = queueName;
            Mode = mode;
            IsTrans = isTrans;
            MaxRetry = DefaultMaxRetry;
            ReloadOnStart = false;
        }

        /// <summary>
        /// QProperties ctor
        /// </summary>
        /// <param name="node"></param>
        public QSettings(XmlNode node)
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
            ConnectTimeout = (int)parser.GetAttributeValue(node, "ConnectTimeout", "value", (int)5000);
            ReloadOnStart = Types.ToBool(parser.GetAttributeValue(node, "ReloadOnStart", "value", "false"), false);
            CommitMode =(CommitMode) (byte)parser.GetAttributeValue(node, "MaxRetry", "value", (byte)CommitMode.OnDisk);
            SyncMode =(SynchronizationModes) (byte)parser.GetAttributeValue(node, "MaxRetry", "value", (byte)SynchronizationModes.Normal);
            JournalMode =(SQLiteJournalModeEnum) (byte)parser.GetAttributeValue(node, "MaxRetry", "value", (byte)SQLiteJournalModeEnum.Wal);
            PageSize = (int)parser.GetAttributeValue(node, "MaxRetry", "value", (int)4096);
            CacheSize = (int)parser.GetAttributeValue(node, "MaxRetry", "value", (int)100000);

        }

        #endregion

        #region methods

        public IDictionary ToDictionary()
        {
            IDictionary prop = new Hashtable();
            prop["ServerPath"] = ServerPath;
            prop["QueueName"] = QueueName;
            prop["Mode"] =(int) Mode;
            prop["IsTrans"] = IsTrans;
            prop["MaxRetry"] = MaxRetry;
            prop["ReloadOnStart"] = ReloadOnStart;
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
           return prop;
       }


        public static QSettings Create(IDictionary prop)
        {

            QSettings mqp = new QSettings(Types.NZ(prop["QueueName"], "MQueue"));
            mqp.ServerPath = Types.NZ(prop["ServerPath"], "localhost");
            mqp.QueueName = Types.NZ(prop["QueueName"], "MQueue");
            mqp.Mode = (CoverMode)Types.ToInt(prop["Mode"], (int)CoverMode.Memory);
            mqp.IsTrans = Types.ToBool(prop["IsTrans"], false);
            mqp.MaxRetry = (byte)Types.ToInt(prop["MaxRetry"], DefaultMaxRetry);
            mqp.ReloadOnStart = Types.ToBool(prop["ReloadOnStart"], false);


            return mqp;
        }

        public static QSettings Get(GenericNameValue gnv)
        {
            QSettings mqp = new QSettings()
            {
                QueueName = gnv.Get("QueueName"),
                ServerPath = gnv.Get("ServerPath", "localhost"),
                Mode = (CoverMode)gnv.Get<int>("Mode", (int)CoverMode.Memory),
                IsTrans = gnv.Get<bool>("IsTrans", false),
                MaxRetry = (byte)gnv.Get<byte>("MaxRetry", DefaultMaxRetry),
                ReloadOnStart = gnv.Get<bool>("ReloadOnStart", false)
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
            streamer.WriteValue(CoverPath);
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
            CoverPath = streamer.ReadString();
            
        }

       
        #endregion

    }
 
}


