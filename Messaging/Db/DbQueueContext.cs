using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Data;
using Nistec.Data.Entities;
using Nistec.Generic;
using Nistec.Data;

namespace Nistec.Messaging.Db
{
   
    #region ResourceLang

    [Serializable]
    public class DbQueueResources : EntityLocalizer
    {
        public static CultureInfo GetCulture()
        {
            return new CultureInfo( "he-IL");
        }

        #region override

        protected override string CurrentCulture()
        {
            return GetCulture().Name;
        }

        protected override void BindLocalizer()
        {
            //init by config key
            //base.Init(CurrentCulture(), "DataEntityDemo.Resources.AdventureWorks");
            //or
            //base.Init("Nistec.Data.Resources.DbQueue");
            //or
            //base.Init(MControl.Sys.NetResourceManager.GetResourceManager("DataEntityDemo.Resources.AdventureWorks", this.GetType()));
            //or
            //base.Init(MControl.Sys.NetResourceManager.GetResourceManager( "DataEntityDemo.Resources.AdventureWorks",this.GetType()));
        }
        #endregion
    }
    #endregion

    #region DbContext

    [DbContext("DbQueueContext", ConnectionKey = "cnn_queue", Provider = DBProvider.SqlServer)]
    [Serializable]
    public class DbQueueContext : DbContext
    {
        public static string Cnn
        {
            get { return NetConfig.AppSettings["cnn_queue"]; }
        }

        protected override void EntityBind()
        { 
            //base.SetConnection("AdventureWorks", Cnn, DBProvider.SqlServer);
            //base.SetEntity("Contact", "Person.Contact", new EntityKeys("ContactID"));
            //base.SetEntity<ActiveContact>();
        }

        public static IDbContext Instance
        {
            get { return new DbQueueContext(); }
        }

        
    }
    #endregion

}
