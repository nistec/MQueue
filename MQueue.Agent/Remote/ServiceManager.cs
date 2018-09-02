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
using System.Security;
using System.Security.Permissions;
using Nistec.Messaging.Server.Pipe;
using Nistec.Messaging.Server.Tcp;
using Nistec.Logging;
using Nistec.Messaging.Server.Http;
using Nistec.Messaging.Config;
using Nistec.Messaging;

namespace Nistec.Services
{
    public class ServiceManager
    {
        private const string serviceName = "MQueue agent";

        private PipeServerChannel m_PipeServerListener;
        private TcpServerChannel m_TcpServerListener;
        private HttpServerChannel m_HttpServerListener;
        //Channel
        private PipeServerChannel m_PipeServerChannel;
        private TcpServerChannel m_TcpServerChannel;
        private HttpServerChannel m_HttpServerChannel;
        
        //private PipeServerDequeue m_ServerDequeue;
        private PipeManagerServer m_ServerQueueManager;

       
        //private FolderServerListener m_FolderServer;
        //private DbServerListener m_DbServer;
        
        bool _loaded = false;

        public bool Loaded
        {
            get { return _loaded; }
        }

        public ServiceManager()
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

                if (AgentManager.Settings.EnablePipeChannel)
                {
                    m_PipeServerChannel = new PipeServerChannel(QueueSettings.DefaultQueueChannel,false);
                    m_PipeServerChannel.Start();
                    //QLogger.Info("PipeServerChannel started...");
                }


                if (AgentManager.Settings.EnablePipeListener)
                {
                    m_PipeServerListener = new PipeServerChannel(QueueSettings.DefaultQueueListener, true);
                    m_PipeServerListener.Start();
                    //QLogger.Info("PipeServerListener started...");
                }


                //m_ServerDequeue = new PipeServerDequeue();
                //m_ServerDequeue.Start(false);


                if (AgentManager.Settings.EnableTcpChannel)
                {
                    m_TcpServerChannel = new TcpServerChannel(QueueSettings.DefaultQueueChannel, false);
                    m_TcpServerChannel.Start();
                    //QLogger.Info("TcpServerChannel started...");
                }
                if (AgentManager.Settings.EnableTcpListener)
                {
                    m_TcpServerListener = new TcpServerChannel(QueueSettings.DefaultQueueListener, true);
                    m_TcpServerListener.Start();
                    //QLogger.Info("TcpServerListener started...");
                }
                if (AgentManager.Settings.EnableHttpChannel)
                {
                    m_HttpServerChannel = new HttpServerChannel(QueueSettings.DefaultQueueChannel, false);
                    m_HttpServerChannel.Start();
                    //QLogger.Info("HttpServerChannel started...");
                }
                if (AgentManager.Settings.EnableHttpListener)
                {
                    m_HttpServerListener = new HttpServerChannel(QueueSettings.DefaultQueueListener, true);
                    m_HttpServerListener.Start();
                    //QLogger.Info("HttpServerListener started...");
                }

                if (AgentManager.Settings.EnableQueueManager)
                {
                    m_ServerQueueManager = new PipeManagerServer();
                    m_ServerQueueManager.IsAsync = false;
                    m_ServerQueueManager.Start();// (false);
                    //QLogger.Info("ServerQueueManager started...");
                }

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

            //if (m_ServerDequeue != null)
            //    m_ServerDequeue.Stop();
            if (m_PipeServerChannel != null)
                m_PipeServerChannel.Stop();
            if (m_TcpServerChannel != null)
                m_TcpServerChannel.Stop();
            if (m_HttpServerChannel != null)
                m_HttpServerChannel.Stop();

            if (m_PipeServerListener != null)
                m_PipeServerListener.Stop();
            if (m_TcpServerListener != null)
                m_TcpServerListener.Stop();
            if (m_HttpServerListener != null)
                m_HttpServerListener.Stop();

            if (m_ServerQueueManager != null)
                m_ServerQueueManager.Stop();


            //if (m_FolderServer != null)
            //    m_FolderServer.Stop(true);
            //if (m_DbServer != null)
            //    m_DbServer.Stop(true);

            Netlog.Debug(serviceName + " stoped.");
        }

    }
}
