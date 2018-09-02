using System;
using System.Collections.Generic;
using System.Text;

namespace Nistec.Messaging
{

    public static class MessageContext
    {
        //public const string MMSG = "MMSG";
        public const string QREQ = "QREQ";
        public const string QACK = "QACK";
        public const string QPRO = "QPRO";
        public const string QPTR = "QPTR";
    }

    
    // Summary:
    //     Specifies the result of an attempted message delivery.
    public enum Acknowledgment
    {
        // Summary:
        //     The message is not an acknowledgment message.
        None = 0,
        //
        // Summary:
        //     A positive arrival acknowledgment indicating that the original message reached
        //     its destination queue.
        ReachQueue = 2,
        //
        // Summary:
        //     A positive read acknowledgment indicating that the original message was received
        //     by the receiving application.
        Receive = 16384,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that the destination queue is
        //     not available to the sending application.
        BadDestinationQueue = 32768,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that the message was purged
        //     before reaching its destination queue.
        Purged = 32769,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that the time-to-reach-queue
        //     or time-to-be-received timer expired before the original message could reach
        //     the destination queue.
        ReachQueueTimeout = 32770,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that the original message was
        //     not delivered because its destination queue is full.
        QueueExceedMaximumSize = 32771,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that the sending application
        //     does not have the necessary rights to send a message to the destination queue.
        AccessDenied = 32772,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that the original message's
        //     hop count (which indicates the number of intermediate servers) was exceeded.
        HopCountExceeded = 32773,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that the original message's
        //     digital signature is not valid and could not be authenticated by Message
        //     Queuing.
        BadSignature = 32774,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that the destination queue manager
        //     could not decrypt a private message.
        BadEncryption = 32775,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that the source queue manager
        //     could not encrypt a private message.
        CouldNotEncrypt = 32776,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that a transactional message
        //     was sent to a non-transactional queue.
        NotTransactionalQueue = 32777,
        //
        // Summary:
        //     A negative arrival acknowledgment indicating that a non-transactional message
        //     was sent to a transactional queue.
        NotTransactionalMessage = 32778,
        //
        // Summary:
        //     A negative read acknowledgment indicating that the queue was deleted before
        //     the message could be read.
        QueueDeleted = 49152,
        //
        // Summary:
        //     A negative read acknowledgment indicating that the queue was purged before
        //     the message could be read.
        QueuePurged = 49153,
        //
        // Summary:
        //     A negative read acknowledgment indicating that the original message was not
        //     received from the queue before its time-to-be-received timer expired.
        ReceiveTimeout = 49154,
    }

    //// Summary:
    ////     Specifies the status of a message.
    //public enum MessageState
    //{
    //    // Summary:
    //    //     The message has been created.
    //    Created = 0,
    //    //
    //    // Summary:
    //    //     The message is being read.
    //    Read = 1,
    //    //
    //    // Summary:
    //    //     The message has been written.
    //    Written = 2,
    //    //
    //    // Summary:
    //    //     The message has been copied.
    //    Copied = 3,
    //    //
    //    // Summary:
    //    //     The message has been closed and can no longer be accessed.
    //    Closed = 4,
    //}

    // Summary:
    //     Identifies the type of a message. A message can be a typical Message Queuing
    //     message, a positive (arrival and read) or negative (arrival and read) acknowledgment
    //     message, or a report message.
    


    public enum ReceiveState
    {
        Wait = 0,
        Success=1,
        Failed=2,
        Timeout=3
    }

    //public enum QueueMode
    //{
    //    Manual = 0,
    //    Auto = 1
    //}

    

    public enum DistributionMode
    {
        BatchToMailer,
        SplitToQueue,
        DirectToQueue,
        //BatchToFolder
    }

    public enum ChunkMode
    {
        OneItem = 0,
        Multiple = 1,
        //Attachments = 2
    }

    //public enum TransState
    //{
    //    Wait = 0,
    //    Abort = 1,
    //    Commit = 2,
    //}

    public enum ItemState
    {
        Hold = 0,
        Enqueue = 1,
        Dequeue = 2,
        DequeueTran = 3,
        Abort = 4,
        Commit = 5,
    }


    //public enum QueueItemStatus
    //{
    //    Enqueue = 0,
    //    Dequeue = 1,
    //    Failed = 2,
    //    Completed = 4,
    //}

    public enum QueueItemType
    {
        QueueItems = 0,
        AttachItems = 1,
        HoldItems = 2,
        AllItems = 4,
    }

    public enum Priority : byte
    {
        Normal = 2,
        Medium = 1,
        High = 0
    }

    public enum ChannelType
    {
        AsyncQueue,
        MSMQueue
    }

    public enum QueueProvider
    {
        None = 0,
        Sqlite = 1,
        SqlServer = 2
    }

    public enum QueueItemFormat
    {
        Base64 = 0,
        Bytes = 1
    }

    //public enum QueueMode : byte
    //{
    //    Memory,
    //    MemoryFile,
    //    //FileStream,
    //    MemoryDb,
    //}

    public enum CoverMode : byte
    {
        Memory=0,
        Persistent=1,
        File=2,
        Db=3,
        Rout=100
    }

    public enum PersistCommitMode:byte
    {
        OnDisk = 0,
        OnMemory = 1,
        None = 2
    }

    //public enum CoverMode
    //{

    //    None,
    //    Memory,
    //    MemoryFile,
    //    FileStream,
    //    FileSystem,
    //    Transactional

    //    ////ItemsOnly = 1,
    //    ////ItemsAndLog = 2,
    //    ////LogNoState = 3,
    //    ////LogAndState = 4,
    //    //FileSystem = 5
    // }

    public enum PtrState : byte
    {
        None = 0,
        //QueueInHold,
        //MaxRetryExceeds,

        Arrived = 11,
        Receiving = 12,
        Received = 13,

        OperationFailed = 20,
        OperationCanceled = 21,
        TransAborted = 22,
        TransCommited = 23,

        CapacityExeeded = 112,
        RetryExceeds = 114,
        QueueInHold = 118,
    }


    public enum MessageState : byte
    {
        None = 0,
        Ok = 1,

        Sending = 10,
        Arrived = 11,
        Receiving = 12,
        Received = 13,
        Peeking=14,
        Peeked = 15,

        OperationFailed = 20,
        OperationCanceled = 21,
        TransAborted = 22,
        TransCommited = 23,
        FailedEnqueue = 24,
        FailedDequeue = 25,
        FailedPeek = 26,

        AllreadyExists = 100,
        QueueNotFound = 101,
        Timeout = 102,
        PipeError = 103,
        RemoteConnectionError = 104,
        MessageError = 105,
        SerializeError = 106,
        SecurityError = 107,
        ArgumentsError = 108,
        BadDestination = 109,
        InvalidMessageHost = 110,
        InvalidMessageBody = 111,
        CapacityExeeded = 112,
        MessageTypeNotSupported = 113,
        RetryExceeds = 114,
        InvalidMessageAction = 115,
        PathNotFound = 116,
        StreamReadWriteError = 117,
        QueueInHold = 118,
        UnExpectedError = 199
    }

    public enum ReadFileState
    {
        None,
        Completed,
        NotExists,
        IOException,
        TransactionException,
        Exception

    }

 
    /*
    /// <summary>
    /// AcknowledgStatus
    /// </summary>
    public enum MessageState
    {
        /// <summary>None 0</summary>
        None = 0,
        /// <summary>Received 1</summary>
        Received = 1,
        /// <summary>Delivered 2</summary>
        Delivered = 2,
        /// <summary>UnExpectedError 1000</summary>
        UnExpectedError = 1000,
        /// <summary>XmlParsingError 1001</summary>
        XmlParsingError = 1001,
        /// <summary>AccountError 1002</summary>
        AccountError = 1002,
        /// <summary>CreditError 1003</summary>
        CreditError = 1003,
        /// <summary>NetworkError 1004</summary>
        NetworkError = 1004,
        /// <summary>TargetError 1005</summary>
        TargetError = 1005,
        /// <summary>AccessDenied 1006</summary>
        AccessDenied = 1006,
        /// <summary>BadDestination 1007</summary>
        BadDestination = 1007,
        /// <summary>ReceiveTimeout 1008</summary>
        ReceiveTimeout = 1008,
        /// <summary>BadRequest 1009</summary>
        BadRequest = 1009,
        /// <summary>InvalidMailerHost 1010</summary>
        InvalidMessageHost = 1010,
        /// <summary>InvalidMailerBody 1011</summary>
        InvalidMessageBody = 1011,
        /// <summary>RemoteConnectionError 1012</summary>
        RemoteConnectionError = 1012,

        InvalidMailerHost=1013


    }
    
    /// <summary>
    /// MessageStatus
    /// </summary>
    public enum MessageStatus
    {
        /// <summary>Wait 0</summary>
        Wait = 0,
        /// <summary>Process 1</summary>
        Process = 1,
        /// <summary>Canceled 2</summary>
        Canceled = 2,
        /// <summary>Rejected 3</summary>
        Rejected = 3,
        /// <summary>Failed 4</summary>
        Failed = 4,
        /// <summary>Success 9</summary>
        Success = 9
    }
    */
    public enum AsyncRequestState
    {
        Wait,
        Running,
        Completed,
        Abort,
        Timeout
    }

}
