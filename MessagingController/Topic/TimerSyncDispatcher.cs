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
using Nistec.Caching.Remote;
using System.Collections.Concurrent;
using Nistec.Caching.Sync;
using Nistec.Caching.Server;
using System.Threading.Tasks;
using System.Threading;
using Nistec.Caching.Data;
using Nistec.Caching.Config;

namespace Nistec.Caching
{
    internal class TimerSyncDispatcher : IDisposable
    {
        #region memebers

        int synchronized;

        public static readonly TimerSyncDispatcher Instance = new TimerSyncDispatcher(true);

        private ThreadTimer SettingTimer;

        private ConcurrentDictionary<string, SyncTask> m_SyncItems;

        #endregion

        #region properties

        public int IntervalSeconds
        {
            get;
            internal set;
        }

        public bool IsRemote
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
        public CacheSyncState SyncState
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
        #endregion

        #region ctor

        private TimerSyncDispatcher(bool autoStart)
            : this(60, 10, true)
        {
            if (autoStart)
            {
                Start();
            }
        }

        public TimerSyncDispatcher(int intervalSeconds, int initialCapacity, bool isRemote)
        {
            
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            if (initialCapacity < 10)
                initialCapacity = 10;

            m_SyncItems = new ConcurrentDictionary<string, SyncTask>(concurrencyLevel, initialCapacity);

            this.IntervalSeconds = intervalSeconds;
            this.IsRemote = isRemote;
            this.Initialized = false;
            this.SyncState = CacheSyncState.Idle;
            this.LastSyncTime = DateTime.Now;
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized SyncTimerDispatcher");
        }

        public void Dispose()
        {
            if (SettingTimer != null)
            {
                DisposeTimer();
            }
 
        }
        #endregion

        #region events

        public event EventHandler SyncStarted;

        //public event SyncEntityTimeCompletedEventHandler SyncCompleted;

        protected virtual void OnSyncStarted(EventArgs e)
        {
            if (this.SyncStarted != null)
            {
                this.SyncStarted(this, e);
            }

            this.OnSyncTimer();

            //SyncBox.Instance.DoSyncAll();
        }

        //protected virtual void OnSyncCompleted(SyncEntityTimeCompletedEventArgs e)
        //{
        //    if (this.SyncCompleted != null)
        //    {
        //        this.SyncCompleted(this, e);
        //    }
        //}

        #endregion

        #region cache timeout

        public string GetItemKey(SyncTask item)
        {
            return item.ItemName;
        }

        public void Add(SyncTask item)
        {
            if (item == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(item.ItemName))
            {
                return;
            }
            m_SyncItems[item.ItemName] = item;
        }

        public bool Remove(SyncTask item)
        {
            if (item == null)
                return false;
            SyncTask syncItem;

            return m_SyncItems.TryRemove(item.ItemName, out syncItem);
        }

        public bool Remove(string itemName)
        {
            if (itemName == null)
                return false;
            SyncTask syncItem;
            return m_SyncItems.TryRemove(itemName, out syncItem);
        }
        public void Clear()
        {
                m_SyncItems.Clear();
        }

        public SyncTask[] GetTimedItems()
        {
            List<SyncTask> list = new List<SyncTask>();

            var dt = DateTime.Now;

            KeyValuePair<string, SyncTask>[] items = m_SyncItems.Where(dic => dt > dic.Value.GetNextTime()).ToArray();

            foreach (var item in items)
            {
                SyncTask syncItem = item.Value;
                
                if (syncItem.ShouldRun())
                {
                    list.Add(syncItem);
                    m_SyncItems[item.Key] = syncItem;
                }
            }

            return list.ToArray();
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
                this.LastSyncTime = DateTime.Now;
                this.OnSyncStarted(EventArgs.Empty);
                DateTime time = this.LastSyncTime.AddSeconds((double)this.IntervalSeconds);
                //this.NextSyncTime = time;
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
            this.LogAction(CacheAction.General, CacheActionState.None, "Dispose Timer");
        }

        private void InitializeTimer()
        {
            this.SettingTimer = new ThreadTimer((long)(this.IntervalSeconds * 1000));
            this.SettingTimer.AutoReset = true;
            this.SettingTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer.Enabled = true;
            this.SettingTimer.Start();
            this.LogAction(CacheAction.General, CacheActionState.None, "Initialized SyncTimer Interval:{0}", new string[] { this.SettingTimer.Interval.ToString() });
        }

        public void DoSync()
        {
            OnSyncTimer();
        }

        protected virtual void OnSyncTimer()
        {
            try
            {

                var syncBox = SyncBox.Instance;

                //0 indicates that the method is not in use.
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    bool enabletrigger=CacheSettings.EnableSyncTypeEventTrigger;

                    SyncTask[] list = GetTimedItems();
                    if (list != null && list.Length > 0)
                    {
                        foreach (SyncTask e in list)
                        {
                            if (e.Item.IsDisposed)
                            {
                                Remove(e);
                                this.LogAction(CacheAction.SyncTime, CacheActionState.Failed, "OnSyncTimer RenderTask error, Item is Disposed and removed from TimerSync.");
                            }
                            else
                            {

                                if (e.Entity == null)
                                {
                                    if (enabletrigger)
                                        syncBox.Add(new SyncBoxTask(e.Item, e.ItemName));
                                }
                                else
                                {
                                    syncBox.Add(new SyncBoxTask(e.Entity, e.Owner));
                                }

                            }
                            Thread.Sleep(10);
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                this.LogAction(CacheAction.SyncTime, CacheActionState.Error, "OnSync End error :" + ex.Message);

            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);

            }
        }

        #endregion

        #region LogAction
        protected virtual void LogAction(CacheAction action, CacheActionState state, string text)
        {
            if (IsRemote)
            {
                CacheLogger.Logger.LogAction(action, state, text);
            }
        }

        protected virtual void LogAction(CacheAction action, CacheActionState state, string text, params string[] args)
        {
            if (IsRemote)
            {
                CacheLogger.Logger.LogAction(action, state, text, args);
            }
        }
        #endregion

    }
}
