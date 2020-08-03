using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MControl.Messaging;
using MControl;
using MControl.Generic;

namespace QueueInsert
{
    class Program
    {
        static void Main(string[] args)
        {
            Netlog.Info("test log");

            string qtype = "remote";

            string queueName = "NC_DEMO";

            /*
            RemotingTable rt = new RemotingTable();
            rt.AddTrans("123",new TimeSpan(0,0,30));
            //rt.Push("123", "hello");

            TransItem ti= rt.Pop("123");
            Console.WriteLine(ti.ItemState.ToString());
            //rt.Push("123", "hello");
            Console.WriteLine("trans push");
            */

            //if (args.Length == 0)
            //{
            //    throw new ArgumentNullException("Invalid argument");
            //}


            //McQueueProperties prop = new McQueueProperties(queueName);

            //prop.CoverMode = CoverMode.FileSystem;
            //prop.Server = 0;
            //prop.IsTrans = false;
            //McQueue queue = McQueue.Create(prop);


            if (qtype == "queue")
            {
                McQueue q = new McQueue("demo");
                
            }
            else
            {
                RemoteQueueInserter qi = new RemoteQueueInserter(args);
                while (true)
                {
                    DateTime start = DateTime.Now;
                    qi.Start(1000);
                    TimeSpan ts = DateTime.Now.Subtract(start);
                    Console.WriteLine("Total secondes={0}", ts.TotalSeconds);
                    Thread.Sleep(2000);
                }
            }

            Console.ReadLine();
        }
    }

    public class RemoteQueueInserter
    {
        string[] qList = new string[] { "NC_Quick" };//, "NC_Bulk" };

        public RemoteQueueInserter(string[] args)
        {
            //qList = args[0].Split(',');
        }


        public void Start(int count)
        {
            int qIndex = 0;
            Console.WriteLine("QueueInserter Started...");

            for (int i = 0; i < count; i++)
            {
                QueueInserterWorker(qList, i);
                qIndex = (++qIndex >= qList.Length) ? 0 : qIndex;
            }
        }


        private void QueueInserterWorker(string[] names, int i)
        {
            foreach (string name in names)
            {
                Priority p = (i % 5 == 0) ? Priority.High : (i % 2 == 0) ? Priority.Medium : Priority.Normal;
                QueueItem item = new QueueItem(p);
                item.MessageId = i;
                item.Body = name + " abc this is a test..";
                item.Subject = "test";
                item.Sender = "ibm";
                item.Destination = "nissim";


                RemoteQueue r = new RemoteQueue(name);
                r.Enqueue(item);
                Console.WriteLine("Queue {0} Inserted: {1} ", name, item.ItemId);
            }
            //logger.WriteLoge("Enqueue: " + ItemToString(item), Mode.INFO);
            Thread.Sleep(10);
        }
    }

    public class QueueInserter
    {
        string[] qList = new string[] { "NC_Quick" };//, "NC_Bulk" };

        public QueueInserter(string[] args)
        {
            //qList = args[0].Split(',');
        }


        public void Start(int count)
        {
            int qIndex = 0;
            Console.WriteLine("QueueInserter Started...");

            for (int i = 0; i < count; i++)
            {
                QueueInserterWorker(qList, i);
                qIndex = (++qIndex >= qList.Length) ? 0 : qIndex;
            }
        }


        private void QueueInserterWorker(string[] names, int i)
        {
            foreach (string name in names)
            {
                Priority p = (i % 5 == 0) ? Priority.High : (i % 2 == 0) ? Priority.Medium : Priority.Normal;
                QueueItem item = new QueueItem(p);
                item.MessageId = i;
                item.Body = name + " abc this is a test..";
                item.Subject = "test";
                item.Sender = "ibm";
                item.Destination = "nissim";


                RemoteQueue r = new RemoteQueue(name);
                r.Enqueue(item);
                Console.WriteLine("Queue {0} Inserted: {1} ", name, item.ItemId);
            }
            //logger.WriteLoge("Enqueue: " + ItemToString(item), Mode.INFO);
            Thread.Sleep(10);
        }
    }
}
