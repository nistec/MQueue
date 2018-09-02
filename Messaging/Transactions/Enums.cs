using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Transactions
{
    
    public enum TransItemState
    {
        Wait = 0,
        Timeout = 1,
        Abort = 2,
        Commit = 3,
        Retry = 4
    }

    /// <summary>
    /// TransSyncState
    /// </summary>
    public enum TransSyncState
    {
        /// <summary>
        /// Cache TransSyncState is in idle
        /// </summary>
        Idle = 0,
        /// <summary>
        /// Cache TransSyncState Should Start
        /// </summary>
        ShouldStart = 1,
        /// <summary>
        /// Cache TransSyncState is Started
        /// </summary>
        Started = 2,
        /// <summary>
        /// Cache TransSyncState is Stoped
        /// </summary>
        Finished = 3
    }

    #region SyncTimeCompleted
    /*
    /// <summary>
    /// SyncTimeCompletedEventHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SyncTimerEventHandler(object sender, SyncTimerEventArgs e);

    /// <summary>
    /// CacheEventArgs
    /// </summary>
    public class SyncTimerEventArgs : EventArgs
    {
        Guid[] items;

        /// <summary>
        /// SyncTimeCompletedEventArgs
        /// </summary>
        /// <param name="items"></param>
        public SyncTimerEventArgs(Guid[] items)
        {
            this.items = items;
        }

        #region Properties Implementation
        /// <summary>
        /// Items
        /// </summary>
        public Guid[] Items
        {
            get { return this.items; }
        }

        #endregion

    }
    */
    #endregion

}
