using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nistec.Runtime;
using Nistec.Serialization;
using Nistec.IO;
using Nistec.Channels;

namespace Nistec.Messaging
{
    public class QueueAck: ISerialEntity, IQueueAck
    {
        public QueueAck() {
            Creation = DateTime.Now;
        }



        public QueueAck(MessageState state, string identifier, string host)
        {
            this.Identifier = identifier;
            this.Label = null;
            this.MessageState = state;
            this.Creation = DateTime.Now;
            this.Host = host;
        }
        public QueueAck(MessageState state, string result, string identifier, string host)
        {
            this.Identifier = identifier;
            this.Label = result;
            this.MessageState = state;
            this.Creation = DateTime.Now;
            this.Host = host;
        }
        public QueueAck(MessageState state, string destination, Exception ex)
        {
            this.Label = ex.Message;
            this.MessageState = state;
            this.Creation = DateTime.Now;
            this.Host = destination;
        }
        public QueueAck(MessageState state, IQueueItem item)
        {
            this.Identifier = item.Identifier;
            this.Label = item.Label;
            this.MessageState = state;
            this.Creation = DateTime.Now;
            this.Host = item.Host;
        }
        public QueueAck(MessageState state, IQueueMessage item, string label)
        {
            this.Identifier = item.Identifier;
            this.Label = label;
            this.MessageState = state;
            this.Creation = DateTime.Now;
            this.Host = item.Host;
        }
        public QueueAck(IQueueAck[] acks)
        {

            for (int i = 0; i < acks.Count(); i++)
            {
                var ack = acks[i];
                if (i == 0)
                {
                    this.Identifier = ack.Identifier;
                    this.Label = ack.Label;
                    this.MessageState = ack.MessageState;
                    this.Creation = DateTime.Now;
                    this.Host = ack.Host;
                }
                else
                {

                    this.Identifier +="|"+ ack.Identifier;
                    this.Label += "|" + ack.Label;
                    //this.MessageState += "|" + ack.MessageState;
                    this.Host += "|" + ack.Host;
                }
            }

            this.Count = acks.Count();
        }

        public QueueAck(NetStream stream)
        { 
            EntityRead(stream, null);
        }

        //public NetStream ToStream()
        //{
        //    NetStream stream = new NetStream();
        //    EntityWrite(stream, null);
        //    return stream;
        //}
        public TransStream ToTransStream()
        {
            TransStream stream = new TransStream(this);//, TransType.Object);
            return stream;
        }

        #region property

        /// <summary>
        /// Get or Set The ItemId.
        /// </summary>
        public string Identifier { get; internal set; }
        /// <summary>
        /// Get the message state
        /// </summary>
        public MessageState MessageState { get; internal set; }
        /// <summary>
        /// Get or Set the arrived time.
        /// </summary>
        public DateTime ArrivedTime { get; internal set; }
        /// <summary>
        /// Get or Set the send time.
        /// </summary>
        public DateTime Creation { get; internal set; }
        /// <summary>
        /// Get Host
        /// </summary>
        public string Host { get; internal set; }
        /// <summary>
        /// Get or Set Label
        /// </summary>
        public string Label { get; internal set; }
        /// <summary>
        /// Get or Set Duration
        /// </summary>
        public int Duration { get; internal set; }
        /// <summary>
        /// Get or Set Count
        /// </summary>
        public int Count { get; internal set; }
        public string Print()
        {
            return string.Format("Creation:{0}, MessageState:{1}, Label:{2}, Host:{3}, Identifier:{4}, Duration:{5}", Creation, MessageState, Label, Host, Identifier, Duration);

            //StringBuilder sb = new StringBuilder();
            //sb.Append("QueueAck Print:");
            //sb.AppendFormat("\r\n{0}", MessageState);
            //sb.AppendFormat("\r\n{0}", Identifier);
            ////sb.AppendFormat("\r\n{0}", ArrivedTime);
            //sb.AppendFormat("\r\n{0}", SentTime);
            //sb.AppendFormat("\r\n{0}", Destination);
            //sb.AppendFormat("\r\n{0}", Label);
            //sb.AppendFormat("\r\n{0}", Count);
            //return sb.ToString();
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

            streamer.WriteValue((byte)MessageState);
            streamer.WriteString(Identifier);
            streamer.WriteValue(Creation);
            streamer.WriteString(Host);
            streamer.WriteString(Label);
            streamer.WriteValue(Duration);
            streamer.WriteValue(Count);
            streamer.WriteValue(ArrivedTime);
            //string arrived = ArrivedTime == null || ArrivedTime.HasValue == false ? null : ArrivedTime.ToString();
            //streamer.WriteString(arrived);

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
            MessageState = (MessageState)streamer.ReadValue<byte>();
            Identifier = streamer.ReadString();
            Creation = streamer.ReadValue<DateTime>();
            Host = streamer.ReadString();
            Label = streamer.ReadString();
            Duration = streamer.ReadValue<int>();
            Count = streamer.ReadValue<int>();
            //string arrived = streamer.ReadString();
            //ArrivedTime= Types.ToNullableDate(arrived);
            ArrivedTime = streamer.ReadValue<DateTime>();
        }

        #endregion

        internal void SetArrived()
        {
            DateTime arrived = DateTime.Now;
            ArrivedTime = arrived;
            var d = arrived.Subtract(Creation).TotalMilliseconds;
            d = Math.Min(d, int.MaxValue);
            Duration = (int)d;
        }
        //internal void SetReceived()
        //{
        //    DateTime now = DateTime.Now;
        //    MessageState = MessageState.Received;
        //    ArrivedTime = now;
        //    var d = now.Subtract(Creation).TotalMilliseconds;
        //    d = Math.Min(d, int.MaxValue);
        //    Duration = (int)d;
        //}
    }
}
