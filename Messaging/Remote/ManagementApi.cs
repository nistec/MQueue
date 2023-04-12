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

        public const string HostName = "nistec_queue_manager";

        /// <summary>
        /// Get queue api.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static ManagementApi Get()
        {
             return new ManagementApi() { Protocol = NetProtocol.Pipe, RemoteHostAddress = HostName };
        }

        /// <summary>
        /// Get queue api.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static ManagementApi Get(string hostAddress,NetProtocol protocol)
        {
           
            if (protocol == NetProtocol.NA)
            {
                protocol = ChannelSettings.DefaultProtocol;
            }
            if (protocol == NetProtocol.Pipe)
            {
                if (hostAddress == null)
                    throw new ArgumentException("hostAddress is required");
                hostAddress = HostName;
                return new ManagementApi() { Protocol = protocol, RemoteHostAddress = hostAddress };

            }
            else
            {
                string[] args = hostAddress.SplitTrim(':');
                hostAddress = args[0];
                if (args.Length < 2)
                    throw new ArgumentException("hostAddress and port is required");

                int port = Types.ToInt(args[1]);
                return new ManagementApi() { Protocol = protocol, RemoteHostAddress = hostAddress, RemoteHostPort = port };
            }
        }

        public static ManagementApi Get(string hostAddress, string queueName,NetProtocol protocol)
        {
            if (protocol == NetProtocol.NA)
            {
                protocol = ChannelSettings.DefaultProtocol;
            }
            if (protocol == NetProtocol.Pipe)
            {
                if (hostAddress == null)
                    throw new ArgumentException("hostAddress is required");
                hostAddress = HostName;
                return new ManagementApi() { Protocol = protocol, QueueName = queueName, RemoteHostAddress = hostAddress };

            }
            else
            {
                string[] args = hostAddress.SplitTrim(':');
                hostAddress = args[0];
                if (args.Length < 2)
                    throw new ArgumentException("hostAddress and port is required");

                int port = Types.ToInt(args[1]);
                return new ManagementApi() { Protocol = protocol, QueueName = queueName, RemoteHostAddress = hostAddress, RemoteHostPort = port };
            }
        }

        private ManagementApi()
        {
            //RemoteHostName = ChannelSettings.RemoteQueueHostName;
            EnableRemoteException = ChannelSettings.DefaultEnableRemoteException;
        }

        protected void OnFault(string message)
        {
            Console.WriteLine("QueueApi Fault: " + message);
        }
        protected void OnCompleted(QueueMessage message)
        {
            Console.WriteLine("QueueApi Completed: " + message.Identifier);
        }

        public TransStream Reply()
        {
            QueueRequest message = new QueueRequest()
            {
                Host = QueueName,
                QCommand = QueueCmd.Reply,
            };
            var response = RequestItemStream(message, ConnectTimeout);
            return response;
        }

        #region ctor

        public ManagementApi(string hostAddress) 
            : this()
        {
            var qh = QueueHost.Parse(hostAddress);

            //_QueueName = queueName;
            HostProtocol = qh.Protocol;
            RemoteHostAddress = qh.Endpoint;
            RemoteHostPort = qh.Port;
            Protocol = qh.Protocol.GetProtocol();
        }
     
        public ManagementApi(QueueHost host) 
            : this()
        {
            QueueName = host.HostName;
            HostProtocol = host.Protocol;
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
                Host = QueueName
            };

            var result = ExecDuplexStream(request, ConnectTimeout, ReadTimeout);
            
            return result;

        }
        
        public void ReportAsync(QueueCmdReport command, Action<TransStream> action)
        {
            using (

                    Task<TransStream> task = Task<TransStream>.Factory.StartNew(() =>
                        Report(command, null)
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

        public TransStream Report(QueueCmdReport cmd, string queueName = null)
        {
            QueueRequest request = new QueueRequest()
            {
                Host = queueName ?? QueueName,
                QCommand = (QueueCmd)(int)cmd
                //Command = (QueueCmd)(int)cmd
            };
            var response = ExecDuplexStream(request, ConnectTimeout, ReadTimeout);
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

        public T Report<T>(QueueCmdReport cmd, string queueName)
        {
            var res = Report(cmd, queueName);
            if (res == null)
                return default(T);
            return res.ReadValue<T>();
        }
        public TransStream OperateQueue(QueueRequest message)
        {
            var response = RequestItemStream(message, ConnectTimeout);
            return response;//==null? null: response.ToMessage();
        }
        public TransStream OperateQueue(QueueCmdOperation cmd, string queueName=null)
        {
            QueueRequest message = new QueueRequest()//queueName, (QueueCmd)(int)cmd)
            {
                Host = queueName??QueueName,
                QCommand = (QueueCmd)(int)cmd
                //Command = (QueueCmd)(int)cmd
            };
            var response= RequestItemStream(message,ConnectTimeout);
            return response;//==null? null: response.ToMessage();
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, (QueueCmd)(int)cmd);
        }
        public TransStream OperateQueue(QueueCmd cmd, string queueName)
        {
            QueueRequest message = new QueueRequest()//queueName, (QueueCmd)(int)cmd)
            {
                Host = queueName ?? QueueName,
                QCommand = cmd,
                //Command = (QueueCmd)(int)cmd
            };
            var response = RequestItemStream(message, ConnectTimeout);
            return response;//==null? null: response.ToMessage();
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, (QueueCmd)(int)cmd);
        }
        public TransStream AddQueue(QProperties qp)
        {
            var message = new QueueRequest()
            {
                Host = QueueName,
                QCommand = QueueCmd.AddQueue,
            };

            message.SetBody(qp.GetEntityStream(false), qp.GetType().FullName);
            var response = RequestItemStream(message, ConnectTimeout);
            return response;// == null ? null : response.ToMessage();
        }

        public TransStream AddQueue(CoverMode mode, bool isTrans)
        {
           

            QProperties qp = new QProperties()
            {
                QueueName = QueueName,
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
            //var message = new QueueMessage()
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

        public TransStream RemoveQueue(string queueName)
        {
            QueueRequest message = new QueueRequest()
            {
                Host = queueName ?? QueueName,
                QCommand = QueueCmd.RemoveQueue,
            };
            var response= RequestItemStream(message, ConnectTimeout);
            return response;// == null ? null : response.ToMessage();

            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, QueueCmd.RemoveQueue);
        }

        public TransStream QueueExists(string queueName)
        {
            QueueRequest message = new QueueRequest()
            {
                Host = queueName ?? QueueName,
                QCommand = QueueCmd.Exists,
            };
            var response= RequestItemStream(message, ConnectTimeout);
            return response;// == null ? null : response.ToMessage();
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, QueueCmd.RemoveQueue);
        }


        #endregion

        public string DoHttpJson(string command, string key, string sessionId = null, string label = null, object value = null, int expiration = 0, bool pretty = false)
        {

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key is required");
            }
            var msg = new QueueRequest() { Command = command, CustomId = key, SessionId = sessionId, Expiration = expiration };
            //var msg = new QueueRequest() { Command = command, Identifier = key, GroupId = groupId, Expiration = expiration };
            msg.SetBody(value);
            return SendHttpJsonDuplex(msg, pretty);


            //string cmd = "cach_" + command.ToLower();
            //switch (cmd)
            //{
            //    case "AddQueue":
            //        //return Add(key, value, expiration);
            //        {
            //            if (string.IsNullOrWhiteSpace(key))
            //            {
            //                throw new ArgumentNullException("key is required");
            //            }
            //            var msg = new QueueRequest() { Command = cmd, Id = key, GroupId = groupId, Expiration = expiration };
            //            msg.SetBody(value);
            //            return SendHttpJsonDuplex(msg, pretty);
            //        }
            //    case QueueCmd.CopyTo:
            //    //return CopyTo(key, detail, expiration);
            //    case QueueCmd.CutTo:
            //        //return CutTo(key, detail, expiration);
            //        {
            //            if (string.IsNullOrWhiteSpace(key))
            //            {
            //                throw new ArgumentNullException("key is required");
            //            }
            //            var msg = new CacheMessage() { Command = cmd, Id = key, GroupId = groupId, Expiration = expiration };
            //            msg.SetBody(value);
            //            msg.Args = MessageStream.CreateArgs(KnownArgs.Source, label, KnownArgs.Destination, key);
            //            return SendHttpJsonDuplex(msg, pretty);
            //        }
            //    case QueueCmd.Fetch:
            //    case QueueCmd.Get:
            //    case QueueCmd.GetEntry:
            //    case QueueCmd.GetRecord:
            //    case QueueCmd.RemoveItemsBySession:
            //    case QueueCmd.Reply:
            //    case QueueCmd.Remove:
            //    case QueueCmd.ViewEntry:
            //        {
            //            if (string.IsNullOrWhiteSpace(key))
            //            {
            //                throw new ArgumentNullException("key is required");
            //            }
            //            return SendHttpJsonDuplex(new QueueRequest() { Command = cmd, Id = key }, pretty);
            //        }
            //    case QueueCmd.KeepAliveItem:
            //    case QueueCmd.RemoveAsync:
            //        {
            //            if (string.IsNullOrWhiteSpace(key))
            //            {
            //                throw new ArgumentNullException("key is required");
            //            }
            //            SendHttpJsonOut(new CacheMessage() { Command = cmd, Id = key });
            //            return CacheState.Ok.ToString();
            //        }

            //    //case QueueCmd.LoadData:
            //    //    return LoadData();
            //    case QueueCmd.Set:
            //        //return Set(key, value, expiration);
            //        {
            //            if (string.IsNullOrWhiteSpace(key))
            //            {
            //                throw new ArgumentNullException("key is required");
            //            }

            //            if (value == null)
            //            {
            //                throw new ArgumentNullException("value is required");
            //            }
            //            var message = new CacheMessage(cmd, key, value, expiration);
            //            return SendHttpJsonDuplex(message, pretty);
            //        }
            //    default:
            //        throw new ArgumentException("Unknown command " + command);
            //}
        }

    }
}
