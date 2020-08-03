  using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nistec.Channels;
using Nistec.Generic;
using System.Collections;
using Nistec.Runtime;
using Nistec.Data.Entities;
using System.IO.Pipes;
using Nistec.IO;
using Nistec.Serialization;
using Nistec.Data;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Nistec.Messaging.Remote
{
    /// <summary>
    /// Represent Queue Api for client.
    /// </summary>
    public class ManagementApi : RemoteApi
    {
        CancellationTokenSource canceller = new CancellationTokenSource();

        const string HostAddress = "nistec_queue_manager";

        /// <summary>
        /// Get queue api.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static ManagementApi Get(NetProtocol protocol= NetProtocol.Pipe)
        {
            if (protocol == NetProtocol.NA)
            {
                protocol = ChannelSettings.DefaultProtocol;
            }
            return new ManagementApi() {Protocol=protocol, RemoteHostAddress = HostAddress };
        }

        public static ManagementApi Get(string queueName,NetProtocol protocol = NetProtocol.Pipe)
        {
            if (protocol == NetProtocol.NA)
            {
                protocol = ChannelSettings.DefaultProtocol;
            }
            return new ManagementApi() { Protocol = protocol, _QueueName=queueName, RemoteHostAddress= HostAddress };
        }

        private ManagementApi()
        {
            //RemoteHostName = ChannelSettings.RemoteQueueHostName;
            EnableRemoteException = ChannelSettings.EnableRemoteException;
        }

        protected void OnFault(string message)
        {
            Console.WriteLine("QueueApi Fault: " + message);
        }
        protected void OnCompleted(QueueItem message)
        {
            Console.WriteLine("QueueApi Completed: " + message.Identifier);
        }
        

        #region ctor

        public ManagementApi(string hostAddress) 
            : this()
        {
            var qh = QueueHost.Parse(hostAddress);

            //_QueueName = queueName;
            _HostProtocol = qh.Protocol;
            RemoteHostAddress = qh.Endpoint;
            RemoteHostPort = qh.Port;
            Protocol = qh.Protocol.GetProtocol();
        }
     
        public ManagementApi(QueueHost host) 
            : this()
        {
            _QueueName = host.HostName;
            _HostProtocol = host.Protocol;
            RemoteHostAddress = host.Endpoint;
            RemoteHostPort = host.Port;
            Protocol = host.NetProtocol;
        }

        #endregion

        #region Report

        public TransStream Command(QueueCmd command)
        {
            QueueRequest request = new QueueRequest()
            {
                QCommand = (QueueCmd)(int)command,
                Host = _QueueName
            };

            var result = ExecDuplexStream(request, ConnectTimeout);
            
            return result;

        }
        
        public void ReportAsync(QueueCmdReport command, Action<TransStream> action)
        {
            using (

                    Task<TransStream> task = Task<TransStream>.Factory.StartNew(() =>
                        Report(command)
                    ,
                    canceller.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    TransStream item = task.Result;
                    if (item != null)
                    {
                        if (action != null)
                            Task.Factory.StartNew(() => action(item));
                    }
                }
                else if (task.IsCanceled)
                {

                }
                else if (task.IsFaulted)
                {

                }
            }
        }
        #endregion

        #region Commit/Abort/Report

        public TransStream Report(QueueCmdReport cmd)
        {
            QueueRequest request = new QueueRequest()
            {
                Host = _QueueName,
                QCommand = (QueueCmd)(int)cmd
                //Command = (QueueCmd)(int)cmd
            };
            var response = ExecDuplexStream(request, ConnectTimeout);
            return response;
        }

        //public void ReportAsync(QueueCmdReport cmd, Action<string> onFault, Action<TransStream> onCompleted, DuplexTypes DuplexType, AutoResetEvent resetEvent)
        //{
        //    QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.Dequeue, null);
        //    {
        //        Host = _QueueName,
        //        QCommand = (QueueCmd)(int)cmd,
        //        DuplexType = DuplexType
        //    };

        //    base.SendDuplexStreamAsync(request, onFault, onCompleted, resetEvent);
        //}

        public T Report<T>(QueueCmdReport cmd)
        {
            var res = Report(cmd);
            if (res == null)
                return default(T);
            return res.ReadValue<T>();
        }

        public IQueueItem OperateQueue(QueueCmdOperation cmd)
        {
            QueueRequest message = new QueueRequest()//queueName, (QueueCmd)(int)cmd)
            {
                Host = _QueueName,
                QCommand = (QueueCmd)(int)cmd,
                //Command = (QueueCmd)(int)cmd
            };
            var response=ConsumItem(message,ConnectTimeout);
            return response;//==null? null: response.ToMessage();
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, (QueueCmd)(int)cmd);
        }

        public IQueueItem AddQueue(QProperties qp)
        {
            var message = new QueueRequest()
            {
                Host = _QueueName,
                QCommand = QueueCmd.AddQueue,
            };

            message.SetBody(qp.GetEntityStream(false), qp.GetType().FullName);
            var response = ConsumItem(message, ConnectTimeout);
            return response;// == null ? null : response.ToMessage();
        }

        public IQueueItem AddQueue(CoverMode mode, bool isTrans)
        {
           

            QProperties qp = new QProperties()
            {
                QueueName = _QueueName,
                ServerPath = "localhost",
                Mode = mode,
                IsTrans = isTrans,
                MaxRetry = QueueDefaults.DefaultMaxRetry,
                ReloadOnStart = false,
                ConnectTimeout = 0,
                TargetPath = "",
                IsTopic=false
            };
            return AddQueue(qp);
            //var message = new QueueItem()
            //{
            //    Host = _QueueName,
            //    Command = QueueCmd.AddQueue,
            //};

            //message.SetBody(qp.GetEntityStream(false), qp.GetType());

            //GenericNameValue header = new GenericNameValue();

            //header.Add("QueueName", _QueueName);
            //header.Add("ServerPath", "localhost");
            //header.Add("Mode", (int)mode);
            //header.Add("IsTrans", isTrans);
            //header.Add("MaxRetry", QueueDefaults.DefaultMaxRetry);
            //header.Add("ReloadOnStart", false);
            //message.SetHeader(header);
            //message.SetBody(qp);

            //var response=base.SendDuplex(message);
            //return response;// == null ? null : response.ToMessage();
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //var res= client.Exec(message, QueueCmd.AddQueue);
            //return (Message)res;// client.Exec(message, QueueCmd.AddQueue);
        }

        public IQueueItem RemoveQueue()
        {
            QueueRequest message = new QueueRequest()
            {
                Host = _QueueName,
                QCommand = QueueCmd.RemoveQueue,
            };
            var response= ConsumItem(message, ConnectTimeout);
            return response;// == null ? null : response.ToMessage();

            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, QueueCmd.RemoveQueue);
        }

        public IQueueItem QueueExists()
        {
            QueueRequest message = new QueueRequest()
            {
                Host = _QueueName,
                QCommand = QueueCmd.Exists,
            };
            var response= ConsumItem(message, ConnectTimeout);
            return response;// == null ? null : response.ToMessage();
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, QueueCmd.RemoveQueue);
        }


        #endregion
   
    }
}
