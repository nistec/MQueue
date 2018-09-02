//licHeader
//===============================================================================================================
// System  : Nistec.Cache - Nistec.Cache Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of cache core.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Nistec.Threading;
using System.Collections.Concurrent;

namespace Nistec.Caching
{
    public class TimerWorker : IDisposable
    {
        private ThreadTimer SettingTimer;

        public int IntervalMilliseconds
        {
            get;
            internal set;
        }

        public bool Initialized
        {
            get;
            private set;
        }

        public DateTime LastSyncTime
        {
            get;
            private set;
        }
        public SyncState SyncState
        {
            get;
            private set;
        }

        public DateTime NextSyncTime
        {
            get
            {
                return this.LastSyncTime.AddSeconds((double)this.IntervalSeconds);
            }
        }

        public TimerDispatcher(int intervalSeconds, int initialCapacity, bool isRemote)
        {

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            if (initialCapacity < 100)
                initialCapacity = 101;

            this.IntervalSeconds = intervalSeconds;
            this.Initialized = false;
            this.SyncState = CacheSyncState.Idle;
            this.LastSyncTime = DateTime.Now;
            this.LogAction("Initialized TimeoutDispatcher");
        }

        public void Dispose()
        {
            if (SettingTimer != null)
            {
                DisposeTimer();
            }
        }

        #region events

        public event EventHandler SyncStarted;

        public event SyncTimeCompletedEventHandler SyncCompleted;

        protected virtual void OnSyncStarted(EventArgs e)
        {
            if (this.SyncStarted != null)
            {
                this.SyncStarted(this, e);
            }

            this.OnSyncTimer();
        }

        protected virtual void OnSyncCompleted(SyncTimeCompletedEventArgs e)
        {
            if (this.SyncCompleted != null)
            {
                this.SyncCompleted(this, e);
            }
        }

        #endregion

   

        #region Timer Sync

        internal void SetCacheSyncState(CacheSyncState state)
        {
            this.SyncState = state;
        }

        private void SettingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Initialized && (this.SyncState == CacheSyncState.Idle))
            {
                this.LogAction("Synchronize Start");
                this.LastSyncTime = DateTime.Now;
                this.OnSyncStarted(EventArgs.Empty);
                DateTime time = this.LastSyncTime.AddSeconds((double)this.IntervalSeconds);
                this.LogAction("Synchronize End, Next Sync:{0}", new string[] { time.ToString() });
           }
        }

        public void Start()
        {

            if (!this.Initialized)
            {
                this.SyncState = CacheSyncState.Idle;
                this.Initialized = true;
                this.InitializeTimer();
            }
        }

        public void Stop()
        {
            if (this.Initialized)
            {
                this.Initialized = false;
                this.SyncState = CacheSyncState.Idle;
                this.DisposeTimer();
            }
        }

        private void DisposeTimer()
        {
            this.SettingTimer.Stop();
            this.SettingTimer.Enabled = false;
            this.SettingTimer.Elapsed -= new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer = null;
            this.LogAction("Dispose Timer");
        }

        private void InitializeTimer()
        {
            this.SettingTimer = new ThreadTimer((long)(this.IntervalSeconds));
            this.SettingTimer.AutoReset = true;
            this.SettingTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer.Enabled = true;
            this.SettingTimer.Start();
            this.LogAction("Initialized Timer Interval:{0}", new string[] { this.SettingTimer.Interval.ToString() });
        }

        public void DoSync()
        {
            OnSyncTimer();
        }

        protected virtual void OnSyncTimer()
        {
            try
            {
                this.LogAction("OnSyncTimer Start");

                string[] list = GetTimedoutItems();
                if (list != null && list.Length > 0)
                {
                    OnSyncCompleted(new SyncTimeCompletedEventArgs(list));
                    this.LogAction("OnSync End, items removed:{0}", new string[] { list.Length.ToString() });
                }
            }
            catch (Exception ex)
            {
                this.LogAction("OnSync End error :" + ex.Message);

            }
        }

        #endregion

        #region LogAction
        protected virtual void LogAction(string text)
        {
            
        }

        protected virtual void LogAction(string text, params string[] args)
        {
          
        }
        #endregion

    }
}
