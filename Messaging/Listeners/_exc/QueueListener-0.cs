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
using Nistec.Messaging.Adapters;

namespace Nistec.Messaging.Session
{
     
    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection for client.
    /// </summary>
    public class QueueListener : ListenerHandler, IListenerHandler
    {

        #region ctor


        //public QueueListener(AdapterProperties[] queues)
        //    : base(queues)
        //{

        //}

        public QueueListener(AdapterProperties adapter)
            : base(adapter)
        {
            _adapter = adapter;
            _Listener = new ListenerQ(this, adapter);
        }

        //public QueueListener(string queueName, string serverName = ".")
        //    : base(queueName, serverName)
        //{
        //    _adapter = new AdapterProperties()
        //    {
                 
        //    };
        //}

        #endregion

        #region override

        protected override Listener CreateListener(AdapterProperties lp)
        {
            return new ListenerQ(this, lp);
        }

        public override Listener Find(string hostId)
        {
            if (hostId == null)
            {
                throw new ArgumentNullException("Find.hostName");
            }
            return Listeners.Where(q => q.Source.HostId == hostId).FirstOrDefault<Listener>();
        }

        public void Abort(Ptr ptr)
        {
            var listener = EnsureListener(_adapter.Source);
            listener.Abort(ptr);
        }

        public void Commit(Ptr ptr)
        {
            var listener = EnsureListener(_adapter.Source);
            listener.Commit(ptr);
        }
        
        #endregion

        public class ListenerQ : Listener
        {
            AdapterBase _Adapter;

            internal ListenerQ(IListenerHandler owner, AdapterProperties properties)
                : base(owner, properties)
            {
                _Adapter = properties.GetAdapter();
                _Adapter.ConnectTimeout = properties.ConnectTimeout;
            }

            protected override IQueueAck Enqueue(Message message)
            {
                return _Adapter.Enqueue(message);
            }

            protected override Message Dequeue()
            {
                return _Adapter.Dequeue();
            }

            protected override int DequeueAsync()
            {
                return _Adapter.DequeueAsync();
            }

            protected override int ReceiveTo()
            {
                return _Adapter.ReceiveTo();
            }

            public override void Abort(Ptr ptr)
            {
                _Adapter.Abort(ptr);
            }

            public override void Commit(Ptr ptr)
            {
                _Adapter.Commit(ptr);
            }

         }

    }
}
