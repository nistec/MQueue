using Nistec.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Nistec.Messaging
{
    public class MessageReader
    {

        //public static QueueMessage ReadStream(NetworkStream stream, int ProcessTimeout, int ReceiveBufferSize = 8192)
        //{
        //      NetStream netstream=new NetStream();
        //      netstream.CopyWithTerminateCount(stream,ProcessTimeout,ReceiveBufferSize);

        //      return new QueueMessage(netstream,null);
        //}

        //public static QueueMessage ReadStream(PipeStream stream, int ReceiveBufferSize=8192)
        //{
        //    NetStream netstream=new NetStream();
        //    netstream.CopyFrom(stream,ReceiveBufferSize);
        //    return new QueueMessage(netstream,null);
        //}

        public static QueueMessage ReadQStream(NetworkStream stream, int ReadTimeout, int ReceiveBufferSize = 8192)
        {
            NetStream netstream = new NetStream();
            //netstream.CopyWithTerminateCount(stream, ProcessTimeout, ReceiveBufferSize);
            netstream.CopyFrom(stream, ReadTimeout,ReceiveBufferSize);
            return new QueueMessage(netstream, null);
        }

        public static QueueMessage ReadQStream(PipeStream stream, int ReceiveBufferSize = 8192)
        {
            NetStream netstream = new NetStream();
            netstream.CopyFrom(stream, ReceiveBufferSize);
            return new QueueMessage(netstream, null);
        }

        public static QueueAck ReadAckStream(NetworkStream stream, int ReadTimeout, int ReceiveBufferSize = 8192)
        {
            NetStream netstream = new NetStream();
            //netstream.CopyWithTerminateCount(stream, ProcessTimeout, ReceiveBufferSize);
            netstream.CopyFrom(stream, ReadTimeout, ReceiveBufferSize);
            return new QueueAck(netstream);
        }

        public static QueueAck ReadAckStream(PipeStream stream, int ReceiveBufferSize = 8192)
        {
            NetStream netstream = new NetStream();
            netstream.CopyFrom(stream, ReceiveBufferSize);
            return new QueueAck(netstream);
        }
        public static object ReadBodyStream(NetworkStream stream, int ReadTimeout, int ReceiveBufferSize = 8192)
        {
            QueueMessage m = ReadQStream(stream, ReadTimeout, ReceiveBufferSize);
            if (m == null)
                return null;

            return m.GetBody();
        }

        public static object ReadBodyStream(PipeStream stream, int ReceiveBufferSize = 8192)
        {
            QueueMessage m = ReadQStream(stream, ReceiveBufferSize);
            if (m == null)
                return null;

            return m.GetBody();
        }
        public static QueueAck ReadAck(PipeStream stream, int ReceiveBufferSize = 8192)
        {
            NetStream netstream = new NetStream();
            netstream.CopyFrom(stream, ReceiveBufferSize);
            return new QueueAck(netstream);
        }
    }
}
