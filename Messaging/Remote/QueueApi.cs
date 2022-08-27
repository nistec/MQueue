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
using Nistec.Threading;

namespace Nistec.Messaging.Remote
{
    /// <summary>
    /// Represent Queue Api for client.
    /// </summary>
    public class QueueApi : RemoteApi, IQueueClient
    {

        #region ctor

        ///// <summary>
        ///// Get queue api.
        ///// </summary>
        ///// <param name="protocol"></param>
        ///// <returns></returns>
        //public static QueueApi Get(NetProtocol protocol= NetProtocol.Tcp)
        //{
        //    if (protocol == NetProtocol.NA)
        //    {
        //        protocol = ChannelSettings.DefaultProtocol;
        //    }
        //    return new QueueApi() {Protocol=protocol };
        //}

        //public QueueApi()
        //{
        //    Protocol = ChannelSettings.DefaultProtocol;
        //    //RemoteHostName = ChannelSettings.RemoteQueueHostName;
        //    EnableRemoteException = ChannelSettings.EnableRemoteException;
        //}

        

        #endregion

        #region override




        ///// <summary>
        ///// Get item value from queue as json.
        ///// </summary>
        ///// <param name="queueKey"></param>
        ///// <param name="format"></param>
        ///// <returns></returns>
        //public string GetJson(string queueKey, JsonFormat format)
        //{
        //    var obj = GetValue(queueKey);
        //    if (obj == null)
        //        return null;
        //    return JsonSerializer.Serialize(obj, null, format);
        //}
        #endregion

        #region members

        CancellationTokenSource canceller = new CancellationTokenSource();

        ///// <summary>
        ///// No limit timeout.
        ///// </summary>
        //public const int InfiniteTimeout = 0;
        ///// <summary>
        ///// 5 minute timeout.
        ///// </summary>
        //public const int ShortTimeout = 307200;//5 minute
        ///// <summary>
        ///// 30 minute timeout.
        ///// </summary>
        //public const int LongTimeout = 1843200;//30 minute

        //string _QueueName;
        ////string _ServerName = ".";
        ////string _HostAddress;
        //HostProtocol _HostProtocol;
        //public bool IsCoverable { get; set; }

        #endregion

        #region ctor


        public QueueApi(NetProtocol protocol = NetProtocol.Tcp, int connectTimeout = 0)
        {
            if (protocol == NetProtocol.NA)
            {
                protocol = ChannelSettings.DefaultProtocol;
            }
            Protocol = protocol;
            ConnectTimeout = (connectTimeout <= 0) ? DefaultConnectTimeout : connectTimeout;
            //RemoteHostName = ChannelSettings.RemoteQueueHostName;
            EnableRemoteException = ChannelSettings.DefaultEnableRemoteException;
        }

        //public QueueApi(string queueName) : this()
        //{
        //    _QueueName = queueName;
        //    _HostProtocol = HostProtocol.ipc;
        //    RemoteHostAddress = null;

        //    Protocol = NetProtocol.Pipe;
        //}

        public QueueApi(string queueName, string hostAddress) 
            : this()
        {
            var qh = QueueHost.Parse(hostAddress);

            QueueName = queueName;
            HostProtocol = qh.Protocol;
            RemoteHostAddress = qh.Endpoint;
            RemoteHostPort = qh.Port;
            Protocol = qh.Protocol.GetProtocol();
        }
        public QueueApi(string queueName, HostProtocol protocol, string endpoint, int port, string hostName) 
            : this()
        {
            QueueName = queueName;
            HostProtocol = protocol;
            RemoteHostAddress = endpoint;// QueueHost.GetRawAddress(protocol,serverName,port, hostName);
            RemoteHostPort = port;
            Protocol = protocol.GetProtocol();
        }

        public QueueApi(QueueHost host) 
            : this()
        {
            QueueName = host.HostName;
            HostProtocol = host.Protocol;
            RemoteHostAddress = host.Endpoint;
            RemoteHostPort = host.Port;
            Protocol = host.NetProtocol;
        }

        public static QueueApi Get(string hostAddress, int connectTimeout = 0, bool isAsync=false)
        {
            var host = QueueHost.Parse(hostAddress);
            var api = new QueueApi(host);
            api.IsAsync = isAsync;
            api.ConnectTimeout = connectTimeout;
            return api;
        }
        public static QueueApi Get(QueueHost host, int connectTimeout = 0, bool isAsync = false)
        {
            var api = new QueueApi(host);
            api.IsAsync = isAsync;
            api.ConnectTimeout = connectTimeout;
            return api;
        }

        #endregion

        #region publish/subscribe


        //public IQueueAck Publish(QueueItem message)
        //{

        //    var ack = base.Enqueue(message, OnFault);
        //    return ack;
        //    //DequeueApi client = new DequeueApi(QueueDefaults.DequeuePipeName, true);
        //    //return client.Exec(request, QueueCmd.Dequeue);
        //}


        //public IEnumerable<IQueueAck> Publish(QueueItem message, TopicPublisher publisher)
        //{
        //    List<IQueueAck> acks = new List<IQueueAck>();
        //    foreach (var sub in publisher.Subscribers.Values)
        //    {
        //        message.Host = sub.Host;
        //        Protocol = sub.Protocol;
        //        var ack= base.Enqueue(message, OnFault);
        //        acks.Add(ack);
        //    }
        //    return acks.ToArray();
        //    //DequeueApi client = new DequeueApi(QueueDefaults.DequeuePipeName, true);
        //    //return client.Exec(request, QueueCmd.Dequeue);
        //}

        #endregion

        #region Enqueue

        

        //public IQueueAck Enqueue(QueueItem message)
        //{
        //    return Enqueue(message, ConnectTimeout);
        //}

        //public IQueueAck Enqueue(QueueItem message, int connectTimeout)
        //{
        //    //EnqueueApi client = new EnqueueApi(QueueDefaults.EnqueuePipeName, true);
        //    message.QCommand = QueueCmd.Enqueue;
        //    ConnectTimeout = connectTimeout;

        //   // var ack = Enqueue(message, connectTimeout,OnFault);//., ".", connectTimeout);


        //    message.MessageState = MessageState.Sending;
        //    TransStream ts = SendDuplexStream(message, connectTimeout, IsAsync);
        //    if (ts == null)
        //    {
        //        return null;
        //    }
        //    var ack = ts.ReadValue<QueueAck>(OnFault);


        //    //var ack = (IQueueAck)res;
        //    if (ack == null)
        //    {
        //        if (message.IsDuplex)
        //            ack = new QueueAck(MessageState.UnExpectedError, "Server was not responsed for this message", message.Identifier, message.Host);
        //        else
        //            ack = new QueueAck(MessageState.Arrived, "Message Arrived on way", message.Identifier, message.Host);

        //        //ack.HostAddress = message.HostAddress;
        //    }
        //    Assists.SetArrived(ack);
        //    return ack;
        //}

        public IQueueAck Enqueue(QueueItem message, int connectTimeout=0)
        {
            message.QCommand = QueueCmd.Enqueue;
            //message.Host = this._QueueName;

            return PublishItem(message, EnsureConnectTimeout(connectTimeout));

            //TransStream ts = PublishItem(message, connectTimeout, OnFault);

            //QueueAck ack = (ts == null) ? null : ts.ReadValue<QueueAck>(OnFault);

            //if (ack == null)
            //{
            //    if (message.IsDuplex)
            //        ack = new QueueAck(MessageState.UnExpectedError, "Server was not responsed for this message", message.Identifier, message.Host);
            //    else
            //        ack = new QueueAck(MessageState.Arrived, "Message Arrived on way", message.Identifier, message.Host);

            //    //ack.HostAddress = message.HostAddress;
            //}

            //Assists.SetArrived(ack);
            //return ack;
        }

        public void EnqueueAsync(QueueItem message, int connectTimeout, Action<IQueueAck> onCompleted)
        {
            message.QCommand = QueueCmd.Enqueue;
            //message.Host = this._QueueName;
            //message.MessageState = MessageState.Sending;

            PublishItem(message, EnsureConnectTimeout(connectTimeout), onCompleted);

            //PublishItem(message, connectTimeout, OnFault, (TransStream ts) =>
            //{
            //    QueueAck ack = (ts == null) ? null: ts.ReadValue<QueueAck>(OnFault) ;

            //    if (ack == null)
            //    {
            //        if (message.IsDuplex)
            //            ack = new QueueAck(MessageState.UnExpectedError, "Server was not responsed for this message", message.Identifier, message.Host);
            //        else
            //            ack = new QueueAck(MessageState.Arrived, "Message Arrived on way", message.Identifier, message.Host);

            //        //ack.HostAddress = message.HostAddress;
            //    }
 
            //    Assists.SetArrived(ack);

            //    onCompleted(ack);
            //});
        }
        #endregion

        #region Dequeue

        //public IQueueItem Dequeue(QueueRequest message)
        //{
        //    return Dequeue(message, ConnectTimeout);
        //}

        public IQueueItem Dequeue(int connectTimeout = 0)
        {
            QueueRequest message = new QueueRequest()
            {
                QCommand = QueueCmd.Dequeue,
                Host = QueueName,
                DuplexType = DuplexTypes.WaitOne
            };

            return RequestItem(message, connectTimeout);

        }

        public IQueueItem Dequeue(QueueRequest message, int connectTimeout=0)
        {
            message.QCommand = QueueCmd.Dequeue;
            //message.Host = this._QueueName;

            return RequestItem(message, connectTimeout);

        }
 
        public void DequeueAsync(QueueRequest message, int connectTimeout, Action<IQueueItem> onCompleted, IDynamicWait aw)
        {
            message.QCommand = QueueCmd.Dequeue;
            //message.Host = this._QueueName;
            //message.MessageState = MessageState.Sending;

            RequestItem(message, connectTimeout, onCompleted, aw);

        }
        //public void DequeueAsync(QueueRequest message, int connectTimeout, Action<IQueueItem> onCompleted, Action<bool> onAck, AutoResetEvent resetEvenet)
        //{
        //    message.QCommand = QueueCmd.Dequeue;
        //    //message.Host = this._QueueName;
        //    //message.MessageState = MessageState.Sending;

        //    ConsumItem(message, connectTimeout, onCompleted, onAck, resetEvenet);

        //}
        public IQueueItem Dequeue(Priority priority)
        {
            QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.DequeuePriority, null);
            {
                Host = QueueName,
                QCommand = QueueCmd.DequeuePriority
            };
            request.Priority = priority;

            return Dequeue(request);
        }

        //public IQueueItem Dequeue(DuplexTypes DuplexType = DuplexTypes.WaitOne)
        //{
        //    QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.Dequeue, null);
        //    {
        //        Host = _QueueName,
        //        QCommand = QueueCmd.Dequeue,
        //        DuplexType = DuplexType
        //    };

        //    return Dequeue(request);
        //}
        #endregion

        #region Consume
        public IQueueItem Consume(int maxWaitSecond)
        {
            QueueRequest message = new QueueRequest()
            {
                QCommand = QueueCmd.Consume,
                Host = QueueName,
                DuplexType = DuplexTypes.WaitOne
            };

            return ConsumeItem(message, maxWaitSecond);

        }
        #endregion

        #region Peek

        //public IQueueItem Peek(QueueRequest message)
        //{
        //    return Peek(message, ConnectTimeout);
        //}

        public IQueueItem Peek(int connectTimeout = 0)
        {
            QueueRequest message = new QueueRequest()
            {
                QCommand = QueueCmd.Peek,
                Host = QueueName,
            };

            return RequestItem(message, connectTimeout);

        }

        public IQueueItem Peek(QueueRequest message, int connectTimeout=0)
        {
            message.QCommand = QueueCmd.Peek;
            //message.Host = this._QueueName;

            return RequestItem(message, connectTimeout);

        }

        public void PeekAsync(QueueRequest message, int connectTimeout, Action<IQueueItem> onCompleted)
        {
            message.QCommand = QueueCmd.Peek;
            //message.Host = this._QueueName;
            //message.MessageState = MessageState.Sending;

            //void OnAck(bool ack) { }

            RequestItem(message, connectTimeout, onCompleted, DynamicWait.Empty);

        }
        #endregion

        #region Send

        public IQueueAck SendAsync(QueueItem message, int connectTimeout)
        {
            using (

                    Task<IQueueAck> task = Task<IQueueAck>.Factory.StartNew(() =>
                        PublishItem(message, EnsureConnectTimeout(connectTimeout))
                    ,
                    canceller.Token,
                    TaskCreationOptions.None,
                    TaskScheduler.Default))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    IQueueAck ack = task.Result;
                    return ack;
                }
                else if (task.IsCanceled)
                {
                    return new QueueAck(MessageState.OperationCanceled, message);
                }
                else if (task.IsFaulted)
                {
                    return new QueueAck(MessageState.OperationFailed, message);
                }
                else
                {
                    return new QueueAck(MessageState.UnExpectedError, message);
                }
            }
        }

        public void SendAsync(QueueItem message, int connectTimeout, Action<IQueueAck> action)
        {
            using (

                    Task<IQueueAck> task = Task<IQueueAck>.Factory.StartNew(() =>
                        PublishItem(message, EnsureConnectTimeout(connectTimeout))
                    ,
                    canceller.Token,
                    TaskCreationOptions.None,
                    TaskScheduler.Default))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    action(task.Result);

                    //IQueueAck item = task.Result;
                    //if (item != null)
                    //{
                    //    if (action != null)
                    //        Task.Factory.StartNew(() => action(item));
                    //}
                }
                else if (task.IsCanceled)
                {
                    if (action != null)
                        action(new QueueAck(MessageState.OperationCanceled, message));
                }
                else if (task.IsFaulted)
                {
                    if (action != null)
                        action(new QueueAck(MessageState.OperationFailed, message));
                }
                else
                {
                    if (action != null)
                        action(new QueueAck(MessageState.UnExpectedError, message));
                }
            }
        }

        public Task<IQueueAck> SendAsyncTask(QueueItem message, int connectTimeout)
        {
            Task<IQueueAck> task = Task<IQueueAck>.Factory.StartNew(() =>
                PublishItem(message, EnsureConnectTimeout(connectTimeout))
            ,
            canceller.Token,
            TaskCreationOptions.None,
            TaskScheduler.Default);
                task.Wait();
            return task;
        }
        #endregion

        #region Receive

        /*

        //bool KeepAlive = false;
        //public void ListenerStart()
        //{
        //    KeepAlive = true;
        //}
        //public void ListenerStop()
        //{
        //    KeepAlive = false;
        //}
        //public void Listener(int connectTimeout, Action<IQueueItem> action)
        //{
        //    KeepAlive = true;
        //    ConnectTimeout = connectTimeout;
        //    while (KeepAlive)
        //    {
        //        var message = Receive();// connectTimeout);
        //        if (message != null)
        //            action(message);
        //        Thread.Sleep(100);
        //    }
        //}

        //public void ReceiveAsync(Action<string> onFault, Action<QueueItem> onCompleted,DuplexTypes DuplexType, AutoResetEvent resetEvent)// = DuplexTypes.WaitOne)
        //{
        //    QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.Dequeue, null);
        //    {
        //        Host = _QueueName,
        //        QCommand = QueueCmd.Dequeue,
        //        DuplexType = DuplexType
        //    };

        //    base.SendDuplexAsync(request, onFault, onCompleted, resetEvent);


        //    //var response = base.SendDuplexAsync(request, OnFault);
        //    //Assists.SetDuration(response);
        //    //return response;// == null ? null : response.ToMessage();

        //    //DequeueApi client = new DequeueApi(QueueDefaults.DequeuePipeName, true);
        //    //return client.Exec(request, QueueCmd.Dequeue);
        //}

        public IQueueItem Receive(DuplexTypes DuplexType= DuplexTypes.WaitOne)
        {
            QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.Dequeue, null);
            {
                Host = _QueueName,
                QCommand = QueueCmd.Dequeue,
                DuplexType= DuplexType
            };
            

            var response = base.SendDuplex(request,OnFault);
            Assists.SetReceived(response, MessageState.Received);
            return response;// == null ? null : response.ToMessage();

            //DequeueApi client = new DequeueApi(QueueDefaults.DequeuePipeName, true);
            //return client.Exec(request, QueueCmd.Dequeue);
        }
        public IQueueItem Receive(QueueRequest request)
        {
            var response = base.SendDuplex(request, OnFault);
            Assists.SetReceived(response, MessageState.Received);
            return response;// == null ? null : response.ToMessage();
            //DequeueApi client = new DequeueApi(QueueDefaults.DequeuePipeName, true);
            //return client.Exec(request, QueueCmd.Dequeue, _ServerName, connectTimeout);
        }
        //public IQueueItem Receive(int connectTimeout)
        //{
        //    QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.Dequeue, null);
        //    {
        //        Host = _QueueName,
        //        QCommand = QueueCmd.Dequeue,
        //        DuplexType = DuplexTypes.NoWaite
        //    };
        //    var response = base.SendDuplex(request, OnFault);
        //    Assists.SetReceived(response, MessageState.Received);
        //    return response;// == null ? null : response.ToMessage();
        //    //DequeueApi client = new DequeueApi(QueueDefaults.DequeuePipeName, true);
        //    //return client.Exec(request, QueueCmd.Dequeue, _ServerName, connectTimeout);
        //}

        public IQueueItem Receive(Priority priority)
        {
            QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.DequeuePriority, null);
            {
                Host = _QueueName,
                QCommand = QueueCmd.DequeuePriority
            };
            request.Priority = priority;
            var response = base.SendDuplex(request, OnFault);
            Assists.SetReceived(response, MessageState.Received);
            return response;// == null ? null : response.ToMessage();
            //DequeueApi client = new DequeueApi(QueueDefaults.DequeuePipeName, true);
            //return client.Exec(request, QueueCmd.DequeuePriority);
        }

        public int Receive(int connectTimeout, Action<IQueueItem> action)
        {
            ConnectTimeout = connectTimeout;
            var message = Receive();
            if (message == null)
                return 0;
            action(message);
            return 1;
        }

        public IQueueItem Peek()
        {
            QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.Peek, null);
            {
                Host = _QueueName,
                QCommand = QueueCmd.Peek
            };
            var response = base.SendDuplex(request, OnFault);
            Assists.SetReceived(response, MessageState.Peeked);
            return response;// == null ? null : response.ToMessage();
            //DequeueApi client = new DequeueApi(QueueDefaults.DequeuePipeName, true);
            //return client.Exec(request, QueueCmd.Peek);
        }

        public IQueueItem Peek(Priority priority)
        {
            QueueRequest request = new QueueRequest()//_QueueName, QueueCmd.PeekPriority, null);
            {
                Host = _QueueName,
                QCommand = QueueCmd.PeekPriority
            };
            request.Priority = priority;
            var response = base.SendDuplex(request, OnFault);
            Assists.SetReceived(response, MessageState.Peeked);
            return response;// == null ? null : response.ToMessage();
            //DequeueApi client = new DequeueApi(QueueDefaults.DequeuePipeName, true);
            //return client.Exec(request, QueueCmd.PeekPriority);
        }
        */
        #endregion

        #region Recieve async
        /*        
        public void ReceiveAsync(int connectTimeout, Action<IQueueItem> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("ReceiveAsync.action");
            }

            ConnectTimeout = connectTimeout;
            using (
                
                    Task<IQueueItem> task = Task<IQueueItem>.Factory.StartNew(()=>
                        Receive()
                    ,
                    canceller.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    IQueueItem item = task.Result;
                    if (item != null)
                    {
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

        public bool ReceiveAsync(int connectTimeout, out IQueueItem message)
        {
            ConnectTimeout = connectTimeout;
            using (

                    Task<IQueueItem> task = Task<IQueueItem>.Factory.StartNew(() =>
                        Receive()
                    ,
                    canceller.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    message = task.Result;
                    return true;
                }
                message = null;
                return false;
            }
        }
        */
        #endregion

        #region Report

        public IQueueItem Report(QueueCmdReport command, string host)
        {
            QueueRequest request = new QueueRequest()
            {
                QCommand = (QueueCmd)(int)command,
                Host = host
            };

            var ack = ConsumeItem(request, ConnectTimeout);
            if (ack == null)
            {
                ack = new QueueItem()//MessageState.UnExpectedError, "Server was not responsed for this message", command.ToString(), host);
                {
                    MessageState = MessageState.UnExpectedError,
                    Label = "Server was not responsed for this message",
                    Host = host
                };
            }
            return ack;
        }

        public void ReportAsync(QueueCmdReport command, string host, Action<IQueueItem> action)
        {
            using (

                    Task<IQueueItem> task = Task<IQueueItem>.Factory.StartNew(() =>
                        Report(command, host)
                    ,
                    canceller.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default))
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    IQueueItem item = task.Result;
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

        public void Commit(Ptr ptr)
        {
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, false);

            var message = new QueueItem()
            {
                Host = ptr.Host,
                QCommand = QueueCmd.Commit,
                MessageState= MessageState.TransCommited,
                Identifier=ptr.Identifier,
                Retry = (byte)ptr.Retry
            };

            //var message=new Message(ptr, QueueCmd.Commit, MessageState.TransCommited, null);
            //client.Exec(message, QueueCmd.Commit);
            base.SendOut(message);
        }

        public void Abort(Ptr ptr)
        {
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, false);
            var message = new QueueItem()
            {
                Host = ptr.Host,
                QCommand = QueueCmd.Abort,
                MessageState = MessageState.TransAborted,
                Identifier = ptr.Identifier,
                Retry=(byte)ptr.Retry
            };
            //client.Exec(message, QueueCmd.Abort);
            base.SendOut(message);
        }


        public IQueueItem Report(QueueCmdReport cmd)
        {
            QueueRequest request = new QueueRequest()
            {
                Host = QueueName,
                QCommand = (QueueCmd)(int)cmd,
                //Command = (QueueCmd)(int)cmd
            };
            var response = RequestItem(request, ConnectTimeout);
            return response;// == null ? null : response.ToMessage();
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, (QueueCmd)(int)cmd);
        }

        public T Report<T>( QueueCmdReport cmd)
        {
            QueueRequest request = new QueueRequest()
            {
                Host = QueueName,
                QCommand = (QueueCmd)(int)cmd,
                //Command = (QueueCmd)(int)cmd
            };
            var res = RequestItem(request, ConnectTimeout);
            //var res= response == null ? null : response.ToMessage();

            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //var res = client.Exec(message, (QueueCmd)(int)cmd);
            if (res == null)
                return default(T);
            return res.GetBody<T>();
        }

        public IQueueItem OperateQueue(QueueCmdOperation cmd)
        {
            QueueRequest message = new QueueRequest()//queueName, (QueueCmd)(int)cmd)
            {
                Host = QueueName,
                QCommand = (QueueCmd)(int)cmd,
                //Command = (QueueCmd)(int)cmd
            };
            var response= RequestItem(message, ConnectTimeout);
            return response;//==null? null: response.ToMessage();
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, (QueueCmd)(int)cmd);
        }

        public IQueueItem AddQueue(QProperties qp)
        {
            var message = new QueueRequest()
            {
                Host = QueueName,
                QCommand = QueueCmd.AddQueue,
            };

            message.SetBody(qp.GetEntityStream(false), qp.GetType().FullName);
            var response = RequestItem(message, ConnectTimeout);
            return response;// == null ? null : response.ToMessage();
        }

        public IQueueItem AddQueue(CoverMode mode, bool isTrans, bool isTopic)
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
                IsTopic=isTopic
            } ;
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
                Host = QueueName,
                QCommand = QueueCmd.RemoveQueue,
            };
            var response= RequestItem(message, ConnectTimeout);
            return response;// == null ? null : response.ToMessage();

            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, QueueCmd.RemoveQueue);
        }

        public IQueueItem QueueExists()
        {
            QueueRequest message = new QueueRequest()
            {
                Host = QueueName,
                QCommand = QueueCmd.Exists,
            };
            var response= RequestItem(message, ConnectTimeout);
            return response;// == null ? null : response.ToMessage();
            //ReportApi client = new ReportApi(QueueDefaults.QueueManagerPipeName, true);
            //return (Message)client.Exec(message, QueueCmd.RemoveQueue);
        }


        #endregion
        
        #region Recieve to

        //public IQueueAck ReceiveTo(QueueHost target, int connectTimeout, Action<IQueueItem> recieveAction)
        //{
        //    IQueueAck ack = null;

        //    using (

        //            Task<IQueueItem> task = Task<IQueueItem>.Factory.StartNew(() =>
        //                Receive(connectTimeout)
        //            ,
        //            canceller.Token,
        //            TaskCreationOptions.LongRunning,
        //            TaskScheduler.Default))
        //    {
        //        task.Wait();
        //        if (task.IsCompleted)
        //        {
        //            var message =(QueueItem) task.Result;//.ToMessage();

        //            if (message != null)
        //            {
        //                message.Host = target.RawHostAddress;
        //                var response=base.SendDuplex(message);
                        
        //                //RemoteClient client = new RemoteClient(target);
        //                //ack = client.Send(message, connectTimeout);

        //                if (recieveAction != null)
        //                    Task.Factory.StartNew(() => recieveAction(response));
        //                return ack;
        //            }

        //            //return 0;
        //        }
        //        return null;
        //    }
        //}

        #endregion

        #region  asyncInvoke

        private AsyncCallback onRequestCompleted;
        private ManualResetEvent resetEvent;
        public event ReceiveMessageCompletedEventHandler ReceiveCompleted;

        private ManualResetEvent ResetEvent
        {
            get
            {
                if (resetEvent == null)
                    resetEvent = new ManualResetEvent(false);
                return resetEvent;
            }
        }

        protected virtual void OnReceiveCompleted(ReceiveMessageCompletedEventArgs e)
        {
            if (ReceiveCompleted != null)
                ReceiveCompleted(this, e);
        }


        private IQueueItem ReceiveItemWorker(TimeSpan timeout, object state)
        {
            IQueueItem item = null;
            TimeOut to = new TimeOut(timeout);
            while (item == null)
            {
                if (to.IsTimeOut())
                {
                    state = (int)ReceiveState.Timeout;
                    break;
                }
                item = Dequeue();// this.Receive();
                if (item == null)
                {
                    Thread.Sleep(100);
                }
            }
            if (item != null)
            {
                state = (int)ReceiveState.Success;
                Console.WriteLine("Dequeue item :{0}", item.Identifier);
            }
            return item;
        }

        public IQueueItem AsyncReceive()
        {
            return AsyncReceive(null);
        }

        public IQueueItem AsyncReceive(object state)
        {
            if (state == null)
            {
                state = new object();
            }
            TimeSpan timeout = TimeSpan.FromMilliseconds(QueueApi.LongTimeout);
            ReceiveMessageCallback caller = new ReceiveMessageCallback(this.ReceiveItemWorker);

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(timeout, state, CreateCallBack(), caller);

            result.AsyncWaitHandle.WaitOne();

            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
            IQueueItem item = caller.EndInvoke(result);
            AsyncCompleted(item);
            return item;

        }

        public IAsyncResult BeginReceive(object state)
        {
            return BeginReceive(TimeSpan.FromMilliseconds(QueueApi.LongTimeout), state, null);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, object state)
        {
            return BeginReceive(timeout, state, null);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, object state, AsyncCallback callback)
        {

            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if ((totalMilliseconds < 0L) || (totalMilliseconds > 4294967295L))
            {
                throw new ArgumentException("InvalidParameter", "timeout");
            }
            ReceiveMessageCallback caller = new ReceiveMessageCallback(ReceiveItemWorker);

            if (callback == null)
            {
                callback = CreateCallBack();
            }
            if (state == null)
            {
                state = new object();
            }
            state = (int)ReceiveState.Wait;

            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke(timeout, state, callback, caller);

            this.ResetEvent.Set();
            //OnReceiveCompleted(new ReceiveResultEventArgs(this, result));
            return result;
        }


        // Callback method must have the same signature as the
        // AsyncCallback delegate.
        public IQueueItem EndReceive(IAsyncResult asyncResult)
        {

            // Retrieve the delegate.
            ReceiveMessageCallback caller = (ReceiveMessageCallback)asyncResult.AsyncState;

            // Call EndInvoke to retrieve the results.
            IQueueItem item = (IQueueItem)caller.EndInvoke(asyncResult);

            AsyncCompleted(item);
            this.ResetEvent.WaitOne();
            return item;
        }

        private AsyncCallback CreateCallBack()
        {
            if (this.onRequestCompleted == null)
            {
                this.onRequestCompleted = new AsyncCallback(this.OnRequestCompleted);
            }
            return this.onRequestCompleted;
        }

        private void AsyncCompleted(IQueueItem item)
        {
            //if (item != null)
            //{
            //    if (item != null && IsTrans)
            //    {
            //        //this.TransBegin(item);
            //    }
            //    else
            //    {
            //        this.Completed(item.ItemId, (int)ItemState.Commit);
            //    }
            //}
        }

        private void OnRequestCompleted(IAsyncResult asyncResult)
        {
            OnReceiveCompleted(new ReceiveMessageCompletedEventArgs(this, asyncResult));
        }

        #endregion


#if(false)
        #region items
        /// <summary>
        /// Reply for test
        /// </summary>
        /// <returns></returns>
        public string Reply(string text)
        {
            return SendDuplex<string>(QueueCmd.Reply, text);
        }
        
       
       /// <summary>
       /// Remove item from queue
       /// </summary>
       /// <param name="queueKey"></param>
        /// <returns>return <see cref="QueueState"/></returns>
        /// <example>
        /// <code>
        /// //Remove item from queue.
        ///public void RemoveItem()
        ///{
        ///    var state = QueueApi.RemoveItem("item key 3");
        ///    Console.WriteLine(state);
        ///}
        /// </code>
        /// </example>
        public MessageState RemoveItem(string queueKey)
        {
            return (MessageState)SendDuplex<int>(QueueCmd.RemoveItem, queueKey);
        }

        /// <summary>
        /// Remove item from queue asynchronizly
        /// </summary>
        /// <param name="queueKey"></param>
        public void RemoveItemAsync(string queueKey)
        {
            SendOut(QueueCmd.RemoveItemAsync, queueKey);
        }

        /// <summary>
        /// Get value from queue as <see cref="NetStream"/>
        /// </summary>
        /// <param name="queueKey"></param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// //Get item value from queue.
        ///public void GetStream()
        ///{
        ///    string key = "item key 1";
        ///    <![CDATA[var item = QueueApi.GetStream(key);]]>
        ///    Print(item, key);
        ///}
        /// </code>
        /// </example>
        public NetStream GetStream(string queueKey)
        {
            return SendDuplex<NetStream>(QueueCmd.GetValue, queueKey);
        }
        /// <summary>
        /// Get value from queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueKey"></param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// //Get item value from queue.
        ///public void GetValue()
        ///{
        ///    string key = "item key 1";
        ///    <![CDATA[var item = QueueApi.GetValue<EntitySample>(key);]]>
        ///    Print(item, key);
        ///}
        /// </code>
        /// </example>
        public T GetValue<T>(string queueKey)
        {
            return SendDuplex<T>(QueueCmd.GetValue, queueKey);
        }

        /// <summary>
        /// Get value from queue.
        /// </summary>
        /// <param name="queueKey"></param>
        /// <returns></returns>
        public object GetValue(string queueKey)
        {
            return SendDuplex(QueueCmd.GetValue, queueKey,typeof(object).FullName);
        }

        /// <summary>
        /// Fetch Value from queue (Cut item from queue)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueKey"></param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// //Fetch item value from queue.
        ///public void FetchValue()
        ///{
        ///    string key = "item key 2";
        ///    <![CDATA[var item = QueueApi.FetchValue<EntitySample>(key);]]>
        ///    Print(item, key);
        ///}
        /// </code>
        /// </example>
        public T FetchValue<T>(string queueKey)
        {
            return SendDuplex<T>(QueueCmd.FetchValue, queueKey);
        }
        /// <summary>
        /// Load data from db to queue or get it if exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionKey"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="keyValueParameters"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public T LoadData<T>(string connectionKey, string commandText, CommandType commandType, int commandTimeout, object[] keyValueParameters, int expiration)
        {
            CommandContext item = new CommandContext(connectionKey, commandText, commandType, commandTimeout,typeof(T));
            item.CreateParameters(keyValueParameters);
            if (item == null)
                return default(T);
            using (var message = new QueueMessage(QueueCmd.LoadData, item.CreateKey(), item, expiration))
            {
                return SendDuplex<T>(message);
            }
        }
       
        /// <summary>
        /// Add new item to queue
        /// </summary>
        /// <param name="item"></param>
        /// <returns>return <see cref="QueueState"/></returns>
        public QueueState AddItem(QueueEntry item)
        {
            if (item == null || item.IsEmpty)
                return QueueState.ArgumentsError;
            using (var message = new QueueMessage() { Command = QueueCmd.AddItem, Key = item.Key, BodyStream = item.BodyStream })
            {
                return (QueueState)SendDuplex<int>(message);
            }
        }
        /// <summary>
        /// Add new item to queue
        /// </summary>
        /// <param name="queueKey"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="QueueState"/></returns>
        /// <example>
        /// <code>
        /// //Add items to remote queue.
        ///public void AddItems()
        ///{
        ///    int timeout = 30;
        ///    QueueApi.AddItem("item key 1", new EntitySample() { Id = 123, Name = "entity sample 1", Creation = DateTime.Now, Value = "entity item one" }, timeout);
        ///    QueueApi.AddItem("item key 2", new EntitySample() { Id = 124, Name = "entity sample 2", Creation = DateTime.Now, Value = "entity item second" }, timeout);
        ///    QueueApi.AddItem("item key 3", new EntitySample() { Id = 125, Name = "entity sample 3", Creation = DateTime.Now, Value = "entity item minute" }, timeout);
        ///}
        /// </code>
        /// </example>
        public QueueState AddItem(string queueKey, object value, int expiration)
        {
            if (value == null)
                return QueueState.ArgumentsError;

            using (var message = new QueueMessage(QueueCmd.AddItem, queueKey, value, expiration))
            {
                return (QueueState)SendDuplex<int>(message);
            }
        }
        /// <summary>
        /// Add new item to queue
        /// </summary>
        /// <param name="queueKey"></param>
        /// <param name="value"></param>
        /// <param name="sessionId"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="QueueState"/></returns>
        public QueueState AddItem(string queueKey, object value, string sessionId, int expiration)
        {
            if (value == null)
                return QueueState.ArgumentsError;

            using (var message = new QueueMessage(QueueCmd.AddItem, queueKey, value, expiration, sessionId))
            {
                return (QueueState)SendDuplex<int>(message);
            }
        }

        /// <summary>
        /// Get item copy from queue
        /// </summary>
        /// <param name="queueKey"></param>
        /// <returns>return <see cref="QueueEntry"/></returns>
        public QueueEntry ViewItem(string queueKey)
        {
            return SendDuplex<QueueEntry>(QueueCmd.ViewItem, queueKey);
        }

        #endregion

        #region Copy and merge
        /// <summary>
        /// Copy item in queue from source to another destination.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="QueueState"/></returns>
        /// <example>
        /// <code>
        /// //Duplicate existing item from queue to a new destination.
        ///public void CopyItem()
        ///{
        ///    string source = "item key 1";
        ///    string dest = "item key 2";
        ///    var state = QueueApi.CopyItem(source, dest, timeout);
        ///    Console.WriteLine(state);
        ///}
        /// </code>
        /// </example>
        public QueueState CopyItem(string source, string dest, int expiration)
        {
            using (var message = new QueueMessage()
            {
                Command = QueueCmd.CopyItem,
                Args = MessageStream.CreateArgs(KnowsArgs.Source, source, KnowsArgs.Host, dest),
                Expiration = expiration,
                IsDuplex = false,
                Key = dest
            })
            {
                return (QueueState)SendDuplex<int>(message);
            }
        }
        /// <summary>
        /// Cut item in queue from source to another destination.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="expiration"></param>
        /// <returns>return <see cref="QueueState"/></returns>
        /// <example>
        /// <code>
        /// //Duplicate existing item from queue to a new destination and remove the old one.
        ///public void CutItem()
        ///{
        ///    string source = "item key 2";
        ///    string dest = "item key 3";
        ///    var state = QueueApi.CutItem(source, dest, timeout);
        ///    Console.WriteLine(state);
        ///}
        /// </code>
        /// </example>
        public QueueState CutItem(string source, string dest, int expiration)
        {

            using (var message = new QueueMessage()
            {
                Command = QueueCmd.CutItem,
                Args = MessageStream.CreateArgs(KnowsArgs.Source, source, KnowsArgs.Host, dest),
                Expiration = expiration,
                IsDuplex = false,
                Key = dest
            })
            {
                return (QueueState)SendDuplex<int>(message);
            }
        }
    
        #endregion

        /// <summary>
        /// Remove all session items from queue.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>return number of items removed from queue.</returns>
        public int RemoveQueueSessionItems(string sessionId)
        {
            if (sessionId == null)
                return -1;
            using (var message = new QueueMessage() { Command = QueueCmd.RemoveQueueSessionItems, Key = sessionId })
            {
                return SendDuplex<int>(message);
            }
        }
        /// <summary>
        /// Keep Alive Queue Item.
        /// </summary>
        /// <param name="queueKey"></param>
        public void KeepAliveItem(string queueKey)
        {
            if (queueKey == null)
                return;
            using (var message = new QueueMessage() { Command = QueueCmd.KeepAliveItem, Key = queueKey })
            {
                SendOut(message);
            }
        }

#endif
    }
}
