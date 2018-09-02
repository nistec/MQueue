using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Runtime;
using Nistec.Generic;
using Nistec.Serialization;

namespace Nistec.Messaging
{
    internal interface IQueuePerformance
    {
       

        /// <summary>
        ///  Sets the memory size as an atomic operation.
        /// </summary>
        /// <param name="memorySize"></param>
        void MemorySizeExchange(ref long memorySize);
        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        long GetMaxSize();

        /// <summary>
        /// Get thie sync interval in seconds.
        /// </summary>
        int IntervalSeconds { get; }
        /// <summary>
        /// Get indicate wether the cache item is initialized.
        /// </summary>
        bool Initialized { get; }
        /// <summary>
        /// Get indicate wether the cache item is remote cache.
        /// </summary>
        bool IsRemote { get; }
        
        
    }

    public enum QueueAgentType
    {
        MQueue
    }

    /// <summary>
    /// Represent a thread safe cache item performance counter.
    /// </summary>
    [Serializable]
    public class QueuePerformanceCounter
    {

        IQueuePerformance Owner;

        /// <summary>
        /// Initialize a new instance of performance counter.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="agentType"></param>
        /// <param name="name"></param>
        internal QueuePerformanceCounter(IQueuePerformance agent, QueueAgentType agentType, string name)
        {
            Owner = agent;
            MaxSize = Owner.GetMaxSize();
            AgentType = agentType;
            CounterName = name;
            StartTime = DateTime.Now;
            LastRequestTime = DateTime.Now;
            LastDequeueTime = DateTime.Now;
            InitStateCounter();
        }

        #region members

        /// <summary>
        /// Get Queue Agent Type.
        /// </summary>
        public QueueAgentType AgentType { get; internal set; }
        /// <summary>
        /// Get Counter Name.
        /// </summary>
        public string CounterName { get; internal set; }

        long _ItemsCount;
        /// <summary>
        /// Get Items count as an atomic operation.
        /// </summary>
        public long ItemsCount { get { return Interlocked.Read(ref _ItemsCount); } }

        long _EnqueueCount;
        /// <summary>
        /// Get Emqueued Items count as an atomic operation.
        /// </summary>
        public long EnqueueCount { get { return Interlocked.Read(ref _EnqueueCount); } }

        long _DequeueCount;
        /// <summary>
        /// Get Dequeued Items count as an atomic operation.
        /// </summary>
        public long DequeueCount { get { return Interlocked.Read(ref _DequeueCount); } }


        long _RequestCount;
        /// <summary>
        /// Get Request count as an atomic operation.
        /// </summary>
        public long RequestCount { get { return Interlocked.Read(ref _RequestCount); } }

        long _DequeueCountPerHour;
        /// <summary>
        /// Get Dequeue count per hour as an atomic operation.
        /// </summary>
        public long DequeueCountPerHour { get { return Interlocked.Read(ref _DequeueCountPerHour); } }

        long _DequeueCountPerDay;
        /// <summary>
        /// Get Dequeue count per day as an atomic operation.
        /// </summary>
        public long DequeueCountPerDay { get { return Interlocked.Read(ref _DequeueCountPerDay); } }

        long _DequeueCountPerMonth;
        /// <summary>
        /// Get Dequeue count per month as an atomic operation.
        /// </summary>
        public long DequeueCountPerMonth { get { return Interlocked.Read(ref _DequeueCountPerMonth); } }

        long _SyncCount;
        /// <summary>
        /// Get Sync count as an atomic operation.
        /// </summary>
        public long SyncCount { get { return Interlocked.Read(ref _SyncCount); } }

        /// <summary>
        /// Get Start Time.
        /// </summary>
        public DateTime StartTime { get; internal set; }
        /// <summary>
        /// Get the last time of request action.
        /// </summary>
        public DateTime LastRequestTime { get; internal set; }
        /// <summary>
        /// Get the last time of dequeue action.
        /// </summary>
        public DateTime LastDequeueTime { get; internal set; }
        /// <summary>
        /// Get Last Sync Time.
        /// </summary>
        public DateTime LastSyncTime { get; internal set; }
        /// <summary>
        /// Get Max Hit Per Minute.
        /// </summary>
        public int MaxHitPerMinute { get; internal set; }
        /// <summary>
        /// Get the avarage hit Per Minute.
        /// </summary>
        public int AvgHitPerMinute { get; internal set; }

        //float _AvgDequeueTime;
        ///// <summary>
        ///// Get avarage dequeue time.
        ///// </summary>
        //public float AvgDequeueTime
        //{
        //    get { return _AvgDequeueTime; }
        //}

        /// <summary>
        /// Get avarage sync time.
        /// </summary>
        public float AvgSyncTime
        {
            get { return (float)(SyncCount == 0 ? 0 : (float)(m_SyncTimeSum / SyncCount)); }
        }

        #endregion

        DateTime m_LastDequeueSycle;
        DateTime m_LastMinuteSycle;
        DateTime m_LastHourSycle;
        DateTime m_LastDaySycle;
        DateTime m_LastMonthSycle;
        //double m_DequeueTimeSum;
        int m_HitMinuteCounter;
        double m_SyncTimeSum;
        ConcurrentDictionary<MessageState, int> m_StateCounter;

        void InitStateCounter()
        {
            m_StateCounter = new ConcurrentDictionary<MessageState, int>();

            foreach (var state in Enum.GetValues(typeof(MessageState)))
            {
                m_StateCounter[(MessageState)state] = 0;
            }
        }

        internal void AddStateCounter(MessageState state)
        {
            m_StateCounter[state] += 1;
        }

        internal void AddRequest()
        {
            Interlocked.Increment(ref _RequestCount);
            LastRequestTime = DateTime.Now;
        }

        internal void AddEnqueue(int size)
        {
            Interlocked.Increment(ref _RequestCount);
            LastRequestTime = DateTime.Now;
            AddStateCounter(MessageState.Arrived);
            AddRemoveSizeAndCounter(size,1);
        }

        internal void AddDequeue(int size)
        {
            DateTime now = DateTime.Now;
            MessageState state = MessageState.Received;
  
            AddRemoveSizeAndCounter(size,-1);


            long dequeueCountPerHour = DequeueCountPerHour;

            if (now.Month != m_LastMonthSycle.Month)
            {
                m_LastMonthSycle = now;
                Interlocked.Exchange(ref _DequeueCountPerMonth, 0);
            }

            if (now.Day != m_LastDaySycle.Day)
            {
                m_LastDaySycle = now;
                Interlocked.Exchange(ref _DequeueCountPerDay, 0);
            }

            if (now.Hour != m_LastHourSycle.Hour)
            {
                Interlocked.Exchange(ref _DequeueCountPerHour, 0);
                //Interlocked.Exchange(ref m_DequeueTimeSum, 0);
                m_LastHourSycle = now;
            }

            LastDequeueTime = now;
            //m_DequeueTimeSum += dequeueTime;
            //if (dequeueCountPerHour > 0)
            //{
            //    _AvgDequeueTime =  (float)(m_DequeueTimeSum / dequeueCountPerHour);
            //}

            Interlocked.Increment(ref _DequeueCountPerHour);
            Interlocked.Increment(ref _DequeueCountPerDay);
            Interlocked.Increment(ref _DequeueCountPerMonth);
            Interlocked.Increment(ref m_HitMinuteCounter);
            AddStateCounter(state);
            //int avg = AvgHitPerMinute;

            if (now.Subtract(m_LastDequeueSycle).TotalSeconds > 1)
            {
                

                 int totalMinute =(int) (now - m_LastHourSycle).TotalMinutes;
                 if (totalMinute <= 0)
                     totalMinute = 1;

                 AvgHitPerMinute = (int)(DequeueCountPerHour / totalMinute);

                //if (avg > 0 && m_HitMinuteCounter > 0)
                //    AvgHitPerMinute = (avg + m_HitMinuteCounter) / 2;
                //else if (m_HitMinuteCounter > 0)
                //    AvgHitPerMinute = m_HitMinuteCounter;

 
                 MaxHitPerMinute = Math.Max( Math.Max(m_HitMinuteCounter,AvgHitPerMinute), MaxHitPerMinute);
                 if (now.Subtract(m_LastMinuteSycle).TotalMinutes > 1)
                 {
                     Interlocked.Exchange(ref m_HitMinuteCounter, 0);
                     m_LastMinuteSycle = now;
                 }
                                

                m_LastDequeueSycle = DateTime.Now;
            }
            //m_HitMinuteCounter++;
        }

        internal void AddSync(DateTime startTime)
        {
            LastSyncTime = DateTime.Now;
            Interlocked.Increment(ref _SyncCount);
            m_SyncTimeSum += LastSyncTime.Subtract(startTime).TotalMinutes;
        }


        /// <summary>
        /// Queue Performance Item Schema as <see cref="DataTable"/> class.
        /// </summary>
        /// <returns></returns>
        internal static DataTable QueuePerformanceSchema()
        {

            DataTable dt = new DataTable("QueuePerformance");
            dt.Columns.Add("AgentType", typeof(string));
            dt.Columns.Add("CounterName", typeof(string));
            dt.Columns.Add("ItemsCount", typeof(long));
            dt.Columns.Add("RequestCount", typeof(long));
            dt.Columns.Add("EnqueueCount", typeof(long));
            dt.Columns.Add("DequeueCount", typeof(long));
            dt.Columns.Add("DequeueCountPerHour", typeof(long));
            dt.Columns.Add("DequeueCountPerDay", typeof(long));
            dt.Columns.Add("DequeueCountPerMonth", typeof(long));
            dt.Columns.Add("SyncCount", typeof(long));
            dt.Columns.Add("StartTime", typeof(DateTime));
            dt.Columns.Add("LastRequestTime", typeof(DateTime));
            dt.Columns.Add("LastDequeueTime", typeof(DateTime));
            dt.Columns.Add("LastSyncTime", typeof(DateTime));
            dt.Columns.Add("MaxHitPerMinute", typeof(int));
            dt.Columns.Add("AvgHitPerMinute", typeof(int));
            //dt.Columns.Add("AvgDequeueTime", typeof(float));
            dt.Columns.Add("AvgSyncTime", typeof(float));
            dt.Columns.Add("MaxSize", typeof(long));
            dt.Columns.Add("MemorySize", typeof(long));
            dt.Columns.Add("FreeSize", typeof(long));
            dt.Columns.Add("MemoryUsage", typeof(long));
            return dt.Clone();
        }

        /// <summary>
        /// Get cache properties as dictionary.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetPerformanceReport()
        {
            Dictionary<string, object> prop = new Dictionary<string, object>();

            
            prop["AgentType"] = AgentType;
            prop["CounterName"] = CounterName;
            prop["ItemsCount"] = ItemsCount;
            prop["RequestCount"] = RequestCount;
            prop["EnqueueCount"] = RequestCount;
            prop["DequeueCount"] = DequeueCount;
            prop["DequeueCountPerHour"] = DequeueCountPerHour;
            prop["DequeueCountPerDay"] = DequeueCountPerDay;
            prop["DequeueCountPerMonth"] = DequeueCountPerMonth;
            prop["SyncCount"] = SyncCount;
            prop["StartTime"] = StartTime;
            prop["LastRequestTime"] = LastRequestTime;
            prop["LastDequeueTime"] = LastDequeueTime;
            prop["LastSyncTime"] = LastSyncTime;
            prop["MaxHitPerMinute"] = MaxHitPerMinute;
            prop["AvgHitPerMinute"] = AvgHitPerMinute;
            //prop["AvgDequeueTime"] = AvgDequeueTime;
            prop["AvgSyncTime"] = AvgSyncTime;
            prop["MaxSize"] = MaxSize;
            prop["MemorySize"] = MemoSize;
            prop["FreeSize"] = FreeSize;
            prop["MemoryUsage"] = GetMemoryUsage();

            prop["IntervalMinute"] = Owner.IntervalSeconds;
            prop["Initialized"] = Owner.Initialized;
            prop["IsRemote"] = Owner.IsRemote;

            return prop;
        }

        /// <summary>
        /// Get view as <see cref="DataTable"/>
        /// </summary>
        public DataTable GetDataView()
        {
            DataTable dt = QueuePerformanceSchema();
            dt.Rows.Add(GetItemArray());
            return dt;
        }

        /// <summary>
        /// Get array of performance properties.
        /// </summary>
        /// <returns></returns>
        public object[] GetItemArray()
        {
            return new object[]{
            AgentType.ToString(),
            CounterName,
            ItemsCount,
            RequestCount,
            EnqueueCount,
            DequeueCount,
            DequeueCountPerHour,
            DequeueCountPerDay,
            DequeueCountPerMonth,
            SyncCount,
            StartTime,
            LastRequestTime,
            LastDequeueTime,
            LastSyncTime,
            MaxHitPerMinute,
            AvgHitPerMinute,
            //AvgDequeueTime,
            AvgSyncTime,
            MaxSize,
            MemoSize,
            FreeSize,
            GetMemoryUsage()};

        }

        #region Size properties

        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        public long MaxSize { get; internal set; }

        long _MemoSize;
        /// <summary>
        /// Get memory size for current item in bytes as an atomic operation.
        /// </summary>
        public long MemoSize
        {
            get { return Interlocked.Read(ref _MemoSize); }
        }
        /// <summary>
        /// Get the free size memory in bytes for current item as an atomic operation.
        /// </summary>
        public long FreeSize
        {
            get { return MaxSize - MemoSize; }
        }

        //Count = cache.Count;
        //   FreeSize = cache.FreeSize;// / 1024;
        //   MaxSize = cache.MaxSize;// / 1024;
        //   Usage = cache.Usage;// / 1024;

        /// <summary>
        /// Refresh memory size async.
        /// </summary>
        public void RefreshSize()
        {
            Task.Factory.StartNew(() => RefreshSizeInternal());
        }

        /// <summary>
        /// Refresh memory size.
        /// </summary>
        internal void RefreshSizeInternal()
        {
            Owner.MemorySizeExchange(ref _MemoSize);
            //Interlocked.Exchange(ref _MemoSize, size);
        }

        internal MessageState SizeValidate(long newSize)
        {
            long freeSize = FreeSize;
            return freeSize > newSize ? MessageState.Ok : MessageState.CapacityExeeded;
        }

        internal MessageState AddRemoveSizeAndCounter(long size, int oprtator)
        {
            long memo = 0;

            Interlocked.Add(ref _ItemsCount, oprtator);
            memo = Interlocked.Add(ref _MemoSize, size * oprtator);
            return ExchangeComplete(size, memo);
        }

        internal MessageState ExchangeSizeAndCounter(long size, int itemsCount)
        {
            long memo = 0;

            Interlocked.Exchange(ref _ItemsCount, itemsCount);
            memo = Interlocked.Exchange(ref _MemoSize, size);

            return ExchangeComplete(size, memo);
        }

        MessageState ExchangeComplete(long size, long memo)
        {

            //long memo = Interlocked.Read(ref _MemoSize);
            if (memo < 0)
            {
                RefreshSizeInternal();
                memo = Interlocked.Add(ref _MemoSize, size);
            }
            else if (size > 0 && memo > MaxSize)
            {
                RefreshSizeInternal();
                memo = Interlocked.Add(ref _MemoSize, size);
                if ((memo) > MaxSize)
                {
                    throw new MessageException(MessageState.CapacityExeeded, this.CounterName + " Capacity Exeeded !!!");

                    //QueueLogger.Logger.LogAction(QueueAction.AddItem, QueueActionState.None, this.CounterName + " memory is full!!!");
                    //this.OnQueueException("The Queue memory is full!!!", QueueErrors.ErrorSetValue);
                    //return MessageState.QueueIsFull;
                }
            }

            return MessageState.Ok;
        }

        #endregion


        #region static

        /// <summary>
        /// Convert name argument to <see cref="QueueAgentType"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static QueueAgentType GetAgent(string name)
        {
           return EnumExtension.Parse<QueueAgentType>(name, QueueAgentType.MQueue);
        }

        /// <summary>
        /// Get memory usage in bytes.
        /// </summary>
        /// <returns></returns>
        public static long GetMemoryUsage()
        {
            string execName = SysNet.GetExecutingAssemblyName();// System.Reflection.Assembly.GetExecutingAssembly().GetName().FullName;
            System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName(execName);
            long usage = 0;
            if (process == null)
                return 0;
            for (int i = 0; i < process.Length; i++)
            {
                usage += (int)((int)process[i].WorkingSet64);
            }

            return usage;
        }
        #endregion
    }

     /// <summary>
    /// Represent cache performance counter report.
    /// </summary>
    [Serializable]
    public class QueuePerformanceReport
    {
        /// <summary>
        /// Initialize a new instance of cache performance report.
        /// </summary>
        public QueuePerformanceReport()
        {
            dtReport = QueuePerformanceCounter.QueuePerformanceSchema();
            CounterName = "SummarizeReport";
        }

        /// <summary>
        /// Initialize a new instance of cache performance report.
        /// </summary>
        /// <param name="agent"></param>
        public QueuePerformanceReport(QueueAgentType agent)
        {
            dtReport = QueuePerformanceCounter.QueuePerformanceSchema();
            CounterName = agent.ToString();
        }

        internal void InitReport()
        {
            dtReport.Clear();
            ItemsCount = 0;
            RequestCount = 0;
            EnqueueCount = 0;
            DequeueCount = 0;
            DequeueCountPerHour = 0;
            DequeueCountPerDay = 0;
            DequeueCountPerMonth = 0;
            FreeSize = 0;
            MaxSize = 0;
            MemoSize = 0;
            SyncCount = 0;
            //AvgDequeueTime = 0;
            AvgSyncTime = 0;
            AvgHitPerMinute = 0;
            MaxHitPerMinute = 0;
            StartTime = DateTime.Now.AddHours(-1);
            LastRequestTime = DateTime.Now.AddHours(-1);
            LastDequeueTime = DateTime.Now.AddHours(-1);
            LastSyncTime = DateTime.Now.AddHours(-1);
        }

        internal void AddItemReport(QueuePerformanceCounter agent)
        {
           
            dtReport.Rows.Add(agent.GetItemArray());

            ItemsCount += agent.ItemsCount;
            RequestCount += agent.RequestCount;
            EnqueueCount += agent.EnqueueCount;
            DequeueCount += agent.DequeueCount;
            DequeueCountPerHour += agent.DequeueCountPerHour;
            DequeueCountPerDay += agent.DequeueCountPerDay;
            DequeueCountPerMonth += agent.DequeueCountPerMonth;
            FreeSize += agent.FreeSize;
            MaxSize += agent.MaxSize;
            MemoSize += agent.MemoSize;
            SyncCount += agent.SyncCount;

            //if (AvgDequeueTime > 0 && agent.AvgDequeueTime > 0)
            //    AvgDequeueTime = (AvgDequeueTime + agent.AvgDequeueTime) / 2;
            //else if (agent.AvgDequeueTime > 0)
            //    AvgDequeueTime = agent.AvgDequeueTime;

            if (AvgSyncTime > 0 && agent.AvgSyncTime > 0)
                AvgSyncTime = (AvgSyncTime + agent.AvgSyncTime) / 2;
            else if (agent.AvgSyncTime > 0)
                AvgSyncTime = agent.AvgSyncTime;

            if (AvgHitPerMinute > 0 && agent.AvgHitPerMinute > 0)
                AvgHitPerMinute = (AvgHitPerMinute + agent.AvgHitPerMinute) / 2;
            else if (agent.AvgHitPerMinute > 0)
                AvgHitPerMinute = agent.AvgHitPerMinute;

            MaxHitPerMinute = Math.Max(MaxHitPerMinute, agent.MaxHitPerMinute);

            StartTime = agent.StartTime > StartTime ? StartTime : agent.StartTime;
            LastRequestTime = agent.LastRequestTime < LastRequestTime ? LastRequestTime : agent.LastRequestTime;
            LastDequeueTime = agent.LastDequeueTime < LastDequeueTime ? LastDequeueTime : agent.LastDequeueTime;
            LastSyncTime = agent.LastSyncTime < LastSyncTime ? LastSyncTime : agent.LastSyncTime;

            //dtSum = QueuePerformanceCounter.QueuePerformanceSchema();

           
        }

        internal void AddTotalReport()
        {
 
            dtReport.Rows.Add(new object[]{
            "Report",
            "Summarize",
            ItemsCount,
            RequestCount,
            EnqueueCount,
            DequeueCount,
            DequeueCountPerHour,
            DequeueCountPerDay,
            DequeueCountPerMonth,
            SyncCount,
            StartTime,
            LastRequestTime,
            LastDequeueTime,
            LastSyncTime,
            MaxHitPerMinute,
            AvgHitPerMinute,
            //AvgDequeueTime,
            AvgSyncTime,
            MaxSize,
            MemoSize,
            FreeSize,
            QueuePerformanceCounter.GetMemoryUsage()});
        }

        #region members

        /// <summary>
        /// Get Counter Name.
        /// </summary>
        public string CounterName { get; internal set; }
                
        /// <summary>
        /// Get Items count as an atomic operation.
        /// </summary>
        public long ItemsCount { get; private set;  }

        
        /// <summary>
        /// Get Request count as an atomic operation.
        /// </summary>
        public long RequestCount {  get; private set; }

        /// <summary>
        /// Get Enqueued count per interval as an atomic operation.
        /// </summary>
        public long EnqueueCount { get; private set; }

        /// <summary>
        /// Get Dequeue count per interval as an atomic operation.
        /// </summary>
        public long DequeueCount { get; private set; }
      

        /// <summary>
        /// Get Dequeue count per hour as an atomic operation.
        /// </summary>
        public long DequeueCountPerHour { get; private set; }
      
        /// <summary>
        /// Get Dequeue count per day as an atomic operation.
        /// </summary>
        public long DequeueCountPerDay { get; private set; }

        
        /// <summary>
        /// Get Dequeue count per month as an atomic operation.
        /// </summary>
        public long DequeueCountPerMonth { get; private set; }

        /// <summary>
        /// Get Sync count as an atomic operation.
        /// </summary>
        public long SyncCount { get; private set; }

        /// <summary>
        /// Get Start Time.
        /// </summary>
        public DateTime StartTime { get; internal set; }
        /// <summary>
        /// Get the last time of request action.
        /// </summary>
        public DateTime LastRequestTime { get; internal set; }
        /// <summary>
        /// Get the last time of dequeue action.
        /// </summary>
        public DateTime LastDequeueTime { get; internal set; }
        /// <summary>
        /// Get Last Sync Time.
        /// </summary>
        public DateTime LastSyncTime { get; internal set; }
        /// <summary>
        /// Get Max Hit Per Minute.
        /// </summary>
        public int MaxHitPerMinute { get; internal set; }
        /// <summary>
        /// Get the avarage hit Per Minute.
        /// </summary>
        public int AvgHitPerMinute { get; internal set; }
        /// <summary>
        /// Get the max size defined by user for current item.
        /// </summary>
        public long MaxSize { get; internal set; }

       
        /// <summary>
        /// Get memory size for current item in bytes as an atomic operation.
        /// </summary>
        public long MemoSize
        {
            get;
            internal set; 
        }
        /// <summary>
        /// Get the free size memory in bytes for current item as an atomic operation.
        /// </summary>
        public long FreeSize
        {
            get;
            internal set; 
        }

        ///// <summary>
        ///// Get avarage dequeue time.
        ///// </summary>
        //public float AvgDequeueTime
        //{
        //    get; internal set; 
        //}

        /// <summary>
        /// Get avarage sync time.
        /// </summary>
        public float AvgSyncTime
        {
           get; internal set; 
        }

        #endregion

        //QueuePerformanceCounter sumReport;

        DataTable dtReport;
        //DataTable dtSum;

        /// <summary>
        /// Get Queue prformance report
        /// </summary>
        /// <returns></returns>
        //[EntitySerialize]
        public DataTable PerformanceReport
        {
            get { return dtReport; }
        }

        ///// <summary>
        ///// Get Queue prformance summarize report.
        ///// </summary>
        ///// <returns></returns>
        //[EntitySerialize]
        //public DataTable PerformanceSummarizeReport
        //{
        //    get { return dtSum; }
        //}

    }

}
