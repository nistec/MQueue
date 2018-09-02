using System;
using System.Collections.Generic;
using System.Text;
using Nistec.Data;
using System.Data;

namespace Nistec.Messaging.UI
{
    public class Dal : Nistec.Data.SqlClient.DbCommand
    {

        public Dal(IDalBase dalBase)
            : base(dalBase)
        {

        }

        public DataTable ExecuteDataTable(string sql)
        {
            return base.ExecuteCommand<DataTable>(sql);
        }


    }
}
