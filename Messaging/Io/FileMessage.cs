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
using Nistec.Messaging.Listeners;
using System.Collections;
using Nistec.Logging;

namespace Nistec.Messaging.Io
{
    /// <summary>
    /// Represent an db adapter for client\server file queue.
    /// </summary>
    public class FileMessage:IDisposable
    {
        #region members
 
        public const int DefaultConnectTimeout = 5000;

        //QueueItem CurrentItem = null;

        List<QueueItem> m_Items;
        List<QueueItem> Items
        {
            get
            {
                if (m_Items == null)
                {
                    m_Items = new List<QueueItem>();
                }
                return m_Items;
            }
        }

        public QueueItem[] ItemsArray
        {
            get
            {
                if (m_Items != null)
                {
                    lock (((ICollection)m_Items).SyncRoot)
                    {
                        return m_Items.ToArray();
                    }
                }
                return new List<QueueItem>().ToArray();
            }
        }


        QueueHost m_host;

        public QueueHost Host
        {
            get { return m_host; }
        }

        /// <summary>
        /// Get or Set HostName
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// Get or Set RootPath
        /// </summary>
        public string RootPath { get; private set; }

        internal string GetQueuePath()
        {
            return Path.Combine(RootPath, Assists.FolderQueue,HostName);
        }
        internal string GetInfoPath()
        {
            return Path.Combine(RootPath, Assists.FolderInfo, HostName);
        }

        internal string GetQueueOrInfoPath()
        {
            return Assists.GetQueuePath(RootPath, HostName, IsCoverable);// m_host.HostAddress;
        }
      
        public string GetQueueFilename(string identifier)
        {
            string path = Assists.EnsureIdentifierPath(QueuePath, identifier);
            return Path.Combine(path, Assists.GetQueueFilename(identifier));
        }

        public string GetInfoFilename(string identifier)
        {
            string path = Assists.EnsureIdentifierPath(QueueInfoPath, identifier);
            return Path.Combine(path, Assists.GetQueueFilename(identifier));
        }

        /// <summary>
        /// Get or Set RootPath
        /// </summary>
        public string QueuePath { get; private set; }
        public string QueueInfoPath { get; private set; }
        public string QueueCoveredPath { get; private set; }
        public string QueueSuspendPath { get; private set; }

        void InitFolders()
        {
            QueuePath = Path.Combine(RootPath, Assists.FolderQueue, HostName);
            QueueInfoPath = Path.Combine(RootPath, Assists.FolderInfo, HostName);
            QueueCoveredPath = Path.Combine(RootPath, Assists.FolderCovered, HostName);
            QueueSuspendPath = Path.Combine(RootPath, Assists.FolderSuspend, HostName);

        }

        #endregion

        #region properties
        
        /// <summary>
        /// Get or Set the destination <see cref="QueueHost"/> host properties.
        /// </summary>
        public QueueHost Destination { get; set; }
        ///// <summary>
        ///// Get or Set the <see cref="AdapterOperations"/> property.
        ///// </summary>
        //public AdapterOperations OperationType { get; set; }
        ///// <summary>
        ///// Get or Set the <see cref="FileOrderTypes"/> property.
        ///// </summary>
        //public FileOrderTypes FileOrderType { get; set; }
       
        /// <summary>
        /// Get or Set indicating whether the adapter use transactional operation.
        /// </summary>
        public bool IsCoverable { get; set; }
        ///// <summary>
        ///// Get or Set the maximum number of items to fetch for each session, default is 1.
        ///// </summary>
        //public int MaxItemsPerSession { get; set; }
        ///// <summary>
        ///// Get or Set the delegate of target methods.
        ///// </summary>
        //public Action<Message> TargetAction { get; set; }
        ///// <summary>
        ///// Get or Set the delegate of acknowledgment methods.
        ///// </summary>
        //public Action<IQueueAck> AckAction { get; set; }

        public Action<IQueueItem> QueueAction { get; set; }

        public Action<IQueueAck> TransferAction { get; set; }

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

        /// <summary>
        /// Return the host name of current adapter.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Host: {0}", m_host == null ? "" : m_host.HostName);
        }

        ///// <summary>
        ///// Ensure that adapter use the correct method.
        ///// </summary>
        //public void EnsureOperations(AdapterOperations operationType)
        //{
        //    EnsureRecieve();

        //    switch (operationType)
        //    {
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
        /// Ensure that adapter use recieve method and the target action is defined.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void EnsureRecieve()
        {
            //if (OperationType != AdapterOperations.Async)
            //{
            //    throw new ArgumentException("Incorrect OperationType, it is not an async type");
            //}
            if (QueueAction == null)
            {
                throw new ArgumentException("Invalid TargetAction Adapter");
            }
            //if (MaxItemsPerSession <= 0)
            //{
            //    MaxItemsPerSession = 1;
            //}
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
            if (TransferAction == null)
            {
                throw new ArgumentException("Invalid AckAction Adapter");
            }
            if (Destination == null)
            {
                throw new ArgumentException("Invalid Destination Adapter");
            }

            //if (MaxItemsPerSession <= 0)
            //{
            //    MaxItemsPerSession = 1;
            //}
        }

        #endregion

        #region ctor

        FileMessage()
        {

            //OperationType = AdapterOperations.Sync;
            //FileOrderType = FileOrderTypes.ByName;
            IsCoverable = false;
            //MaxItemsPerSession = 1;
            ConnectTimeout = DefaultConnectTimeout;
        }

        /// <summary>
        /// Initialize a new instance of folder queue.
        /// </summary>
        /// <param name="host"></param>
        public FileMessage(QueueHost host):this()
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            m_host = host;
            m_host.EnsureHost();
            HostName = host.HostName;
            RootPath = host.HostAddress;
            InitFolders();
       }

        /// <summary>
        /// Initialize a new instance of folder queue.
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="hostAddress"></param>
        public FileMessage(string hostName, string rootPath)
            : this()
        {
            if (hostName == null || rootPath == null)
            {
                throw new ArgumentNullException("host or hostAddress");
            }
            m_host = new QueueHost(QueueHost.GetRawAddress(HostProtocol.file, ".", rootPath, hostName));
            //m_host = new QueueHost(hostName, ".", rootPath, HostProtocol.file);
            m_host.EnsureHost();
            HostName = hostName;
            RootPath = rootPath;
            InitFolders();
        }
        /// <summary>
        /// Initialize a new instance of folder queue for async operation.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="action"></param>
        public FileMessage(QueueHost host, Action<IQueueItem> action)
            : this()
       {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            m_host = host;
            m_host.EnsureHost();
            HostName = host.HostName;
            RootPath = host.HostAddress;
            QueueAction = action;
            InitFolders();
            EnsureRecieve();
        }
        /// <summary>
        /// Initialize a new instance of folder queue for Transfer operation.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="destination"></param>
        /// <param name="action"></param>
        public FileMessage(QueueHost host, QueueHost destination, Action<IQueueAck> action)
            : this()
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            m_host = host;
            m_host.EnsureHost();
            HostName = host.HostName;
            RootPath = host.HostAddress;
            Destination = destination;
            TransferAction = action;
            InitFolders();
            EnsureTransfer();
        }
        #endregion

        #region Dispose

        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        bool disposed = false;
        /// <summary>
        /// Get indicate wether the current instance is Disposed.
        /// </summary>
        protected bool IsDisposed
        {
            get { return disposed; }
        }
        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                this.m_Items = null;
                this.HostName = null;
                this.QueueAction = null;
                this.TransferAction = null;
                if (Destination != null)
                {
                    Destination.Dispose();
                    Destination = null;
                }
                if (m_host != null)
                {
                    m_host.Dispose();
                    m_host = null;
                }
            }
            disposed = true;
        }
        #endregion
 
        #region folder listener

        //private int m_IntervalDequeue = 30000;
        private int InProcess = 0;
        //private int IsActive = 0;
        private int m_DeleteIntervalSeconds = 1000;


        //enqueue
        //set ptr to info
        //set item to queue
        
        //dequeue
        //get ptr from info
        //set ptr retry ++
        //set ptr to covered
        //delete ptr from info
        //get item from queue
        //set item retry


        //commit
        //delete ptr from covered
        //delete item from queue

        //abort
        //if retry>= maxRetry
        //move item from queue to suspended
        //move ptr from covered to suspended
        //else
        //set ptr retry ++
        //move info from covered to info


        
        public virtual bool CanQueue()
        {
            if (Thread.VolatileRead(ref InProcess) == 1)
            {
                return false;
            }
            Items.Clear();

            return true;
        }

        ///// <summary>
        ///// Send message to queue.
        ///// </summary>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public IQueueAck Enqueue(Message message)
        //{
        //    try
        //    {
        //        message.SetArraived();
        //        string qfile = GetQueueFilename(message.Identifier);
        //        using (var stream = message.Serialize(true))
        //        {
        //            stream.SaveToFile(qfile);
        //        }
        //        if (IsCoverable)
        //        {
        //            string ifile = GetInfoFilename(message.Identifier);
        //            Ptr ptr = message.GetPtr();
        //            ptr.SaveToFile(RootPath);
        //        }
        //        return new QueueAck(MessageState.Arrived, message.Label, message.Identifier);

        //        //return new MessageAck(message, MessageState.Arrived, message.Label);

        //        //QueueItem item = new QueueItem(message, true, QueueCmd.Ack);
        //        ////string filename = Ptr.GetPtrLocation(m_host.QueuePath, message.Identifier);
        //        //var copy = message.Copy(false, false);
        //        //copy.SetMessageState(MessageState.Ok);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new QueueAck(MessageState.MessageError, ex);
        //    }
        //}

        /// <summary>
        /// Send message to queue.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IQueueAck Enqueue(QueueItem message)
        {
            Ptr ptr = message.SetArrivedPtr(HostName);
            string qfile = GetQueueFilename(message.Identifier);
            message.SaveToFile(qfile);
            
            if (IsCoverable)
            {
                string ifile = GetInfoFilename(message.Identifier);
                //Ptr ptr = message.GetPtr(HostName);
                ptr.SaveToFile(RootPath);
            }
            //message.SetState(MessageState.Received);
            //return message.GetMessage();

            //var item= QueueItem.ReadFile(qfile);
            //Console.WriteLine(item.Print());

            return new QueueAck(MessageState.Received,message.Label, message.Identifier, message.Host);

            //return new Message(message.Identifier, HostName, MessageState.Received, (string)null);
        }

        public string GetFirstBatchFolder(FileOrderTypes fileOrderType)
        {
            string folder = null;
            try
            {
                string path = GetQueueOrInfoPath();

                DirectoryInfo di = new DirectoryInfo(path);
                if (di.Exists)
                {
                    DirectoryInfo[] dirs = di.GetDirectories();
                    if (dirs != null && dirs.Length > 0)
                    {
                        DirectoryInfo firstDir = null;
                        if (fileOrderType == FileOrderTypes.ByCreation)
                            firstDir = dirs.OrderBy(f => f.CreationTime).FirstOrDefault();
                        else
                            firstDir = dirs.OrderBy(f => f.Name).FirstOrDefault();

                        DirectoryInfo dir = (DirectoryInfo)firstDir;
                        folder = dir.FullName;
                    }
                    else
                    {
                        return di.FullName;
                    }
                }
            }
            catch (IOException iox)
            {
                Netlog.ErrorFormat("error GetFirstBatchFolder : " + iox.Message);
            }
            return folder;
        }

        public Ptr ReadInfoFile(string filename, bool checkRetry)
        {

            Ptr ptr = Ptr.Empty;

            ptr = Ptr.ReadFile(filename);
            if (ptr.IsEmpty)
            {
                throw new MessageException(MessageState.RetryExceeds, "item info not found in Ptr.ReadFile " + filename);
            }

            if (checkRetry && ptr.Retry >= Assists.MaxRetry)
            {
                Task.Factory.StartNew(()=> AbortRery(ptr, filename));
                throw new Exception("item info retry exceeds " + filename);
            }
            else
            {
                ptr.DoRetry();
                return ptr;
            }
        }

        public QueueItem ReadItem(string identifier, bool checkRetry)
        {
            string filename = GetQueueFilename(identifier);
            return ReadFile(filename, checkRetry);
        }

        public QueueItem ReadFile(string filename, bool checkRetry)
        {
            
            QueueItem item = null;

            item = QueueItem.ReadFile(filename);
            if (item == null)
            {
                throw new MessageException(MessageState.PathNotFound, "item not found in QueueItem.ReadFile " + filename);
            }

            if (checkRetry && item.Retry >= Assists.MaxRetry)
            {
                Task.Factory.StartNew(() => AbortRery(item, filename));
                throw new Exception("item retry exceeds " + filename);
            }
            else
            {
                item.DoRetryInternal();
                return item;
            }
        }

       
        internal void Commit(Ptr ptr)
        {
            
            if (IsCoverable)
            {
                string infofilename = Assists.GetInfoFilename(RootPath, HostName, ptr.Identifier);
                DeleteInfoAndItem(infofilename);
            }
            else
            {
                string itemfilename = Assists.GetQueueFilename(RootPath, HostName, ptr.Identifier);
                string itemcovered = Assists.QueueToCovered(itemfilename);
                DeleteItem(itemfilename);
                DeleteItem(itemcovered);
            }
        }

        internal void Abort(Ptr ptr)
        {
            if (IsCoverable)
            {
                string infofilename = Assists.GetInfoFilename(RootPath, HostName, ptr.Identifier);
                string infocovered = Assists.InfoToCovered(infofilename);
                Ptr p = ReadInfoFile(infocovered, false);
                if (p.Retry > Assists.MaxRetry)
                {
                    AbortRery(p, infofilename);
                }
                else if (!p.IsEmpty)
                {
                    DoRery(p, infofilename);
                }
            }
            else
            {
                string itemfilename = Assists.GetQueueFilename(RootPath, HostName, ptr.Identifier);
                string covered = Assists.QueueToCovered(itemfilename);
                var item = ReadFile(covered, false);
                if (item == null)
                {
                    return;
                }
                if (item.Retry > Assists.MaxRetry)
                {
                    AbortRery(item, itemfilename);
                }
                else
                {
                    DoRery(item, itemfilename);
                }
            }
        }


        internal void DoRery(Ptr ptr, string filename)
        {
            string infocovered = Assists.InfoToCovered(filename);
            ptr.DoRetry();
            ptr.SaveToFile(filename);
            File.Delete(infocovered);
        }

        internal void DoRery(QueueItem item, string filename)
        {
            string covered = Assists.QueueToCovered(filename);
            item.DoRetryInternal();
            item.SaveToFile(filename);
            File.Delete(covered);
        }
        internal void AbortRery(Ptr ptr, string filename)
        {
            string suspendPath = Assists.EnsureQueueSectionPath(RootPath, Assists.FolderSuspend, HostName);
            string infocovered = Assists.InfoToCovered(filename);

            ptr.SaveToFile(Assists.InfoToSuspend(filename));
            File.Delete(filename);
            File.Delete(infocovered);

            string itemfile = Assists.InfoToQueue(filename);
            var itemq = ReadFile(itemfile, false);
            if (itemq != null)
            {
                AbortRery(itemq, itemfile);
            }
        }

        internal void AbortRery(QueueItem item, string filename)
        {
            string suspendPath = Assists.EnsureQueueSectionPath(RootPath, Assists.FolderSuspend, HostName);
            string suspendfile = Assists.QueueToSuspend(filename);
            string coveredfile = Assists.QueueToCovered(filename);
            item.SaveToFile(suspendfile);
            File.Delete(filename);
            File.Delete(coveredfile);
        }

        internal void SuspendInfo(Ptr ptr, string filename, string infocoverfile)
        {
            string suspendPath = Assists.EnsureQueueSectionPath(RootPath, Assists.FolderSuspend, HostName);
            string infosuspendfile = Assists.InfoToSuspend(filename);
            ptr.SaveToFile(Assists.InfoToCovered(infosuspendfile));
            File.Delete(infocoverfile);
        }

        internal void DequeueInfo(string filename, ref QueueItem item)
        {
            Ptr ptr = ReadInfoFile(filename,false);
            string coverPath = Assists.EnsureQueueSectionPath(RootPath, Assists.FolderCovered, HostName);
            string infocoverfile = Assists.InfoToCovered(filename);
            ptr.SaveToFile(infocoverfile);
            File.Delete(filename);
            string queuefile = Assists.InfoToQueue(filename);
            item = ReadFile(queuefile,false);
            if (item == null)
            {
                Task.Factory.StartNew(() => SuspendInfo(ptr, filename, infocoverfile));
            }
        }

        public QueueItem DequeueItem(string identifier)
        {
            string filename = GetQueueFilename(identifier);
            return DequeueFile(filename);
        }

        public QueueItem DequeueFile(string filename)
        {
            QueueItem item = null;
            try
            {

                if (IsCoverable)
                {
                     DequeueInfo(filename, ref item);
                }
                else
                {
                    item = ReadFile(filename,true);
                    File.Delete(filename);
                }

                Netlog.DebugFormat("item deleted: " + filename);
            }
            catch (IOException iox)
            {
                Netlog.ErrorFormat("error IO DequeueFile item: " + iox.Message);
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("error DequeueFile MessageState:{0}, message:{1} ", mex.MessageState, mex.Message);
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("Unexpected error DequeueFile item: {0}", ex.Message);
            }

            return item;
        }


        int DequeueFileSync(string filename)
        {
            int count = 0;

            QueueItem item = null;

            //if (IsCoverable)
            //{
            //    DequeueInfo(filename, ref item);
            //}
            //else
            //{
            //    item = ReadFile(filename);
            //}
            //if (item == null)
            //    return count;

            item = DequeueFile(filename);
            if (item == null)
                return count;
            lock (((ICollection)Items).SyncRoot)
            {
                Items.Add(item);
            }
            count++;
            return count;
        }

        public int DequeueFileAsync(string filename)
        {
            int count = 0;
            QueueItem item = null;
            try
            {
                item = DequeueFile(filename);
                if (item == null)
                    return count;
                               
                if (QueueAction == null)
                {
                    throw new MessageException(MessageState.InvalidMessageAction, "TargetAction not defined in " + m_host.HostName);
                }

                QueueAction(item);//.GetMessage());
               
                //if (IsTrans)
                //{
                //    Directory.CreateDirectory(m_host.CoveredPath);
                //    item.SaveToFile(m_host.CoveredPath);
                //}

                //File.Delete(filename);
                count++;
                Netlog.DebugFormat("item deleted: " + filename);
            }
            catch (IOException iox)
            {
                Netlog.ErrorFormat("error IO DequeueFile item: " + iox.Message);
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("error DequeueFile MessageState:{0}, message:{1} ", mex.MessageState, mex.Message);
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("Unexpected error DequeueFile item: {0}", ex.Message);
            }

            return count;
        }

        public int DequeueFileTransfer(string filename)
        {
            int count = 0;
            QueueItem item = null;
            try
            {
                item = DequeueFile(filename);
                if (item == null)
                    return count;

                if (TransferAction == null)
                {
                    throw new MessageException(MessageState.InvalidMessageAction, "AckAction not defined in " + m_host.HostName);
                }
                if (Destination == null)
                {
                    throw new MessageException(MessageState.InvalidMessageHost, "Destination host not defined in " + m_host.HostName);
                }
                //TODO
                //RemoteClient client = new RemoteClient(Destination);
                //client.SendAsync(item.GetMessage(), ConnectTimeout, AckAction);
                //end todo


                
                //CurrentItem = item;

                //if (IsTrans)
                //{
                //    Directory.CreateDirectory(m_host.CoveredPath);
                //    item.SaveToFile(m_host.CoveredPath);
                //}

                //File.Delete(filename);

                count++;
                Netlog.DebugFormat("item deleted: " + filename);
            }
            catch (IOException iox)
            {
                Netlog.ErrorFormat("error IO DequeueFile item: " + iox.Message);
            }
            catch (MessageException mex)
            {
                Netlog.ErrorFormat("error DequeueFile MessageState:{0}, message:{1} ", mex.MessageState, mex.Message);
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("Unexpected error DequeueFile item: {0}", ex.Message);
            }

            return count;
        }

        public int DequeueFolder(FileOrderTypes fileOrderType, int maxItemsPerSession)
        {
            int count = 0;
            bool completed = false;
            int maxItems = 0;

            if(!CanQueue())
            {
                return count;
            }
            if (maxItemsPerSession <= 0)
                maxItemsPerSession = 1;
            string path = GetFirstBatchFolder(fileOrderType);
                        
            Interlocked.Exchange(ref InProcess, 1);

            string[] messages = Assists.GetFiles(path,IsCoverable);
            if (messages == null)
            {
                return count;
            }
            maxItems = messages == null ? 0 : messages.Length;
            if (maxItems == 0)
            {
                DeleteFolder(path, true);
                Interlocked.Exchange(ref InProcess, 0);
                return count;
            }

            Netlog.DebugFormat("current batch items {0}, path: {1}", maxItems, m_host.RawHostAddress);

            //bool isDeleted = false;
            //int state = 0;//

            foreach (string message in messages)
            {
                count += DequeueFileAsync(message);

                //switch (operation)
                //{
                //    //case AdapterOperations.Sync:
                //    //    count += DequeueFileSync(message); break;
                //    case AdapterOperations.Recieve:
                //        count += DequeueFileAsync(message); break;
                //    case AdapterOperations.Transfer:
                //        count += DequeueFileTransfer(message); break;
                //}
                if (count > maxItemsPerSession)
                {
                    completed = true;
                    break;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }//end for

            Netlog.DebugFormat("current batch sent items: {0}, path: {1}", count, m_host.RawHostAddress);

            if (completed || count >= maxItems)
            {
                if (count < maxItemsPerSession)
                    DeleteFolder(path, maxItems == 0);
                Thread.Sleep(1000);
            }

            Interlocked.Exchange(ref InProcess, 0);

            return count;
        }

        public int DequeueFolderTransfer(FileOrderTypes fileOrderType, int maxItemsPerSession)
        {
            int count = 0;
            bool completed = false;
            int maxItems = 0;

            if (!CanQueue())
            {
                return count;
            }
            if (maxItemsPerSession <= 0)
                maxItemsPerSession = 1;
            string path = GetFirstBatchFolder(fileOrderType);

            Interlocked.Exchange(ref InProcess, 1);

            string[] messages = Assists.GetFiles(path, IsCoverable);
            if (messages == null)
            {
                return count;
            }
            maxItems = messages == null ? 0 : messages.Length;
            if (maxItems == 0)
            {
                DeleteFolder(path, true);
                Interlocked.Exchange(ref InProcess, 0);
                return count;
            }

            Netlog.DebugFormat("current batch items {0}, path: {1}", maxItems, m_host.RawHostAddress);

            //bool isDeleted = false;
            //int state = 0;//

            foreach (string message in messages)
            {
                count += DequeueFileTransfer(message);

                //switch (operation)
                //{
                //    //case AdapterOperations.Sync:
                //    //    count += DequeueFileSync(message); break;
                //    case AdapterOperations.Recieve:
                //        count += DequeueFileAsync(message); break;
                //    case AdapterOperations.Transfer:
                //        count += DequeueFileTransfer(message); break;
                //}
                if (count > maxItemsPerSession)
                {
                    completed = true;
                    break;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }//end for

            Netlog.DebugFormat("current batch sent items: {0}, path: {1}", count, m_host.RawHostAddress);

            if (completed || count >= maxItems)
            {
                if (count < maxItemsPerSession)
                    DeleteFolder(path, maxItems == 0);
                Thread.Sleep(1000);
            }

            Interlocked.Exchange(ref InProcess, 0);

            return count;
        }

        public QueueItem[] DequeueItems(FileOrderTypes fileOrderType, int maxItemsPerSession)
        {

            bool completed = false;
            int maxItems = 0;
            

            if (!CanQueue())
            {
                return null;
            }
            if (maxItemsPerSession <= 0)
                maxItemsPerSession = 1;

            string path = GetFirstBatchFolder(fileOrderType);

            Interlocked.Exchange(ref InProcess, 1);

            string[] messages = Assists.GetFiles(path, IsCoverable);
            if (messages == null)
            {
                return null;
            }

            List<QueueItem> list = new List<QueueItem>();

            maxItems = messages == null ? 0 : messages.Length;
            if (maxItems == 0)
            {
                DeleteFolder(path, true);
                Interlocked.Exchange(ref InProcess, 0);
                return null;
            }

            Netlog.DebugFormat("current batch items {0}, path: {1}", maxItems, m_host.RawHostAddress);


            foreach (string message in messages)
            {
                var item = DequeueFile(message);
                if (item != null)
                {
                    list.Add(item);
                }
                else
                {
                    break;
                }

                if (list.Count > maxItemsPerSession)
                {
                    completed = true;
                    break;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }//end for

            Netlog.DebugFormat("current batch sent items: {0}, path: {1}", list.Count, m_host.RawHostAddress);

            if (completed || list.Count >= maxItems)
            {
                if (list.Count < maxItemsPerSession)
                    DeleteFolder(path, maxItems == 0);
                Thread.Sleep(1000);
            }

            Interlocked.Exchange(ref InProcess, 0);

            return list.ToArray();
        }

        public QueueItem DequeueFirstItem(FileOrderTypes fileOrderType)
        {

            bool completed = false;
            int maxItems = 0;


            if (!CanQueue())
            {
                return null;
            }
           
            string path = GetFirstBatchFolder(fileOrderType);

            Interlocked.Exchange(ref InProcess, 1);

            string[] messages = Assists.GetFiles(path, IsCoverable);
            if (messages == null)
            {
                return null;
            }

            QueueItem item = null;

            maxItems = messages == null ? 0 : messages.Length;
            if (maxItems == 0)
            {
                DeleteFolder(path, true);
                Interlocked.Exchange(ref InProcess, 0);
                return null;
            }

            Netlog.DebugFormat("current batch items {0}, path: {1}", maxItems, m_host.RawHostAddress);


            foreach (string message in messages)
            {
                item = DequeueFile(message);
                if (item != null)
                {
                    completed = true;
                    break;
                }
             }//end for

            Netlog.DebugFormat("current batch sent items: {0}, path: {1}", 1, m_host.RawHostAddress);

            if (completed || 1 >= maxItems)
            {
                DeleteFolder(path, maxItems == 0);
                Thread.Sleep(1000);
            }

            Interlocked.Exchange(ref InProcess, 0);

            return item;
        }

/*
        protected virtual int DequeueFile(string filename)
        {
            int count = 0;

            bool isDeleted = false;
            int state = 0;//

            state = 0;
            isDeleted = false;
            QueueItem item = null;
            try
            {
                if (Thread.VolatileRead(ref InProcess) == 0)
                {
                    Netlog.DebugFormat("current batch not InProcess, path: {0}", m_host.HostAddress);
                    return count;
                }
                state = 1;
                //Netlog.DebugFormat("current message: {0}", message);
               
                item = QueueItem.ReadFile(filename);
                if (item == null)
                {
                    throw new Exception("item not found in QueueItem.ReadFile " + filename);
                }

                if (item.Retry >= 3)
                {
                    //File.Move(filename,newFile);
                    Directory.CreateDirectory(m_host.SuspendPath);
                    item.SaveToFile(m_host.SuspendPath);
                    File.Delete(filename);
                    isDeleted = true;
                }
                else
                {
                    item.DoRetry();
                    bool sentTo = false;

                    switch (OperationType)
                    {
                        case AdapterOperations.Sync:
                            CurrentItem = item;
                            //m_adapter.Message = item.GetMessage();
                            sentTo = true;
                            break;
                        case AdapterOperations.Async:
                            if (TargetAction != null)
                            {
                                TargetAction(item.GetMessage());
                                sentTo = true;
                            }
                            break;
                        case AdapterOperations.Transfer:
                            RemoteClient client = new RemoteClient(Destination);
                            client.SendAsync(item.GetMessage(), ConnectTimeout, AckAction);
                            CurrentItem = item;
                            sentTo = true;
                            break;
                    }
                    if (sentTo == false)
                    {
                        throw new Exception("Incorrect adapter properties");
                    }
                    else //if (sentTo)
                    {
                        if (IsTrans)
                        {
                            Directory.CreateDirectory(m_host.CoveredPath);
                            item.SaveToFile(m_host.CoveredPath);
                        }

                        File.Delete(filename);

                        count++;
                        isDeleted = true;
                        Netlog.DebugFormat("item deleted: " + filename);
                    }

                    //if (count > m_adapter.MaxItemsPerSession)
                    //{
                    //    completed = true;
                    //    return;
                    //}
                    //else
                    //    Thread.Sleep(100);
                }


                if (!isDeleted)
                {
                    state = 2;
                    Netlog.DebugFormat("save retry DequeueFolder item: " + filename);

                    File.Delete(filename);
                    item.SaveToFile(m_host.QueuePath);//retry again
                }

            }
            catch (IOException iox)
            {
                Netlog.ErrorFormat("error IO DequeueFolder item: " + iox.Message);
            }
            catch (Exception ex)
            {
                if (state <= 1)
                    Netlog.ErrorFormat("error DequeueFolder item: {0} trace:{1}", ex.Message, ex.StackTrace);
                else if (state == 2)
                    Netlog.ErrorFormat("save retry error DequeueFolder item: " + ex.Message);
                else
                    Netlog.ErrorFormat("Unexpected error DequeueFolder item: " + ex.Message);
            }

            return count;
        }

        protected virtual int DequeueFolder(string path)
        {
            int count = 0;
            bool completed = false;
            int maxItems = 0;

            if (Thread.VolatileRead(ref InProcess) == 1)
            {
                return count;
            }

            if (Directory.Exists(path))
            {
                Interlocked.Exchange(ref InProcess, 1);

                //Netlog.DebugFormat("current batch folder:{0}", path);
                string[] messages = Directory.GetFiles(path, "*" + FileExt);
                maxItems = messages == null ? 0 : messages.Length;
                if (maxItems == 0)
                {
                    DeleteFolder(path, true);
                    Interlocked.Exchange(ref InProcess, 0);
                    return count;
                }

                //ResetSender();

                Netlog.DebugFormat("current batch items {0}, path: {1}", maxItems, m_host.HostAddress);

                //bool isDeleted = false;
                //int state = 0;//

                foreach (string message in messages)
                {

                    count += DequeueFile(message);
                    if (count > MaxItemsPerSession)
                    {
                        completed = true;
                        break;
                    }
                    else
                        Thread.Sleep(100);
                }//end for

                Netlog.DebugFormat("current batch sent items: {0}, path: {1}", count, m_host.HostAddress);

                if (completed || count >= maxItems)
                {
                    if (count < MaxItemsPerSession)
                        DeleteFolder(path, maxItems == 0);
                    Thread.Sleep(1000);
                }
            }
            Interlocked.Exchange(ref InProcess, 0);

            return count;
        }
*/

        public bool DeleteInfoAndItem(string infofilename)
        {
            try
            {
                string itemfile = Assists.InfoToQueue(infofilename);
                string coverdinfofile = Assists.InfoToCovered(infofilename);
                File.Delete(infofilename);
                File.Delete(coverdinfofile);
                File.Delete(itemfile);
                Netlog.DebugFormat("info deleted:{0}, item deleted:{1}", infofilename, itemfile);
                return true;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("Error delete item: " + ex.Message);
                return false;
            }
        }

        public bool DeleteItem(string filename)
        {
            try
            {
                File.Delete(filename);
                Netlog.DebugFormat("item deleted: " + filename);
                return true;
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("Error delete item: " + ex.Message);
                return false;
            }
        }

        public void DeleteFolder(string path, bool checkInterval)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                if (di.Exists)
                {
                    if (checkInterval && DateTime.Now.Subtract(di.CreationTime).TotalSeconds > m_DeleteIntervalSeconds)
                    {
                        di.Delete();
                        Netlog.DebugFormat("current batch folder deleted :{0}", path);
                    }
                }
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("error DeleteFolder: " + ex.Message);
            }
        }
        #endregion

        #region operation

        public void ClearItems()
        {

            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        #endregion

        public static string GetFirstBatchFolder(FileOrderTypes fileOrderType, string queuePath)
        {
            string folder = null;
            try
            {

                DirectoryInfo di = new DirectoryInfo(queuePath);
                if (di.Exists)
                {
                    DirectoryInfo[] dirs = di.GetDirectories();
                    if (dirs != null && dirs.Length > 0)
                    {
                        DirectoryInfo firstDir = null;
                        if (fileOrderType == FileOrderTypes.ByCreation)
                            firstDir = dirs.OrderBy(f => f.CreationTime).FirstOrDefault();
                        else
                            firstDir = dirs.OrderBy(f => f.Name).FirstOrDefault();

                        DirectoryInfo dir = (DirectoryInfo)firstDir;
                        folder = dir.FullName;
                    }
                    else
                    {
                        return di.FullName;
                    }
                }
            }
            catch (IOException iox)
            {
                Netlog.ErrorFormat("error GetFirstBatchFolder : " + iox.Message);
            }
            return folder;

        }

        //public static QueueItem DequeueFile(string filename)
        //{
        //    QueueItem item = null;
        //    try
        //    {
        //        item = QueueItem.ReadFile(filename);

        //        if (item == null)
        //        {
        //            throw new MessageException(MessageState.RetryExceeds, "item not found in QueueItem.ReadFile " + filename);
        //        }

        //        if (item.Retry >= 3)
        //        {
        //            //File.Move(filename,newFile);
        //            Directory.CreateDirectory(m_host.SuspendPath);
        //            item.SaveToFile(m_host.SuspendPath);
        //            File.Delete(filename);
        //            throw new Exception("item retry exceeds " + filename);
        //        }
        //        else
        //        {
        //            item.DoRetry();

        //            //if (IsTrans)
        //            //{
        //            //    Directory.CreateDirectory(m_host.CoveredPath);
        //            //    item.SaveToFile(m_host.CoveredPath);
        //            //}

        //            File.Delete(filename);

        //            Netlog.DebugFormat("item deleted: " + filename);

        //            return item;

        //        }
        //    }
        //    catch (IOException iox)
        //    {
        //        Netlog.ErrorFormat("error IO DequeueFile item: " + iox.Message);
        //    }
        //    catch (MessageException mex)
        //    {
        //        Netlog.ErrorFormat("error DequeueFile MessageState:{0}, message:{1} ", mex.MessageState, mex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Netlog.ErrorFormat("Unexpected error DequeueFile item: {0}", ex.Message);
        //    }

        //    return item;
        //}

        //public static QueueItem[] DequeueFolder(string path, AdapterOperations operation, int maxItemsPerSession)
        //{
        //    int count = 0;
        //    bool completed = false;
        //    int maxItems = 0;

        //    if (!Directory.Exists(path))
        //    {
        //        return null;// throw new MessageException(MessageState.PathNotFound, "Folder not exists :" + path);
        //    }

        //    List<QueueItem> list = new List<QueueItem>();
            
        //    string[] messages = Directory.GetFiles(path, "*" + FileExt);

        //    maxItems = messages == null ? 0 : messages.Length;
        //    if (maxItems == 0)
        //    {
        //        return null;
        //    }

        //    Netlog.DebugFormat("current batch items {0}, path: {1}", maxItems, m_host.HostAddress);
            
        //    foreach (string message in messages)
        //    {
        //        var item= DequeueFile(message);
        //        if (item != null)
        //        {
        //            list.Add(item);
        //        }

        //        if (count > maxItemsPerSession)
        //        {
        //            completed = true;
        //            break;
        //        }
        //        else
        //        {
        //            Thread.Sleep(100);
        //        }
        //    }//end for

        //    Netlog.DebugFormat("current batch sent items: {0}, path: {1}", count, m_host.HostAddress);

        //    if (completed || count >= maxItems)
        //    {
        //        if (count < maxItemsPerSession)
        //            DeleteFolder(path, maxItems == 0);
        //        Thread.Sleep(1000);
        //    }

            
        //    return count;
        //}

        ///// <summary>
        ///// Dequeue message from queue, using sync methods.
        ///// </summary>
        ///// <returns></returns>
        //public static Message Dequeue(string queuePath, FileOrderTypes fileOrderType)
        //{

        //    try
        //    {
        //        CurrentItem = null;
        //        EnsureSync();


        //        //Thread.Sleep(1000);
        //        string folder = GetFirstBatchFolder(fileOrderType, queuePath);
        //        if (folder != null)
        //        {
        //            if (DequeueFolder(folder, OperationType, MaxItemsPerSession) > 0)
        //            {
        //                return CurrentItem.GetMessage();
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Netlog.ErrorFormat("error FolderQueue.Dequeue :{0}, Trace:{1} ", ex.Message, ex.StackTrace);
        //    }
        //    return null;
        //}


        
        public static ReadFileState DequeueFile(Ptr ptr, string rootPath, bool isCoverable, out IQueueItem item)
        {
            string filename = Assists.GetFilename(rootPath, ptr.Host, ptr.Identifier, isCoverable); //string.Format("{0}\\{1}", rootPath, ptr.Location);
            return DequeueFile(filename, isCoverable, out item);
        }

        
        public static ReadFileState DequeueFile(string filename, bool isCoverable, out IQueueItem item)
        {
            if (!File.Exists(filename))
            {
                item = null;
                return ReadFileState.NotExists;
            }

            try
            {

                NetStream memoryStream = new NetStream();
                using (Stream input = File.OpenRead(filename))
                {
                    input.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                QueueItem qitem = new QueueItem(memoryStream,null);// QueueItem.Create(memoryStream);

                if (isCoverable)
                {
                    //TODO: SET TARNS
                }
                else
                {
                    Task.Factory.StartNew(() => File.Delete(filename));
                }

                item = qitem as IQueueItem;
                return ReadFileState.Completed;
            }
            catch (IOException ioex)
            {
                Netlog.Exception("ReadFile IOException ", ioex);
                item = null;
                return ReadFileState.IOException;
            }
            catch (System.Transactions.TransactionException tex)
            {
                Netlog.Exception("ReadFile TransactionException ", tex);
                item = null;
                return ReadFileState.TransactionException;
            }
            catch (Exception ex)
            {
                Netlog.Exception("ReadFile Exception ", ex);
                item = null;
                return ReadFileState.Exception;
            }

            //FileStream fStream = File.OpenRead(filename);
            //return new QueueItem(fStream, null);

            //return Deserialize(File.ReadAllBytes(filename));

        }

        //public static string SaveFile(QueueItem itemstream, string rootPath,string location)
        //{
        //    string filename = FormatQueueFilename(rootPath,location);
        //    itemstream.BodyStream.SaveToFile(filename);
        //    return filename;
        //}

        //public static string SaveFile(QueueItem itemstream, string rootPath,Ptr ptr)
        //{
        //    string filename = FormatQueueFilename(rootPath,ptr.Location);
        //    itemstream.BodyStream.SaveToFile(filename);
        //    return filename;
        //}

        ///// <summary>
        ///// Get an instance of <see cref="QueueItem"/> from file.
        ///// </summary>
        ///// <param name="ptr"></param>
        ///// <returns></returns>
        //public static ReadFileState ReadFile(string rootPath,Ptr ptr, out IQueueItem item)
        //{
        //    string filename = FormatQueueFilename(rootPath,ptr.Location);
        //    return QueueItem.ReadFile(filename, out item);
        //}

        /// <summary>
        /// Get an instance of <see cref="QueueItem"/> from file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static ReadFileState DequeueFileWithScop(QueueItem itemstream, string filename, bool isTrans, out IQueueItem item)
        {
            if (!File.Exists(filename))
            {
                item = null;
                return ReadFileState.NotExists;
            }

            try
            {

                var scopeOptions = new TransactionOptions();
                scopeOptions.IsolationLevel = IsolationLevel.ReadCommitted;
                scopeOptions.Timeout = TimeSpan.FromSeconds(60);

                //Create the transaction scope
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, scopeOptions))
                {
                    //Transaction.Current.TransactionCompleted += new TransactionCompletedEventHandler(Current_TransactionCompleted);

                    NetStream memoryStream = new NetStream();
                    using (Stream input = File.OpenRead(filename))
                    {
                        input.CopyTo(memoryStream);
                    }
                    memoryStream.Position = 0;
                    QueueItem qitem = new QueueItem(memoryStream,null);// QueueItem.Create(memoryStream);

                    if (isTrans)
                    {
                        //TODO: SET TARNS
                    }
                    else
                    {
                        Task.Factory.StartNew(() => File.Delete(filename));
                    }

                    item = qitem as IQueueItem;
                    scope.Complete();
                }
                return ReadFileState.Completed;
            }
            catch (IOException ioex)
            {
                Netlog.Exception("ReadFile IOException ", ioex);
                item = null;
                return ReadFileState.IOException;
            }
            catch (System.Transactions.TransactionException tex)
            {
                Netlog.Exception("ReadFile TransactionException ", tex);
                item = null;
                return ReadFileState.TransactionException;
            }
            catch (Exception ex)
            {
                Netlog.Exception("ReadFile Exception ", ex);
                item = null;
                return ReadFileState.Exception;
            }

            //FileStream fStream = File.OpenRead(filename);
            //return new QueueItem(fStream, null);

            //return Deserialize(File.ReadAllBytes(filename));

        }

        public static string FormatQueueFilename(string rootPath, string ptrLocation)
        {
            return string.Format("{0}\\{1}", rootPath, ptrLocation);
        }
        public static string FormatQueuePath(string rootPath, string hostName)
        {
            return string.Format("{0}\\{1}", rootPath, hostName);
        }
        public static string FormatFilename(string identifier)
        {
            return string.Format("{0}{1}", identifier, Assists.FileExt);
        }
        public static string FormatFullFilename(string rootPath,string hostName,string identifier)
        {
            return string.Format("{0}\\{1}", Path.Combine(rootPath, hostName), FormatFilename(identifier));
        }
    }
}
