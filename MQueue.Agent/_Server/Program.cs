//#define SERVICE

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Configuration.Install;
using System.ServiceProcess;
using Nistec.Messaging;
using Nistec.Messaging.Server;

namespace Nistec.Queue.Service
{
    class Program
    {
 #if (SERVICE)

        //static void Main(string[] args)
        //{
        //    System.ServiceProcess.ServiceBase[] ServicesToRun;
        //    ServicesToRun = new System.ServiceProcess.ServiceBase[] { new RemoteQueueService() };
        //    System.ServiceProcess.ServiceBase.Run(ServicesToRun);
        //}


        const string ServiceProcess = "MQueue.Service.exe";
        const string ServiceName = "Nistec.Queue";


            /// <summary>
            /// Application main entry point.
            /// </summary>
            /// <param name="args">Command line argumnets.</param>
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower() == "-install")
            {
                ManagedInstallerClass.InstallHelper(new string[] { ServiceProcess });

                ServiceController c = new ServiceController(ServiceName);
                c.Start();
            }
            else if (args.Length > 0 && args[0].ToLower() == "-uninstall")
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", ServiceProcess });
            }
            else
            {
                System.ServiceProcess.ServiceBase[] servicesToRun = new System.ServiceProcess.ServiceBase[] { new RemoteQueueService() };
                System.ServiceProcess.ServiceBase.Run(servicesToRun);
            }
        }

#else
        static void Main(string[] args)
        {
 
            //string s=ServiceConfig.ConnectionString;

            ServerManager svr = new ServerManager();
            svr.Start();

            Thread.Sleep(10000);

            Console.ReadKey();

            /*
            string[] list = AgentManager.Queue.GetQueueList();
            foreach (string s in list)
            {
                Console.WriteLine(s);
            }

            Priority p = Priority.Normal;
            QueueItem item = new QueueItem();
            item.MessageId = 0;
            item.Body = "name" + " abc this is a test..";
            item.Subject = "test";
            item.Sender = "ibm";
            item.Destination = "nissim";

            AgentManager.Queue.Get("NC_Quick").Enqueue(item);
           */

          


            //const string cnn = "Data Source=mcontrol; Initial Catalog=McQueueDB; uid=sa;password=tishma; Connection Timeout=30";

            //const string cnn = @"ServerType=1;User=SYSDBA;Password=masterkey;Database=mcqueuedb.fdb;Dialect=3";

            //RunRemoteQueue();
            //RunAsyncQueue();
 

            Console.ReadLine();
        }
 

        //=================== Async Queue =====================================
        /*
        //static RemoteQueue rq;
        //static IAsyncQueue queue;
        //static ManualResetEvent signal = new ManualResetEvent(false);
        static string qnmae = "Cellcom";
        private static void RunAsyncQueue()
        {
            McQueueProperties prop = new McQueueProperties(qnmae);

            prop.ConnectionString = cnn;
            prop.CoverMode = CoverMode.ItemsAndLog;
            prop.Server = 0;
            prop.Provider = QueueProvider.SqlServer;
            RemoteQueueManager.AddQueue(prop);
            RemoteQueue rq= new RemoteQueue(prop.QueueName);
            rq.ReceiveCompleted += new ReceiveCompletedEventHandler(q_DequeueCompleted);


            IQueueItem[] items = CreateItems(1000);

            Console.WriteLine("start enqueue");
            foreach (IQueueItem item in items)
            {
                rq.Enqueue(item);
            }

            while (true)
            {
                //queue.BeginReceive();
                rq.BeginReceive(TimeSpan.FromSeconds(10.0), 0,
                    new AsyncCallback(MsgReceiveCompleted));

                Thread.Sleep(100);
            }

            //signal.WaitOne();

            Console.ReadLine();
        }

        // Provides an event handler for the ReceiveCompleted event.
        private static void MsgReceiveCompleted(IAsyncResult asyncResult)
        {
            // Connect to the queue.
            //McQueue queue = McQueue.Create(qnmae);
            RemoteQueue rq = new RemoteQueue(qnmae);
            // End the asynchronous receive operation.
            IQueueItem msg = rq.EndReceive(asyncResult);

            Console.WriteLine("Message body: {0}", (string)msg.Body);
        }


        static void q_DequeueCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            // Connect to the queue.
            IReceiveCompleted mq = (IReceiveCompleted)sender;

            // End the asynchronous receive operation.
            IQueueItem item = mq.EndReceive(e.AsyncResult);

            Console.WriteLine(item.ItemId.ToString());

            //count += 1;
            //if (count == 10)
            //{
            //    signal.Set();
            //}
            //signal.Set();
            // Restart the asynchronous receive operation.
            //mq.BeginReceive();

        }
        */
        //=================== End Async Queue =====================================
        /*
        private static void test()
        {
            McQueueProperties prop = new McQueueProperties("Cellcom");
            prop.ConnectionString = cnn;
            //prop.MaxThread = 1;
            prop.CoverMode = CoverMode.ItemsAndLog;
            //prop.Enabled = true;
            prop.Server = 0;
            prop.Provider = QueueProvider.SqlServer;
            //prop.Mode = QueueMode.Manual;

            RemoteQueueManager.AddQueue(prop);

            RemoteQueue rq = new RemoteQueue("Cellcom");
            Console.WriteLine(rq.Reply("test"));

            int countq = rq.Count;

            IQueueItem[] items = new IQueueItem[3];
            items[0] = new QueueItem(Priority.Normal);
            items[0].Body = "test";
            items[1] = new QueueItem(Priority.Normal);
            items[1].Body = "test";
            items[2] = new QueueItem(Priority.Normal);
            items[2].Body = "test";


            rq.Enqueue(items[0]);
            rq.Enqueue(items[1]);
            rq.Enqueue(items[2]);

            countq = rq.Count;

            System.Threading.Thread.Sleep(100);
            IQueueItem[] items2 = new IQueueItem[3];
            int intout = 0;
            while (intout < 2)
            {
                IQueueItem it = rq.Dequeue();
                if (it != null)
                {
                    items2[intout] = it;
                    intout++;
                }
                countq = rq.Count;
                Console.WriteLine(countq.ToString());
                System.Threading.Thread.Sleep(100);
            }
        }
        */
        //=================== Remote Queue =====================================
        /*
        static RemoteQueue rq;
        const string cnn = "Data Source=mcontrol; Initial Catalog=McQueueDB; uid=sa;password=tishma; Connection Timeout=30";

        private static void RunRemoteQueue()
        {
            McQueueProperties prop = new McQueueProperties("Orange");// ("Cellcom");

            prop.ConnectionString = cnn;
            prop.CoverMode = CoverMode.ItemsAndLog;
            prop.Server = 0;
            prop.Provider = QueueProvider.SqlServer;
            Console.WriteLine("create remote queue");
            rq = new RemoteQueue(prop.QueueName);
            RemoteQueueManager.AddQueue(prop);
            rq.ClearFinallItems(QueueItemType.HoldItems);
            Console.WriteLine("remote queue created");
            //rq.MessageArraived += new QueueItemEventHandler(rq_MessageArraived);
            //rq.MessageReceived += new QueueItemEventHandler(rq_MessageReceived);
            Console.WriteLine(rq.Reply("test"));

            Console.WriteLine("Count: " + rq.Count);

            RunThreads(new ThreadStart(AsyncRemoteDequeue), 1);
            RunThreads(new ThreadStart(AsyncManagerWorker),1);

            
        }

        static void rq_MessageReceived(object sender, QueueItemEventArgs e)
        {
            //RemoteQueueClient q = new RemoteQueueClient("Cellcom");
            rq.Completed(e.Item.ItemId, (int)ItemState.Commit,false);
            Console.WriteLine("MessageReceived: " + e.Item.ItemId);
        }

        static void rq_MessageArraived(object sender, QueueItemEventArgs e)
        {
            //RemoteQueueClient q = new RemoteQueueClient("Cellcom");
            IQueueItem item = rq.Dequeue();
            Console.WriteLine("MessageArraived: " + item.ItemId);
        }

        private static void AsyncManagerWorker()
        {
            for (int i = 0; i <= 1; i++)
            {
                Priority p = (i % 5 == 0) ? Priority.High : (i % 2 == 0) ? Priority.Medium : Priority.Normal;
                QueueItem item = new QueueItem(p);
                item.MessageId = i;
                item.Body = "this is a test  אכן זוהי דוגמא";
                item.Subject = "test";
                item.Sender = "ibm";
                item.Destination = "nissim";
                //RemoteQueueClient q = new RemoteQueueClient("Cellcom");
                rq.Enqueue(item);
                //logger.WriteLoge("Enqueue: " + ItemToString(item), Mode.INFO);
                //counter = i;
            }
        }

        private static void AsyncRemoteDequeue()
        {
            while (true)
            {
                //RemoteQueueClient q = new RemoteQueueClient("Cellcom");

                IQueueItem item = rq.Dequeue();
                if (item != null)
                {
                    Console.WriteLine("Dequeue: " + ItemToString(item));
                    //logger.WriteLoge("Dequeue: " + ItemToString(item), Mode.INFO);
                    rq_MessageReceived(rq, new QueueItemEventArgs(item, ItemState.Commit));
                }

                Console.WriteLine("Count: " + rq.Count);

                //}
                Thread.Sleep(100);
            }
        }
         */ 
        //=================== End Remote Queue =====================================


        //=================== Common =====================================


        //private static void RunThreads(ThreadStart worker, int maxThread)
        //{
        //    //const int maxThread=1;
        //    Thread[] th = new Thread[maxThread];
        //    for (int i = 0; i < maxThread; i++)
        //    {
        //        th[i] = new Thread(worker);
        //        th[i].Start();
        //    }

        //}
        //private static IQueueItem[] CreateItems(int count)
        //{
        //    QueueItem[] items = new QueueItem[count];

        //    //for (int i = 0; i < count; i++)
        //    //{
        //    //    items[i] = new QueueItem();
        //    //    items[0].SetBodyStream("test");
        //    //}
        //    return items;
        //}
        //private static string ItemToString(IQueueItem item)
        //{
        //    return string.Format("{0}:{1}:{2}:{3}", item.ItemId, item.Priority, item.MessageId, item.Status);
        //}

#endif

    }
}
