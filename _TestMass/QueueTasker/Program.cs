using Nistec.Data.Entities;
using Nistec.Messaging;
using Nistec.Messaging.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueTasker
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("QueueTasker started...");

            //QueueClientDemo.SendItem(true);

            QueueClientDemo.SendMulti(true,10000);
       

            Console.WriteLine("QueueTasker finished...");
            Console.ReadLine();
      
        }

    }
}
