using Nistec.IO;
using Nistec.Messaging;
using Nistec.Messaging.Remote;
using Nistec.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueTasker
{
    public class QueueClient
    {

        public static QueueHost GetHost(string host_address, string queueName) {

            //"tcp:127.0.0.1:15000?NC_Quick"
            var host = QueueHost.Parse("tcp:127.0.0.1:15000?"+ queueName);
            return host;
        }
        public static QueueHost GetHost(string protocol, string host_address, string queueName)
        {
            //127.0.0.1:15000
            var host = QueueHost.Parse(protocol + ":" + host_address + "?" + queueName);
            return host;
        }

        public static QueueApi GetApi(string queueName, string hostAddress)
        {
            QueueApi q = new QueueApi(queueName, hostAddress);
            q.IsAsync = false;
            return q;
        }
        public static QueueApi GetApi(string queueName, HostProtocol protocol, string endpoint, int port, string hostName)
        {
            QueueApi q = new QueueApi(queueName, protocol, endpoint, port, hostName);
            q.IsAsync = false;
            return q;
        }

        public static QueueApi GetApi(QueueHost host)
        {
            QueueApi q = new QueueApi(host);
            q.IsAsync = false;
            return q;
        }

        public static QueueItem CreateQueueItem(string body, string label) {
            QueueItem msg = new QueueItem();
            msg.SetBody(body);
            msg.Label = label;

            return msg;
        }

        public static QueueItem CreateQueueItem(string body, string label, Priority priority, string id)
        {
            QueueItem msg = new QueueItem();
            msg.SetBody(body);

            msg.Label = label;
            msg.MessageType = MQTypes.Message;
            msg.Priority = priority;
            msg.QCommand = QueueCmd.Enqueue;
            //msg.Identifier= identifier;
            msg.Id = id;
            //msg.GroupId = groupId;

            return msg;
        }


        public static QueueRequest GetQRequest(QueueCmd command,int version, Priority priority, TransformType transformType, string host, NetStream bodyStream)
        {
            return new QueueRequest()
            {
                //Version = version,
                //MessageType = messageType,
                QCommand = command,
                Priority = priority,
                TransformType = transformType,
                Host = host,
                //Creation = DateTime.Now,
                //Modified = DateTime.Now,
                //ArrivedTime = Assists.NullDate,
                BodyStream = bodyStream
            };
        }


        public static IQueueAck SendItem(QueueApi q, QueueItem item, int connectTimeOut)
        {
            var ack = q.Send(item, connectTimeOut);
            //Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item);
            return ack;
        }
        public static IQueueAck SendItemAsync(QueueApi q, QueueItem item, int connectTimeOut)
        {
            return q.SendAsync(item, connectTimeOut);
        }

        public static void SendItemAsync(QueueApi q, QueueItem item, int connectTimeOut, Action<IQueueAck> action)
        {

            q.SendAsync(item, connectTimeOut, action);

            //DateTime start = DateTime.Now;
            //q.SendAsync(item, connectTimeOut, (ack) =>
            //{
            //    //Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item);
            //});
        }

    }

    public static class QueueClientDemo {

       public static void SendItem(bool isAsync)
        {
            var host = QueueHost.Parse("tcp:127.0.0.1:15000?NC_Quick");
            QueueApi q = QueueClient.GetApi(host);
            var item = QueueClient.CreateQueueItem("Hello world " + DateTime.Now.ToString("s"), "test");
            IQueueAck ack = null;

            if (q.IsAsync)
                ack = QueueClient.SendItemAsync(q, item, 0);
            else
                ack = QueueClient.SendItem(q, item, 0);

            Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item.Id);

            //var duration = DateTime.Now.Subtract(start);
            //var milliseconds = duration.TotalMilliseconds;
            //Console.WriteLine("duration: {0}, item: {1}", milliseconds, item);
        }

       public static void SendMulti(bool isAsync, int maxItems)
        {
            long counter = 0;
            int interval = 1;
            DateTime start = DateTime.Now;

            var host = QueueHost.Parse("tcp:127.0.0.1:15000?NC_Bulk");
            QueueApi q = QueueClient.GetApi(host);
            //IQueueAck ack = null;


            for (int i = 0; i < maxItems; i++)
            {

                var item = QueueClient.CreateQueueItem("Hello world " + DateTime.Now.ToString("s"), i.ToString());

                Task.Factory.StartNew(() =>
                {

                    if (isAsync)
                    {
                        q.SendAsync(item, 50000000, (ack) =>
                        {
                            Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration);
                            Interlocked.Increment(ref counter);
                        });
                    }
                    else
                    {
                        var ack = q.Send(item, 50000000);
                        Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration);
                        Interlocked.Increment(ref counter);
                    }
                    
                });
                //Thread.Sleep(interval);
            }

            while (Interlocked.Read(ref counter) < (maxItems))
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

            //    //QueueItem msg = new QueueItem();
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
