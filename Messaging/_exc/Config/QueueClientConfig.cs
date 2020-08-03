using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;
using Nistec.Generic;
using Nistec.Runtime;

namespace Nistec.Messaging.Config
{
    /// <summary>
    /// Represent queue config for client.
    /// </summary>
    public class QueueClientConfig : ConfigurationSection
    {
        /// <summary>
        /// Get <see cref="QueueClientConfig"/>.
        /// </summary>
        /// <returns></returns>
        public static QueueClientConfig GetConfig()
        {
            return (QueueClientConfig)System.Configuration.ConfigurationManager.GetSection("MQueue") ?? new QueueClientConfig();
        }
        /// <summary>
        /// Get <see cref="PipeClientConfigItems"/> collection.
        /// </summary>
        [System.Configuration.ConfigurationProperty("PipeClientSettings")]
        [ConfigurationCollection(typeof(PipeClientConfigItems), AddItemName = "pipe")]
        public PipeClientConfigItems PipeClientSettings
        {
            get
            {
                object o = this["PipeClientSettings"];
                return o as PipeClientConfigItems;
            }
        }
        /// <summary>
        /// Find <see cref="PipeConfigItem"/> item.
        /// </summary>
        /// <param name="pipeName"></param>
        /// <returns></returns>
        public PipeConfigItem FindPipeClient(string pipeName)
        {
            return PipeClientSettings[pipeName];
        }

    }

}
