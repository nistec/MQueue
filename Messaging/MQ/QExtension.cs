using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.IO;
using System.IO;
using Nistec.Generic;
using System.Threading.Tasks;
using Nistec.Messaging.Remote;
using Nistec.Data;
using Nistec.Runtime;
using Nistec.Messaging.Io;
using Nistec.Serialization;
using Nistec.Channels;
using System.Threading;

namespace Nistec.Messaging
{
    public static class QExtension
    {
 

        public static NetProtocol GetProtocol(this HostProtocol protocol)
        {
            if ((int)protocol > 3)
                return NetProtocol.NA;
            return (NetProtocol)(int)protocol;
        }

        /// <summary>
        /// Serialize message as stream.
        /// </summary>
        /// <param name="writeContextType"></param>
        /// <returns></returns>
        public static NetStream Serialize(this IMessage message, bool writeContextType = true)
        {
            NetStream ns = new NetStream();
            var streamer = new BinaryStreamer(ns);
            if (writeContextType)
            {
                streamer.WriteContextType(SerialContextType.SerialEntityType);
            }
            message.EntityWrite(ns, streamer);
            return ns;
        }

        /// <summary>
        /// Serialize message as stream.
        /// </summary>
        /// <param name="writeContextType"></param>
        /// <returns></returns>
        public static NetStream Serialize(this IQueueMessage message, bool writeContextType)
        {
            NetStream ns = new NetStream();
            var streamer = new BinaryStreamer(ns);
            if (writeContextType)
            {
                streamer.WriteContextType(SerialContextType.SerialEntityType);
            }
            message.EntityWrite(ns, streamer);
            return ns;
        }


        public static TransStream DoResponse(this QueueMessage item)
        {
            if (item != null)
                return item.ToTransStream();//.BodyStream;
            return null;
        }
        public static TransStream DoResponse(this IQueueMessage item)
        {
            if (item != null)
                return item.ToTransStream();
            return null;
        }

        //public static IQueueAck SendToQueue(this Message message, QueueHost target, int connectTimeout)
        //{
        //    message.HostAddress = target.RawHostAddress;
        //    QueueClient client = new QueueClient(target.HostName, target.ServerName);
        //    return client.Send(message, connectTimeout);
        //}

        //public static IQueueAck SendToFile(this Message message, QueueHost target)
        //{
        //    message.SaveToFile(target.QueuePath);
        //    return new MessageAck(message, MessageState.Received, (string)null);
        //}

        public static string GetFileName(this QueueMessage message, string path)
        {
            return SysIO.PathFix(path + "\\" + message.Filename);
        }

        public static void SaveToFile(this QueueMessage message, string path)
        {
            string filename = message.GetFileName(path);
            var stream = message.Serialize(true);
            stream.SaveToFile(filename);

            //File.WriteAllBytes(filename, ToByteArray());

            //using (FileStream fs = File.Create(filename))
            //{
            //    fs.BeginWrite
            //    ToStream(fs);
            //    SysUtil.StreamCopy(message, fs);

            //    // Create message info file for the specified relay message.
            //    RelayMessageInfo messageInfo = new RelayMessageInfo(sender, to, date, false, targetHost);

            //    File.WriteAllBytes(filename, ToByteArray());
            //}
        }
        /*
        public static object[] MessageItemArray(this QueueMessage msg)
        {
            //return new object[] {
            //MessageState,
            //MessageType,
            //Command,
            //Priority,
            //Identifier,
            //Retry,
            //ArrivedTime,
            //Creation,
            //Modified,
            //Expiration,
            //TransformType,
            //Destination,
            //Label,
            //Sender,
            ////BodyStream,
            //TypeName
            //};

            return new object[]{
            msg.MessageState,
            msg.Command,
            msg.Priority,
            //msg.UniqueId,
            msg.Retry,
            msg.ArrivedTime,
            msg.Creation,
            msg.Modified,
            msg.Duration,
            msg.Identifier,
            msg.BodyStream

            };

        }
        */
        public static DataParameter[] MessageDataParameters(this QueueMessage msg)
        {
            return new DataParameter[]{
            new DataParameter("Host", msg.Host),
            new DataParameter("MessageState", (int)msg.MessageState),
            new DataParameter("Command", (int)msg.QCommand),
            new DataParameter("Priority", (int)msg.Priority),
            new DataParameter("Identifier", msg.Identifier),
            new DataParameter("Retry", msg.Retry),
            new DataParameter("ArrivedTime", msg.ArrivedTime),
            new DataParameter("Modified", msg.Creation),
            //new DataParameter("Modified",msg.Modified),
            new DataParameter("Expiration", msg.Expiration),
            new DataParameter("MessageId", msg.CustomId),
            new DataParameter("Body", msg.Body)
            };

            //return new DataParameter[]{
            //new DataParameter("MessageState",msg.MessageState),
            //new DataParameter("Command",msg.Command),
            //new DataParameter("Priority",msg.Priority),
            ////new DataParameter("UniqueId",msg.UniqueId),
            //new DataParameter("Retry",msg.Retry),
            //new DataParameter("ArrivedTime",msg.ArrivedTime),
            //new DataParameter("Creation",msg.Creation),
            ////new DataParameter("Modified",msg.Modified),
            //new DataParameter("Duration",msg.Duration),
            //new DataParameter("Identifier",msg.Identifier),
            //new DataParameter("BodyStream",msg.BodyStream)

            //};

        }

    }
}
