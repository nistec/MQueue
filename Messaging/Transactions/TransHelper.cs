using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace Nistec.Messaging.Transactions
{
    public static class TransHelper
    {
        public const int DefaultTransTimeout = 60;

        public static TransactionOptions GetTransactionOptions()
        {
            return GetTransactionOptions(TimeSpan.FromSeconds(DefaultTransTimeout));
        }

        public static TransactionOptions GetTransactionOptions(TimeSpan timeout)
        {
            var scopeOptions = new TransactionOptions();
            scopeOptions.IsolationLevel = IsolationLevel.ReadCommitted;
            scopeOptions.Timeout = timeout;// TimeSpan.FromSeconds(60);
            return scopeOptions;
        }

        public static TransactionScope GetTransactionScope(TimeSpan timeout)
        {
            return new TransactionScope(TransactionScopeOption.Required, GetTransactionOptions(timeout));
        }

        public static TransactionScope GetTransactionScope()
        {
            return new TransactionScope(TransactionScopeOption.Required, GetTransactionOptions());
        }
    }
}
