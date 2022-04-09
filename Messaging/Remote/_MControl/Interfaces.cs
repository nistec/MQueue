using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Threading;
using System.Messaging;

using MControl.Data;
using MControl.Runtime;
using MControl.Threading;

namespace MControl.Messaging
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
        string Peek(string queueName, Priority priority);
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
        string Dequeue(string queueName, Priority priority);
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

 
}


