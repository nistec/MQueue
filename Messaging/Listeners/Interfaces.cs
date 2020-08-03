using Nistec.Channels;
using Nistec.Logging;
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

    public interface IChannelService
    {
        /// <summary>
        /// Get <see cref="ChannelServiceState"/> State.
        /// </summary>
        ChannelServiceState ServiceState { get; }
        void Start();
        void Stop();
        void Pause();
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        ILogger Log { get; set; }
    }

    public interface IListener
    {
        void Start();
        void Stop();
        void Shutdown(bool waitForWorkers);
        void Pause(int seconds);
        //void Delay(TimeSpan time);
    }
}