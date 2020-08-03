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
    /// Represent pipe client configuration element collection.
    /// </summary>
    public class PipeClientConfigItems : ConfigurationElementCollection
    {
        /// <summary>
        /// Get or Set <see cref="PipeConfigItem"/> item by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public PipeConfigItem this[int index]
        {
            get
            {
                return base.BaseGet(index) as PipeConfigItem;
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
        /// Get or Set <see cref="PipeConfigItem"/> item by key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new PipeConfigItem this[string key]
        {
            get { return (PipeConfigItem)BaseGet(key); }
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
        /// Create New Element
        /// </summary>
        /// <returns></returns>
        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new PipeConfigItem();
        }
        /// <summary>
        /// Get Element Key
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((PipeConfigItem)element).PipeName;
        }
    }

}
