using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Nistec.Messaging;

namespace Nistec
{
    public class TimeoutDemo
    {

        public static void Demo()
        {
            DateTime start = DateTime.Now;
            TimeOutHandle toh = new TimeOutHandle();
            toh.TimeoutOccured += new EventHandler(toh_TimeoutOccured);
            toh.StartTimeOut(new TimeSpan(0, 0, 50));

            while (true)
            {
                TimeSpan ts = DateTime.Now.Subtract(start);
                Console.WriteLine(ts.TotalSeconds.ToString());
                Thread.Sleep(100);
                if (ts.TotalSeconds > 20)
                    break;
                if (toh.IsTimeOut)
                    break;
            }
        }
        static void toh_TimeoutOccured(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
            Console.WriteLine(sender.ToString());
        }
    }
}
