using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nistec.Messaging.Server
{
    public class TaskSchedulerDemo
    {
        static void Demo()
        {
            var cts = new CancellationTokenSource();
            var myTs = new SingleThreadTaskScheduler(cts.Token);

            myTs.Schedule(() =>
            { Print("Init start"); Thread.Sleep(1000); Print("Init done"); });
            myTs.Schedule(() => Print("Work 1"));
            myTs.Schedule(() => Print("Work 2"));
            myTs.Schedule(() => Print("Work 3"));
            var lastOne = myTs.Schedule(() => Print("Work 4"));

            Print("Starting TS");
            myTs.Start();

            // Wait for all of them to complete...
            lastOne.GetAwaiter().GetResult();

            Thread.Sleep(1000);

            // And try to schedule another
            myTs.Schedule(() => Print("After emptied")).GetAwaiter().GetResult();

            // And shutdown; it's also pretty useful to have the 
            // TaskScheduler return a "complete task" to await
            myTs.Complete();

            Print("On main thread again");
        }
        static void Print(string str)
        {
            Console.WriteLine("{0}: {1}", Thread.CurrentThread.ManagedThreadId, str);
            Thread.Sleep(100);
        }
    }


    public class SingleTaskRunner
    {
        public static readonly SingleTaskRunner Instance = new SingleTaskRunner();
        private readonly BlockingCollection<Action> _taskQueue = new BlockingCollection<Action>();
        private readonly Thread _workerThread;

        public SingleTaskRunner()
        {
            _workerThread = new Thread(() =>
            {
                foreach (var task in _taskQueue.GetConsumingEnumerable())
                {
                    task();
                }
            });
            _workerThread.IsBackground = true;
            _workerThread.Start();
        }

        public void Enqueue(Action action)
        {
            _taskQueue.Add(action);
        }

        public void Stop()
        {
            _taskQueue.CompleteAdding();
        }
    }

    public sealed class SingleThreadTaskScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool isExecuting;
        private readonly CancellationToken cancellationToken;

        private readonly BlockingCollection<Task> taskQueue;

        public SingleThreadTaskScheduler(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            this.taskQueue = new BlockingCollection<Task>();
        }

        public void Start()
        {
            new Thread(RunOnCurrentThread) { Name = "STTS Thread" }.Start();
        }

        // Just a helper for the sample code
        public Task Schedule(Action action)
        {
            return
                Task.Factory.StartNew
                    (
                        action,
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        this
                    );
        }

        // You can have this public if you want - just make sure to hide it
        private void RunOnCurrentThread()
        {
            isExecuting = true;

            try
            {
                foreach (var task in taskQueue.GetConsumingEnumerable(cancellationToken))
                {
                    TryExecuteTask(task);
                }
            }
            catch (OperationCanceledException)
            { }
            finally
            {
                isExecuting = false;
            }
        }

        // Signaling this allows the task scheduler to finish after all tasks complete
        public void Complete() { taskQueue.CompleteAdding(); }
        protected override IEnumerable<Task> GetScheduledTasks() { return null; }

        protected override void QueueTask(Task task)
        {
            try
            {
                taskQueue.Add(task, cancellationToken);
            }
            catch (OperationCanceledException)
            { }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // We'd need to remove the task from queue if it was already queued. 
            // That would be too hard.
            if (taskWasPreviouslyQueued) return false;

            return isExecuting && TryExecuteTask(task);
        }
    }
}
