using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nistec.Runtime;
using Nistec.Serialization;
using Nistec.IO;
using Nistec.Channels;
using Nistec.Generic;

namespace Nistec.Messaging
{
    [Serializable]
    public class QueueAck: ISerialEntity, IQueueAck, IAck
    {

        public static QueueAck DoResponse(MessageState state, string message, object response = null)
        {
            return new QueueAck() { MessageState = state, Label = message, Response = response };
        }
        //public static QueueAck DoResponse(MessageState state, string label, string identifier, string host, object response = null)
        //{
        //    return new QueueAck() { MessageState = state, Label = label, Response = response, Identifier = identifier, Host = host };
        //}
        public QueueAck()
        {
            Creation = DateTime.Now;
        }
        public QueueAck(MessageState state, string label, string identifier, string host, object response = null)
        {
            MessageState = state;
            Label = label;
            Response = response;
            Identifier = identifier;
            Host = host;
        }
        public QueueAck(MessageState state, string identifier, string host):this()
        {
            this.Identifier = identifier;
            this.Label = null;
            this.MessageState = state;
            this.Host = host;
        }
        public QueueAck(MessageState state, string result, string identifier, string host) : this()
        {
            this.Identifier = identifier;
            this.Label = result;
            this.MessageState = state;
            this.Host = host;
        }
        public QueueAck(MessageState state, string destination, Exception ex) : this()
        {
            this.Label = ex.Message;
            this.MessageState = state;
            this.Host = destination;
        }
        public QueueAck(MessageState state, IQueueMessage item, object response = null) : this()
        {
            this.Identifier = item.Identifier;
            this.Label = item.Label;
            this.MessageState = state;
            this.Host = item.Host;
            this.Response = response;
        }
        public QueueAck(MessageState state, IQueueRequest item, object response = null) : this()
        {
            this.Identifier = item.Identifier;
            this.Label = item.Label;
            this.MessageState = state;
            this.Host = item.Host;
            this.Response = response;
        }
        public QueueAck(IQueueAck[] acks) : this()
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

                    this.Identifier += "|" + ack.Identifier;
                    this.Label += "|" + ack.Label;
                    //this.MessageState += "|" + ack.MessageState;
                    this.Host += "|" + ack.Host;
                }
            }

            //this.Count = acks.Count();
        }

        public QueueAck(NetStream stream) : this()
        { 
            EntityRead(stream, null);
        }

        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }
        public TransStream ToTransStream()
        {
            TransStream stream = new TransStream(this, TransType.Ack);
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
        ///// <summary>
        ///// Get or Set the arrived time.
        ///// </summary>
        //public DateTime ArrivedTime { get; internal set; }
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
        ///// <summary>
        ///// Get or Set Count
        ///// </summary>
        //public int Count { get; internal set; }
        public string Print()
        {
            return string.Format("Creation:{0}, MessageState:{1}, Label:{2}, Host:{3}, Identifier:{4}, Duration:{5}", Creation, MessageState, Label, Host, Identifier, Duration);
        }



        #endregion

        #region IAck
        public string Message { get { return Label; } }
        //public DateTime Modified { get; protected set; }
        public object Response { get; set; }
        public int Status { get { return (int)MessageState; } }
        public bool IsOk { get { return MessageState.IsStateOk(); } }
        public string ToJson()
        {
            KeyValueArgs a = new KeyValueArgs();
            a.Add("Label", Label);
            a.Add("Response", Response);
            a.Add("State", MessageState.ToString());
            if (!Host.IsNull()) a.Add("Identifier", Identifier);
            if (!Host.IsNull()) a.Add("Host", Host);
            if (!Host.IsNull()) a.Add("Label", Label);

            return JsonSerializer.Serialize(a.ToJson());
            //return GenericKeyValue.Create("Label", Label, "Response", Response, "State", MessageState).ToJson();
        }
        public string Display()
        {
            return Strings.ReflatJson(GenericKeyValue.Create("Label", Label, "Response", Response, "State", MessageState.ToString()).ToJson());
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
            streamer.WriteValue(Response);
            //streamer.WriteValue(Count);
            //streamer.WriteValue(ArrivedTime);
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
            Response = streamer.ReadValue();
            //Count = streamer.ReadValue<int>();
            //string arrived = streamer.ReadString();
            //ArrivedTime= Types.ToNullableDate(arrived);
            //ArrivedTime = streamer.ReadValue<DateTime>();
        }

        #endregion
               
  
        internal void SetArrived()
        {
            DateTime arrived = DateTime.Now;
            //ArrivedTime = arrived;
            var d = arrived.Subtract(Creation).TotalMilliseconds;
            d = Math.Min(d, int.MaxValue);
            Duration = (int)d; //Math.Round(d, 4, MidpointRounding.AwayFromZero);
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
