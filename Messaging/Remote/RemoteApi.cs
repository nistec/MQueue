﻿using System;
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
using Nistec.Threading;

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
        public const int DefaultConnectTimeout = 6000;
        public const int DefaultReadTimeout = 180000;
        public const int DefaultWaitTimeout = 180000;
        public const int DefaultWaitInterval = 100;


        protected NetProtocol Protocol;
        protected string RemoteHostAddress;
        protected int RemoteHostPort;
        protected bool EnableRemoteException;

        bool _IsAsync = false;
        public bool IsAsync { get { return _IsAsync; } set { _IsAsync = value; } }
        //int _MaxRetry = 1;
        //public int MaxRetry { get { return _MaxRetry; } set { _MaxRetry = value <=0 ? 1: value > 5 ? 5: value; } }

        int _WaitInterval = DefaultWaitInterval;
        public int WaitInterval { get { return _WaitInterval; } set { _WaitInterval = value<=10? DefaultWaitInterval: value; } }
        int _ConnectTimeout = DefaultConnectTimeout;
        public int ConnectTimeout { get { return _ConnectTimeout; } set { _ConnectTimeout = (value <=0 ? DefaultConnectTimeout : value); } }
        int _ReadTimeout = DefaultReadTimeout;
        public int ReadTimeout { get { return _ReadTimeout; } set { _ReadTimeout = ((value == 0 || value< -1) ? DefaultReadTimeout : value); } }
        int _WaitTimeout = DefaultWaitTimeout;
        public int WaitTimeout { get { return _WaitTimeout; } set { _WaitTimeout = ((value == 0 || value <=0) ? DefaultWaitTimeout : value); } }

        public int EnsureConnectTimeout(int timeout) {

            if (timeout <= 0)
                return ConnectTimeout;
            return timeout;
        }
        public string EnsureHost(string host)
        {
            return (host == null || host == "") ? QueueName : host;
        }
        public string QueueName
        {
            get { return _QueueName; }
        }

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
        /*
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
        */

        #region on completed

        protected void OnFault(string message)
        {
            Console.WriteLine("QueueApi Fault: " + message);
        }
        //protected void OnCompleted(QueueItem message)
        //{
        //    Console.WriteLine("QueueApi Completed: " + message.Identifier);
        //}

        protected void OnItemCompleted(TransStream ts, IQueueMessage message, Action<IQueueAck> onCompleted) {

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


            IQueueAck ack = OnItemCompleted(ts, message);

            onCompleted(ack);
        }

        protected IQueueAck OnItemCompleted(TransStream ts, IQueueMessage message)
        {

            QueueAck ack = (ts == null || ts.IsEmpty) ? null : ts.ReadValue<QueueAck>(OnFault);

            if (ack == null)
            {
                if (message.IsDuplex)
                    ack = new QueueAck(MessageState.UnExpectedError, "Server was not responsed for this message", message.Identifier, message.Host);
                else
                    ack = new QueueAck(MessageState.Arrived, "Message Arrived on way", message.Identifier, message.Host);

                //ack.HostAddress = message.HostAddress;
            }

            Assists.SetArrived(ack);

            return ack;
        }

        protected bool OnQItemCompleted(TransStream ts, Action<IQueueItem> onCompleted)
        {

            IQueueItem item = OnQItemCompleted(ts);
            if (item != null)
            {
                onCompleted(item);
                return true;
            }
            return false;
        }

        protected IQueueItem OnQItemCompleted(TransStream ts)//, IQueueMessage message)
        {

            QueueItem item = (ts == null || ts.IsEmpty) ? null : ts.ReadValue<QueueItem>(OnFault);

            if (item == null)
            {

                return null;
                //if (message.IsDuplex)
                //    item = new QueueItem() { MessageState = MessageState.UnExpectedError, Label = "Server was not responsed for this message", Identifier = message.Identifier, Host = message.Host };
                //else
                //    item = new QueueItem() { MessageState = MessageState.Arrived, Label = "Message Arrived on way", Identifier = message.Identifier, Host = message.Host };

                ////ack.HostAddress = message.HostAddress;
            }

            Assists.SetArrived(item);

            return item;
        }
        #endregion

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

            /*

        internal QueueAck Enqueue(QueueItem message, Action<string> onFault)
        {
            message.Host = this._QueueName;

            //ChannelSettings.HttpTimeout;
            //ChannelSettings.IsAsync
            message.MessageState = MessageState.Sending;
            TransStream ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
            if (ts == null)
            {
                if (message.IsDuplex)
                    onFault(message.QCommand.ToString() + " return null");
                return null;
            }
            return ts.ReadValue<QueueAck>(onFault);
        }

        internal QueueAck Enqueue(QueueItem message, int timeout, Action<string> onFault)
        {
            message.Host = this._QueueName;

            //ChannelSettings.HttpTimeout;
            //ChannelSettings.IsAsync
            message.MessageState = MessageState.Sending;
            TransStream ts = SendDuplexStream(message, EnsureConnectTimeout(timeout), IsAsync);
            if (ts == null)
            {
                //"Enqueue return null"
                //if (message.IsDuplex)
                //    onFault(message.QCommand.ToString() + " return null");
                return null;
            }
            return ts.ReadValue<QueueAck>(onFault);
        }
        */
        //internal void EnqueueAsync(QueueItem message, int timeout, Action<string> onFault, Action<TransStream> onCompleted)
        //{
        //    message.Host = this._QueueName;

        //    //ChannelSettings.HttpTimeout;
        //    //ChannelSettings.IsAsync
        //    message.MessageState = MessageState.Sending;

        //    PublishAsync(message, timeout, onFault, onCompleted, null);

        //    //if (ts == null)
        //    //{
        //    //    if (message.IsDuplex)
        //    //        onFault(message.QCommand.ToString() + " return null");
        //    //    return null;
        //    //}
        //    //return ts.ReadValue<QueueAck>(onFault);
        //}

        /*
        internal TransStream SendWaitOneDuplex(QueueRequest message)
        {
            message.Host = this._QueueName;
            QueueCmd cmd = message.QCommand;
            //QueueItem qs = new QueueItem(message);
            TransStream ts = null;

            //Task.Factory.StartNew(() => SendItem(q, counter));

            bool ok = false;

            message.QCommand = QueueCmd.QueueHasValue;
            ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
            if (ts != null && ts.ReadValue<int>() > 0)
            {
                message.QCommand = cmd;
                ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                ok = (ts != null && ts.GetLength() > 0);
                Console.WriteLine(ok);
            }



            return ts;//.ReadValue<QueueItem>(onFault);

        }
        */

        #region Publish

        public TransStream PublishItemStream(QueueItem message, int timeout)
        {
            message.Host = EnsureHost(message.Host);
            message.MessageState = MessageState.Sending;
            timeout = EnsureConnectTimeout(timeout);
            EnableRemoteException = true;
            TransStream ts = ExecDuplexStream(message, timeout);
            return ts;
        }

        public IQueueAck PublishItem(QueueItem message)
        {
            message.Host = EnsureHost(message.Host);
            message.MessageState = MessageState.Sending;
            try
            {

                TransStream ts = ExecDuplexStream(message, ConnectTimeout);
                return OnItemCompleted(ts, message);
            }
            catch (Exception ex)
            {
                OnFault("PublishItem error:" + ex.Message);
                return OnItemCompleted(null, message);
            }
        }

        public IQueueAck PublishItem(QueueItem message, int timeout)
        {
            message.Host = EnsureHost(message.Host);
            message.MessageState = MessageState.Sending;
            try
            {

                TransStream ts = ExecDuplexStream(message, EnsureConnectTimeout(timeout));
                return OnItemCompleted(ts, message);
            }
            catch (Exception ex)
            {
                OnFault("PublishItem error:" + ex.Message);
                return OnItemCompleted(null, message);
            }
        }

        public void PublishItemStream(QueueItem message, int timeout, Action<TransStream> onCompleted)
        {
            message.Host = EnsureHost(message.Host);
            message.MessageState = MessageState.Sending;
            timeout = EnsureConnectTimeout(timeout);
            bool isCompleted = false;
            EnableRemoteException = true;
            
            Task task = Task.Factory.StartNew(() =>
            {
                ExecDuplexStreamAsync(message, timeout, (TransStream ts) =>
                {
                    onCompleted(ts);
                    isCompleted = true;
                }, IsAsync);

                //while (!isCompleted)
                //{
                //    Thread.Sleep(100);
                //}

            });

            task.Wait(WaitTimeout);
        }

        public void PublishItem(QueueItem message, int timeout, Action<IQueueAck> onCompleted)
        {
            message.Host = EnsureHost(message.Host);
            message.MessageState = MessageState.Sending;
            timeout = EnsureConnectTimeout(timeout);
            bool isCompleted = false;

            try
            {

                Task task = Task.Factory.StartNew(() =>
                {
                    ExecDuplexStreamAsync(message, timeout, (TransStream ts) =>
                    {
                        OnItemCompleted(ts, message,onCompleted);
                        isCompleted = true;
                    }, IsAsync);

                    //while (!isCompleted)
                    //{
                    //    Thread.Sleep(100);
                    //}

                });

                task.Wait(WaitTimeout);
            }
            catch (Exception ex)
            {
                OnFault("PublishItem error:" + ex.Message);
                OnItemCompleted(null, message, onCompleted);
            }
        }

        public void PublishItemStream(QueueItem message, int timeout, Action<string> onFault, Action<TransStream> onCompleted)
        {
            message.Host = EnsureHost(message.Host);
            message.MessageState = MessageState.Sending;
            timeout = EnsureConnectTimeout(timeout);
            bool isCompleted = false;

            try
            {

                Task task = Task.Factory.StartNew(() =>
                {
                    ExecDuplexStreamAsync(message, timeout, (TransStream ts) =>
                    {
                        onCompleted(ts);
                        isCompleted = true;
                    }, IsAsync);

                    //while (!isCompleted)
                    //{
                    //    Thread.Sleep(100);
                    //}

                });

                task.Wait(WaitTimeout);
            }
            catch (Exception ex)
            {
                onFault("PublishItem error:" + ex.Message);
            }
        }

        public void PublishItemStream(QueueItem message, int timeout, Action<string> onFault, Action<TransStream> onCompleted, CancellationTokenSource cts)
        {
            message.Host = EnsureHost(message.Host);
            message.MessageState = MessageState.Sending;
            timeout = EnsureConnectTimeout(timeout);
            bool isCompleted = false;
            CancellationToken ct = cts.Token;

            try
            {

                Task task = Task.Factory.StartNew(() =>
                {

                    if (ct.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitTimeout)))
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                    ExecDuplexStreamAsync(message, timeout, (TransStream ts) =>
                    {
                        onCompleted(ts);
                        isCompleted = true;
                    }, IsAsync);

                    while (!isCompleted)
                    {
                        // Poll on this property if you have to do
                        // other cleanup before throwing.
                        if (ct.IsCancellationRequested)
                        {
                            // Clean up here, then...
                            ct.ThrowIfCancellationRequested();
                        }
                        Thread.Sleep(WaitInterval);
                    }

                }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                task.Wait(WaitTimeout);
            }
            catch (OperationCanceledException cex)
            {
                //Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {cex.Message}");
                onFault("PublishItem OperationCanceledException:" + cex.Message);
            }
            catch (Exception ex)
            {
                onFault("PublishAsync error:" + ex.Message);
            }
            finally
            {
                cts.Dispose();
            }
        }

        #endregion

        #region Consum

        public IQueueAck ConsumItem(QueueItem message, int timeout)
        {
            message.Host = EnsureHost(message.Host);
            message.MessageState = MessageState.Receiving;
            timeout = EnsureConnectTimeout(timeout);

            try
            {

                TransStream ts = ExecDuplexStream(message, timeout);
                return OnItemCompleted(ts, message);
            }
            catch (Exception ex)
            {
                OnFault("ConsumItem error:" + ex.Message);
                return OnItemCompleted(null, message);
            }
        }


        public IQueueItem ConsumItem(QueueRequest message, int timeout)
        {
            message.Host = EnsureHost(message.Host);
            //message.MessageState = MessageState.Receiving;

            try
            {

                TransStream ts = ExecDuplexStream(message, EnsureConnectTimeout(timeout));
                return OnQItemCompleted(ts);
            }
            catch (Exception ex)
            {
                OnFault("ConsumItem error:" + ex.Message);
                return null;// OnQItemCompleted(null, message);
            }
        }


        public void ConsumItem(QueueRequest message, int timeout, Action<IQueueItem> onCompleted, IDynamicWait dw)//Action<bool> onAck)
        {
            message.Host = EnsureHost(message.Host);
            //message.MessageState = MessageState.Receiving;
            timeout = EnsureConnectTimeout(timeout);
            bool isCompleted = false;
            bool ack = false;
            try
            {

                Task task = Task.Factory.StartNew(() =>
                {
                    ExecDuplexStreamAsync(message, timeout, (TransStream ts) =>
                    {
                        if (TransStream.IsEmptyStream(ts))
                        {
                            ack = false;
                        }
                        else
                        {
                            OnQItemCompleted(ts, onCompleted);
                            ack = true;
                        }
                        //if (onAck != null)
                        //    onAck(ack);
                        if (dw != null)
                            dw.DynamicWaitAck(ack);

                        isCompleted = true;
                    }, IsAsync);

                    //while (!isCompleted)
                    //{
                    //    Thread.Sleep(WaitInterval);
                    //}

                });

                task.Wait(WaitTimeout);
            }
            catch (Exception ex)
            {
                OnFault("ConsumItem error:" + ex.Message);
                //OnQItemCompleted(null, message, onCompleted);
            }
        }

        //public void ConsumItem(QueueRequest message, int timeout, Action<IQueueItem> onCompleted, Action<bool> onAck, AutoResetEvent resetEvent)
        //{
        //    message.Host = EnsureHost(message.Host);
        //    //message.MessageState = MessageState.Receiving;
        //    timeout = EnsureConnectTimeout(timeout);
        //    bool isCompleted = false;
        //    bool ack = false;
        //    try
        //    {

        //        Task task = Task.Factory.StartNew(() =>
        //        {
        //            ExecDuplexStreamAsync(message, timeout, (TransStream ts) =>
        //            {
        //                if (TransStream.IsEmptyStream(ts))
        //                    ack = false;
        //                else
        //                {
        //                    OnQItemCompleted(ts, onCompleted);
        //                    ack = true;
        //                }
        //                onAck(ack);
        //                isCompleted = true;
        //                resetEvent.Set();
        //            }, IsAsync);

        //            //while (!isCompleted)
        //            //{
        //            //    Thread.Sleep(WaitInterval);
        //            //}

        //        });

        //        task.Wait(WaitTimeout);
        //    }
        //    catch (Exception ex)
        //    {
        //        OnFault("ConsumItem error:" + ex.Message);
        //        //OnQItemCompleted(null, onCompleted);
        //    }
        //}


        public void ConsumItem(QueueRequest message, Action<string> onFault, Action<IQueueItem> onCompleted)
        {
            message.Host = EnsureHost(message.Host);
            bool isCompleted = false;

            try
            {

                Task task = Task.Factory.StartNew(() =>
                {
                    ExecDuplexStreamAsync(message, ConnectTimeout, (TransStream ts) =>
                    {
                        OnQItemCompleted(ts, onCompleted);
                        isCompleted = true;
                    }, IsAsync);

                    //while (!isCompleted)
                    //{
                    //    Thread.Sleep(WaitInterval);
                    //}

                });
                task.Wait(WaitTimeout);
            }
            catch (Exception ex)
            {
                onFault("ConsumItem error:" + ex.Message);
                //OnQItemCompleted(null, message, onCompleted);
            }
        }
 
        public void ConsumItemStream(QueueRequest message, int timeout, Action<string> onFault, Action<TransStream> onCompleted)
        {
            message.Host = EnsureHost(message.Host);
            timeout = EnsureConnectTimeout(timeout);
            bool isCompleted = false;

            try
            {

                Task task = Task.Factory.StartNew(() =>
                {
                    ExecDuplexStreamAsync(message, timeout, (TransStream ts) =>
                    {
                        onCompleted(ts);
                        isCompleted = true;
                    }, IsAsync);

                    //while (!isCompleted)
                    //{
                    //    Thread.Sleep(100);
                    //}

                });

                task.Wait(WaitTimeout);
            }
            catch (Exception ex)
            {
                onFault("ConsumItemStream error:" + ex.Message);
            }
        }

        public void ConsumItemStream(QueueRequest message, int timeout, Action<string> onFault, Action<TransStream> onCompleted, CancellationTokenSource cts)
        {
            message.Host = EnsureHost(message.Host);
            timeout = EnsureConnectTimeout(timeout);
            bool isCompleted = false;
            CancellationToken ct = cts.Token;

            try
            {

                Task task = Task.Factory.StartNew(() =>
                {

                    if (ct.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitTimeout)))
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                    ExecDuplexStreamAsync(message, timeout, (TransStream ts) =>
                    {
                        onCompleted(ts);
                        isCompleted = true;
                    }, IsAsync);

                    while (!isCompleted)
                    {
                        // Poll on this property if you have to do
                        // other cleanup before throwing.
                        if (ct.IsCancellationRequested)
                        {
                            // Clean up here, then...
                            ct.ThrowIfCancellationRequested();
                        }
                        Thread.Sleep(WaitInterval);
                    }

                }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                task.Wait(WaitTimeout);
            }
            catch (OperationCanceledException cex)
            {
                //Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {cex.Message}");
                onFault("ConsumItemStream OperationCanceledException:" + cex.Message);
            }
            catch (Exception ex)
            {
                onFault("ConsumItemStream error:" + ex.Message);
            }
            finally
            {
                cts.Dispose();
            }
        }

        /*
        public IQueueItem ConsumWaitOne(QueueRequest message, Action<string> onFault)
        {
            //message.Host = this._QueueName;
            //int timeout = -1;
            //bool isCompleted = false;
            //CancellationToken ct = cts.Token;


            try
            {

                TransStream ts = WaitForStream(message);
                return OnQItemCompleted(ts, message);

            }
            catch (OperationCanceledException cex)
            {
                //Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {cex.Message}");
                onFault("ConsumItemStream OperationCanceledException:" + cex.Message);
                return null;
            }
            catch (Exception ex)
            {
                onFault("ConsumItemStream error:" + ex.Message);
                return null;
            }
            finally
            {
                //cts.Dispose();
                //resetEvent.Set();
            }
        }

        public void ConsumWaitOne(QueueRequest message, Action<string> onFault, Action<IQueueItem> onCompleted, AutoResetEvent resetEvent)
        {
            //message.Host = this._QueueName;
            //int timeout = -1;
            bool isCompleted = false;
            //CancellationToken ct = cts.Token;


            try
            {

                Task task = Task.Factory.StartNew(() =>
                {

                    //if (ct.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitInterval)))
                    //{
                    //    ct.ThrowIfCancellationRequested();
                    //}
                    WaitForStream(message, (TransStream ts) =>
                    {
                        OnQItemCompleted(ts, message, onCompleted);
                        isCompleted = true;
                    }, IsAsync);
                    resetEvent.WaitOne();

                    //while (!isCompleted)
                    //{
                    //    // Poll on this property if you have to do
                    //    // other cleanup before throwing.
                    //    if (ct.IsCancellationRequested)
                    //    {
                    //        // Clean up here, then...
                    //        ct.ThrowIfCancellationRequested();
                    //    }
                    //    Thread.Sleep(100);
                    //}

                }, TaskCreationOptions.LongRunning);//, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                task.Wait(WaitTimeout);
            }
            catch (OperationCanceledException cex)
            {
                //Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {cex.Message}");
                onFault("ConsumItemStream OperationCanceledException:" + cex.Message);
            }
            catch (Exception ex)
            {
                onFault("ConsumItemStream error:" + ex.Message);
            }
            finally
            {
                //cts.Dispose();
                resetEvent.Set();
            }
        }

        */

        #endregion

        /*
        internal void b_SendDuplexStreamAsync(QueueItem message, int timeout, Action<string> onFault, Action<TransStream> onCompleted, AutoResetEvent resetEvent)
        {
            message.Host = this._QueueName;
            QueueCmd cmd = message.QCommand;
            TransStream ts = null;
            timeout = EnsureConnectTimeout(timeout);

            var cancellationTokenSource = new CancellationTokenSource();

            if (message.DuplexType == DuplexTypes.WaitOne)
            {
                Task.Factory.StartNew(
                () =>
                {
                    for (; ; )
                    {
                        if (cancellationTokenSource.Token.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitInterval)))
                        {
                            break;
                        }

                        message.QCommand = QueueCmd.QueueHasValue;
                        ts = SendDuplexStream(message, timeout, IsAsync);
                        if (ts != null && ts.ReadValue<int>() > 0)
                        {
                            message.QCommand = cmd;
                            ts = SendDuplexStream(message, timeout, IsAsync);
                            bool ok = (ts != null && ts.GetLength() > 0);
                            Console.WriteLine(ok);
                            if (ok)
                            {
                                //var res = ts.ReadValue<QueueItem>(onFault);
                                //Assists.SetReceived(res, cmd);
                                //Assists.SetArrived(res);
                                onCompleted(ts);
                                if (resetEvent != null)
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
                   ts = SendDuplexStream(message, timeout, IsAsync);
                   if (ts != null && ts.ReadValue<int>() > 0)
                   {
                       message.QCommand = cmd;
                       ts = SendDuplexStream(message, timeout, IsAsync);
                       bool ok = (ts != null && ts.GetLength() > 0);
                       Console.WriteLine(ok);
                       if (ok)
                       {
                           //var res =ts.ReadValue<QueueItem>(onFault);
                           //Assists.SetReceived(res, cmd);
                           //Assists.SetArrived(res);
                           onCompleted(ts);
                           if (resetEvent != null)
                               resetEvent.Set();
                           return;
                       }
                   }
               }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }
        
        internal void b_SendDuplexStreamAsync(QueueRequest message, Action<string> onFault, Action<TransStream> onCompleted, AutoResetEvent resetEvent)
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
                    for (; ; )
                    {
                        if (cancellationTokenSource.Token.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitInterval)))
                        {
                            break;
                        }

                        message.QCommand = QueueCmd.QueueHasValue;
                        ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                        if (ts != null && ts.ReadValue<int>() > 0)
                        {
                            message.QCommand = cmd;
                            ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                            bool ok = (ts != null && ts.GetLength() > 0);
                            Console.WriteLine(ok);
                            if (ok)
                            {
                                //var res = ts.ReadValue<QueueItem>(onFault);
                                //Assists.SetReceived(res, cmd);
                                //Assists.SetArrived(res);
                                onCompleted(ts);
                                if (resetEvent != null)
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
                   ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                   if (ts != null && ts.ReadValue<int>() > 0)
                   {
                       message.QCommand = cmd;
                       ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                       bool ok = (ts != null && ts.GetLength() > 0);
                       Console.WriteLine(ok);
                       if (ok)
                       {
                           //var res =ts.ReadValue<QueueItem>(onFault);
                           //Assists.SetReceived(res, cmd);
                           //Assists.SetArrived(res);
                           onCompleted(ts);
                           if (resetEvent != null)
                               resetEvent.Set();
                           return;
                       }
                   }
               }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        internal void b_SendDuplexAsync(QueueRequest message, Action<string> onFault, Action<QueueItem> onCompleted, AutoResetEvent resetEvent)
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
                    for (; ; )
                    {
                        if (cancellationTokenSource.Token.WaitCancellationRequested(TimeSpan.FromMilliseconds(WaitInterval)))
                        {
                            break;
                        }

                        message.QCommand = QueueCmd.QueueHasValue;
                        ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                        if (ts != null && ts.ReadValue<int>() > 0)
                        {
                            message.QCommand = cmd;
                            ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                            bool ok = (ts != null && ts.GetLength() > 0);
                            //Console.WriteLine(ok);
                            if (ok)
                            {
                                var res = ts.ReadValue<QueueItem>(onFault);
                                Assists.SetReceived(res, cmd);
                                //Assists.SetArrived(res);
                                onCompleted(res);
                                if (resetEvent != null)
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
                //ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                //if (ts != null && ts.ReadValue<int>() > 0)
                //{
                //    message.QCommand = cmd;
                //    ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
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
                   ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                   if (ts != null && ts.ReadValue<int>() > 0)
                   {
                       message.QCommand = cmd;
                       ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                       bool ok = (ts != null && ts.GetLength() > 0);
                       //Console.WriteLine(ok);
                       if (ok)
                       {
                           var res = ts.ReadValue<QueueItem>(onFault);
                           Assists.SetReceived(res, cmd);
                           //Assists.SetArrived(res);
                           onCompleted(res);
                           if (resetEvent != null)
                               resetEvent.Set();
                           return;
                       }
                   }
               }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                //ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
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


                ts = WaitForStream(message, IsAsync);// SendDuplexStream(message, ConnectTimeout, IsAsync);
                if (ts == null)
                    onFault(message.QCommand.ToString() + " return null");


                //bool ok = false;
                //while (!ok)
                //{
                //    //Task task = Task.Factory.Scheduler.StartNew(() => SendWaitOneDuplex(message));
                //    //{
                //    //    task.Wait(ConnectTimeout);
                //    //    if (task.IsCompleted)
                //    //    {
                //    //        ok = (ts != null && ts.GetLength() > 0);
                //    //        if (ok)
                //    //            return ts.ReadValue<QueueItem>(onFault);
                //    //    }
                //    //}
                //    //task.TryDispose();

                //    message.QCommand = QueueCmd.QueueHasValue;
                //    ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                //    if (ts != null && ts.ReadValue<int>() > 0)
                //    {
                //        message.QCommand = cmd;
                //        ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                //        ok = (ts != null && ts.GetLength() > 0);
                //    }
                //    if (!ok)
                //    {
                //        //Thread.Sleep(WaitInterval);
                //        return null;
                //    }
                //}
                else
                {
                    qitem = ts.ReadValue<QueueItem>(onFault);
                    Assists.SetReceived(qitem, cmd);
                }
                return qitem;
            }
            else
            {

                ts = SendDuplexStream(message, ConnectTimeout, IsAsync);
                if (ts == null)
                    onFault(message.QCommand.ToString() + " return null");
                else
                {
                    qitem = ts.ReadValue<QueueItem>(onFault);
                    Assists.SetReceived(qitem, cmd);
                }
                return qitem;
            }
        }
        */
        internal void SendOut(QueueItem message)
        {
            //message.Host = this._QueueName;
            //QueueItem qs = new QueueItem(message);

            switch (Protocol)
            {
                case NetProtocol.Http:
                    HttpClient.SendOut(message, RemoteHostAddress, RemoteHostPort, ChannelSettings.HttpMethod, ConnectTimeout, EnableRemoteException);
                    break;
                case NetProtocol.Pipe:
                    PipeClient.SendOut(message, RemoteHostAddress, EnableRemoteException, IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None);
                    break;
                case NetProtocol.Tcp:
                default:
                    TcpClient.SendOut(message, RemoteHostAddress, RemoteHostPort, ConnectTimeout, IsAsync, EnableRemoteException);
                    break;
            }
        }



        #region Exec Stream


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

        public void ExecDuplexStreamAsync(QueueItem message, int connectTimeout, Action<TransStream> onCompleted, bool isChannelAsync = false)
        {
            message.TransformType = TransformType.Stream;

            switch (Protocol)
            {
                case NetProtocol.Http:
                    HttpClient.SendDuplexStreamAsync(message, RemoteHostAddress, RemoteHostPort, ChannelSettings.HttpMethod, ConnectTimeout, onCompleted, EnableRemoteException);
                    break;
                case NetProtocol.Pipe:
                    //ChannelSettings.IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None
                    PipeClient.SendDuplexStreamAsync(message, RemoteHostAddress, onCompleted, EnableRemoteException, isChannelAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None);
                    break;
                case NetProtocol.Tcp:
                    TcpClient.SendDuplexStreamAsync(message, RemoteHostAddress, RemoteHostPort, connectTimeout, onCompleted, isChannelAsync, EnableRemoteException);
                    break;
            }
        }

        public TransStream ExecDuplexStream(QueueItem message, int connectTimeout, bool isAsync = false)
        {
            message.TransformType = TransformType.Stream;

            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClient.SendDuplexStream(message, RemoteHostAddress, RemoteHostPort,ChannelSettings.HttpMethod, ConnectTimeout, EnableRemoteException);

                case NetProtocol.Pipe:
                    //ChannelSettings.IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None
                    return PipeClient.SendDuplexStream(message, RemoteHostAddress, EnableRemoteException, isAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClient.SendDuplexStream(message, RemoteHostAddress, RemoteHostPort, connectTimeout, isAsync, EnableRemoteException);
        }

        public void ExecDuplexStreamAsync(QueueRequest message, int connectTimeout, Action<TransStream> onCompleted, bool isChannelAsync = false)
        {
            message.TransformType = TransformType.Stream;

            switch (Protocol)
            {
                case NetProtocol.Http:
                    HttpClient.SendDuplexStreamAsync(message, RemoteHostAddress, RemoteHostPort, ChannelSettings.HttpMethod, ConnectTimeout, onCompleted, EnableRemoteException);
                    break;
                case NetProtocol.Pipe:
                    //ChannelSettings.IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None
                    PipeClient.SendDuplexStreamAsync(message, RemoteHostAddress, onCompleted, EnableRemoteException, isChannelAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None);
                    break;
                case NetProtocol.Tcp:
                    TcpClient.SendDuplexStreamAsync(message, RemoteHostAddress, RemoteHostPort, connectTimeout, onCompleted, isChannelAsync, EnableRemoteException);
                    break;
            }
        }

        public TransStream ExecDuplexStream(QueueRequest message, int connectTimeout, bool isAsync = false)
        {
            message.TransformType = TransformType.Stream;

            switch (Protocol)
            {
                case NetProtocol.Http:
                    return HttpClient.SendDuplexStream(message, RemoteHostAddress, RemoteHostPort, ChannelSettings.HttpMethod, ConnectTimeout, EnableRemoteException);

                case NetProtocol.Pipe:
                    //ChannelSettings.IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None
                    return PipeClient.SendDuplexStream(message, RemoteHostAddress, EnableRemoteException, isAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClient.SendDuplexStream(message, RemoteHostAddress, RemoteHostPort, connectTimeout, isAsync, EnableRemoteException);

        }
        /*
        public TransStream WaitForStream(QueueRequest message, bool isAsync = false)
        {
            message.TransformType = TransformType.Stream;

            switch (Protocol)
            {
                case NetProtocol.Http:
                    //return HttpClient.SendDuplexStream(message, RemoteHostAddress, RemoteHostPort, ChannelSettings.HttpMethod, GetTimeout(connectTimeout, ChannelSettings.HttpTimeout), EnableRemoteException);
                    throw new InvalidOperationException("Http not supported this method");
                case NetProtocol.Pipe:
                    //ChannelSettings.IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None
                    return PipeClient.SendDuplexStream(message, RemoteHostAddress,0, EnableRemoteException, isAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None);

                case NetProtocol.Tcp:
                    break;
            }
            return TcpClient.SendDuplexStream(message, RemoteHostAddress, RemoteHostPort, ConnectTimeout, -1, isAsync, EnableRemoteException);

        }
        public void WaitForStream(QueueRequest message, Action<TransStream> onCompleted, bool isChannelAsync = false)
        {
            message.TransformType = TransformType.Stream;

            switch (Protocol)
            {
                case NetProtocol.Http:
                    //HttpClient.SendDuplexStreamAsync(message, RemoteHostAddress, RemoteHostPort, ChannelSettings.HttpMethod, GetTimeout(connectTimeout, ChannelSettings.HttpTimeout), onCompleted, EnableRemoteException);
                    throw new InvalidOperationException("Http not supported this method");
                case NetProtocol.Pipe:
                    //ChannelSettings.IsAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None
                    PipeClient.SendDuplexStreamAsync(message, RemoteHostAddress, 0, onCompleted, EnableRemoteException, isChannelAsync ? System.IO.Pipes.PipeOptions.Asynchronous : System.IO.Pipes.PipeOptions.None);
                    break;
                case NetProtocol.Tcp:
                    TcpClient.SendDuplexStreamAsync(message, RemoteHostAddress, RemoteHostPort, ConnectTimeout, -1, onCompleted, isChannelAsync, EnableRemoteException);
                    break;
            }
        }
        */
        #endregion
    }
}
