using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Channels;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Serialization;
using Nistec.Messaging.Channels;
using Nistec.Runtime;
using Nistec.Channels.Http;
using Nistec.Channels.Tcp;
using System.Threading;
using System.Threading.Tasks;

namespace Nistec.Messaging.Remote
{

    //internal static class PollRepeat
    //{
    //    public static Task Interval(
    //        TimeSpan pollInterval,
    //        Func<TransStream> action,
    //        CancellationToken token)
    //    {
    //        // We don't use Observable.Interval:
    //        // If we block, the values start bunching up behind each other.
    //        return Task.Factory.StartNew(
    //            () =>
    //            {
    //                for (;;)
    //                {
    //                    if (token.WaitCancellationRequested(pollInterval))
    //                        break;

    //                  var ts=  action();
    //                  bool  ok = (ts != null && ts.GetLength() > 0);
    //                    if (ok)
    //                    {
    //                        break;
    //                    }
    //                }
    //            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    //    }
    //}

    //static class CancellationTokenExtensions
    //{
    //    public static bool WaitCancellationRequested(
    //        this CancellationToken token,
    //        TimeSpan timeout)
    //    {
    //        return token.WaitHandle.WaitOne(timeout);
    //    }
    //}

    public abstract class RemoteApi
    {
        public const int DefaultTimeout = 5000;
        public const int DefaultWaitInterval = 100;


        protected NetProtocol Protocol;
        protected string RemoteHostAddress;
        protected int RemoteHostPort;
        protected bool EnableRemoteException;

        bool _IsAsync = false;
        public bool IsAsync { get { return _IsAsync; } set { _IsAsync = value; } }

        int _WaitInterval = DefaultWaitInterval;
        public int WaitInterval { get { return _WaitInterval; } set { _WaitInterval = value<=10? DefaultWaitInterval: value; } }
        int _Timeout = DefaultTimeout;
        public int Timeout { get { return _Timeout; } set { _Timeout = value <0 ? DefaultTimeout : value; } }

        #region members

        /// <summary>
        /// No limit timeout.
        /// </summary>
        public const int InfiniteTimeout = 0;
        /// <summary>
        /// 5 minute timeout.
        /// </summary>
        public const int ShortTimeout = 307200;//5 minute
        /// <summary>
        /// 30 minute timeout.
        /// </summary>
        public const int LongTimeout = 1843200;//30 minute

        protected string _QueueName;
        //string _ServerName = ".";
        //string _HostAddress;
        protected HostProtocol _HostProtocol;
        public bool IsCoverable { get; set; }

        #endregion


        /// <summary>
        /// CConvert stream to json format.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToJson(NetStream stream, JsonFormat format)
        {
            using (BinaryStreamer streamer = new BinaryStreamer(stream))
            {
                var obj = streamer.Decode();
                if (obj == null)
                    return null;
                else
                    return JsonSerializer.Serialize(obj, null, format);
            }
        }

        public int GetTimeout(int timeout, int protocolTimeout)
        {
            if (timeout > 0)
                return timeout;
            return protocolTimeout;
        }

        public int GetTimeout(int timeout)
        {
            if (timeout > 0)
                return timeout;

            switch (Protocol)
            {
                case NetProtocol.Http:
                    return ChannelSettings.HttpTimeout;

                case NetProtocol.Pipe:
                    return DefaultTimeout;

                case NetProtocol.Tcp:
                    return ChannelSettings.TcpTimeout;
                default:
                    return DefaultTimeout;
            }
        }

        #region internal
        /*
        internal QueueAck Enqueue(QueueItem message, int timeout = 0)
        {
            message.Host = this._QueueName;
            //QueueItem qs = new QueueItem(message);

            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientQueue.Enqueue(message, RemoteHostAddress, ChannelSettings.HttpMethod, GetTimeout(timeout, ChannelSettings.HttpTimeout), ChannelSettings.IsAsync, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientQueue.Enqueue(message, RemoteHostAddress, ChannelSettings.IsAsync? System.IO.Pipes.PipeOptions.Asynchronous: System.IO.Pipes.PipeOptions.None, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientQueue.Enqueue(message, RemoteHostAddress, RemoteHostPort, GetTimeout(timeout, ChannelSettings.TcpTimeout), ChannelSettings.IsAsync, EnableRemoteException);
        }

        internal QueueItem SendDuplex(IQueueMessage message, int timeout = 0)
        {
            message.Host = this._QueueName;
            //QueueItem qs = new QueueItem(message);

            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClientQueue.SendDuplex(message, RemoteHostAddress, ChannelSettings.HttpMethod, GetTimeout(timeout, ChannelSettings.HttpTimeout), ChannelSettings.IsAsync, EnableRemoteException);

                case NetProtocol.Pipe:
                    return PipeClientQueue.SendDuplex(message, RemoteHostAddress, ChannelSettings.IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None, EnableRemoteException);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClientQueue.SendDuplex(message, RemoteHostAddress, RemoteHostPort, GetTimeout(timeout, ChannelSettings.TcpTimeout), ChannelSettings.IsAsync, EnableRemoteException);
        }
        
        internal void SendOut(QueueItem message, int timeout = 0)
        {
            //message.Host = this._QueueName;
            //QueueItem qs = new QueueItem(message);

            switch (Protocol)
            {
                case NetProtocol.Http:
                    HttpClientQueue.SendOut(message, RemoteHostAddress, ChannelSettings.HttpMethod, GetTimeout(timeout, ChannelSettings.HttpTimeout), ChannelSettings.IsAsync, EnableRemoteException);
                    break;
                case NetProtocol.Pipe:
                    PipeClientQueue.SendOut(message, RemoteHostAddress, ChannelSettings.IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None, EnableRemoteException);
                    break;
                case NetProtocol.Tcp:
                default:
                    TcpClientQueue.SendOut(message, RemoteHostAddress, RemoteHostPort, GetTimeout(timeout, ChannelSettings.TcpTimeout), ChannelSettings.IsAsync, EnableRemoteException);
                    break;
            }
        }

        //internal QueueItem Management(QueueRequest message, int timeout = 0)
        //{
        //    message.Host = this._QueueName;
        //    //QueueItem qs = new QueueItem(message);

        //    switch (Protocol)
        //    {
        //        case NetProtocol.Http:
        //            return HttpClientQueue.Management(message, RemoteHostAddress, ChannelSettings.HttpMethod, GetTimeout(timeout, ChannelSettings.HttpTimeout), ChannelSettings.IsAsync, EnableRemoteException);

        //        case NetProtocol.Pipe:
        //            return PipeClientQueue.Management(message, RemoteHostAddress, ChannelSettings.IsAsync, EnableRemoteException);

        //        case NetProtocol.Tcp:
        //            break;
        //    }
        //    return TcpClientQueue.Management(message, RemoteHostAddress, ChannelSettings.TcpPort, GetTimeout(timeout, ChannelSettings.TcpTimeout), ChannelSettings.IsAsync, EnableRemoteException);
        //}

        //internal QueueItem SendDuplex(QueueRequest message, int timeout = 0)
        //{
        //    message.Host = this._QueueName;
        //    //QueueItem qs = new QueueItem(message);

        //    switch (Protocol)
        //    {
        //        case NetProtocol.Http:
        //            return HttpClientQueue.SendDuplex(message, RemoteHostAddress, ChannelSettings.HttpMethod, GetTimeout(timeout, ChannelSettings.HttpTimeout), ChannelSettings.IsAsync, EnableRemoteException);

        //        case NetProtocol.Pipe:
        //            return PipeClientQueue.SendDuplex(message, RemoteHostAddress, ChannelSettings.IsAsync, EnableRemoteException);

        //        case NetProtocol.Tcp:
        //            break;
        //    }
        //    return TcpClientQueue.SendDuplex(message, RemoteHostAddress, ChannelSettings.TcpPort, GetTimeout(timeout, ChannelSettings.TcpTimeout), ChannelSettings.IsAsync, EnableRemoteException);
        //}

        //internal void SendOut(QueueRequest message, int timeout = 0)
        //{
        //    message.Host = this._QueueName;
        //    //QueueItem qs = new QueueItem(message);

        //    switch (Protocol)
        //    {
        //        case NetProtocol.Http:
        //            HttpClientQueue.SendOut(message, RemoteHostAddress, ChannelSettings.HttpMethod, GetTimeout(timeout, ChannelSettings.HttpTimeout), ChannelSettings.IsAsync, EnableRemoteException);
        //            break;
        //        case NetProtocol.Pipe:
        //            PipeClientQueue.SendOut(message, RemoteHostAddress, ChannelSettings.IsAsync, EnableRemoteException);
        //            break;
        //        case NetProtocol.Tcp:
        //        default:
        //            TcpClientQueue.SendOut(message, RemoteHostAddress, ChannelSettings.TcpPort, GetTimeout(timeout, ChannelSettings.TcpTimeout), ChannelSettings.IsAsync, EnableRemoteException);
        //            break;
        //    }
        //}
        
            */
        #endregion

        internal QueueAck Enqueue(QueueItem message, Action<string> onFault)
        {
            message.Host = this._QueueName;

            //ChannelSettings.HttpTimeout;
            //ChannelSettings.IsAsync
            message.MessageState = MessageState.Sending;
            TransStream ts = SendDuplexStream(message, Timeout, IsAsync);
            if (ts == null)
            {
                onFault(message.QCommand.ToString() + " return null");
                return null;
            }
            return ts.ReadValue<QueueAck>(onFault);
        }


        internal TransStream SendWaitOneDuplex(QueueRequest message)
        {
            message.Host = this._QueueName;
            QueueCmd cmd = message.QCommand;
            //QueueItem qs = new QueueItem(message);
            TransStream ts = null;

            //Task.Factory.StartNew(() => SendItem(q, counter));

            bool ok = false;

            message.QCommand = QueueCmd.QueueHasValue;
            ts = SendDuplexStream(message, Timeout, IsAsync);
            if (ts != null && ts.ReadValue<int>() > 0)
            {
                message.QCommand = cmd;
                ts = SendDuplexStream(message, Timeout, IsAsync);
                ok = (ts != null && ts.GetLength() > 0);
                Console.WriteLine(ok);
            }



            return ts;//.ReadValue<QueueItem>(onFault);

        }

         internal void SendDuplexStreamAsync(QueueRequest message, Action<string> onFault, Action<TransStream> onCompleted, AutoResetEvent resetEvent)
        {
            message.Host = this._QueueName;
            QueueCmd cmd = message.QCommand;
            TransStream ts = null;

            var cancellationTokenSource = new CancellationTokenSource();

            if (message.DuplexType == DuplexTypes.WaitOne)
            {
                Task.Factory.StartNew(
                () =>
                {
                    for (;;)
                    {
                        if (cancellationTokenSource.Token.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitInterval)))
                        {
                            break;
                        }

                        message.QCommand = QueueCmd.QueueHasValue;
                        ts = SendDuplexStream(message, Timeout, IsAsync);
                        if (ts != null && ts.ReadValue<int>() > 0)
                        {
                            message.QCommand = cmd;
                            ts = SendDuplexStream(message, Timeout, IsAsync);
                            bool ok = (ts != null && ts.GetLength() > 0);
                            Console.WriteLine(ok);
                            if (ok)
                            {
                                //var res = ts.ReadValue<QueueItem>(onFault);
                                //Assists.SetReceived(res, cmd);
                                //Assists.SetArrived(res);
                                onCompleted(ts);
                                resetEvent.Set();
                                break;
                            }
                        }
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            }
            else
            {

                Task.Factory.StartNew(
               () =>
               {
                   if (cancellationTokenSource.Token.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitInterval)))
                   {
                       return;
                   }

                   message.QCommand = QueueCmd.QueueHasValue;
                   ts = SendDuplexStream(message, Timeout, IsAsync);
                   if (ts != null && ts.ReadValue<int>() > 0)
                   {
                       message.QCommand = cmd;
                       ts = SendDuplexStream(message, Timeout, IsAsync);
                       bool ok = (ts != null && ts.GetLength() > 0);
                       Console.WriteLine(ok);
                       if (ok)
                       {
                           //var res =ts.ReadValue<QueueItem>(onFault);
                           //Assists.SetReceived(res, cmd);
                           //Assists.SetArrived(res);
                           onCompleted(ts);
                           resetEvent.Set();
                           return;
                       }
                   }
               }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        internal void SendDuplexAsync(QueueRequest message, Action<string> onFault, Action<QueueItem> onCompleted,AutoResetEvent resetEvent)
        {
            message.Host = this._QueueName;
            QueueCmd cmd = message.QCommand;
            TransStream ts = null;

            var cancellationTokenSource = new CancellationTokenSource();

            if (message.DuplexType == DuplexTypes.WaitOne)
            {
                Task.Factory.StartNew(
                () =>
                {
                    for (;;)
                    {
                        if (cancellationTokenSource.Token.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitInterval)))
                        {
                            break;
                        }

                        message.QCommand = QueueCmd.QueueHasValue;
                        ts = SendDuplexStream(message, Timeout, IsAsync);
                        if (ts != null && ts.ReadValue<int>() > 0)
                        {
                            message.QCommand = cmd;
                            ts = SendDuplexStream(message, Timeout, IsAsync);
                            bool ok = (ts != null && ts.GetLength() > 0);
                            Console.WriteLine(ok);
                            if (ok)
                            {
                                var res = ts.ReadValue<QueueItem>(onFault);
                                Assists.SetReceived(res, cmd);
                                //Assists.SetArrived(res);
                                onCompleted(res);
                                resetEvent.Set();
                                break;
                            }
                        }
                        //if (!ok)
                        //{
                        //    Thread.Sleep(WaitInterval);
                        //}


                        //ts = SendWaitOneDuplex(message);
                        //bool ok = (ts != null && ts.GetLength() > 0);
                        //if (ok)
                        //{
                        //    break;
                        //}
                    }
                }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                //message.QCommand = QueueCmd.QueueHasValue;
                //ts = SendDuplexStream(message, Timeout, IsAsync);
                //if (ts != null && ts.ReadValue<int>() > 0)
                //{
                //    message.QCommand = cmd;
                //    ts = SendDuplexStream(message, Timeout, IsAsync);
                //    ok = (ts != null && ts.GetLength() > 0);
                //    Console.WriteLine(ok);
                //}
                //if (!ok)
                //{
                //    Thread.Sleep(WaitInterval);
                //}


                //return ts.ReadValue<QueueItem>(onFault);
            }
            else
            {

                Task.Factory.StartNew(
               () =>
               {

                   if (cancellationTokenSource.Token.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitInterval)))
                   {
                       return;
                   }

                   message.QCommand = QueueCmd.QueueHasValue;
                   ts = SendDuplexStream(message, Timeout, IsAsync);
                   if (ts != null && ts.ReadValue<int>() > 0)
                   {
                       message.QCommand = cmd;
                       ts = SendDuplexStream(message, Timeout, IsAsync);
                       bool ok = (ts != null && ts.GetLength() > 0);
                       Console.WriteLine(ok);
                       if (ok)
                       {
                           var res = ts.ReadValue<QueueItem>(onFault);
                           Assists.SetReceived(res, cmd);
                           //Assists.SetArrived(res);
                           onCompleted(res);
                           resetEvent.Set();
                           return;
                       }
                   }
               }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                //ts = SendDuplexStream(message, Timeout, IsAsync);
                //if (ts == null)
                //    onFault(message.QCommand.ToString() + " return null");
                ////return ts.ReadValue<QueueItem>(onFault);

                //var item = ts.ReadValue<QueueItem>(onFault);
                //Assists.SetReceived(item, cmd);
                //onCompleted(item);
            }
        }
        internal QueueItem SendDuplex(QueueRequest message, Action<string> onFault)
        {
            message.Host = this._QueueName;
            QueueCmd cmd = message.QCommand;
            //QueueItem qs = new QueueItem(message);
            TransStream ts = null;
            QueueItem qitem = null;

            if (message.DuplexType == DuplexTypes.WaitOne)
            {

                
                bool ok = false;
                while (!ok)
                {
                    //Task task = Task.Factory.Scheduler.StartNew(() => SendWaitOneDuplex(message));
                    //{
                    //    task.Wait(Timeout);
                    //    if (task.IsCompleted)
                    //    {
                    //        ok = (ts != null && ts.GetLength() > 0);
                    //        if (ok)
                    //            return ts.ReadValue<QueueItem>(onFault);
                    //    }
                    //}
                    //task.TryDispose();


                    message.QCommand = QueueCmd.QueueHasValue;
                    ts = SendDuplexStream(message, Timeout, IsAsync);
                    if (ts != null && ts.ReadValue<int>() > 0)
                    {
                        message.QCommand = cmd;
                        ts = SendDuplexStream(message, Timeout, IsAsync);
                        ok = (ts != null && ts.GetLength() > 0);
                    }
                    if (!ok)
                    {
                        Thread.Sleep(WaitInterval);
                    }
                }

                qitem = ts.ReadValue<QueueItem>(onFault);
                Assists.SetReceived(qitem, cmd);
                return qitem;
            }


            ts = SendDuplexStream(message, Timeout, IsAsync);
            if (ts == null)
                onFault(message.QCommand.ToString() + " return null");
            qitem= ts.ReadValue<QueueItem>(onFault);
            Assists.SetReceived(qitem, cmd);
            return qitem;
        }

        internal void SendOut(QueueItem message)
        {
            //message.Host = this._QueueName;
            //QueueItem qs = new QueueItem(message);

            switch (Protocol)
            {
                case NetProtocol.Http:
                    HttpClient.SendOut(message, RemoteHostAddress, RemoteHostPort, ChannelSettings.HttpMethod, GetTimeout(Timeout, ChannelSettings.HttpTimeout), EnableRemoteException);
                    break;
                case NetProtocol.Pipe:
                    PipeClient.SendOut(message, RemoteHostAddress, EnableRemoteException, IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None);
                    break;
                case NetProtocol.Tcp:
                default:
                    TcpClient.SendOut(message, RemoteHostAddress, RemoteHostPort, Timeout, IsAsync, EnableRemoteException);
                    break;
            }
        }

      

        #region Send Stream


        //public T SendDuplexStream<T>(QueueItem message, Action<string> onFault)
        //{
        //    TransStream ts = SendDuplexStream(message);
        //    if (ts == null)
        //        onFault(message.Command + " return null");
        //    return ts.ReadValue<T>(onFault);
        //}
        //public object SendDuplexStreamValue(QueueItem message, Action<string> onFault, int timeout = 0, bool isAsync = false)
        //{
        //    TransStream ts = SendDuplexStream(message, timeout, isAsync);
        //    if (ts == null)
        //        onFault(message.Command + " return null");
        //    return ts.ReadValue(onFault);
        //}
        //public CacheState SendDuplexState(CacheMessage message)
        //{
        //    TransStream ts = SendDuplexStream(message);
        //    if (ts == null)
        //        return CacheState.UnKnown;
        //    return (CacheState)ts.ReadState();
        //}

        public TransStream SendDuplexStream(QueueItem message, int timeout, bool isAsync = false)
        {
            message.TransformType = TransformType.Stream;

            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClient.SendDuplexStream(message, RemoteHostAddress, RemoteHostPort,ChannelSettings.HttpMethod, GetTimeout(timeout, ChannelSettings.HttpTimeout), EnableRemoteException);

                case NetProtocol.Pipe:
                    //ChannelSettings.IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None
                    return PipeClient.SendDuplexStream(message, RemoteHostAddress, EnableRemoteException, isAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClient.SendDuplexStream(message, RemoteHostAddress, RemoteHostPort, timeout, isAsync, EnableRemoteException);
        }
               

        public TransStream SendDuplexStream(QueueRequest message, int timeout, bool isAsync = false)
        {
            message.TransformType = TransformType.Stream;

            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClient.SendDuplexStream(message, RemoteHostAddress, RemoteHostPort, ChannelSettings.HttpMethod, GetTimeout(timeout, ChannelSettings.HttpTimeout), EnableRemoteException);

                case NetProtocol.Pipe:
                    //ChannelSettings.IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None
                    return PipeClient.SendDuplexStream(message, RemoteHostAddress, EnableRemoteException, isAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClient.SendDuplexStream(message, RemoteHostAddress, RemoteHostPort,timeout, isAsync, EnableRemoteException);

        }
       
        #endregion
    }
}
