using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Generic;
using System.Collections.ObjectModel;
using Nistec.Messaging.Remote;
using Nistec.Runtime;
using Nistec.Channels;
using Nistec.Messaging.Channels;
using Nistec.Channels.Tcp;
using Nistec.Channels.Http;
using Nistec.Data.Entities;
using Nistec.Logging;
using Nistec.Threading;

namespace Nistec.Messaging.Listeners
{

    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection for client.
    /// </summary>
    public class TopicSbscriberListener : IControllerHandler, IListener
    {

        IChannelService _ChannelService;

        IPriorityQueue Queue;
        ILogger _Logger;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }
        public bool EnablePersistQueue { get; private set; }

        public Func<IQueueItem, TransStream> OnItemReceived { get; set; }
        public Action<string> OnError { get; set; }
        public string SbscriberHost { get; private set; }
        public HostProtocol HostProtocol { get; private set; }
        public ListenerState State { get; private set; }
        public string HostInfo { get; private set; }
        public string ChannelInfo { get; private set; }
        #region ctor

        public TopicSbscriberListener(QueueHost qhost, bool enablePersistQueue = false, int consumeInterval = 100)
        {
            SbscriberHost = qhost.HostName;
            HostProtocol = qhost.Protocol;
            HostInfo = string.Format("HostName:{0},Protocol:{1}, HostAddress:{2}", qhost.HostName, qhost.Protocol, qhost.HostAddress);

            EnablePersistQueue = enablePersistQueue;
            if (enablePersistQueue)
            {
                Queue = new PriorityFsQueue(qhost, consumeInterval);
            }
            else
            {
                Queue = new PriorityMemQueue(qhost.HostName, consumeInterval);
            }
            //if (autoStart)
            //{
            //    InitServerQueue(settings, autoStart);
            //}
        }

        public void InitServerQueue(IChannelSettings settings, bool autoStart = false)
        {
            if (_ChannelService != null)
                return;

            ChannelInfo = settings.RawHostAddress;

            switch (settings.Protocol)
            {
                case NetProtocol.Http:
                    _ChannelService = new HttpServerQueue((HttpSettings)settings, this);
                    break;
                case NetProtocol.Pipe:
                    _ChannelService = new PipeServerQueue((PipeSettings)settings, this);
                    break;
                case NetProtocol.Tcp:
                    _ChannelService = new TcpServerQueue((TcpSettings)settings, this);
                    break;
                default:
                    throw new InvalidOperationException("Protocol not supported");
            }
            if (_Logger == null)
                _Logger = new LoggerConsole(true);

            if (_Logger != null)
            {
                _ChannelService.Log = _Logger;
                _Logger.Info("TopicSbscriberListener Initilaized protocol:" + settings.Protocol.ToString());
                _Logger.Info("QueueHost Info - " + HostInfo);
                _Logger.Info("Channel Info: " + ChannelInfo);
            }

            if (autoStart)
            {
                Start();
            }
        }

        //public void InitHttpServerQueue(HttpSettings settings) {

        //    _ChannelService = new HttpServerQueue(settings, this);
        //}

        //public void InitPipeServerQueue(PipeSettings settings)
        //{
        //    _ChannelService = new PipeServerQueue(settings, this);

        //}
        //public void InitTcpServerQueue(TcpSettings settings)
        //{
        //    _ChannelService = new TcpServerQueue(settings, this);
        //}
        #endregion

        #region DynamicWorker
        DynamicWorker ActionWorker;

        public void StartDynamicWorker() {


            if (EnablePersistQueue)
            {
                if (ActionWorker != null)
                    return;

                ActionWorker = new DynamicWorker(DynamicWaitType.DynamicWait)
                {
                    ActionLog = (LogLevel level,string message) =>
                    {
                        if (_Logger != null)
                            _Logger.Log((LoggerLevel)level, message);
                    },
                    ActionTask = () =>
                    {
                        try
                        {
                            if (OnItemReceived != null)
                            {
                                IQueueItem item;
                                if (TryDequeue(out item))
                                {
                                    OnItemReceived(item);
                                    return true;
                                }
                                //var item = Dequeue();
                                //if (item != null)
                                //{
                                //    OnItemReceived(item);
                                //    return true;
                                //}
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_Logger != null)
                                _Logger.Exception("Topic Sender Worker error ", ex);
                        }
                        return false;
                    },
                    Interval = 100,
                    MaxThreads = 1
                };
                ActionWorker.Start();
            }
        }

        public void Start()
        {
            if (_ChannelService == null)
            {
                throw new InvalidOperationException("Invalid ChannelService");
            }
            _ChannelService.Start();

            StartDynamicWorker();

            if (_Logger != null)
                _Logger.Info("TopicSbscriberListener Started:" + SbscriberHost);
        }

        public void Stop()
        {
            if (_ChannelService == null)
            {
                throw new InvalidOperationException("Invalid ChannelService");
            }
            _ChannelService.Stop();

            if (ActionWorker != null)
                ActionWorker.Stop();
            if (_Logger != null)
                _Logger.Info("TopicSbscriberListener Stopted: " + SbscriberHost);
        }

        public void PauseChannelService()
        {
            _ChannelService.Pause();
            if (_Logger != null)
                _Logger.Info("TopicSbscriberListener Paused: " + SbscriberHost);
        }

        public bool Pause(OnOffState onff)
        {
            if (EnablePersistQueue)
            {
                if (_Logger != null)
                {
                    _Logger.Info("TopicSbscriberListener PausePersistQueue: " + SbscriberHost);
                }
                return ActionWorker.Pause(onff);
            }
            return false;
        }

        public void Shutdown(bool waitForWorkers)
        {

            if (ActionWorker != null)
                ActionWorker.Shutdown(waitForWorkers);
            if (Logger != null)
                Logger.Info("TopicSbscriberListener Shutdown!");
        }
        #endregion

        #region override

        public virtual TransStream OnMessageReceived(IQueueItem message)
        {
            Enqueue(message);

            //if (EnablePersistQueue)
            //    return Enqueue(message).ToTransStream();
            //if (OnItemReceived != null)
            //    return OnItemReceived(message);

            return new QueueAck(Nistec.Messaging.MessageState.Received, message).ToTransStream();
        }

        public virtual void OnErrorOcurred(string message) {

            if (_Logger != null)
                _Logger.Error("TopicSbscriberListener Error: " + message);
        }

        #endregion

        #region persist queue
        protected void Enqueue(IQueueItem item)
        {
            Queue.Enqueue(item);
        }
        protected bool TryDequeue(out IQueueItem item)
        {
            return Queue.TryDequeue(out item);
        }
        #endregion

        #region persist queue

        //protected IQueueAck Enqueue(IQueueItem item)
        //{
        //    return Queue.Enqueue(item);
        //}

        protected IQueueItem Peek()
        {
            return Queue.Peek();
        }

        protected IQueueItem Dequeue()
        {
            return Queue.Dequeue();
        }

        //protected IQueueItem GetFirstItem()
        //{
        //    return Queue.GetFirstItem();
        //}

        public IEnumerable<IPersistEntity> QueryItems()
        {
            return Queue.QueryItems();
        }

        public void ClearItems()
        {
            Queue.Clear();
        }

        public void ReloadItems()
        {
            // QueueList.Clear();
        }
        public int Count()
        {
            return Queue.TotalCount;
        }


        #endregion

    }

    public abstract class TopicSbscriberListener<T> : IControllerHandler<T>, IListener where T : IHostMessage
    {

        IChannelService _ChannelService;

        //IPriorityQueue Queue;
        GenericQueue<T> Queue;
        ILogger _Logger;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger Logger { get { return _Logger; } set { if (value != null) _Logger = value; } }
        public bool EnablePersistQueue { get; private set; }

        public Func<T, TransStream> OnItemReceived { get; set; }
        public Action<string> OnError { get; set; }
        public string SbscriberHost { get; private set; }
        public HostProtocol HostProtocol { get; private set; }
        public ListenerState State { get; private set; }
        public string HostInfo { get; private set; }
        public string ChannelInfo { get; private set; }
        #region ctor

        public TopicSbscriberListener(QueueHost qhost, bool enablePersistQueue = false, int consumeInterval = 100)
        {
            SbscriberHost = qhost.HostName;
            HostProtocol = qhost.Protocol;
            HostInfo = string.Format("HostName:{0},Protocol:{1}, HostAddress:{2}", qhost.HostName, qhost.Protocol, qhost.HostAddress);

            EnablePersistQueue = enablePersistQueue;

            Queue = new GenericQueue<T>(qhost.HostName);

            //if (enablePersistQueue)
            //{
            //    Queue = new PriorityFsQueue(qhost, consumeInterval);
            //}
            //else
            //{
            //    Queue = new PriorityMemQueue(qhost.HostName, consumeInterval);
            //}
            //if (autoStart)
            //{
            //    InitServerQueue(settings, autoStart);
            //}
        }

        public void InitServerQueue(IChannelSettings settings, bool autoStart = false)
        {
            if (_ChannelService != null)
                return;

            ChannelInfo = settings.RawHostAddress;

            switch (settings.Protocol)
            {
                case NetProtocol.Http:
                    _ChannelService = new HttpServerGeneric<T>((HttpSettings)settings, this);
                    break;
                case NetProtocol.Pipe:
                    _ChannelService = new PipeServerGeneric<T>((PipeSettings)settings, this);
                    break;
                case NetProtocol.Tcp:
                    _ChannelService = new TcpServerGeneric<T>((TcpSettings)settings, this);
                    break;
                default:
                    throw new InvalidOperationException("Protocol not supported");
            }
            if (_Logger == null)
                _Logger = new LoggerConsole(true);

            if (_Logger != null)
            {
                _ChannelService.Log = _Logger;
                _Logger.Info("TopicSbscriberListener Initilaized protocol:" + settings.Protocol.ToString());
                _Logger.Info("QueueHost Info - " + HostInfo);
                _Logger.Info("Channel Info: " + ChannelInfo);
            }

            if (autoStart)
            {
                Start();
            }
        }

        //public void InitHttpServerQueue(HttpSettings settings) {

        //    _ChannelService = new HttpServerQueue(settings, this);
        //}

        //public void InitPipeServerQueue(PipeSettings settings)
        //{
        //    _ChannelService = new PipeServerQueue(settings, this);

        //}
        //public void InitTcpServerQueue(TcpSettings settings)
        //{
        //    _ChannelService = new TcpServerQueue(settings, this);
        //}
        #endregion

        #region DynamicWorker
        DynamicWorker ActionWorker;

        public void StartDynamicWorker()
        {


            if (EnablePersistQueue)
            {
                if (ActionWorker != null)
                    return;

                ActionWorker = new DynamicWorker(DynamicWaitType.DynamicWait)
                {
                    ActionLog = (LogLevel level, string message) =>
                    {
                        if (_Logger != null)
                            _Logger.Log((LoggerLevel)level, message);
                    },
                    ActionTask = () =>
                    {
                        try
                        {
                            if (OnItemReceived != null)
                            {
                                T item;
                                if (TryDequeue(out item))
                                {
                                    OnItemReceived(item);
                                    return true;
                                }
                                //var item = Dequeue();
                                //if (item != null)
                                //{
                                //    OnItemReceived(item);
                                //    return true;
                                //}
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_Logger != null)
                                _Logger.Exception("Topic Sender Worker error ", ex);
                        }
                        return false;
                    },
                    Interval = 100,
                    MaxThreads = 1
                };
                ActionWorker.Start();
            }
        }

        public void Start()
        {
            if (_ChannelService == null)
            {
                throw new InvalidOperationException("Invalid ChannelService");
            }
            _ChannelService.Start();

            StartDynamicWorker();

            if (_Logger != null)
                _Logger.Info("TopicSbscriberListener Started:" + SbscriberHost);
        }

        public void Stop()
        {
            if (_ChannelService == null)
            {
                throw new InvalidOperationException("Invalid ChannelService");
            }
            _ChannelService.Stop();

            if (ActionWorker != null)
                ActionWorker.Stop();
            if (_Logger != null)
                _Logger.Info("TopicSbscriberListener Stopted: " + SbscriberHost);
        }

        public void PauseChannelService()
        {
            _ChannelService.Pause();
            if (_Logger != null)
                _Logger.Info("TopicSbscriberListener Paused: " + SbscriberHost);
        }

        public bool Pause(OnOffState onff)
        {
            if (EnablePersistQueue)
            {
                if (_Logger != null)
                {
                    _Logger.Info("TopicSbscriberListener PausePersistQueue: " + SbscriberHost);
                }
                return ActionWorker.Pause(onff);
            }
            return false;
        }

        public void Shutdown(bool waitForWorkers)
        {

            if (ActionWorker != null)
                ActionWorker.Shutdown(waitForWorkers);
            if (Logger != null)
                Logger.Info("TopicSbscriberListener Shutdown!");
        }
        #endregion

        #region override

        public virtual TransStream OnMessageReceived(T message)
        {
            Enqueue(message);

            //if (EnablePersistQueue)
            //    return Enqueue(message).ToTransStream();
            //else

            //if (OnItemReceived != null)
            //    return OnItemReceived(message);

            //return new TransStream(message, TransType.Object);

            return new QueueAck(MessageState.Received, message.Identifier, message.Host).ToTransStream();
        }

        public virtual void OnErrorOcurred(string message)
        {

            if (_Logger != null)
                _Logger.Error("TopicSbscriberListener Error: " + message);
        }

        #endregion

        #region persist queue



        //protected virtual IQueueAck Enqueue(T message)
        //{
        //    Queue.Enqueue(message);
        //    return new QueueAck(MessageState.Received, message.Identifier, message.Host);

        //}

        protected virtual void EnqueuePersist(T item)
        {

        }
        protected virtual void DequeuePersist(T item)
        {

        }

        protected virtual void Enqueue(T item)
        {
            Queue.Enqueue(item);
            if (EnablePersistQueue)
            {
                EnqueuePersist(item);
            }
            // return new QueueAck(MessageState.Received, message.Identifier, message.Host);
        }

        protected T Peek()
        {
            return Queue.Peek();
        }

        protected bool TryDequeue(out T item)
        {
            if (Queue.TryDequeue(out item))
            {
                DequeuePersist(item);
                return true;
            }
            return false;

        }

        protected T Dequeue()
        {
            T item;
            if(Queue.TryDequeue(out item))
            {
                DequeuePersist(item);
            }
            return item;
        }

        //protected IQueueItem GetFirstItem()
        //{
        //    return Queue.GetFirstItem();
        //}

        public IEnumerable<T> QueryItems()
        {
            return Queue.ToList();
        }

        public void ClearItems()
        {
            Queue.Clear();
        }

        public void ReloadItems()
        {
            // QueueList.Clear();
        }
        public int Count()
        {
            return Queue.Count;
        }


        #endregion

        #region persist queue
        /*
        protected abstract IQueueAck Enqueue(T item);
       

        public abstract T Peek();
       

        public abstract T Dequeue();
       

        //protected IQueueItem GetFirstItem()
        //{
        //    return Queue.GetFirstItem();
        //}

        public virtual IEnumerable<T> QueryItems()
        {
            return null;// Queue.QueryItems();
        }

        public virtual void ClearItems()
        {
            //Queue.Clear();
        }

        public void ReloadItems()
        {
            // QueueList.Clear();
        }
        public int Count()
        {
            return 0;// Queue.TotalCount;
        }
        */

        #endregion

    }
}
