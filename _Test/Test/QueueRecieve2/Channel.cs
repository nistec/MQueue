using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MControl.Messaging;
using MControl.Win;
using MControl;
using MControl.Generic;

namespace QueueRecieve2
{
    public class Channel : RemoteChannel
    {
        public DateTime start;

        public Channel(McChannelProperties prop)
            : base(prop)
        {
            Netlog.InfoFormat("Start Queue{0} ...", QueueName);

        }

        protected override void OnErrorOcurred(ErrorOcurredEventArgs e)
        {
            base.OnErrorOcurred(e);
            Netlog.InfoFormat("OnErrorOcurred:{0} ", e.Message);
        }

        protected override void OnMessageArraived(QueueItemEventArgs e)
        {
            base.OnMessageArraived(e);
            Netlog.InfoFormat("OnMessageArraived:{0} Arrived:{1}", e.Item.MessageId, e.Item.ArrivedTime);
        }

        protected override void OnMessageReceived(QueueItemEventArgs e)
        {
            base.OnMessageReceived(e);
            IQueueItem item = e.Item;
            if (item == null)
                return;

            if (e.State == ItemState.Dequeue)
            {
                base.CommitTrans(item.ItemId, item.HasAttach);
            }
            Console.WriteLine("Queue{0} Items count: {1}", QueueName, base.Count);
            if (item != null)
            {
                Console.WriteLine("Queue{0} MessageReceived: {1}, Duration:{2}", QueueName, item.ItemId,item.Duration());
           }
            else
            {
                Console.WriteLine("Queue{0} Receive timeout", QueueName);
            }

            Netlog.InfoFormat("OnMessageReceived:{0} Received:{1}", e.Item.MessageId, e.Item.SentTime);

            if (base.Count <= 0)
            {
                TimeSpan ts = DateTime.Now.Subtract(start);
                Console.WriteLine("Time took:{0}", ts.TotalSeconds);
            }
        }

        protected override void OnReceiveCompleted(ReceiveCompletedEventArgs e)
        {
            base.OnReceiveCompleted(e);

            IQueueItem item = e.Item;
            if (item == null)
                return;

            if (e.AsyncResult.IsCompleted)
            {
               base.CommitTrans(e.Item.ItemId, item.HasAttach);
            }
            Console.WriteLine("Queue{0} Items count: {1}", QueueName, base.Count);
            //MControl.Messaging.Netlog.InfoFormat("Queue{0} Items count: {1}", QueueName, base.Count);
            if (item != null)
            {
                Console.WriteLine("Queue{0} ReceiveCompleted: {1} ,thread{2}, Duration:{3}", base.QueueName, e.Item.ItemId, Thread.CurrentThread.Name, e.Item.Duration());
            }
            else
            {
                Console.WriteLine("Queue{0} Receive timeout ,thread{1}", base.QueueName, Thread.CurrentThread.Name);
            }
            if (base.Count <= 0)
            {
                TimeSpan ts = DateTime.Now.Subtract(start);
                Console.WriteLine("Time took:{0}", ts.TotalSeconds);
            }

            base.AfterReceiveCompleted();
        }

    }

    
}
