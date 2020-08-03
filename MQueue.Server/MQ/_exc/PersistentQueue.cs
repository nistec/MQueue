using Nistec.Data;
using Nistec.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Nistec.Messaging
{
    public class PersistentQueue : PersistentDictionary<IPersistItem>
    {

        #region ctor

        public PersistentQueue(DbLiteSettings settings)
            : base(settings)
        {

        }

        public PersistentQueue(string name)
            : base(new DbLiteSettings() { Name = name })
        {

        }

        #endregion

        #region override

        #region property

        public int Version { get { return 4022; } }

        /// <summary>
        /// Get Identifier
        /// </summary>
        public string Identifier { get; set; }


        /// <summary>
        /// Get MessageState
        /// </summary>
        public MessageState MessageState { get; set; }

        /// <summary>
        /// Get Command
        /// </summary>
        public QueueCmd Command { get; set; }

        /// <summary>
        /// Get or Set transformation type.
        /// </summary>
        public TransformTypes TransformType { get; set; }

        /// <summary>
        /// Get or Set message type.
        /// </summary>
        public MQTypes MQType { get; set; }

        /// <summary>
        /// Get Priority
        /// </summary>
        public Priority Priority { get; set; }

        /// <summary>
        /// Get The message Destination\Queue name.
        /// </summary>
        public string Destination { get; set; }

        //NetStream m_BodyStream;
        ///// <summary>
        ///// Get or Set The message body stream.
        ///// </summary>
        //public NetStream BodyStream { get { return m_BodyStream; } }

        public byte[] Body { get; set; }


        /// <summary>
        /// Get Retry
        /// </summary>
        public byte Retry { get; set; }

        /// <summary>
        /// Get ArrivedTime
        /// </summary>
        public DateTime ArrivedTime { get; set; }

        /// <summary>
        /// Get the last modified time.
        /// </summary>
        public DateTime Modified { get; set; }
        #endregion

        //SQLiteParameter>("Identifier", "Body", "Header", "Retry", "ArrivedTime", "Expiration", "MessageState"
 
                    const string sqlcreate = @"CREATE TABLE IF NOT EXISTS {0} (
                          Identifier TEXT PRIMARY KEY,
                          Body BLOB,
                          Header BLOB,
                          Retry INTEGER DEFAULT 0,
                          ArrivedTime DATETIME,
                          Expiration INTEGER,
                          MessageState INTEGER DEFAULT 0,
                          Modified DATETIME DEFAULT CURRENT_TIMESTAMP     
                        ) WITHOUT ROWID;";
        const string sqlinsert = "insert into {0} (Identifier, Body, Header, Retry, ArrivedTime, Expiration,MessageState) values (@Identifier, @Body, @Header, @Retry, @ArrivedTime, @Expiration, @MessageState)";
        const string sqldelete = "delete from {0} where Identifier=@Identifier";
        const string sqlupdate = "update {0} set body=@body, timestamp=CURRENT_TIMESTAMP where Identifier=@Identifier";
        const string sqlinsertOrIgnore = "insert or ignore into {0}(Identifier, Body, Header, Retry, ArrivedTime, Expiration,MessageState) values(@Identifier, @Body, @Header, @Retry, @ArrivedTime, @Expiration, @MessageState)";
        const string sqlinsertOrReplace = "insert or replace into {0}(Identifier, Body, Header, Retry, ArrivedTime, Expiration,MessageState) values(@Identifier, @Body, @Header, @Retry, @ArrivedTime, @Expiration, @MessageState)";
        const string sqlselect = "select {1} from {0} where Identifier=@Identifier";

        const string sqlupdatestate = "update {0} set MessageState=@MessageState,Modified=CURRENT_TIMESTAMP where Identifier=@Identifier";


        /*
        const string sqlcreate = @"CREATE TABLE IF NOT EXISTS {0} (
                          Identifier TEXT PRIMARY KEY,
                          Body BLOB,
                          MQType INTEGER,
                          Priority INTEGER,
                          TransformType INTEGER,
                          MessageState INTEGER DEFAULT 0,
                          Retry INTEGER DEFAULT 0,
                          ArrivedTime DATETIME,
                          Modified DATETIME DEFAULT CURRENT_TIMESTAMP     
                        ) WITHOUT ROWID;";
        const string sqlinsert = "insert into {0} (Identifier, Body, MQType, Priority, TransformType, MessageState, Retry, ArrivedTime) values (@Identifier, @Body, @MQType, @Priority, @TransformType, @MessageState, @Retry, @ArrivedTime)";
        const string sqldelete = "delete from {0} where Identifier=@Identifier";
        const string sqlupdate = "update {0} set body=@body, timestamp=CURRENT_TIMESTAMP where Identifier=@Identifier";
        const string sqlinsertOrIgnore = "insert or ignore into {0}(Identifier, Body, MQType, Priority, TransformType, MessageState, Retry, ArrivedTime) values(@Identifier, @Body, @MQType, @Priority, @TransformType, @MessageState, @Retry, @ArrivedTime)";
        const string sqlinsertOrReplace = "insert or replace into {0}(Identifier, Body, MQType, Priority, TransformType, MessageState, Retry, ArrivedTime) values(@Identifier, @Body, @MQType, @Priority, @TransformType, @MessageState, @Retry, @ArrivedTime)";
        const string sqlselect = "select {1} from {0} where Identifier=@Identifier";

        const string sqlupdatestate = "update {0} set MessageState=@MessageState,Modified=CURRENT_TIMESTAMP where Identifier=@Identifier";
        */
        protected override string DbCreateCommand()
        {
            return string.Format(sqlcreate, Name);
        }
        protected override string DbAddCommand()
        {
            return string.Format(sqlinsert, Name);
        }

        protected override string DbDeleteCommand()
        {
            return string.Format(sqldelete, Name);
        }

        protected override string DbUpdateCommand()
        {
            return string.Format(sqlupdate, Name);
        }

        protected override string DbUpsertCommand()
        {
            return string.Format(sqlinsertOrReplace, Name);
        }

        protected override string DbSelectCommand(string select, string where)
        {
            return string.Format(sqlselect, Name, select, where);
        }

        protected override string DbLookupCommand()
        {
            return string.Format(sqlselect, Name, "Body");
        }

        protected override string DbUpdateStateCommand()
        {
            return string.Format(sqlupdatestate, Name);
        }

        protected override object GetDataValue(IPersistItem value)
        {
            return value;
        }

        protected override IPersistItem DecompressValue(IPersistItem value)
        {
            return value;
        }

        #endregion

        #region Commands UpdateState

        /// <summary>
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public int UpdateState(string key, int state)
        {
            
            bool iscommited = false;
            int res = 0;
            IPersistItem val = null;
            try
            {
                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpdateStateCommand();
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", key, "MessageState", state), (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryGetValue(key, out val))
                                    {
                                        val.MessageState =(MessageState) state;
                                        dictionary[key] = val;
                                    }
                                    //if (dictionary.ContainsKey(key))
                                    //    dictionary[key].State = state;
                                    trans.Commit();
                                    iscommited = true;
                                }
                            });
                        }

                        break;
                    default:
                        //if (dictionary.ContainsKey(key))
                        //    dictionary[key].State = state;
                        if (dictionary.TryGetValue(key, out val))
                        {
                            val.MessageState = (MessageState)state;
                            dictionary[key] = val;
                        }
                        if (_CommitMode == CommitMode.OnMemory)
                        {
                            var task = new PersistanceTask()
                            {
                                CommandText = DbUpdateStateCommand(),
                                CommandType = "DbUpdateState",
                                ConnectionString = ConnectionString,
                                Parameters = DataParameter.Get<SQLiteParameter>("Identifier", key, "MessageState", state)
                            };
                            task.ExecuteTask(_EnableTasker);
                        }
                        res = 1;
                        iscommited = true;
                        break;
                }
                if (iscommited)
                {
                    OnItemChanged("Update", null, val);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("Update", ex.Message);
            }

            return res;
        }

     
        #endregion

        #region Commands


        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int AddOrUpdate(string key, IPersistItem value)
        {
            int res = 0;

            //dictionary.AddOrUpdate(key, value, (oldkey, oldValue) =>
            //{
            //    isExists = true;
            //    return value;
            //});

            bool iscommited = false;

            try
            {

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpsertCommand();
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", value.Identifier, "Body", value.ItemBinary, "Header", value.Header, "Retry", value.Retry, "ArrivedTime", value.ArrivedTime, "Expiration", value.Duration, "MessageState", (byte)value.MessageState), (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = value;
                                    trans.Commit();
                                    iscommited = true;
                                }
                            });
                        }
                        break;
                    default:
                        dictionary.AddOrUpdate(key, value, (oldkey, oldValue) =>
                        {
                            return value;
                        });
                        if (_CommitMode == CommitMode.OnMemory)
                        {
                            var cmdText = DbUpsertCommand();
                            res = ExecuteAsync(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", value.Identifier, "Body", value.ItemBinary, "Header", value.Header, "Retry", value.Retry, "ArrivedTime", value.ArrivedTime, "Expiration", value.Duration, "MessageState", (byte)value.MessageState));
                        }
                        res = 1;
                        iscommited = true;
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("AddOrUpdate", null, null);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("AddOrUpdate", ex.Message);
            }

            return res;
        }

        /// <summary>
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int Update(string key, IPersistItem value)
        {
            bool iscommited = false;
            int res = 0;

            try
            {
                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpdateCommand();
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", value.Identifier, "Body", value.ItemBinary, "Header", value.Header, "Retry", value.Retry, "ArrivedTime", value.ArrivedTime, "Expiration", value.Duration, "MessageState", (byte)value.MessageState), (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = value;
                                    trans.Commit();
                                    iscommited = true;
                                }
                            });
                        }

                        break;
                    default:
                        dictionary[key] = value;
                        if (_CommitMode == CommitMode.OnMemory)
                        {
                            var cmdText = DbUpdateCommand();
                            res = ExecuteAsync(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", value.Identifier, "Body", value.ItemBinary, "Header", value.Header, "Retry", value.Retry, "ArrivedTime", value.ArrivedTime, "Expiration", value.Duration, "MessageState", (byte)value.MessageState));
                        }
                        res = 1;
                        iscommited = true;
                        break;
                }
                if (iscommited)
                {
                    OnItemChanged("Update", null, null);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("Update", ex.Message);
            }

            return res;
        }

        /// <summary>
        /// Attempts to add the specified key and value to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
        ///     in Visual Basic) for reference types.</param>
        /// <returns>
        /// true if the key/value pair was added to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     successfully. If the key already exists, this method returns false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.OverflowException">The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public override bool TryAdd(string key, IPersistItem value)
        {

            bool iscommited = false;

            try
            {
                //var copy=value.Copy();
                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbAddCommand();
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", value.Identifier, "Body", value.ItemBinary, "Header", value.Header, "Retry", value.Retry, "ArrivedTime", value.ArrivedTime, "Expiration", value.Duration, "MessageState", (byte)value.MessageState), (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryAdd(key, value))
                                    {
                                        trans.Commit();
                                        iscommited = true;
                                    }
                                }
                            });
                        }
                        break;
                    default:
                        if (dictionary.TryAdd(key, value))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var cmdText = DbAddCommand();
                                var res = ExecuteAsync(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", value.Identifier, "Body", value.ItemBinary, "Header", value.Header, "Retry", value.Retry, "ArrivedTime", value.ArrivedTime, "Expiration", value.Duration, "MessageState", (byte)value.MessageState));
                                iscommited = true;
                            }
                        }
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryAdd", key, value);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("TryAdd", ex.Message);
            }
            return iscommited;

        }

        /// <summary>
        /// Summary:
        ///     Attempts to remove and return the value with the specified key from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the element to remove and return.
        ///
        ///   value:
        ///     When this method returns, value contains the object removed from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     or the default value of if the operation failed.
        ///
        /// Returns:
        ///     true if an object was removed successfully; otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is a null reference (Nothing in Visual Basic).
        /// </summary>
        public override bool TryRemove(string key, out IPersistItem value)
        {

            bool iscommited = false;
            IPersistItem outval = value = null;
            try
            {
                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbDeleteCommand();
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", value.Identifier), (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryRemove(key, out outval))
                                    {
                                        trans.Commit();
                                        iscommited = true;
                                    }
                                }
                            });
                        }
                        break;
                    default:
                        if (dictionary.TryRemove(key, out outval))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var cmdText = DbDeleteCommand();
                                var res = ExecuteAsync(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", value.Identifier));
                            }
                            iscommited = true;
                        }
                        break;
                }

                value = outval;

                if (iscommited)
                {
                    OnItemChanged("TryRemove", key, value);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("TryRemove", ex.Message);
            }
            return iscommited;
        }


        /// <summary>
        /// Summary:
        ///     Compares the existing value for the specified key with a specified value,
        ///     and if they are equal, updates the key with a third value.
        ///
        /// Parameters:
        ///   key:
        ///     The key whose value is compared with comparisonValue and possibly replaced.
        ///
        ///   newValue:
        ///     The value that replaces the value of the element with key if the comparison
        ///     results in equality.
        ///
        ///   comparisonValue:
        ///     The value that is compared to the value of the element with key.
        ///
        /// Returns:
        ///     true if the value with key was equal to comparisonValue and replaced with
        ///     newValue; otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is a null reference.
        /// </summary>
        public override bool TryUpdate(string key, IPersistItem newValue, IPersistItem comparisonValue)
        {

            bool iscommited = false;

            try
            {
                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpdateCommand();
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", newValue.Identifier, "Body", newValue.ItemBinary, "Header", newValue.Header, "Retry", newValue.Retry, "ArrivedTime", newValue.ArrivedTime, "Expiration", newValue.Duration, "MessageState", newValue.MessageState), (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryUpdate(key, newValue, comparisonValue))
                                    {
                                        trans.Commit();
                                        iscommited = true;
                                    }
                                }
                            });
                        }
                        break;
                    default:

                        if (dictionary.TryUpdate(key, newValue, comparisonValue))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var cmdText = DbUpdateCommand();
                                var res = ExecuteAsync(cmdText, DataParameter.Get<SQLiteParameter>("Identifier", newValue.Identifier, "Body", newValue.ItemBinary, "Header", newValue.Header, "Retry", newValue.Retry, "ArrivedTime", newValue.ArrivedTime, "Expiration", newValue.Duration, "MessageState", newValue.MessageState));
                            }
                            iscommited = true;
                        }
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryUpdate", key, newValue);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("TryUpdate", ex.Message);
            }
            return iscommited;

        }


        #endregion

        #region override
        protected override void OnErrorOcurred(string action, string message)
        {
            base.OnErrorOcurred(action, message);
            QLogger.InfoFormat("PersistentQueue OnError : Name:{0}, action:{1}, message:{2}", this.Name, action, message);
        }
        protected override void OnInitilaized(EventArgs e)
        {
            base.OnInitilaized(e);
            QLogger.InfoFormat("PersistentQueue OnInitilaized : Name:{0}", this.Name);

        }

        protected override void OnItemChanged(string action, string key, IPersistItem value)
        {
            base.OnItemChanged(action, key, value);
            QLogger.InfoFormat("PersistentQueue OnItemChanged : Name:{0}", this.Name, action, key, key == null ? "" : value.Print());

        }

     
        #endregion

    }
}
