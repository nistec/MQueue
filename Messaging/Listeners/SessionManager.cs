using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Nistec.Messaging.Listeners
{

 
    public class SessionManager
    {

        static SessionDispatcher _QueueDispatcher;// = new MQueueList();

        public static SessionDispatcher QueueDispatcher
        {
            get
            {
                if (_QueueDispatcher == null)
                {
                    _QueueDispatcher = new SessionDispatcher();
                }
                return _QueueDispatcher;
            }
        }

        static SessionDispatcher _TopicDispatcher;

        public static SessionDispatcher TopicDispatcher
        {
            get
            {
                if (_TopicDispatcher == null)
                {
                    _TopicDispatcher = new SessionDispatcher();
                }
                return _TopicDispatcher;
            }
        }

        // server
        /*
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
            Queue.LoadQueueConfig();
        }

        public static void Stop()
        {

        }
        */
    }
}
