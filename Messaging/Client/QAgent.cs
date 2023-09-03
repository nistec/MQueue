using Nistec.Messaging.Listeners;
using Nistec.Messaging.Remote;
using Nistec.Runtime;
using Nistec.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nistec.Messaging.Client
{
    public abstract class QAgent: SessionListener
    {
        protected QueueApi QApi { get; private set; }

        //string SourceName;
        //DynamicWaitType WaitType;
        //int MaxThread;
        //int Interval;
        //int MaxCponnections = 1;
        //bool IsMultiTask = true;
        ////protected QSettings qSettings;
        ////protected MsgQueue<T> Q;
        //protected QAgentSettings Settings { get; private set; }
        //protected QueueListener Q { get; private set; }

        protected QAgent(QueueAdapter adapter):base(adapter)
        {
            //SourceName = Extentions.GetMethodFullName(new System.Diagnostics.StackTrace().GetFrame(1));
            //Settings = settings;
            //Q = new QueueListener(settings);
            //WaitType = settings.ListenerWaitType;
            //IsMultiTask = settings.ListenerMultiTask;
            //MaxThread = settings.MaxWorkers;
            //MaxCponnections = settings.MaxConnections;
            //Interval = settings.WorkerInterval;
        }
        //protected QAgent(QAgentSettings settings, QueueListener listener)
        //{
        //    //SourceName = Extentions.GetMethodFullName(new System.Diagnostics.StackTrace().GetFrame(1));
        //    Settings = settings;
        //    Q = listener;
        //    WaitType = settings.ListenerWaitType;
        //    IsMultiTask = settings.ListenerMultiTask;
        //    MaxThread = settings.MaxWorkers;
        //    MaxCponnections = settings.MaxConnections;
        //    Interval = settings.WorkerInterval;
        //}

        #region override

        //in case of ResetEvent and fixed interval using
        protected override void ReceiveAsync(IDynamicWait dw)
        {
            QueueRequest request = new QueueRequest()
            {
                Host = QApi.QueueName,
                QCommand = QueueCmd.Dequeue,
                DuplexType = DuplexTypes.WaitOne
            };

            QApi.DequeueAsync(request, ConnectTimeout, OnCompleted, dw);

        }

        //in case of DynamicWait or fixed interval using
        protected override IQueueMessage Receive()
        {
            QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.Dequeue, null);
            {
                Host = QApi.QueueName,
                QCommand = QueueCmd.Dequeue,
                DuplexType = DuplexTypes.WaitOne
            };
            return QApi.Dequeue(request);
        }

        public override void Abort(Ptr ptr)
        {
            QApi.Abort(ptr);
        }

        public override void Commit(Ptr ptr)
        {
            QApi.Commit(ptr);
        }

        #endregion

        protected override void OnMessageReceived(IQueueMessage message)
        {
           
        }
    }
}
