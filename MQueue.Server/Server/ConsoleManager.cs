using Nistec.Channels;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Logging;
using Nistec.Messaging.Config;
using Nistec.Messaging.Topic;
using Nistec.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Nistec.Messaging.Server
{

    public enum QueueCmd
    {
        Reply=0,
        Enqueue = 10,

        //operation
        AddQueue = 30,
        RemoveQueue = 31,
        HoldEnqueue = 32,
        NoneHoldEnqueue = 33,
        HoldDequeue = 34,
        NoneHoldDequeue = 35,
        EnableQueue = 36,
        DisableQueue = 37,
        ClearQueue = 38,
        //publish\subscribe
        TopicAdd = 40,
        TopicRemove = 41,
        TopicPublish = 42,
        TopicSubscribe = 43,
        TopicRemoveItem = 44,
        TopicCommit = 45,
        TopicAbort = 46,
        //reports
        Exists = 50,
        QueueProperty = 51,
        ReportQueueList = 52,
        ReportQueueItems = 53,
        ReportQueueStatistic = 54,
        PerformanceCounter = 55,
        QueueCount = 56,

    }

    /// <summary>
    /// Represent Singleton Cosole Manager
    /// </summary>
    public class ConsoleManager
    {

        static Nistec.Threading.AsyncTasker _Tasker;
        /// <summary>
        /// Get <see cref="AsyncTasker"/> as Singleton.
        /// </summary>
        public static AsyncTasker Tasker
        {
            get
            {
                if (_Tasker == null)
                {
                    _Tasker = new Threading.AsyncTasker(false, true, 300, 3000);

                    _Tasker.Start();
                    //~Console.WriteLine("Debuger-AgentManager.Tasker satart...");
                }
                return _Tasker;
            }
        }

        static AsyncTasker _PerformanceTasker;
        /// <summary>
        /// Get <see cref="AsyncTasker"/> as Singleton.
        /// </summary>
        public static AsyncTasker PerformanceTasker
        {
            get
            {
                if (_PerformanceTasker == null)
                {
                    _PerformanceTasker = new AsyncTasker(false, false, 10, 5000);

                    _PerformanceTasker.Start();
                    //~Console.WriteLine("Debuger-AgentManager.PerformanceTasker satart...");
                }
                return _PerformanceTasker;
            }
        }

        internal static void OnTaskCompleted(GenericEventArgs<TaskItem> e)
        {

            string message = string.Format("OnTaskCompleted: {0}, state: {1}", e.Args.Key, e.Args.State.ToString());

            QLogger.LogAction(CacheAction.SyncTime, CacheActionState.None, message);

            Console.WriteLine(message);
        }

        internal static TransStream ExecCommand(MessageStream message)
        {
           
            QueueItem item = null;

            try
            {
                if (message == null || message.Command == null)
                {
                    QLogger.Error("ConsoleManager.ExecCommand error: Message is null or Command not supported!");
                    return TransStream.Write("Unknown message or command", TransType.Error);
                }
                //string CommandType = message.Command.Substring(0, 5);

                item = (QueueItem)message;
                QLogger.Debug("QueueController ExecRequset : {0}", item.Print());

                return AgentManager.Queue.ExecRequset(item);

            }
            //catch (MessageException mex)
            //{
            //    QLogger.Exception("ExecGet MessageException: ", mex, true);
            //    return MessageAckServer.DoError(mex.MessageState, request, responseAck, mex);
            //}
            //catch (ArgumentException ase)
            //{
            //    QLogger.Exception("ExecGet ArgumentException: ", ase, true, true);
            //    return MessageAckServer.DoError(MessageState.ArgumentsError, request, responseAck, ase);
            //}
            //catch (SerializationException se)
            //{
            //    QLogger.Exception("ExecGet SerializationException: ", se, true);
            //    return MessageAckServer.DoError(MessageState.SerializeError, request, responseAck, se);
            //}
            catch (Exception ex)
            {
                QLogger.Exception("ExecCommand Exception: ", ex, true, true);
                return TransStream.Write("ExecCommand error: " + ex.Message, TransType.Error);
            }
        }

        /// <summary>
        /// Execute remote command from client to cache managment using <see cref="CacheMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static TransStream ExecManager(MessageStream message)
        {
            CacheState state = CacheState.Ok;
            DateTime requestTime = DateTime.Now;
            try
            {
                //NetStream stream = null;

                if (message == null || message.Command == null)
                {
                    throw new ArgumentNullException("ExecManager.message");
                }

                if (!message.Command.StartsWith("mang_"))
                    return ExecCommand(message);

                switch (message.Command.ToLower())
                {
                    case QueueCmd.Reply:
                        return TransStream.Write("Reply: " + message.Id, TransType.Object);
                    case QueueCmd.CacheProperties:
                        if (_Cache == null)
                            return null;
                        return AsyncTransObject(() => Cache.PerformanceCounter.GetPerformanceProperties(), message.Command);

                    case QueueCmd.ReportCacheItems:
                        return AsyncTransObject(() => Cache.GetReport(), message.Command);
                    case QueueCmd.ReportSessionItems:
                        return AsyncTransObject(() => Session.GetReport(), message.Command);
                    case QueueCmd.ReportDataTimer:
                        return AsyncTransObject(() => DbCache.GetTimerReport(), message.Command);

                    case QueueCmd.ReportCacheTimer:
                        return AsyncTransObject(() => Cache.GetTimerReport(), message.Command);
                    case QueueCmd.ReportSessionTimer:
                        return AsyncTransObject(() => Session.GetTimerReport(), message.Command);
                    case QueueCmd.ReportSyncBoxItems:
                        return AsyncTransObject(() => SyncBox.Instance.GetBoxReport(), message.Command);
                    case QueueCmd.ReportSyncBoxQueue:
                        return AsyncTransObject(() => SyncBox.Instance.GetQueueReport(), message.Command);
                    case QueueCmd.ReportTimerSyncDispatcher:
                        return AsyncTransObject(() => TimerSyncDispatcher.Instance.GetReport(), message.Command);


                    case QueueCmd.CloneItems:
                        if (_Cache == null)
                            return null;
                        var args = message.GetArgs();
                        CloneType ct = EnumExtension.Parse<CloneType>(args.Get<string>("value"), CloneType.All);
                        return AsyncTransObject(() => Cache.CloneItems(ct), message.Command);

                    case QueueCmd.GetAllKeys:
                        if (_Cache == null)
                            return null;
                        return AsyncTransObject(() => Cache.GetAllKeys(), message.Command);
                    case QueueCmd.GetAllKeysIcons:
                        if (_Cache == null)
                            return null;
                        return AsyncTransObject(() => Cache.GetAllKeysIcons(), message.Command);
                    case QueueCmd.StateCounterCache:
                        return AsyncTransObject(() => CacheStateCounter(CacheAgentType.Cache), message.Command);
                    case QueueCmd.StateCounterSync:
                        return AsyncTransObject(() => CacheStateCounter(CacheAgentType.SyncCache), message.Command);
                    case QueueCmd.StateCounterSession:
                        return AsyncTransObject(() => CacheStateCounter(CacheAgentType.SessionCache), message.Command);
                    case QueueCmd.StateCounterDataCache:
                        return AsyncTransObject(() => CacheStateCounter(CacheAgentType.DataCache), message.Command);
                    case QueueCmd.GetStateCounterReport:
                        return AsyncTransObject(() => CacheStateCounter(), message.Command);
                    case QueueCmd.GetPerformanceReport:
                        return AsyncTransObject(() => PerformanceReport(), message.Command);
                    case QueueCmd.GetAgentPerformanceReport:
                        CacheAgentType agentType = CachePerformanceCounter.GetAgent(message.Id);
                        return AsyncTransObject(() => PerformanceReport(agentType), message.Command);
                    case QueueCmd.ResetPerformanceCounter:
                        message.AsyncTask(() => ResetPerformanceCounter());
                        return null;
                    case QueueCmd.GetAllDataKeys:
                        if (_DbCache == null)
                            return null;
                        return AsyncTransObject(() => DbCache.GetAllDataKeys(), message.Command);
                    case QueueCmd.GetAllSyncCacheKeys:
                        if (_SyncCache == null)
                            return null;
                        return AsyncTransObject(() => SyncCache.CacheKeys().ToArray(), message.Command);
                    case QueueCmd.CacheLog:
                        return AsyncTransObject(() => CacheLogger.Logger.CacheLog(), message.Command);
                }
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                state = CacheState.SerializationError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "ExecManager error: " + se.Message);
            }
            catch (Exception ex)
            {
                state = CacheState.UnexpectedError;
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "ExecManager error: " + ex.Message);
            }

            return TransStream.Write(message.Command + ", " + state.ToString(), CacheUtil.ToTransType(state));
        }
        //TOD:~
        internal static TransStream ExecCommand(MessageStream message)
        {
            if (message == null || message.Command == null)
            {
                CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "AgentManager.ExecCommand error: Message is null or Command not supported!");
                return TransStream.Write("Unknown message or command", TransType.Error);
            }

            string CommandType = message.Command.Substring(0, 5);

            switch (CommandType)
            {
                case "cach_":
                    return AgentManager.Cache.ExecRemote(message);
                case "sync_":
                    return AgentManager.SyncCache.ExecRemote(message);
                case "sess_":
                    return AgentManager.Session.ExecRemote(message);
                case "data_":
                    return AgentManager.DbCache.ExecRemote(message);
                case "mang_":
                    return ExecManager(message);
                default:
                    CacheLogger.Logger.LogAction(CacheAction.CacheException, CacheActionState.Error, "AgentManager.ExecCommand error: Command not supported " + message.Command);
                    return TransStream.Write("CommandNotSupported", TransType.Error);
            }
        }

        #region Async Task

        internal static TransStream AsyncTransStream(Func<NetStream> action, string command, CacheState successState = CacheState.Ok, CacheState failedState = CacheState.NotFound, TransType transType = TransType.Object)//TransformType transform = TransformType.Message)
        {
            Task<NetStream> task = Task.Factory.StartNew<NetStream>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result != null)
                    {
                        //SendState(requestTime, successState);
                        return TransStream.Write(task.Result, transType);
                    }
                }
            }
            task.TryDispose();
            //SendState(requestTime, failedState);
            return TransStream.Write(command + ": " + failedState.ToString(), TransType.Error);
        }
        internal static TransStream AsyncTransObject(Func<object> action, string command, CacheState successState = CacheState.Ok, CacheState failedState = CacheState.NotFound, TransType transType = TransType.Object)//TransformType transform = TransformType.Message)
        {
            Task<object> task = Task.Factory.StartNew<object>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    if (task.Result != null)
                    {
                        //SendState(requestTime, successState);
                        return TransStream.Write(task.Result, transType);// TransStream.ToTransType(transform));
                    }
                }
            }
            task.TryDispose();
            //SendState(requestTime, failedState);
            return TransStream.Write(command + ": " + failedState.ToString(), TransType.Error);
        }

        internal static TransStream AsyncTransState(Func<CacheState> action, CacheState failedState = CacheState.NotFound)
        {
            Task<CacheState> task = Task.Factory.StartNew<CacheState>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    //SendState(requestTime, task.Result);
                    return TransStream.Write((int)task.Result, TransType.State);
                }
            }
            task.TryDispose();
            //SendState(requestTime, failedState);
            return TransStream.Write((int)failedState, TransType.State);
        }


        internal static TransStream AsyncTransState(Func<bool> action, CacheState successState, CacheState failedState)
        {
            Task<bool> task = Task.Factory.StartNew<bool>(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    CacheState state = task.Result ? successState : failedState;
                    //SendState(requestTime, state);
                    return TransStream.Write((int)state, TransType.State);
                }
            }
            task.TryDispose();
            //SendState(requestTime, failedState);
            return TransStream.Write((int)failedState, TransType.State);
        }

        internal static TransStream AsyncTransState(Action action, CacheState successState = CacheState.Ok, CacheState failedState = CacheState.UnKnown)
        {
            Task task = Task.Factory.StartNew(action);
            {
                task.Wait();
                if (task.IsCompleted)
                {
                    //SendState(requestTime, successState);
                    return TransStream.Write((int)successState, TransType.State);
                }
            }
            task.TryDispose();
            //SendState(requestTime, failedState);
            return TransStream.Write((int)failedState, TransType.State);
        }

        #endregion


    }

}

