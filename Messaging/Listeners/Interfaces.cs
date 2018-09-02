using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Listeners
{
    internal interface IListenerHandler
    {
        void DoMessageReceived(QueueItem message);

        void DoErrorOcurred(string message);
    }

    public interface IListener
    {
        void Start();

        void Shutdown(bool waitForWorkers);

        void Delay(TimeSpan time);
    }
}
