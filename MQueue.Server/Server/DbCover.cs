using Nistec.Data.Persistance;
using Nistec.Messaging.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nistec.Messaging.Server
{
    public class DbCover
    {
        public const string TableName = "QCover";

        static PersistentSqlCover<QueueMessage> _Instance;
        public static PersistentSqlCover<QueueMessage> Instance
        {

            get {
                if (_Instance == null)
                {
                    _Instance = new PersistentSqlCover<QueueMessage>(Settings.DbCoverConnection, Data.DBProvider.SqlServer, TableName);
                }
                return _Instance;
            }
        }

        static QueueSettings Settings {

            get { return AgentManager.Settings; }
        }

        public static bool Add(QueueMessage item)
        {
            return Instance.TryAdd(item.Identifier, item.Host, item);
        }

        public static void Renqueue(string hostName) {

            IPersistBinaryItem pitem;
            if (Instance.TryFetch(hostName, out pitem))
            {
                QueueMessage item = QueueMessage.Deserialize(pitem.body);
                AgentManager.Queue.ExecSet(item);
            }
        }

        public static void Renqueue(string hostName, int maxCount=1000)
        {

            
            Task<int> t = Task<int>.Factory.StartNew(() =>
            {
                int i = 0;
                bool found = false;
                try
                {
                    do
                    {
                        IPersistBinaryItem pitem;
                        found = (Instance.TryFetch(hostName, out pitem));
                        if (found)
                        {
                            QueueMessage item = QueueMessage.Deserialize(pitem.body);
                            if (item.IsExpired)
                            {
                                Instance.TryAddJournal(item.Host, pitem);
                            }
                            else
                            {
                                AgentManager.Queue.ExecSet(item);
                                i++;
                            }
                        }
                        Thread.Sleep(10);

                    } while ((i < maxCount && maxCount > 0) || found == false);
                }
                catch (Exception ex)
                {
                    QLogger.Exception("DbCover.Renqueue error", ex);
                }

                return i;
            });
        }

        public static void RenqueueAll(string hostName)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                int count = 0;
                try
                {
                    do
                    {
                        count = (Instance.FetchCount(hostName));
                        if (count > 0)
                        {
                            Renqueue(hostName);
                        }
                        Thread.Sleep(100);

                    } while (count > 0);
                }
                catch (Exception ex)
                {
                    QLogger.Exception("DbCover.RenqueueAll error", ex);
                }

            });

        }

        public static void RenqueueAction(string hostName, int maxCount, Action<QueueMessage> action)
        {


            Task<int> t = Task<int>.Factory.StartNew(() =>
            {
                int i = 0;
                bool found = false;
                try
                {
                    do
                    {
                        IPersistBinaryItem pitem;
                        found = (Instance.TryFetch(hostName, out pitem));
                        if (found)
                        {
                            QueueMessage item = QueueMessage.Deserialize(pitem.body);
                            if (item.IsExpired)
                            {
                                Instance.TryAddJournal(item.Host, pitem);
                                action(null);
                            }
                            else
                            {
                                action(item);
                                i++;
                            }
                        }
                        Thread.Sleep(10);

                    } while ((i < maxCount && maxCount > 0) || found == false);
                }
                catch (Exception ex)
                {
                    QLogger.Exception("DbCover.RenqueueAction error", ex);
                }

                return i;
            });
        }

    }
}
