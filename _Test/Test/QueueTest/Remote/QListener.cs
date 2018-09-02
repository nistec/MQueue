using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Nistec.Messaging;
using Nistec.Messaging.Remote;
using Nistec.Messaging.Listeners;
using Nistec.Messaging.Adapters;

namespace Nistec
{
    public class QListener: QueueListener
    {

        public static void Test()
        {
            AdapterProperties ap = new AdapterProperties(queuenmae, ".");
            QListener listener= new QListener(ap);
            listener.Start();
        }


        static string queuenmae = "Demo";

        public QListener(AdapterProperties lp)
            : base(lp)
        {

        }
                
        protected override void OnErrorOcurred(Generic.GenericEventArgs<string> e)
        {
            base.OnErrorOcurred(e);

            Console.WriteLine("OnError: " + e.Args);
        }

        protected override void OnMessageReceived(Generic.GenericEventArgs<Message> e)
        {
            base.OnMessageReceived(e);

            Console.WriteLine("OnMessageReceived: " + e.Args.Identifier);
        }
    }
}
