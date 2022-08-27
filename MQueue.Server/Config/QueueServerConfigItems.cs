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
   

    #region queue server

    /// <summary>
    /// Represent queue server configuration element collection.
    /// </summary>
    public class QueueServerConfigItems : ConfigurationElementCollection
    {

        /// <summary>
        /// Get or Set <see cref="QueueServerConfigItem"/> item by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public QueueServerConfigItem this[int index]
        {
            get
            {
                return base.BaseGet(index) as QueueServerConfigItem;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
        /// <summary>
        /// Get or Set <see cref="QueueServerConfigItem"/> item by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new QueueServerConfigItem this[string key]
        {
            get { return (QueueServerConfigItem)BaseGet(key); }
            set
            {
                if (BaseGet(key) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                }
                BaseAdd(value);
            }
        }
        /// <summary>
        /// Create New Element.
        /// </summary>
        /// <returns></returns>
        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new QueueServerConfigItem();
        }
        /// <summary>
        /// Get Element Key
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((QueueServerConfigItem)element).QueueName;
        }
    }

    /// <summary>
    /// Represent Queue  <see cref="ConfigurationElement"/> item.
    /// </summary>
    public class QueueServerConfigItem : ConfigurationElement, IQProperties
    {

        /// <summary>
        /// Get  Server Path.
        /// </summary>
        [ConfigurationProperty("ServerPath", IsRequired = false)]
        public string ServerPath
        {
            get
            {
                return this["ServerPath"] as string;
            }
        }
        /// <summary>
        /// Get  IsTopic.
        /// </summary>
        [ConfigurationProperty("IsTopic", IsRequired = false)]
        public bool IsTopic
        {
            get
            {
                return Types.ToBool(this["IsTopic"], false);
            }
        }

        /// <summary>
        /// Get queue name.
        /// </summary>
        [ConfigurationProperty("QueueName", IsRequired = true)]
        public string QueueName
        {
            get
            {
                return this["QueueName"] as string;
            }
        }
        /// <summary>
        /// Get Is Transactional queue.
        /// </summary>
        [ConfigurationProperty("IsTrans", DefaultValue = false, IsRequired = false)]
        public bool IsTrans
        {
            get
            {
                return Types.ToBool(this["IsTrans"], false); 
            }
        }
        /// <summary>
        /// Get max retry.
        /// </summary>
        [ConfigurationProperty("MaxRetry", DefaultValue = (byte)3, IsRequired = false)]
        public byte MaxRetry
        {
            get
            {
                return Types.ToByte(this["MaxRetry"], (byte)3); 
            }
        }

        [ConfigurationProperty("MaxWait", DefaultValue = QProperties.DefaultMaxWait, IsRequired = false)]
        public int MaxWait
        {
            get
            {
                return Types.ToInt(this["MaxWait"], QProperties.DefaultMaxWait);
            }
        }

        [ConfigurationProperty("ConsumeInterval", DefaultValue = QProperties.DefaultConsumeInterval, IsRequired = false)]
        public int ConsumeInterval
        {
            get
            {
                return Types.ToInt(this["ConsumeInterval"], QProperties.DefaultMaxWait);
            }
        }

        /// <summary>
        /// Get cover mode.
        /// </summary>
        [ConfigurationProperty("CoverMode", DefaultValue = CoverMode.Memory, IsRequired = false)]
        public CoverMode Mode
        {
            get
            {
                return (CoverMode)this["CoverMode"]; 
            }
        }
        ///// <summary>
        ///// Get queue provider.
        ///// </summary>
        //[ConfigurationProperty("QueueProvider", DefaultValue = 0, IsRequired = false)]
        //public int QueueProvider
        //{
        //    get
        //    {
        //        return Types.ToInt(this["QueueProvider"], 0);
        //    }
        //}


        /// <summary>
        /// Get queue provider.
        /// </summary>
        [ConfigurationProperty("CommitMode", DefaultValue = PersistCommitMode.OnDisk, IsRequired = false)]
        public PersistCommitMode CommitMode
        {
            get
            {
                return (PersistCommitMode)this["CommitMode"];
            }
        }
        /// <summary>
        /// Get queue provider.
        /// </summary>
        [ConfigurationProperty("ReloadOnStart", DefaultValue = false, IsRequired = false)]
        public bool ReloadOnStart
        {
            get
            {
                return Types.ToBool(this["ReloadOnStart"], false);
            }
        }
        /// <summary>
        /// Get queue name.
        /// </summary>
        [ConfigurationProperty("TargetPath", IsRequired = false)]
        public string TargetPath
        {
            get
            {
                return this["TargetPath"] as string;
            }
        }

        public QueueHost GetRoutHost()
        {

            if (Mode == CoverMode.Rout)
            {
                if (TargetPath == null || TargetPath == "")
                {
                    return null;
                }
                return QueueHost.Parse(TargetPath);
            }
            return null;
        }

        public int ConnectTimeout
        {
            get { return QueueDefaults.DefaultConnectTimeOut; }
        }
        public string Print()
        {
            return string.Format("QueueName: {0}, ServerPath: {1}, IsTrans: {2}, CoverMode: {3}, ConnectTimeout: {4}, TargetPath:{5}",
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
    }
    #endregion

    
}
