using System;
//using System.Collections.Generic;
//using System.Collections;
//using System.Text;
//using System.Data;
//using System.Threading;
//using System.Messaging;
//using Nistec.Runtime;
//using Nistec.Threading;
using Nistec.IO;
//using System.IO;
using Nistec.Serialization;
using Nistec.Channels;

namespace Nistec.Messaging
{

    public interface IControllerHandler
    {
        TransStream OnMessageReceived(IQueueItem message);
        void OnErrorOcurred(string msg);

        //void OnMessageReceived(IQueueMessage message);

        //void OnErrorOcurred(string message);
    }

    public interface IQueueClient
    {
        IQueueItem EndReceive(IAsyncResult asyncResult);
        void Commit(Ptr ptr);
        bool IsCoverable { get;}
    }

   
    public interface IMessage : ISerialEntity
    {
        //NetStream GetBodyStream();

        NetStream BodyStream { get; }

        //void SetBody(object value);

        //void SetBody(byte[] body, Type type);

        ///// <summary>
        ///// Deserialize body stream to object, This method is a part of <see cref="IMessageStream"/> implementation.
        ///// </summary>
        ///// <returns></returns>
        //object GetBody();

        /////  Deserialize body stream to generic object.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //T GetBody<T>();


        //bool IsEmpty { get; }
    }
    //public interface IReceiveMessageCompleted
    //{
    //    bool IsTrans { get; }
    //    void Completed(Guid itemId, int status);
    //    Message EndReceive(IAsyncResult asyncResult);
    //}

    public interface IReceiveCompleted
    {
        //bool IsTrans { get; }
        //void Completed(Guid itemId, int status);
        //IQueueItem EndReceive(IAsyncResult asyncResult);
    }

    //public interface IQueueTrans
    //{
    //    void CommitTrans(Guid itemId);
    //    void AbortTrans(Guid itemId);
    //}

    public interface ITransScop
    {
        void CommitTrans(Guid itemId);
        void AbortTrans(Guid itemId);
        //void TransEnded(IQueueItem item, ItemState state);

    }

    public interface ITransItem
    {
 
        /// <summary>
        /// Get ItemId
        /// </summary>
        Guid ItemId { get; }
        /// <summary>
        /// Get MessageId
        /// </summary>
        int MessageId { get; set; }
        /// <summary>
        /// Get Priority
        /// </summary>
        Priority Priority { get; set; }
        /// <summary>
        /// Get Retry
        /// </summary>
        int Retry { get; }
        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        DateTime ArrivedTime { get; }
        /// <summary>
        /// Get SentTime
        /// </summary>
        DateTime SentTime { get; }

        /// <summary>
        /// Get or Set Status
        /// </summary>
        ItemState Status { get; set; }

        /// <summary>
        /// Get or Set timeout in seconds
        /// </summary>
        int TimeOut { get; set; }

        
        /// <summary>
        /// Get indicate wether the item is timeout 
        /// </summary>
        bool IsTimeOut { get; }

    }

}


