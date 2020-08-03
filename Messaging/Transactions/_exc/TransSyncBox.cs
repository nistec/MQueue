using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Threading;
using Nistec.Generic;

namespace Nistec.Messaging.Transactions
{
    public interface ISyncTask
    {
        void DoSync();
    }

    internal class TransSyncBox : IDisposable
    {
        #region memebers

        int synchronized;

        public static readonly TransSyncBox Instance = new TransSyncBox(true, true);
        private ConcurrentQueue<ISyncTask> m_SynBox;
        private bool KeepAlive = false;

        #endregion

        #region properties

        /// <summary>
        /// Get indicate whether the sync box is remote.
        /// </summary>
        public bool IsRemote
        {
            get;
            internal set;
        }

        /// <summary>
        /// Get indicate whether the sync box intialized.
        /// </summary>
        public bool Initialized
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of elements contained in the SyncTaskBox. 
        /// </summary>
        public int Count
        {
            get { return m_SynBox.Count; }
        }
      
        #endregion

        #region ctor

        public TransSyncBox(bool autoStart, bool isRemote)
        {
            m_SynBox = new ConcurrentQueue<ISyncTask>();
            IsRemote = isRemote;
            this.Initialized = true;
            Netlog.Debug("Initialized SynTaskBox");

            if (autoStart)
            {
                Start();
            }
        }

        public void Dispose()
        {
           
        }
        #endregion

        #region queue methods

        //public void Add(SyncTask task, DataSyncEntity entity, IDataCache owner)
        //{
        //    if (task == null || entity == null || owner == null)
        //    {

        //        this.LogAction(CacheAction.SyncTime, CacheActionState.Failed, "SyncTaskBox can not add task null!");
        //        return;
        //    }
        //    Add(new SyncBoxTask()
        //        {
        //            Entity = entity,
        //            //ItemName = task.ItemName,
        //            Owner = owner
        //            //TaskItem = task.Item
        //        });
        //}

        public void Add(ISyncTask item)
        {
            if (item == null)
            {
                Netlog.Warn("SyncTaskBox can not add task null!");
                return;
            }
         
            m_SynBox.Enqueue(item);
            Netlog.DebugFormat("SyncTaskBox Added SyncBoxTask {0}", item.ItemName);
        }

        private ISyncTask Get()
        {
            ISyncTask res = null;
             m_SynBox.TryDequeue(out res);
            return res;
        }

        
        public void Clear()
        {
            while (m_SynBox.Count > 0)
            {
                Get();
            }
            
        }

        #endregion

        #region Timer Sync

       
        public void Start()
        {

            if (!this.Initialized)
            {
                throw new Exception("The SyncTaskBox not initialized!");
            }

            if (KeepAlive)
                return;
            Netlog.Debug("SyncTaskBox Started...");

            KeepAlive = true;
            Thread.Sleep(1000);
            Thread th = new Thread(new ThreadStart(InternalStart));
            th.IsBackground = true;
            th.Start();
        }

        public void Stop()
        {
            KeepAlive = false;
            this.Initialized = false;
            Netlog.Debug("SyncTaskBox Stoped");
        }


        private void InternalStart()
        {
            while (KeepAlive)
            {
                DoSync();
                Thread.Sleep(1000);
            }
            Netlog.Warn("Initialized SyncTaskBox Not keep alive");
        }

        public void DoSync()
        {
            OnSyncTask();
        }

        protected virtual void OnSyncTask()
        {
            try
            {
                //this.LogAction(CacheAction.SyncTime, CacheActionState.Debug, "SyncTaskBox OnSyncTask... ");

                //0 indicates that the method is not in use.
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    ISyncTask syncTask = null;
                    if (m_SynBox.TryDequeue(out syncTask))
                    {
                        //RenderTask(syncTask);
                        
                        syncTask.DoSync();

                                               

                        //this.LogAction(CacheAction.SyncTime, CacheActionState.Debug, "SyncTaskBox OnSyncTask RenderTask Start {0}", syncTask.ItemName);

                        //Task task = Task.Factory.StartNew(() => syncTask.DoSynchronize());
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Netlog.Exception("SyncTaskBox OnSyncTask End error :" , ex);

            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);
            }
        }

        #endregion

       

    }
}
