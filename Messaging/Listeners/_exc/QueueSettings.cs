using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;
using Nistec.Generic;
using Nistec.Runtime;
using System.IO;
using Nistec.Messaging.Config;

namespace Nistec.Messaging.Session
{
       /// <summary>
    /// Represent the queue settings as read only.
    /// </summary>
    public class QueueSettings
    {
        /// <summary>MQueueQueueRootPath.</summary>
        public const string DefaultRootPath = @"C:\Nistec\MQueue\";

        ///// <summary>QueuesPath.</summary>
        //public string QueuesPath { get { return Path.Combine(RootPath, @"\Queues\"); } }

        public string QueuesPath()
        {
            return Path.Combine(RootPath, QueuesFolder); 
        }
        
        /// <summary>QueuesPath.</summary>
        public const string QueuesFolder = "Queues";

        /// <summary>QueuesPath.</summary>
        public readonly string RootPath = DefaultRootPath;

        /// <summary>MaxRetry.</summary>
        public readonly int MaxRetry = QueueDefaults.DefaultMaxRetry;

        /// <summary>MaxSize.</summary>
        public readonly long MaxSize = QueueDefaults.DefaultQueueMaxSize;
        /// <summary>DefaultExpiration.</summary>
        public readonly int DefaultExpiration = 30;
        /// <summary>Sync Interval in seconds.</summary>
        public readonly int SyncInterval = QueueDefaults.DefaultIntervalSeconds;
        /// <summary>InitialCapacity.</summary>
        public readonly int InitialCapacity = 100;
        /// <summary>EnableLog.</summary>
        public readonly bool EnableLog = false;
        /// <summary>InBufferSize.</summary>
        public readonly int InBufferSize = 8192;
        /// <summary>OutBufferSize.</summary>
        public readonly int OutBufferSize = 8192;
        /// <summary>QueueConfigFile.</summary>
        public readonly string QueueConfigFile = "";
        /// <summary>EnableFileWatcher.</summary>
        public readonly bool EnableFileWatcher = false;
        /// <summary>SyncTaskerTimeout.</summary>
        public readonly int TaskerTimeout = 60;
        /// <summary>EnableAsyncTask.</summary>
        public readonly bool EnableAsyncTask = true;

        /// <summary>EnableMailerQueue.</summary>
        public readonly bool EnableMailerQueue = false;
        /// <summary>EnableQueueManager.</summary>
        public readonly bool EnableQueueManager = false;
        /// <summary>EnableTcpListener.</summary>
        public readonly bool EnablePipeListener = false;
        /// <summary>EnableTcpListener.</summary>
        public readonly bool EnableTcpListener = false;
        /// <summary>EnableTcpListener.</summary>
        public readonly bool EnableHttpListener = false;
        /// <summary>EnableDbListener.</summary>
        public readonly bool EnableDbListener = false;
        /// <summary>EnableFolderListener.</summary>
        public readonly bool EnableFolderListener = false;

        /// <summary>EnableSizeHandler.</summary>
        public readonly bool EnableSizeHandler = false;
        /// <summary>EnablePerformanceCounter.</summary>
        public readonly bool EnablePerformanceCounter = false;


        public MailerServerConfigItems LoadMailerSettings()
        {
            return QueueServerConfig.GetConfig().MailerSettings;
        }
        public ListenerHostConfigItems LoadListenerSettings()
        {
            return QueueServerConfig.GetConfig().ListenerSettings;
        }
        

         /// <summary>MailerQueuePath.</summary>
        public readonly string MailerQueuePath = @"C:\Nistec\MailQueue\";
        /// <summary>MailerStorePath.</summary>
        public readonly string MailerStorePath = @"C:\Nistec\MailStore\";
        /// <summary>MailerDefaultHost.</summary>
        public readonly string MailerDefaultHost = "";
        /// <summary>MailerMinItemsPerQueue.</summary>
        public readonly int MailerMinItemsPerQueue = 999999;
        /// <summary>MailerMaxItemsPerSession.</summary>
        public readonly int MailerMaxItemsPerSession = 1000;
        /// <summary>MailerDeleteIntervalSeconds.</summary>
        public readonly int MailerDeleteIntervalSeconds = 60;
        /// <summary>EnableChunk.</summary>
        public readonly bool EnableChunk = true;
        /// <summary>MailerIntervalManager.</summary>
        public readonly int MailerIntervalManager = 60000;
        /// <summary>MailerIntervalDequeue.</summary>
        public readonly int MailerIntervalDequeue = 60000;


        public  QueueSettings()
        {
            //XmlTable table = NetConfig.GetCustomConfig("QueueSettings");

            var section = QueueServerConfig.GetConfig();
            var table = section.QueueSettings;

            if (table == null)
            {
                throw new ArgumentException("Can not load XmlTable config");
            }
            RootPath = table.Get<string>("RootPath", DefaultRootPath);
    
            MaxSize = table.Get<long>("MaxSize", QueueDefaults.DefaultQueueMaxSize);
            DefaultExpiration = table.Get<int>("DefaultExpiration", 30);
            SyncInterval = table.Get<int>("SyncInterval", 60);
            InitialCapacity = table.Get<int>("InitialCapacity", QueueDefaults.InitialCapacity);
            EnableLog = table.Get<bool>("EnableLog", false);
            InBufferSize = table.Get<int>("InBufferSize", 8192);
            OutBufferSize = table.Get<int>("OutBufferSize", 8192);
            QueueConfigFile = table.Get("QueueConfigFile");
            EnableFileWatcher = table.Get<bool>("EnableFileWatcher", false);
            TaskerTimeout = table.Get<int>("TaskerTimeout", 60);
            EnableAsyncTask = table.Get<bool>("EnableAsyncTask", true);
            EnablePipeListener = table.Get<bool>("EnablePipeListener", false);

            EnableMailerQueue = table.Get<bool>("EnableMailerQueue", false);
            EnableQueueManager = table.Get<bool>("EnableQueueManager", false);
            EnableTcpListener = table.Get<bool>("EnableTcpListener", false);
            EnableHttpListener = table.Get<bool>("EnableHttpListener", false);
            EnableFolderListener = table.Get<bool>("EnableFolderListener", false);
            EnableDbListener = table.Get<bool>("EnableDbListener", false);
            EnableSizeHandler = table.Get<bool>("EnableSizeHandler", false);
            EnablePerformanceCounter = table.Get<bool>("EnablePerformanceCounter", false);

            QueueDefaults.DefaultExpiration = DefaultExpiration;
            QueueDefaults.EnableLog = EnableLog;
        }

#if(false)

         QueueSettings()
        {
        //    LoadCustomSettings();
        //}

        //void LoadQueueConfig()
        //{

            var table = QueueSettings.Settings;

            if (table == null)
            {
                throw new ArgumentException("Can not load XmlTable config");
            }

            QueueName = table.QueueName;
            MaxSize = table.MaxSize;
            DefaultExpiration = table.DefaultExpiration;
            RemoveExpiredItemOnSync = table.RemoveExpiredItemOnSync;
            SyncInterval = table.SyncInterval;
            InitialCapacity = table.InitialCapacity;
            LoadFactor = table.LoadFactor;
            SessionTimeout = table.SessionTimeout;
            MaxSessionTimeout = table.MaxSessionTimeout;
            EnableLog = table.EnableLog;
            InBufferSize = table.InBufferSize;
            OutBufferSize = table.OutBufferSize;
            SyncConfigFile = table.SyncConfigFile;
            EnableSyncFileWatcher = table.EnableSyncFileWatcher;
            ReloadSyncOnChange = table.ReloadSyncOnChange;
            SyncTaskerTimeout = table.SyncTaskerTimeout;
            EnableAsyncTask = table.EnableAsyncTask;

            EnableRemoteQueue = table.EnableRemoteQueue;
            EnableSyncQueue = table.EnableSyncQueue;
            EnableSessionQueue = table.EnableSessionQueue;
            EnableDataQueue = table.EnableDataQueue;
            EnableQueueManager = table.EnableQueueManager;

            QueueDefaults.MaxSessionTimeout = MaxSessionTimeout;
            QueueDefaults.SessionTimeout = SessionTimeout;
            QueueDefaults.DefaultExpiration = DefaultExpiration;
            QueueDefaults.EnableLog = EnableLog;


        }
        void LoadCustomSettings()
        {
 
            var table = NetConfig.GetCustomSectionCollection("Nistec", "QueueSettings");//.AppSettings;

            if (table == null)
            {
                throw new ArgumentException("Can not load XmlTable config");
            }

            QueueName = table.Get("name");
            MaxSize = table.Get<int>("MaxSize", QueueDefaults.QueueMaxSize);
            DefaultExpiration = table.Get<int>("DefaultExpiration", 30);
            RemoveExpiredItemOnSync = table.Get<bool>("RemoveExpiredItemOnSync", true);
            SyncInterval = table.Get<int>("SyncInterval", 30);
            InitialCapacity = table.Get<int>("InitialCapacity", QueueDefaults.InitialCapacity);
            LoadFactor = table.Get<float>("LoadFactor", (float)QueueDefaults.LoadFactor);
            SessionTimeout = table.Get<int>("SessionTimeout", 30);
            MaxSessionTimeout = table.Get<int>("MaxSessionTimeout", 1440);
            EnableLog = table.Get("EnableLog", false);
            InBufferSize = table.Get<int>("InBufferSize", 8192);
            OutBufferSize = table.Get<int>("OutBufferSize", 8192);
            SyncConfigFile = table.Get("SyncConfigFile");
            EnableSyncFileWatcher = table.Get<bool>("EnableSyncFileWatcher", false);
            ReloadSyncOnChange = table.Get<bool>("ReloadSyncOnChange", false);
            SyncTaskerTimeout = table.Get<int>("SyncTaskerTimeout", 60);
            EnableAsyncTask = table.Get<bool>("EnableAsyncTask", true);

            EnableRemoteQueue = table.Get<bool>("EnableRemoteQueue", false);
            EnableSyncQueue = table.Get<bool>("EnableSyncQueue", false);
            EnableSessionQueue = table.Get<bool>("EnableSessionQueue", false);
            EnableDataQueue = table.Get<bool>("EnableDataQueue", false);
            EnableQueueManager = table.Get<bool>("EnableQueueManager", false);

            QueueDefaults.MaxSessionTimeout = MaxSessionTimeout;
            QueueDefaults.SessionTimeout = SessionTimeout;
            QueueDefaults.DefaultExpiration = DefaultExpiration;
            QueueDefaults.EnableLog = EnableLog;


        }

        /// <summary>
        /// QueueSettings ctor
        /// </summary>
        void LoadQueueSettings()
        {
           
            XmlTable table = NetConfig.GetCustomConfig("QueueSettings");

            if (table == null)
            {
                throw new ArgumentException("Can not load XmlTable config");
            }

            QueueName = table.GetValue("name");
            MaxSize = table.Get<int>("MaxSize", QueueDefaults.QueueMaxSize);
            DefaultExpiration = table.Get<int>("DefaultExpiration", 30);
            RemoveExpiredItemOnSync = table.Get<bool>("RemoveExpiredItemOnSync", true);
            SyncInterval = table.Get<int>("SyncInterval", 30);
            InitialCapacity = table.Get<int>("InitialCapacity", QueueDefaults.InitialCapacity);
            LoadFactor = table.Get<float>("LoadFactor", (float)QueueDefaults.LoadFactor);
            SessionTimeout = table.Get<int>("SessionTimeout", 30);
            MaxSessionTimeout = table.Get<int>("MaxSessionTimeout", 1440);
            EnableLog = table.Get<bool>("EnableLog", false);
            InBufferSize = table.Get<int>("InBufferSize", 8192);
            OutBufferSize = table.Get<int>("OutBufferSize", 8192);
            SyncConfigFile = table.GetValue("SyncConfigFile");
            EnableSyncFileWatcher = table.Get<bool>("EnableSyncFileWatcher", false);
            ReloadSyncOnChange = table.Get<bool>("ReloadSyncOnChange", false);
            SyncTaskerTimeout = table.Get<int>("SyncTaskerTimeout", 60);
            EnableAsyncTask = table.Get<bool>("EnableAsyncTask", true);

            EnableRemoteQueue = table.Get<bool>("EnableRemoteQueue", false);
            EnableSyncQueue = table.Get<bool>("EnableSyncQueue", false);
            EnableSessionQueue = table.Get<bool>("EnableSessionQueue", false);
            EnableDataQueue = table.Get<bool>("EnableDataQueue", false);
            EnableQueueManager = table.Get<bool>("EnableQueueManager", false);

            QueueDefaults.MaxSessionTimeout = MaxSessionTimeout;
            QueueDefaults.SessionTimeout = SessionTimeout;
            QueueDefaults.DefaultExpiration = DefaultExpiration;
            QueueDefaults.EnableLog = EnableLog;


        }


        /// <summary>
        /// QueueSettings ctor
        /// </summary>
        void LoadQueueWebSettings()
        {

            var table = NetConfig.GetCustomSectionCollection("Nistec", "QueueSettings");//.AppSettings;

            if (table == null)
            {
                throw new ArgumentException("Can not load XmlTable config");
            }

            QueueName = table.Get("name");
            MaxSize = Types.ToInt(table.Get("MaxSize"), QueueDefaults.QueueMaxSize);
            DefaultExpiration = Types.ToInt(table.Get("DefaultExpiration"), 30);
            RemoveExpiredItemOnSync = Types.ToBool(table.Get("RemoveExpiredItemOnSync"), true);
            SyncInterval = Types.ToInt(table.Get("SyncInterval"), 30);
            InitialCapacity = Types.ToInt(table.Get("InitialCapacity"), QueueDefaults.InitialCapacity);
            LoadFactor = Types.ToFloat(table.Get("LoadFactor"), (float)QueueDefaults.LoadFactor);
            SessionTimeout = Types.ToInt(table.Get("SessionTimeout"), 30);
            MaxSessionTimeout = Types.ToInt(table.Get("MaxSessionTimeout"), 1440);
            EnableLog = Types.ToBool(table.Get("EnableLog"), false);
            InBufferSize = Types.ToInt(table.Get("InBufferSize"), 8192);
            OutBufferSize = Types.ToInt(table.Get("OutBufferSize"), 8192);
            SyncConfigFile = table.Get("SyncConfigFile");
            EnableSyncFileWatcher = Types.ToBool(table.Get("EnableSyncFileWatcher"), false);
            ReloadSyncOnChange = Types.ToBool(table.Get("ReloadSyncOnChange"), false);
            SyncTaskerTimeout = Types.ToInt(table.Get("SyncTaskerTimeout"), 60);
            EnableAsyncTask = Types.ToBool(table.Get("EnableAsyncTask"), true);

            EnableRemoteQueue = Types.ToBool(table.Get("EnableRemoteQueue"), false);
            EnableSyncQueue = Types.ToBool(table.Get("EnableSyncQueue"), false);
            EnableSessionQueue = Types.ToBool(table.Get("EnableSessionQueue"), false);
            EnableDataQueue = Types.ToBool(table.Get("EnableDataQueue"), false);
            EnableQueueManager = Types.ToBool(table.Get("EnableQueueManager"), false);

            QueueDefaults.MaxSessionTimeout = MaxSessionTimeout;
            QueueDefaults.SessionTimeout = SessionTimeout;
            QueueDefaults.DefaultExpiration = DefaultExpiration;
            QueueDefaults.EnableLog = EnableLog;


        }

        //void LoadQueueSettings(System.Collections.Specialized.NameValueCollection prop)
        //{
        //    DefaultExpiration = Types.ToInt(prop["DefaultExpiration"], 30);
        //    SessionTimeout = (int)Types.ToInt(prop["SessionTimeout"], 30);
        //}
#endif

    }

    /// <summary>
    /// Represents a queue section  settings within a configuration file.
    /// </summary>
    public class QueueConfigItem : System.Configuration.ConfigurationElement
    {

   //     private QueueSettings settings;
   ////= ConfigurationManager.GetSection("QueueSettings") as QueueConfig;

   //     /// <summary>
   //     /// Get the <see cref="QueueSettings"/>
   //     /// </summary>
   //     public QueueSettings Settings
   //     {
   //         get
   //         {
   //             if (settings == null)
   //             {
   //                 settings = (QueueSettings)ConfigurationManager.GetSection("QueueSettings") ?? new QueueSettings();
   //             }
   //             return settings;
   //         }
   //     }

       
        /// <summary>Get QueueName</summary>
        [ConfigurationProperty("name", DefaultValue = "mcQueue", IsRequired = false)]
        public string QueueName
        {
            get { return (string)this["name"]; }
        }
        /// <summary>Get Queue Max Size</summary>
        [ConfigurationProperty("MaxSize", DefaultValue = QueueDefaults.DefaultQueueMaxSize, IsRequired = false)]
        public long MaxSize
        {
            get { return Types.ToLong(this["MaxSize"], QueueDefaults.DefaultQueueMaxSize); }
        }
        /// <summary>Get Default Expiration in minutes</summary>
        [ConfigurationProperty("DefaultExpiration", DefaultValue = 30, IsRequired = false)]
        public int DefaultExpiration
        {
            get { return Types.ToInt(this["DefaultExpiration"], 30); }
        }

        /// <summary>Get if Remove Expired Item On Sync</summary>
        [ConfigurationProperty("RemoveExpiredItemOnSync", DefaultValue = true, IsRequired = false)]
        public bool RemoveExpiredItemOnSync
        {
            get { return Types.ToBool(this["RemoveExpiredItemOnSync"], true); }
        }
        /// <summary>Get Sync Interval in seconds</summary>
        [ConfigurationProperty("SyncInterval", DefaultValue = QueueDefaults.DefaultIntervalSeconds, IsRequired = false)]
        public int SyncInterval
        {
            get { return Types.ToInt(this["SyncInterval"], QueueDefaults.DefaultIntervalSeconds); }
        }
        /// <summary>Get Initial Capacity</summary>
        [ConfigurationProperty("InitialCapacity", DefaultValue = QueueDefaults.InitialCapacity, IsRequired = false)]
        public int InitialCapacity
        {
            get { return Types.ToInt(this["InitialCapacity"], QueueDefaults.InitialCapacity); }
        }
       

        /// <summary>Get Max Session Timeout in minutes</summary>
        [ConfigurationProperty("MaxSessionTimeout", DefaultValue = 1440, IsRequired = false)]
        public int MaxSessionTimeout
        {
            get { return Types.ToInt(this["MaxSessionTimeout"], 1440); }
        }

        /// <summary>Get if Enable Logging</summary>
        [ConfigurationProperty("EnableLog", DefaultValue = false, IsRequired = false)]
        public bool EnableLog
        {
            get { return Types.ToBool(this["EnableLog"], false); }
        }
        /// <summary>Get Pipe In Buffer Size</summary>
        [ConfigurationProperty("InBufferSize", DefaultValue = 8192, IsRequired = false)]
        public int InBufferSize
        {
            get { return Types.ToInt(this["InBufferSize"], 8192); }
        }
        /// <summary>Get Pipe Out Buffer Size</summary>
        [ConfigurationProperty("OutBufferSize", DefaultValue = 8192, IsRequired = false)]
        public int OutBufferSize
        {
            get { return Types.ToInt(this["OutBufferSize"], 8192); }
        }
        /// <summary>Get Sync Config File</summary>
        [ConfigurationProperty("SyncConfigFile", DefaultValue = "", IsRequired = false)]
        public string SyncConfigFile
        {
            get { return (string)this["SyncConfigFile"]; }
        }
        /// <summary>Get Db Config File</summary>
        [ConfigurationProperty("DbConfigFile", DefaultValue = "", IsRequired = false)]
        public string DbConfigFile
        {
            get { return (string)this["DbConfigFile"]; }
        }
        /// <summary>Get if Enable SyncFileWatcher</summary>
        [ConfigurationProperty("EnableSyncFileWatcher", DefaultValue = false, IsRequired = false)]
        public bool EnableSyncFileWatcher
        {
            get { return Types.ToBool(this["EnableSyncFileWatcher"], false); }
        }
        /// <summary>Get if Reload Sync OnChange</summary>
        [ConfigurationProperty("ReloadSyncOnChange", DefaultValue = false, IsRequired = false)]
        public bool ReloadSyncOnChange
        {
            get { return Types.ToBool(this["ReloadSyncOnChange"], false); }
        }
        /// <summary>Get QueueName</summary>
        [ConfigurationProperty("EnableAsyncTask", DefaultValue = true, IsRequired = false)]
        public bool EnableAsyncTask
        {
            get { return Types.ToBool(this["EnableAsyncTask"], true); }
        }
        /// <summary>Get Sync Tasker Timeout in seconds</summary>
        [ConfigurationProperty("SyncTaskerTimeout", DefaultValue = 60, IsRequired = false)]
        public int SyncTaskerTimeout
        {
            get { return Types.ToInt(this["SyncTaskerTimeout"], 60); }
        }

        /// <summary>Get if Enable RemoteQueue</summary>
        [ConfigurationProperty("EnableRemoteQueue", DefaultValue = false, IsRequired = false)]
        public bool EnableRemoteQueue
        {
            get { return Types.ToBool(this["EnableRemoteQueue"], false); }
        }
        /// <summary>Get if Enable SyncQueue</summary>
        [ConfigurationProperty("EnableSyncQueue", DefaultValue = false, IsRequired = false)]
        public bool EnableSyncQueue
        {
            get { return Types.ToBool(this["EnableSyncQueue"], false); }
        }
        /// <summary>Get if Enable SessionQueue</summary>
        [ConfigurationProperty("EnableSessionQueue", DefaultValue = true, IsRequired = false)]
        public bool EnableSessionQueue
        {
            get { return Types.ToBool(this["EnableSessionQueue"], false); }
        }
        /// <summary>Get if Enable DataQueue</summary>
        [ConfigurationProperty("EnableDataQueue", DefaultValue = false, IsRequired = false)]
        public bool EnableDataQueue
        {
            get { return Types.ToBool(this["EnableDataQueue"], false); }
        }
        /// <summary>Get if Enable QueueManager</summary>
        [ConfigurationProperty("EnableQueueManager", DefaultValue = false, IsRequired = false)]
        public bool EnableQueueManager
        {
            get { return Types.ToBool(this["EnableQueueManager"], false); }
        }
        /// <summary>Get if Enable size handler</summary>
        [ConfigurationProperty("EnableSizeHandler", DefaultValue = false, IsRequired = false)]
        public bool EnableSizeHandler
        {
            get { return Types.ToBool(this["EnableSizeHandler"], false); }
        }
        /// <summary>Get if Enable queue performance counter</summary>
        [ConfigurationProperty("EnablePerformanceCounter", DefaultValue = false, IsRequired = false)]
        public bool EnablePerformanceCounter
        {
            get { return Types.ToBool(this["EnablePerformanceCounter"], false); }
        }

        //====================================================================

        
        /// <summary>Get mailer queue path</summary>
        [ConfigurationProperty("MailerDefaultHost", DefaultValue = "", IsRequired = false)]
        public string MailerDefaultHost
        {
            get { return (string)this["MailerDefaultHost"]; }
        }
        /// <summary>Get mailer queue path</summary>
        [ConfigurationProperty("MailerQueuePath", DefaultValue = "", IsRequired = false)]
        public string MailerQueuePath
        {
            get { return (string)this["MailerQueuePath"]; }
        }
        /// <summary>Get mailer store path</summary>
        [ConfigurationProperty("MailerStorePath", DefaultValue = "", IsRequired = false)]
        public string MailerStorePath
        {
            get { return (string)this["MailerStorePath"]; }
        }
        /// <summary>Get minimum items per queue</summary>
        [ConfigurationProperty("MailerMinItemsPerQueue", DefaultValue = 999999, IsRequired = false)]
        public int MailerMinItemsPerQueue
        {
            get { return Types.ToInt(this["MailerMinItemsPerQueue"], 999999); }
        }
        /// <summary>Get max items per session</summary>
        [ConfigurationProperty("MailerMaxItemsPerSession", DefaultValue = 1000, IsRequired = false)]
        public int MailerMaxItemsPerSession
        {
            get { return Types.ToInt(this["MailerMaxItemsPerSession"], 1000); }
        }
        /// <summary>Get mailer delete interval in seconds</summary>
        [ConfigurationProperty("MailerDeleteIntervalSeconds", DefaultValue = 60, IsRequired = false)]
        public int MailerDeleteIntervalSeconds
        {
            get { return Types.ToInt(this["MailerDeleteIntervalSeconds"], 60); }
        }

        /// <summary>Get if Enable chunk items.</summary>
        [ConfigurationProperty("EnableChunk", DefaultValue = false, IsRequired = false)]
        public bool EnableChunk
        {
            get { return Types.ToBool(this["EnableChunk"], false); }
        }
        /// <summary>Get mailer interval manager</summary>
        [ConfigurationProperty("MailerIntervalManager", DefaultValue = 60000, IsRequired = false)]
        public int MailerIntervalManager
        {
            get { return Types.ToInt(this["MailerIntervalManager"], 60000); }
        }
        /// <summary>Get mailer interval dequeue</summary>
        [ConfigurationProperty("MailerIntervalDequeue", DefaultValue = 60000, IsRequired = false)]
        public int MailerIntervalDequeue
        {
            get { return Types.ToInt(this["MailerIntervalDequeue"], 60000); }
        }
       

    }

   
}
