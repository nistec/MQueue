using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Messaging;

using Nistec.Threading;
using Nistec.Data.SqlClient;
using Nistec.Data;
using System.Data;
using System.Data.SqlClient;
using Nistec.Runtime;
using System.IO;

namespace Nistec.Legacy
{
    
  
    /// <summary>
    /// AsyncQueueHandlerBase
    /// </summary>
    [Serializable]
    public abstract class McQueueSys : QueueBase
    {

        internal bool recoverable;
        //CoverAssist coverAssist;
        private string  m_StartupPath= "";
        private string m_QueuesPath = "";

     
        private void CurrentDomain_UnhandledException(object sender,UnhandledExceptionEventArgs e)
        {
            McError.DumpError((Exception)e.ExceptionObject,new System.Diagnostics.StackTrace());
        }

        private string GetFilename(string itemId)
        {
            return m_QueuesPath + QueueName + "\\" + itemId + ".mcq";
        }
        private string GetQueuePath()
        {
            return m_QueuesPath + QueueName + "\\";
        }
        private string GetRelayPath()
        {
            return m_QueuesPath + QueueName + "\\Relay\\";
        }
        private string GetBackupPath()
        {
            return m_QueuesPath + QueueName + "\\Backup\\";
        }

		#region Constructor
        /// <summary>
        /// AsyncQueue Ctor
        /// </summary>
        /// <param name="mqp"></param>
        public McQueueSys(McQueueProperties mqp)
            : base(mqp)
        {
            recoverable = false;

             // Add unhandled exception handler
            System.AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // Get startup path
            m_StartupPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\";
            m_QueuesPath = m_StartupPath + "Queues\\";

            // Set error file locaton
            McError.ErrorFilePath = m_StartupPath;

            if (CoverMode > Messaging.CoverMode.Memory)
            {
                recoverable = true;

                if (CoverMode == Messaging.CoverMode.FileStream)
                {

                    CleanFolder();
                    UNLOCK();

                }
                else
                {
                    throw new Exception("CoverMode not supported");

                }
                //else if (!string.IsNullOrEmpty(mqp.ConnectionString))
                //{
                //    Console.WriteLine("Init McQueueCover " + mqp);
                //    connection = mqp.ConnectionString;
                //    if (Provider == QueueProvider.SqlServer && !connection.Contains("Max Pool Size"))
                //    {
                //        connection += ";Max Pool Size=250";
                //    }
                //    coverAssist = new CoverAssist(connection);
                //    coverAssist.Start();
                //}
            }
        }

      

 
        /// <summary>
        /// Dispose
        /// </summary>
        public new void Dispose()
        {
            //if (coverAssist != null)
            //{
            //    coverAssist.Stop();
            //}
            base.Dispose();
        }

        internal void InitRecoverQueue(int intervalMinuteRecover)
        {

            Console.WriteLine("Init RecoverQueue");

            try
            {
                if (CoverMode == CoverMode.FileSystem)
                {
                    string path = GetQueuePath();
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                        Console.WriteLine("Create McQueue Folder: " + path);
                    }
                    AsyncReEnqueueItems();

                }
                else
                {
                    //ClearFinalItems();

                    AsyncReEnqueueItems();// ReEnqueueQueueItems();//intervalMinuteRecover);
                }
                if (IsTrans)
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(QueueName + " Error:" + ex.Message);

            }
        }

     
        #endregion

        #region Invoke re enqueue

        private void CleanFolder()
        {
            string path = GetQueuePath();
            string pathrelay = GetRelayPath();
            string pathback = GetBackupPath();

            if (Directory.Exists(path))
            {

                if (!Directory.Exists(pathrelay))
                {
                    Directory.CreateDirectory(pathrelay);
                }

                if (!Directory.Exists(pathback))
                {
                    Directory.CreateDirectory(pathback);
                }
                //clean relay files to backup
                string[] relays = Directory.GetFiles(pathrelay, "*.mcq");
                if (relays != null)
                {
                    foreach (string rely in relays)
                    {
                        string relyID = Path.GetFileNameWithoutExtension(rely);
                        string backfile = IoHelper.PathFix(pathback + relyID + ".mcq");
                        IoHelper.MoveFile(rely, backfile);
                    }

                    //Netlog.InfoFormat("CleanFolder Buckup files: {0} ", relays.Length);
                }

                //clean folder items and move them to relay
                string[] messages = Directory.GetFiles(path, "*.mcq");
                if (messages != null)
                {
                    foreach (string message in messages)
                    {
                        string messageID = Path.GetFileNameWithoutExtension(message);
                        string newfile = IoHelper.PathFix(pathrelay + messageID + ".mcq");
                        IoHelper.MoveFile(message, newfile);
                    }
                    //Netlog.InfoFormat("CleanFolder items files: {0} ", messages.Length);
                }
            }
        }

        private void ReEnqueueFiles()
        {
            if (reEnqueueItems)
                return;
            try
            {
                Console.WriteLine("Start ReEnqueueQueueItems");

                reEnqueueItems = true;

                string path =GetRelayPath();

                if (Directory.Exists(path))
                {
                    string[] messages = Directory.GetFiles(path, "*.mcq");
                    if (messages == null || messages.Length == 0)
                    {
                        return;
                    }

                    Console.WriteLine("{0} items found to ReEnqueue", messages.Length);

                    Netlog.InfoFormat("ReEnqueueFiles: {0} ", messages.Length);


                    foreach (string message in messages)
                    {
                        //while (this.Count > 1000)
                        //{

                        //    Thread.Sleep(1000);
                        //}

                        QueueItem item = QueueItem.ReadFile(message);
                        if (item != null)
                        {
                            Enqueue(item);
                        }
                        IoHelper.DeleteFile(message);
                        Thread.Sleep(100);
                    }
                    Netlog.Info("ReEnqueueFiles finished. ");
                }

            }
            catch (Exception ex)
            {
                string s = ex.Message;

            }
            finally
            {
                reEnqueueItems = false;
            }
        }
/*
        private void ReEnqueueDB()
        {
            if (reEnqueueItems)
                return;
            try
            {
                Console.WriteLine("Start ReEnqueueQueueItems");

                reEnqueueItems = true;
                DataTable dt = null;
                using (Command cmd = new Command(this.ConnectionString))
                {
                    //cmd.ExecuteCmd(string.Format(SQLCMD.SqlSelectItemsTimedOut, m_QueueName, m_Server));
                    dt = cmd.Execute_DataTable("QueueItems", string.Format(SQLCMD.SqlSelectReEnqueueItemsTimeOut, m_QueueName, TimeStarted.ToString(DateFormat), m_Server));
                }
                UNLOCK();
                if (dt == null || dt.Rows.Count == 0)
                {
                    Console.WriteLine("No items found to ReEnqueue");
                    return;
                }
                List<IQueueItem> items = DataTableToQueueItem(dt);

                Console.WriteLine("{0} items found to ReEnqueue", items.Count);

                foreach (IQueueItem item in items)
                {
                    ReEnqueue(item);
                    ////TODO:init to trans list
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;

            }
            finally
            {
                reEnqueueItems = false;
            }
        }
*/

        private void AsyncReEnqueueItems()
        {
            Thread th = null;

            if (CoverMode == CoverMode.FileSystem)
            {
                th = new Thread(new ThreadStart(ReEnqueueFiles));
            }
            else
            {
                return;
               //th= new Thread(new ThreadStart(ReEnqueueDB));
            }
            th.Start();
        }
        #endregion

        #region Cover items

        /// <summary>
        /// CompletedMessage
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="status"></param>
        /// <param name="hasAttach"></param>
        public override void Completed(Guid ItemId, int status, bool hasAttach)
        {
            if (!recoverable)
                return;
            if (CoverMode == Messaging.CoverMode.FileStream)
            {
                IoHelper.DeleteFile(GetFilename(ItemId.ToString()));

            }
            //else if (CoverMode != CoverMode.LogNoState)//CoverMode > CoverMode.None)
            //{
            //    using (Command cmd = new Command(this.ConnectionString))
            //    {
            //        cmd.DqueueStateCommand(ItemId, (ItemState)status, CoverMode, hasAttach);
            //    }

            //}
        }


        internal void DeleteCoverItem(IQueueItem item)
        {
            if (!recoverable)
                return;
            if (CoverMode == Messaging.CoverMode.FileStream)
            {

                IoHelper.DeleteFile(GetFilename(item.ItemId.ToString()));

            }
            else
            {
                //using (Command cmd = new Command(this.ConnectionString))
                //{
                //    cmd.ExecuteCmd(string.Format(SQLCMD.SqlRemoveItem, item.ItemId));
                //}
            }
        }




        internal void OnCoverQueueItem(IQueueItem item, Messaging.ItemState status)//, int retry)
        {
            if (!recoverable)
                return;
            int doCover = 0;

            switch (status)
            {
                case Messaging.ItemState.Hold:
                case Messaging.ItemState.Enqueue:
                    if (item.Retry > 0) return;
                    doCover = 1;
                   break;
                case Messaging.ItemState.Dequeue:
                default:
                   //if (CoverMode == CoverMode.LogNoState)
                   //    return;
                   doCover = 2;
                    break;

            }
                     

            if (CoverMode == Messaging.CoverMode.FileStream)
            {
                if (doCover == 1)
                {
                    //TOTDO if (item.HasAttach)
                    item.Save(GetQueuePath());
                }
                else if (doCover == 2)
                {
                    //TOTDO if (item.HasAttach)
                    item.Delete(GetQueuePath());
                }
            }
            else
            {
                //int res = 0;

                //using (Command cmd = new Command(this.ConnectionString))
                //{
                //    if (doCover == 1)
                //    {
                //        res = cmd.QueueItemsCover(m_QueueName, status, item, CoverMode, SerializeBody);

                //        if (item.HasAttach)
                //        {
                //            if (item.AttachItems.Count > 0 && item.Retry == 0)
                //            {
                //                for (int i = 0; i < item.AttachItems.Count; i++)
                //                {
                //                    QueueAttachItem attItem = item.AttachItems[i];
                //                    cmd.QueueAttachItemsCover(attItem.AttachId, attItem.AttachStream, attItem.AttachPath, attItem.MsgId, CoverMode);
                //                }
                //            }
                //        }

                //    }
                //    else if (doCover == 2)
                //    {
                //        res = cmd.QueueItemDelete(item);

                //    }
                //}
            }
        }


        //protected void ClearFinalItems()
        //{
        //    //using (Command cmd = new Command(this.ConnectionString))
        //    //{
        //    //    cmd.ClearFinalItems(QueueName, Server);
        //    //}
        //}

    
  
        #endregion

        #region Cover items

        #endregion

        #region Hold items
        internal void OnCoverHoldItem(IQueueItem item, Messaging.ItemState status, int retry)
        {
            if (!recoverable)
                return;
            if (item.Retry > 0) return;

            //if (item.HasAttach)
            //{
            //    if (item.AttachItems.Count > 0 && item.Retry == 0)
            //    {
            //        for (int i = 0; i < item.AttachItems.Count; i++)
            //        {
            //            QueueAttachItem attItem = item.AttachItems[i];
            //        }
            //    }
            //}

        }

        /// <summary>
        /// Get HoldItemsCount by interval minute
        /// </summary>
        /// <param name="intervalMinute"></param>
        /// <returns></returns>
        public int HoldItemsCount()
        {
            //if (!recoverable)
            return 0;

        }

        /// <summary>
        /// Re Enqueue all Hold Items remain in DB by interval minute 
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="intervalMinute"></param>
        public void HoldItemsEnqueue(int capacity)//, int intervalMinute)
        {
            if (!recoverable)
                return;
            if (enqueueHoldItems)
                return;
            if (capacity <= 0)
                return;
            try
            {
                enqueueHoldItems = true;

                //foreach (IQueueItem item in items)
                //{
                //    ((QueueItem)item).retry += 1;
                //    if (HoldItemsRemove(item) > 0)
                //    {
                //        Enqueue(item);
                //    }
                //}
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            finally
            {
                enqueueHoldItems = false;
            }
        }

        private int HoldItemsRemove(IQueueItem item)
        {
            return 0;
        }

        #endregion

        
    }
}


