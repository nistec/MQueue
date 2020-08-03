using Nistec.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Topic
{
   
    public class TopicPublisher
    {
        public string TopicId { get; set; }
        public string TopicName { get; set; }

        Dictionary<string,TopicSubscriber> _Subscribers;
        public Dictionary<string, TopicSubscriber> Subscribers
        {
            get
            {
                if (_Subscribers == null)
                {
                    _Subscribers = new Dictionary<string, TopicSubscriber>();
                }
                return _Subscribers;
            }
        }

    }
    public class TopicSubscriber
    {
        public string TopicId { get; set; }
        public string Subscriber { get; set; }
        public string Host { get; set; }
        public NetProtocol Protocol { get; set; }


    }
}