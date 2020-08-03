using Nistec.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging
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

        public void AddSubscriber(TopicSubscriber subscriber) {
            Subscribers[subscriber.HostName] = subscriber;
        }
        public void RemoveSubscriber(TopicSubscriber subscriber)
        {
            Subscribers[subscriber.HostName] = subscriber;
        }
    }
    public class TopicSubscriber
    {
        public string TopicId { get; set; }
        public string HostName { get; set; }
        public string Host { get; set; }
        public NetProtocol Protocol { get; set; }
        public QueueHost QHost { get; set; }

        public static TopicSubscriber Create(string s, string topicId)
        {

            var qh = QueueHost.Parse(s);
           return new TopicSubscriber()
           {
               Host = qh.HostAddress,
               Protocol = qh.NetProtocol,
               HostName = qh.HostName,
               TopicId = topicId,
               QHost = qh
           };

        }
    }
}