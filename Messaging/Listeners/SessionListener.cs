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


namespace Nistec.Messaging.Listeners
{
    

    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection.
    /// </summary>
    public abstract class SessionListener : IListener
    {

        public const int DefaultInterval = 1000;

        protected QueueAdapter _adapter;

        //protected QueueApi _api;

        //protected Listener _Listener;
        //internal Listener Listener
        //{
        //    get
        //    {
        //        return _Listener;
        //    }
        //}
        CancellationTokenSource canceller = new CancellationTokenSource();

        QueueHost _Source;
        public QueueHost Source { get { return _Source; } }
        //QueueHost _TransferTo;
        //public QueueHost TransferTo { get { return _TransferTo; } }



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

        bool _IsAsync;
        public bool IsAsync { get { return _IsAsync; } }

        //AdapterOperations _AdapterOperation;
        //public AdapterOperations OperationType { get { return _AdapterOperation; } }

        Action<IQueueItem> _Action;
        public Action<IQueueItem> QueueAction { get { return _Action; } }

        //Action<IQueueAck> _ActionTransfer;
        //public Action<IQueueAck> TransferAction { get { return _ActionTransfer; } }

        //IListenerHandler _Owner;

        #region message events

        /// <summary>
        /// ErrorOcurred
        /// </summary>
        public event GenericEventHandler<string> ErrorOcurred;
        /// <summary>
        /// QueueItem Received
        /// </summary>
        public event GenericEventHandler<IQueueItem> MessageReceived;

        void DoMessageReceived(IQueueItem message)
        {
            OnMessageReceived(new GenericEventArgs<IQueueItem>(message));
        }

        void DoErrorOcurred(string message)
        {
            OnErrorOcurred(new GenericEventArgs<string>(message));
        }
        /// <summary>
        /// Occured when message received.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessageReceived(GenericEventArgs<IQueueItem> e)
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

        protected void OnFault(string message)
        {
            OnErrorOcurred(new GenericEventArgs<string>(message));
        }

        protected void OnCompleted(QueueItem item)
        {
            OnMessageReceived(item);
        }


        #endregion

        internal SessionListener(QueueAdapter adapter, int interval)
        {
            if (adapter == null)
            {
                throw new ArgumentNullException("adapter");
            }
            if (adapter.Source == null)
            {
                throw new ArgumentNullException("adapter.Source");
            }
            _adapter = adapter;

            //_Owner = owner;
            _Source = adapter.Source;
            //_TransferTo = adapter.TransferTo;

            //_ServerName = channel.ServerName;
            //_QueueName = channel.Source;
            _Interval = interval <= 0 ? DefaultInterval : interval;// 1000;

            //_Interval = adapter.Interval;
            _ConnectTimeout = adapter.ConnectTimeout;
            _WorkerCount = adapter.WorkerCount;
            _IsAsync = adapter.IsAsync;
            _Action = adapter.QueueAction;
            //_ActionTransfer = adapter.AckAction;
            //_AdapterOperation = adapter.OperationType;

            //_api = new QueueApi(adapter.Source);


        }

        protected abstract IQueueAck Send(QueueItem message);

        protected abstract IQueueItem Receive();

        protected abstract void ReceiveAsync(AutoResetEvent resetEvent);

        //protected abstract IQueueItem ReceiveRound();

        //protected abstract void ReceiveAsync(int connectTimeout, Action<QueueItem> action);

        //protected abstract IQueueAck ReceiveTo();// QueueHost target, int connectTimeout, Action<QueueItem> recieveAction);

        public abstract void Commit(Ptr ptr);

        public abstract void Abort(Ptr ptr);

        //protected virtual void OnMessageTransfer(IQueueAck ack)
        //{
        //    if (ack != null)
        //    {
        //        if (_Action != null)
        //        {
        //            Task.Factory.StartNew(() => _ActionTransfer(ack));
        //        }
        //        //else
        //        //    Task.Factory.StartNew(() => DoMessageTransfer(item));
        //    }
        //}
        protected virtual void OnMessageReceived(IQueueItem item)
        {
            if (item != null)
            {
                if (_Action != null)
                {
                    Task.Factory.StartNew(() => _Action(item));
                }
                else
                    Task.Factory.StartNew(() => DoMessageReceived(item));
            }
        }

        bool lockWasTaken = false;
        object _locker = new object();
        Thread[] _workers;
        long delay;
 
        public void Start()
        {
            if (_isalive)
            {
                return;
            }
            _workers = new Thread[_WorkerCount];
            ThreadStart threadWorker = IsAsync ? new ThreadStart(TaskWorkerAsync) : new ThreadStart(TaskWorker);
            for (int i = 0; i < _WorkerCount; i++)
            {
                _workers[i] = new Thread(new ThreadStart(threadWorker));
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

        //int _Current;
        //IQueueItem RecieveRound()
        //{
        //    var message = Receive();
        //    autoResetEvent.Set();
        //    return message;
        //}

        IQueueItem RecieveInternal()
        {
            var message = Receive();
            autoResetEvent.Set();
            return message;
        }

        //IQueueAck TrabnsferInternal()
        //{
        //    var message = ReceiveTo();
        //    autoResetEvent.Set();
        //    return message;
        //}

        static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

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

                    Monitor.Enter(_locker);
                    lockWasTaken = true;

                    var task = Task.Factory.StartNew(() => RecieveInternal());
                    autoResetEvent.WaitOne();
                    OnMessageReceived(task.Result);
                    
                    //switch (_AdapterOperation)
                    //{
                    //    case AdapterOperations.Transfer:
                    //        {
                    //            var task = Task.Factory.StartNew(() => TrabnsferInternal());
                    //            autoResetEvent.WaitOne();
                    //            OnMessageTransfer(task.Result);
                    //        }
                    //        break;
                    //    //case AdapterOperations.ReceiveRound:
                    //    //    {
                    //    //        var task = Task.Factory.StartNew(() => RecieveInternal());
                    //    //        autoResetEvent.WaitOne();
                    //    //        OnMessageReceived(task.Result);
                    //    //    }
                    //    //    break;
                    //    //case AdapterOperations.Sync:
                    //    case AdapterOperations.Recieve:
                    //        {
                    //            //Task task = Task.Factory.StartNew(() =>
                    //            //{
                    //            //    Receive();
                    //            //});
                    //            //autoResetEvent.WaitOne();

                    //            //OnMessageReceived(task.Result);

                    //            var task = Task.Factory.StartNew(() => RecieveInternal());
                    //            autoResetEvent.WaitOne();
                    //            OnMessageReceived(task.Result);

                    //            //task.Wait();
                    //            //if (task.IsCompleted)
                    //            //{
                    //            //    OnMessageReceived(task.Result);
                    //            //}
                    //        }
                    //        break;
                    //    //case AdapterOperations.Sync:
                    //    //    QueueItem item = RecieveInternal();
                            
                    //    //    OnMessageReceived(item);
                    //    //    //if (item != null)
                    //    //    //{
                    //    //    //    if (_Action != null)
                    //    //    //    {
                    //    //    //        Task.Factory.StartNew(() => _Action(item));
                    //    //    //    }
                    //    //    //    else
                    //    //    //        Task.Factory.StartNew(() => DoMessageReceived(item));
                    //    //    //}
                    //    //    break;
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine("QListener error: " + ex.Message);
                    Task.Factory.StartNew(() => DoErrorOcurred(ex.Message));
                }
                finally
                {
                    if (lockWasTaken) Monitor.Exit(_locker);
                }
                Thread.Sleep(_Interval);
            }

            Console.WriteLine("QListener stoped...");
        }

        void TaskWorkerAsync()
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

                    Monitor.Enter(_locker);
                    lockWasTaken = true;

                    var task = Task.Factory.StartNew(() =>
                    //RecieveInternal()
                    ReceiveAsync(autoResetEvent)
                    //autoResetEvent.Set();
                    //return message;
                    );
                    autoResetEvent.WaitOne();
                    //OnMessageReceived(task.Result);

                    //switch (_AdapterOperation)
                    //{
                    //    case AdapterOperations.Transfer:
                    //        {
                    //            var task = Task.Factory.StartNew(() => TrabnsferInternal());
                    //            autoResetEvent.WaitOne();
                    //            OnMessageTransfer(task.Result);
                    //        }
                    //        break;
                    //    //case AdapterOperations.ReceiveRound:
                    //    //    {
                    //    //        var task = Task.Factory.StartNew(() => RecieveInternal());
                    //    //        autoResetEvent.WaitOne();
                    //    //        OnMessageReceived(task.Result);
                    //    //    }
                    //    //    break;
                    //    //case AdapterOperations.Sync:
                    //    case AdapterOperations.Recieve:
                    //        {
                    //            //Task task = Task.Factory.StartNew(() =>
                    //            //{
                    //            //    Receive();
                    //            //});
                    //            //autoResetEvent.WaitOne();

                    //            //OnMessageReceived(task.Result);

                    //            var task = Task.Factory.StartNew(() => RecieveInternal());
                    //            autoResetEvent.WaitOne();
                    //            OnMessageReceived(task.Result);

                    //            //task.Wait();
                    //            //if (task.IsCompleted)
                    //            //{
                    //            //    OnMessageReceived(task.Result);
                    //            //}
                    //        }
                    //        break;
                    //    //case AdapterOperations.Sync:
                    //    //    QueueItem item = RecieveInternal();

                    //    //    OnMessageReceived(item);
                    //    //    //if (item != null)
                    //    //    //{
                    //    //    //    if (_Action != null)
                    //    //    //    {
                    //    //    //        Task.Factory.StartNew(() => _Action(item));
                    //    //    //    }
                    //    //    //    else
                    //    //    //        Task.Factory.StartNew(() => DoMessageReceived(item));
                    //    //    //}
                    //    //    break;
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine("QListener error: " + ex.Message);
                    Task.Factory.StartNew(() => DoErrorOcurred(ex.Message));
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
