using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.IO;
using System.IO;
using Nistec.Runtime;
using Nistec.Serialization;
using Nistec.Channels;
using Nistec.Generic;

namespace Nistec.Messaging
{
    public interface IHostMessage: ITransformMessage,IDisposable
    {
        T Parse<T>(Stream stream);
        string Identifier { get; }
        //string Label { get; }
        string Host { get; }
    }

    public interface c__IQueueRequest : IMessageStream, ISerialEntity, ITransformMessage, IDisposable
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

        #region IMessageStream

        ///// <summary>
        ///// Get or Set Formatter type.
        ///// </summary>
        //Formatters Formatter { get; }
        ///// <summary>
        ///// Get or Set The message Label.
        ///// </summary>
        //string Label { get; }
        #endregion


        ///// <summary>
        ///// Get or Set transformation type.
        ///// </summary>
        //TransformTypes TransformType { get; }



        ///// <summary>
        ///// Get EncodingName
        ///// </summary>
        //string EncodingName { get; }

        /// <summary>
        /// Get Priority
        /// </summary>
        Priority Priority { get; }

        /// <summary>
        /// Get The message Host\Queue name.
        /// </summary>
        string Host { get; set; }
       
        ///// <summary>
        ///// Get ItemId
        ///// </summary>
        //string Identifier { get;}

        /// <summary>
        /// Get the current body stream.
        /// </summary>
        NetStream BodyStream { get; }

        NetStream ToStream();

        TransStream ToTransStream();

        string BodyToString();

        string ToJson();
        string Print();
    }


    public interface b__IQueueRequest : ISerialEntity, ITransformMessage, IDisposable
    {
        /// <summary>
        /// Get message type.
        /// </summary>
        MQTypes MessageType { get; }

        /// <summary>
        /// Get Creation Time
        /// </summary>
        DateTime Creation { get; }
        /// <summary>
        /// Get Command
        /// </summary>
        QueueCmd QCommand { get; }

        /// <summary>
        /// Get or Set indicate wether the message is a duplex type.
        /// </summary>
        DuplexTypes DuplexType { get; set; }

        #region IMessageStream

        ///// <summary>
        ///// Get or Set Formatter type.
        ///// </summary>
        //Formatters Formatter { get; }
        /// <summary>
        /// Get or Set The message Label.
        /// </summary>
        string Label { get; }
        #endregion


        /// <summary>
        /// Get or Set transformation type.
        /// </summary>
        TransformType TransformType { get; }



        /// <summary>
        /// Get EncodingName
        /// </summary>
        string EncodingName { get; }

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
        string Identifier { get; }

        /// <summary>
        /// Get the current body stream.
        /// </summary>
        NetStream BodyStream { get; }

        NetStream ToStream();

        TransStream ToTransStream();

        string BodyToString();

        string ToJson();
        string Print();
    }

    public interface c__IQueueMessage : c__IQueueRequest //, IMessageStream//changeIQueueMessage
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
        ///// Get Destination
        ///// </summary>
        //mqh-string Destination { get; }
        ///// <summary>
        ///// Get Channel
        ///// </summary>
        //int ChannelId { get; }
        ///// <summary>
        ///// Get Channel
        ///// </summary>
        //int AccountId { get; }
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
        //mqh-NameValueArgs<int> IArgs { get; }

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

        #region IQueueRequest

        ///// <summary>
        ///// Get the current body stream.
        ///// </summary>
        //NetStream BodyStream { get; }

        ///// <summary>
        ///// Get Command
        ///// </summary>
        //QueueCmd QCommand { get; }

        //TransStream ToTransStream();

        #endregion

        #region IMessageStream

        //string Print();
        ///// <summary>
        ///// Get Priority
        ///// </summary>
        //Priority Priority { get; }

        ///// <summary>
        ///// Get The message Host\Queue name.
        ///// </summary>
        //string Host { get; set; }
        ///// <summary>
        ///// Get ItemId
        ///// </summary>
        //string Identifier { get; }

        ///// <summary>
        ///// Get the last modified time.
        ///// </summary>
        //DateTime Modified { get;  }

        ///// <summary>
        ///// Get or Set The message Sender.
        ///// </summary>
        //string Sender { get; }
        #endregion

        ///// <summary>
        ///// Get or Set The message Label.
        ///// </summary>
        //string Label { get; }

        /// <summary>
        /// Get Duration in milliseconds/Expiration in minutes
        /// </summary>
        int Duration { get; }

        #endregion

        QueueMessage Copy();
        
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

        byte[] Serialize();

        ///// <summary>
        ///// Get body stream after set the position to first byte in buffer, This method is a part of <see cref="IMessageStream"/> implementation.
        ///// </summary>
        ///// <returns></returns>
        //NetStream GetItemStream();

        bool IsExpired { get; }

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


    public interface IQueueRequest : ISerialEntity, ITransformMessage, IDisposable
    {
        //QHeader Header { get;}

        /// <summary>
        /// Get Creation Time
        /// </summary>
        DateTime Creation { get; }
        /// <summary>
        /// Get Command
        /// </summary>
        QueueCmd QCommand { get; }
        /// <summary>
        /// Get or Set timeout in seconds
        /// </summary>
        int Expiration { get; }


        #region IMessageStream
        /// <summary>
        /// Get or Set the extra arguments for current message.
        /// </summary>
        NameValueArgs Args { get; set; }
        ///// <summary>
        ///// Get or Set Formatter type.
        ///// </summary>
        //Formatters Formatter { get; }
        /// <summary>
        /// Get or Set The message Label.
        /// </summary>
        string Label { get; }
        #endregion


        ///// <summary>
        ///// Get or Set transformation type.
        ///// </summary>
        //TransformTypes TransformType { get; }



        ///// <summary>
        ///// Get EncodingName
        ///// </summary>
        //string EncodingName { get; }


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
        string Identifier { get; }

        /// <summary>
        /// Get the current body stream.
        /// </summary>
        NetStream BodyStream();// { get; }

        NetStream ToStream();

        TransStream ToTransStream();

        string BodyToString();

        string ToJson();
        string Print();
    }

    public interface IQueueMessage : IQueueRequest //, IMessageStream//changeIQueueMessage
    {

        #region property

        ///// <summary>
        ///// Get ItemId
        ///// </summary>
        //string Identifier { get; }

        /// <summary>
        /// Get MessageState
        /// </summary>
        MessageState MessageState { get; }

        ///// <summary>
        ///// Get Destination
        ///// </summary>
        //mqh-string Destination { get; }
        ///// <summary>
        ///// Get Channel
        ///// </summary>
        //int ChannelId { get; }
        ///// <summary>
        ///// Get Channel
        ///// </summary>
        //int AccountId { get; }
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
        //mqh-NameValueArgs<int> IArgs { get; }

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

        #region IQueueRequest

        ///// <summary>
        ///// Get the current body stream.
        ///// </summary>
        //NetStream BodyStream { get; }

        ///// <summary>
        ///// Get Command
        ///// </summary>
        //QueueCmd QCommand { get; }

        //TransStream ToTransStream();

        #endregion

        #region IMessageStream

        //string Print();
        ///// <summary>
        ///// Get Priority
        ///// </summary>
        //Priority Priority { get; }

        ///// <summary>
        ///// Get The message Host\Queue name.
        ///// </summary>
        //string Host { get; set; }
        ///// <summary>
        ///// Get ItemId
        ///// </summary>
        //string Identifier { get; }

        ///// <summary>
        ///// Get the last modified time.
        ///// </summary>
        //DateTime Modified { get;  }

        ///// <summary>
        ///// Get or Set The message Sender.
        ///// </summary>
        //string Sender { get; }
        #endregion

        ///// <summary>
        ///// Get or Set The message Label.
        ///// </summary>
        //string Label { get; }

        /// <summary>
        /// Get Duration in milliseconds/Expiration in minutes
        /// </summary>
        int Duration { get; }

        #endregion

        QueueMessage Copy();

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

        byte[] Serialize();

        ///// <summary>
        ///// Get body stream after set the position to first byte in buffer, This method is a part of <see cref="IMessageStream"/> implementation.
        ///// </summary>
        ///// <returns></returns>
        //NetStream GetItemStream();

        bool IsExpired { get; }

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

    public interface IMessageQueue //: IQueueRequest //, IMessageStream//changeIQueueMessage
    {
        MessageHeader Header { get; }
        string Source { get; }
        string Label { get; }
        byte Retry { get; }
        DateTime ArrivedTime { get; }
        Priority Priority { get; }
        MessageState MessageState {get;}
        int Duration { get; }
        byte[] Body { get; }
    }
}
