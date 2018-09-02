using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nistec.Messaging.Adapters;
using Nistec.Messaging.Io;

namespace Nistec.Messaging.Io
{
    
    public static class IoAssists
    {
        public const string FileExt = ".mcq";
        public const string FileInfoExt = ".mci";

        public const string FolderQueue = "Queue";
        public const string FolderInfo = "Info";
        public const string FolderCovered = "Covered";
        public const string FolderSuspend = "Suspend";




        public static void Exception_QueueDbNotSupported()
        {
            throw new Exception("Queue db not supported");
        }
        public static void Exception_QueueFileStreamNotSupported()
        {
            throw new Exception("Queue file stream not supported");
        }

        public static string GetQueuePath(string rootSection, string queueName)
        {
            if (rootSection == null || rootSection.Length == 0)
            {
                throw new ArgumentNullException("QueuePath root");
            }
            if (queueName == null || queueName.Length == 0)
            {
                throw new ArgumentNullException("QueuePath queueName");
            }
            return Path.Combine(rootSection, queueName);
        }

        public static string GetQueueSectionPath(string root, string section, string queueName)
        {
            if (root == null || root.Length==0)
            {
                throw new ArgumentNullException("QueueSectionPath root");
            }
            if (queueName == null || queueName.Length == 0)
            {
                throw new ArgumentNullException("QueueSectionPath queueName");
            }
            if (section == null || section.Length == 0)
            {
                throw new ArgumentNullException("QueueSectionPath queueName");
            }
            //string path = Path.Combine(root,queueName);
            return Path.Combine(root, section, queueName);
        }

        public static string GetQueuePath(string root, string queueName, bool isCoverable)
        {
            if (root == null || root.Length == 0)
            {
                throw new ArgumentNullException("QueueSectionPath root");
            }
            if (queueName == null || queueName.Length == 0)
            {
                throw new ArgumentNullException("QueueSectionPath queueName");
            }
            if (isCoverable)

                return Path.Combine(root, IoAssists.FolderInfo, queueName);
            else
                return Path.Combine(root, IoAssists.FolderQueue, queueName);
        }

        public static string[] GetFiles(string path, bool isCoverable)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            string ext = isCoverable ? IoAssists.FolderInfo : IoAssists.FolderQueue;
            return Directory.GetFiles(path, "*" + ext);
        }

        public static string EnsureQueueSectionPath(string root, string section, string queueName)
        {
            string path = GetQueueSectionPath(root, section,queueName);
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }

            return path;
        }
        public static string EnsureIdentifierPath(string queuePath, string identifier)
        {
            string path = Path.Combine(queuePath, GetFolderId(identifier));
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }

            return path;
        }
        public static string GetFilename(string root, string hostName, string identifier, bool isCoverable)
        {
            if (isCoverable)

                return GetInfoFilename(root, hostName, identifier);
            else
                return GetQueueFilename(root, hostName, identifier);
        }
        public static string GetFolderId(Guid itemId, Priority priority)
        {
            return GetFolderId(Assists.GetIdentifier(itemId, priority));
        }
        public static string GetFolderId(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }
            if (identifier.Length < 6)
                return identifier.Substring(0, identifier.Length - 2);
            return identifier.Substring(0, identifier.Length - 6);
        }

        //public static string GetQueueFilename(long UniqueId, Priority priority)
        //{
        //    return GetQueueFilename(Assists.GetIdentifier(UniqueId, priority));
        //}
        public static string GetQueueFilename(string identifier)
        {
            return string.Format("{0}{1}", identifier, IoAssists.FileExt);
        }
        public static string GetQueueFilename(string root, string hostName, string identifier)
        {
            string path = Path.Combine(root, IoAssists.FolderQueue, hostName, GetFolderId(identifier));
            //string path = Path.Combine(infopath, hostName);
            return string.Format("{0}\\{1}", path, GetQueueFilename(identifier));
        }
        //public static string GetInfoFilename(long UniqueId, Priority priority)
        //{
        //    return GetInfoFilename(Assists.GetIdentifier(UniqueId, priority));
        //}
        public static string GetInfoFilename(string identifier)
        {
            return string.Format("{0}{1}", identifier, IoAssists.FileInfoExt);
        }
        public static string GetInfoFilename(string root, string hostName, string identifier)
        {
            string infopath = Path.Combine(root, IoAssists.FolderInfo);
            string path = Path.Combine(infopath, hostName);
            return string.Format("{0}\\{1}", path, GetInfoFilename(identifier));
        }
        public static string QueueToCovered(string filename)
        {
            return filename.Replace("\\Queue\\", string.Format("\\{0}\\", IoAssists.FolderCovered));
        }
        public static string QueueToSuspend(string filename)
        {
            return filename.Replace("\\Queue\\", string.Format("\\{0}\\", IoAssists.FolderSuspend));
        }
        public static string QueueToInfo(string filename)
        {
            return filename.Replace("\\Queue\\", string.Format("\\{0}\\", IoAssists.FolderInfo)).Replace(IoAssists.FileExt, IoAssists.FileInfoExt);
        }
        public static string InfoToQueue(string filename)
        {
            return filename.Replace("\\Info\\", string.Format("\\{0}\\", IoAssists.FolderQueue)).Replace(IoAssists.FileInfoExt,IoAssists.FileExt);
        }
        public static string InfoToCovered(string filename)
        {
            return filename.Replace("\\Info\\", string.Format("\\{0}\\", IoAssists.FolderCovered));
        }
        public static string InfoToSuspend(string filename)
        {
            return filename.Replace("\\Info\\", string.Format("\\{0}\\", IoAssists.FolderSuspend));
        }
        public static string CoveredToInfo(string filename)
        {
            return filename.Replace(string.Format("\\{0}\\", IoAssists.FolderCovered), "\\Info\\");
        }

        //public static char ToChar(this Priority priority)
        //{
        //    return "abc"[(int)priority];
        //}

        //public static Priority FromChar(char priority)
        //{
        //    switch (priority)
        //    {
        //        case 'c': return Priority.Normal;
        //        case 'b': return Priority.Medium;
        //        case 'a': return Priority.High;
        //        default:
        //            return Priority.Normal;
        //    }
        //}

        public static string GetFolderId(DateTime Modified, Priority priority)
        {
            int time;
            //201310270810
            int.TryParse(Modified.ToString("yyyyMMddHHmm"), out time);
            time = (int)((float)(time / 10));
            string folderId = time.ToString();
            return string.Format("{0}-{1}", (int)priority, folderId);
        }

        //public static string GetIdentifier(long uniqueId, Priority priority)
        //{
        //    return string.Format("{0}{1}", priority.ToChar(), uniqueId);
        //}

        //public static string GetFolderId(long uniqueId, Priority priority)
        //{
        //    return string.Format("{0}{1}", priority.ToChar(), uniqueId / 100000);
        //}

        //public static string GetFolderId(string identifier)
        //{
        //    if (identifier == null)
        //    {
        //        throw new ArgumentNullException("identifier");
        //    }
        //    return identifier.Substring(0,identifier.Length-6);
        //}

        //public static long GetUniqueId(string identifier)
        //{
        //    if (identifier == null)
        //    {
        //        throw new ArgumentNullException("identifier");
        //    }
        //    if (Char.IsLetter(identifier[0]))
        //        return Types.ToLong(identifier.Substring(1));
        //    else
        //        return Types.ToLong(identifier);
        //}

        //static void test()
        //{
        //    System.Messaging.Message msg;

        //    System.Messaging.MessageQueue mq;

        //    System.ServiceModel.Channels.Message sm;
        //}
    }
}
