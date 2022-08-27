using Nistec.Messaging.Config;
using Nistec.Messaging.Topic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Nistec.Messaging.Server
{

 
    public class AgentManager
    {

        readonly System.Collections.Concurrent.ConcurrentDictionary<string, int> memWaiter = new System.Collections.Concurrent.ConcurrentDictionary<string, int>();


        public static void StartController()
        {
            Queue.Start();
            //if (Settings.EnableQueueController && Queue.IsStarted==false)
            //    Queue.Start();
            //if (Settings.EnableTopicController && Topic.IsStarted == false)
            //    Topic.Start();
        }


        public static void StopController()
        {
            Queue.Stop();
            //if (_Queues!= null)
            //    Queue.Stop();
            //if (_Topic!=null)
            //    Topic.Stop();
        }


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

        //static TopicController _Topic;

        //public static TopicController Topic
        //{
        //    get
        //    {
        //        if (_Topic == null)
        //        {
        //            _Topic = new TopicController();
        //        }
        //        return _Topic;
        //    }
        //}


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

        public static void Start()//bool enableQueueController, bool enableTopicController)
        {
            Settings.Load();
            Queue.LoadQueueConfig(Settings.EnableJournalQueue);

            //if (enableQueueController)
            //    Queue.LoadQueueConfig();
            //if (enableTopicController)
            //    Topic.LoadTopicConfig();
        }

        public static void Stop()
        {

        }

    }
}
