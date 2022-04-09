using Nistec.Messaging.Listeners;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Remote
{
    public class RemoteManager:IListener
    {

        //static readonly ConcurrentDictionary<string, QueueAdapter> Pool = new ConcurrentDictionary<string, QueueAdapter>();
        static readonly ConcurrentDictionary<string, QueueListener> Pool = new ConcurrentDictionary<string, QueueListener>();

        public static readonly RemoteManager Instance = new RemoteManager();

        /// <summary>
        /// Get or Set the delegate of target methods.
        /// </summary>
        public Action<IQueueItem,string> MessageReceivedAction { get; set; }
        /// <summary>
        /// Get or Set the delegate of acknowledgment methods.
        /// </summary>
        public Action<IQueueAck, string> MessageAckAction { get; set; }
        public Action<string, string> MessageFaultAction { get; set; }

        
        public ListenerState State { get; private set; }

        public RemoteManager() {  }

        public void Init(QueueAdapter[] adapters)
        {
            foreach(var a in adapters)
                TryUpdate(a.Source.HostName,a);
            State = ListenerState.Initilaized;
        }

        protected void OnListenerLoaded(QueueListener ql)
        {
            //ql.ErrorOcurred -= Ql_ErrorOcurred;
            //ql.MessageReceived -= Ql_MessageReceived;

            ql.ErrorOcurred += Ql_ErrorOcurred;
            ql.MessageReceived += Ql_MessageReceived;
        }
        protected void OnListenerRemoved(QueueListener ql)
        {
            //if(ql.ErrorOcurred !=null)
            ql.ErrorOcurred -= Ql_ErrorOcurred;
            //if (ql.MessageReceived != null)
            ql.MessageReceived -= Ql_MessageReceived;
        }

        protected virtual void OnMessageReceived(IQueueItem item, string hostName)
        {
            if (MessageReceivedAction != null)
                MessageReceivedAction(item, hostName);
        }

        protected virtual void OnErrorOcurred(string message, string hostName)
        {
            if (MessageFaultAction != null)
                MessageFaultAction(message, hostName);
        }


        private void Ql_MessageReceived(object sender, Generic.GenericEventArgs<IQueueItem> e)
        {
            var message = e.Args;
            QueueListener ql = (QueueListener)sender;

            //if (MessageReceivedAction != null)
            //    MessageReceivedAction(message,ql.Source.HostName);

            OnMessageReceived(message, ql.Source.HostName);
            //Console.WriteLine("RemoteManager.MessageReceived  State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}", message.MessageState, message.ArrivedTime, message.Host, message.Label, message.Identifier);
        }

        private void Ql_ErrorOcurred(object sender, Generic.GenericEventArgs<string> e)
        {
            var message = e.Args;
            QueueListener ql = (QueueListener)sender;
            //if (MessageFaultAction != null)
            //    MessageFaultAction(message, ql.Source.HostName);

            OnErrorOcurred(message, ql.Source.HostName);
            //Console.WriteLine("RemoteManager.MessageReceived HostName: {0}, Error: {1}", ql.Source.HostName, message);
        }

       
        //public void Set(string name, QueueAdapter adapter)
        //{
        //    Pool[name] = new QueueListener(adapter);
        //}
        public bool TryAdd(string hostname, QueueAdapter adapter)
        {
            var listener = new QueueListener(adapter);

            if (Pool.TryAdd(hostname, listener))
            {
                OnListenerLoaded(listener);
                return true;
            }
            return false;
        }

        public bool TryUpdate(string hostname, QueueAdapter adapter)
        {
            var listener = new QueueListener(adapter);

            QueueListener cur;
            if (Pool.TryGetValue(hostname, out cur))
            {
                if (Pool.TryUpdate(hostname, listener, cur))
                {

                    OnListenerRemoved(cur);
                    OnListenerLoaded(listener);
                    return true;
                }
            }
            if (Pool.TryAdd(hostname, listener))
            {
                OnListenerLoaded(listener);
                return true;
            }
            return false;
        }

        public bool TryRemove(string name, out QueueListener ql)
        {
            if (Pool.TryRemove(name, out ql))
            {
                OnListenerRemoved(ql);
                return true;
            }
            return false;
        }

        public bool Remove(string name)
        {
            QueueListener ql;
            if (Pool.TryRemove(name, out ql)) {
                OnListenerRemoved(ql);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            foreach (var ql in Pool)
                Remove(ql.Key);

            Pool.Clear();
        }

        public static QueueListener Get(string hostname)
        {
            QueueListener ql;
            if (Pool.TryGetValue(hostname, out ql))
            {
                return ql;
            }
            return null;
        }
        public static bool TryGet(string hostname, out QueueListener ql)
        {
            return Pool.TryGetValue(hostname, out ql);
        }

        public void Start()
        {
            foreach (var a in Pool)
                a.Value.Start();

        }
        public void Stop()
        {
            foreach (var a in Pool)
                a.Value.Stop();

        }
        public bool Pause(OnOffState onoff)
        {
            foreach (var a in Pool)
                return a.Value.Pause(onoff);
            return false;
        }
        public void Shutdown(bool waitForWorkers)
        {
            foreach (var Key in Pool.Keys)
                Shutdown(Key, waitForWorkers);
        }

        public void Start(string hostname)
        {
            QueueListener ql;
            if (TryGet(hostname, out ql)){
                ql.Start();
            }

        }
        public void Stop(string hostname)
        {
            QueueListener ql;
            if (TryGet(hostname, out ql))
            {
                ql.Stop();
            }
        }
        public bool Pause(string hostname, OnOffState onoff)
        {
            QueueListener ql;
            if (TryGet(hostname, out ql))
            {
               return ql.Pause(onoff);
            }
            return false;
        }
        public void Shutdown(string hostname,bool waitForWorkers)
        {
            QueueListener ql;
            if (Pool.TryRemove(hostname, out ql))
            {
                ql.Shutdown(waitForWorkers);
            }
        }
    }
}
