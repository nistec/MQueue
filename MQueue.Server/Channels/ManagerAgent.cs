using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Generic;
using Nistec.Threading;
using Nistec.IO;
using Nistec.Runtime;
using System.Data;


namespace Nistec.Messaging.Server
{
    /// <summary>
    /// Represent Singleton Agent Manager
    /// </summary>
    public class ManagerAgent
    {

        #region Remote queue

        static QueueController _Queue;
        /// <summary>
        /// Get <see cref="QueueAgent"/> as Singleton.
        /// </summary>
        public static QueueController Queue
        {
            get
            {
                if (_Queue == null)
                {
                    _Queue = new QueueController();
                }
                return _Queue;
            }
        }

        

        #endregion

        #region PerformanceReport
 /*       
        /// <summary>
        /// Get Queue prformance report <see cref="QueuePerformanceReport"/>
        /// </summary>
        /// <returns></returns>
        public static QueuePerformanceReport PerformanceReport()
        {
            QueuePerformanceReport report = new QueuePerformanceReport();
            report.InitReport();

            if (_Queue != null)
                report.AddItemReport(Queue.PerformanceCounter);
            
            report.AddTotalReport();

            return report;
        }
        /// <summary>
        /// Get Queue prformance State Counter report.
        /// </summary>
        /// <returns></returns>
        public static DataTable QueueStateCounter()
        {
            QueueStateCounterReport report = new QueueStateCounterReport();
            if (_Queue != null)
                report.AddItemReport(Queue.PerformanceCounter);
            

            return report.StateReport;
        }

        /// <summary>
        /// Get Queue prformance State Counter report.
        /// </summary>
        /// <returns></returns>
        public static DataTable QueueStateCounter(QueueAgentType agentType)
        {
            QueueStateCounterReport report = new QueueStateCounterReport();
            switch (agentType)
            {
                case QueueAgentType.Queue:
                    if (_Queue != null)
                        report.AddItemReport(Queue.PerformanceCounter); break;
                
            }
            return report.StateReport;
        }

        /// <summary>
        /// Reset Queue prformance counter.
        /// </summary>
        /// <returns></returns>
        public static void ResetPerformanceCounter()
        {
            QueuePerformanceReport report = new QueuePerformanceReport();
            if (_Queue != null)
                report.ResetCounter(Queue.PerformanceCounter);
            
            report.InitReport();
        }


        /// <summary>
        /// Get Queue prformance report <see cref="QueuePerformanceReport"/> using <see cref="QueueAgentType"/>.
        /// </summary>
        /// <param name="agentType"></param>
        /// <returns></returns>
        public static QueuePerformanceReport PerformanceReport(QueueAgentType agentType)
        {
            QueuePerformanceReport report = new QueuePerformanceReport(agentType);
            if (_Queue != null)
                report.AddItemReport(Queue.PerformanceCounter);

            return report;
        }
*/
#endregion

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
                    _Tasker = new Threading.AsyncTasker(100, 1000);
                   
                    _Tasker.Start();
                }
                return _Tasker;
            }
        }

        static Nistec.Threading.TaskerQueue _PerformanceTasker;
        /// <summary>
        /// Get <see cref="AsyncTasker"/> as Singleton.
        /// </summary>
        public static TaskerQueue PerformanceTasker
        {
            get
            {
                if (_PerformanceTasker == null)
                {
                    _PerformanceTasker = new Threading.TaskerQueue(100, 100);

                    _PerformanceTasker.Start();
                }
                return _PerformanceTasker;
            }
        }

        internal static void OnTaskCompleted(GenericEventArgs<TaskItem> e)
        {

            string message = string.Format("OnTaskCompleted: {0}, state: {1}", e.Args.Key, e.Args.State.ToString());

            QueueLogger.Logger.LogAction(QueueAction.SyncTime, QueueActionState.None, message);

            Console.WriteLine(message);
        }

        /// <summary>
        /// Execute remote command from client to queue managment using <see cref="QueueMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static NetStream ExecManager(QueueMessage message)
        {
            QueueState state = QueueState.Ok;
            try
            {
                NetStream stream = null;
                switch (message.Command)
                {
                    case QueueManagerCmd.Reply:
                        return QueueEntry.GetAckStream(QueueState.Ok, QueueManagerCmd.Reply, message.Key);
                    case QueueManagerCmd.QueueProperties:
                        if (_Queue == null)
                            return null;
                        return message.AsyncTask(() => Queue.PerformanceCounter.GetPerformanceProperties(), message.Command);


                    case QueueManagerCmd.CloneItems:
                        if (_Queue == null)
                            return null;
                        var args = message.GetArgs();
                        CloneType ct = EnumExtension.Parse<CloneType>(args.Get<string>("value"), CloneType.All);
                        return message.AsyncTask(() => Queue.CloneItems(ct), message.Command);

                    case QueueManagerCmd.GetAllKeys:
                        if (_Queue == null)
                            return null;
                        return message.AsyncTask(() => Queue.GetAllKeys(), message.Command);
                    case QueueManagerCmd.GetAllKeysIcons:
                        if (_Queue == null)
                            return null;
                        return message.AsyncTask(() => Queue.GetAllKeysIcons(), message.Command);
                    case QueueManagerCmd.StateCounterQueue:
                        return message.AsyncTask(() => QueueStateCounter(QueueAgentType.Queue), message.Command);
                    case QueueManagerCmd.StateCounterSync:
                        return message.AsyncTask(() => QueueStateCounter(QueueAgentType.SyncQueue), message.Command);
                    case QueueManagerCmd.StateCounterSession:
                        return message.AsyncTask(() => QueueStateCounter(QueueAgentType.SessionQueue), message.Command);
                    case QueueManagerCmd.StateCounterDataQueue:
                        return message.AsyncTask(() => QueueStateCounter(QueueAgentType.DataQueue), message.Command);
                    case QueueManagerCmd.GetStateCounterReport:
                        return message.AsyncTask(() => QueueStateCounter(), message.Command);
                    case QueueManagerCmd.GetPerformanceReport:
                        return message.AsyncTask(() => PerformanceReport(), message.Command);
                    case QueueManagerCmd.GetAgentPerformanceReport:
                        QueueAgentType agentType = QueuePerformanceCounter.GetAgent(message.Key);
                        return message.AsyncTask(() => PerformanceReport(agentType), message.Command);
                    case QueueManagerCmd.ResetPerformanceCounter:
                        message.AsyncTask(() => ResetPerformanceCounter());
                        return null;
                    case QueueManagerCmd.GetAllDataKeys:
                        if (_DbQueue == null)
                            return null;
                        return message.AsyncTask(() => DbQueue.GetAllDataKeys(), message.Command);
                    case QueueManagerCmd.GetAllSyncQueueKeys:
                        if (_SyncQueue == null)
                            return null;
                        return message.AsyncTask(() => SyncQueue.QueueKeys().ToArray(), message.Command);
                    case QueueManagerCmd.QueueLog:
                        return message.AsyncTask(() => QueueLogger.Logger.QueueLog(), message.Command);
                    case QueueManagerCmd.GetAllSessionsKeys:
                        if (_Session == null)
                            return null;
                        return message.AsyncTask(() => Session.GetAllSessionsKeys(), message.Command);
                    case QueueManagerCmd.GetAllSessionsStateKeys:
                        if (_Session == null)
                            return null;
                        stream = new NetStream();
                        SessionState st = (SessionState)message.GetArgs().Get<int>("state");
                        return message.AsyncTask(() => Session.GetAllSessionsStateKeys(st), message.Command);
                    case QueueManagerCmd.GetSessionItemsKeys:
                        if (_Session == null)
                            return null;
                        return message.AsyncTask(() => Session.GetSessionsItemsKeys(message.Id), message.Command);

                    //=== Queue api===================================================
                    case QueueCmd.ViewItem:
                    case QueueCmd.RemoveItem:
                        return Queue.ExecRemote(message);

                    //=== Data Queue api===================================================
                    case DataQueueCmd.GetItemProperties:
                    case DataQueueCmd.RemoveTable:
                    //case DataQueueCmd.GetDataStatistic:
                    case DataQueueCmd.GetDataTable:

                        return DbQueue.ExecRemote(message);

                    //=== Sync Queue api===================================================

                    case SyncQueueCmd.RemoveSyncItem:
                    case SyncQueueCmd.GetSyncItem:
                    //case SyncQueueCmd.GetSyncStatistic:
                    case SyncQueueCmd.GetItemsReport:

                        return SyncQueue.ExecRemote(message);

                    //=== Session Queue api===================================================

                    case SessionCmd.RemoveSession:
                    case SessionCmd.GetExistingSession:

                        return Session.ExecRemote(message);


                }
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                state = QueueState.SerializationError;
                QueueLogger.Logger.LogAction(QueueAction.QueueException, QueueActionState.Error, "ExecManager error: " + se.Message);
            }
            catch (Exception ex)
            {
                state = QueueState.UnexpectedError;
                QueueLogger.Logger.LogAction(QueueAction.QueueException, QueueActionState.Error, "ExecManager error: " + ex.Message);
            }

            return QueueEntry.GetAckStream(state, message.Command); //null;
        }


        internal static NetStream ExecCommand(QueueItemStream message)
        {

            switch (message.Command)
            {
                case QueueCmd.Reply:
                case QueueCmd.AddItem:
                case QueueCmd.GetValue:
                case QueueCmd.FetchValue:
                case QueueCmd.GetItem:
                case QueueCmd.FetchItem:
                case QueueCmd.ViewItem:
                case QueueCmd.RemoveItem:
                case QueueCmd.RemoveItemAsync:
                case QueueCmd.CopyItem:
                case QueueCmd.CutItem:
                case QueueCmd.KeepAliveItem:
                case QueueCmd.RemoveQueueSessionItems:
                case QueueCmd.LoadData:
                    return AgentManager.Queue.ExecRemote(message);

                default:
                    QueueLogger.Logger.LogAction(QueueAction.QueueException, QueueActionState.Error, "AgentManager.ExecCommand error: Command not supported " + message.Command);
                    return QueueEntry.GetAckStream(QueueState.CommandNotSupported, message.Command);

            }
        }
    }
}
