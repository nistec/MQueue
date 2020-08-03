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
            //QueueReceiver.DoListnning(QueueReceiver.GetHost("tcp", "127.0.0.1:15001", "NC_Bulk"));
            QueueReceiver.DoSbscriberListener();

            //TopicSubs topicSubs = new TopicSubs();
            //topicSubs.Start();


            /*          
                       //var api= QueueApi.Get(Nistec.Channels.NetProtocol.Pipe);
                       var hostPipe = QueueHost.Parse("ipc:.:nistec_queue_listener?NC_Quick");
                       var hostTcp = QueueHost.Parse("tcp:localhost:15001?NC_Quick");


                       //var lista = AppDomain.CurrentDomain.GetAssemblies();
                       //Console.WriteLine(lista);

                       //Nistec.Serialization.SerializeTools.ResolveLoadAssemblies();//139 - 24
                       //Nistec.Serialization.SerializeTools.LoadReferencedAssemblies();//111 - 21
                       //Nistec.Serialization.SerializeTools.LoadReferencedAssembly(typeof(Nistec.Data.Sqlite.CommitMode));//36 - 20
                       //Nistec.Serialization.SerializeTools.LoadReferencedAssembly();//119 - 21

                       //var com= Nistec.Data.Sqlite.CommitMode.None;

                       //var listb = AppDomain.CurrentDomain.GetAssemblies();
                       //Console.WriteLine(listb);

                       DoGet(hostTcp);


                       //DoQuery(hostPipe);

                       //DoListnning(hostTcp);
           */

            Console.ReadLine();
            Console.WriteLine("QueueListener finished...");
        }

    }
}
