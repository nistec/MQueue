using Nistec.Data;
using Nistec.Data.Persistance;
using Nistec.Data.Sqlite;
using Nistec.Messaging.Server;
using Nistec.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Nistec.Messaging
{
    public static class QServerExtension
    {
        public static bool TryFetch(this ConcurrentDictionary<Ptr, IQueueMessage> conDictionary, Ptr key, out IQueueMessage message)
        {
            IQueueMessage item;
            if (conDictionary.TryRemove(key, out item))
            {
                message = item.Copy();
                item.Dispose();
                item = null;
                return true;
            }
            message = null;
            return false;
        }

        public static DbLiteSettings GetDbSettings(string QueueName)
        {

            return new DbLiteSettings()
            {
                Name = QueueName,
                //CommitMode = (CommitMode)(int)qp.CommitMode,
                DbPath = AgentManager.Settings.QueuesPath
            };
        }

        public static IList<PersistItem> QueryItems(string QueueName, string select, string where, params object[] keyValueParameters)
        {
            var settings = GetDbSettings(QueueName);
            using (var db = new DbLite(settings.GetConnectionString(), DBProvider.SQLite))
            {
                var sql = string.Format("select {1} from {0}", QueueName, select); 
                var list = db.Query<PersistItem>(sql, keyValueParameters);
                return list;
            }
        }

        public static string DbQueueReport(string QueueName, string select)
        {
            StringBuilder sb = new StringBuilder();
            var settings = GetDbSettings(QueueName);
            IList<PersistItem> items;
            using (var db = new DbLite(settings.GetConnectionString(), DBProvider.SQLite))
            {
                var sql = string.Format("select {1} from {0}", QueueName, select);
                items = db.Query<PersistItem>(sql);
                //return list;
            }
            foreach (var item in items)
            {
                sb.AppendLine(string.Format("name:{0}, key:{1}, timestamp:{2}, body:{3}", item.name, item.key, item.timestamp.ToString("s"), item.body));
            }
            return sb.ToString();
        }

        public static string DbQuery(string QueueName, string select, string where, params object[] keyValueParameters)
        {
            StringBuilder sb = new StringBuilder();
            var settings = GetDbSettings(QueueName);
            IList<PersistItem> items;
            using (var db = new DbLite(settings.GetConnectionString(), DBProvider.SQLite))
            {
                var sql = string.Format("select {1} from {0}", QueueName, select);
                items = db.Query<PersistItem>(sql, keyValueParameters);
                //return list;
            }
            foreach(var item in items)
            {
                sb.AppendLine(string .Format("name:{0}, key:{1}, timestamp:{2}, body:{3}", item.name, item.key,item.timestamp.ToString("s"),item.body));
            }
            return sb.ToString();
        }

        public static int DbQueueClear(string QueueName)
        {
            var settings = GetDbSettings(QueueName);
            using (var db = new DbLite(settings.GetConnectionString(), DBProvider.SQLite))
            {
                return db.ExecuteCommandNonQuery(string.Format("delete from {0}", QueueName));
            }
        }

        public static int DbQueueClearItem(string QueueName, string key)
        {
            var settings = GetDbSettings(QueueName);
            using (var db = new DbLite(settings.GetConnectionString(), DBProvider.SQLite))
            {
                return db.ExecuteCommandNonQuery("delete from {0} where key=@key", DataParameter.Get<SQLiteParameter>("key", key));
            }
        }

        //internal static PriorityQueue Factory(this IQProperties prop)
        //{
        //    switch (prop.Mode)
        //    {
        //        //case QueueMode.Transactional:
        //        //    return new PriorityTransQueue(prop.QueueName);
        //        //case CoverMode.Db:
        //        //    Assists.Exception_QueueDbNotSupported();
        //        //    return null;
        //        //case CoverMode.File:
        //        //    return new PriorityFileQueue(prop.QueueName);
        //        case CoverMode.Persistent:
        //            return new PriorityPersistQueue(prop);
        //        case CoverMode.Memory:
        //        default:
        //            return new PriorityMemQueue(prop.QueueName);
        //    }
        //}


        //public static void DoRetry(this QueueMessage item)
        //{
        //    item.Retry++;
        //    item.Modified = DateTime.Now;
        //    //item.Header = null;
        //    //m_stream.Replace(Retry, offset + 24);
        //    //m_stream.Replace(Modified.Ticks, offset + 44);
        //}
    }
}
