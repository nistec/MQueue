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

namespace Nistec.Messaging.Listeners
{

    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection for client.
    /// </summary>
    public class QueueListener : SessionListener//ListenerHandler, IListenerHandler
    {

        protected QueueApi _api;

        #region ctor


        //public TopicListener(AdapterProperties[] queues)
        //    : base(queues)
        //{

        //}

        public QueueListener(QueueAdapter adapter, int interval)
            : base(adapter, interval)
        {
            _api = new QueueApi(adapter.Source);
            //_Listener= new ListenerQ(this, adapter);
        }

        //public TopicListener(string queueName, string serverName = ".")
        //    : base(queueName, serverName)
        //{

        //}

        #endregion

        #region override

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


        protected override IQueueAck Send(QueueItem message)
        {
            return _api.Send(message);
        }


        protected override void ReceiveAsync(AutoResetEvent resetEvent)//, Action<string> onFault, Action<QueueItem> onCompleted)
        {

            _api.ReceiveAsync(
                OnFault,
                OnCompleted,
                 DuplexTypes.WaitOne,
                 resetEvent
                );

            //_api.SendDuplexAsync(message, 
            //    (err)=> OnErrorOcurred(new GenericEventArgs<string>(err)), 
            //    (qitem)=> OnMessageReceived(qitem));
        }


        protected override IQueueItem Receive()
        {
            return _api.Receive(DuplexTypes.WaitOne);
        }
        //protected override IQueueAck ReceiveTo()//QueueHost target, int connectTimeout, Action<QueueItem> recieveAction)
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
