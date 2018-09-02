using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Proxies
{
    public static class MailerDefaults
    {
        public const string FolderRelay = "_Relay";
        public const string FolderRetry = "_Retry";
        public const string FolderQuick = "_Quick";

        public const int MinChunkItems = 10;

        public const int DefaultIntervalSetting = 60000;
        //public const int DefaultIntervalMinute = 10;
    }
}
