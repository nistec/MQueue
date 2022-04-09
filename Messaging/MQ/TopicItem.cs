using Nistec.Channels;
using Nistec.Data.Entities;
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
        public void RemoveSubscriber(string HostName)
        {
            Subscribers.Remove(HostName);
        }

        public bool TryGetSubscriber(string subscriberName, out TopicSubscriber ts)
        {
            return Subscribers.TryGetValue(subscriberName, out ts);
        }

        public void HoldSubscriber(string subscriberName)
        {
            TopicSubscriber ts;
            if (TryGetSubscriber(subscriberName, out ts))
            {
                ts.IsHold = true;
            }
        }

        public void HoldReleaseSubscriber(string subscriberName)
        {
            TopicSubscriber ts;
            if (TryGetSubscriber(subscriberName, out ts))
            {
                ts.IsHold = false;
            }
        }

    }
    public class TopicSubscriber
    {
        public bool IsHold { get; set; }
        public string TopicId { get; set; }
        public string HostName { get; set; }
        public string Host { get; set; }
        public NetProtocol Protocol { get; set; }
        public QueueHost QHost { get; set; }

        public static TopicSubscriber Parse(string commaPipe)
        {
            var ts= EntityExtension.ToEntity<TopicSubscriber>(commaPipe);
            return ts;
        }
        public static TopicSubscriber Create(string qhosts, string topicId)
        {

            var qh = QueueHost.Parse(qhosts);
           return new TopicSubscriber()
           {
               IsHold=false,
               Host = qh.HostAddress,
               Protocol = qh.NetProtocol,
               HostName = qh.HostName,
               TopicId = topicId,
               QHost = qh
           };

        }
    }
}