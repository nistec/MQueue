using Nistec;
using Nistec.Generic;
using Nistec.Messaging;
using Nistec.Messaging.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueListenerDemo
{

    public class SwifterAgent : QueueListener//, INetcellAgent//: QueueReciever
    {
        int Server;
        string QueueAddress;
        //Publishers Publisher;

        public SwifterAgent(QueueHost host)
        {
            //Publisher = Publishers.ApiSms;


            //Source = MsgConfig.AgentFileWatcher.GetQueueHost("MQ_Dequeue", "Swifter");

            IsAsync = true;// AgentConfig.SwifterAgentIsAsync;
            EnableDynamicWait = false;// AgentConfig.SwifterAgentIsDynamicWait;
            ConnectTimeout = 8000;// AgentConfig.SwifterAgentConnectTimeout;
            //QueueName = AgentsConfig.QueueName;
            MaxConnection = 1;// AgentConfig.SwifterAgentMaxConnection;
            WorkerCount = 1;// AgentConfig.SwifterAgentWorkerCount;
            Interval = 1000;// AgentConfig.SwifterAgentInterval;
            //WaitSecond = AgentConfig.MQueueAgentWaitSecond;
            ReadTimeout = 5000;// AgentConfig.SwifterAgentReadTimeout;
            IsMultiTask = true;// AgentConfig.SwifterAgentIsMultiTask;
            Server = 0;// ViewConfig.Server;
            QueueAddress = host.RawHostAddress;// AgentConfig.SwifterAgentHostAddress;// Types.NZ(AgentConfig.MQueueAgentAddress, QueueAddress);
            Source = QueueHost.Parse(QueueAddress);
            InitApi();
        }

        protected override void OnError(string message)
        {
            Console.WriteLine(message);
        }
        protected override void OnInfo(string message)
        {
            Console.WriteLine(message);
        }
        protected override void OnStateChanged(ListenerState state)
        {
            Console.WriteLine("OnStateChanged {0}, State: {1}", HostName, state.ToString());
        }
        protected override int ShouldPause()
        {
            //if (Netcell.Caching.CacheApi.DbRule(7) > 0)
            //    return 60000;
            return base.ShouldPause();
        }
        protected void CommitAsync(IAck ack, IQueueMessage message)
        {
            if (ack.IsOk)
                Commit(message.GetPtr());
            else
                Abort(message.GetPtr());
        }

        #region Publish

        protected override void OnMessageReceived(IQueueMessage message)
        {

            if (message != null)
            {
                PublishMessageAsync(message, (ack) =>
                {
                    OnInfo($"SwifterAgent OnReceivedEvent Result: {ack.ToJson()}");
                    CommitAsync(ack, message);
                }).ConfigureAwait(false);
            }
        }

        protected void PublishMessage(IQueueMessage message, Action<IAck> ack)
        {
            try
            {
                OnInfo($"SwifterAgent PublishMessageAsync ExecuteAsync {message.Print()}");
                var msgIn = message.GetBody();
                if (msgIn == null)
                {
                    throw new Exception($"GetBody error Cast from IQueueMessage.Body to MessageIn, item: {message.Print()}");
                }
                //var publisher = new PublishIn(Publisher, msgIn);
                //publisher.Invoke(ack);
                Console.WriteLine($"PublishMessageAsync Host: {HostName}, Messgae: {message.Print()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("PublishMessageAsync error :{0}, {1}", HostName, ex.Message);
            }
        }

        protected async Task PublishMessageAsync(IQueueMessage message, Action<IAck> ack)
        {
            try
            {
                OnInfo($"SwifterAgent PublishMessageAsync ExecuteAsync {message.Print()}");
                var msgIn = message.GetBody();
                if (msgIn == null)
                {
                    throw new Exception($"GetBody error Cast from IQueueMessage.Body to MessageIn, item: {message.Print()}");
                }
                //var publisher = new PublishIn(Publisher, msgIn);
                //await publisher.InvokeAsync(ack);
                Console.WriteLine($"PublishMessageAsync Host: {HostName}, Messgae: {message.Print()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("PublishMessageAsync error :{0}, {1}", HostName, ex.Message);
            }
        }

        #endregion
    }

    public class SwifterReceiverAgent : QueueAgentReceiver//, INetcellAgent//: QueueReciever
    {

        public static QueueHost GetHost(string protocol, string host_address, string queueName)
        {
            //"tcp:127.0.0.1:15000?NC_Quick"
            //127.0.0.1:15000
            var host = QueueHost.Parse(protocol + ":" + host_address + "?" + queueName);
            return host;
        }

        //Publishers Publisher;
        public SwifterReceiverAgent(QueueHost host)
        {
            //Publisher = Publishers.ApiSms;
            Source = host;// MsgConfig.AgentFileWatcher.GetQueueHost("MQ_Dequeue", "Swifter");

            //IsMultiTask = AgentConfig.SwifterAgentIsMultiTask;
            //Interval = AgentConfig.SwifterAgentInterval;
            //ConnectTimeout = AgentConfig.SwifterAgentConnectTimeout;
            //ReadTimeout = AgentConfig.SwifterAgentReadTimeout;
            //WorkerCount = AgentConfig.SwifterAgentWorkerCount;
            //MaxConnection = AgentConfig.SwifterAgentMaxConnection;
            //EnableDynamicWait = AgentConfig.SwifterAgentIsDynamicWait;

            //QueueName = AgentsConfig.QueueName;
            MaxConnection = 1;// NetConfig.SwifterAgentMaxConnection;
            WorkerCount = 1;// AgentConfig.SwifterAgentWorkerCount;
            Interval = 1000;// AgentConfig.SwifterAgentInterval;
            //WaitSecond = AgentConfig.MQueueAgentWaitSecond;
            ReadTimeout = 8000;// AgentConfig.SwifterAgentReadTimeout;
            Server = 0;// ViewConfig.Server;
            QueueAddress = host.RawHostAddress;// AgentConfig.SwifterAgentHostAddress;// Types.NZ(AgentConfig.MQueueAgentAddress, QueueAddress);
            Init(QueueAddress);
        }

        protected override void OnError(string message)
        {
            Console.WriteLine(message);
        }
        protected override void OnInfo(string message)
        {
            Console.WriteLine(message);
        }
        protected override void OnStateChanged(ListenerState state)
        {
            Console.WriteLine("OnStateChanged {0}, State: {1}", HostName, state.ToString());
        }

        protected override int ShouldPause()
        {
            return 0;// Netcell.Caching.CacheApi.DbRule(7) > 0;//Hold Controller Service
        }

        #region Publish

        protected override void OnMessageReceived(IQueueMessage message)
        {

            if (message != null)
            {
                PublishMessageAsync(message, (ack) =>
                {
                    OnInfo($"SwifterAgent OnReceivedEvent Result: {ack.ToJson()}");
                    CommitAsync(ack, message);
                }).ConfigureAwait(false);
            }
        }

        protected void PublishMessage(IQueueMessage message, Action<IAck> ack)
        {
            try
            {
                OnInfo($"SwifterAgent PublishMessageAsync ExecuteAsync {message.Print()}");
                var msgIn = message.GetBody();
                if (msgIn == null)
                {
                    throw new Exception($"GetBody error Cast from IQueueMessage.Body to MessageIn, item: {message.Print()}");
                }
                //Task.Delay(TimeSpan.FromSeconds(30));
                //var publisher = new PublishIn(Publisher, msgIn);
                //publisher.Invoke(ack);
                Console.WriteLine($"PublishMessageAsync Host: {HostName}, Messgae: {message.Print()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("PublishMessageAsync error :{0}, {1}", HostName, ex.Message);
            }
        }

        protected async Task PublishMessageAsync(IQueueMessage message, Action<IAck> ack)
        {
            try
            {
                OnInfo($"SwifterAgent PublishMessageAsync ExecuteAsync {message.Print()}");
                var msgIn = message.GetBody();
                if (msgIn == null)
                {
                    throw new Exception($"GetBody error Cast from IQueueMessage.Body to MessageIn, item: {message.Print()}");
                }
                //await Task.Delay(TimeSpan.FromSeconds(30));
                //var publisher = new PublishIn(Publisher, msgIn);
                //await publisher.InvokeAsync(ack);
                Console.WriteLine($"PublishMessageAsync Host: {HostName}, Messgae: {message.Print()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("PublishMessageAsync error :{0}, {1}", HostName, ex.Message);
            }
        }

        #endregion

    }
}
