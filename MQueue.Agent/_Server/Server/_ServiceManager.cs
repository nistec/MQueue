using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.Runtime.Remoting;
using System.ServiceProcess;
using System.IO;
using Nistec.Generic;
using Nistec.Messaging.Server;
using Nistec.Messaging.Proxies;


namespace Nistec.Queue.Service
{
    public class ServiceManager
    {
        private const string serviceName = "MQueue agent";

        private ServerEnqueue m_ServerEnqueue;
        private ServerDequeue m_ServerDequeue;
        private ServerQueueManager m_ServerQueueManager;
      

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
                Thread Th = new Thread(new ThreadStart(InternalStart));
                Th.Start();
            }

            private void InternalStart()
            {
                try
                {
                    Netlog.Debug(serviceName+" start...");

                    m_ServerEnqueue = new ServerEnqueue();
                    m_ServerEnqueue.Start(false);

                    m_ServerDequeue = new ServerDequeue();
                    m_ServerDequeue.Start(false);

                    AgentManager.Start();

                    if (QueueSettings.EnableQueueManager)
                    {
                        m_ServerQueueManager = new ServerQueueManager();
                        m_ServerQueueManager.Start(false);
                    }
                    if (QueueSettings.EnableMailerQueue)
                    {
                        MailerStart();
                    }
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

            #region mailer

            MailerManager manager;
            ServiceHost host_mailer;

            private void host_mailerStart()
            {
                //ManagerConfig config = Config.CreateManagerConfig();// new ManagerConfig();
                //manager.Start(McLock.Lock.ValidateLock(),true);

                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(1033);
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(1033);

                //using (ServiceHost host = new ServiceHost(typeof(QueueService)))
                //{

                host_mailer = new ServiceHost(typeof(QueueProxy));
                host_mailer.Open();

                //RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, false);

                Console.WriteLine("QueueProxy is available.");

                //Console.ReadKey();
                //}

            }

            void MailerStart()
            {
                try
                {
                    //Log.Debug("MailerManager start...");
                    //ManagerConfig config = Config.CreateManagerConfig();// new ManagerConfig();
                    manager = new MailerManager();
                    manager.Start();//McLock.Lock.ValidateLock(), true);
                    host_mailerStart();
                    //Log.Debug("MailerManager started!");
                }
                catch (Exception ex)
                {
                    Netlog.ErrorFormat("QueueProxy error:{0},Trace:{1}", ex.Message, ex.StackTrace);
                }
            }

            void MailerStop()
            {
                //Log.Debug("MailerManager stop...");
                if (manager != null)
                {
                    manager.Stop();
                }
                if (host_mailer != null)
                {
                    host_mailer.Close();
                }
                Netlog.Debug("QueueProxy stoped");
            }
            #endregion

            /*
            ServiceHost host_service;
            private void host_serviceStart()
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(1033);
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(1033);

                host_service = new ServiceHost(typeof(MobileDeviceService));
                host_service.Open();

                RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, false);

                Console.WriteLine("MobileService is available.");

            }
          */
            public void Stop()
            {
                Netlog.Debug(serviceName + " stop...");
             
                if (m_ServerDequeue != null)
                    m_ServerDequeue.Stop();
                if (m_ServerEnqueue != null)
                    m_ServerEnqueue.Stop();
                if (m_ServerQueueManager != null)
                    m_ServerQueueManager.Stop();

                MailerStop();

                //if (svr != null)
                //{
                //    svr.Stop();
                //}
                //if (host_service != null)
                //{
                //    host_service.Close();
                //}

                Netlog.Debug(serviceName + " stoped.");
            }
     
    }
}
