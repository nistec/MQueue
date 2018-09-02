using Nistec.Messaging.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Nistec.Messaging.Server
{

 
    public class AgentManager
    {

        static QueueController _Queues;// = new MQueueList();

        public static QueueController Queue
        {
            get
            {
                if (_Queues == null)
                {
                    _Queues = new QueueController();
                }
                return _Queues;
            }
        }

        static QueueSettings _Settings;
        public static QueueSettings Settings
        {
            get
            {
                if (_Settings == null)
                {
                    _Settings = new QueueSettings();
                }
                return _Settings;
            }
        }

        public static void Start()
        {
            Settings.Load();
            Queue.LoadQueueConfig();
        }

        public static void Stop()
        {

        }

    }
}
