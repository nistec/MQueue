using Nistec.Channels;
using Nistec.Generic;
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

        public static QueueMessage CreateQueueItem(object body, string label)
        {
            QueueMessage msg = new QueueMessage();
            msg.SetBody(body);
            msg.Label = label;

            return msg;
        }

        public static QueueMessage CreateQueueItem(string body, string label) {
            QueueMessage msg = new QueueMessage();
            msg.SetBody(body);
            msg.Label = label;

            return msg;
        }

        public static QueueMessage CreateQueueItem(string body, string label, Priority priority, string id)
        {
            QueueMessage msg = new QueueMessage();
            msg.SetBody(body);

            msg.Label = label;
            //mqh-msg.MessageType = MQTypes.Message;
            msg.Priority = priority;
            msg.Command = QueueCmd.Enqueue.ToString();
            //msg.Identifier= identifier;
            msg.CustomId = id;
            //msg.GroupId = groupId;

            return msg;
        }

        public static MessageFlex CreateMessage(int i)
        {
            return new MessageFlex()
            {
                Command = "Send",
                CustomId = i.ToString(),
                Message = "<response duration=\"0.0244219303131\" end=\"1276683822.25\" queries=\"15\" start=\"1276683822.23\"><status code=\"1\">DISCARDED</status><message queue_id=\"0\"><status code=\"1\">DISCARDED</status><recipients count=\"1\" successful_count=\"0\"><recipient cli=\"972545650999\" mcc=\"425\" mnc=\"99\"><status code=\"401\">BLACKLISTED</status><reason>NOROUTE</reason></recipient></recipients></message></response> ",
                //Query = @"tel:\*\d{4}|(|\()(0|972)(\d{1}|\d{2})(|[\)\/\.-])([0-9]{7})|(|\()(18|17)00(|[\)\/\.-])[0-9]{3}(|[\)\/\.-])[0-9]{3}$",
                Source = "MsgQueueDemo",
                SessionId = "MongoCommands",
                Label = "QDemo"
            };
        }

        public static QueueMessage CreateItem(int i)
        {
            return new QueueMessage()
            {
                Command = "Send",
                CustomId = i.ToString(),
                Body = NetStream.GetBytes("<response duration=\"0.0244219303131\" end=\"1276683822.25\" queries=\"15\" start=\"1276683822.23\"><status code=\"1\">DISCARDED</status><message queue_id=\"0\"><status code=\"1\">DISCARDED</status><recipients count=\"1\" successful_count=\"0\"><recipient cli=\"972545650999\" mcc=\"425\" mnc=\"99\"><status code=\"401\">BLACKLISTED</status><reason>NOROUTE</reason></recipient></recipients></message></response> "),
                Args = NameValueArgs.Create("Query", @"tel:\*\d{4}|(|\()(0|972)(\d{1}|\d{2})(|[\)\/\.-])([0-9]{7})|(|\()(18|17)00(|[\)\/\.-])[0-9]{3}(|[\)\/\.-])[0-9]{3}$"),
                Source = "MsgQueueDemo",
                SessionId = "MongoCommands",
                Label = "QDemo"
            };

            
        }

        public static QueueRequest GetQRequest(QueueCmd command,int version, Priority priority, TransformType transformType, string host, NetStream bodyStream)
        {
            return new QueueRequest(bodyStream, typeof(NetStream))
            {
                //Version = version,
                //MessageType = messageType,
                Command = command.ToString(),
                Priority = priority,
                TransformType = transformType,
                Host = host,
                //Creation = DateTime.Now,
                //Modified = DateTime.Now,
                //ArrivedTime = Assists.NullDate,
                //BodyStream = bodyStream
            };
        }


        public static IQueueAck SendItem(QueueApi q, QueueMessage item, int connectTimeOut)
        {
            var ack = q.PublishItem(item, connectTimeOut);
            //Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item);
            return ack;
        }
        public static IQueueAck PublishItem(QueueApi q, QueueMessage item, int connectTimeOut)
        {
            return q.PublishItem(item, connectTimeOut);
        }

        public static void PublishItem(QueueApi q, QueueMessage item, int connectTimeOut, Action<IQueueAck> action)
        {
            q.PublishItem(item, connectTimeOut, action);
        }

        public static void EnqueueItem(QueueApi q, QueueMessage item, int connectTimeOut, Action<IQueueAck> action)
        {

            q.EnqueueAsync(item, connectTimeOut, action);

            //DateTime start = DateTime.Now;
            //q.SendAsync(item, connectTimeOut, (ack) =>
            //{
            //    //Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item);
            //});
        }

    }

    public static class QueueClientDemo {

        public static void PublishItem(int i)
        {
            var host = QueueHost.Parse("tcp:127.0.0.1:15000?Netcell");
            QueueApi q = QueueClient.GetApi(host);
            //var item = QueueClient.CreateQueueItem("Hello world " + DateTime.Now.ToString("s"), "test");
            var item = QueueClient.CreateItem(i);
            item.Host = "Netcell";
            item.Command = QueueCmd.Enqueue.ToString();
            //IQueueAck ack = null;

            QueueClient.PublishItem(q, item, 0, (IQueueAck ack) =>
            {
                Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item.Identifier);

            });

        }

        public static void PublishMulti(int maxItems)
        {
            long counter = 0;
            int interval = 1;
            DateTime start = DateTime.Now;

            for (int i = 0; i < maxItems; i++)
            {
                PublishItem(i);
                //Thread.Sleep(interval);
            }

            var duration = DateTime.Now.Subtract(start);
            var milliseconds = duration.TotalMilliseconds;
            Console.WriteLine("duration: {0}, count: {1}, itemDuration: {2}", milliseconds - (interval * counter), counter, (milliseconds - (interval * counter)) / counter);

        }

        public static void SendItem(bool isAsync)
        {
            var host = QueueHost.Parse("tcp:127.0.0.1:15000?NC_Bulk");
            QueueApi q = QueueClient.GetApi(host);
            var item = QueueClient.CreateQueueItem("Hello world " + DateTime.Now.ToString("s"), "test");
            item.Host = "NC_Bulk";
            //IQueueAck ack = null;

            QueueClient.EnqueueItem(q, item, 0, (IQueueAck ack) =>
            {
                Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item.Identifier);

            });

            //if (q.IsAsync)
            //{
            //    IQueueAck ack = QueueClient.SendItemAsync(q, item, 0);
            //    Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item.Id);
            //}
            //else
            //{
            //    QueueClient.SendItemAsync(q, item, 0, (IQueueAck ack) =>
            //    {
            //        Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}, item:{6}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration, item.Id);
            //    });
            //}

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
                        q.SendAsync(item, 0, (ack) =>
                        {
                            Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}", ack.MessageState, ack.Creation, ack.Host, ack.Label, ack.Identifier, ack.Duration);
                            Interlocked.Increment(ref counter);
                        });
                    }
                    else
                    {
                        var ack = q.Enqueue(item, 0);
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
