using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Nistec.Messaging
{
    public class TimeOut
    {

        public static readonly TimeSpan MaxTimeout = TimeSpan.FromMilliseconds(4294967295);

      
        DateTime start;
        TimeSpan timeout;
        public TimeOut(TimeSpan timeout)
        {
            //long totalMilliseconds = (long)timeout.TotalMilliseconds;
            //if ((totalMilliseconds < 0L) || (totalMilliseconds > 4294967295L))
            //{
            //    totalMilliseconds = (long)MaxTimeout.TotalMilliseconds;
            //    //throw new ArgumentException("InvalidParameter", "timeout");
            //}

            this.timeout = timeout;
            this.start = DateTime.Now;        
        }

        public bool IsTimeOut()
        {
            TimeSpan ts = DateTime.Now.Subtract(start);
            return (ts > timeout);
        }

    }

    public class TimeOutHandle
    {
        public event EventHandler TimeoutOccured;

        protected virtual void OnTimeout(EventArgs e)
        {
            if (TimeoutOccured != null)
                TimeoutOccured(this, e);
        }

        private bool isTimeOut;

        public bool IsTimeOut
        {
            get { return isTimeOut; }
        }
 
        public TimeOutHandle()
        {
            isTimeOut = false;
        }

  
        // The delegate must have the same signature as the method
        // it will call asynchronously.
        internal delegate bool AsyncMethodCaller(int timeout, out int threadId);


         // Asynchronous method puts the thread id here.
        private int threadId;

        public void StartTimeOut(TimeSpan timeout) 
        {
            // Create an instance of the test class.
            AsyncTimeOut ad = new AsyncTimeOut();

            // Create the delegate.
            AsyncMethodCaller caller = new AsyncMethodCaller(ad.TestMethod);

            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke((int)timeout.TotalSeconds,
                out threadId, 
                new AsyncCallback(CallbackMethod),
                caller );

        }

        // Callback method must have the same signature as the
        // AsyncCallback delegate.
         void CallbackMethod(IAsyncResult ar) 
        {
            // Retrieve the delegate.
            AsyncMethodCaller caller = (AsyncMethodCaller) ar.AsyncState;

            // Call EndInvoke to retrieve the results.
            bool returnValue = caller.EndInvoke(out threadId, ar);

            if (returnValue)
            {
                OnTimeout(EventArgs.Empty);
            }

            //Console.WriteLine("The call executed on thread {0}, with return value \"{1}\".",
            //    threadId, returnValue);
        }
    

        internal class AsyncTimeOut
        {
            TimeOut to;

            // The method to be executed asynchronously.
            public bool TestMethod(int timeout, out int threadId)
            {
                Console.WriteLine("Test method begins.");
                TimeSpan sp = TimeSpan.FromSeconds((double)timeout);
                to = new TimeOut(sp);
                Thread.Sleep(sp);
                threadId = Thread.CurrentThread.ManagedThreadId;
                return to.IsTimeOut();//String.Format("My call time was {0}.", callDuration.ToString());
            }
        }

    }
}
