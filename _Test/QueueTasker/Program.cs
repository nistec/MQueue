﻿using Nistec;
using Nistec.Data.Entities;
using Nistec.Messaging;
using Nistec.Messaging.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueTasker
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("QueueTasker started...");
            int items = 0;
            string k = null;
            do
            {
                Console.WriteLine("enter number of items to run");
                k = Console.ReadLine();

                if (k != "quit")
                {
                    items = Types.ToInt(k);
                    Console.WriteLine("items to run {0}", items);

                    if (items > 0)
                        QueueClientDemo.PublishMulti(items);
                }
            } while (k != "quit");

            //QueueClientDemo.PublishItem();

            //QueueClientDemo.SendItem(false);
            //QueueClientDemo.SendMulti(false,10000);

            //var host = QueueHost.Parse("tcp:127.0.0.1:15000?NC_Quick");
            //QueueApi q = new QueueApi(host);
            //q.IsAsync = false;

            //SendItem(q,1);

            //SendMulti(q,100);

            Console.ReadLine();
            Console.WriteLine("QueueTasker finished...");

        }

        static void SendItem(QueueApi q, long item)
        {

            DateTime start = DateTime.Now;
            QueueMessage msg = new QueueMessage();
            msg.SetBody("Hello world " + DateTime.Now.ToString("s"));
            if (q.IsAsync)
            {
                q.SendAsync(msg, 50000000, (ack) =>
                {
                    Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item);
                });
            }
            else
            {
                var ack = q.Enqueue(msg, 50000000);
                Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item);
            }
            //var duration = DateTime.Now.Subtract(start);
            //var milliseconds = duration.TotalMilliseconds;
            //Console.WriteLine("duration: {0}, item: {1}", milliseconds, item);
        }
        static void SendMulti(QueueApi q, int maxItems)
        {
            long counter = 0;
            int interval = 1;
            DateTime start = DateTime.Now;

            for (int i = 0; i < maxItems; i++)
            {
                //Task.Factory.StartNew(() => SendItem(q, i));

                QueueMessage msg = new QueueMessage();
                msg.Label = i.ToString();
                msg.SetBody("Hello world " + DateTime.Now.ToString("s"));

                Task.Factory.StartNew(() =>
                {
                    
                    if (q.IsAsync)
                    {
                        q.SendAsync(msg, 50000000, (ack) =>
                        {
                            Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration );
                            Interlocked.Increment(ref counter);
                        });
                    }
                    else
                    {
                        var ack = q.Enqueue(msg, 50000000);
                        Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration);
                        Interlocked.Increment(ref counter);
                    }
                });
                //Thread.Sleep(interval);
            }
            
            while(Interlocked.Read(ref counter) < (maxItems))
            {
                Thread.Sleep(interval);
            }


            //while (true)
            //{
            //    if (counter > maxItems)
            //        break;
            //    Interlocked.Increment(ref counter);

            //    Task.Factory.StartNew(() => SendItem(q, Interlocked.Read(ref counter)));

            //    //SendItem(q,counter);
                
            //    //QueueMessage msg = new QueueMessage();
            //    //msg.SetBodyText("Hello world " + DateTime.Now.ToString("s"));
            //    //q.SendAsync(msg, 5000, (ack) =>
            //    //{
            //    //    Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier);
            //    //});

            //    //Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}", ack.MessageState,ack.Creation,ack.Host,ack.Label, ack.Identifier);

            //    //counter++;
            //    Thread.Sleep(interval);
            //}

            var duration = DateTime.Now.Subtract(start);
            var milliseconds = duration.TotalMilliseconds;
            Console.WriteLine("duration: {0}, count: {1}, itemDuration: {2}", milliseconds - (interval * counter), counter, (milliseconds - (interval * counter)) / counter);
           
        }

    }
}
