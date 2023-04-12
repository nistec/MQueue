using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Nistec.Legacy
{
    internal class SQLCMD
    {

        static DataTable queueItemTableSchema;

        public static DataTable QueueItemTableSchema
        {
            get
            {
                if (queueItemTableSchema == null)
                {
                    DataTable dt = new DataTable("QueueItem");
                    DataColumn colItemId = new DataColumn("ItemId", typeof(Guid));
                    DataColumn colStatus = new DataColumn("Status", typeof(int));

                    dt.Columns.Add(colItemId);
                    dt.Columns.Add(colStatus);
                    dt.Columns.Add(new DataColumn("MessageId", typeof(int)));
                    dt.Columns.Add(new DataColumn("Priority", typeof(int)));
                    dt.Columns.Add(new DataColumn("Retry", typeof(int)));
                    dt.Columns.Add(new DataColumn("ArrivedTime", typeof(DateTime)));
                    dt.Columns.Add(new DataColumn("SentTime", typeof(DateTime)));
                    dt.Columns.Add(new DataColumn("Body", typeof(object)));
                    dt.Columns.Add(new DataColumn("Subject", typeof(string)));
                    dt.Columns.Add(new DataColumn("Sender", typeof(string)));
                    dt.Columns.Add(new DataColumn("Destination", typeof(string)));
                    dt.Columns.Add(new DataColumn("SenderId", typeof(int)));
                    dt.Columns.Add(new DataColumn("OperationId", typeof(int)));
                    dt.Columns.Add(new DataColumn("HasAttach", typeof(bool)));
                    dt.Columns.Add(new DataColumn("Notify", typeof(string)));
                    dt.Columns.Add(new DataColumn("Price", typeof(decimal)));
                    dt.Columns.Add(new DataColumn("Identifer", typeof(int)));
                    dt.Columns.Add(new DataColumn("Label", typeof(string)));
                    dt.Columns.Add(new DataColumn("TransactionId", typeof(string)));
                    dt.Columns.Add(new DataColumn("AppSpecific", typeof(int)));
                    dt.Columns.Add(new DataColumn("Segments", typeof(int)));
                    dt.Columns.Add(new DataColumn("ClientContext", typeof(string)));
                    dt.Columns.Add(new DataColumn("Server", typeof(int)));
                    dt.Columns.Add(new DataColumn("TimeOut", typeof(int)));
                    dt.PrimaryKey = new DataColumn[] { colItemId, colStatus };
                    queueItemTableSchema = dt;
                }
                return queueItemTableSchema.Clone();
            }
        }
        internal static DataRow GetQueueItemRowSchema()
        {
            return QueueItemTableSchema.NewRow();
        }

        internal static DataTable GetQueueItemsTable(IQueueItem[] items)
        {
            DataTable dt = QueueItemTableSchema;

            foreach (QueueItem item in items)
            {
                DataRow dr = dt.NewRow();
                item.FillDataRow(dr);
                dt.Rows.Add(dr);
            }
            return dt;
        }

        static DataTable queueAttachItemTableSchema;

        public static DataTable QueueAttachItemTableSchema
        {
            get
            {
                if (queueAttachItemTableSchema == null)
                {
                    DataTable dt = new DataTable("QueueAttachItem");

                    DataColumn colItemId = new DataColumn("AttachId", typeof(Guid));
                    DataColumn colMessageId = new DataColumn("MsgId", typeof(int));

                    dt.Columns.Add(colItemId);
                    dt.Columns.Add(colMessageId);
                    dt.Columns.Add(new DataColumn("AttachStream", typeof(string)));
                    dt.Columns.Add(new DataColumn("AttachPath", typeof(string)));
                    dt.PrimaryKey = new DataColumn[] { colItemId, colMessageId };
                    queueAttachItemTableSchema = dt;
                }
                return queueAttachItemTableSchema.Clone();
            }
        }

        internal static DataTable GetQueueAttachItemsTable(IQueueItem items)
        {
            DataTable dt = QueueAttachItemTableSchema;

            foreach (QueueAttachItem item in items.AttachItems)
            {
                DataRow dr = dt.NewRow();
                item.FillDataRow(dr);
                dt.Rows.Add(dr);
            }
            return dt;
        }

        internal static DataRow GetQueueAttachItemRowSchema()
        {
            return QueueAttachItemTableSchema.NewRow();
        }
    }
}
