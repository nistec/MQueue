using Nistec.Channels;
using Nistec.Messaging;
using Nistec.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueListenerDemo
{
    public class QueueDemo
    {
        const string QueueName = "Netcell";

        static QueueRequest CreateRequest()
        {

           return new QueueRequest()
            {
                Command = QueueCmd.Consume.ToString(),
                Host = QueueName,
                DuplexType = DuplexTypes.Respond
            };
        }

        public static void DemoRequest()
        {
            QueueRequest message = CreateRequest();

            byte[] bytes=message.Serialize();

            IDataStream daatStream= new TransStream(bytes);//binary

            QueueRequest resmsg = QueueRequest.Deserialize(daatStream.DataStream());

            Console.WriteLine(resmsg.Label);

        }

    }
}
