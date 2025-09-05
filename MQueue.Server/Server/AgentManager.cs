using Nistec.Messaging.Config;
using Nistec.Messaging.Topic;
using Nistec.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Nistec.Messaging.Server
{

 
    public class AgentManager
    {

        //readonly System.Collections.Concurrent.ConcurrentDictionary<string, int> memWaiter = new System.Collections.Concurrent.ConcurrentDictionary<string, int>();


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

        static GcWatcher gcwatcher;

        public static void Start()//bool enableQueueController, bool enableTopicController)
        {
            Settings.Load();
            Queue.LoadQueueConfig(Settings.EnableJournalQueue);

            int gcinterval = Settings.GcWatcherInterval;
            gcwatcher = new GcWatcher(gcinterval);
            gcwatcher.Start();
        }

        public static void Stop()
        {
            gcwatcher.Stop();
            //Interlocked.Exchange(ref gc_listen,0);
        }
    }

    public class GcWatcher:ThreadTimer
    {
        int LastCount = 0;
        int counter = 0;
        public GcWatcher(int interval) : base(interval)
        {

        }

        protected override void OnElapsed(ElapsedEventArgs e)
        {
            base.OnElapsed(e);
            counter++;
            try
            {
                int count=AgentManager.Queue.QueueAllCount();

                if (count == 0 || (count == LastCount && counter > 5))
                {
                    counter = 0;
                    int memory = (int)AgentManager.Queue.CmdMemoryFree() / 1024;
                    AgentManager.Queue.Logger.Log(Logging.LoggerLevel.Info, $"Memory: {memory} kb");


                }
                LastCount = count;
            }
            catch (Exception ex)
            {
                AgentManager.Queue.Logger.Log(Logging.LoggerLevel.Error, $"AgentManager.RunGcWatcher error : {ex.Message}");
            }
        }
    }
}
