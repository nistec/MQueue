using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging
{

    //public enum MessageTypes_2: byte
    //{
    //    /// <summary>
    //    /// A normal Message Queuing message.
    //    /// </summary>
    //    QueueMessage=100,
    //    /// <summary>
    //    /// A multiple Messages, Queuing messages.
    //    /// </summary>
    //    AttachItems=101,
    //    /// <summary>
    //    /// Batch To Mailer Queue
    //    /// </summary>
    //    BatchToMailer=102,
    //    /// <summary>
    //    /// Batch, Split file To Queue
    //    /// </summary>
    //    SplitToQueue=103,
    //    /// <summary>
    //    /// Batch, Direct To Queue
    //    /// </summary>
    //    DirectToQueue=104,
    //    /// <summary>
    //    /// An acknowledgment message.
    //    /// </summary>
    //    Ack=105,
    //    /// <summary>
    //    /// A report message.
    //    /// </summary>
    //    Report=106
    //}

    public enum MQTypes : byte
    {
        Message = 0,
        MessageRequest = 1,
        Ack =2 ,
        Json=3
    }

    public enum TransformTypes : byte
    {
        Duplex = 0,
        OneWay = 1,
        Notify = 2
    }

    public enum QueueCmd : byte //: byte
    {
        None=0,
        Reply=1,

        Enqueue = 10,
        Dequeue = 11,
        DequeuePriority = 12,
        DequeueItem = 13,
        Peek = 14,
        PeekPriority = 15,
        PeekItem = 16,
        RemoveItem = 17,
        Consume=18,
        //trans
        Commit = 20,
        Abort = 21,
        QueueHasValue = 22,

        //operation
        AddQueue = 30,
        RemoveQueue = 31,
        HoldEnqueue = 32,
        ReleaseHoldEnqueue = 33,
        HoldDequeue = 34,
        ReleaseHoldDequeue = 35,
        EnableQueue = 36,
        DisableQueue = 37,
        ClearQueue = 38,
        BackupQueue=39,
        //publish\subscribe
        TopicAdd = 40,
        TopicRemove = 41,
        TopicPublish = 42,
        TopicSubscribe = 43,
        TopicRemoveItem = 44,
        TopicCommit = 45,
        TopicAbort = 46,

        TopicHold = 47,
        TopicHoldRelease = 48,
        TopicSubscribeHold=49,
        TopicSubscribeRelease=50,
        TopicSubscribeAdd = 51,
        TopicSubscribeRemove = 52,

        BackupAll= 53,
        LoadFromBackup=54,
        MemoryFree = 55,
        //reports
        Exists = 60,
        QueueProperty = 61,
        ReportQueueList = 62,
        ReportQueueItems = 63,
        ReportQueueStatistic = 64,
        PerformanceCounter = 65,
        QueueCount = 66,
        QueueCountAll = 67,
        DbQueueReport = 68,
        DbQueueClear = 69,
        DbQueueClearItem = 70,
        QueryLabels=71,
        ReportQueueCounters=72,
        ReportCountersAll = 73,
        //QueueItemsCount = 72,
        //QueueItemsCountAll = 73,

        /// <summary>
        /// A normal Message Queuing message.
        /// </summary>
        QueueMessage = 100,
        /// <summary>
        /// A multiple Messages, Queuing messages.
        /// </summary>
        AttachItems=101,
        /// <summary>
        /// Batch To Mailer Queue
        /// </summary>
        BatchToMailer=102,
        /// <summary>
        /// Batch, Split file To Queue
        /// </summary>
        SplitToQueue=103,
        /// <summary>
        /// Batch, Direct To Queue
        /// </summary>
        DirectToQueue=104,
        /// <summary>
        /// An acknowledgment message.
        /// </summary>
        Ack=105,
        /// <summary>
        /// A report message.
        /// </summary>
        Report=106

    };

    public enum QueueCmdOperation : byte //: byte
    {
        HoldEnqueue = 32,
        ReleaseHoldEnqueue = 33,
        HoldDequeue = 34,
        ReleaseHoldDequeue = 35,
        EnableQueue = 36,
        DisableQueue = 37,
        ClearQueue=38,
        BackupQueue = 39,
        TopicAdd = 40,
        TopicRemove = 41,
        TopicPublish = 42,
        TopicSubscribe = 43,
        TopicRemoveItem = 44,
        TopicHold = 47,
        TopicHoldRelease = 48,
        TopicSubscribeHold = 49,
        TopicSubscribeRelease = 50,
        TopicSubscribeAdd = 51,
        TopicSubscribeRemove = 52,
        BackupAll = 53,
        LoadFromBackup = 54,
        MemoryFree = 55
    };

    public enum QueueCmdReport : byte //: byte
    {
        Exists = 60,
        QueueProperty = 61,
        ReportQueueList = 62,
        ReportQueueItems = 63,
        ReportQueueStatistic = 64,
        PerformanceCounter = 65,
        QueueCount = 66,
        QueueCountAll = 67,
        DbQueueReport = 68,
        DbQueueClear = 69,
        DbQueueClearItem = 70,
        QueryLabels=71,
        ReportQueueCounters = 72,
        ReportCountersAll = 73
        //QueueItemsCount = 72,
        //QueueItemsCountAll = 73
    };


    public class KnownArgs
    {
        public const string Priority = "Priority";
        public const string Ptr = "Ptr";
 
    }
}
