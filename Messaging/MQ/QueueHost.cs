using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nistec.Runtime;
using Nistec.Messaging.Io;
using Nistec.Serialization;
using Nistec.Channels;
using Nistec.Generic;

namespace Nistec.Messaging
{
    [Serializable]
    public class QueueHost : HostChannel, IDisposable//, ISerialEntity
    {
        #region ctor

        public QueueHost() : base()
        {
            CommitMode = PersistCommitMode.None;
            CoverMode = CoverMode.Memory;
            ReloadOnStart = false;
        }
        public QueueHost(string address):base(address)
        {
            
        }

        public QueueHost(HostProtocol protocol, string serverAddress, string hostPort, string hostName): base(protocol, serverAddress, hostPort, hostName)
        {
            
        }
        public QueueHost(HostProtocol protocol, string serverAddress, int port, string hostName) : base(protocol, serverAddress, port.ToString(), hostName)
        {

        }
        public QueueHost(HostProtocol protocol, string address, string hostName): base(protocol, address, hostName)
        {
           
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        bool disposed = false;
        /// <summary>
        /// Get indicate wether the current instance is Disposed.
        /// </summary>
        protected bool IsDisposed
        {
            get { return disposed; }
        }
        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Segments = null;

                //HostName = null;
                //ServerName = null;
                //_RawHostAddress = null;
                //_HostAddress = null;
            }
            disposed = true;
        }
        #endregion

        #region properties
        /// <summary>
        /// Cover Mode
        /// </summary>
        public CoverMode CoverMode { get; set; }
        /// <summary>
        /// Commit Mode
        /// </summary>
        public PersistCommitMode CommitMode { get; set; }
        /// <summary>
        /// Reload On Start
        /// </summary>
        public bool ReloadOnStart { get; set; }
        #endregion

        #region Parse
        public static QueueHost ParseLocal(string hostName, string hostAddress = ".")
        {
            if (string.IsNullOrEmpty(hostAddress) || hostAddress == ".")
                hostAddress = ".";

            var qh = new QueueHost();
            qh.Segments[1] = hostAddress;
            qh.Segments[3] = hostName;
            //ServerName = serverName;
            qh.RawHostAddress = string.Format("local:{0}:{1}?{2}", hostAddress, ".", hostName);
            //_HostAddress = "local";
            qh.Protocol = HostProtocol.local;
            qh.Port = 0;
            return qh;
        }


        public static QueueHost Parse(string hostAddress)
        {
            QueueHost host = new QueueHost(hostAddress);
            return host;
        }
        #endregion
    }
    
    /*
    public enum HostProtocol : byte
    {
        local = 0,
        ipc = 1,
        tcp = 2,
        http = 3,
        file = 4,
        db = 5
    }

    [Serializable]
    public class QueueHost : ISerialEntity,IDisposable
    {



        public QueueHost() {
            CommitMode = PersistCommitMode.None;
            CoverMode = CoverMode.Memory;
            ReloadOnStart = false;
        }

        //public QueueHost(string hostName, string serverName, string hostAddress, HostProtocol addressType)
        //{
        //    HostName = hostName;
        //    ServerName = serverName;
        //    _RawHostAddress = hostAddress;
        //    ParseHostAddress(hostAddress, ref _HostAddress, ref addressType, ref _Port);
        //    _HostProtocol = addressType;
        //}

        //public QueueHost(string hostName, string serverName, string hostAddress)
        //{
        //    HostName = hostName;
        //    ServerName = serverName;
        //    _RawHostAddress = hostAddress;
        //    ParseHostAddress(hostAddress, ref _HostAddress, ref _HostProtocol, ref _Port);
        //}

        public QueueHost(string hostName, string serverName = "."):this()
        {
            CreateLocalAddress(hostName);
        }

        public QueueHost(HostProtocol protocol, string serverName, int hostPort, string hostName)
        {
            string address = GetRawAddress(protocol, serverName, hostPort.ToString(), hostName);
            _Port = hostPort;
            _HostAddress = address;
            _HostProtocol = protocol;
            HostName = hostName;
        }

        public QueueHost(HostProtocol protocol, string serverName, string hostPort, string hostName)
        {
            string address = GetRawAddress(protocol, serverName, hostPort, hostName);

            if (protocol == HostProtocol.tcp)
                _Port = Types.ToInt(hostPort);

            _HostAddress = address;
            _HostProtocol = protocol;
            HostName = hostName;
        }


        void CreateLocalAddress(string hostName, string serverName = ".")
        {
            if (string.IsNullOrEmpty(serverName) || serverName == ".")
                serverName = ".";

            HostName = hostName;
            ServerName = serverName;
            _RawHostAddress = string.Format("local:{0}:{1}?{2}", ServerName, ".", hostName);
            _HostAddress = "local";
            _HostProtocol = HostProtocol.local;
            _Port = 0;
        }

        private void Create(string address)
        {
            //protocol:server/address:port/hostName

            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            string[] args = address.Split(':', '?');
            if (args.Length < 4)
            {
                CreateLocalAddress(address);
                //throw new Exception("Host raw address is incorrect");
            }
            else
            {
                //return string.Format("ipc:{0}:{1}?{2}", serverName, hostPort, hostName);

                this._RawHostAddress = address;
                this.HostName = args[3];

                int port = 0;
                string server = args[1].TrimStart('/');
                string protocol = args[0];

                switch (protocol)
                {
                    case "local"://local:.:.?queuName
                        this._HostProtocol = HostProtocol.local;
                        this._Port = port;
                        this._HostAddress = args[2];
                        this.ServerName = server;
                        break;
                    case "ipc"://ipc:.:nistec_enqueue?queuName
                        this._HostProtocol = HostProtocol.ipc;
                        this._Port = port;
                        this._HostAddress = args[2];
                        this.ServerName = server;
                        break;
                    case "file"://file:root:folder?queuName
                        if (server == Assists.EXECPATH)
                            server = GetExecutingLocation();
                        this._HostProtocol = HostProtocol.file;
                        this._Port = port;
                        this._HostAddress = string.Format("{0}\\{1}\\{2}", server, args[2], args[3]); ;
                        this.ServerName = server;
                        break;
                    case "tcp"://tcp:127.0.0.1:9015?queuName
                        port = Types.ToInt(args[2]);
                        if (port <= 0)
                        {
                            throw new Exception("Invalid port number for tcp.");
                        }
                        this._HostProtocol = HostProtocol.tcp;
                        this._Port = port;
                        this._HostAddress = string.Format("{0}:{1}", server, args[2]);
                        this.ServerName = args[1];
                        break;
                    case "http"://http://127.0.0.1:9015/queuName
                        port = Types.ToInt(args[2]);
                        if (port <= 0)
                        {
                            port = 80;
                        }
                        this._HostProtocol = HostProtocol.http;
                        this._Port = port;
                        this._HostAddress = string.Format("{0}:{1}", server, args[2]);
                        this.ServerName = server;
                        break;
                    case "db"://db:serve:catalog?queuename
                        this._HostProtocol = HostProtocol.db;
                        this._Port = port;
                        this._HostAddress = args[2];
                        this.ServerName = server;
                        break;
                    default:
                        throw new Exception("Incorrecr address or AddressType not supported");

                }
            }
        }

        #region Dispose

        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        bool disposed = false;
        /// <summary>
        /// Get indicate wether the current instance is Disposed.
        /// </summary>
        protected bool IsDisposed
        {
            get { return disposed; }
        }
        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                HostName = null;
                ServerName = null;
                _RawHostAddress = null;
                _HostAddress = null;
            }
            disposed = true;
        }
        #endregion

        public string HostId
        {
            get
            {
                return string.Format("{0}-{1}-{2}", ServerName, Port, HostName);
                //return string.Format("{0}-{1}-{2}-{3}", ServerName, Port, HostName,QueueName);
            }
        }

        /// <summary>
        /// Cover Mode
        /// </summary>
        public CoverMode CoverMode { get; set; }
        /// <summary>
        /// Commit Mode
        /// </summary>
        public PersistCommitMode CommitMode { get; set; }
        /// <summary>
        /// Reload On Start
        /// </summary>
        public bool ReloadOnStart { get; set; }

        ///// <summary>
        ///// Get or Set QueueName
        ///// </summary>
        //public string QueueName { get; set; }

        /// <summary>
        /// Get or Set HostName
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Get or Set ServerName
        /// </summary>
        public string ServerName { get; private set; }

        public string[] HostNames { get { return HostName==null? new string[] {""}: HostName.SplitTrim (";"); } }


        /// <summary>
        /// Get or Set Endpoint Address
        /// </summary>
        public string Endpoint
        {
            get
            {
                switch (_HostProtocol)
                {
                    case HostProtocol.ipc:
                        return HostAddress;
                    case HostProtocol.tcp:
                        return ServerName;
                    case HostProtocol.http:
                        return ServerName;
                    default:
                        return HostAddress;
                }
            }
        }

        //string _OriginalHostAddress;
        //public string OriginalHostAddress
        //{
        //    get { return _OriginalHostAddress; }
        //}

        string _RawHostAddress;
        public string RawHostAddress
        {
            get { return _RawHostAddress; }
        }

        string _HostAddress;
        public string HostAddress
        {
            get { return _HostAddress; }
        }

        HostProtocol _HostProtocol;
        public HostProtocol Protocol
        {
            get { return _HostProtocol; }
        }



        int _Port;
        public int Port
        {
            get { return _Port; }
        }


        public NetProtocol NetProtocol
        {
            get
            {
                switch (_HostProtocol)
                {
                    case HostProtocol.ipc:
                        return NetProtocol.Pipe;
                    case HostProtocol.tcp:
                        return NetProtocol.Tcp;
                    case HostProtocol.http:
                        return NetProtocol.Http;
                    default:
                        return NetProtocol.NA;
                }
            }
        }

        public static QueueHost Parse(string address)
        {
            QueueHost host = new QueueHost();
            host.Create(address);
            return host;
        }

        public static string EnsureRawAddress(string host)
        {

            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            string[] args = host.Split(':', '/');
            if (args.Length < 4)
            {
                return GetRawAddress(HostProtocol.local, ".", ".", host);
            }

            switch (args[0])
            {
                case "local"://local:././queuName
                    return GetRawAddress(HostProtocol.local, ".", ".", host);
                case "ipc"://ipc:./nistec_enqueue/queuName
                    return GetRawAddress(HostProtocol.ipc, args[1], args[2], args[3]);
                case "file"://file:root/folder/queuName
                    if (args[1] == Assists.EXECPATH)
                        args[1] = GetExecutingLocation();
                    return GetRawAddress(HostProtocol.file, args[1], args[2], args[3]);
                case "tcp"://tcp:127.0.0.1:9015/queuName
                    return GetRawAddress(HostProtocol.tcp, args[1], args[2], args[3]);
                case "http"://http://127.0.0.1:9015/queuName
                    return GetRawAddress(HostProtocol.http, args[1], args[2], args[3]);
                case "db"://db:serve/catalog/queuename
                    return GetRawAddress(HostProtocol.db, args[1], args[2], args[3]);
                default:
                    throw new Exception("Incorrecr address or HostProtocol not supported");

            }
        }

        public static string GetRawAddress(HostProtocol protocol, string serverName, string hostPort, string hostName)
        {
            switch (protocol)
            {

                case HostProtocol.local://local:././queuName
                    return string.Format("local:{0}/{1}/{2}", serverName, hostPort, hostName);
                case HostProtocol.ipc://ipc:.:nistec_enqueue/queuName
                    return string.Format("ipc:{0}/{1}/{2}", serverName, hostPort, hostName);
                case HostProtocol.file://file:root/folder/queuName
                    if (serverName == Assists.EXECPATH)
                        serverName = GetExecutingLocation();
                    return string.Format("file:{0}/{1}/{2}", serverName, hostPort, hostName);
                case HostProtocol.tcp://tcp:127.0.0.1:9015/queuName
                    return string.Format("tcp:{0}:{1}/{2}", serverName, hostPort, hostName);
                case HostProtocol.http://http://127.0.0.1:9015/queuName
                    return string.Format("http://{0}:{1}/{2}", serverName, hostPort, hostName);
                case HostProtocol.db://db:serve/catalog/queuename
                    return string.Format("db:{0}/{1}/{2}", serverName, hostPort, hostName);
                default:
                    throw new Exception("Incorrecr address or HostProtocol not supported");
            }
        }

        //public bool IsPingOk { get; private set; }
        public bool PingValidate()
        {
            try
            {
                switch (_HostProtocol)
                {
                    case HostProtocol.ipc:
                        return Nistec.Channels.PipeClient.Ping(ServerName, HostName, 5000);
                    case HostProtocol.tcp:
                        return Nistec.Channels.Tcp.TcpClient.Ping(ServerName, Port, 5000);
                    case HostProtocol.http:
                        return Nistec.Channels.Http.HttpClient.Ping(HostAddress, Port, 5000);
                    default:
                        return false;// NetProtocol.NA;
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine("PingValidate error: " + ex.Message);
            }
            return false;
        }

        //public static QueueHost Get(HostProtocol protocol, string serverName, string hostPort, string hostName)
        //{
        //    string address=GetRawAddress(protocol, serverName, hostPort, hostName);

        //    return new QueueHost() { _HostAddress = address, _HostProtocol = protocol, _Port = hostPort, HostName = hostName };
        //}

        //public static void ParseHostAddress(string address)
        //{
        //    //local:
        //    string hostName = null;
        //    string serverName = ".";
        //    string hostAddress = null; HostProtocol addressType = HostProtocol.local; int port = 0;
        //    ParseHostAddress(address, ref hostAddress, ref addressType, ref port);
        //    QueueHost host = new QueueHost()
        //    {
        //        HostName = hostName,
        //        ServerName = serverName,
        //        _RawHostAddress = hostAddress,
        //        _HostProtocol = addressType
        //    };

        //    // , ref string hostAddress, ref HostProtocol addressType, ref int port
        //}

        //public static void ParseHostAddress(string address,ref string hostAddress, ref HostProtocol addressType, ref int port)
        //{
        //    if (string.IsNullOrEmpty(address))
        //    {
        //        addressType = HostProtocol.local;
        //        hostAddress = address;
        //        port = 0;
        //        return;
        //    }

        //    string[] args = address.Split(':');
        //    if (args.Length < 2)
        //    {
        //        addressType = HostProtocol.local;
        //        hostAddress = address;
        //        port = 0;
        //        return;
        //    }
        //    else
        //    {
        //        if (args.Length > 2)
        //        {
        //            port = Types.ToInt(args[2].Trim());
        //        }
        //        else
        //        {
        //            port = 0;
        //        }
        //        string adrsType = args[0];// address.Split(':')[0];
        //        switch (adrsType)
        //        {
        //            case "ipc"://ipc:./nistec_enqueue/queuName
        //                addressType = HostProtocol.ipc;
        //                hostAddress = address.Substring(4);
        //                break;
        //            case "file"://file:./
        //                addressType = HostProtocol.file;
        //                hostAddress = address.Substring(5);
        //                break;
        //            case "tcp"://tcp:127.0.0.1:9015/queuName
        //                if (port <= 0)
        //                {
        //                    throw new Exception("Invalid port number for tcp.");
        //                }
        //                addressType = HostProtocol.tcp;
        //                hostAddress = args[1];
        //                break;
        //            case "http"://http://127.0.0.1:9015/queuName
        //                if (port <= 0)
        //                {
        //                    port = 80;
        //                }
        //                addressType = HostProtocol.http;
        //                hostAddress = address;
        //                break;
        //            case "db"://db:serve:0/catalog/queuename
        //                addressType = HostProtocol.db;
        //                hostAddress = address;
        //                break;
        //            default:
        //                throw new Exception("Incorrecr address or AddressType not supported");
        //        }
        //    }
        //}


        //public void AddHostAddress(HostProtocol addressType, string address)
        //{
        //    _RawHostAddress = string.Format("{0}:{1}", addressType.ToString(), address);
        //}

        //public static HostProtocol ParseProtocol(string address)
        //{
        //    if (string.IsNullOrEmpty(address))
        //    {
        //        return HostProtocol.local;
        //    }
        //    string adrsType = address.Split(':')[0];
        //    switch (adrsType)
        //    {
        //        case "local":
        //            return HostProtocol.local;
        //        case "ipc":
        //            return HostProtocol.ipc;
        //        case "file":
        //            return HostProtocol.file;
        //        case "tcp":
        //            return HostProtocol.tcp;
        //        case "http":
        //            return HostProtocol.http;
        //        case "db":
        //            return HostProtocol.db;
        //        default:
        //            throw new Exception("Incorrecr address or AddressType not supported");
        //    }
        //}


        //public static string ValidateHostAddress(string address, ref HostProtocol addressType, ref int port)
        //{
        //    if (string.IsNullOrEmpty(address))
        //    {
        //        addressType = HostProtocol.local;
        //        return null;
        //    }

        //    string[] args = address.Split(':');
        //    if (args.Length < 2)
        //    {
        //        return address;
        //    }
        //    else
        //    {
        //        if (args.Length > 2)
        //        {
        //            port = Types.ToInt(args[2].Trim());
        //        }
        //        else
        //        {
        //            port = 0;
        //        }
        //        string adrsType = args[0];// address.Split(':')[0];
        //        switch (adrsType)
        //        {
        //            case "ipc":
        //                addressType = HostProtocol.ipc;
        //                return address.Substring(4);
        //            case "file":
        //                addressType = HostProtocol.file;
        //                return address.Substring(5);
        //            case "tcp":
        //                if (port <= 0)
        //                {
        //                    throw new Exception("Invalid port number for tcp.");
        //                }
        //                addressType = HostProtocol.tcp;
        //                return args[1];
        //            case "http":
        //                if (port <= 0)
        //                {
        //                    port = 80;
        //                }
        //                addressType = HostProtocol.http;
        //                return address;
        //            case "db":
        //                addressType = HostProtocol.db;
        //                return address;
        //            default:
        //                throw new Exception("Incorrecr address or AddressType not supported");
        //        }
        //    }
        //}

        //public string OriginalHostAddress()
        //{
        //        if (string.IsNullOrEmpty(HostAddress))
        //            return null;
        //        switch (HostAddressType)
        //        {
        //            case HostProtocol.ipc:
        //                return HostAddress.Substring(4);
        //            case HostProtocol.file:
        //                return HostAddress.Substring(5);
        //            case HostProtocol.tcp:
        //                return HostAddress.Substring(4);
        //            case HostProtocol.http:
        //                return HostAddress;
        //            case HostProtocol.db:
        //                return HostAddress.Substring(3);
        //            default:
        //                return null;
        //        }
        //}

        //public HostProtocol HostAddressType
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(HostAddress))
        //            return HostProtocol.ipc;
        //        string addressType= HostAddress.Split(':')[0];
        //        switch (addressType)
        //        {
        //            case "ipc":
        //                return HostProtocol.ipc;
        //            case "file":
        //                return HostProtocol.file;
        //            case "tcp":
        //                return HostProtocol.tcp;
        //            case "http":
        //                return HostProtocol.http;
        //            case "db":
        //                return HostProtocol.db;
        //            default:
        //                return HostProtocol.ipc;
        //        }

        //    }
        //}

        /// <summary>
        /// Get indicate wether this host can distrebute.
        /// </summary>
        public bool CanDistrebute
        {
            get { return !string.IsNullOrEmpty(RawHostAddress) && RawHostAddress.StartsWith("tcp:"); }
        }

        /// <summary>
        /// Get indicate wether this host is local.
        /// </summary>
        public bool IsLocal
        {
            get { return Types.NZ(ServerName, ".") == "."; }
        }

        #region  ISerialEntity

        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteString(HostName);
            streamer.WriteString(ServerName);
            streamer.WriteString(RawHostAddress);
            streamer.WriteString(HostAddress);
            streamer.WriteValue((byte)Protocol);
            streamer.Flush();
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            HostName = streamer.ReadString();
            ServerName = streamer.ReadString();
            _RawHostAddress = streamer.ReadString();
            _HostAddress = streamer.ReadString();
            _HostProtocol = (HostProtocol)streamer.ReadValue<byte>();
        }

        #endregion

        #region assists

        public static string GetExecutingLocation()
        {
            return SysNet.GetExecutingAssemblyPath();
        }

        //public string GetQueueSectionPath(string section)
        //{
        //    return Assists.GetQueueSectionPath(HostAddress, HostName, section);
        //}

        //public string EnsureQueueSectionPath(string queueName, string section)
        //{
        //    return Assists.EnsureQueueSectionPath(HostAddress, HostName, section);
        //}

        public void EnsureHost()
        {
            if (HostName == null || HostName.Length == 0)
            {
                throw new Exception("QueueHost HostName");
            }
            if (HostAddress == null || HostAddress.Length == 0)
            {
                throw new Exception("QueueHost OriginalHostAddress");
            }
            //Assists.GetQueuePath(OriginalHostAddress, HostName);
        }

        //public string GetFullFilename(string identifier) 
        //{
        //    EnsureHost();
        //    return Path.Combine(QueuePath, string.Format("{0}{1}", identifier, Assists.FileExt));
        //}

        public string RootPath { get { return HostAddress; } }
        //public string QueuePath { get { return Assists.GetQueuePath(HostAddress, Assists.FolderQueue); } }

        //public string QueueInfoPath { get { return Assists.GetQueuePath(HostAddress, Assists.FolderInfo); } }// "Info\\" + HostName); } }
        //public string SuspendPath { get { return GetQueueSectionPath(Assists.FolderSuspend); } }
        //public string CoveredPath { get { return GetQueueSectionPath(Assists.FolderCovered); } }

        #endregion
    }
    */

}
