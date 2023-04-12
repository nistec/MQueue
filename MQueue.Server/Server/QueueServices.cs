using Nistec.Logging;
using Nistec.Messaging.Io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nistec.Messaging.Server
{
    public class QueueServices
    {
        string QueueName;
        //MQueue : IQueueReceiver, IQueuePerformance, IDisposable
        IQueueReceiver Qr;
        public QueueServices(string queueName)
        {
            Qr.
            QueueName = queueName;
            m_QueuesPath = AgentManager.Settings.QueuesPath;
        }
        
        private string m_QueuesPath = "";
        internal QueueHost RoutHost { get; set; }
        public string TargetPath { get; internal set; }

        bool onBackgroundProcess = false;

        private string GetFilename(string itemId)
        {
            return Path.Combine(GetQueuePath(), itemId) + ".mcq";
        }
        private string GetQueuePath()
        {
            return Path.Combine(m_QueuesPath, QueueName) + "\\";
        }
        private string GetRelayPath()
        {
            return Path.Combine(GetQueuePath(), "Relay") + "\\";
        }
        private string GetBackupPath()
        {
            return Path.Combine(GetQueuePath(), "Backup") + "\\";
        }


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
                        string backfile = SysUtil.PathFix(pathback + relyID + ".mcq");
                        SysUtil.MoveFile(rely, backfile);
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
                        string newfile = SysUtil.PathFix(pathrelay + messageID + ".mcq");
                        SysUtil.MoveFile(message, newfile);
                    }
                    //Netlog.InfoFormat("CleanFolder items files: {0} ", messages.Length);
                }
            }
        }

        public string BackupToFiles()
        {
            if (onBackgroundProcess)
                return null;
            try
            {
                Console.WriteLine("Start Backup QueueItems");
                string backupName = Guid.NewGuid().ToString();
                onBackgroundProcess = true;
                //string path = GetBackupPath();

                string path = Path.Combine(GetBackupPath(), backupName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (Directory.Exists(path))
                {

                    var items = Q.GetAllItems();

                    Netlog.InfoFormat("Backup Files: {0} ", items.Count());

                    foreach (var item in items)
                    {

                        string filename = SysIO.EnsureQueueFilename(path, item.Identifier);

                        var stream = item.ToStream();
                        if (stream != null)
                        {
                            stream.Copy().SaveToFile(filename);
                        }
                    }

                    Netlog.Info("BackupToFiles finished. ");
                    return path;
                }

            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            finally
            {
                onBackgroundProcess = false;
            }
            return null;
        }

        private void LoadFromBackup()
        {
            if (onBackgroundProcess)
                return;
            try
            {
                Console.WriteLine("Start ReEnqueueQueueItems");

                onBackgroundProcess = true;

                string path = GetRelayPath();

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

                        QueueMessage item = QueueMessage.ReadFile(message);
                        if (item != null)
                        {
                            Enqueue(item as IQueueMessage);
                        }
                        SysUtil.DeleteFile(message);
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
                onBackgroundProcess = false;
            }
        }

        private void AsyncLoadFromBackup()
        {
            Thread th = null;

            if (IsFileQueue)
            {
                th = new Thread(new ThreadStart(LoadFromBackup));
            }
            else
            {
                return;
                //th= new Thread(new ThreadStart(ReEnqueueDB));
            }
            th.Start();
        }
        #endregion
    }
    
}
