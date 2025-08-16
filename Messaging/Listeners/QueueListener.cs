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

        public QueueListener()//, int interval)
        {
            
        }

        public QueueListener(QueueAdapter adapter)//, int interval)
        {
            Init(adapter);
        }

        public override void Init(QueueAdapter adapter)
        {
            base.Init(adapter);
            InitApi();
            //_Listener= new ListenerQ(this, adapter);
        }
        public virtual void InitApi()
        {
            QApi = new QueueApi(Source);
            QApi.ReadTimeout = ReadTimeout;
            //_Listener= new ListenerQ(this, adapter);
        }
        #endregion

        protected override IQueueMessage Receive()
        {
            return QApi.Consume(60000);// int.MaxValue);
        }
  
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
