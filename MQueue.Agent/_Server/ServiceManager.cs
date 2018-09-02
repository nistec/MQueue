using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.Runtime.Remoting;
using MControl.Queue.Service;
using MControl.Messaging;

namespace MControl.Queue.Service
{

    public class ServiceManager
    {
        private readonly RemoteServer svr = new RemoteServer();

        public ServiceManager()
        {
        }

        public void Start()
        {
            Thread Th = new Thread(new ThreadStart(InternalStart));
            Th.Start();
        }

        public void Stop()
        {
            svr.Stop();
            if (ServiceConfig.EnableMailer)
            {
                MailerStop();
            }
        }

        private void InternalStart()
        {
            ServiceConfig.LoadConfig();
            svr.Start();

            if(ServiceConfig.EnableMailer)
            {
                MailerStart();
            }
        }

        #region queue service

        Queue_Manager queue_manager;
        ServiceHost host_queue;

        private void host_queueStart()
        {
            //ManagerConfig config = Config.CreateManagerConfig();// new ManagerConfig();
            //manager.Start(McLock.Lock.ValidateLock(),true);

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(1033);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(1033);

            //using (ServiceHost host = new ServiceHost(typeof(QueueService)))
            //{

            host_queue = new ServiceHost(typeof(QueueService));
            host_queue.Open();

            //RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, false);

            Console.WriteLine("QueueService is available.");

            //Console.ReadKey();
            //}

        }

        void QueueStart()
        {
            try
            {
                //Log.Debug("MailerManager start...");
                //ManagerConfig config = Config.CreateManagerConfig();// new ManagerConfig();
                queue_manager = new  Queue_Manager();
                queue_manager.Start();//McLock.Lock.ValidateLock(), true);
                host_queueStart();
                //Log.Debug("MailerManager started!");
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("QueueService error:{0},Trace:{1}", ex.Message, ex.StackTrace);
            }
        }

        void QueueStop()
        {
            //Log.Debug("MailerManager stop...");
            if (manager != null)
            {
                manager.Stop();
            }
            if (host_queue != null)
            {
                host_queue.Close();
            }
            Netlog.Debug("QueueService stoped");
        }
        #endregion

        #region mailer

        Mailer_Manager manager;
        ServiceHost host_mailer;

        private void host_mailerStart()
        {
            //ManagerConfig config = Config.CreateManagerConfig();// new ManagerConfig();
            //manager.Start(McLock.Lock.ValidateLock(),true);

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(1033);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(1033);

            //using (ServiceHost host = new ServiceHost(typeof(QueueService)))
            //{

            host_mailer = new ServiceHost(typeof(MailerService));
            host_mailer.Open();

            //RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, false);

            Console.WriteLine("MailerService is available.");

            //Console.ReadKey();
            //}

        }

        void MailerStart()
        {
            try
            {
                //Log.Debug("MailerManager start...");
                //ManagerConfig config = Config.CreateManagerConfig();// new ManagerConfig();
                manager = new Mailer_Manager();
                manager.Start();//McLock.Lock.ValidateLock(), true);
                host_mailerStart();
                //Log.Debug("MailerManager started!");
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("MailerService error:{0},Trace:{1}", ex.Message, ex.StackTrace);
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
            Netlog.Debug("MailerService stoped");
        }
        #endregion
    }

    
}
