using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MControl.Messaging.Mail;
using MControl.Messaging;

namespace MControl.Queue.Service
{
   
    public class Mailer_Manager
    {

        private bool keepAlive = false;
        private Thread threadManager;
        private int intervalManager = 60000;
        private Dictionary<string, Mail_Distrebuter> m_disterbuters;
        private MailHosts m_channels;

        public Mailer_Manager()
        {
            m_channels = MailHosts.GetChannels();
            m_disterbuters = new Dictionary<string, Mail_Distrebuter>();
            intervalManager = MailConfig.IntervalManager;
        }

        public void Start()
        {
            if (keepAlive)
                return;

            //Netcell.Log.Debug("Start Mailer_Manager");


            foreach (KeyValuePair<string, MailHost> channel in m_channels.Items)
            {
                channel.Value.ValidateDirectory(MailConfig.MailQueuePath);
                Mail_Distrebuter mailer = new Mail_Distrebuter(channel.Value);
                m_disterbuters[channel.Key] = mailer;
            }

            foreach (string key in m_disterbuters.Keys)
            {
                m_disterbuters[key].Start();
                Thread.Sleep(100);
            }

            keepAlive = true;

            threadManager = new Thread(new ThreadStart(MailerProcess));
            threadManager.Start();


            Netlog.Debug("Mailer_Manager in process");

        }


        private void MailerProcess()
        {
            while (keepAlive)
            {
                Thread.Sleep(intervalManager);
            }
            Netlog.WarnFormat("Mailer_Manager not keep Alive");
        }


        public void Stop()
        {
            //Netcell.Log.Debug("Stop Mailer_Manager");
            try
            {

                keepAlive = false;

                if (m_disterbuters != null)
                {
                    foreach (string key in m_disterbuters.Keys)
                    {
                        m_disterbuters[key].Stop();
                        Thread.Sleep(10);
                    }
                }

                if (threadManager != null)
                {
                    threadManager.Abort(); ;
                    threadManager = null;
                }
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("Stop Mailer_Manager:{0}", ex.Message);
            }
        }

    }

}
