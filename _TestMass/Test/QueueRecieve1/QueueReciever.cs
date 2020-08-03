using System;
using System.Collections.Generic;
using System.Text;
using MControl.Messaging;
using MControl;
using MControl.Win;

namespace QueueRecieve1
{
    public class ChannelReciever:RemoteChannel
    {
        public ChannelReciever(McChannelProperties prop)
            : base(prop)
        {

        }

        protected override void OnErrorOcurred(ErrorOcurredEventArgs e)
        {
            base.OnErrorOcurred(e);
        }

        protected override void OnMessageReceived(QueueItemEventArgs e)
        {
            base.OnMessageReceived(e);
            IQueueItem item = e.Item;
            if (e.State == ItemState.Commit)
            {
                base.CommitTrans(e.Item.ItemId, item.HasAttach);
            }
            if (item != null)
            {
                Console.WriteLine("Queue{0} ReceiveCompleted: {1}", QueueName, e.Item.ItemId);
            }
            else
            {
                Console.WriteLine("Queue{0} Receive timeout", QueueName);
            }
        }
        
        protected override void OnReceiveCompleted(ReceiveCompletedEventArgs e)
        {
            base.OnReceiveCompleted(e);
            IQueueItem item = e.Item;
            if (item != null)
            {
                Console.WriteLine("Queue{0} ReceiveCompleted: {1}", QueueName, e.Item.ItemId);
            }
            else
            {
                Console.WriteLine("Queue{0} Receive timeout", QueueName);
            }
        }


    }
}
