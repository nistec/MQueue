using Nistec.Collections;
using Nistec.Data;
using Nistec.Data.Entities;
using Nistec.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Nistec.Messaging.Db
{

    public enum CommitMode
    {
        OnDisk = 0,
        OnMemory = 1,
        None = 2
    }
    public enum SynchronizationModes
    {
        Normal = 0,
        Full = 1,
        Off = 2
    }
    public enum SQLiteJournalModeEnum
    {
        Default = -1,
        Delete = 0,
        Persist = 1,
        Off = 2,
        Truncate = 3,
        Memory = 4,
        Wal = 5
    }

    public interface IPersistItem : IEntityItem
    {
        // byte[] Serilaize();
        string key { get; }
    }

    public class PersistItem : IPersistItem, IPersistEntity
    {
        public string key { get; set; }
        public object body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }

        public object value()
        {
            return body;
        }
    }

    public class PersistTextItem : IPersistItem
    {
        public string key { get; set; }
        public string body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class PersistBinaryItem : IPersistItem
    {
        public string key { get; set; }
        public byte[] body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class BagItem : IPersistItem
    {
        public string key { get; set; }
        public string body { get; set; }
        public int state { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class Tasker : QueueListener<PersistanceTask>
    {
        public static readonly Tasker Queue = new Tasker();

        //protected override void OnMessageArraived(Generic.GenericEventArgs<TaskPersistance> e)
        //{
        //    base.OnMessageArraived(e);
        //}

        protected override void OnMessageReceived(GenericEventArgs<PersistanceTask> e)
        {
            base.OnMessageReceived(e);
            e.Args.ExecuteAsync();
        }

    }

    public class PersistanceTask
    {
        public string CommandType { get; set; }
        public string CommandText { get; set; }
        public string ConnectionString { get; set; }
        public IDbDataParameter[] Parameters { get; set; }
        public int Result { get; set; }


        public void ExecuteTask(bool enableTasker)
        {
            //TaskItem item = new TaskItem(() => Execute(), 0);
            if (enableTasker)
                Tasker.Queue.Enqueue(this);
            else
                ExecuteAsync();
        }

        public void ExecuteAsync()
        {
            Task.Factory.StartNew(() => Execute());
        }

        public void Execute(DBProvider provider= DBProvider.SqlServer)
        {
            using (var db = new DbContext(ConnectionString, provider))
            {
                Result = db.ExecuteCommandNonQuery(CommandText, Parameters);
            }
        }
    }

    public class DbLiteSettings
    {
        public const string DefaultPassword = "giykse876435365&%$^#%@$#@)_(),kxa;l bttsklf12[]}{}{)(*XCJHG^%%";
        public const string InMemoryFilename = ":memory:";

        public DbLiteSettings()
        {
            DefaultTimeout = 5000;
            //SyncMode = SynchronizationModes.Off;
            //JournalMode = SQLiteJournalModeEnum.Memory;
            //PageSize = 65536;
            //CacheSize = 16777216;
            FailIfMissing = false;
            ReadOnly = false;
            InMemory = false;

            //PRAGMA main.locking_mode = EXCLUSIVE;
            SyncMode = SynchronizationModes.Normal;
            JournalMode = SQLiteJournalModeEnum.Wal;
            PageSize = 4096;
            CacheSize = 100000;


            Name = "xPersistent";
            AutoSave = true;
            EnableTasker = false;
            DbPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            //Filename = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + ".xconfig";
            //RootTag = "xconfig";
            UseFileWatcher = true;
            CommitMode = CommitMode.OnDisk;
            EnableTasker = false;
            EnableCompress = false;
            CompressLevel = 0;
            //Encrypted = false;
            //Password = DefaultPassword;
        }

        public void SetFast()
        {
            DefaultTimeout = 5000;
            SyncMode = SynchronizationModes.Off;
            JournalMode = SQLiteJournalModeEnum.Memory;
            PageSize = 65536;
            CacheSize = 16777216;
            FailIfMissing = false;
        }

        public void Validate()
        {
            //512 to 65536

            if (PageSize < 512)
                PageSize = 1024;
            else if (PageSize > 65536)
                PageSize = 65536;

            if (CacheSize < 1000)
                CacheSize = 10000;
        }

        public string GetConnectionString()
        {
            Validate();

            SQLiteConnectionStringBuilder conString = new SQLiteConnectionStringBuilder();
            if (InMemory)
                conString.DataSource = InMemoryFilename;
            else
                conString.DataSource = DbFilename;

            conString.DefaultTimeout = DefaultTimeout;
            conString.SyncMode = SyncMode;
            conString.JournalMode = JournalMode;
            conString.PageSize = PageSize;
            conString.CacheSize = CacheSize;
            conString.FailIfMissing = FailIfMissing;
            conString.ReadOnly = ReadOnly;
            return conString.ConnectionString;

        }

        public int DefaultTimeout { get; set; }
        public SynchronizationModes SyncMode { get; set; }
        public SQLiteJournalModeEnum JournalMode { get; set; }
        public int PageSize { get; set; }
        public int CacheSize { get; set; }
        public bool FailIfMissing { get; set; }
        public bool ReadOnly { get; set; }
        public bool InMemory { get; set; }

        /// <summary>
        /// Get The config full file path.
        /// </summary>
        public string ConfigFilename { get { return Path.Combine(DbPath, ".xconfig"); } }
        /// <summary>
        /// Get or Set The db full file path.
        /// </summary>
        public string DbFilename { get { return Path.Combine(DbPath, Name + ".db"); } }


        /// <summary>
        /// Get or Set The Persistent Name.
        /// Default is 'xPersistent';
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get or Set value indicating is XConfig will save changes to config file when each item value has changed, 
        /// Default is true;
        /// </summary>
        public bool AutoSave { get; set; }

        /// <summary>
        /// Get or Set The config file path.
        /// Default is (Current Location);
        /// </summary>
        public string DbPath { get; set; }

        /// <summary>
        /// Use event of a System.IO.FileSystemWatcher class.
        /// Default is true;
        /// </summary>
        public bool UseFileWatcher { get; set; }

        /// <summary>
        /// Use commit mode.
        /// Default is OnDisk;
        /// </summary>
        public CommitMode CommitMode { get; set; }

        /// <summary>
        /// Get or Set if enable tasker queue for CommitMode.OnMemory
        /// </summary>
        public bool EnableTasker { get; set; }

        /// <summary>
        /// Get or Set if enable compress data
        /// </summary>
        public bool EnableCompress { get; set; }
        /// <summary>
        /// Get or Set the compress level (1-9) if enable compress data
        /// </summary>
        public int CompressLevel { get; set; }

        public int ValidCompressLevel
        {
            get
            {
                if (CompressLevel < 1 || CompressLevel > 9)
                    return 9;
                return CompressLevel;
            }
        }

    }

    public class PersistentBinary<T> : PersistentDictionary<T, PersistBinaryItem>
    {

        #region ctor

        public PersistentBinary(DbLiteSettings settings)
            : base(settings)
        {

        }

        public PersistentBinary(string name)
            : base(new DbLiteSettings() { Name = name })
        {

        }

        #endregion

        #region override

        //const string sqlcreate = @"CREATE TABLE bookmarks(
        //    users_id INTEGER,
        //    lessoninfo_id INTEGER,
        //    UNIQUE(users_id, lessoninfo_id)
        //);";

        const string sqlcreate = @"CREATE TABLE IF NOT EXISTS {0} (
                          key TEXT PRIMARY KEY,
                          body BLOB,
                          name TEXT,
                          timestamp DATETIME DEFAULT CURRENT_TIMESTAMP     
                        ) WITHOUT ROWID;";
        const string sqlinsert = "insert into {0} (key, body, name) values (@key, @body, @name)";
        const string sqldelete = "delete from {0} where key=@key";
        const string sqlupdate = "update {0} set body=@body, timestamp=CURRENT_TIMESTAMP where key=@key";
        const string sqlinsertOrIgnore = "insert or ignore into {0}(key, body, name) values(@key, @body, @name)";
        const string sqlinsertOrReplace = "insert or replace into {0}(key, body, name) values(@key, @body, @name)";
        const string sqlselect = "select {1} from {0} where key=@key";
        const string sqlselectall = "select {1} from {0}";

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
            if (where == null)
                return string.Format(sqlselectall, Name, select);

            return string.Format(sqlselect, Name, select, where);
        }

        protected override string DbLookupCommand()
        {
            return string.Format(sqlselect, Name, "body");
        }

        protected override string DbUpdateStateCommand()
        {
            return string.Format(sqlupdate, Name);
        }

        protected override object GetDataValue(T item)
        {
            return BinarySerializer.SerializeToBytes(item);
            //return value;
        }
        protected override string GetItemKey(PersistBinaryItem value)
        {
            return value.key;
        }

        protected override T DecompressValue(PersistBinaryItem value)
        {
            return BinarySerializer.Deserialize<T>(value.body);
        }

        protected override PersistBinaryItem ToPersistItem(string key, T item)
        {
            return new PersistBinaryItem()
            {
                body = BinarySerializer.SerializeToBytes(item),
                key = key,
                name = this.Name,
                timestamp = DateTime.Now
            };
        }
        protected override T FromPersistItem(PersistBinaryItem value)
        {
            return BinarySerializer.Deserialize<T>(value.body);
        }


        #endregion

        #region Commands


        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="item">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int AddOrUpdate(string key, T item)
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
                var value = ToPersistItem(key, item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpsertCommand();
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name), (result) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = item;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
                            });
                        }
                        break;
                    case CommitMode.OnMemory:
                        {
                            dictionary.AddOrUpdate(key, item, (oldkey, oldValue) =>
                            {
                                return item;
                            });
                            var cmdText = DbUpsertCommand();
                            ExecuteTask(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name));
                            res = 1;
                            iscommited = true;
                        }
                        break;
                    default:
                        dictionary.AddOrUpdate(key, item, (oldkey, oldValue) =>
                        {
                            return item;
                        });

                        res = 1;
                        iscommited = true;
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("AddOrUpdate", null, item);
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
        /// <param name="item">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int Update(string key, T item)
        {
            bool iscommited = false;
            int res = 0;

            try
            {
                var value = ToPersistItem(key, item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpdateCommand();
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name), (result) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = item;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
                            });
                        }

                        break;
                    case CommitMode.OnMemory:
                        {
                            dictionary[key] = item;
                            var cmdText = DbUpdateCommand();
                            ExecuteTask(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name));
                            res = 1;
                            iscommited = true;
                        }
                        break;
                    default:
                        dictionary[key] = item;
                        res = 1;
                        iscommited = true;
                        break;
                }
                if (iscommited)
                {
                    OnItemChanged("Update", null, item);
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
        /// <param name="item">The value of the element to add. The value can be a null reference (Nothing
        ///     in Visual Basic) for reference types.</param>
        /// <returns>
        /// true if the key/value pair was added to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     successfully. If the key already exists, this method returns false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.OverflowException">The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public override bool TryAdd(string key, T item)
        {

            bool iscommited = false;

            try
            {

                var value = ToPersistItem(key, item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbAddCommand();

                            //db.ExecuteNonQueryTrans(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name), (result) =>
                            //{

                            //    if (result > 0)
                            //    {
                            //        if (dictionary.TryAdd(key, item))
                            //        {
                            //            //trans.Commit();
                            //            iscommited = true;
                            //        }
                            //    }
                            //    return iscommited;
                            //});


                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryAdd(key, item))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    case CommitMode.OnMemory:
                        {
                            if (dictionary.TryAdd(key, item))
                            {
                                var cmdText = DbAddCommand();
                                //var res = ExecuteAsync(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name));
                                ExecuteTask(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name));
                                iscommited = true;
                            }
                        }
                        break;
                    default:
                        iscommited = dictionary.TryAdd(key, item);
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryAdd", key, item);
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
        public override bool TryRemove(string key, out T item)
        {

            bool iscommited = false;
            T outval = item = default(T);
            try
            {
                //var value = GetPersistItem(key, item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbDeleteCommand();
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryRemove(key, out outval))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    case CommitMode.OnMemory:
                        {
                            if (dictionary.TryRemove(key, out outval))
                            {
                                var cmdText = DbDeleteCommand();
                                ExecuteTask(cmdText, DataParameter.Get<SQLiteParameter>("key", key));
                                iscommited = true;
                            }
                        }
                        break;
                    default:
                        iscommited = dictionary.TryRemove(key, out outval);
                        break;
                }

                item = outval;

                if (iscommited)
                {
                    OnItemChanged("TryRemove", key, item);
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
        public override bool TryUpdate(string key, T newItem, T comparisonValue)
        {

            bool iscommited = false;

            try
            {
                var newValue = ToPersistItem(key, newItem);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpdateCommand();
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", newValue.body, "name", newValue.name), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryUpdate(key, newItem, comparisonValue))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    case CommitMode.OnMemory:
                        {
                            if (dictionary.TryUpdate(key, newItem, comparisonValue))
                            {
                                var cmdText = DbUpdateCommand();
                                ExecuteTask(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", newValue.body, "name", newValue.name));
                                iscommited = true;
                            }
                        }
                        break;
                    default:
                        iscommited = dictionary.TryUpdate(key, newItem, comparisonValue);
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryUpdate", key, newItem);
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

        //public override void LoadDb()
        //{
        //    IList<PersistBinaryItem> list;

        //    using (TransactionScope tran = new TransactionScope())
        //    {
        //        try
        //        {
        //            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
        //            {

        //                var sql = "select * from " + Name;
        //                list = db.Query<PersistBinaryItem>(sql, null);
        //                if (list != null && list.Count > 0)
        //                {
        //                    foreach (var entry in list)
        //                    {
        //                        var val = BinarySerializer.Deserialize<T>((byte[])entry.body);

        //                        //var o = BinarySerializer.Deserialize((byte[])entry.Value);

        //                        //BinaryStreamer streamer = new BinaryStreamer(new NetStream((byte[])entry.Value), null);

        //                        dictionary[entry.key] = val;// GenericTypes.Convert<T>(val);
        //                    }
        //                }

        //            }

        //            tran.Complete();
        //        }
        //        catch (Exception ex)
        //        {
        //            string err = ex.Message;
        //        }
        //    }
        //}

    }

    /// <summary>
    /// Represent a Config file as Dictionary key-value
    /// <example>
    /// <sppSttings>
    /// <myname value='nissim' />
    /// <mycompany value='mcontrol' />
    /// </sppSttings>
    /// </example>
    /// </summary>
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public abstract class PersistentDictionary<T, PI>  where  PI : IPersistItem 
    {

        protected readonly ConcurrentDictionary<string, T> dictionary;

        //XDictionaryettings settings;
        //string connectionString;
        //string dbName;
        public const DBProvider DbProvider = DBProvider.SqlServer;// DbLite.ProviderName;
        protected CommitMode _CommitMode;
        protected bool _EnableTasker;
        
        #region properties

        public DbLiteSettings Settings { get; protected set; }
        public string ConnectionString { get; protected set; }
        public string Name { get; protected set; }

        public bool EnableCompress { get; protected set; }
        public int CompressLevel { get; protected set; }
        /// <summary>
        /// Get value indicating if the config file exists
        /// </summary>
        public bool FileExists
        {
            get
            {
                return File.Exists(Settings.DbFilename);
            }
        }

        #endregion

        #region events

        bool ignorEvent = false;
        //public event ConfigChangedHandler ItemChanged;
        //public event EventHandler ConfigFileChanged;
        public event GenericEventHandler<string> ErrorOcurred;
        public event EventHandler Initilaized;
        public event EventHandler BeginLoading;
        public event GenericEventHandler<string, int> LoadCompleted;
        //public event GenericEventHandler<string, T> ItemLoaded;
        public event GenericEventHandler<string, string, T> ItemChanged;
        public event EventHandler ClearCompleted;

        public Action<T> ItemLoaded { get; set; }

        protected virtual void OnInitilaized(EventArgs e)
        {
            if (Initilaized != null)
            {
                Initilaized(this, e);
            }
        }
        protected virtual void OnBeginLoading()
        {
            if (BeginLoading != null)
            {
                BeginLoading(this, EventArgs.Empty);
            }
        }
        protected virtual void OnClearCompleted()
        {
            if (ClearCompleted != null)
            {
                ClearCompleted(this, EventArgs.Empty);
            }
        }
        protected virtual void OnLoadCompleted(string message, int count)
        {
            if (LoadCompleted != null)
            {
                LoadCompleted(this, new GenericEventArgs<string, int>(message, count));
            }
        }
        protected virtual void OnLoadCompleted(GenericEventArgs<string, int> e)
        {
            if (LoadCompleted != null)
            {
                LoadCompleted(this, e);
            }
        }

        protected virtual void OnItemLoaded(T value)
        {
            if (ItemLoaded != null)
                ItemLoaded(value);
            //OnItemLoaded(new GenericEventArgs<string, T>(key, value));
        }

        //protected virtual void OnItemLoaded(GenericEventArgs<string, T> e)
        //{
        //    if (ItemLoaded != null)
        //        ItemLoaded(this, e);
        //}

        protected virtual void OnErrorOcurred(string action, string message)
        {
            if (ErrorOcurred != null)
            {
                OnErrorOcurred(new GenericEventArgs<string>("ErrorOcurred in: " + action + ", Messgae: " + message));
            }
        }
        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
            {
                ErrorOcurred(this, e);
            }
        }

        protected virtual void OnItemChanged(string action, string key, T value)
        {
            if (!ignorEvent)
                OnItemChanged(new GenericEventArgs<string, string, T>(action, key, value));
        }

        protected virtual void OnItemChanged(GenericEventArgs<string, string, T> e)
        {
            if (ItemChanged != null)
                ItemChanged(this, e);
        }

        //protected virtual void OnItemChanged(ConfigChangedArgs e)
        //{
        //    //if (settings.AutoSave)
        //    //{
        //    //    Save();
        //    //}
        //    if (ItemChanged != null)
        //        ItemChanged(this, e);
        //}

        // protected virtual void OnConfigFileChanged(EventArgs e)
        //{
        //    FileToDictionary();

        //    if (ConfigFileChanged != null)
        //        ConfigFileChanged(this, e);
        //}

        #endregion

        #region ctor
        /// <summary>
        /// PersistentDictionary ctor with a specefied filename
        /// </summary>
        /// <param name="filename"></param>
        public PersistentDictionary(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("PersistentDictionary.filename");
            }
            dictionary = new ConcurrentDictionary<string, T>();
            this.Settings = new DbLiteSettings()
            {
                Name = name,
                CommitMode = CommitMode.OnDisk
                //DbPath = dbpath
            };
            Name = Settings.Name;
            _CommitMode = CommitMode.OnDisk;
            Init();
        }

        /// <summary>
        /// PersistentDictionary ctor with a specefied Dictionary
        /// </summary>
        /// <param name="dict"></param>
        public PersistentDictionary(string dbpath, string name, IDictionary<string, T> dict)
        {
            if (string.IsNullOrEmpty(dbpath))
            {
                throw new ArgumentNullException("PersistentDictionary.dbpath");
            }
            dictionary = new ConcurrentDictionary<string, T>();
            this.Settings = new DbLiteSettings()
            {
                DbPath = dbpath,
                Name = name,
                CommitMode = CommitMode.OnDisk
            };
            Name = Settings.Name;
            _CommitMode = CommitMode.OnDisk;
            Init();
            Load(dict);
            OnInitilaized(EventArgs.Empty);
        }

        /// <summary>
        /// XConfig ctor with default filename CallingAssembly '.mconfig' in current direcory
        /// </summary>
        public PersistentDictionary(DbLiteSettings settings)
        {
            dictionary = new ConcurrentDictionary<string, T>();
            this.Settings = settings;
            Name = Settings.Name;
            _CommitMode = Settings.CommitMode;
            Init();
            //if (loadPersistItems)
            //    LoadDb();
            //else
            //    Clear();
            OnInitilaized(EventArgs.Empty);
        }
        #endregion

        private void Init()
        {
            //string filename = null;
            try
            {
                if (this.Settings.InMemory)
                {
                    //filename = DbLiteSettings.InMemoryFilename;
                    throw new Exception("PersistentDictionary do not supported in memory db");
                }
                else
                {
                    //filename = this.Settings.DbFilename;

                    //DbLiteUtil.CreateFolder(Settings.DbPath);
                    //DbLiteUtil.CreateFile(filename);
                    //DbLiteUtil.ValidateConnection(filename, true);
                }
                ConnectionString = Settings.GetConnectionString();
                EnableCompress = Settings.EnableCompress;
                CompressLevel = Settings.ValidCompressLevel;

                using (var db = new DbContext(ConnectionString, DbProvider))
                {
                    var sql = DbCreateCommand();
                    db.ExecuteCommandNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Initilaized Error: " + ex.Message));
                throw ex;
            }

        }

        public virtual void LoadDb()
        {
            IList<PI> list;
            OnBeginLoading();
            using (TransactionScope tran = new TransactionScope())
            {
                try
                {
                    using (var db = new DbContext(ConnectionString, DbProvider))
                    {
                        var sql = "select * from " + Name;
                        list = db.Query<PI>(sql, null);
                        if (list != null && list.Count > 0)
                        {
                            foreach (var entry in list)
                            {
                                var val = FromPersistItem(entry);
                                if (val != null)
                                {
                                    dictionary[entry.key] = val;
                                    OnItemLoaded(val);
                                    //if (onTake != null)
                                    //    onTake(val);
                                }
                            }
                        }

                    }

                    tran.Complete();
                    OnLoadCompleted(this.Name, this.Count);
                }
                catch (Exception ex)
                {
                    OnErrorOcurred("LoadDb", ex.Message);
                }
            }
        }


        //string sqlCreateTable = "create table demo_score (name varchar(20), score int)";

        protected abstract string DbCreateCommand();// DbLite db);
        protected abstract string DbUpsertCommand();// string key, T value);
        protected abstract string DbAddCommand();//string key, T value);
        protected abstract string DbUpdateCommand();//string key, T value);
        protected abstract string DbDeleteCommand();//string key);
        protected abstract string DbSelectCommand(string select, string where);
        protected abstract string DbLookupCommand();//string key);
        protected abstract string DbUpdateStateCommand();//string key, int state);
        protected abstract string GetItemKey(PI value);
        protected abstract object GetDataValue(T item);
        protected abstract T DecompressValue(PI value);
        protected abstract PI ToPersistItem(string key, T item);
        protected abstract T FromPersistItem(PI value);


        #region Commands
        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual int AddOrUpdate(string key, T value)
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
                var body = GetDataValue(value);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbContext(ConnectionString, DbProvider))
                        {
                            var cmdText = DbUpsertCommand();//key, value);
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SqlParameter>("key", key, "body", body), (result) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = value;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
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
                            var task = new PersistanceTask()
                            {
                                CommandText = DbUpsertCommand(),//key, value),
                                CommandType = "DbUpsert",
                                ConnectionString = ConnectionString,
                                Parameters = DataParameter.Get<SqlParameter>("key", key, "body", body)
                            };
                            task.ExecuteTask(_EnableTasker);
                        }
                        res = 1;
                        iscommited = true;
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("AddOrUpdate", null, default(T));
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
        public virtual int Update(string key, T value)
        {
            bool iscommited = false;
            int res = 0;

            try
            {
                var body = GetDataValue(value);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbContext(ConnectionString, DbProvider))
                        {
                            var cmdText = DbUpdateCommand();//key, value);
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SqlParameter>("key", key, "body", body), (result) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = value;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
                            });
                        }

                        break;
                    default:
                        dictionary[key] = value;
                        if (_CommitMode == CommitMode.OnMemory)
                        {
                            var task = new PersistanceTask()
                            {
                                CommandText = DbUpdateCommand(),//key, value),
                                CommandType = "DbUpdate",
                                ConnectionString = ConnectionString,
                                Parameters = DataParameter.Get<SqlParameter>("key", key, "body", body)
                            };
                            task.ExecuteTask(_EnableTasker);
                        }
                        res = 1;
                        iscommited = true;
                        break;
                }
                if (iscommited)
                {
                    OnItemChanged("Update", null, default(T));
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
        public virtual bool TryAdd(string key, T value)
        {

            bool iscommited = false;

            try
            {
                var body = GetDataValue(value);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbContext(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbAddCommand();//key, value);
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SqlParameter>("key", key, "body", body), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryAdd(key, value))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:
                        if (dictionary.TryAdd(key, value))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistanceTask()
                                {
                                    CommandText = DbAddCommand(),//(key, value),
                                    CommandType = "DbAdd",
                                    ConnectionString = ConnectionString,
                                    Parameters = DataParameter.Get<SqlParameter>("key", key, "body", body)
                                };
                                task.ExecuteTask(_EnableTasker);
                            }
                            iscommited = true;
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
        public virtual bool TryRemove(string key, out T value)
        {

            bool iscommited = false;
            T outval = value = default(T);
            try
            {

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbContext(ConnectionString, DbProvider))
                        {
                            var cmdText = DbDeleteCommand();//key);
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SqlParameter>("key", key), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryRemove(key, out outval))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:
                        if (dictionary.TryRemove(key, out outval))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistanceTask()
                                {
                                    CommandText = DbDeleteCommand(),//key),
                                    CommandType = "DbDelete",
                                    ConnectionString = ConnectionString,
                                    Parameters = DataParameter.Get<SqlParameter>("key", key)
                                };
                                task.ExecuteTask(_EnableTasker);
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
        public virtual bool TryUpdate(string key, T newValue, T comparisonValue)
        {

            bool iscommited = false;

            try
            {
                var body = GetDataValue(newValue);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbContext(ConnectionString, DbProvider))
                        {
                            var cmdText = DbUpdateCommand();//key, newValue);
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SqlParameter>("key", key, "body", body), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryUpdate(key, newValue, comparisonValue))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:

                        if (dictionary.TryUpdate(key, newValue, comparisonValue))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistanceTask()
                                {
                                    CommandText = DbUpdateCommand(),//key, newValue),
                                    CommandType = "DbUpdate",
                                    ConnectionString = ConnectionString,
                                    Parameters = DataParameter.Get<SqlParameter>("key", key, "body", body)
                                };
                                task.ExecuteTask(_EnableTasker);
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

        #region ConcurrentDictionary<string,T>


        /// <summary>
        /// Gets a value that indicates whether the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue> is empty.
        /// Returns:
        ///     true if the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     is empty; otherwise, false.
        /// </summary>

        public bool IsEmpty { get { return dictionary.IsEmpty; } }

 

        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual void Add(string key, T value)
        {

            TryAdd(key, value);

        }


        /// <summary>
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual bool Remove(string key)
        {
            T value = default(T);
            return TryRemove(key, out value);

        }


        //
        // Summary:
        //     Removes all keys and values from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            string cmdText = "delete from [" + Name + "]";
            bool iscommited = false;

            try
            {
                //Insert create script here.
                using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                {
                    db.ExecuteTransCommandNonQuery(cmdText, (result) =>
                    {
                        if (result > 0)
                        {
                            dictionary.Clear();
                            //trans.Commit();
                            iscommited = true;
                        }
                        return iscommited;
                    });
                }
                if (iscommited)
                {
                    OnClearCompleted(); //OnItemChanged("Clear", null, default(T));
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("Clear", ex.Message);
            }

        }


        /// <summary>
        /// Determines whether the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.</param>
        /// <returns>
        ///     true if the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is a null reference</exception>
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }

   
        /// <summary>
        /// Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key
        ///     if the key is already in the dictionary, or the new value if the key was
        ///     not in the dictionary.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"> key is a null reference.</exception>
        /// <exception cref="System.OverflowException"> The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public T GetOrAdd(string key, T value)
        {
            T val;
            if (dictionary.TryGetValue(key, out val))
            {
                return val;
            }
            if (TryAdd(key, value))
            {
                return value;
            }
            //int res = TryAdd(key, value);
            //if (res > 0)
            //{
            //    return value;
            //}
            return default(T);
            //return dictionary.GetOrAdd(key, value);
        }

        /// <summary>
        /// Copies the key and value pairs stored in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     to a new array.
        /// </summary>
        /// <returns>
        ///  A new array containing a snapshot of key and value pairs copied from the
        ///     System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        /// </returns>
        public KeyValuePair<string, T>[] ToArray()
        {
            return dictionary.ToArray();
        }



        /// <summary>
        /// Summary:
        ///     Attempts to get the value associated with the specified key from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the value to get.
        ///
        ///   value:
        ///     When this method returns, value contains the object from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     with the specified key or the default value of , if the operation failed.
        ///
        /// Returns:
        ///     true if the key was found in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is a null reference (Nothing in Visual Basic).
        /// </summary>
        public bool TryGetValue(string key, out T value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        #endregion

        #region IDictionary implemenation

        /// <summary>
        /// Summary:
        ///     Gets an System.Collections.Generic.ICollection<T> containing the keys of
        ///     the System.Collections.Generic.IDictionary<TKey,TValue>.
        ///
        /// Returns:
        ///     An System.Collections.Generic.ICollection<T> containing the keys of the object
        ///     that implements System.Collections.Generic.IDictionary<TKey,TValue>.
        /// </summary>
        public ICollection<string> Keys { get { return dictionary.Keys; } }

        /// <summary>
        /// Summary:
        ///     Gets an System.Collections.Generic.ICollection<T> containing the values in
        ///     the System.Collections.Generic.IDictionary<TKey,TValue>.
        ///
        /// Returns:
        ///     An System.Collections.Generic.ICollection<T> containing the values in the
        ///     object that implements System.Collections.Generic.IDictionary<TKey,TValue>.
        /// </summary>
        public ICollection<T> Values { get { return dictionary.Values; } }

        /// <summary>
        /// Summary:
        ///     Gets or sets the element with the specified key.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the element to get or set.
        ///
        /// Returns:
        ///     The element with the specified key.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is null.
        ///
        ///   System.Collections.Generic.KeyNotFoundException:
        ///     The property is retrieved and key is not found.
        ///
        ///   System.NotSupportedException:
        ///     The property is set and the System.Collections.Generic.IDictionary<TKey,TValue>
        ///     is read-only.
        /// </summary>
        public T this[string key]
        {
            get { return dictionary[key]; }
            set
            {

                AddOrUpdate(key, value);
            }
        }

  
        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>>

        // Summary:
        //     Gets the number of elements contained in the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     The number of elements contained in the System.Collections.Generic.ICollection<T>.
        public int Count { get { return dictionary.Count; } }
        //
        // Summary:
        //     Gets a value indicating whether the System.Collections.Generic.ICollection<T>
        //     is read-only.
        //
        // Returns:
        //     true if the System.Collections.Generic.ICollection<T> is read-only; otherwise,
        //     false.
        public bool IsReadOnly
        {
            get
            {
                return false;// dictionary.IsReadOnly; 
            }
        }

        // Summary:
        //     Adds an item to the System.Collections.Generic.ICollection<T>.
        //
        // Parameters:
        //   item:
        //     The object to add to the System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
            //dictionary.Add(item);
        }
        //
        // Summary:
        //     Removes all items from the System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        //public void Clear()
        //{

        //}
        //
        // Summary:
        //     Determines whether the System.Collections.Generic.ICollection<T> contains
        //     a specific value.
        //
        // Parameters:
        //   item:
        //     The object to locate in the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     true if item is found in the System.Collections.Generic.ICollection<T>; otherwise,
        //     false.
        public bool Contains(KeyValuePair<string, T> item)
        {
            T val;
            if (TryGetValue(item.Key, out val))
            {
                return item.Value.Equals(val);
            }
            return false;

            //return dictionary.ContainsKey(item.Key);
        }
        //
        // Summary:
        //     Copies the elements of the System.Collections.Generic.ICollection<T> to an
        //     System.Array, starting at a particular System.Array index.
        //
        // Parameters:
        //   array:
        //     The one-dimensional System.Array that is the destination of the elements
        //     copied from System.Collections.Generic.ICollection<T>. The System.Array must
        //     have zero-based indexing.
        //
        //   arrayIndex:
        //     The zero-based index in array at which copying begins.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     array is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     arrayIndex is less than 0.
        //
        //   System.ArgumentException:
        //     The number of elements in the source System.Collections.Generic.ICollection<T>
        //     is greater than the available space from arrayIndex to the end of the destination
        //     array.
        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            array = dictionary.ToArray();//.CopyTo(array,arrayIndex);
        }
        //
        // Summary:
        //     Removes the first occurrence of a specific object from the System.Collections.Generic.ICollection<T>.
        //
        // Parameters:
        //   item:
        //     The object to remove from the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     true if item was successfully removed from the System.Collections.Generic.ICollection<T>;
        //     otherwise, false. This method also returns false if item is not found in
        //     the original System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        public bool Remove(KeyValuePair<string, T> item)
        {
            T val;
            return TryRemove(item.Key, out val);
            // return dictionary.Remove(item);
        }

        #endregion

        #region IEnumerable<out T> : IEnumerable

        // Summary:
        //     Returns an enumerator that iterates through the collection.
        //
        // Returns:
        //     A System.Collections.Generic.IEnumerator<T> that can be used to iterate through
        //     the collection.
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return dictionary.GetEnumerator();// ((IDictionary<string, T>)dictionary).GetEnumerator();
        }


        #endregion

        #region select query

        public T SelectValue(string key)
        {
            //T value-default(T);
            var sql = DbLookupCommand();
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var res = db.QuerySingle<PI>(sql, null);

                if (EnableCompress)
                {
                    return DecompressValue(res);
                }
                return FromPersistItem(res);
            }
        }

        public IEnumerable<IPersistEntity> QueryDictionaryItems()
        {

            List<IPersistEntity> list = new List<IPersistEntity>();
            try
            {
                if (dictionary != null && dictionary.Count > 0)
                {
                    foreach (var g in this.dictionary)
                    {
                        list.Add(new PersistItem() { body = g.Value, key = g.Key, name = this.Name, timestamp = DateTime.Now });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }
        public IList<PersistItem> QueryItems(string select, string where, params object[] keyValueParameters)
        {
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var sql = DbSelectCommand(select, where);
                var list = db.Query<PersistItem>(sql, keyValueParameters);
                return list;
            }
        }

        public IList<T> Query(string select, string where, params object[] keyValueParameters)
        {
            IList<T> list = new List<T>();
            using (var db = new DbContext(ConnectionString, DbProvider))
            {
                var sql = DbSelectCommand(select, where);
                var res = db.Query<PI>(sql, keyValueParameters);
                if (EnableCompress)
                {
                    for (int i = 0; i < res.Count; i++)
                    {
                        var item = DecompressValue(res[i]);
                        list.Add(item);
                        //res[i] = item;
                    }
                }
                else
                {
                    for (int i = 0; i < res.Count; i++)
                    {
                        var item = FromPersistItem(res[i]);
                        list.Add(item);
                        //res[i] = item;
                    }
                }
                return list;
            }
        }

        public T QuerySingle(string select, string where, params object[] keyValueParameters)
        {
            using (var db = new DbContext(ConnectionString, DBProvider.SQLite))
            {
                var sql = DbSelectCommand(select, where);
                var res = db.QuerySingle<PI>(sql, keyValueParameters);
                if (EnableCompress)
                {
                    return DecompressValue(res);
                }
                return FromPersistItem(res);
            }
        }

        //public TVal QuerySingle<TVal>(string select, string where, TVal returnIfNull, params object[] keyValueParameters)
        //{
        //    using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
        //    {
        //        var sql = DbSelectCommand(select, where);
        //        var res = db.QueryScalar<TVal>(sql, returnIfNull, keyValueParameters);

        //        return res;
        //    }
        //}


        #endregion

        public void ReloadOrClearPersist(bool loadPersistItems)
        {
            if (loadPersistItems)
            {
                dictionary.Clear();
                LoadDb();
            }
            else
                Clear();
        }

        public void Load(IDictionary<string, T> dict)
        {
            ignorEvent = true;
            try
            {

                foreach (KeyValuePair<string, T> entry in dict)
                {
                    AddOrUpdate(entry.Key, entry.Value);
                }
            }
            finally
            {
                ignorEvent = false;
            }
        }

        /// <summary>
        /// Compress data to byte array
        /// </summary>
        /// <param name="value"></param>
        /// <param name="level">from 1-9, 1 is fast p is bettr</param>
        /// <returns></returns>
        public byte[] Compress(string value, int level)
        {
            return Nistec.Generic.NetZipp.Compress(value);
        }

        public string Decompress(byte[] b)
        {
            return Nistec.Generic.NetZipp.Decompress(b);
        }

        /// <summary>
        /// Compress data to byte array
        /// </summary>
        /// <param name="value"></param>
        /// <param name="level">from 1-9, 1 is fast p is bettr</param>
        /// <returns></returns>
        public string Zip(string value, int level)
        {
            return Nistec.Generic.NetZipp.Zip(value);
        }

        public string UnZip(string compressed)
        {
            return Nistec.Generic.NetZipp.UnZip(compressed);
        }


        public Task ExecuteTask(string cmdText, IDbDataParameter[] parameters)
        {
            return Task.Factory.StartNew<int>(() => Execute(cmdText, parameters));
        }

        public int ExecuteAsync(string cmdText, IDbDataParameter[] parameters)
        {
            var result = Task.Factory.StartNew<int>(() => Execute(cmdText, parameters));
            return (result == null) ? 0 : result.Result;
        }

        public int Execute(string cmdText, IDbDataParameter[] parameters)
        {
            using (var db = new DbContext(ConnectionString, DbProvider))
            {
                return db.ExecuteCommandNonQuery(cmdText, parameters);
            }
        }

    }
}
