using Nistec.IO;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Messaging
{

        public class MessageHeader
        {

        internal static MessageHeader Get(byte[] value)
        {
            var header = new MessageHeader();
            header.EntityWrite(new NetStream(value), null);
            return header;
        }

        #region property

            public int Version { get { return 4022; } }

            /// <summary>
            /// Get ItemId
            /// </summary>
            public string Identifier { get; set; }


            /// <summary>
            /// Get MessageState
            /// </summary>
            public MessageState MessageState { get; set; }

            /// <summary>
            /// Get Command
            /// </summary>
            public QueueCmd Command { get; set; }

            /// <summary>
            /// Get or Set transformation type.
            /// </summary>
            public TransformTypes TransformType { get; set; }

            /// <summary>
            /// Get or Set message type.
            /// </summary>
            public MQTypes MessageType { get; set; }

            /// <summary>
            /// Get Priority
            /// </summary>
            public Priority Priority { get; set; }

            /// <summary>
            /// Get The message Destination\Queue name.
            /// </summary>
            public string Host { get; set; }

            /// <summary>
            /// Get Retry
            /// </summary>
            public byte Retry { get; set; }

            /// <summary>
            /// Get ArrivedTime
            /// </summary>
            public DateTime ArrivedTime { get; set; }
            /// <summary>
            /// Get Creation Time
            /// </summary>
            public DateTime Creation { get; set; }
            /// <summary>
            /// Get the last modified time.
            /// </summary>
            public DateTime Modified { get; set; }

            /// <summary>
            /// Get or Set Expiration in minutes
            /// </summary>
            public int Duration { get; set; }

            /// <summary>
            /// Get or Set The message Sender.
            /// </summary>
            public string Sender { get; internal set; }

            /// <summary>
            /// Get or Set The message Label.
            /// </summary>
            public string Label { get; set; }

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
                streamer.WriteValue((byte)MessageType);
                streamer.WriteValue((byte)Command);
                streamer.WriteValue((byte)Priority);
                streamer.WriteString(Identifier);//.WriteValue(ItemId);
                streamer.WriteValue(Retry);
                streamer.WriteValue(ArrivedTime);
                streamer.WriteValue(Creation);
                streamer.WriteValue(Modified);
                streamer.WriteValue(Duration);
                streamer.WriteValue((byte)TransformType);
                streamer.WriteString(Host);
                streamer.WriteString(Sender);
                streamer.WriteString(Label);
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
                MessageType = (MQTypes)streamer.ReadValue<byte>();
                Command = (QueueCmd)streamer.ReadValue<byte>();
                Priority = (Priority)streamer.ReadValue<byte>();
                Identifier = streamer.ReadString();//.ReadValue<Guid>();
                Retry = streamer.ReadValue<byte>();
                ArrivedTime = streamer.ReadValue<DateTime>();
                Creation = streamer.ReadValue<DateTime>();
                Modified = streamer.ReadValue<DateTime>();
                Duration = streamer.ReadValue<int>();
                TransformType = (TransformTypes)streamer.ReadValue<byte>();
                Host = streamer.ReadString();
                Sender = streamer.ReadString();
                Label = streamer.ReadString();

            }

            #endregion

            public NetStream ToStream()
            {
                NetStream stream = new NetStream();
                EntityWrite(stream, null);
                return stream;
            }
            public byte[] ToBinary()
            {
                return ToStream().ToArray();
            }
        }
    }
