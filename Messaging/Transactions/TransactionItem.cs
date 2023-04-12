using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Nistec.Threading;
using System.Collections.Concurrent;
using System.IO;
using Nistec.Runtime;
using Nistec.Runtime.Advanced;
using Nistec.Serialization;

namespace Nistec.Messaging.Transactions
{
  
    [Serializable]
    public class TransactionItem : ISerialEntity, ISyncItem
    {

        #region properties
        public object Item { get; private set; }
        /// <summary>
        /// Get ItemId
        /// </summary>
        public string Identifier { get; private set; }
        /// <summary>
        /// Get or Set the item location.
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Get Retry
        /// </summary>
        public int Retry { get; internal set; }

        public TimeSpan Timeout { get; private set; }
        public DateTime ArrivedTime { get; private set; }
        public TransItemState ItemState { get; private set; }

        internal bool HasTimeout
        {
            get { return !(Timeout == TimeSpan.Zero); }
        }

        public DateTime Expiration
        {
            get { return ArrivedTime.Add(Timeout); }
        }
        #endregion

        #region ctor
        public TransactionItem(string identifier, string location, TimeSpan timeout)
        {
            this.Identifier = identifier;
            this.Location = location;
            this.ArrivedTime = DateTime.Now;
            this.Timeout = timeout;
            ItemState = TransItemState.Wait;
            Retry = 0;
        }

        public TransactionItem(IQueueMessage item, string location)
        {
            this.Item = item;
            this.Identifier = item.Identifier;
            this.Location = location;
            this.ArrivedTime = item.ArrivedTime;
            //this.Timeout = TimeSpan.FromSeconds(item.Expiration);
            ItemState = TransItemState.Wait;
            Retry = 0;
        }
        #endregion

        #region ISyncTimer

        public void DoSync()
        {
            switch (ItemState)
            {
                case TransItemState.Abort:

                    break;
                case TransItemState.Commit:

                    break;
                case TransItemState.Timeout:

                    break;
                case TransItemState.Retry:

                    break;
            }
        }

        public void DoSync(TransItemState state)
        {
            ItemState = state;
            DoSync();
        }

        public void SyncExpired()
        {
            DoSync(TransItemState.Timeout);
        }

        #endregion

        #region  ISerialEntity


        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteString(Identifier);//.WriteValue(Identifier);
            streamer.WriteValue((int)ItemState);
            streamer.WriteValue(Retry);
            streamer.WriteValue(ArrivedTime);
            streamer.WriteValue(Timeout);
            streamer.Flush();
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            Identifier = streamer.ReadString();//.ReadValue<Guid>();
            ItemState = (TransItemState)streamer.ReadValue<int>();
            Retry = streamer.ReadValue<int>();
            ArrivedTime = streamer.ReadValue<DateTime>();
            Timeout = TimeSpan.FromSeconds(streamer.ReadValue<int>());

        }

        #endregion

        #region IO

        internal void SaveItemInfo()
        {

        }

        internal void DeleteItemInfo()
        {

        }

        internal void DeleteQueueItem()
        {

        }

        internal void MoveToJournal()
        {

        }

        #endregion
    }

}
