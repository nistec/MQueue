using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Nistec.Generic;
using System.IO;
using Nistec.Channels;

namespace Nistec.Messaging.Listeners
{

    /// <summary>
    /// Represent an adapter properties for all kinds of queue adapters.
    /// </summary>
    public class QueueAdapter : IDisposable
    {
               
        #region properties
        /// <summary>
        /// Get or Set the source <see cref="QueueHost"/> host properties.
        /// </summary>
        public QueueHost Source { get; set; }
        ///// <summary>
        ///// Get or Set the destination <see cref="QueueHost"/> host properties.
        ///// </summary>
        //public QueueHost TransferTo { get; set; }
        ///// <summary>
        ///// Get or Set the <see cref="AdapterOperations"/> property.
        ///// </summary>
        //public AdapterOperations OperationType { get; set; }
        /// <summary>
        /// Get or Set the <see cref="FileOrderTypes"/> property.
        /// </summary>
        public FileOrderTypes FileOrderType { get; set; }
        /// <summary>
        /// Get or Set the <see cref="AdapterProtocols"/> property.
        /// </summary>
        public AdapterProtocols ProtocolType { get; set; }

        /// <summary>
        /// Get or Set indicating whether the adapter use async operation.
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// Get or Set indicating whether the adapter use transactional operation.
        /// </summary>
        public bool IsTrans { get; set; }
        /// <summary>
        /// Get or Set indicating whether the adapter use topic operation.
        /// </summary>
        public bool IsTopic { get; set; }
        /// <summary>
        /// Get or Set TargetPath.
        /// </summary>
        public string TargetPath { get; set; }
        /// <summary>
        /// Get or Set the maximum number of items to fetch for each session, default is 1.
        /// </summary>
        public int MaxItemsPerSession { get; set; }

        public int Interval { get; set; }

        /// <summary>
        /// Get or Set the delegate of target methods.
        /// </summary>
        public Action<IQueueMessage> MessageReceivedAction { get; set; }
        /// <summary>
        /// Get or Set the delegate of acknowledgment methods.
        /// </summary>
        public Action<IQueueAck> MessageAckAction { get; set; }
        public Action<string> MessageFaultAction { get; set; }
        int _WorkerCount;
        /// <summary>
        /// Gets or Set the number of worker count.
        /// </summary>
        public int WorkerCount
        {
            get { return _WorkerCount; }
            set
            {
                if (value > 0)
                {
                    _WorkerCount = value;
                }
            }
        }
        int _MaxConnection;
        /// <summary>
        /// Gets or Set the number of worker count.
        /// </summary>
        public int MaxConnection
        {
            get { return _MaxConnection; }
            set
            {
                if (value > 0)
                {
                    _MaxConnection = value;
                }
            }
        }
        public bool IsMultiTask { get; set; }
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

        /// <summary>
        /// Get or Set the read tomeout in milliseconds.
        /// </summary>
        public int ReadTimeout { get; set; }
        public bool EnableResetEvent { get; set; }
        public bool EnableDynamicWait { get; set; }
        #endregion

        #region ctor
        /// <summary>
        /// Initialize a new instance of <see cref="QueueAdapter"/>.
        /// </summary>
        public QueueAdapter()
        {
            //OperationType = AdapterOperations.Recieve;
            FileOrderType = FileOrderTypes.ByCreation;
            ProtocolType = AdapterProtocols.NamedPipe;
            IsTrans = false;
            IsTopic = false;
            IsMultiTask = true;
            MaxItemsPerSession = 1;
            Interval = 1000;// interval<=0? DefaultInterval:interval;// 1000;
            ConnectTimeout = Defaults.ConnectTimeout;
            ReadTimeout = Defaults.ReadTimeout;
            WorkerCount = 1;
            MaxConnection = 9999;
            IsAsync = false;
            EnableResetEvent = false;
            EnableDynamicWait = false;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="QueueAdapter"/>.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="serverName"></param>
        public QueueAdapter(QueueHost host) : this()
        {
            Source = host;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="QueueAdapter"/>.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="serverName"></param>
        public QueueAdapter(string queueName, string serverName) : this()
        {
            Source = QueueHost.ParseLocal(queueName, serverName);
        }

        /// <summary>
        /// Initialize a new instance of <see cref="QueueAdapter"/>.
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="serverName"></param>
        /// <param name="hostPort"></param>
        /// <param name="hostName"></param>
        public QueueAdapter(HostProtocol protocol, string serverName, int hostPort, string hostName) : this()
        {
            Source = new QueueHost(protocol, serverName, hostPort.ToString(), hostName);
        }
        public QueueAdapter(string serverName, string pipeName, string hostName) : this()
        {
            Source = new QueueHost(HostProtocol.ipc, serverName, pipeName, hostName);
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

                if (Source != null)
                {
                    Source.Dispose();
                    Source = null;
                }
                //if (TransferTo != null)
                //{
                //    TransferTo.Dispose();
                //    TransferTo = null;
                //}
                //if (Message != null)
                //{
                //    Message.Dispose();
                //    Message = null;
                //}
                if (MessageReceivedAction != null)
                {
                    MessageReceivedAction = null;
                }
                if (MessageFaultAction != null)
                {
                    MessageFaultAction = null;
                }
            }
            disposed = true;
        }
        #endregion

        #region methods

        /// <summary>
        /// Return the host name of current adapter.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Host: {0}", Source == null ? "" : Source.HostName);
        }

        /// <summary>
        /// Ensure that adapter properties.
        /// </summary>
        public void EnsureProperties()
        {
            if (Source == null)
            {
                throw new ArgumentException("Invalid Adapter Source");
            }
            Source.EnsureHost();

            switch (ProtocolType)
            {
                case AdapterProtocols.Http:
                    break;
                case AdapterProtocols.NamedPipe:
                    break;
                case AdapterProtocols.Tcp:
                    if (Source.Port == 0)
                    {
                        throw new ArgumentException("Invalid Adapter Source.Port");
                    }
                    break;
                case AdapterProtocols.Db:
                    break;
                case AdapterProtocols.File:
                    //if (string.IsNullOrEmpty(RootPath))
                    //{
                    //    throw new ArgumentException("Invalid Adapter RootPath");
                    //}
                    break;
            }
        }

        /// <summary>
        /// Ensure that adapter use the correct method.
        /// </summary>
        public void EnsureOperations()
        {
            EnsureRecieve();


            //switch (OperationType)
            //{
            //    //case AdapterOperations.Sync:
            //    //    EnsureSync();break;
            //    case AdapterOperations.Recieve:
            //        EnsureRecieve(); break;
            //    case AdapterOperations.Transfer:
            //        EnsureTransfer(); break;
            //}
        }

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
            if (MessageReceivedAction == null)
            {
                throw new ArgumentException("Invalid QueueAction Adapter");
            }
            if (MaxItemsPerSession <= 0)
            {
                MaxItemsPerSession = 1;
            }
        }
        ///// <summary>
        ///// Ensure that adapter use transfer method and the destination host and target action is defined.
        ///// </summary>
        ///// <exception cref="ArgumentException"></exception>
        //public void EnsureTransfer()
        //{
        //    if (OperationType != AdapterOperations.Transfer)
        //    {
        //        throw new ArgumentException("Incorrect OperationType, it is not an transfer type");
        //    }
        //    if (QueueAction == null)
        //    {
        //        throw new ArgumentException("Invalid QueueAction Adapter");
        //    }
        //    if (TransferTo == null)
        //    {
        //        throw new ArgumentException("Invalid Destination Adapter");
        //    }

        //    if (MaxItemsPerSession <= 0)
        //    {
        //        MaxItemsPerSession = 1;
        //    }
        //}

        #endregion

        ///// <summary>
        ///// Get the adapter by <see cref="AdapterProtocols"/> protocol.
        ///// </summary>
        ///// <returns></returns>
        //public AdapterBase GetAdapter()
        //{
        //    switch (ProtocolType)
        //    {
        //        case AdapterProtocols.Http:
        //            return new HttpAdapter(this);
        //        case AdapterProtocols.NamedPipe:
        //            return new IpcAdapter(this);
        //        case AdapterProtocols.Tcp:
        //            return new TcpAdapter(this);
        //        //case AdapterProtocols.Db:
        //        //    return new DbAdapter(this);
        //        //case AdapterProtocols.File:
        //        //    return new FileAdapter(this);
        //    }

        //    return null;
        //}

        /// <summary>
        /// Get the <see cref="AdapterProtocols"/> adapter protocol by <see cref="HostProtocol"/> address type.
        /// </summary>
        /// <param name="addressType"></param>
        /// <returns></returns>
        public static AdapterProtocols GetProtocol(HostProtocol addressType)
        {
            switch (addressType)
            {
                case HostProtocol.tcp:
                    return AdapterProtocols.Tcp;
                case HostProtocol.http:
                    return AdapterProtocols.Http;
                case HostProtocol.ipc:
                    return AdapterProtocols.NamedPipe;
                case HostProtocol.db:
                case HostProtocol.file:
                default:
                    return AdapterProtocols.NamedPipe;
            }
        }
    }
}