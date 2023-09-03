using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nistec.Runtime;
using Nistec.Serialization;
using Nistec.IO;
using Nistec.Channels;

namespace Nistec.Messaging
{

    public interface IQueueAck: IAck
    {
        #region property

        /// <summary>
        /// Get or Set The ItemId.
        /// </summary>
        string Identifier { get; }
        /// <summary>
        /// Get the message state
        /// </summary>
        MessageState MessageState { get; }
        ///// <summary>
        ///// Get or Set the arrived time.
        ///// </summary>
        //DateTime ArrivedTime { get; }
        /// <summary>
        /// Get or Set the send time.
        /// </summary>
        DateTime Creation { get; }
        /// <summary>
        /// Get Host
        /// </summary>
        string Host { get; }
        /// <summary>
        /// Get or SetLabel
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Get Duration
        /// </summary>
        int Duration { get; }

        ///// <summary>
        ///// Get Count
        ///// </summary>
        //int Count { get;}
        string Print();

        TransStream ToTransStream();


        #endregion

        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        void EntityRead(Stream stream, IBinaryStreamer streamer);
    }
    
}
