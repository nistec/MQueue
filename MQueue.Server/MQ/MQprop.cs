using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging
{
    public class MQprop
    {
        public MQprop() { }
        internal MQprop(MQueue q)
        {
            this.Collate = q.collate;
            this.Count = q.Count;
            this.Enabled = q.Enabled;
            this.EnqueueHoldItems = q.EnqueueHoldItems;
            this.HoldDequeue = q.HoldDequeue;
            this.HoldEnqueue = q.HoldEnqueue;
            this.Initilaized = q.Initilaized;
            this.IsCoverable = q.IsCoverable;
            this.IsDbQueue = q.IsDbQueue;
            this.IsFileQueue = q.IsFileQueue;
            this.IsTrans = q.IsTrans;
            this.MaxCapacity = q.MaxCapacity;
            this.MaxRetry = q.MaxRetry;
            this.MinCapacity = q.MinCapacity;
            this.Mode = q.Mode;
            this.QueueName = q.QueueName;

            this.RoutHost = q.RoutHost;

        }

        public string Collate { get; set; }
        public int Count { get; set; }
        public bool Enabled { get; set; }
        public bool EnqueueHoldItems { get; set; }
        public bool HoldDequeue { get; set; }
        public bool HoldEnqueue { get; set; }
        public bool Initilaized { get; set; }
        public bool IsCoverable { get; set; }
        public bool IsDbQueue { get; set; }
        public bool IsFileQueue { get; set; }
        public bool IsTrans { get; set; }
        public int MaxCapacity { get; set; }
        public int MaxRetry { get; set; }
        public int MinCapacity { get; set; }
        public CoverMode Mode { get; set; }
        public string QueueName { get; set; }
        public QueueHost RoutHost { get; set; }

    }
}
