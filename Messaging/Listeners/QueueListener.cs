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
using Nistec.Threading;

namespace Nistec.Messaging.Listeners
{

    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection for client.
    /// </summary>
    public class QueueListener : SessionListener//ListenerHandler, IListenerHandler
    {

        protected QueueApi _api;

        #region ctor


        public QueueListener(QueueAdapter adapter)//, int interval)
            : base(adapter)//, interval)
        {
            _api = new QueueApi(adapter.Source);
            _api.ReadTimeout = adapter.ReadTimeout;
            //_Listener= new ListenerQ(this, adapter);
        }

        #endregion

 
        //protected override IQueueAck Send(QueueMessage message)
        //{
        //    return _api.PublishItem(message);
        //}


        protected override void ReceiveAsync(IDynamicWait dw)
        {
            QueueRequest request = new QueueRequest()
            {
                Host = _api.QueueName,
                QCommand = QueueCmd.Dequeue,
                DuplexType = DuplexTypes.WaitOne
            };

            //void OnNack()
            //{
            //    CalcDynamicWait(false);
            //}

            //void OnAck(bool ack)
            //{
            //    aw.DynamicWaitAck(ack);
            //}

            //if (EnableResetEvent)
            //    _api.DequeueAsync(request, ConnectTimeout, OnCompleted, OnAck, resetEvent);
            //else
             _api.DequeueAsync(request, ConnectTimeout, OnCompleted, dw);

            //_api.ReceiveAsync(
            //    OnFault,
            //    OnCompleted,
            //     DuplexTypes.WaitOne,
            //     resetEvent
            //    );

            //_api.SendDuplexAsync(message,
            //    (err) => OnErrorOcurred(new GenericEventArgs<string>(err)),
            //    (qitem) => OnMessageReceived(qitem));
        }


        protected override IQueueMessage Receive()
        {
            QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.Dequeue, null);
            {
                Host = _api.QueueName,
                QCommand = QueueCmd.Dequeue,
                DuplexType = DuplexTypes.WaitOne
            };
            return _api.Dequeue(request);
        }

        //protected override IQueueAck ReceiveTo()//QueueHost target, int connectTimeout, Action<QueueMessage> recieveAction)
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


    }
}
