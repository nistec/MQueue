using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.IO;
using System.IO;
using Nistec.Runtime;
using Nistec.Serialization;
using Nistec.Channels;

namespace Nistec.Messaging
{

    public interface IQueueMessage : ISerialEntity, ITransformMessage, IDisposable
    {
        /// <summary>
        /// Get message type.
        /// </summary>
        MQTypes MessageType { get; }

        /// <summary>
        /// Get Creation Time
        /// </summary>
        DateTime Creation { get;}
        /// <summary>
        /// Get Command
        /// </summary>
        QueueCmd QCommand { get;}

        ///// <summary>
        ///// Get or Set transformation type.
        ///// </summary>
        //TransformTypes TransformType { get; }

        /// <summary>
        /// Get Priority
        /// </summary>
        Priority Priority { get; }

        /// <summary>
        /// Get The message Host\Queue name.
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// Get ItemId
        /// </summary>
        string Identifier { get;}
        /// <summary>
        /// Get the current body stream.
        /// </summary>
        NetStream BodyStream { get; }

        TransStream ToTransStream();

        NetStream ToStream();
        string ToJson();
        string Print();
    }

    public interface IQueueItem : IQueueMessage
    {

        #region property

        ///// <summary>
        ///// Get ItemId
        ///// </summary>
        //string Identifier { get; }

        /// <summary>
        /// Get MessageState
        /// </summary>
        MessageState MessageState { get;}

        ///// <summary>
        ///// Get Command
        ///// </summary>
        //QueueCmd Command { get; }

        ///// <summary>
        ///// Get or Set transformation type.
        ///// </summary>
        //TransformTypes TransformType { get; }

        ///// <summary>
        ///// Get or Set message type.
        ///// </summary>
        //MQTypes MessageType { get; }

        ///// <summary>
        ///// Get Priority
        ///// </summary>
        //Priority Priority { get; }

        ///// <summary>
        ///// Get The message Destination\Queue name.
        ///// </summary>
        //string Host { get; }

        ///// <summary>
        ///// Get the current body stream.
        ///// </summary>
        //NetStream BodyStream { get; }

        //byte[] ItemBinary { get; }

        //byte[] Header { get; }

        /// <summary>
        /// Get Retry
        /// </summary>
        byte Retry { get; }

        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        DateTime ArrivedTime { get; }

        /// <summary>
        /// Get the last modified time.
        /// </summary>
        DateTime Modified { get;  }

        /// <summary>
        /// Get or Set The message Sender.
        /// </summary>
        string Sender { get; }

        /// <summary>
        /// Get or Set The message Label.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Get Duration in milliseconds/Expiration in minutes
        /// </summary>
        int Duration { get; }

        #endregion

        QueueItem Copy();
        
        //Message ToMessage();

        Ptr GetPtr();

        /// <summary>
        /// Deserialize body stream to object, This method is a part of <see cref="IMessageStream"/> implementation.
        /// </summary>
        /// <returns></returns>
        object GetBody();

        /// <summary>
        ///  Deserialize body stream to generic object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetBody<T>();

        byte[] Serilaize();

        ///// <summary>
        ///// Get body stream after set the position to first byte in buffer, This method is a part of <see cref="IMessageStream"/> implementation.
        ///// </summary>
        ///// <returns></returns>
        //NetStream GetItemStream();


        #region property
        /*
        /// <summary>
        /// Get MessageState
        /// </summary>
        MessageState MessageState { get;}
        /// <summary>
        /// Get Command
        /// </summary>
        QueueCmd Command { get;}
        /// <summary>
        /// Get or Set transformation type.
        /// </summary>
        TransformTypes TransformType { get; set; }

        /// <summary>
        /// Get ItemId
        /// </summary>
        Guid ItemId { get;}
        /// <summary>
        /// Get MessageId
        /// </summary>
        int MessageId { get; }
        /// <summary>
        /// Get Priority
        /// </summary>
        Priority Priority { get;}
        /// <summary>
        /// Get Retry
        /// </summary>
        byte Retry { get; }
        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        DateTime ArrivedTime { get; }
        /// <summary>
        /// Get SentTime
        /// </summary>
        DateTime SentTime { get;}
       
        /// <summary>
        /// Get or Set timeout in seconds
        /// </summary>
        int Expiration { get; }

        /// <summary>
        /// Get the last modified time.
        /// </summary>
        DateTime Modified { get;}

        ///// <summary>
        ///// Get indicate wether the item is timeout 
        ///// </summary>
        //bool IsTimeOut { get;}
        
        ///// <summary>
        ///// Get UniqueId
        ///// </summary>
        //long UniqueId { get;}

        /// <summary>
        /// Get Identifier
        /// </summary>
        string Identifier { get; }
        
        //string Filename { get;}


        //string FolderId { get; }

        /// <summary>
        /// Get the current body stream.
        /// </summary>
        NetStream BodyStream { get; }

        /// <summary>
        /// Get or Set The message queue name.
        /// </summary>
        string QueueName { get; }
        */
        #endregion

    }
}
