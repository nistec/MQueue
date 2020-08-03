using Nistec.Data.Entities;
using Nistec.Data.Sqlite;
using Nistec.Messaging;
using Nistec.Messaging.Listeners;
using Nistec.Messaging.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace QueueListenerDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("QueueListener started...");

            //QueueReceiver.DoGet(QueueReceiver.GetHost("tcp", "127.0.0.1:15001", "NC_Bulk"));
            QueueReceiver.DoListnning(QueueReceiver.GetHost("tcp", "127.0.0.1:15001", "NC_Bulk"));

 
            Console.WriteLine("QueueListener finished...");
            Console.ReadLine();
        }
     
    }
}
