using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Channels.Config;

namespace Nistec.Messaging.Config
{
    /// <summary>
    /// Represent queue config section in <see cref="ConfigurationSection"/>
    /// </summary>
    public class QueueServerConfig : ConfigurationSection
    {
        /// <summary>
        /// Get MQueue config section.
        /// </summary>
        /// <returns></returns>
        public static QueueServerConfig GetConfig()
        {
            return (QueueServerConfig)System.Configuration.ConfigurationManager.GetSection("MQueue") ?? new QueueServerConfig();
        }
        /// <summary>
        /// Get Queue Settings items.
        /// </summary>
        [System.Configuration.ConfigurationProperty("QueueSettings")]
        [ConfigurationCollection(typeof(NetConfigItems), AddItemName = "add")]
        public NetConfigItems QueueSettings
        {
            get
            {
                object o = this["QueueSettings"];
                return o as NetConfigItems;
            }
        }

        /// <summary>
        /// Get <see cref="QueueServerConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("RemoteQueueSettings")]
        [ConfigurationCollection(typeof(QueueServerConfigItem), AddItemName = "queue")]
        public QueueServerConfigItems RemoteQueueSettings
        {
            get
            {
                object o = this["RemoteQueueSettings"];
                return o as QueueServerConfigItems;
            }
        }

        ///// <summary>
        ///// Get <see cref="MailerServerConfigItems"/> collection.
        ///// </summary>
        //[System.Configuration.ConfigurationProperty("MailerSettings")]
        //[ConfigurationCollection(typeof(QueueServerConfigItem), AddItemName = "host")]
        //public MailerServerConfigItems MailerSettings
        //{
        //    get
        //    {
        //        object o = this["MailerSettings"];
        //        return o as MailerServerConfigItems;
        //    }
        //}

        ///// <summary>
        ///// Get <see cref="MailerServerConfigItems"/> collection.
        ///// </summary>
        //[System.Configuration.ConfigurationProperty("ListenerSettings")]
        //[ConfigurationCollection(typeof(ListenerHostConfigItem), AddItemName = "host")]
        //public ListenerHostConfigItems ListenerSettings
        //{
        //    get
        //    {
        //        object o = this["ListenerSettings"];
        //        return o as ListenerHostConfigItems;
        //    }
        //}

        /// <summary>
        /// Get <see cref="PipeServerConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("PipeServerSettings")]
        [ConfigurationCollection(typeof(PipeServerConfigItem), AddItemName = "host")]
        public PipeServerConfigItems PipeServerSettings
        {
            get
            {
                object o = this["PipeServerSettings"];
                return o as PipeServerConfigItems;
            }
        }
        /// <summary>
        /// Find pipe server item.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public PipeServerConfigItem FindPipeServer(string hostName)
        {
            return PipeServerSettings[hostName];
        }

        /// <summary>
        /// Get <see cref="TcpServerConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("TcpServerSettings")]
        [ConfigurationCollection(typeof(TcpServerConfigItem), AddItemName = "host")]
        public TcpServerConfigItems TcpServerSettings
        {
            get
            {
                object o = this["TcpServerSettings"];
                return o as TcpServerConfigItems;
            }
        }
        /// <summary>
        /// Find pipe server item.
        /// </summary>
        /// <param name="pipeName"></param>
        /// <returns></returns>
        public TcpServerConfigItem FindTcpServer(string hostName)
        {
            return TcpServerSettings[hostName];
        }

        /// <summary>
        /// Get <see cref="HttpServerConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("HttpServerSettings")]
        [ConfigurationCollection(typeof(HttpServerConfigItem), AddItemName = "host")]
        public HttpServerConfigItems HttpServerSettings
        {
            get
            {
                object o = this["HttpServerSettings"];
                return o as HttpServerConfigItems;
            }
        }
        /// <summary>
        /// Find pipe server item.
        /// </summary>
        /// <param name="pipeName"></param>
        /// <returns></returns>
        public HttpServerConfigItem FindHttpServer(string hostName)
        {
            return HttpServerSettings[hostName];
        }
    }

}
