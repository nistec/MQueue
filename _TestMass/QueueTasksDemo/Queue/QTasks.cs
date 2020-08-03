using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueTasksDemo.Queue
{
    public class QTasks
    {
        public void Example()
        {
            BlockingCollection<LogItem> _queue = new BlockingCollection<LogItem>();

            // Start queue listener...
            CancellationTokenSource canceller = new CancellationTokenSource();
            Task listener = Task.Factory.StartNew(() =>
            {
                while (!canceller.Token.IsCancellationRequested)
                {
                    LogItem item;
                    if (_queue.TryTake(out item))
                        Console.WriteLine(item.Message);
                }
            },
            canceller.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

            // Add some log messages in parallel...
            Parallel.Invoke(
                () => { _queue.Add(new LogItem("Log from task A")); },
                () =>
                {
                    _queue.Add(new LogItem("Log from task B"));
                    _queue.Add(new LogItem("Log from task B1"));
                },
                () => { _queue.Add(new LogItem("Log from task C")); },
                () => { _queue.Add(new LogItem("Log from task D")); });

            // Pretend to do other things...
            Thread.Sleep(1000);

            // Shut down the listener...
            canceller.Cancel();
            listener.Wait();
        }
    }
}
