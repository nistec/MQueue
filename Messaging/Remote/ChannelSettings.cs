using Nistec.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Remote
{
    public class ChannelSettings
    {
        public static NetProtocol DefaultProtocol = NetProtocol.Pipe;
        public const bool IsAsync = false;

        public const string HttpMethod = "post";
        public const int HttpTimeout = 10000;

        //public const int TcpPort = 10000;
        public const int TcpTimeout = 10000;



        public const string RemoteQueueHostName ="";
        public const bool   EnableRemoteException=false;


    }
}
