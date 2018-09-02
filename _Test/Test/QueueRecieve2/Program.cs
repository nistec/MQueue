using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MControl.Messaging;

namespace QueueRecieve2
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length == 0)
            //{
            //    throw new ArgumentNullException("Invalid argument");
            //}
            Console.WriteLine("Start Queue Reciever...");


            McChannelProperties prop1 = new McChannelProperties("NC_Quick");
            //McChannelProperties prop2 = new McChannelProperties("NC_Bulk");
            prop1.RecieveTimeout = 60;
            prop1.MaxThread = 5;
            prop1.AvailableThread = 5;
            prop1.IsTrans = false;
            prop1.UseMessageQueueListner = true;
            Channel cr1 = new Channel(prop1);

            cr1.start = DateTime.Now;
            cr1.StartAsyncQueue();


            //ChannelReciever cr2 = new ChannelReciever(prop2);
            //cr2.StartAsyncQueue();

/*
            QueueReciever qr = new QueueReciever("NC_Quick");//args[0]);//"NC_Quick");
            qr.BeginReceive();
            qr.Start();
*/ 

            Console.ReadLine();
        }

        public class QueueReciever
        {
            bool keepalive = false;
            RemoteQueue rque;

            public QueueReciever(string name)
            {
                rque = RemoteManager.Create(name);
                rque.ReceiveCompleted += new ReceiveCompletedEventHandler(rque_ReceiveCompleted);
                //rque.MessageReceived += new QueueItemEventHandler(rque_MessageReceived);
            }

            public void BeginReceive()
            {
                rque.BeginReceive(new TimeSpan(0, 0, 20), null);
            }

            void rque_MessageReceived(object sender, QueueItemEventArgs e)
            {
                IQueueItem item = e.Item;
                    if (item != null)
                    {
                        Console.WriteLine("Queue{0} ReceiveCompleted: {1}", rque.QueueName, e.Item.ItemId);
                    }
                    else
                    {
                        Console.WriteLine("Queue{0} Receive timeout", rque.QueueName);
                    }
            }


            void rque_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
            {
                IQueueItem item = e.Item;

                try
                {
                    if (item != null)
                    {
                        Console.WriteLine("Queue{0} ReceiveCompleted: {1}, Duration:{2}", rque.QueueName, e.Item.ItemId, e.Item.Duration());
                    }
                    else
                    {
                        Console.WriteLine("Queue{0} Receive timeout", rque.QueueName);
                    }
                    Thread.Sleep(100);
                }
                finally
                {
                    rque.BeginReceive(new TimeSpan(0, 0, 20), null);
                }

            }


            private void Listner()
            {
                DateTime start = DateTime.Now;

                Console.WriteLine("Queue Reciever {0} Started", rque.QueueName);
                int index = 0;
                while (keepalive)
                {
                    object state = new object();
                    rque.Dequeue();//.BeginReceive(new TimeSpan(0, 0, 20), state);
                    index++;
                    Thread.Sleep(100);
                    if (index >= 1000)
                        break;
                }
                Console.WriteLine("Queue Reciever {0} Stoped", rque.QueueName);
                TimeSpan ts = DateTime.Now.Subtract(start);
                Console.WriteLine("Total secondes={0}", ts.TotalSeconds);
            }

            public void Start()
            {
                keepalive = true;
                //Thread th = new Thread(Listner);
                //th.IsBackground = true;
                //th.Start();
            }

            public void Stop()
            {
                keepalive = false;
            }


        }
    }
}
