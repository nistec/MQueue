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
    //    QueueItem=100,
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

        //reports
        Exists = 60,
        QueueProperty = 61,
        ReportQueueList = 62,
        ReportQueueItems = 63,
        ReportQueueStatistic = 64,
        PerformanceCounter = 65,
        QueueCount = 66,

        /// <summary>
        /// A normal Message Queuing message.
        /// </summary>
        QueueItem = 100,
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
    };

    //public enum QueueManagerCmd : byte
    //{
    //    QueueList = 0,
        
    //}


    //public class QueueCmd
    //{
    //    public const string Reply = "Reply";
    //    public const string RemoveItem = "RemoveItem";
    //    public const string Enqueue = "Enqueue";
    //    public const string Dequeue = "Dequeue";
    //    public const string DequeuePriority = "DequeuePriority";
    //    public const string Fetch = "Fetch";
    //    public const string Peek = "Peek";
    //    public const string Commit = "Commit";

       
    //}

    //public class QueueManagerCmd
    //{
    //    public const string Reply = "Reply";
    //    public const string QueueProperties = "QueueProperties";
    //    public const string Timeout = "Timeout";
    //    public const string SessionTimeout = "SessionTimeout";
    //    public const string GetAllKeys = "GetAllKeys";
    //    public const string GetAllKeysIcons = "GetAllKeysIcons";
    //    public const string CloneItems = "CloneItems";
    //    public const string GetStatistic = "GetStatistic";
    //    public const string GetDataStatistic = "GetDataStatistic";
    //    public const string QueueToXml = "QueueToXml";
    //    public const string QueueFromXml = "QueueFromXml";
    //    public const string QueueLog = "QueueLog";
    //    public const string Log = "Log";
    //    public const string Reset = "Reset";

    //}

    public class KnownArgs
    {
        public const string Priority = "Priority";
        public const string Ptr = "Ptr";
 
    }
}
