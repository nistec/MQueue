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
    /// Represents a thread-safe queue listener (FIFO) collection.
    /// </summary>
    public abstract class Listener : IListener
    {
        QueueHost _Source;
        public QueueHost Source { get { return _Source; } }
        QueueHost _TransferTo;
        public QueueHost TransferTo { get { return _TransferTo; } }

        //string _HostName;
        //public string HostName { get { return _HostName; } }
        //string _ServerName;
        //public string ServerName { get { return _ServerName; } }
        int _Interval;
        public int Interval { get { return _Interval; } }
        int _ConnectTimeout;
        public int ConnectTimeout { get { return _ConnectTimeout; } }
        bool _isalive = false;
        public bool IsAlive { get { return _isalive; } }
        int _WorkerCount;
        public int WorkerCount { get { return _WorkerCount; } }
        AdapterOperations _AdapterOperation;
        public AdapterOperations OperationType { get { return _AdapterOperation; } }
        Action<Message> _Action;
        public Action<Message> QueueAction { get { return _Action; } }

        IListenerHandler _Owner;


        internal Listener(IListenerHandler owner, AdapterProperties channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            if (channel.Source == null)
            {
                throw new ArgumentNullException("channel.Source");
            }
            _Owner = owner;
            _Source = channel.Source;
            _TransferTo = channel.TransferTo;

            //_ServerName = channel.ServerName;
            //_QueueName = channel.Source;
            _Interval = channel.Interval;
            _ConnectTimeout = channel.ConnectTimeout;
            _WorkerCount = channel.WorkerCount;
            _Action = channel.TargetAction;
            _AdapterOperation = channel.OperationType;
            
        }

        protected abstract IQueueAck Enqueue(Message message);

        protected abstract Message Dequeue();

        protected abstract int DequeueAsync();

        protected abstract int ReceiveTo();

        public abstract void Commit(Ptr ptr);

        public abstract void Abort(Ptr ptr);

        bool lockWasTaken = false;
        readonly object _locker = new object();
        Thread[] _workers;
        long delay;
 
        public void Start()
        {
            if (_isalive)
            {
                return;
            }
            _workers = new Thread[_WorkerCount];

            for (int i = 0; i < _WorkerCount; i++)
            {
                _workers[i] = new Thread(new ThreadStart(TaskWorker));
                _workers[i].IsBackground = true;
                _workers[i].Start();
            }
        }

        public void Shutdown(bool waitForWorkers)
        {
            _isalive = false;

            // Wait for workers to finish
            if (waitForWorkers)
                foreach (Thread worker in _workers)
                    worker.Join();
        }

        public void Delay(TimeSpan time)
        {
            Interlocked.Exchange(ref delay, (long)time.TotalMilliseconds);
        }

        void TaskWorker()
        {
            _isalive = true;
            // Start queue listener...
            Console.WriteLine("QListener started...");

            while (_isalive)
            {
                try
                {
                    if (Interlocked.Read(ref delay) > 0)
                    {
                        Thread.Sleep((int)delay);
                        Interlocked.Exchange(ref delay, 0);
                    }

                    Monitor.Enter(_locker, ref lockWasTaken);

                    switch (_AdapterOperation)
                    {
                        case AdapterOperations.Transfer:
                            Task.Factory.StartNew(() => ReceiveTo());
                            break;
                        case AdapterOperations.Async:
                            Task.Factory.StartNew(() => DequeueAsync());
                            break;
                        case AdapterOperations.Sync:
                            Message item = Dequeue();
                            if (item != null)
                            {
                                if (_Action != null)
                                {
                                    Task.Factory.StartNew(() => _Action(item));
                                }
                                else
                                    Task.Factory.StartNew(() => _Owner.DoMessageReceived(item));
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("QListener error: " + ex.Message);
                    Task.Factory.StartNew(() => _Owner.DoErrorOcurred(ex.Message));
                }
                finally
                {
                    if (lockWasTaken) Monitor.Exit(_locker);
                }
                Thread.Sleep(_Interval);
            }

            Console.WriteLine("QListener stoped...");
        }
    }
}
