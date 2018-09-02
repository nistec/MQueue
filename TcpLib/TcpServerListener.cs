using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Generic;
using System.Collections.ObjectModel;
using Nistec.Messaging.Remote;
using Nistec.Messaging.Adapters;
using Nistec.Messaging.Server;
using System.Xml;
using Nistec.Xml;
using System.Data;
using Nistec.Data;
using Nistec.Messaging.Config;
using Nistec.Net;

namespace Nistec.Messaging.Listeners
{

    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection.
    /// </summary>
    public class TcpServerListener : ListenerServer,IAdapterListener
    {

        #region ctor

        public TcpServerListener()
            : base(HostAddressTypes.tcp)
        {

        }
                
        #endregion

        #region start/stop

        TcpServer m_listener;
        /// <summary>
        /// Start the queue listener.
        /// </summary>
        /// <param name="adapters"></param>
        /// <returns></returns>
        protected override bool StartListener(AdapterProperties[] adapters)
        {
 
            if (Adapters.Count > 0)
            {
                m_listener = new TcpServer();
                m_listener.Load(Adapters.ToArray());
                m_listener.Start();
                return m_listener.IsRunning;
            }
            return false;
        }
        /// <summary>
        /// Stop the queue listener.
        /// </summary>
        /// <returns></returns>
        protected override bool StopListener()
        {
            if (m_listener == null)
                return false;

            m_listener.Stop();

            return m_listener.IsRunning;
        }
       
        #endregion

    }
}
