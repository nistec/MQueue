using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Data.Entities;
using Nistec.IO;
using Nistec.Data;


namespace Nistec.Messaging.Db
{

    /// <summary>
    /// Provide an entity db context for MessageEntity entity.
    /// </summary>
    [Entity(ConnectionKey = "nistec_db", EntityKey = new string[] { "UniqueId" }, EntityName = "DbMessage", MappingName = "FifoQueue", Mode = EntityMode.Generic, EntitySourceType = EntitySourceType.Table)]
    public class DbMessageContext : EntityContext<DbMessage>
    {
        public DbMessageContext() : base() { }

        /// <summary>
        /// Get all the services as list of entities <see cref="MessageEntity"/> from database.
        /// </summary>
        /// <returns></returns>
        public static IList<DbMessage> GetItems()
        {
            using (DbMessageContext db = new DbMessageContext())
            {
                return db.EntityList();
            }
        }

        /// <summary>
        /// Get <see cref="MessageEntity"/> from <see cref="GenericEntity"/>.
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public static DbMessage GetItem(GenericEntity rec)
        {
            using (DbMessageContext db = new DbMessageContext())
            {
                db.Set(rec.Record);
                return db.Entity;
            }
        }


        /// <summary>
        /// Get the <see cref="MessageEntity"/> item as stream.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public void WriteStream(DbMessage entity, NetStream stream)
        {
            this.Set(entity);
            this.EntityWrite(stream, null);
        }

        public int Enqueue(DbMessage message)
        {
            this.Set(message);
            //this.SaveChanges(Nistec.Data.UpdateCommandType.Insert);
            return this.ExecuteNonQuery("sp_enqueue_fifo", this.EntityProperties.ToInsertParameters(), CommandType.StoredProcedure);
        }

        public int Enqueue(QueueItem message)
        {
            return this.ExecuteNonQuery("sp_enqueue_fifo", message.MessageDataParameters(), CommandType.StoredProcedure);
        }

        public byte[] PeekStream(string host, string identifier)
        {
            return this.ExecuteCommand<byte[]>("sp_peek_fifo_item", DataParameter.GetSql("Host", host, "Identifier", identifier), CommandType.StoredProcedure);
        }

        public byte[] PeekStream(string host)
        {
            return this.ExecuteCommand<byte[]>("sp_peek_fifo", DataParameter.GetSql("Host", host), CommandType.StoredProcedure);
        }

        public byte[] DequeueStream(string host)
        {
            return this.ExecuteCommand<byte[]>("sp_dequeue_fifo", DataParameter.GetSql("Host", host), CommandType.StoredProcedure);
        }

        public byte[] DequeueStreamTrans(string host, int transTimeout)
        {
            return this.ExecuteCommand<byte[]>("sp_dequeue_fifo_trans", DataParameter.GetSql("Host", host, "Expiration", transTimeout), CommandType.StoredProcedure);
        }

        public byte[] DequeueStream(string host, string identifier)
        {
            return this.ExecuteCommand<byte[]>("sp_dequeue_fifo_item", DataParameter.GetSql("Host", host, "Identifier", identifier), CommandType.StoredProcedure);
        }

        public byte[] DequeueStreamTrans(string host, string identifier, int transTimeout)
        {
            return this.ExecuteCommand<byte[]>("sp_dequeue_fifo_trans_item", DataParameter.GetSql("Host", host, "Identifier", identifier, "Expiration", transTimeout), CommandType.StoredProcedure);
        }

        public QueueItem Peek(string host)
        {
            byte[] bytes = PeekStream(host);
            if (bytes == null)
                return null;
            var msg = new QueueItem(new NetStream(bytes), null, MessageState.Peeked);
            return msg;
        }

        public QueueItem PeekItem(string host, string identifier)
        {
            byte[] bytes = PeekStream(host, identifier);
            if (bytes == null)
                return null;
            var msg = new QueueItem(new NetStream(bytes), null, MessageState.Peeked);
            return msg;
        }

        public QueueItem Dequeue(string host, bool isTrans = false)
        {
            byte[] bytes = null;
            if (isTrans)
                bytes = DequeueStreamTrans(host, QueueDefaults.DefaultExpiration);
            else
                bytes = DequeueStream(host);
            if (bytes == null)
                return null;
            var msg = new QueueItem(new NetStream(bytes), null, MessageState.Received);
            //msg.SetMessageState(MessageState.Received);
            return msg;
        }

        public QueueItem Dequeue(string host, string identifier, bool isTrans = false)
        {
            byte[] bytes = null;
            if (isTrans)
                bytes = DequeueStreamTrans(host,identifier, QueueDefaults.DefaultExpiration);
            else
                bytes = DequeueStream(host,identifier);
            if (bytes == null)
                return null;
            var msg = new QueueItem(new NetStream(bytes), null, MessageState.Received);
            //msg.SetMessageState(MessageState.Received);
            return msg;
        }

        public void TransCommit(string identifier)
        {
            this.ExecuteNonQuery("sp_fifo_commit", DataParameter.GetSql("Identifier", identifier), CommandType.StoredProcedure);
        }

        public void TransAbort(string identifier, int maxRetry)
        {
            this.ExecuteNonQuery("sp_fifo_abort", DataParameter.GetSql("Identifier", identifier, "MaxRetry", maxRetry), CommandType.StoredProcedure);
        }

        public void TransJob()
        {
            this.ExecuteNonQuery("sp_fifo_trans_job", null, CommandType.StoredProcedure);
        }

        public int Count(string host)
        {
            return this.ExecuteCommand<int>("sp_fifo_count", DataParameter.GetSql("Host", host), CommandType.StoredProcedure);
        }

        public int ClearAllItems(string host)
        {
            return this.ExecuteNonQuery("sp_fifo_clear", DataParameter.GetSql("Host", host), CommandType.StoredProcedure);
        }

        //public static byte[] DequeueStream(string host)
        //{
        //    using (MessageContext context = new MessageContext())
        //    {
        //        return context.ExecuteCommand<byte[]>("sp_dequeue_fifo", DataParameter.Get("Host", host), CommandType.StoredProcedure);
        //    }
        //}

        //public static byte[] DequeueStreamTrans(string host, int transTimeout)
        //{
        //    using (MessageContext context = new MessageContext())
        //    {
        //        return context.ExecuteCommand<byte[]>("sp_dequeue_fifo_trans", DataParameter.Get("Host", host, "Expiration", transTimeout), CommandType.StoredProcedure);
        //    }
        //}

        public static QueueItem DequeueItem(string host, bool isTrans = false)
        {
            byte[] bytes = null;
            using (DbMessageContext context = new DbMessageContext())
            {
                if (isTrans)
                    bytes = context.DequeueStreamTrans(host, QueueDefaults.DefaultExpiration);
                else
                    bytes = context.DequeueStream(host);
            }
            if (bytes == null)
                return null;
            var msg = new QueueItem(new NetStream(bytes), null, MessageState.Received);
            //msg.SetMessageState(MessageState.Received);
            return msg;
        }

        public static QueueItem DequeueItem(string host, string identifier, bool isTrans = false)
        {
            byte[] bytes = null;
            using (DbMessageContext context = new DbMessageContext())
            {
                if (isTrans)
                    bytes = context.DequeueStreamTrans(host, identifier, QueueDefaults.DefaultExpiration);
                else
                    bytes = context.DequeueStream(host, identifier);
            }
            if (bytes == null)
                return null;
            var msg = new QueueItem(new NetStream(bytes), null, MessageState.Received);
            //msg.SetMessageState(MessageState.Received);
            return msg;
        }
    }


}
