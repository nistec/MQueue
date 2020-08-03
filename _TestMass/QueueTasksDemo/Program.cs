using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Nistec.Collections;

namespace QueueTasksDemo
{
    class Program
    {
        //static QListener _queue=new QListener();

        static void Main(string[] args)
        {
            Console.WriteLine("QueueTasksDemo started...");

            var watch=Stopwatch.StartNew();

            QueueListener<LogItem> _queue = new QueueListener<LogItem>(MessageReceivedAction<LogItem>);
            //_queue.MessageReceived += new MControl.Generic.GenericEventHandler<LogItem>(_queue_MessageReceived);
            _queue.Start();

            int counter = 0;
            int items = 0;

            while (_queue.IsAlive)
            {
                if (++counter > 200)
                {
                    break;
                }
                // Add some log messages in parallel...

                Task.Factory.StartNew(() => _queue.Enqueue(new LogItem("Log from task A")));
                Task.Factory.StartNew(() => _queue.Enqueue(new LogItem("Log from task B")));
                Task.Factory.StartNew(() => _queue.Enqueue(new LogItem("Log from task B1")));
                Task.Factory.StartNew(() => _queue.Enqueue(new LogItem("Log from task C")));
                Task.Factory.StartNew(() => _queue.Enqueue(new LogItem("Log from task D")));
                items += 5;
                // Pretend to do other things...
                Thread.Sleep(100);
            }

            while (_queue.Count > 0)
            {
                Thread.Sleep(100);
            }

            //_queue.Start();

            //QTest.Invoke(_queue,200);

            watch.Stop();

            Console.WriteLine("Duration: {0}", watch.ElapsedMilliseconds);

            Console.WriteLine("Items per second : {0}", (float)items/(watch.ElapsedMilliseconds / 1000));

            Console.WriteLine("QueueTasksDemo waiting...");

            Console.ReadKey();

            _queue.Stop();

            Console.WriteLine("QueueTasksDemo finished...");

            Console.ReadKey();
        }

        static void MessageReceivedAction<T>(LogItem e)
        {
            Console.WriteLine("<{0}>  {1}", Thread.CurrentThread.ManagedThreadId, e.Message);
        }

        static void _queue_MessageReceived(object sender, Nistec.Generic.GenericEventArgs<LogItem> e)
        {
            Console.WriteLine(e.Args.Message);
        }
    }

   
}
