using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Net;
using System.Net;

namespace Nistec.Messaging.Tcp
{
    public class TcpSettings
    {

        public TcpSettings()
        {

        }

        public TcpSettings(QueueHost host)
        {
            LoadTcpSettings(host);
        }

        public TcpSettings(QueueHost[] hosts)
        {
            foreach (var host in hosts)
            {
                LoadTcpSettings(host);
            }
        }

        public void LoadTcpSettings(QueueHost host)
        {
            IPBindInfo bindinfo = new IPBindInfo(host.HostAddress, NetworkProtocol.Tcp, IPAddress.Any, host.Port);
            MaxConnections = 0;
            MaxConnectionsPerIP = 0;
            TaskIdleTimeout = 0;

        }
        /// <summary>
        /// Gets or sets Tcp server IP bindings.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public IPBindInfo[] Bindings { get; set; }
        /// <summary>
        /// Gets local listening IP end points.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public IPEndPoint[] LocalEndPoints { get; set; }
        /// <summary>
        /// Gets or sets maximum allowed concurent connections. Value 0 means unlimited.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when negative value is passed.</exception>
        public long MaxConnections { get; set; }
        /// <summary>
        /// Gets or sets maximum allowed connections for 1 IP address. Value 0 means unlimited.
        /// </summary>
        public long MaxConnectionsPerIP { get; set; }
        /// <summary>
        /// Gets or sets maximum allowed task idle time in seconds, after what task will be terminated. Value 0 means unlimited,
        /// but this is strongly not recommened.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentException">Is raised when negative value is passed.</exception>
        public int TaskIdleTimeout { get; set; }

    }
}
