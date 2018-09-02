using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Nistec.Messaging.Config
{
    /// <summary>
    /// Represent pipe  <see cref="ConfigurationElement"/> item.
    /// </summary>
    public class PipeConfigItem : ConfigurationElement
    {
        /// <summary>
        /// Get pipe name.
        /// </summary>
        [ConfigurationProperty("PipeName", IsRequired = true)]
        public string PipeName
        {
            get
            {
                return this["PipeName"] as string;
            }
        }
        /// <summary>
        /// Get pipe direction.
        /// </summary>
        [ConfigurationProperty("PipeDirection", DefaultValue = "InOut", IsRequired = false)]
        public string PipeDirection
        {
            get
            {
                return this["PipeDirection"] as string;
            }
        }
        /// <summary>
        /// Get pipe options.
        /// </summary>
        [ConfigurationProperty("PipeOptions", DefaultValue = "None", IsRequired = false)]
        public string PipeOptions
        {
            get
            {
                return this["PipeOptions"] as string;
            }
        }
        /// <summary>
        /// Get Verify Pipe Name.
        /// </summary>
        [ConfigurationProperty("VerifyPipe", DefaultValue = "nistec_queue", IsRequired = false)]
        public string VerifyPipe
        {
            get
            {
                return this["VerifyPipe"] as string;
            }
        }
        /// <summary>
        /// Get connection timeout.
        /// </summary>
        [ConfigurationProperty("ConnectTimeout", DefaultValue = "5000", IsRequired = false)]
        public int ConnectTimeout
        {
            get
            {
                return Types.ToInt(this["ConnectTimeout"], 5000);
            }
        }
        /// <summary>
        /// Get In buffer size in bytes.
        /// </summary>
        [ConfigurationProperty("InBufferSize", DefaultValue = "8192", IsRequired = false)]
        public int InBufferSize
        {
            get
            {
                return Types.ToInt(this["InBufferSize"], 8192);
            }
        }
        /// <summary>
        /// Get Out buffer size in bytes.
        /// </summary>
        [ConfigurationProperty("OutBufferSize", DefaultValue = "8192", IsRequired = false)]
        public int OutBufferSize
        {
            get
            {
                return Types.ToInt(this["OutBufferSize"], 8192);
            }
        }
    }

}
