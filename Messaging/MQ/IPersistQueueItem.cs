using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging
{
    public interface IPersistQueueItem: ISerialEntity
    {
        /// <summary>
        /// Get ItemId
        /// </summary>
        string Identifier { get; }
        /// <summary>
        /// Get MessageState
        /// </summary>
        MessageState MessageState { get; set; }
        /// <summary>
        /// Get Retry
        /// </summary>
        byte Retry { get; set; }
        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        DateTime ArrivedTime { get; set; }
        /// <summary>
        /// Get or Set Expiration in minutes
        /// </summary>
        int Duration { get; }

        string TypeName { get;}

        //byte[] Header { get; set; }
        byte[] ItemBinary { get; }
        //byte[] Body { get; }

        string Print();
        //QueueMessage Copy();
    }
}
