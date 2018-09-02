using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Nistec.Messaging;
using Nistec.Messaging.Remote;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Messaging.Io;
using System.IO;

namespace Nistec
{
    public class QClient
    {

        public static void Test()
        {
            QClient client = new QClient();

            client.AddQueue();
            client.AddItems(10);
            client.QueueCount();
            client.DequeueItems();
        }

        public static void RuntimeTest()
        {
            var message = new Message("QClient", queuenmae, new EntityDemo(0));
            
            var mStream = message.GetEntityStream(false);
            //mStream.Position = 0;
            var desMsg = Message.Create(mStream);

            QueueItemStream item = new QueueItemStream(mStream, true);

            item.SetState(MessageState.Receiving);

            Message recMsg = item.GetMessage();

            var body = recMsg.GetBody<EntityDemo>();

            string filename = Path.Combine(@"D:\Nistec\Services\MQueue\Queues\Queue\Demo", Assists.GetQueueFilename(item.Identifier));

            string fname= item.SaveToFile(filename);

            QueueItemStream ritem = QueueItemStream.ReadFile(filename);

            Message rMsg = ritem.GetMessage();

            var rbody = recMsg.GetBody<EntityDemo>();

            Console.WriteLine( rbody==null? 0: rbody.EntityId);



        }

        public static void ReadFile(string filename)
        {
            var item= QueueItemStream.ReadFile(filename);
            var msg=item.GetMessage();
            var body= msg.GetBody();
            Console.WriteLine(item.Print());
        }

        static string queuenmae = "Demo";

        public void AddQueue()
        {
            QueueClient client = new QueueClient(queuenmae);
          var response=  client.AddQueue(CoverMode.File, false);
          Console.WriteLine(response.MessageState);
        }

        public void AddItem(int index)
        {
            QueueClient client = new QueueClient(queuenmae);
            var message = new Message("QClient", queuenmae, new EntityDemo(index));
            var ack = client.Send(message);
            Netlog.DebugFormat("Client Send state:{0}", ack.Print());
        }

        public void AddItems(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddItem(i);
            }
        }

        public void QueueCount()
        {
            QueueClient client = new QueueClient(queuenmae);
            var ack = client.Report<int>(QueueCmdReport.QueueCount);
            Netlog.DebugFormat("Client QueueCount :{0}", ack);
        }

        public void DequeueItems()
        {
            QueueClient client = new QueueClient(queuenmae);
 
            while (client.Report<int>(QueueCmdReport.QueueCount) > 0)
            {
                DequeueItem();
            }
        }

        public void DequeueItem()
        {
            QueueClient client = new QueueClient(queuenmae);
            var message = client.Receive();
            Netlog.DebugFormat("Client Receive state:{0}", message.Print());
        }


    }

    //public class seq
    //{
    //    private static object _toSeqGuidLock = new object();

    //    //''' <summary>
    //    //''' Replaces the most significant eight bytes of the GUID (according to SQL Server ordering) with the current UTC-timestamp.
    //    //''' </summary>
    //    //''' <remarks>Thread-Safe</remarks>

    //    static Int64 lastTicks = -1;

    //    [System.Runtime.CompilerServices.Extension()]
    //    public static Guid ToSeqGuid(Guid guid)
    //    {

    //        long ticks = DateTime.UtcNow.Ticks;

    //        lock (_toSeqGuidLock)
    //        {
    //            if (ticks <= lastTicks)
    //            {
    //                ticks = lastTicks + 1;
    //            }

    //            lastTicks = ticks;

    //        }

    //        byte[] ticksBytes = BitConverter.GetBytes(ticks);

    //        Array.Reverse(ticksBytes);

    //        byte[] guidBytes = guid.ToByteArray();

    //        Array.Copy(ticksBytes, 0, guidBytes, 10, 6);
    //        Array.Copy(ticksBytes, 6, guidBytes, 8, 2);

    //        return new Guid(guidBytes);


    //    }
    //}
}
