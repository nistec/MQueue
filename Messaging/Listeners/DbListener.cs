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

namespace Nistec.Messaging.Listeners
{
     
    /// <summary>
    /// Represents a thread-safe db queue listener (FIFO) collection.
    /// </summary>
    public class DbListener : ListenerHandler, IListenerHandler
    {
 
        #region ctor

        /// <summary>
        /// Initialize a new instance of db adapter.
        /// </summary>
        /// <param name="dbs"></param>
        public DbListener(AdapterProperties[] dbs)
            : base(dbs)
        {
         
        }
        /// <summary>
        /// Initialize a new instance of db adapter.
        /// </summary>
        /// <param name="db"></param>
        public DbListener(AdapterProperties db)
            : base(db)
        {
            
        }

        //public FolderListener(string queueName, string serverName = ".")
        //    : base(queueName, serverName)
        //{
 
        //}

        #endregion

        #region override

        protected override Listener CreateListener(AdapterProperties lp)
        {
            return new ListenerQ(this, lp);
        }

        public override Listener Find(string hostName)
        {
            if (hostName == null)
            {
                throw new ArgumentNullException("Find.hostName");
            }
            return Listeners.Where(q => q.Source.HostName == hostName).FirstOrDefault<Listener>();
        }

        public void Abort(Message ack)
        {
            var listener = EnsureListener(ack);
            listener.Abort(ack.GetPtr());
        }

        public void Commit(Message ack)
        {
            var listener = EnsureListener(ack);
            listener.Commit(ack.GetPtr());
        }
        
        #endregion

        public class ListenerQ : Listener
        {
            DbAdapter _Adapter;

            internal ListenerQ(IListenerHandler owner, AdapterProperties properties)
                : base(owner, properties)
            {
                _Adapter = new DbAdapter(properties);
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
