﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;
using Nistec.Generic;
using Nistec.Runtime;
using System.IO;


namespace Nistec.Messaging.Config
{
       /// <summary>
    /// Represent the queue settings as read only.
    /// </summary>
    public class QueueSettings
    {


        /// <summary>MQueueQueueRootPath.</summary>
        public const string DefaultRootPath = @"C:\Nistec\MQueue\";

        public const string DefaultQueueListener = "nistec_queue_listener";
        public const string DefaultQueueChannel = "nistec_queue_channel";
        public const string DefaultQueueManager = "nistec_queue_manager";
        
        /// <summary>QueuesPath.</summary>
        public const string QueuesFolder = "Queues";
        /// <summary>QueuesPath.</summary>
        public readonly string RootPath = DefaultRootPath;
        /// <summary>QueuesPath.</summary>
        public readonly string QueuesPath= DefaultRootPath+ QueuesFolder;


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
        /// <summary>ReceiveBufferSize.</summary>
        public readonly int ReceiveBufferSize = 8192;
        /// <summary>SendBufferSize.</summary>
        public readonly int SendBufferSize = 8192;
        /// <summary>QueueConfigFile.</summary>
        public readonly string QueueConfigFile = "";
        /// <summary>EnableFileWatcher.</summary>
        public readonly bool EnableFileWatcher = false;
        /// <summary>SyncTaskerTimeout.</summary>
        public readonly int TaskerTimeout = 60;
        /// <summary>EnableAsyncTask.</summary>
        public readonly bool EnableAsyncTask = true;

        public readonly bool EnableDebugLog = false;
        //public readonly int LogMonitorCapacityLines = 1000;

        /// <summary>EnablePipeChannel.</summary>
        public readonly bool EnablePipeChannel = false;
        /// <summary>EnableTcpChannel.</summary>
        public readonly bool EnableTcpChannel = false;
        /// <summary>EnableHttpChannel.</summary>
        public readonly bool EnableHttpChannel = false;

        /// <summary>EnableTcpListener.</summary>
        public readonly bool EnablePipeListener = false;
        /// <summary>EnableTcpListener.</summary>
        public readonly bool EnableTcpListener = false;
        /// <summary>EnableTcpListener.</summary>
        public readonly bool EnableHttpListener = false;


        /// <summary>EnableMailerQueue.</summary>
        public readonly bool EnableMailerQueue = false;
        /// <summary>EnableQueueManager.</summary>
        public readonly bool EnableQueueManager = false;
        
        /// <summary>EnableDbListener.</summary>
        public readonly bool EnableDbListener = false;
        /// <summary>EnableFolderListener.</summary>
        public readonly bool EnableFolderListener = false;


        /// <summary>EnableSizeHandler.</summary>
        public readonly bool EnableSizeHandler = false;
        /// <summary>EnablePerformanceCounter.</summary>
        public readonly bool EnablePerformanceCounter = false;
        
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
            QueuesPath= Path.Combine(RootPath, QueuesFolder);
     
            MaxSize = table.Get<long>("MaxSize", QueueDefaults.DefaultQueueMaxSize);
            DefaultExpiration = table.Get<int>("DefaultExpiration", 30);
            SyncInterval = table.Get<int>("SyncInterval", 60);
            InitialCapacity = table.Get<int>("InitialCapacity", QueueDefaults.InitialCapacity);
            EnableLog = table.Get<bool>("EnableLog", false);
            ReceiveBufferSize = table.Get<int>("ReceiveBufferSize", 8192);
            SendBufferSize = table.Get<int>("SendBufferSize", 8192);
            QueueConfigFile = table.Get("QueueConfigFile");
            EnableFileWatcher = table.Get<bool>("EnableFileWatcher", false);
            TaskerTimeout = table.Get<int>("TaskerTimeout", 60);
            EnableAsyncTask = table.Get<bool>("EnableAsyncTask", true);
            EnableMailerQueue = table.Get<bool>("EnableMailerQueue", false);
            EnableQueueManager = table.Get<bool>("EnableQueueManager", false);

            EnableDebugLog = table.Get<bool>("EnableDebugLog", false);
            //LogMonitorCapacityLines = table.Get<int>("LogMonitorCapacityLines", 1000);

            EnablePipeListener = table.Get<bool>("EnablePipeListener", false);
            EnableTcpListener = table.Get<bool>("EnableTcpListener", false);
            EnableHttpListener = table.Get<bool>("EnableHttpListener", false);

            EnablePipeChannel = table.Get<bool>("EnablePipeChannel", false);
            EnableTcpChannel = table.Get<bool>("EnableTcpChannel", false);
            EnableHttpChannel = table.Get<bool>("EnableHttpChannel", false);

            EnableFolderListener = table.Get<bool>("EnableFolderListener", false);
            EnableDbListener = table.Get<bool>("EnableDbListener", false);
            EnableSizeHandler = table.Get<bool>("EnableSizeHandler", false);
            EnablePerformanceCounter = table.Get<bool>("EnablePerformanceCounter", false);

            QueueDefaults.DefaultExpiration = DefaultExpiration;
            QueueDefaults.EnableLog = EnableLog;

            QLogger.SetLogger(EnableLog, EnableDebugLog);

        }



        public void Load()
        {

        }
    }

    /// <summary>
    /// Represents a queue section  settings within a configuration file.
    /// </summary>
    public class QueueConfigItem : System.Configuration.ConfigurationElement
    {

       
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
        [ConfigurationProperty("ReceiveBufferSize", DefaultValue = 8192, IsRequired = false)]
        public int ReceiveBufferSize
        {
            get { return Types.ToInt(this["ReceiveBufferSize"], 8192); }
        }
        /// <summary>Get Pipe Out Buffer Size</summary>
        [ConfigurationProperty("SendBufferSize", DefaultValue = 8192, IsRequired = false)]
        public int SendBufferSize
        {
            get { return Types.ToInt(this["SendBufferSize"], 8192); }
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
