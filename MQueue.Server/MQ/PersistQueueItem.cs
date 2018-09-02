using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging
{
    public class PersistQueueItem
    {
        /// <summary>
        /// Get ItemId
        /// </summary>
        public string Identifier { get; internal set; }
        /// <summary>
        /// Get MessageState
        /// </summary>
        public MessageState MessageState { get; set; }
        /// <summary>
        /// Get Retry
        /// </summary>
        public byte Retry { get; set; }
        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        public DateTime ArrivedTime { get; set; }
        /// <summary>
        /// Get or Set Expiration in minutes
        /// </summary>
        public int Expiration { get; internal set; }

        public byte[] Header { get; internal set; }
        public byte[] Body { get; internal set; }


        public string Print()
        {

            return string.Format("MessageState:{0},Identifier:{1},Retry:{2},ArrivedTime:{3}",
            MessageState,
            Identifier,
            Retry,
            ArrivedTime);
        }
    }
}