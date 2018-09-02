using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Nistec;
using Nistec.Messaging;
using Nistec.Messaging.Remote;

namespace QueueTest
{
    public class McQueueDemo
    {
        const string cnn = "Data Source=mcontrol; Initial Catalog=McQueueDB; uid=sa;password=tishma; Connection Timeout=30";

        //static RemoteQueue rq;
        //static IAsyncQueue queue;
        //static ManualResetEvent signal = new ManualResetEvent(false);
        static string qnmae = "Demo";
        static McQueue queue;

        public static void RunAsyncQueue()
        {

            DateTime start = DateTime.Now;

            QueueClient client = new QueueClient();

            McQueueProperties prop = new McQueueProperties(qnmae);

            //prop.ConnectionString = cnn;
            //prop.CoverMode = CoverMode.ItemsAndLog;
            //prop.Server = 0;
            //prop.Provider = QueueProvider.SqlServer;
            //prop.IsTrans = true;
            queue = McQueue.Create(prop);
            //queue.ReceiveCompleted += new ReceiveCompletedEventHandler(q_DequeueCompleted);


            Console.WriteLine("start enqueue");
            QueueInserter.InsertItems(100, queue);
            int count = queue.Count;
            Console.WriteLine("count:{0}", count);

            QueueListner();

            
            int index = 0;

          
            while (true)//index < count)
            {
                queue.BeginReceive(null);
                //queue.BeginReceive(TimeSpan.FromSeconds(10.0), 0,
                //    new AsyncCallback(MsgReceiveCompleted));
                index++;
                Thread.Sleep(10);
            }
            TimeSpan ts = DateTime.Now.Subtract(start);
            Console.WriteLine("took:{0}", ts.TotalSeconds);
            //signal.WaitOne();

        }

        private static void OnMessageReceived(IQueueItem item)
        {
            if (item == null)
                return;
            Console.WriteLine("Queue{0} Items count: {1}", queue.QueueName, queue.Count);
            if (item != null)
            {
                Console.WriteLine("Queue{0} ReceiveCompleted: {1}", queue.QueueName, item.ItemId);
            }
            else
            {
                Console.WriteLine("Queue{0} Receive timeout", queue.QueueName);
            }

            Netlog.InfoFormat("OnMessageReceived:{0} Received:{1}", item.MessageId, item.SentTime);

        }

        private static void QueueListner()
        {
            while (true)//index < count)
            {
                IQueueItem item= queue.Dequeue();
                OnMessageReceived(item);
                Thread.Sleep(10);
            }
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
            queue.AbortTrans(item.ItemId, item.HasAttach);
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

    }
}
