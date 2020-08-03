using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.Runtime.Remoting;
using System.IO;
using Nistec.Generic;
using Nistec.Messaging.Server;
using Nistec.Messaging.Proxies;
using Nistec.Messaging.Listeners;
using System.Security;
using System.Security.Permissions;


namespace Nistec.Messaging.Server
{
    public class ServerManager
    {
        private const string serviceName = "MQueue agent";

        private PipeServerEnqueue m_ServerEnqueue;
        private PipeServerDequeue m_ServerDequeue;
        private PipeServerManager m_ServerQueueManager;

        private TcpServerListener m_TcpServer;
        private FolderServerListener m_FolderServer;
        private DbServerListener m_DbServer;
        
        bool _loaded = false;

        public bool Loaded
        {
            get { return _loaded; }
        }

        public ServerManager()
        {
        }

        public void Start()
        {
            if (_loaded)
                return;
            Thread Th = new Thread(new ThreadStart(InternalStart));
            Th.Start();
        }
        [SecuritySafeCritical]
        //[PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private void InternalStart()
        {
            try
            {
                       //System.Security.SecurityRules(RuleSet=System.Security.SecurityRuleSet.Level2)

                //using (FileStream fs = new FileStream(@"D:\Nistec\Services\Logs\qlog.log", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                //{
                //    StreamWriter w = new StreamWriter(fs);     // create a Char writer 
                //    w.BaseStream.Seek(0, SeekOrigin.End);      // set the file pointer to the end
                //    w.Write("log test" + "\r\n");
                //    w.Flush();  // update underlying file
                //}

                Netlog.Debug(serviceName + " start...");

                AgentManager.Start();

                //m_ServerEnqueue = new PipeServerEnqueue();
                //m_ServerEnqueue.Start(false);

                //m_ServerDequeue = new PipeServerDequeue();
                //m_ServerDequeue.Start(false);


                //if (AgentManager.Settings.EnableQueueManager)
                //{
                //    m_ServerQueueManager = new PipeServerManager();
                //    m_ServerQueueManager.Start(false);
                //}
                //if (AgentManager.Settings.EnableTcpListener)
                //{
                    m_TcpServer = new TcpServerListener();
                    if (m_TcpServer.Adapters.Count > 0)
                    {
                        //m_TcpServer.Start();
                    }

                    m_TcpServer.Start();
                //}
                //if (AgentManager.Settings.EnableFolderListener)
                //{
                //    m_FolderServer = new FolderServerListener();
                //    m_FolderServer.Start();
                //}
                //if (AgentManager.Settings.EnableDbListener)
                //{
                //    m_DbServer = new DbServerListener();
                //    m_DbServer.Start();
                //}

                //svr.Start();//McLock.Lock.ValidateLock(), true);
                //host_serviceStart();
                Netlog.Debug(serviceName + " started!");
            }
            catch (Exception ex)
            {
                Netlog.Exception(serviceName + " InternalStart error ", ex, true, true);

                //File.AppendAllText(@"D:\Nistec\Services\MQueue.Agent\error.log", "error: " + ex.Message);
            }
        }


        public void Stop()
        {
            Netlog.Debug(serviceName + " stop...");

            if (m_ServerDequeue != null)
                m_ServerDequeue.Stop();
            if (m_ServerEnqueue != null)
                m_ServerEnqueue.Stop();
            if (m_ServerQueueManager != null)
                m_ServerQueueManager.Stop();
            if (m_TcpServer != null)
                m_TcpServer.Stop(true);
            if (m_FolderServer != null)
                m_FolderServer.Stop(true);
            if (m_DbServer != null)
                m_DbServer.Stop(true);

            Netlog.Debug(serviceName + " stoped.");
        }

    }
}
