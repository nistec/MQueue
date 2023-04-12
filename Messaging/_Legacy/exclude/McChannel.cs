using Nistec.Runtime;
using Nistec.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Xml;

namespace Nistec.Legacy
{
    /// <summary>
    /// McChannelProperties
    /// </summary>
    public class McChannelProperties : McQueueProperties
    {
        public int MaxThread;
        public int MinThread;
        public int AvailableThread;
        public bool AutoThreadSettings;
        public bool UseThreadSettings;
        public bool Enabled;
        //public string ChannelName;
        public int RecieveTimeout;
        public int IntervalSetting;
        public bool UseMessageQueueListner;
        public int MaxCapacity;

        public McQueueProperties QueueProperties
        {
            get { return this as McQueueProperties; }
        }

        public McChannelProperties(string queueName)
            : base(queueName)
        {
            //ChannelName = channelName;
            MinThread = 1;
            MaxThread = 2;
            AvailableThread = 1;
            AutoThreadSettings = false;
            UseThreadSettings = false;
            Enabled = true;
            RecieveTimeout = 60 * 30;//30 minute
            IntervalSetting = 60000 * 10;//minute
            UseMessageQueueListner = false;
            MaxCapacity = 1000000;

        }

        public McChannelProperties(string queueName, int minThread, int maxThread, int availableThread, bool autoThreadSettings, bool useMessageQueueListner)
            : base(queueName)
        {
            //ChannelName = channelName;
            MinThread = minThread;
            MaxThread = maxThread;
            AvailableThread = availableThread;
            AutoThreadSettings = autoThreadSettings;
            UseThreadSettings = false;
            Enabled = true;
            RecieveTimeout = 60 * 30;//30 minute
            IntervalSetting = 60000 * 10;//minute
            UseMessageQueueListner = useMessageQueueListner;

        }


    }

    #region McQueueProperties
    /// <summary>
    /// AsyncQueueThread
    /// </summary>
    [Serializable]
    [SecurityPermission(SecurityAction.Assert)]
    public class McQueueProperties
    {
        public const byte DefaultMaxRetry = 5;
        internal const string EmbeddedCnn = @"ServerType=1;User=SYSDBA;Password=masterkey;Database=mcqueuedb.fdb;Dialect=3";

        public int Server;
        public string QueueName;
        public bool IsTrans;
        public byte MaxRetry;
        //public QueueProvider Provider;
        public Messaging.CoverMode CoverMode = Messaging.CoverMode.Memory;
        public bool ReloadOnStart;

        //private string connectionString;

        //public string ConnectionString
        //{
        //    get 
        //    {
        //        if (Provider == QueueProvider.Embedded)
        //        {
        //            return EmbeddedCnn;
        //        }

        //        return connectionString; 
        //    }
        //    set
        //    {
        //        connectionString = value;
        //    }
        //}

        public bool IsDbQueue
        {
            get
            {
                return (int)CoverMode > (int)Messaging.CoverMode.Memory && CoverMode != Messaging.CoverMode.FileStream;
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
            if ((int)CoverMode > (int)Messaging.CoverMode.Memory && CoverMode != Messaging.CoverMode.FileStream)
            {
                //if (string.IsNullOrEmpty(connectionString))
                //{
                //    throw new ArgumentException("Invalid Connection");
                //}
            }
            return true;
        }

        internal McQueueProperties()
        {

        }

        /// <summary>
        /// McQueueProperties ctor
        /// </summary>
        /// <param name="queueName"></param>
        public McQueueProperties(string queueName)
        {
            Server = 0;
            QueueName = queueName;
            //ConnectionString ="";// EmbeddedCnn;
            CoverMode = Messaging.CoverMode.Memory;
            //Enabled = true;
            IsTrans = false;
            MaxRetry = DefaultMaxRetry;
            //Provider = QueueProvider.None;
            ReloadOnStart = false;
        }

        /// <summary>
        /// McQueueProperties ctor
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="isTrans"></param>
        /// <param name="recoverable"></param>
        public McQueueProperties(string queueName, bool isTrans, Messaging.CoverMode coverMode)
        {
            Server = 0;
            QueueName = queueName;
            //ConnectionString = "";// EmbeddedCnn;
            CoverMode = coverMode;
            //Enabled = true;
            IsTrans = isTrans;
            MaxRetry = DefaultMaxRetry;
            //Provider = QueueProvider.SqlServer;
            ReloadOnStart = false;
        }

        /// <summary>
        /// McQueueProperties ctor
        /// </summary>
        /// <param name="node"></param>
        public McQueueProperties(XmlNode node)
        {
            //XmlNode node = xml.SelectSingleNode("//queueSettings");
            if (node == null)
            {
                throw new ArgumentException("Inavlid Xml Root, 'queueSettings' ");
            }

            XmlParser parser = new XmlParser(node.OuterXml);

            QueueName = parser.GetAttributeValue(node, "name", true);
            Server = parser.GetAttributeValue(node, "Server", "value", 0);
            //ConnectionString = parser.GetAttributeValue(node, "ConnectionString", "value", "");
            CoverMode = (Messaging.CoverMode)(int)parser.GetAttributeValue(node, "CoverMode", "value", (int)Messaging.CoverMode.FileStream);
            IsTrans = Types.ToBool(parser.GetAttributeValue(node, "IsTrans", "value", "false"), false);
            MaxRetry = (byte)parser.GetAttributeValue(node, "MaxRetry", "value", (int)DefaultMaxRetry);
            //Provider = (QueueProvider)(int)parser.GetAttributeValue(node, "Provider", "value", (int)QueueProvider.SqlServer);

        }

        public IDictionary ToDictionary()
        {
            IDictionary prop = new Hashtable();
            prop["Server"] = Server;
            prop["QueueName"] = QueueName;
            //prop["ConnectionString"] = ConnectionString;
            //prop["MaxThread"] = MaxThread;
            prop["CoverMode"] = (int)CoverMode;
            //prop["Enabled"] = Enabled;
            prop["IsTrans"] = IsTrans;
            prop["MaxRetry"] = MaxRetry;
            //prop["Provider"] = (int)Provider;
            prop["ReloadOnStart"] = ReloadOnStart;
            //prop["QueueMode"] = (int)QueueMode;
            return prop;
        }

        public static McQueueProperties Create(IDictionary prop)
        {

            McQueueProperties mqp = new McQueueProperties(Types.NZ(prop["QueueName"], "McQueue"));
            mqp.Server = Types.ToInt(prop["Server"], 0);
            mqp.QueueName = Types.NZ(prop["QueueName"], "McQueue");
            //mqp.ConnectionString = Types.NZ(prop["ConnectionString"], "");
            //mqp.MaxThread = Types.ToInt(prop["MaxThread"], DefaultMaxThread);
            mqp.CoverMode = (Messaging.CoverMode)Types.ToInt(prop["CoverMode"], (int)Messaging.CoverMode.FileStream);
            //mqp.Enabled = Types.ToBool(prop["Enabled"], true);
            mqp.IsTrans = Types.ToBool(prop["IsTrans"], false);
            mqp.MaxRetry = (byte)Types.ToInt(prop["MaxRetry"], DefaultMaxRetry);
            //mqp.Provider = (QueueProvider)Types.ToInt(prop["Provider"], (int)QueueProvider.Embedded);
            mqp.ReloadOnStart = Types.ToBool(prop["ReloadOnStart"], false);
            //mqp.QueueMode = (QueueMode)Types.ToInt(prop["QueueMode"], (int)QueueMode.Auto);
            return mqp;
        }

        /*
                public static McQueueProperties[] CreateFromRegistry(QueueProvider provider,string connectionString)
                {
                    DataTable dt = null;
                    using (IQueueCommand cmd = McQueueCommand.Factory(provider))
                    {
                        if (provider == QueueProvider.SqlServer)
                        {
                            cmd.ConnectionString = connectionString;
                        }
                        dt= cmd.Execute_DataTable("QueueList", SQLCMD.SqlSelectQueues);
                    }

                    if (dt == null)
                        return null;
                    int i = 0;
                    int count = dt.Rows.Count;

                    McQueueProperties[] props = new McQueueProperties[count];

                    foreach (DataRow dr in dt.Rows)
                    {
                        props[i] = new McQueueProperties(dr["QueueName"].ToString());
                        //props[i].ConnectionString = dr["ConnectionString"].ToString();
                        //props[i].Enabled = Types.ToBool(dr["Enabled"], true);
                        //props[i].IsTrans = Types.ToBool(dr["IsTrans"], false);
                        //props[i].MaxRetry = (byte)Types.ToInt(dr["MaxRetry"], McQueueProperties.DefaultMaxRetry);
                        //props[i].MaxThread = Types.ToInt(dr["MaxThread"], McQueueProperties.DefaultMaxRetry);
                        //props[i].QueueMode = QueueMode.Manual;// mq.GetValue("QueueMode", 1);
                        //props[i].Provider = (QueueProvider)Types.ToInt(dr["Provider"], 1);
                        //props[i].Recoverable = Types.ToBool(dr["Recoverable"], true);
                        //props[i].Server = Types.ToInt(dr["Server"], 0);
                    }

                    return props;
                }
        */

    }
    #endregion
}
