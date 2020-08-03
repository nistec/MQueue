using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using Nistec.Data.Entities;
using Nistec.Messaging.Remote;
using Nistec.Serialization;
using Nistec.Channels;

namespace Nistec.Messaging
{

 

    /// <summary>
    /// Represent a message stream for network communication like namedPipe or Tcp.
    /// This message can serialize/desrialize fast and easly using the <see cref="BinaryStreamer"/>
    /// </summary>
    [Serializable]
    public sealed class QueueReport 
    {

        public QueueReport()
        {
            //ReportCreated = DateTime.Now;
        }

        #region property


        //public DateTime ReportCreated { get; internal set; }

        /// <summary>
        /// Get Queue Creation Time
        /// </summary>
        public DateTime QueueCreated { get; set; }

        /// <summary>
        /// Get LastEnqueue
        /// </summary>
        public DateTime LastEnqueue { get; set; }
        /// <summary>
        /// Get LastDequeue
        /// </summary>
        public DateTime LastDequeue { get; set; }
        /// <summary>
        /// Get items Count.
        /// </summary>
        public int ItemsCount { get; set; }



        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
  
        //public string Print()
        //{
        //    return string.Format("MessageType:{0},Command:{1},Priority:{2},Identifier:{3},Creation:{4},TransformType:{5},Host:{6}",
        //    MessageType,
        //    Command,
        //    Priority,
        //    Identifier,
        //    Creation,
        //    TransformType,
        //    Host);
        //}
        #endregion
    }
}
