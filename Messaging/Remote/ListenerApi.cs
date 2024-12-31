using Nistec.Channels;
using Nistec.Logging;
using Nistec.Messaging.Listeners;
using Nistec.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nistec.Messaging.Remote
{
    public class ListenerApi: QueueApi
    {
  

        long _CosumeWait = 0;
        public void CancelWait()
        {
            Interlocked.Exchange(ref _CosumeWait, 0);
        }

        QueueHost QHost;

        #region ctor
        public ListenerApi(int connectTimeout = 0)
        {
            ConnectTimeout = (connectTimeout <= 0) ? DefaultConnectTimeout : connectTimeout;
            //RemoteHostName = ChannelSettings.RemoteQueueHostName;
            EnableRemoteException = ChannelSettings.DefaultEnableRemoteException;
        }

        //public ListenerApi(NetProtocol protocol = NetProtocol.Tcp, int connectTimeout = 0)
        //{
        //    if (protocol == NetProtocol.NA)
        //    {
        //        protocol = ChannelSettings.DefaultProtocol;
        //    }
        //    Protocol = protocol;
        //    ConnectTimeout = (connectTimeout <= 0) ? DefaultConnectTimeout : connectTimeout;
        //    //RemoteHostName = ChannelSettings.RemoteQueueHostName;
        //    EnableRemoteException = ChannelSettings.DefaultEnableRemoteException;
        //}

        public ListenerApi(string queueName, string hostAddress)
            : this()
        {
            QHost = QueueHost.Parse(hostAddress);

            QueueName = queueName;
            HostProtocol = QHost.Protocol;
            RemoteHostAddress = QHost.HostAddress;
            RemoteHostPort = QHost.Port;
            Protocol = QHost.Protocol.GetProtocol();
        }
        public ListenerApi(string queueName, HostProtocol protocol, string endpoint, int port, string hostName)
            : this()
        {
            QueueName = queueName;
            HostProtocol = protocol;
            RemoteHostAddress = endpoint;// QueueHost.GetRawAddress(protocol,serverName,port, hostName);
            RemoteHostPort = port;
            Protocol = protocol.GetProtocol();
            QHost = new QueueHost(protocol, endpoint, port, hostName);
        }

        public ListenerApi(QueueHost host)
            : this()
        {
            QueueName = host.HostName;
            HostProtocol = host.Protocol;
            RemoteHostAddress = host.Endpoint;
            RemoteHostPort = host.Port;
            Protocol = host.NetProtocol;
            QHost = host;
        }

        public static ListenerApi GetListener(string hostAddress, int connectTimeout = 0, bool isAsync = false)
        {
            var host = QueueHost.Parse(hostAddress);
            var api = new ListenerApi(host);
            api.IsAsync = isAsync;
            api.ConnectTimeout = connectTimeout;
            return api;
        }
        public static ListenerApi GetListener(QueueHost host, int connectTimeout = 0, bool isAsync = false)
        {
            var api = new ListenerApi(host);
            api.IsAsync = isAsync;
            api.ConnectTimeout = connectTimeout;
            return api;
        }

        #endregion


        public async Task ConsumeAwait(int maxWaitSecond, Action<IQueueMessage> onCompleted)
        {
            Interlocked.Exchange(ref _CosumeWait, 1);
            do
            {
                await ConsumeAsync(maxWaitSecond, onCompleted);
                Thread.Sleep(100);
            } while (Interlocked.Read(ref _CosumeWait) > 0);
        }


        public QueueAdapter Create(bool IsMultiTask=true,int Interval=10,int ConnectTimeout= 5000, int ReadTimeout=10000,int WorkerCount=1, int MaxConnection=10,bool EnableDynamicWait=true)
        {
            return new QueueAdapter()
            {
                Source = QHost,
                IsAsync = true,
                IsMultiTask = IsMultiTask,
                Interval = Interval,
                ConnectTimeout = ConnectTimeout,
                ReadTimeout = ReadTimeout,
                WorkerCount = WorkerCount < 0 ? 1 : WorkerCount < 4 ? WorkerCount: 3,
                MaxConnection = MaxConnection < 0 ? 1 : MaxConnection < 20 ? MaxConnection : 20,
                EnableDynamicWait = EnableDynamicWait,
            };
        }

        public void QueueListnning(QueueAdapter adapter, Action<IQueueMessage> onReceived, Action<string> onFault)
        {
            if (adapter == null)
                adapter = Create();
            adapter.MessageReceivedAction = onReceived;
            adapter.MessageFaultAction = onFault;
            QueueListener listener = new QueueListener(adapter);
            string logpath = NetlogSettings.GetDefaultPath("qlistener");
            listener.Logger = new Logger(logpath);
            //listener.ErrorOcurred += Listener_ErrorOcurred;
            //listener.MessageReceived += Listener_MessageReceived;
            listener.Start();
        }
    }
}
