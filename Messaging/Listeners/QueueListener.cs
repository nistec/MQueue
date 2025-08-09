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
using System.Security;

namespace Nistec.Messaging.Listeners
{
    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection for client.
    /// </summary>
    public class QueueListener : SessionListener//ListenerHandler, IListenerHandler
    {

        protected QueueApi QApi { get; private set; }

        #region ctor


        public QueueListener(QueueAdapter adapter)//, int interval)
            : base(adapter)//, interval)
        {
            QApi = new QueueApi(adapter.Source);
            QApi.ReadTimeout = adapter.ReadTimeout;
            //_Listener= new ListenerQ(this, adapter);
        }

        #endregion

        //protected override void OnMessageReceived(IQueueMessage message)
        //{
           
        //}

        protected override IQueueMessage Receive()
        {
            return QApi.Consume(60000);// int.MaxValue);
        }

        //protected override IQueueAck Send(QueueMessage message)
        //{
        //    return _api.PublishItem(message);
        //}

        /*
        protected override void Receive(IDynamicWait dw)
        {
            OnEvent("QueueListener.Receive", string.Format("SessionListener IDynamicWait QueueName: {0}", QApi.QueueName));
            QApi.Consume(int.MaxValue, OnDynamicWorkerCompleted);
            
            //QueueRequest request = new QueueRequest()
            //{
            //    Host = QApi.QueueName,
            //    Command = QueueCmd.Dequeue.ToString(),
            //    DuplexType = DuplexTypes.Respond
            //};

            ////void OnNack()
            ////{
            ////    CalcDynamicWait(false);
            ////}

            ////void OnAck(bool ack)
            ////{
            ////    aw.DynamicWaitAck(ack);
            ////}

            ////if (EnableResetEvent)
            ////    _api.DequeueAsync(request, ConnectTimeout, OnCompleted, OnAck, resetEvent);
            ////else
            //QApi.Dequeue(request, ConnectTimeout, OnDynamicWorkerCompleted, dw);
            
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

        protected override async Task ReceiveAsync(IDynamicWait dw)
        {
            OnEvent("QueueListener.ReceiveAsync", string.Format("SessionListener QueueName: {0}", QApi.QueueName));
            await QApi.ConsumeAsync(int.MaxValue, OnDynamicWorkerCompleted);
            
            //QueueRequest request = new QueueRequest()
            //{
            //    Host = QApi.QueueName,
            //    Command = QueueCmd.Dequeue.ToString(),
            //    DuplexType = DuplexTypes.Respond
            //};

            ////void OnNack()
            ////{
            ////    CalcDynamicWait(false);
            ////}

            ////void OnAck(bool ack)
            ////{
            ////    aw.DynamicWaitAck(ack);
            ////}

            ////if (EnableResetEvent)
            ////    _api.DequeueAsync(request, ConnectTimeout, OnCompleted, OnAck, resetEvent);
            ////else
            //await QApi.DequeueAsync(request, ConnectTimeout, OnDynamicWorkerCompleted, dw);
            
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
            OnEvent("QueueListener.ReceiveAsync", string.Format("SessionListener IQueueMessage QueueName: {0}", QApi.QueueName));

            QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.Dequeue, null);
            {
                Host = QApi.QueueName,
                Command = QueueCmd.Dequeue.ToString(),
                DuplexType = DuplexTypes.Respond
            };
            return QApi.Dequeue(request);
        }
        */
        //protected override IQueueAck ReceiveTo()//QueueHost target, int connectTimeout, Action<QueueMessage> recieveAction)
        //{
        //    return _api.ReceiveTo(TransferTo, ConnectTimeout, null);
        //}

        public override void Abort(Ptr ptr)
        {
            QApi.Abort(ptr);
        }

        public override void Commit(Ptr ptr)
        {
            QApi.Commit(ptr);
        }


    }
}
