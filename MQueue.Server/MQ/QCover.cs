using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Messaging.Adapters;
using Nistec.Data.SqlClient;
using Nistec.Messaging.Db;
using Nistec.Data.Sqlite;
using System.IO;
using Nistec.Runtime;
using Nistec.Messaging.Io;
using Nistec.Messaging.Server;

namespace Nistec.Messaging
{
    public enum CoverProviders : byte
    {
        Sqlite,
        File,
        Db
    }

    public class QCover
    {
        public const string CoverTableName = "qcover";

        #region properties
        public CoverProviders CoverProvider { get; set; }
        public string CoverPath { get; set; }
        //public string DbConnection { get; set; }
        //public bool Coverable { get; set; }
        public int ConnectTimeout { get; set; }

        DbLiteSettings dbSettings;

        #endregion

        #region methods

        string GetFilename(string identifier)
        {
            return Path.Combine(CoverPath, Assists.GetFilename(identifier));
        }

        public void Save(QueueItemStream message)
        {
            try
            {
                if (CoverProvider == CoverProviders.File)
                {
                    
                    string filename = GetFilename(message.Identifier);
                    message.SaveToFile(filename);
                    //var stream = message.Serialize(true);
                    //stream.SaveToFile(filename);
                }
                else
                {
                    ExecCover(message);
                }
            }
            catch (Exception ex)
            {
                QLog.Exception("QCover.Save ", ex);
            }
        }


        public void Save(IQueueItem item)
        {
            try
            {
                if (CoverProvider == CoverProviders.File)
                {
                    string filename = GetFilename(item.Identifier);
                    var stream = item.BodyStream;
                    stream.SaveToFile(filename);
                }
                if (CoverProvider == CoverProviders.Sqlite)
                {

                    PersistentQueue bag = new PersistentQueue(dbSettings);
                    bag.AddOrUpdate
                    //var dt = DbLite.MessageToDataTable(message);
                    //using (Nistec.Data.Sqlite.DbLite db = new Nistec.Data.Sqlite.DbLite(CoverPath))
                    //{
                    //    db.ExecuteNonQuery("",)
                    //    //bulk.BulkInsert(dt, CoverTableName, ConnectTimeout, null);
                    //}
                }
                else
                {
                    //ExecCover(((QueueItemStream)item).Copy());
                    ExecCover((QueueItemStream)item);
                }
            }
            catch (Exception ex)
            {
                QLog.Exception("QCover.Save ", ex);
            }
        }

        void ExecCover(QueueItemStream message)
        {
            var dt = DbQueue.MessageToDataTable(message);

            using (DbBulkCopy bulk = new DbBulkCopy(CoverPath))
            {
                bulk.BulkInsert(dt, CoverTableName, ConnectTimeout, null);
            }
        }

        #endregion
    }
}
