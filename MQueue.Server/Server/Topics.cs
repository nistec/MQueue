using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Topic
{
    public class Topics
    {
        ConcurrentDictionary<string, TopicPublisher> TopicPublishers;
        public Topics()
        {
            TopicPublishers = new ConcurrentDictionary<string, TopicPublisher>();
        }

        #region Publisher methods

        public bool TryAddPublisher(string topicId, TopicPublisher item)
        {
            return TopicPublishers.TryAdd(topicId, item);
        }

        public bool TryGetPublisher(string topicId, out TopicPublisher item)
        {
            return TopicPublishers.TryGetValue(topicId, out item);
        }

        public void ClearPublisherItems()
        {
            TopicPublishers.Clear();
        }

        public int PublisherCount()
        {
            return TopicPublishers.Count;
        }


        #endregion

        #region Subscriber methods

        public TopicSubscriber[] GetSubscribers(string topicId)
        {
            TopicPublisher topic;
            if (TryGetPublisher(topicId, out topic))
            {
                return topic.Subscribers.Values.ToArray();
            }
            return null;
        }

        public bool TryAddSubscriber(string topicId, TopicSubscriber item)
        {
            TopicPublisher topic;
            if(TryGetPublisher(topicId, out topic))
            {
                topic.Subscribers[item.HostName] = item;
                return true;
            }
            return false;
        }

        public bool TryGetSubscriber(string topicId, string subscriber, out TopicSubscriber item)
        {
            TopicPublisher topic;
            if (TryGetPublisher(topicId, out topic))
            {
                if(topic.Subscribers.TryGetValue(subscriber, out item))
                {
                    return true;
                }
                
            }
            item = null;
            return false;
        }

        public void ClearSubscriberItems(string topicId)
        {
            TopicPublisher topic;
            if (TryGetPublisher(topicId, out topic))
            {
               topic.Subscribers.Clear();
            }
        }

        public int SubscriberCount(string topicId)
        {
            TopicPublisher topic;
            if (TryGetPublisher(topicId, out topic))
            {
                return topic.Subscribers.Count;
            }
            return 0;
        }


        #endregion

    }
}
