using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Nistec.Generic;
using Nistec.Messaging.Remote;
using Nistec.IO;
using System.Threading.Tasks;
using System.Transactions;
using Nistec.Logging;
using Nistec.Messaging.Listeners;
using Nistec.Runtime;

namespace Nistec.Messaging.Io
{
    /// <summary>
    /// Represent an db adapter for client\server file queue.
    /// </summary>
    public class FolderQueue
    {
        #region members

        //public const string FileExt = ".mcq";
        //public const int DefaultConnectTimeout = 5000;

        public const int DefaultWaitInterval = 100;
        int _WaitInterval = DefaultWaitInterval;
        public int WaitInterval { get { return _WaitInterval; } set { _WaitInterval = value <= 10 ? DefaultWaitInterval : value; } }

        QueueItem CurrentItem = null;


        FileMessage m_fileMessage;
        public FileMessage FileMessage
        {
            get { return m_fileMessage; }
        }

        //QueueHost m_host;
        //public QueueHost Host
        //{
        //    get { return m_host; }
        //}


        #endregion

        #region properties

        ///// <summary>
        ///// Get or Set RootPath
        ///// </summary>
        //public string RootPath { get; private set; }

        /// <summary>
        /// Get or Set the destination <see cref="QueueHost"/> host properties.
        /// </summary>
        public QueueHost Destination { get; set; }
        ///// <summary>
        ///// Get or Set the <see cref="AdapterOperations"/> property.
        ///// </summary>
        //public AdapterOperations OperationType { get; set; }
        /// <summary>
        /// Get or Set the <see cref="FileOrderTypes"/> property.
        /// </summary>
        public FileOrderTypes FileOrderType { get; set; }
       
        /// <summary>
        /// Get or Set indicating whether the adapter use transactional operation.
        /// </summary>
        public bool IsCoverable { get; set; }
        /// <summary>
        /// Get or Set the maximum number of items to fetch for each session, default is 1.
        /// </summary>
        public int MaxItemsPerSession { get; set; }
        /// <summary>
        /// Get or Set the delegate of target methods.
        /// </summary>
        public Action<QueueItem> QueueAction { get; set; }

        /// <summary>
        /// Get or Set the delegate of acknowledgment methods.
        /// </summary>
        public Action<IQueueAck> AckAction { get; set; }

        //int _WorkerCount;
        ///// <summary>
        ///// Gets or Set the number of worker count.
        ///// </summary>
        //public int WorkerCount
        //{
        //    get { return _WorkerCount; }
        //    set
        //    {
        //        if (value > 0)
        //        {
        //            _WorkerCount = value;
        //        }
        //    }
        //}

        //int _interval;
        ///// <summary>
        ///// Gets or Set the interval sleep in milliseconds between tasks.
        ///// </summary>
        //public int Interval
        //{
        //    get { return _interval; }
        //    set
        //    {
        //        if (value > 10)
        //        {
        //            _interval = value;
        //        }
        //    }
        //}

        int _ConnectTimeout;
        /// <summary>
        /// Gets or Set the connect tomeout in milliseconds.
        /// </summary>
        public int ConnectTimeout
        {
            get { return _ConnectTimeout; }
            set
            {
                if (value >= 0)
                {
                    _ConnectTimeout = value;
                }
            }
        }
        #endregion

        #region methods

        string GetFilename(Ptr ptr)
        {
            return Assists.GetFilename(m_fileMessage.RootPath, ptr.Host, ptr.Identifier, IsCoverable);
        }

        /// <summary>
        /// Return the host name of current adapter.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Host: {0}", m_fileMessage == null ? "" : m_fileMessage.HostName);
        }

        ///// <summary>
        ///// Ensure that adapter use the correct method.
        ///// </summary>
        //public void EnsureOperations()
        //{
        //    switch (OperationType)
        //    {
        //        //case AdapterOperations.Sync:
        //        //    EnsureSync(); break;
        //        case AdapterOperations.Recieve:
        //            EnsureRecieve(); break;
        //        case AdapterOperations.Transfer:
        //            EnsureTransfer(); break;
        //    }
        //}

        ///// <summary>
        ///// Ensure that adapter use sync method.
        ///// </summary>
        //public void EnsureSync()
        //{
        //    MaxItemsPerSession = 1;
        //    OperationType = AdapterOperations.Sync;
        //}
        /// <summary>
        /// Ensure that adapter use async method and the target action is defined.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void EnsureRecieve()
        {
            //if (OperationType != AdapterOperations.Recieve)
            //{
            //    throw new ArgumentException("Incorrect OperationType, it is not an async type");
            //}
            if (QueueAction == null)
            {
                throw new ArgumentException("Invalid QueueAction Adapter");
            }
            if (MaxItemsPerSession <= 0)
            {
                MaxItemsPerSession = 1;
            }
        }
        /// <summary>
        /// Ensure that adapter use transfer method and the destination host and target action is defined.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void EnsureTransfer()
        {
            //if (OperationType != AdapterOperations.Transfer)
            //{
            //    throw new ArgumentException("Incorrect OperationType, it is not an transfer type");
            //}
            if (QueueAction == null)
            {
                throw new ArgumentException("Invalid QueueAction Adapter");
            }
            if (Destination == null)
            {
                throw new ArgumentException("Invalid Destination Adapter");
            }

            if (MaxItemsPerSession <= 0)
            {
                MaxItemsPerSession = 1;
            }
        }

        #endregion

        #region ctor

        /// <summary>
        /// Initialize a new instance of folder queue.
        /// </summary>
        /// <param name="host"></param>
        public FolderQueue(QueueHost host)
        {

            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            m_fileMessage = new FileMessage(host);

            //m_host = host;
            //OperationType = AdapterOperations.Recieve;
            FileOrderType = FileOrderTypes.ByName;
            IsCoverable = false;
            MaxItemsPerSession = 1;
            ConnectTimeout = FileMessage.DefaultConnectTimeout;
        }

        //public static FolderQueue CreateSync(
        //    QueueHost host,
        //    int connectTimeout,
        //    int maxItemsPersession,
        //    FileOrderTypes orderType,
        //    bool isCoverable)
        //{
        //    return new FolderQueue(host)
        //    {
        //        FileOrderType = orderType,
        //        MaxItemsPerSession = maxItemsPersession,
        //        ConnectTimeout = connectTimeout,
        //        IsCoverable = isCoverable,
        //        OperationType = AdapterOperations.Async,
        //    };
        //}

        public static FolderQueue CreateAsync(
            QueueHost host, 
            int connectTimeout,
            int maxItemsPersession, 
            FileOrderTypes orderType, 
            Action<QueueItem> targetAction,
            bool isCoverable)
        {
            return new FolderQueue(host)
            {
                FileOrderType = orderType,
                MaxItemsPerSession = maxItemsPersession,
                QueueAction = targetAction,
                ConnectTimeout = connectTimeout,
                IsCoverable = isCoverable
                //OperationType = AdapterOperations.Recieve,
            };
        }

        public static FolderQueue CreateAsyncTransfer(
            QueueHost host,
            int connectTimeout,
            int maxItemsPersession,
            FileOrderTypes orderType,
            Action<IQueueAck> ackAction,
            QueueHost hostDestination,
            bool isCoverable)
        {
            return new FolderQueue(host)
            {
                FileOrderType = orderType,
                MaxItemsPerSession = maxItemsPersession,
                AckAction = ackAction,
                ConnectTimeout = connectTimeout,
                IsCoverable = isCoverable,
                //OperationType = AdapterOperations.Transfer,
                Destination=hostDestination
            };
        }
        #endregion

        #region override
        /// <summary>
        /// Send message to queue.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IQueueAck Enqueue(QueueItem message)
        {
            return m_fileMessage.Enqueue(message);

            //message.SetArraived();
            //string filename = Ptr.GetPtrLocation(m_fileMessage.Host.QueuePath, message.Identifier);
            //var stream = message.Serialize(true);
            //stream.SaveToFile(filename);
            //return new QueueItem(message, MessageState.Received, (string)null);
        }

        /// <summary>
        /// Commit trasaction.
        /// </summary>
        /// <param name="ptr"></param>
        public void Commit(Ptr ptr)
        {
            m_fileMessage.Commit(ptr);
        }
        /// <summary>
        /// Abort transaction and try again, according to the max retry definitions.
        /// </summary>
        /// <param name="ptr"></param>
        public void Abort(Ptr ptr)
        {
            m_fileMessage.Abort(ptr);
        }

        /// <summary>
        /// Dequeue message from queue, using sync methods.
        /// </summary>
        /// <returns></returns>
        public IQueueItem Dequeue()
        {

            try
            {
                CurrentItem = null;
                //EnsureSync();

                if (m_fileMessage.CanQueue())
                {
                    //Thread.Sleep(1000);
                    string folder = m_fileMessage.GetFirstBatchFolder(FileOrderType);
                    if (folder != null)
                    {
                        if (m_fileMessage.DequeueFolder(FileOrderType,MaxItemsPerSession) > 0)
                        {
                            return CurrentItem;//.GetMessage();
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("error FolderQueue.Dequeue :{0}, Trace:{1} ", ex.Message, ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Dequeue message from queue with specified item id , using sync methods.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public IQueueItem DequeueItem(string identifier)
        {
            try
            {
                CurrentItem = null;
                //EnsureOperations();
                EnsureRecieve();

                if (m_fileMessage.CanQueue())
                {
                    string filename = m_fileMessage.Host.GetFullFilename(identifier);
                    var item = m_fileMessage.DequeueFile(filename);
                    if (item == null)
                        return null;
                    return item;//.GetMessage();
                    //if (m_fileMessage.DequeueFile(filename) > 0)
                    //{
                    //    if (CurrentItem != null)
                    //    {
                    //        return CurrentItem.GetMessage();
                    //    }
                    //}
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("error FolderQueue.Dequeue :{0}, Trace:{1} ", ex.Message, ex.StackTrace);
            }
            return null;
        }


        public void DequeueAsync(Action<string> onFault, Action<QueueItem> onCompleted, DuplexTypes DuplexType, AutoResetEvent resetEvent)// = DuplexTypes.WaitOne)
        {
            try
            {
                CurrentItem = null;
                //EnsureSync();

                if (DuplexType == DuplexTypes.WaitOne)
                {

                    var cancellationTokenSource = new CancellationTokenSource();

                    Task.Factory.StartNew(
                    () =>
                    {
                        for (;;)
                        {
                            if (cancellationTokenSource.Token.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitInterval)))
                                break;


                            if (m_fileMessage.CanQueue())
                            {
                                //Thread.Sleep(1000);
                                string folder = m_fileMessage.GetFirstBatchFolder(FileOrderType);
                                if (folder != null)
                                {
                                    if (m_fileMessage.DequeueFolder(FileOrderType, MaxItemsPerSession) > 0)
                                    {
                                        onCompleted(CurrentItem);
                                        break;
                                    }
                                }
                            }
                            //else
                            //{
                            //    Thread.Sleep(1000);
                            //}

                        }
                    }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                }
                else
                {


                    if (m_fileMessage.CanQueue())
                    {
                        //Thread.Sleep(1000);
                        string folder = m_fileMessage.GetFirstBatchFolder(FileOrderType);
                        if (folder != null)
                        {
                            if (m_fileMessage.DequeueFolder(FileOrderType, MaxItemsPerSession) > 0)
                            {
                                onCompleted(CurrentItem);
                                //return CurrentItem;//.GetMessage();
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("error FolderQueue.Dequeue :{0}, Trace:{1} ", ex.Message, ex.StackTrace);
            }


            //QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.Dequeue, null);
            //{
            //    Host = _QueueName,
            //    QCommand = QueueCmd.Dequeue,
            //    DuplexType = DuplexType
            //};

            //base.SendDuplexAsync(request, onFault, onCompleted);

        }
        /// <summary>
        /// Dequeue message from queue, using async methods.
        /// </summary>
        /// <returns></returns>
        public int DequeueAsync()
        {
            int count = 0;
            try
            {
                CurrentItem = null;
                EnsureRecieve();

                if (m_fileMessage.CanQueue())
                {
                    //Thread.Sleep(1000);
                    string folder = m_fileMessage.GetFirstBatchFolder(FileOrderType);
                    if (folder != null)
                    {
                        count = m_fileMessage.DequeueFolder(FileOrderType,MaxItemsPerSession);
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("error FolderQueue.DequeueAsync :{0}, Trace:{1} ", ex.Message, ex.StackTrace);
            }

            return count;
        }
        /// <summary>
        /// Recieve and transfer messages from queue to other queue, using async methods.
        /// </summary>
        /// <returns></returns>
        public IQueueAck ReceiveTo()
        {
            int count = 0;
            try
            {
                CurrentItem = null;
                EnsureTransfer();

                if (m_fileMessage.CanQueue())
                {
                    //Thread.Sleep(1000);
                    string folder = m_fileMessage.GetFirstBatchFolder(FileOrderType);
                    if (folder != null)
                    {
                        count = m_fileMessage.DequeueFolderTransfer(FileOrderType, 1);
                        //if (count > 0 && CurrentItem != null)
                        //{
                        //    return new QueueItem() { Host = m_fileMessage.HostName, State = MessageState.Received, Identifier = CurrentItem.Identifier };
                        //}
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("error FolderQueue.DequeueAsync :{0}, Trace:{1} ", ex.Message, ex.StackTrace);
            }
            //return count;

            //CurrentItem = null;
            //int count = m_fileMessage.DequeueFileTransfer();
            //if (count > 0 && CurrentItem != null)
            //{
            //    return new QueueItem() { Host = m_host.HostName, State = MessageState.Received, Identifier = CurrentItem.Identifier };
            //}
            //return count;


            return new QueueAck()
            {
                //ArrivedTime = DateTime.Now,
                MessageState = MessageState.Received,
                Count = count
            };
        }

        #endregion

    }
}
