using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MControl.Messaging.Mail;
using MControl.Messaging;

namespace MControl.Queue.Service
{
   
    public class Queue_Manager
    {

        private bool keepAlive = false;
        private Thread threadManager;
        private int intervalManager = 60000;

        public Queue_Manager()
        {
            intervalManager = MailConfig.IntervalManager;
        }

        public void Start()
        {
            if (keepAlive)
                return;

            //Netcell.Log.Debug("Start Mailer_Manager");

            keepAlive = true;

            threadManager = new Thread(new ThreadStart(QueueProcess));
            threadManager.Start();


            Netlog.Debug("Queue_Manager in process");

        }


        private void QueueProcess()
        {
            while (keepAlive)
            {
                Thread.Sleep(intervalManager);
            }
            Netlog.WarnFormat("Queue_Manager not keep Alive");
        }


        public void Stop()
        {
            //Netcell.Log.Debug("Stop Mailer_Manager");
            try
            {

                keepAlive = false;

               
                if (threadManager != null)
                {
                    threadManager.Abort(); ;
                    threadManager = null;
                }
            }
            catch (Exception ex)
            {
                Netlog.ErrorFormat("Stop Queue_Manager:{0}", ex.Message);
            }
        }

    }

}
