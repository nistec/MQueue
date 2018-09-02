using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Nistec.Messaging;
using System.Threading;
//using Nistec.Loggers;


namespace Nistec
{
    class Program
    {
        //const string cnn = @"Data Source=localhost Database=D:\Nistec\Bin_2.4.0\Messaging\References\mcqueuedb;User Id=SYSDBA;Password=masterkey; Connection Timeout=30";
        //const string cnn = @"ServerType=1;User=SYSDBA;Password=masterkey;Database=D:\Nistec\Bin_2.4.0\Messaging\References\mcqueuedb.fdb;Dialect=3";
        const string cnnserver = "ServerType=0;User=SYSDBA;Password=masterkey;Database=mcqueuedb.fdb;Dialect=3";
        //const string cnn = @"ServerType=1;User=SYSDBA;Password=masterkey;Database=mcqueuedb.fdb;Dialect=3";
        //uid=sa;password=tishma; 
        const string cnnt = "Data Source=MCONTROL\\SQLEXPRESS; Initial Catalog=mcqueueDB;Integrated Security=SSPI; Connection Timeout=30";

        const string cnn = "Data Source=mcontrol; Initial Catalog=McQueueDB; uid=sa;password=tishma; Connection Timeout=30";

        //;Nistec.Data.IDBCmd=
       

        //static Logger logger;
        static int counter = 0;

        static void Main(string[] args)
        {

            Nistec.Data.SqlClient.DbSqlCmd cmdb = new Nistec.Data.SqlClient.DbSqlCmd(cnnt);
            DataTable dt = cmdb.ExecuteDataTable("Queues");

            McQueueDemo.RunAsyncQueue();
           


             //logger = Logger.Instance;
             //RunAsyncQueue();
             //RunRemoteQueue();
             //RunRemoteChannel();
             //RunQueueTest();

             Console.WriteLine("finished...");
            Console.ReadLine();

   
        }

        static void toh_TimeoutOccured(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
            Console.WriteLine(sender.ToString());
        }

        //=================== Async Queue =====================================

        //static RemoteQueue rq;
        //static IAsyncQueue queue;
        //static ManualResetEvent signal = new ManualResetEvent(false);
        static string qnmae = "Cellcom";
        private static void RunAsyncQueue()
        {

            DateTime start = DateTime.Now;
            McQueueProperties prop = new McQueueProperties(qnmae);

            //prop.ConnectionString = cnn;
            prop.CoverMode = CoverMode.FileSystem;//.ItemsAndLog;
            prop.Server = 0;
            //prop.Provider = QueueProvider.SqlServer;
            prop.IsTrans = true;
            McQueue queue = McQueue.Create(prop);
            //queue.ReceiveCompleted += new ReceiveCompletedEventHandler(q_DequeueCompleted);
            IQueueItem[] items = CreateItems(3, queue);

            int count = queue.Count;

            Console.WriteLine("start enqueue");

            //CreateItems(10,queue);

            count = queue.Count;
            int index = 0;
            Console.WriteLine("count:{0}", count);

            queue.Enqueue(items[0]);
            queue.Enqueue(items[1]);
            queue.Enqueue(items[2]);

            //IQueueItem item= queue.Dequeue();
            //queue.CommitTrans(item);

           
            while (true)//index < count)
            {
                 queue.BeginReceive(null);
                //queue.BeginReceive(TimeSpan.FromSeconds(10.0), 0,
                //    new AsyncCallback(MsgReceiveCompleted));
                index++;
                Thread.Sleep(10);
            }
            TimeSpan ts=DateTime.Now.Subtract(start);
            Console.WriteLine("took:{0}",ts.TotalSeconds);
            //signal.WaitOne();

        }

    
        // Provides an event handler for the ReceiveCompleted event.
        private static void MsgReceiveCompleted(IAsyncResult asyncResult)
        {
            // Connect to the queue.
            McQueue queue = McQueue.Create(qnmae);

            // End the asynchronous receive operation.
            IQueueItem item = queue.EndReceive(asyncResult);
            Console.WriteLine("Message received: {0}", item.ItemId);
            TransWorker(item);
        }

        private static void TransWorker(IQueueItem item)
        {
            McQueue queue = McQueue.Create(qnmae);
            //queue.CommitTrans(item);
            queue.AbortTrans(item.ItemId,item.HasAttach);
            Console.WriteLine("Commit : {0}", item.ItemId);
        }

        static void q_DequeueCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            // Connect to the queue.
            McQueue mq = (McQueue)sender;

            // End the asynchronous receive operation.
            IQueueItem item = mq.EndReceive(e.AsyncResult);

            Console.WriteLine(item.ItemId.ToString());
            TransWorker(item);

            //count += 1;
            //if (count == 10)
            //{
            //    signal.Set();
            //}
            //signal.Set();
            // Restart the asynchronous receive operation.
            //mq.BeginReceive();

        }

        //=================== End Async Queue =====================================

        //=================== Queue =====================================

        //static RemoteQueue rq;
        //static IAsyncQueue queue;
        private static void RunQueueTest()
        {
            McQueueProperties prop = new McQueueProperties("Cellcom");
            
            //prop.ConnectionString = cnn;
            prop.CoverMode = CoverMode.FileSystem;//.ItemsAndLog;
            prop.Server = 0;
            //prop.Provider = QueueProvider.SqlServer;

            RemoteQueue rq = new RemoteQueue(prop.QueueName);
            Console.WriteLine(rq.Reply("test"));
            
            IQueueItem[] items = CreateItems(3,rq);


            RemoteQueue rq2 = new RemoteQueue(prop.QueueName);//"Cellcom");//RemoteQueue.Instance;
            Console.WriteLine(rq2.Reply("test"));

            IQueueItem[] itemsb = CreateItems(3,rq);

            rq2.Enqueue(itemsb[0]);
            rq2.Enqueue(itemsb[1]);
            rq2.Enqueue(itemsb[2]);

            int count = rq2.Count;

            DataTable dt2 = rq.GetQueueItemsTable();
            count = dt2.Rows.Count;


            int countq = rq.Count;
            Console.WriteLine(countq.ToString());


            rq.Enqueue(items[0]);
            rq.Enqueue(items[1]);
            rq.Enqueue(items[2]);
            countq = rq.Count;
            Console.WriteLine(countq.ToString());

            //DataTable dt = McQueue.Queues.GetAllCoverItems();// GetAllItems();//QueueProvider.Embedded, null);
            //     string fileName = Environment.CurrentDirectory + "\\McQueue.xml";
            //    dt.WriteXml(fileName);
    
            
            Thread.Sleep(100);
            IQueueItem[] items2 = new IQueueItem[3];
            int intout=0;
            while (intout < 2)
            {
                IQueueItem it= rq.Dequeue();
                if(it!=null)
                {
                    items2[intout] = it;
                }
                countq=rq.Count;
                Console.WriteLine(countq.ToString());
                Thread.Sleep(100);
            }
        }

        private static void AsyncDequeue()
        {
            while (true)
            {
                //if (counter >= 100)
                //{
                RemoteQueue q = new RemoteQueue("Cellcom");

                IQueueItem item = q.Dequeue();
                if (item != null)
                {
                    //logger.WriteLoge("Dequeue: " + item.Priority.ToString());
                    //logger.WriteLoge("Dequeue: " + ItemToString(item), Mode.INFO);
                    //Console.WriteLine(item.Priority.ToString());
                    //Console.WriteLine(item.ToString());
                }
                //}
                Thread.Sleep(100);
            }
        }
        //=================== End Queue =====================================
        

        //=================== Remote Channel =====================================
        static McChannel channel;
        private static void RunRemoteChannel()
        {
            McChannelProperties prop = new McChannelProperties("Cellcom");
            prop.MaxThread = 20;
            prop.MinThread = 2;
            prop.AutoThreadSettings = true;
            prop.QueueName = "Cellcom";
            //prop.ConnectionString = cnn;
            prop.CoverMode = CoverMode.FileSystem;//.ItemsAndLog;
            prop.Server = 0;
            //prop.Provider = QueueProvider.SqlServer;

            channel = new McChannel(prop,true);
            //channel.ReceiveCompleted += new QueueItemEventHandler(channel_ReceiveCompleted);

            RunThreads(new ThreadStart(AsyncChannelWorker),1);
        }

        static void channel_ReceiveCompleted(object sender, QueueItemEventArgs e)
        {
            Console.WriteLine("ReceiveCompleted: " + e.Item.ItemId);
        }

        private static void AsyncChannelWorker()
        {
            for (int i = 0; i <= 10000; i++)
            {
                Priority p = (i % 5 == 0) ? Priority.High : (i % 2 == 0) ? Priority.Medium : Priority.Normal;
                QueueItem item = new QueueItem(p);
                item.MessageId = i;
                item.Body = "this is a test  אכן זוהי דוגמא";
                item.Subject = "test";
                item.Sender = "ibm";
                item.Destination = "nissim";
                channel.Enqueue(item);
                //logger.WriteLoge("Enqueue: " + ItemToString(item), Mode.INFO);
                counter = i;
            }
        }

        private static IQueueItem[] CreateItems(int count,IAsyncQueue q)
        {
            IQueueItem[] items = new IQueueItem[count];

            for (int i = 0; i < count; i++)
            {
                Priority p = (i % 5 == 0) ? Priority.High : (i % 2 == 0) ? Priority.Medium : Priority.Normal;
                QueueItem item = new QueueItem(p);
                item.MessageId = i;
                item.Body = "this is a test  אכן זוהי דוגמא";
                item.Subject = "test";
                item.Sender = "ibm";
                item.Destination = "nissim";
                q.Enqueue(item);
                items[i] = item;
                //logger.WriteLoge("Enqueue: " + ItemToString(item), Mode.INFO);
                counter = i;
            }
            return items;
        }
        //=================== End Remote Channel =====================================


        //=================== Remote Queue =====================================

        static RemoteQueue rq;
        static RemoteQueue rqc;
        private static void RunRemoteQueue()
        {
            McQueueProperties prop = new McQueueProperties("NC_Quick");

            //prop.ConnectionString = cnn;
            prop.CoverMode = CoverMode.FileSystem;//.ItemsAndLog;
            prop.Server = 0;
            //prop.Provider = QueueProvider.SqlServer;
            prop.ReloadOnStart = true;
            Console.WriteLine("create remote queue");
            RemoteQueue rque = RemoteManager.Create(prop);
            //rque.ReceiveCompleted += new ReceiveCompletedEventHandler(rq_ReceiveCompleted);
            
            
            //RemoteQueue.AddQueue(prop);
            Console.WriteLine("rempote queue created");
            rque.MessageArraived += new QueueItemEventHandler(rq_MessageArraived);
            rque.MessageReceived += new QueueItemEventHandler(rq_MessageReceived);
            //Console.WriteLine(rqc.Reply("test"));

            //AsyncManagerWorker();
            RunThreads(new ThreadStart(AsyncRemoteDequeue), 10);
            RunThreads(new ThreadStart(AsyncManagerWorker),10);

            while (true)
            {
                IAsyncResult result = rque.BeginReceive(null);
                Console.WriteLine("Count: {0}", rque.Count);
                Thread.Sleep(10);
            }
            Console.ReadLine();
        }

        static void rq_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            IReceiveCompleted mq = (IReceiveCompleted)sender;

            // End the asynchronous receive operation.
            IQueueItem item = mq.EndReceive(e.AsyncResult);

            Console.WriteLine(item.ItemId.ToString());
        }

        static void rq_MessageReceived(object sender, QueueItemEventArgs e)
        {
            IAsyncQueue rq = (IAsyncQueue)sender;
            rq.Completed(e.Item.ItemId,(int) ItemState.Commit, e.Item.HasAttach);
            Console.WriteLine("MessageReceived: " + e.Item.ItemId);
        }

        static void rq_MessageArraived(object sender, QueueItemEventArgs e)
        {
            IAsyncQueue rq = (IAsyncQueue)sender;
            IQueueItem item= rq.Dequeue();
            Console.WriteLine("MessageArraived: " + item.ItemId);
        }

        private static void AsyncManagerWorker()
        {
            RemoteQueue r = new RemoteQueue("NC_Quick");
            int count = 10000;
            for (int i = 0; i <= count; i++)
            {
                Priority p = (i % 5 == 0) ? Priority.High : (i % 2 == 0) ? Priority.Medium : Priority.Normal;
                QueueItem item = new QueueItem(p);
                item.MessageId = i;
                item.Body = "this is a test  אכן זוהי דוגמא";
                item.Subject = "test";
                item.Sender = "ibm";
                item.Destination = "nissim";
                //RemoteQueue q = new RemoteQueue("Cellcom");
                r.Enqueue(item);
                //logger.WriteLoge("Enqueue: " + ItemToString(item), Mode.INFO);
                counter = i;
            }
        }

        private static void AsyncRemoteDequeue()
        {
            while (true)
            {
                RemoteQueue q = new RemoteQueue("Cellcom");

                IQueueItem item = q.Dequeue();
                if (item != null)
                {
                    Console.WriteLine("Dequeue: " + ItemToString(item));
                    //logger.WriteLoge("Dequeue: " + ItemToString(item), Mode.INFO);
                    rq_MessageReceived(q,new QueueItemEventArgs(item, ItemState.Commit));
                }

                Console.WriteLine("Count: " + q.Count);

                //}
                Thread.Sleep(100);
            }
        }

       
        //=================== End Remote Queue =====================================


        //=================== Common =====================================


        private static void RunThreads(ThreadStart worker, int maxThread)
        {
            //const int maxThread=1;
            Thread[] th = new Thread[maxThread];
            for (int i = 0; i < maxThread; i++)
            {
                th[i] = new Thread(worker);
                th[i].Start();
            }

        }

        //static void cache_SyncTimeStart(object sender, EventArgs e)
        //{
        //    object o = sender;
        //    int one = (int)cache.GetItem("one").Value;
        //    int two = (int)cache.GetItem("two").Value;
        //    cache["one"] = one += 1;
        //    cache["two"] = two += 2;
          
        //}

        //static void cache_SyncCacheSource(object sender, Nistec.Caching.SyncCacheEventArgs e)
        //{
        //    string s=e.SourceName;
        //}

        //static void cache_CacheStateChanged(object sender, EventArgs e)
        //{
        //    object o = sender;
        //}


        //private static IQueueItem[] CreateItems(int count)
        //{
        //    IQueueItem[] items = new IQueueItem[count];

        //    for (int i = 0; i < count; i++)
        //    {
        //        items[i] = new QueueItem(Priority.Normal);
        //        items[0].Body = "test";
        //    }
        //    return items;
        //}

  
        static void queueHandler_ReceiveCompleted(object sender, QueueItemEventArgs e)
        {
            //logger.WriteLoge("Completed: " + ItemToString(e.Item), Mode.INFO);
            //string s = e.Item.Sender;
            //int res = worker();
            //TestObeectToXml(e.Item as QueueItem);
            //Thread.Sleep(1000);
        }

        private static string ItemToString(IQueueItem item)
        {
            return string.Format("{0}:{1}:{2}:{3}",item.ItemId,item.Priority,item.MessageId,item.Status);
        }
    }
}
