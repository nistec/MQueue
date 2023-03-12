using Nistec;
using Nistec.Channels;
using Nistec.Channels.Tcp;
using Nistec.Collections;
using Nistec.Data.Persistance;
using Nistec.Data.Sqlite;
using Nistec.Logging;
using Nistec.Messaging;
using Nistec.Messaging.Listeners;
using Nistec.Messaging.Remote;
using Nistec.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace QueueListenerDemo
{
    public class QueueReceiver
    {


        //var api= QueueApi.Get(Nistec.Channels.NetProtocol.Pipe);
        //var hostPipe = QueueHost.Parse("ipc:.:nistec_queue_listener?NC_Quick");
        //var hostTcp = QueueHost.Parse("tcp:localhost:15001?NC_Quick");

        public static QueueHost GetHost(string protocol, string host_address, string queueName)
        {
            //"tcp:127.0.0.1:15000?NC_Quick"
            //127.0.0.1:15000
            var host = QueueHost.Parse(protocol + ":" + host_address + "?" + queueName);
            return host;
        }

        public static void Consume(QueueHost host)
        {
            QueueApi q = new QueueApi(host);
            q.ConnectTimeout = 500000000;
            q.ReadTimeout = -1;
            int i = 0;
            do
            {
                i++;
                var item = q.Consume(60);// DuplexTypes.WaitOne);

                if (item != null)
                {
                    Console.WriteLine(item.Print());
                }
                else
                {
                    Console.WriteLine("Get nothing!");
                }
                Thread.Sleep(100);

            } while (i < 100);
        }

        public static void DoGet(QueueHost host)
        {
            QueueApi q = new QueueApi(host);
            q.ConnectTimeout = 500000000;
            //q.ReadTimeout = -1;
            var item = q.Dequeue();// DuplexTypes.WaitOne);

            if (item != null)
            {
                Console.WriteLine(item.Print());
            }
            else
            {
                Console.WriteLine("Get nothing!");
            }
        }

        public static void DoQuery(QueueHost host)
        {
            QueueApi q = new QueueApi(host);
            var req = new QueueRequest() { QCommand = QueueCmd.ReportQueueItems, DuplexType = DuplexTypes.NoWaite, Host = "NC_Quick" };
            var ts = q.ExecDuplexStream(req, 1000000,0);

            if (ts != null)
            {
                //var stream = ts.ReadStream(null);
                //Nistec.Serialization.BinaryStreamer bs = new Nistec.Serialization.BinaryStreamer(stream);
                //var olist= bs.Decode();

                //var olist = ts.ReadValue();

                var list = ts.ReadValue<IEnumerable<PersistItem>>();
                Console.WriteLine(list);
            }
            else
            {
                Console.WriteLine("Get nothing!");
            }
        }


        public static void DoListnning(QueueHost host)
        {

            var adapter = new QueueAdapter()
            {
                Source = host,
                IsAsync = true,
                Interval = 100,
                ConnectTimeout = 5000,
                ReadTimeout = 180000,
                WorkerCount = 1,
                EnableDynamicWait = true,
                MessageReceivedAction = (message) =>
                {
                    Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}, Duration:{5}", message.MessageState, message.ArrivedTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), message.Host, message.Label, message.Identifier, message.Duration);

                    var body = message.GetBody();
                    string sbody = body == null ? "null" : body.ToString();
                    Console.WriteLine("body: " + sbody);
                },
                MessageFaultAction = (message) =>
                {
                    Console.WriteLine(message);
                }
            };

            QueueListener listener = new QueueListener(adapter);
            string logpath = NetlogSettings.GetDefaultPath("qlistener");
            listener.Logger = new Logger(logpath);
            //listener.ErrorOcurred += Listener_ErrorOcurred;
            //listener.MessageReceived += Listener_MessageReceived;
            listener.Start();

            //QueueApi api = new QueueApi(host);
            //api.ReceiveCompleted += api_ReceiveCompleted;

            ////api.Listener(10000, message_ReceiveCompleted);
            //bool KeepAlive = true;
            //int connectTimeout=10000;

            //    while (KeepAlive)
            //    {
            //        api.Receive(connectTimeout,message_ReceiveCompleted);
            //        Thread.Sleep(100);
            //    }




            //Console.WriteLine("QueueListener finished...");
            //Console.ReadLine();
        }

        private static void Listener_MessageReceived(object sender, Nistec.Generic.GenericEventArgs<IQueueItem> e)
        {
            var message = e.Args;
            Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}", message.MessageState, message.ArrivedTime, message.Host, message.Label, message.Identifier);

        }

        private static void Listener_ErrorOcurred(object sender, Nistec.Generic.GenericEventArgs<string> e)
        {
            Console.WriteLine(e.Args);
        }

        public static void DoSbscriberListener()
        {
            var settings = new TcpSettings()
            {
                Address = "127.0.0.1",
                ConnectTimeout = 5000,
                HostName = "Netcell",
                Port = 15002,
                IsAsync = false
            };
            var qhost = QueueHost.Parse(string.Format("file:{0}:Queues?{1}", Assists.EXECPATH, settings.HostName));
            qhost.CoverMode = CoverMode.FileStream;
            qhost.CommitMode = PersistCommitMode.OnMemory;
            qhost.ReloadOnStart = true;

            var listener = new TopicSbscriberListener(qhost, true)
            {
                OnItemReceived = (IQueueItem message) =>
                {

                    Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}", message.MessageState, message.ArrivedTime, message.Host, message.Label, message.Identifier);

                    return new QueueAck(Nistec.Messaging.MessageState.Received, message).ToTransStream();
                },
                OnError= (string message) => {
                    Console.WriteLine("OnError:{0}", message);

                }
            };
            string logpath = NetlogSettings.GetDefaultPath("topicSubs");
            listener.Logger = new Logger(logpath,LoggerMode.Console| LoggerMode.File);
            listener.InitServerQueue(settings,true);
            //listener.PausePersistQueue(true);
        }

    }

    /*
    public class TopicSubs : TopicSbscriberListener
    {

        public TopicSubs() : base()
        {

            var settings = new TcpSettings()
            {
                Address = "127.0.0.1",
                ConnectTimeout = 5000000,
                HostName = "Netcell",
                Port = 15002,
                IsAsync = false
            };
            InitTcpServerQueue(settings);
        }


        public override TransStream OnMessageReceived(IQueueItem message)
        {
            Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}", message.MessageState, message.ArrivedTime, message.Host, message.Label, message.Identifier);

            return new QueueAck(Nistec.Messaging.MessageState.Received,message).ToTransStream();
        }
    }
    */
}
