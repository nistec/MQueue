using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Generic;
using System.Collections.ObjectModel;

namespace Nistec.Messaging.Listeners
{
     
    /// <summary>
    /// Represents a base class for thread-safe queue listener (FIFO) collection.
    /// </summary>
    public class SessionDispatcher : IListenerHandler
    {
        CancellationTokenSource canceller = new CancellationTokenSource();
         
        #region message events

        /// <summary>
        /// ErrorOcurred
        /// </summary>
        public event GenericEventHandler<string> ErrorOcurred;
        /// <summary>
        /// QueueMessage Received
        /// </summary>
        public event GenericEventHandler<QueueMessage> MessageReceived;

        void IListenerHandler.DoMessageReceived(QueueMessage message)
        {
            OnMessageReceived(new GenericEventArgs<QueueMessage>(message));
        }

        void IListenerHandler.DoErrorOcurred(string message)
        {
            OnErrorOcurred(new GenericEventArgs<string>(message));
        }
        /// <summary>
        /// Occured when message received.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessageReceived(GenericEventArgs<QueueMessage> e)
        {

            if (MessageReceived != null)
                MessageReceived(this, e);
        }
        /// <summary>
        /// Occured when operation has error.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
                ErrorOcurred(this, e);
        }

        private void OnErrorOcurred(string msg)
        {
            Console.WriteLine("ErrorOcurred: " + msg);
            OnErrorOcurred(new GenericEventArgs<string>(msg));
        }

        #endregion

        #region ctor

        ///// <summary>
        ///// Initialize the listener handler for multiple adapters using <see cref="AdapterProperties"/> properties.
        ///// </summary>
        ///// <param name="lpArray"></param>
        //public ListenerHandler(AdapterProperties[] lpArray)
        //{
        //    if (lpArray == null)
        //    {
        //        throw new ArgumentNullException("ListenerProperties.lpArray");
        //    }

        //    listeners = new List<Listener>();
        //    foreach (var lp in lpArray)
        //    {
        //        listeners.Add(CreateListener(lp));
        //    }
        //}

        ///// <summary>
        ///// Initialize the listener handler using <see cref="AdapterProperties"/> property.
        ///// </summary>
        ///// <param name="lp"></param>
        //public void Add(AdapterProperties adapter)
        //{
        //    if (adapter == null)
        //    {
        //        throw new ArgumentNullException("ListenerProperties.adapter");
        //    }

        //    if(adapter.IsTopic)
        //        Items.Add(new TopicListener(adapter));
        //    else
        //        Items.Add(new QueueListener(adapter));

        //    //listeners = new List<Listener>();
        //    //Items.Add(CreateListener(adapter));
        //}

        ///// <summary>
        ///// Initialize the listener handler for local queue.
        ///// </summary>
        ///// <param name="queueName"></param>
        ///// <param name="serverName"></param>
        //public ListenerHandler(string queueName, string serverName = ".")
        //{
        //    if (string.IsNullOrEmpty(queueName))
        //    {
        //        throw new ArgumentNullException("queueName");
        //    }
        //    AdapterProperties lp = new AdapterProperties(queueName, serverName);
        //    listeners.Add(CreateListener(lp));
        //}


        /// <summary>
        /// Initialize the listener handler using <see cref="AdapterProperties"/> property.
        /// </summary>
        /// <param name="lp"></param>
        public void Add(SessionListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("ListenerProperties.listener");
            }
            Items.Add(listener);
        }


        #endregion

        ///// <summary>
        ///// Create a new listener using <see cref="AdapterProperties"/> properties.
        ///// </summary>
        ///// <param name="adapter"></param>
        ///// <returns></returns>
        //public Listener CreateListener(IListenerHandler owner,AdapterProperties lp)
        //{
        //    return new QueueListener(owner, lp);
        //}

        #region properties

        List<SessionListener> _listeners;
        List<SessionListener> Items
        {
            get
            {
                if(_listeners==null)
                {
                    _listeners = new List<SessionListener>();
                }
                return _listeners;
            }
        }


        
        /// <summary>
        /// Get the Listeners collection.
        /// </summary>
        public ReadOnlyCollection<SessionListener> Listeners
        {
            get { return Items.AsReadOnly(); }
        }
        /// <summary>
        /// Find listener by host name.
        /// </summary>
        /// <param name="hostId"></param>
        /// <returns></returns>
        public SessionListener Find(string hostId)
        {
            if (hostId == null)
            {
                throw new ArgumentNullException("Find.hostName");
            }
            return Items.Where(q => q.Source.HostId == hostId).FirstOrDefault<SessionListener>();
        }


        /// <summary>
        /// Ensure that lister is exists.
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public virtual SessionListener EnsureListener(QueueHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("Find.ack");
            }
            var listener = Find(host.HostId);
            if (listener == null)
            {
                throw new Exception("Listener not found " + host. HostId);
            }
            return listener;
        }

        /// <summary>
        /// Pause the queue for given time using host name and <see cref="TimeSpan"/> time.
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public bool PauseQueue(string queueName)
        {
            IListener li = Find(queueName);
            if (li == null)
                return false;
            li.Pause(OnOffState.On);
            return true;
        }

        bool _isalive = false;
        /// <summary>
        /// Get indicating whether the queue listener ia alive.
        /// </summary>
        public bool IsAlive
        {
            get { return _isalive; }
        }
        #endregion
      
        #region start/stop

        /// <summary>
        /// Start the queue listener.
        /// </summary>
        public void Start()
        {
            foreach (var channel in Items)
            {
                channel.Start();
            }
            _isalive = true;
            // Start queue listener...
            Console.WriteLine("QListener started...");
        }

        /// <summary>
        /// Stop the queue listener.
        /// </summary>
        public void Stop(bool waitForWorkers)
        {
            foreach (var channel in Items)
            {
                channel.Shutdown(waitForWorkers);
            }
            _isalive = false;

            // Shut down the listener...
            //canceller.Cancel();
            //listener.Wait();
        }

        #endregion

    }

}
