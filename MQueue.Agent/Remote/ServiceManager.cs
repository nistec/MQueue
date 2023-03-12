using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading;
//using System.ServiceModel;
//using System.Runtime.Remoting;
//using System.IO;
//using Nistec.Generic;
using Nistec.Messaging.Server;
//using Nistec.Messaging.Proxies;
using System.Security;
//using System.Security.Permissions;
using Nistec.Logging;
using Nistec.Messaging.Config;
using Nistec.Messaging;

namespace Nistec.Services
{
    public class ServiceManager
    {
        private const string serviceName = "MQueue agent";

        private PipeServerChannel m_PipeServerConsumer;
        private TcpServerChannel m_TcpServerConsumer;
        private HttpServerChannel m_HttpServerConsumer;
        //Channel
        private PipeServerChannel m_PipeServerProducer;
        private TcpServerChannel m_TcpServerProducer;
        private HttpServerChannel m_HttpServerProducer;
        
        //private PipeServerDequeue m_ServerDequeue;
        private PipeServerChannel m_PipeServerQueueManager;
        private TcpServerChannel m_TcpServerQueueManager;

        bool m_enableQueueController;
        bool m_enableTopicController;


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

                //m_enableQueueController = AgentManager.Settings.EnableQueueController;
                //m_enableTopicController = AgentManager.Settings.EnableTopicController;

                AgentManager.Start();// m_enableQueueController, m_enableTopicController);





                if (AgentManager.Settings.EnablePipeProducer)
                {


                    m_PipeServerProducer = new PipeServerChannel(QueueChannel.Producer, QueueSettings.DefaultQueueProducer);
                    m_PipeServerProducer.Start();
                    //QLogger.Info("PipeServerChannel started...");
                }


                if (AgentManager.Settings.EnablePipeConsumer)
                {
                    m_PipeServerConsumer = new PipeServerChannel(QueueChannel.Consumer, QueueSettings.DefaultQueueConsumer);
                    m_PipeServerConsumer.Start();
                    //QLogger.Info("PipeServerListener started...");
                }


                //m_ServerDequeue = new PipeServerDequeue();
                //m_ServerDequeue.Start(false);


                if (AgentManager.Settings.EnableTcpProducer)
                {
                    m_TcpServerProducer = new TcpServerChannel(QueueChannel.Producer, QueueSettings.DefaultQueueProducer);
                    m_TcpServerProducer.Start();
                    //QLogger.Info("TcpServerChannel started...");
                }
                if (AgentManager.Settings.EnableTcpConsumer)
                {
                    m_TcpServerConsumer = new TcpServerChannel(QueueChannel.Consumer, QueueSettings.DefaultQueueConsumer);
                    m_TcpServerConsumer.Start();
                    //QLogger.Info("TcpServerListener started...");
                }
                if (AgentManager.Settings.EnableHttpProducer)
                {
                    m_HttpServerProducer = new HttpServerChannel(QueueChannel.Producer, QueueSettings.DefaultQueueProducer);
                    m_HttpServerProducer.Start();
                    //QLogger.Info("HttpServerChannel started...");
                }
                if (AgentManager.Settings.EnableHttpConsumer)
                {
                    m_HttpServerConsumer = new HttpServerChannel(QueueChannel.Consumer, QueueSettings.DefaultQueueConsumer);
                    m_HttpServerConsumer.Start();
                    //QLogger.Info("HttpServerListener started...");
                }

                if (AgentManager.Settings.EnablePipeQueueManager)
                {
                    m_PipeServerQueueManager = new PipeServerChannel(QueueChannel.Manager, QueueSettings.DefaultQueueManager);
                    m_PipeServerQueueManager.IsAsync = false;
                    m_PipeServerQueueManager.Start();// (false);
                    //QLogger.Info("ServerQueueManager started...");
                }
                if (AgentManager.Settings.EnableTcpQueueManager)
                {
                    m_TcpServerQueueManager = new TcpServerChannel(QueueChannel.Manager, QueueSettings.DefaultQueueManager);
                    //m_TcpServerQueueManager.IsAsync = false;
                    m_TcpServerQueueManager.Start();// (false);
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

            try
            {
                //if (m_ServerDequeue != null)
                //    m_ServerDequeue.Stop();
                if (m_PipeServerProducer != null)
                    m_PipeServerProducer.Stop();
                if (m_TcpServerProducer != null)
                    m_TcpServerProducer.Stop();
                if (m_HttpServerProducer != null)
                    m_HttpServerProducer.Stop();

                if (m_PipeServerConsumer != null)
                    m_PipeServerConsumer.Stop();
                if (m_TcpServerConsumer != null)
                    m_TcpServerConsumer.Stop();
                if (m_HttpServerConsumer != null)
                    m_HttpServerConsumer.Stop();

                if (m_PipeServerQueueManager != null)
                    m_PipeServerQueueManager.Stop();

                if (m_TcpServerQueueManager != null)
                    m_TcpServerQueueManager.Stop();
                //if (m_FolderServer != null)
                //    m_FolderServer.Stop(true);
                //if (m_DbServer != null)
                //    m_DbServer.Stop(true);

                Netlog.Debug(serviceName + " stoped.");
            }
            catch(Exception ex)
            {
                Netlog.Debug(serviceName + " stop error: " + ex.Message);
            }
        }

    }
}
