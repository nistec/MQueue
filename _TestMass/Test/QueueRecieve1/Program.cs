using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MControl.Messaging;

namespace QueueRecieve1
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

            string s = RemoteManager.Client.Reply("test");
                Console.Write(s);
            McChannelProperties prop1 = new McChannelProperties("NC_Quick");
            McChannelProperties prop2 = new McChannelProperties("NC_Bulk");
            prop1.RecieveTimeout = 60;
            prop2.RecieveTimeout = 60;

            ChannelReciever cr1 = new ChannelReciever(prop1);
            ChannelReciever cr2 = new ChannelReciever(prop2);
            cr1.StartAsyncQueue();
            cr2.StartAsyncQueue();

            //RemoteChannelManager.Instance.AddChannel(cr1);
            //RemoteChannelManager.Instance.AddChannel(cr2);
            //RemoteChannelManager.Instance.StartAsyncManager();

            //McThredSettings ts = new McThredSettings(1, 10, 5, true);
            //ts.RecieveTimeout = 30;
            //RemoteChannels rcs = new RemoteChannels(ts);
            //rcs.AddChannel(cr1);
            //rcs.AddChannel(cr2);
            //rcs.StartAsyncManager();

            //QueueReciever qr = new QueueReciever("NC_Quick");//args[0]);//"NC_Quick");
            //qr.Start();
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
            }

            
            void rque_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
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

   
            private void Listner()
            {
                DateTime start = DateTime.Now;

                Console.WriteLine("Queue Reciever {0} Started", rque.QueueName);
                int index = 0;
                while (keepalive)
                {
                    object state = new object();
                    rque.BeginReceive(new TimeSpan(0, 0, 20), state);
                    index++;
                    Thread.Sleep(100);
                    if(index>=1000)
                        break;
                }
                Console.WriteLine("Queue Reciever {0} Stoped", rque.QueueName);
                TimeSpan ts = DateTime.Now.Subtract(start);
                Console.WriteLine("Total secondes={0}", ts.TotalSeconds);
            }

            public void Start()
            {
                keepalive = true;
                Thread th = new Thread(Listner);
                th.IsBackground = true;
                th.Start();
            }

            public void Stop()
            {
                keepalive = false;
            }

  
        }
    }
}
