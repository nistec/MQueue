using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Nistec.Threading;
using System.Collections.Concurrent;
using System.IO;
using Nistec.Runtime;
using Nistec.Runtime.Advanced;

namespace Nistec.Messaging.Transactions
{

    internal class TransactionDispatcher : SyncTimerDispatcher<TransactionItem> //IDisposable
    {
        SyncQueueBox<TransactionItem> m_SyncBox;
        internal SyncQueueBox<TransactionItem> SyncBox
        {
            get
            {
                return m_SyncBox;
            }
        }

        protected override void OnSyncItemCompleted(SyncItemEventArgs<TransactionItem> e)
        {
            base.OnSyncItemCompleted(e);

            e.Item.SyncExpired();

            SyncBox.Add(e.Item);
        }

        #region members
        //private ThreadTimer SettingTimer;
        //private ConcurrentDictionary<Guid, DateTime> m_Timer;
        //private ConcurrentDictionary<Guid, TransactionItem> m_Trans;
        #endregion

        #region properties
        /*
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
        public TransSyncState SyncState
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
         */ 
        #endregion

        #region ctor
        public TransactionDispatcher()
            : this((int)TimeSpan.FromMinutes(1).TotalSeconds, 100, true)
        {

        }

        public TransactionDispatcher(int intervalSeconds, int initialCapacity, bool isRemote)
            : base(intervalSeconds, initialCapacity, isRemote)
        {
            m_SyncBox = new SyncQueueBox<TransactionItem>(isRemote);
            m_SyncBox.SyncItemCompleted += new SyncItemEventHandler<TransactionItem>(m_SyncBox_SyncItemCompleted);
            //int numProcs = Environment.ProcessorCount;
            //int concurrencyLevel = numProcs * 2;
            //if (initialCapacity < 100)
            //    initialCapacity = 101;

            ////m_Timer = new ConcurrentDictionary<Guid, DateTime>(concurrencyLevel, initialCapacity);
            //m_Trans = new ConcurrentDictionary<Guid, TransactionItem>(concurrencyLevel, initialCapacity);

            //this.IntervalSeconds = intervalSeconds;
            //this.IsRemote = isRemote;
            //this.Initialized = false;
            //this.SyncState = TransSyncState.Idle;
            //this.LastSyncTime = DateTime.Now;
        }

        void m_SyncBox_SyncItemCompleted(object sender, SyncItemEventArgs<TransactionItem> e)
        {
            e.Item.DoSync();
        }

        //public void Dispose()
        //{
        //    if (SettingTimer != null)
        //    {
        //        DisposeTimer();
        //    }
        //}
        #endregion

        #region events
        /*
        public event EventHandler SyncStarted;

        public event SyncTimerEventHandler SyncCompleted;

        protected virtual void OnSyncStarted(EventArgs e)
        {
            this.OnSyncTimer();

            if (this.SyncStarted != null)
            {
                this.SyncStarted(this, e);
            }
        }

        protected virtual void OnSyncCompleted(SyncTimerEventArgs e)
        {
            if (this.SyncCompleted != null)
            {
                this.SyncCompleted(this, e);
            }
        }
        */
        #endregion

        #region cache timeout
        /*
        public void Add(TransactionItem item)//, bool chekExists = false)
        {
            if (item.HasTimeout)
            {
                m_Timer[item.ItemId] = item.ExpirationTime;
            }
            m_Trans[item.ItemId] = item;
        }

        public bool Remove(Guid key)
        {
            if (key == Guid.Empty)
                return false;

            TransactionItem item;
            if (m_Trans.TryRemove(key, out item))
            {
                DateTime time;
                m_Timer.TryRemove(key, out time);
                return true;
            }
            return false;
        }

        public TransactionItem Get(Guid key)
        {
            if (key == Guid.Empty)
                return null;

            TransactionItem item;
            if (m_Trans.TryRemove(key, out item))
            {
                DateTime time;
                m_Timer.TryRemove(key, out time);
            }
            return item;
        }

        
        public void Update(Guid key)
        {
            if (key== Guid.Empty)
                return;
            m_Timer[key] = DateTime.Now;
        }

        public Dictionary<Guid, TransactionItem> Copy()
        {
            Dictionary<Guid, TransactionItem> copy = null;

            copy = new Dictionary<Guid, TransactionItem>(m_Trans);

            return copy;
        }


        public void Clear()
        {
            m_Trans.Clear();
            m_Timer.Clear();
        }

        public Guid[] GetTimedoutItems()
        {
            List<Guid> list = new List<Guid>();

            TimeSpan ts = TimeSpan.FromMinutes(1);

            KeyValuePair<Guid, DateTime>[] items = m_Timer.Where(dic => ts < DateTime.Now.Subtract(dic.Value)).ToArray();

            foreach (var item in items)
            {
                list.Add(item.Key);
            }


            return list.ToArray();
        }
        */

        #endregion

        #region Timer Sync
        /*
        internal void SetCacheSyncState(TransSyncState state)
        {
            this.SyncState = state;
        }

        private void SettingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Initialized && (this.SyncState == TransSyncState.Idle))
            {
                this.LogAction(false, "Synchronize Start");
                this.LastSyncTime = DateTime.Now;
                this.OnSyncStarted(EventArgs.Empty);
                DateTime time = this.LastSyncTime.AddSeconds((double)this.IntervalSeconds);
                this.LogAction(false, "Synchronize End, Next Sync:{0}", new string[] { time.ToString() });
           }
        }

        public void Start()
        {

            if (!this.Initialized)
            {
                this.SyncState = TransSyncState.Idle;
                this.Initialized = true;
                this.InitializeTimer();
                //this.OnCacheStateChanged(EventArgs.Empty);
            }
        }

        public void Stop()
        {
            if (this.Initialized)
            {
                this.Initialized = false;
                this.SyncState = TransSyncState.Idle;
                this.DisposeTimer();
                //this.OnCacheStateChanged(EventArgs.Empty);
            }
        }

        private void DisposeTimer()
        {
            this.SettingTimer.Stop();
            this.SettingTimer.Enabled = false;
            this.SettingTimer.Elapsed -= new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer = null;
            this.LogAction(false, "Dispose Timer");
        }

        private void InitializeTimer()
        {
            //this.LogAction(CacheAction.General, "Initialized Timer Interval:{0}", new string[] { _interval.ToString() });
            this.SettingTimer = new ThreadTimer((long)(this.IntervalSeconds * 1000));
            this.SettingTimer.AutoReset = true;
            this.SettingTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer.Enabled = true;
            this.SettingTimer.Start();
            this.LogAction(false, "Initialized Timer Interval:{0}", new string[] { this.SettingTimer.Interval.ToString() });
        }

        public void DoSync()
        {
            OnSyncTimer();
        }

        protected virtual void OnSyncTimer()
        {
            try
            {
                this.LogAction(false, "OnSyncTimer Start");

                Guid[] list = GetTimedoutItems();
                if (list != null && list.Length > 0)
                {
                    OnSyncCompleted(new SyncTimerEventArgs(list));
                    this.LogAction(false, "OnSync End, items removed:{0}", new string[] { list.Length.ToString() });
                }
            }
            catch (Exception ex)
            {
                this.LogAction(true, "OnSync End error :" + ex.Message);
            }
        }
        */
        #endregion

        #region LogAction
        /*
        protected virtual void LogAction(bool isError, string text)
        {
            if (IsRemote)
            {
                //CacheLogger.Logger.LogAction(action, state, text);
            }
        }

        protected virtual void LogAction(bool isError, string text, params string[] args)
        {
            if (IsRemote)
            {
                //CacheLogger.Logger.LogAction(action, state, text, args);
            }
        }
         */ 
        #endregion

    }
}
