using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Logging;
using Nistec.Channels;

namespace Nistec.Messaging
{
    public class MessageAckServer
    {
        static void WriteReponse(Stream pipeServer, byte[] response)
        {
            int length = response.Length;

            QLogger.DebugFormat("Serevr WriteReponse:{0}", length);

            pipeServer.Write(response, 0, length);

            pipeServer.Flush();
        }

        public static void WriteAck(Stream stream, bool state, QueueCmd cmd, string label)
        {
            WriteAck(stream, state ? MessageState.Ok : MessageState.None, cmd, label);
        }

        public static void WriteAck(Stream pipeStream, MessageState state,  QueueCmd cmd, string label)
        {
            if (pipeStream == null)
            {
                throw new MessageException(MessageState.PipeError, "Invalid stream to write ack");
            }
            string lbl = label == null ? state.ToString() : label;
            QueueItem response = QueueItem.Ack(state, cmd,lbl,null);
            using (NetStream stream = new NetStream())
            {
                response.EntityWrite(stream, null);
                WriteReponse(pipeStream, stream.ToArray());
            }

            QLogger.DebugFormat("Server Ack State:{0}, Label: {1}", state, label);
        }

        public static void WriteError(Stream pipeStream, MessageState state, QueueCmd cmd, Exception ex)
        {
            if (pipeStream == null)
                return;
            try
            {
                string lbl = ex.Message;
                QueueItem response = QueueItem.Ack(state, cmd, ex);
                using (NetStream stream = new NetStream())
                {
                    response.EntityWrite(stream, null);
                    WriteReponse(pipeStream, stream.ToArray());
                }
            }
            catch (Exception x)
            {
                QLogger.ErrorFormat("QueueResponse WriteError Exception: " + x.Message);
            }
        }
        

        //public static void WriteResponse(Stream pipeStream, IQueueItem item, MessageState state)
        //{
        //    if (pipeStream == null)
        //    {
        //        throw new MessageException(MessageState.PipeError, "Invalid stream to write response");
        //    }
        //    if (item != null)
        //    {
        //        ((QueueItem)item).SetState(state);
        //        WriteReponse(pipeStream, item.Serilaize());

        //        //using (NetStream stream = new NetStream())
        //        //{
        //        //    //response.EntityWrite(stream, null);
        //        //    WriteReponse(pipeStream, ((QueueItem)item).ItemBinary);
        //        //}
        //    }
        //    else
        //    {
        //        QueueItem response = QueueItem.Ack(MessageState.UnExpectedError,item.QCommand, new MessageException(MessageState.UnExpectedError, "WriteResponse error: there is no item stream to write reponse"));
        //        using (NetStream stream = new NetStream())
        //        {
        //            response.EntityWrite(stream, null);
        //            WriteReponse(pipeStream, stream.ToArray());
        //        }
        //    }

        //    QLogger.DebugFormat("Server WriteResponse State:{0}, Identifier: {1}", item.MessageState, item.Identifier);
        //}

        public static void WriteReport(Stream pipeStream, object item, QueueCmd cmd, MessageState state, string lbl)
        {
            if (pipeStream == null)
            {
                throw new MessageException(MessageState.PipeError, "Invalid stream to write report");
            }
            if (item != null)
            {
                var message = QueueItem.Ack(state, cmd, lbl,null);
                
                message.SetBody(item);
                WriteReponse(pipeStream, message.ToStream().ToArray());
            }
            else
            {
                QueueItem response = QueueItem.Ack(MessageState.UnExpectedError, cmd, new MessageException(MessageState.UnExpectedError, "WriteReport error: there is no item stream to write reponse"));
                WriteReponse(pipeStream, response.ToStream().ToArray());
            }

            QLogger.DebugFormat("Server WriteReport State:{0}, Command: {1}", state, cmd);
        }

        public static TransStream DoError(MessageState state, QueueCmd cmd, Exception ex)
        {

            try
            {
                string lbl = ex.Message;
                return QueueItem.Ack(state, cmd, ex).ToTransStream(); ;
                //using (NetStream stream = new NetStream())
                //{
                //    response.EntityWrite(stream, null);
                //    WriteReponse(pipeStream, stream.ToArray());
                //}
            }
            catch (Exception x)
            {
                QLogger.ErrorFormat("QueueResponse WriteError Exception: " + x.Message);
                //var ack = new Message(MessageState.StreamReadWriteError, new MessageException(MessageState.StreamReadWriteError, "Invalid stream to write ack"));
                //return ack.GetEntityStream(false);
                return null;
            }
        }

        public static TransStream DoResponse(IQueueAck item)
        {
            if (item == null)
            {
                return null;
                //throw new MessageException(MessageState.MessageError, "Invalid queue item to write response");
            }
            QLogger.DebugFormat("QueueController DoResponse IQueueAck: {0}", item.Print());
            return item.ToStream();
        }
        public static TransStream DoResponse(IQueueItem item)
        {
            if (item == null)
            {
                return null;
                //throw new MessageException(MessageState.MessageError, "Invalid queue item to write response");
            }
            QLogger.DebugFormat("QueueController DoResponse IQueueAck: {0}", item.Print());
            return item.ToTransStream();
        }
        public static TransStream DoResponse(IQueueItem item, MessageState state)
        {
            if (item == null)
            {
                return null;
                //throw new MessageException(MessageState.MessageError, "Invalid queue item to write response");
            }

            var ts=((QueueItem)item).ToTransStream(state);

            //((QueueItem)item).SetState(state);
            QLogger.DebugFormat("QueueController DoResponse IQueueAck: {0}", item.Print());
            //return item.ToStream();

            return ts;

            //if (item != null)
            //{
            //    ((QueueItem)item).SetState(state);
            //    return item.GetItemStream();
            //}
            //else
            //{
            //    Message response = Message.Ack(MessageState.UnExpectedError, new MessageException(MessageState.UnExpectedError, "WriteResponse error: there is no item stream to write reponse"));
            //    return response.ToStream();
            //}

           // QLogger.DebugFormat("Server WriteResponse State:{0}, MessageId: {1}", item.MessageState, item.MessageId);
        }

        public static TransStream DoReportValue(object value)
        {
            return new TransStream(value, TransType.Object);
        }


        public static TransStream DoReport( object item, QueueCmd cmd, MessageState state, string lbl)
        {
            if (item == null)
            {
                throw new MessageException(MessageState.PipeError, "Invalid item to write response");
            }
            if (item != null)
            {
                var message = QueueItem.Ack(state, cmd,lbl,null);
                
                message.SetBody(item);
                return message.ToTransStream();
            }
            else
            {
                QueueItem response = QueueItem.Ack(MessageState.UnExpectedError, cmd, new MessageException(MessageState.UnExpectedError, "WriteReport error: there is no item stream to write reponse"));
                return response.ToTransStream();
            }

           // QLogger.DebugFormat("Server WriteReport State:{0}, MessageType: {1}", state, msgType);

        }

    }
}
