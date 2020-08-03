using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Generic;
using Nistec.Channels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;


namespace Nistec.Messaging.Server
{
    /// <summary>
    /// Represent <see cref="MQueue"/> as server agent.
    /// </summary>
    [Serializable]
    public class QueueAgent : QueueManager, IQueuePerformance
    {

        #region IQueuePerformance

        QueuePerformanceCounter m_Perform;
        /// <summary>
        /// Get <see cref="QueuePerformanceCounter"/> Performance Counter.
        /// </summary>
        public QueuePerformanceCounter PerformanceCounter
        {
            get { return m_Perform; }
        }

        /// <summary>
        ///  Sets the memory size as an atomic operation.
        /// </summary>
        /// <param name="memorySize"></param>
        void IQueuePerformance.MemorySizeExchange(ref long memorySize)
        {
            this.LogAction(QueueAction.MemorySizeExchange, QueueActionState.None, "Memory Size Exchange:" + QueueName);
            long size = 0;
            ICollection<QueueEntry> items = m_cacheList.Values;
            foreach (var entry in items)
            {
                size += entry.Size;
            }

            Interlocked.Exchange(ref memorySize, size);

        }

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long IQueuePerformance.GetMaxSize()
        {
            return MaxSize;
        }
        bool IQueuePerformance.IsRemote
        {
            get { return base.IsRemote; }
        }
        int IQueuePerformance.IntervalSeconds
        {
            get { return base.IntervalSeconds; }
        }
        bool IQueuePerformance.Initialized
        {
            get { return base.Initialized; }
        }

        #endregion

        #region size exchange

        /// <summary>
        /// Validate if the new size is not exceeds the QueueMaxSize property.
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        internal protected override QueueState SizeValidate(int newSize)
        {
            if (!QueueSettings.EnableSizeHandler)
                return QueueState.Ok;
            return PerformanceCounter.SizeValidate(newSize);
        }

        /// <summary>
        /// Dispatch size exchange when add , change , remove erc.. items in cache, is the size exchange meet the max size then <see cref="QueueException"/> will thrown.
        /// </summary>
        /// <param name="currentSize"></param>
        /// <param name="newSize"></param>
        /// <param name="currentCount"></param>
        /// <param name="newCount"></param>
        /// <param name="exchange"></param>
        /// <returns></returns>
        /// <exception cref="QueueException"></exception>
        internal protected override QueueState SizeExchage(long currentSize, long newSize, int currentCount, int newCount, bool exchange)
        {
            if (!QueueSettings.EnablePerformanceCounter)
                return QueueState.Ok;
            return PerformanceCounter.ExchangeSizeAndCount(currentSize, newSize, currentCount, newCount, exchange, QueueSettings.EnableSizeHandler);
        }

        /// <summary>
        /// Calculate the size of cache.
        /// </summary>
        internal protected override void SizeRefresh()
        {
            if (QueueSettings.EnablePerformanceCounter)
            {
                PerformanceCounter.RefreshSize();

            }

        }

        #endregion

        #region ctor

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="prop"></param>
        public QueueAgent(QueueProperties prop)
            : base(prop)
        {
            m_Perform = new QueuePerformanceCounter(this, QueueAgentType.Queue, this.QueueName);

        }

        /// <summary>
        /// Reply for test.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Reply(string text)
        {
            return text;
        }
        /// <summary>
        /// Reset cache.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            QueueLogger.Logger.Clear();
        }

        #endregion


        /// <summary>
        /// Execute remote command from client to cache using <see cref="QueueMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NetStream ExecRemote(QueueMessage message)
        {
            QueueState state = QueueState.Ok;
            DateTime requestTime = DateTime.Now;
            try
            {
                switch (message.Command)
                {
                    case QueueCmd.Reply:
                        return QueueEntry.GetAckStream(QueueState.Ok,QueueCmd.Reply,message.Key);
                    case QueueCmd.AddItem:
                        return message.AsyncAckTask(() => AddItemWithAck(message), message.Command);
                    case QueueCmd.GetValue:
                        return message.AsyncAckTask(() => GetValueStream(message.Key), message.Command);
                    case QueueCmd.FetchValue:
                        return message.AsyncAckTask(() => FetchValueStream(message.Key), message.Command);
                    case QueueCmd.GetItem:
                        return message.AsyncAckTask(() => GetItemStream(message.Key), message.Command);
                    case QueueCmd.FetchItem:
                        return message.AsyncAckTask(() => FetchItemStream(message.Key), message.Command);
                    case QueueCmd.ViewItem:
                        return message.AsyncAckTask(() => ViewItemStream(message.Key), message.Command);
                    case QueueCmd.RemoveItem:
                        return message.AsyncAckTask(() => RemoveItem(message), message.Command);
                    case QueueCmd.RemoveItemAsync:
                        message.AsyncTask(() => RemoveItemAsync(message.Key));
                        break;
                    case QueueCmd.CopyItem:
                        {
                            var args = message.GetArgs();
                            return message.AsyncAckTask(() => CopyItemInternal(args.Get<string>(KnowsArgs.Source), args.Get<string>(KnowsArgs.Destination), message.Expiration), message.Command);
                        }
                    case QueueCmd.CutItem:
                        {
                            var args = message.GetArgs();
                            return message.AsyncAckTask(() => CutItemInternal(args.Get<string>(KnowsArgs.Source), args.Get<string>(KnowsArgs.Destination), message.Expiration), message.Command);
                        }
                    case QueueCmd.KeepAliveItem:
                        message.AsyncTask(() => KeepAliveItem(message.Key));
                        break;
                    case QueueCmd.RemoveQueueSessionItems:
                        return message.AsyncAckTask(() => RemoveQueueSessionItemsAsync(message), message.Command);
                    case QueueCmd.LoadData:
                        return message.AsyncAckTask(() => LoadData(message), message.Command);

                }

            }
            catch (QueueException ce)
            {
                state = ce.State;
                LogAction(QueueAction.QueueException, QueueActionState.Error, "QueueAgent.ExecRemote QueueException error: " + ce.Message);
            }
            catch (System.Runtime.Serialization.SerializationException se)
            {
                state = QueueState.SerializationError;
                LogAction(QueueAction.QueueException, QueueActionState.Error, "QueueAgent.ExecRemote SerializationException error: " + se.Message);
            }
            catch (ArgumentException aex)
            {
                state = QueueState.ArgumentsError;
                QueueLogger.Logger.LogAction(QueueAction.QueueException, QueueActionState.Error, "QueueAgent.ExecRemote ArgumentException: " + aex.Message);
            }
            catch (Exception ex)
            {
                state = QueueState.UnexpectedError;
                LogAction(QueueAction.QueueException, QueueActionState.Error, "QueueAgent.ExecRemote error: " + ex.Message);
            }
            finally
            {
                if (QueueSettings.EnablePerformanceCounter)
                {
                    if (QueueSettings.EnableAsyncTask)
                        AgentManager.PerformanceTasker.Add(new Nistec.Threading.TaskItem(() => PerformanceCounter.AddResponse(requestTime, state, true), QueueDefaults.DefaultTaskTimeout));
                    else
                        Task.Factory.StartNew(() => PerformanceCounter.AddResponse(requestTime, state, true));
                }

            }

            return QueueEntry.GetAckStream(state, message.Command);
        }

    }
}


