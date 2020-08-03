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

namespace Nistec.Messaging.Listeners
{

    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection for client.
    /// </summary>
    public class TopicListener : SessionListener, IControllerHandler//ListenerHandler, IListenerHandler
    {
        protected QueueApi _api;

        //PriorityPersistQueue

        IChannelService _ChannelService;

        #region ctor


        //public TopicListener(AdapterProperties[] queues)
        //    : base(queues)
        //{

        //}

        public TopicListener(QueueAdapter adapter,int interval)
            : base(adapter,interval)
        {
            _api = new QueueApi(adapter.Source);
            //_Listener= new ListenerQ(this, adapter);
        }


        //public TopicListener(string queueName, string serverName = ".")
        //    : base(queueName, serverName)
        //{

        //}

        public void InitHttpServerQueue(HttpSettings settings) {

            _ChannelService = new HttpServerQueue(settings, this);
        }

        public void InitPipeServerQueue(PipeSettings settings)
        {
            _ChannelService = new PipeServerQueue(settings, this);

        }
        public void InitTcpServerQueue(TcpSettings settings)
        {
            _ChannelService = new TcpServerQueue(settings, this);
        }
        #endregion

        #region override

        public virtual TransStream OnMessageReceived(IQueueMessage message) {

            return null;
        }

        public virtual void OnErrorOcurred(string message) {


        }

        //protected override Listener CreateListener(AdapterProperties lp)
        //{
        //    return new ListenerQ(this, lp);
        //}

        //public override Listener Find(string hostName)
        //{
        //    if (hostName == null)
        //    {
        //        throw new ArgumentNullException("Find.hostName");
        //    }
        //    return Listeners.Where(q => q.Source.HostName == hostName).FirstOrDefault<Listener>();
        //}

        //public void Abort(Ptr ptr)
        //{
        //    var listener = EnsureListener(_adapter.Source);
        //    listener.Abort(ptr);
        //}

        //public void Commit(Ptr ptr)
        //{
        //    var listener = EnsureListener(_adapter.Source);
        //    listener.Commit(ptr);
        //}



        #endregion

        //bool _isalive;
        //void TaskWorker()
        //{
        //    _isalive = true;
        //    // Start queue listener...
        //    Console.WriteLine("QListener started...");

        //    while (_isalive)
        //    {
        //        try
        //        {
        //            if (Interlocked.Read(ref delay) > 0)
        //            {
        //                Thread.Sleep((int)delay);
        //                Interlocked.Exchange(ref delay, 0);
        //            }

        //            Monitor.Enter(_locker, ref lockWasTaken);

        //            switch (_AdapterOperation)
        //            {
        //                case AdapterOperations.Transfer:
        //                    Task.Factory.StartNew(() => ReceiveTo());
        //                    break;
        //                case AdapterOperations.Async:
        //                    Task.Factory.StartNew(() => ReceiveAsync());
        //                    break;
        //                case AdapterOperations.Sync:
        //                    Message item = Receive();
        //                    if (item != null)
        //                    {
        //                        if (_Action != null)
        //                        {
        //                            Task.Factory.StartNew(() => _Action(item));
        //                        }
        //                        else
        //                            Task.Factory.StartNew(() => DoMessageReceived(item));
        //                    }
        //                    break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("QListener error: " + ex.Message);
        //            Task.Factory.StartNew(() => DoErrorOcurred(ex.Message));
        //        }
        //        finally
        //        {
        //            if (lockWasTaken) Monitor.Exit(_locker);
        //        }
        //        Thread.Sleep(_Interval);
        //    }

        //    Console.WriteLine("QListener stoped...");
        //}

        //public class ListenerQ : Listener
        //{
        //    AdapterBase _Adapter;

        //    internal ListenerQ(IListenerHandler owner, AdapterProperties properties)
        //        : base(owner, properties)
        //    {
        //        _Adapter = properties.GetAdapter();
        //        _Adapter.ConnectTimeout = properties.ConnectTimeout;
        //    }

        protected override IQueueAck Send(QueueItem message)
        {
            return _api.Send(message);
        }

        protected override IQueueItem Receive()
        {
            return _api.Receive();
        }

        protected override void ReceiveAsync(AutoResetEvent resetEvent)//, Action<string> onFault, Action<QueueItem> onCompleted)
        {

            _api.ReceiveAsync(
                (err) => OnErrorOcurred(new GenericEventArgs<string>(err)),
                (qitem) => OnMessageReceived(qitem),
                 DuplexTypes.WaitOne,
                 resetEvent
                );

            //_api.SendDuplexAsync(message, 
            //    (err)=> OnErrorOcurred(new GenericEventArgs<string>(err)), 
            //    (qitem)=> OnMessageReceived(qitem));
        }

        //protected override void ReceiveAsync(int connectTimeout, Action<Message> action)
        //{
        //    _api.ReceiveAsync(connectTimeout, action);
        //}

        //protected override IQueueAck ReceiveTo()//QueueHost target, int connectTimeout, Action<Message> recieveAction)
        //{
        //    return _api.ReceiveTo(TransferTo, ConnectTimeout, null);
        //}

        public override void Abort(Ptr ptr)
        {
            _api.Abort(ptr);
        }

        public override void Commit(Ptr ptr)
        {
            _api.Commit(ptr);
        }

        //}

    }
}
