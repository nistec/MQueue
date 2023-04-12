using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Data.SqlClient;
using Nistec.Messaging.Remote;
using Nistec.Data.Factory;

namespace Nistec.Messaging.Db
{

    public class DbQueue
    {

        public const string DbQueueTableName = "FifoQueue";
        public const string DbCoverTableName = "QCover";

        #region schema


        public static DataTable QueueSchema()
        {

            DataTable dt = new DataTable(DbQueueTableName);
            dt.Columns.Add("Host", typeof(string));
            dt.Columns.Add("MessageState", typeof(Int16));
            dt.Columns.Add("Command", typeof(Int16));
            dt.Columns.Add("Priority", typeof(Int16));
            dt.Columns.Add("UniqueId", typeof(long));
            dt.Columns.Add("Retry", typeof(Int16));
            dt.Columns.Add("ArrivedTime", typeof(DateTime));
            dt.Columns.Add("SentTime", typeof(DateTime));
            dt.Columns.Add("Modified", typeof(DateTime));
            dt.Columns.Add("Expiration", typeof(Int32));
            dt.Columns.Add("MessageId", typeof(Int16));
            dt.Columns.Add("BodyStream", typeof(byte[]));
            return dt.Copy();
        }

        
        #endregion

        #region properties

        int _ConnectTimeout =0;//TODO QueueClient.InfiniteTimeout;

        public int ConnectTimeout
        {
            get { return _ConnectTimeout; }
            set { if (value >= 0) _ConnectTimeout = value; }
        }

        public string ConnectionString { get; set; }

        int _MaxItemsPerSession = 1;
        public int MaxItemsPerSession 
        {
            get { return _MaxItemsPerSession; }
            set { if (value > 0)_MaxItemsPerSession = value; } 
        }

        #endregion

        #region db operation


        public static DataTable MessageToDataTable(QueueMessage message)
        {
            DataTable dt = QueueSchema();
            dt.Rows.Add(message.ItemArray());
            return dt;
        }

        public static DataTable MessageToDataTable(QueueMessage[] messages)
        {
            DataTable dt = QueueSchema();
            foreach (var m in messages)
            {
                dt.Rows.Add(m.ItemArray());
            }
            return dt;
        }

        //public static DataTable MessageToDataTable(QueueMessage message)
        //{
        //    DataTable dt = QueueSchema();
        //    dt.Rows.Add(message.MessageItemArray());
        //    return dt;
        //}

        //public static DataTable MessageToDataTable(QueueMessage[] messages)
        //{
        //    DataTable dt = QueueSchema();
        //    foreach (var m in messages)
        //    {
        //        dt.Rows.Add(m.MessageItemArray());
        //    }
        //    return dt;
        //}

        DataTable ExecDequeue(string host)
        {
            using (var cmd = DbFactory.Create(ConnectionString, Data.DBProvider.SqlServer))
            {
                return cmd.ExecuteDataTable(DbQueueTableName, string.Format("select top {0} * from {1} where Host='{2}';", MaxItemsPerSession, DbQueueTableName, host), false);
            }
        }

        public void ExecEnqueue(QueueMessage message)
        {
            var dt = MessageToDataTable(message);

            using (DbBulkCopy bulk = new DbBulkCopy(ConnectionString))
            {
                bulk.BulkInsert(dt, DbQueueTableName, ConnectTimeout, null);
            }
        }

        public void ExecEnqueue(QueueMessage[] messages)
        {
            var dt = MessageToDataTable(messages);

            using (DbBulkCopy bulk = new DbBulkCopy(ConnectionString))
            {
                bulk.BulkInsert(dt, DbQueueTableName, ConnectTimeout, null);
            }
        }
        
        public void ExecDelete(long id)
        {
            using (var cmd = DbFactory.Create(ConnectionString, Data.DBProvider.SqlServer))
            {
                cmd.ExecuteNonQuery(string.Format("delete from {0} where UniqueId={1};", DbQueueTableName, id));
            }
        }

        void ExecSuspend(DataRow dr)
        {

        }

        void ExecTransBegin(DataRow dr)
        {

        }

        string CommandDelete(Ptr ptr)
        {
            return string.Format("delete from NistecQueue where UniqueId=@UniqueId");
        }

        #endregion

        public QueueMessage Dequeue(string host, string identifier, bool isTrans)
        {
            using (DbMessageContext context = new DbMessageContext())
            {
                return context.Dequeue(host, identifier, isTrans);
            }
        }

        public QueueMessage Dequeue(string host, bool isTrans)
        {
            using (DbMessageContext context = new DbMessageContext())
            {
                return context.Dequeue(host, isTrans);
            }
        }

        public int Enqueue(QueueMessage message)
        {
            using (DbMessageContext context = new DbMessageContext())
            {
                return context.Enqueue(message);
            }
        }

        public void TransCommit(string identifier)
        {
            using (DbMessageContext context = new DbMessageContext())
            {
                context.TransCommit(identifier);
            }
        }

        public void TransAbort(string identifier, int maxRetry)
        {
            using (DbMessageContext context = new DbMessageContext())
            {
                context.TransAbort(identifier, maxRetry);
            }
        }

        public void TransJob()
        {
            using (DbMessageContext context = new DbMessageContext())
            {
                context.TransJob();
            }
        }
    }
}
