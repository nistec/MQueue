using Nistec.Messaging;
using Nistec.Messaging.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace QueueManagement
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("QueueTasker started...");

            var host = QueueHost.Parse("ipc:.:nistec_queue_channel?NC_Quick");
            QueueApi q = new QueueApi(host);
           
            while (true)
            {
               var msg= q.Report(QueueCmdReport.ReportQueueStatistic, "NC_Quick");

                Console.WriteLine("State:{0},Creation:{1},Host:{2},Label:{3}", msg.MessageState, msg.Creation,msg.Host, msg.Label);

                  Thread.Sleep(10000);
            }



            Console.WriteLine("QueueTasker finished...");
            Console.ReadLine();
      
        }
    }
}
