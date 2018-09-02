using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueTasksDemo.Queue
{
    public class QListener
    {
        //static BlockingCollection<LogItem> _queue = new BlockingCollection<LogItem>();
        static ConcurrentQueue<LogItem> _queue = new ConcurrentQueue<LogItem>();
        static CancellationTokenSource canceller = new CancellationTokenSource();
        long _counter;
        public int Count
        {
            get { return _queue.Count; }
        }
        bool _isalive = false;
        public bool IsAlive
        {
            get { return _isalive; }
        }

        object locker = new object();
        public void Add(LogItem item)
        {
            //_queue.Add(item);
            _queue.Enqueue(item);
            Interlocked.Increment(ref _counter);
            Thread.Sleep(1);
            lock (locker)
            {
                Console.WriteLine("QListener item added, Count: {0}", Interlocked.Read(ref _counter));
            }
        }

        public LogItem Get()
        {
            LogItem item;
            if (_queue.TryDequeue(out item))
            {
                Interlocked.Decrement(ref _counter);
            }
            //_queue.TryTake(out item);
            return item;
        }

        public void Start(int maxTasks)
        {
            _isalive = true;
            Task[] tasks = new Task[maxTasks];

            for (int i = 0; i < maxTasks; i++)
            {
                tasks[i] = new Task(TaskWorker);
                tasks[i].Start();
            }
        }

        private void TaskWorker()
        {
            while (_isalive)
            {
                LogItem item;
                if (_queue.TryDequeue(out item))
                //if (_queue.TryTake(out item))
                {
                    Console.WriteLine(item.Message);
                    //Console.WriteLine("QListener is alive...");
                }
                Thread.Sleep(10);
            }
        }

        public void Start()
        {
            _isalive = true;
            // Start queue listener...
            Task listener = Task.Factory.StartNew(() =>
            {
                while (!canceller.Token.IsCancellationRequested)
                {
                    LogItem item;
                    if (_queue.TryDequeue(out item))
                    //if (_queue.TryTake(out item))
                    {
                        Console.WriteLine(item.Message);
                        //Console.WriteLine("QListener is alive...");
                    }
                    Thread.Sleep(10);
                }
                _isalive = false;
                Console.WriteLine("QListener stoped...");
            },
            canceller.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

            Console.WriteLine("QListener started...");


        }

        public void Stop()
        {
            _isalive = false;
            // Shut down the listener...
            canceller.Cancel();
            //listener.Wait();
        }
    }
}
