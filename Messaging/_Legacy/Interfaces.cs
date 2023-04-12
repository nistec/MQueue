using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Threading;
using System.Messaging;

using Nistec.Data;
using Nistec.Runtime;
using Nistec.Threading;

namespace Nistec.Legacy
{
    public interface IRemoteQueue
    {
        //bool IsTrans(string queueName);
        int MaxCapacity(string queueName);
        int Count(string queueName);
        bool Initilaized(string queueName);

        //bool Enabled(string queueName);
        //int MaxItemsPerSecond(string queueName);
        //int Server(string queueName);
        //string DateFormat(string queueName);
        //bool HoldDequeue(string queueName);


        string Reply(string text);
        bool CanQueue(string queueName,uint count);


        #region Queue action

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        string Peek(string queueName);
        /// <summary>
        /// Peek Message
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        string Peek(string queueName, Nistec.Messaging.Priority priority);
        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        string Peek(string queueName, Guid ptr);
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        string Dequeue(string queueName, Guid ptr);
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        string Dequeue(string queueName, Nistec.Messaging.Priority priority);
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        string Dequeue(string queueName);

        void Enqueue(string queueName, string serItem);

        void Completed(string queueName, Guid ItemId, int status, bool hasAttach);

        void ReEnqueue(string queueName, string serItem);

        void AbortTrans(string queueName, Guid ItemId, bool hasAttach);

        void CommitTrans(string queueName, Guid ItemId, bool hasAttach);

        string RemoveItem(string queueName);

        void Clear(string queueName);

        //void TransBegin(string queueName, string serItem);

        #endregion

        //object ExecuteCommand(string queueName, string commandName, string command, params string[] param);

        void SetProperty(string queueName, string propertyName, object propertyValue);

        object GetProperty(string queueName, string propertyName);

        void ValidateCapacity(string queueName);

        //int ReEnqueueLog(string queueName);

        /// <summary>
        /// GetQueueItemsTable
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        DataTable GetQueueItemsTable(string queueName);

        /// <summary>
        /// GetQueueItems
        /// </summary>
        /// <returns></returns>
        IQueueItem[] GetQueueItems(string queueName);



    }
    public interface IRemoteManager
    {
        void AddQueue(IDictionary prop);

        void RemoveQueue(string queueName);

        void ClearAllItems(string name);

        string[] QueueList { get; }

        string Reply(string text);

        bool Initilaized(string queueName);

        bool QueueExists(string queueName);

        DataTable GetStatistic();

        //void TruncateDB();

        bool CanQueue(string queueName, uint count);

    }

    public interface IAsyncChannel
    {
        void StartAsyncChannel(int availableThread);
        void StopAsyncChannel();
        void Dispose();
        string ChannelName { get; }
        string QueueName { get; }
        string TargetName { get; }
        //int MaxThread { get;}
        int AvailableThread { get; }
        GenericThreadPool MainThreadPool { get; }

        event ErrorOcurredEventHandler ErrorOcurred;
        event MessageEventHandler MessageHandler;

    }

    public interface IQueueProperty
    {
        int Server { get; }
        string QueueName { get; }
        int MaxThread { get; }
        bool IsTrans { get; }
        byte MaxRetry { get; }
        //QueueProvider Provider { get;}
        QueueMode QueueMode { get; }
        Messaging.CoverMode CoverMode { get; }
        string ConnectionString { get; }
    }

    /// <summary>
    /// IRemoteChannel
    /// </summary>
    public interface IChannel
    {
        string QueueName { get; }
        void StartAsyncQueue();//int availableThread);
        void StopAsyncQueue();
        void Dispose();

        void Enqueue(IQueueItem item);
        bool IsRemote { get; }
        event ErrorOcurredEventHandler ErrorOcurred;
        //event QueueItemEventHandler ReceiveCompleted;
        event Messaging.QueueItemEventHandler MessageReceived;

    }

    public interface IRemoteChannel : IChannel
    {
        //IQueueItem AsyncReceive();
        IQueueItem AsyncReceive(object state);
        IAsyncResult BeginReceive(object state);
        IAsyncResult BeginReceive(TimeSpan timeout, object state);
        IAsyncResult BeginReceive(TimeSpan timeout, object state, AsyncCallback callback);
        IQueueItem EndReceive(IAsyncResult asyncResult);

        bool HoldDequeue { get; }

        event ReceiveCompletedEventHandler ReceiveCompleted;
    }

    /// <summary>
    /// IAsyncQueue
    /// </summary>
    public interface IAsyncQueue : IReceiveCompleted
    {
        string QueueName { get; }
        int MaxItemsPerSecond { get; set; }
        int Count { get; }
        //bool IsTrans { get; }
        bool HoldDequeue { get; set; }
        bool Enabled { get; set; }
        bool Initilaized { get; }
        int MaxCapacity { get; }

        #region Queue action
        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        IQueueItem Peek();
        /// <summary>
        /// Peek Message
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        IQueueItem Peek(Nistec.Messaging.Priority priority);
        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        IQueueItem Peek(Guid ptr);
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        IQueueItem Dequeue(Guid ptr);
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        IQueueItem Dequeue(Nistec.Messaging.Priority priority);
        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        IQueueItem Dequeue();

        void Enqueue(IQueueItem item);
        //void Completed(IQueueItem item, ItemState status);
        void ReEnqueue(IQueueItem item);
        #endregion

        bool CanQueue(uint count);

        void ValidateCapacity();

        void SetProperty(string propertyName, object propertyValue);

        IQueueItem AsyncReceive();
        IAsyncResult BeginReceive(object state);
        IAsyncResult BeginReceive(TimeSpan timeout, object state);
        IAsyncResult BeginReceive(TimeSpan timeout, object state, AsyncCallback callback);
        //IQueueItem EndReceive(IAsyncResult asyncResult);

        event ReceiveCompletedEventHandler ReceiveCompleted;
        event ErrorOcurredEventHandler ErrorOcurred;

        /// <summary>
        /// GetQueueItemsTable
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        DataTable GetQueueItemsTable();

        /// <summary>
        /// GetQueueItems
        /// </summary>
        /// <returns></returns>
        IQueueItem[] GetQueueItems();

    }


    public interface IReceiveCompleted
    {
        bool IsTrans { get; }
        void Completed(Guid itemId, int status, bool hasAttach);
        IQueueItem EndReceive(IAsyncResult asyncResult);
        //void RequestCompleted(IAsyncResult asyncResult);
    }

    public interface IQueueTrans
    {
        void CommitTrans(Guid itemId, bool hasAttach);
        void AbortTrans(Guid itemId, bool hasAttach);
        //void AbortTrans(Guid itemId, bool rollback);
    }
}


