using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Serialization;

namespace Nistec.Messaging
{
    /// <summary>
    /// Represent Queue Defaults Settings
    /// </summary>
    public class QueueDefaults
    {
        public const int CurrentVersion = 4022;

        /// <summary>MQueueQueueRootPath.</summary>
        //public readonly static string RootQueuesPath = @"C:\Nistec\MQueue\Queues\";

        /// <summary>
        /// Get Default Formatter
        /// </summary>
        public static Formatters DefaultFormatter { get { return Formatters.BinarySerializer; } }

        #region pipe
        /// <summary>
        /// Enqueue PipeName
        /// </summary>
        public const string EnqueuePipeName = "nistec_enqueue";
        /// <summary>
        /// Dequeue PipeName
        /// </summary>
        public const string DequeuePipeName = "nistec_dequeue";
        /// <summary>
        /// Queue Manager PipeName
        /// </summary>
        public const string QueueManagerPipeName = "nistec_queue_manager";
        /// <summary>
        /// Mailer PipeName
        /// </summary>
        public const string MailerPipeName = "nistec_mailer";
        #endregion

        #region TCP

        /// <summary>
        /// DefaultProcessTimeout
        /// </summary>
        public const int DefaultProcessTimeout = 1000;

        /// <summary>
        /// Default Queue Port
        /// </summary>
        public const int DefaultBundlePort = 13100;

        /// <summary>
        /// Default Queue Address
        /// </summary>
        public const string DefaultQueueAddress = "localhost";
        /// <summary>
        /// Default Enqueue Tcp Port
        /// </summary>
        public const int DefaultEnqueuePort = 13101;
        /// <summary>
        /// Default Dequeue Tcp Port
        /// </summary>
        public const int DefaultDequeuePort = 13102;

        /// <summary>
        /// Default Queue Manager Address
        /// </summary>
        public const string DefaultQueueManagerAddress = "localhost";
        /// <summary>
        /// Default Queue Manager Port
        /// </summary>
        public const int DefaultQueueManagerPort = 13105;
        /// <summary>
        /// DefaultTaskTimeout
        /// </summary>
        public const int DefaultTaskTimeout = 240;

        #endregion

        internal const string QueueItemExt = ".mcq";

        internal const string MessageQueueExt = ".mq";

        
        //public static TimeSpan DefaultItemTimeOut { get { return TimeSpan.FromSeconds(DefaultItemTimeOutInSecond); } }

        //public const int DefaultItemTimeOutInSecond = 3600 * 24;
        public const int DefaultRecieveTimeOutInSecond = 3600;

        public const int DefaultConnectTimeOut = 5000;
        public const int DefaultMaxRetry = 3;
        //10 GB max size
        public const long DefaultQueueMaxSize = 10737418240;
        //10 MB
        internal const long MinQueueMaxSize = 10485760;
        public const int InitialCapacity = 100;
        public const int DefaultIntervalSeconds = 60;
        internal const int MinIntervalSeconds = 30;

        internal static int GetValidIntervalSeconds(int intervalSeconds)
        {
            return intervalSeconds < QueueDefaults.MinIntervalSeconds ? QueueDefaults.DefaultIntervalSeconds : intervalSeconds;

        }
        internal static long GetValidQueueMaxSize(long maxSize)
        {
            return maxSize < QueueDefaults.MinQueueMaxSize ? QueueDefaults.DefaultQueueMaxSize : maxSize;

        }


        /// <summary>
        /// Get Default Expiration in minutes
        /// </summary>
        public static int DefaultExpiration { get;  set; }
        /// <summary>
        /// Get if Enable Logging
        /// </summary>
        public static bool EnableLog { get;  set; }


        static QueueDefaults()
        {
            DefaultExpiration = 30;
            EnableLog = false;
        }
    }


}
