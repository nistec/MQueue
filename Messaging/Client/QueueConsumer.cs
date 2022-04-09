using Nistec;
using Nistec.Channels;
using Nistec.Channels.Tcp;
using Nistec.Collections;
using Nistec.Data.Persistance;
using Nistec.Logging;
using Nistec.Messaging;
using Nistec.Messaging.Listeners;
using Nistec.Messaging.Remote;
using Nistec.Runtime;
using Nistec.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Client
{
    public class QueueConsumer : QueueClient
    {

        public QueueConsumer(string queueName, string hostAddress) : base(queueName, hostAddress)
        {

        }
        public QueueConsumer(string queueName, HostProtocol protocol, string endpoint, int port, string hostName) : base(queueName, protocol, endpoint, port, hostName)
        {

        }

        public void DequeueAsync(QueueRequest item, int connectTimeOut, Action<IQueueItem> action, IDynamicWait dw)
        {
            _Api.DequeueAsync(item, connectTimeOut, action, dw);
        }
        public void DequeueAsync(QueueRequest item, int connectTimeOut, Action<IQueueItem> action)
        {
            _Api.DequeueAsync(item, connectTimeOut, action, DynamicWait.Empty);
        }
        public IQueueItem Denqueue(QueueRequest item, int connectTimeOut)
        {
            return _Api.Dequeue(item, connectTimeOut);
        }

        public QueueListener CreateQueueListener(QueueHost host, int workerCount, int connectTimeOut, int readTimeOut, Action<IQueueItem> received, Action<string> fault)
        {

            var adapter = new QueueAdapter()
            {
                Source = host,
                IsAsync = true,
                Interval = 100,
                ConnectTimeout = connectTimeOut,
                ReadTimeout = readTimeOut,
                WorkerCount = workerCount,
                EnableDynamicWait = true,
                MessageReceivedAction = received,
                MessageFaultAction = fault
            };

            QueueListener listener = new QueueListener(adapter);
            string logpath = NetlogSettings.GetDefaultPath("qlistener");
            listener.Logger = new Logger(logpath);
            //listener.ErrorOcurred += Listener_ErrorOcurred;
            //listener.MessageReceived += Listener_MessageReceived;
            //listener.Start();

            return listener;

        }

        public TopicSbscriberListener CreateSbscriberListener(QueueHost qhost, TcpSettings settings, Func<IQueueItem, TransStream> onItemReceived, Action<string> fault)
        {
            var listener = new TopicSbscriberListener(qhost, true)
            {
                OnItemReceived = onItemReceived,
                //{

                //    Console.WriteLine("State:{0},Arrived:{1},Host:{2},Label:{3}, Identifier:{4}", message.MessageState, message.ArrivedTime, message.Host, message.Label, message.Identifier);

                //    return new QueueAck(Nistec.Messaging.MessageState.Received, message).ToTransStream();
                //},
                OnError = fault
            };
            string logpath = NetlogSettings.GetDefaultPath("topicSubs");
            listener.Logger = new Logger(logpath, LoggerMode.Console | LoggerMode.File);
            listener.InitServerQueue(settings, true);
            //listener.PausePersistQueue(true);

            return listener;
        }

        public static IQueueItem DoGet(QueueHost host, int connectTimeOut)
        {
            QueueApi q = new QueueApi(host);
            q.ConnectTimeout = connectTimeOut;
            //q.ReadTimeout = -1;
            return q.Dequeue();// DuplexTypes.WaitOne);
        }
        /*
        public static void DoQuery(QueueHost host)
        {
            QueueApi q = new QueueApi(host);
            var req = new QueueRequest() { QCommand = QueueCmd.ReportQueueItems, DuplexType = DuplexTypes.NoWaite, Host = "NC_Quick" };
            var ts = q.ExecDuplexStream(req, 1000000);

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

    
    */
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
}
