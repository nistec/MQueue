using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nistec.Messaging.Io;
using Nistec.Generic;
using Nistec.Messaging.Listeners;
using System.Collections.Specialized;
using Nistec.Channels;

namespace Nistec.Messaging
{
    public static class Assists
    {
        #region consts
        public const string FileExt = ".mcq";
        public const string FileInfoExt = ".mci";

        public const string FolderQueueList = "Queues";
        public const string FolderQueue = "Queue";
        public const string FolderInfo = "Info";
        public const string FolderCovered = "Covered";
        public const string FolderSuspend = "Suspend";
        public const int MaxRetry = 3;
        public const string EXECPATH = "EXECPATH";
        #endregion

        public static bool IsStateOk(this MessageState state)
        {
            return ((int)state < 20);
        }
        //public static bool IsStateOk(this ChannelState state)
        //{
        //    return ((int)state < 20);
        //}

        //public static bool IsConnectionError(this ChannelState state)
        //{
        //    return state == ChannelState.ConnectionError;
        //}

        public static string NewIdentifier()
        {
            return UUID.Identifier();
        }

        public static DateTime NullDate
        {
            get { return new DateTime(1900, 1, 1); }
        }

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
            if (root == null || root.Length == 0)
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

        public static string EnsurePath(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }

            return path;
        }

        //public static string EnsureQueueSectionPath(string root, string queueName, string section)
        //{
        //    string path = GetQueueSectionPath(root, queueName, section);
        //    DirectoryInfo di = new DirectoryInfo(path);
        //    if (!di.Exists)
        //    {
        //        di.Create();
        //    }

        //    return path;
        //}
        
        public static TimeSpan GetDuration(this IQueueRequest item)
        {
            return DateTime.Now.Subtract(item.Creation);
        }
       
        internal static void SetReceived(QueueMessage item, QueueCmd cmd)
        {
            if (item == null)
                return;
            switch(cmd)
            {
                case QueueCmd.Peek:
                case QueueCmd.PeekItem:
                case QueueCmd.PeekPriority:
                    item.SetReceived(MessageState.Peeked);
                    break;
                case QueueCmd.Dequeue:
                case QueueCmd.DequeueItem:
                case QueueCmd.DequeuePriority:
                    item.SetReceived(MessageState.Received);
                    break;
                default:
                    item.SetReceived(MessageState.None);
                    break;
            }
        }
        internal static void SetReceived(QueueMessage item, MessageState state)
        {
            if (item == null)
                return;
            item.SetReceived(state);
        }
        internal static void SetArrived(QueueMessage item)
        {
            if (item == null)
                return;
            item.SetArrived();
        }
        internal static void SetArrived(QueueAck item)
        {
            if (item == null)
                return;
            item.SetArrived();
        }

        internal static void SetState(this QueueMessage item, MessageState state)
        {
            try
            {
                //item.Modified = DateTime.Now;
                item.MessageState = state;
                //item.Header = null;
                //m_stream.Replace((byte)MessageState, offset + 1);
                //m_stream.Replace(Modified.Ticks, offset + 44);
            }
            catch (Exception ex)
            {
                throw new MessageException(Messaging.MessageState.StreamReadWriteError, "QueueItemStream SetState error: " + ex.Message);
            }
        }

        internal static void DoRetry(this QueueMessage item)
        {
            item.Retry++;
            //item.Modified = DateTime.Now;
            //item.Header = null;
            //m_stream.Replace(Retry, offset + 24);
            //m_stream.Replace(Modified.Ticks, offset + 44);
        }

        //public static void SetDuration(QueueMessage item)
        //{
        //    if (item == null)
        //        return;
        //    var d= DateTime.Now.Subtract(item.Creation).TotalMilliseconds;
        //    d = Math.Min(d,int.MaxValue);
        //    item.Duration = (int) d;
        //}
        //public static void SetDuration(QueueAck item)
        //{
        //    if (item == null)
        //        return;
        //    var d = DateTime.Now.Subtract(item.Creation).TotalMilliseconds;
        //    d = Math.Min(d, int.MaxValue);
        //    item.Duration = (int)d;
        //}


        //public static string GetFilename(long UniqueId, Priority priority)
        //{
        //    return string.Format("{0}{1}{2}", priority.ToChar(), UniqueId, IoAssists.FileExt);
        //}
        public static string GetFilename(string identifier)
        {
            return string.Format("{0}{1}", identifier, Assists.FileExt);
        }
        public static char ToChar(this Priority priority)
        {
            return "abc"[(int)priority];
        }

        public static Priority FromChar(char priority)
        {
            switch (priority)
            {
                case 'c': return Priority.Normal;
                case 'b': return Priority.Medium;
                case 'a': return Priority.High;
                default:
                    return Priority.Normal;
            }
        }

        public static string GetFolderId(DateTime Modified, Priority priority)
        {
            int time;
            //201310270810
            int.TryParse(Modified.ToString("yyyyMMddHHmm"), out time);
            time = (int)((float)(time / 10));
            string folderId = time.ToString();
            return string.Format("{0}-{1}", (int)priority, folderId);
        }

        public static string GetIdentifier(Guid itemId, Priority priority)
        {
            return string.Format("{0}{1}", priority.ToChar(), itemId.UxId());
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
        //    return identifier.Substring(0, identifier.Length-6);
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

        //    System.Messaging.QueueMessage mq;

        //    System.ServiceModel.Channels.Message sm;
        //}


        

        

        

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

                return Path.Combine(root, Assists.FolderInfo, queueName);
            else
                return Path.Combine(root, Assists.FolderQueue, queueName);
        }

        public static string[] GetFiles(string path, bool isInfo=false)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            string ext = isInfo ? Assists.FileInfoExt : Assists.FileExt;
            return Directory.GetFiles(path, "*" + ext);
        }
        public static FileInfo[] GetFilesInfo(string path, string ext= Assists.FileExt, SearchOption searchOption= SearchOption.AllDirectories)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            DirectoryInfo di = new DirectoryInfo(path);
            //string ext = isCoverable ? Assists.FolderInfo : Assists.FolderQueue;
            return di.GetFiles("*" + ext, searchOption);
        }


        public static IOrderedEnumerable<FileInfo> GetOrderedFilesInfo(string path, string ext = Assists.FolderQueue, SearchOption searchOption = SearchOption.AllDirectories)
        {

            var list = GetFilesInfo(path, ext, searchOption);

            return list.OrderBy(f => f.CreationTime);

            //if (orderType == FileOrderTypes.ByCreation)
            //    return list.OrderBy(f => f.CreationTime);
            //else
            //    return list.OrderBy(f => f.Name);
        }

        public static IEnumerable<string> EnumerateFiles(string path, bool isInfo=false, SearchOption so= SearchOption.TopDirectoryOnly)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            string ext = isInfo ? Assists.FileInfoExt : Assists.FileExt;
            return Directory.EnumerateFiles(path, "*" + ext, so);
        }

        public static IEnumerable<string> EnumerateFolders(string path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            return Directory.EnumerateDirectories(path);
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
        public static string GetIdentifierPath(string queuePath, string identifier)
        {
           return Path.Combine(queuePath, GetFolderId(identifier));

        }

        public static string GetFilename(string root, string hostName, string identifier, bool isInfo)
        {
            if (isInfo)

                return GetInfoFilename(root, hostName, identifier);
            else
                return GetQueueFilename(root, hostName, identifier);
        }
        public static string GetFolderId(Guid itemId, Priority priority)
        {
            return GetFolderId(Assists.GetIdentifier(itemId, priority));
        }
        public static string GetFolderId(string identifier,int length=1)
        {
            if (identifier == null || identifier.Length<1 || length > 5)
            {
                throw new ArgumentException("identifier is null or length out of range, should be between 1 and 5");
            }
            return identifier.Substring(0, length);

            //if (identifier.Length < 6)
            //    return identifier.Substring(0, identifier.Length - 2);
            //return identifier.Substring(0, identifier.Length - 6);
        }

        public static string GetRandomFolderId(int length = 1)
        {
            return RandomString(length);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length-1)]).ToArray());

            /*
             string[] ch= new string[length];

            for(int i=0;i< length;i++)
            {
                ch[i] = new string(Enumerable.Repeat(chars, 1)
              .Select(s => s[s.Length-1]).ToArray());
            }

            return string.Join("", ch);
            */
        }

        //public static string GetQueueFilename(long UniqueId, Priority priority)
        //{
        //    return GetQueueFilename(Assists.GetIdentifier(UniqueId, priority));
        //}
        public static string FormatQueueFilename(string identifier)
        {
            return string.Format("{0}{1}", identifier, Assists.FileExt);
        }
        public static string GetQueueFilename(string root, string hostName, string identifier)
        {
            string path = Path.Combine(root, Assists.FolderQueue, hostName, GetFolderId(identifier));
            //string path = Path.Combine(infopath, hostName);
            return string.Format("{0}\\{1}", path, FormatQueueFilename(identifier));
        }
        //public static string GetInfoFilename(long UniqueId, Priority priority)
        //{
        //    return GetInfoFilename(Assists.GetIdentifier(UniqueId, priority));
        //}
        public static string GetInfoFilename(string identifier)
        {
            return string.Format("{0}{1}", identifier, Assists.FileInfoExt);
        }
        public static string GetInfoFilename(string root, string hostName, string identifier)
        {
            string infopath = Path.Combine(root, Assists.FolderInfo);
            string path = Path.Combine(infopath, hostName);
            return string.Format("{0}\\{1}", path, GetInfoFilename(identifier));
        }
        public static string QueueToCovered(string filename, bool removeSplitter=true)
        {
            string dest = filename.Replace("\\Queue\\", string.Format("\\{0}\\", Assists.FolderCovered));
            if (removeSplitter)
            {
                string name = Path.GetFileName(filename);
                string dir = Path.GetDirectoryName(dest);
                dir = Path.GetDirectoryName(dir);
                dest = Path.Combine(dir, name);
            }
            return dest;
        }
        public static string QueueToSuspend(string filename, bool removeSplitter = true)
        {
            string dest = filename.Replace("\\Queue\\", string.Format("\\{0}\\", Assists.FolderSuspend));
            if (removeSplitter)
            {
                string name = Path.GetFileName(filename);
                string dir = Path.GetDirectoryName(dest);
                dir = Path.GetDirectoryName(dir);
                dest = Path.Combine(dir, name);
            }
            return dest;
        }
        public static string QueueToInfo(string filename)
        {
            return filename.Replace("\\Queue\\", string.Format("\\{0}\\", Assists.FolderInfo)).Replace(Assists.FileExt, Assists.FileInfoExt);
        }
        public static string InfoToQueue(string filename)
        {
            return filename.Replace("\\Info\\", string.Format("\\{0}\\", Assists.FolderQueue)).Replace(Assists.FileInfoExt, Assists.FileExt);
        }
        public static string InfoToCovered(string filename)
        {
            return filename.Replace("\\Info\\", string.Format("\\{0}\\", Assists.FolderCovered));
        }
        public static string InfoToSuspend(string filename)
        {
            return filename.Replace("\\Info\\", string.Format("\\{0}\\", Assists.FolderSuspend));
        }
        public static string CoveredToInfo(string filename)
        {
            return filename.Replace(string.Format("\\{0}\\", Assists.FolderCovered), "\\Info\\");
        }


        //public static IDictionary<string, string> CommaPipePrse(string s)
        //{
        //    Dictionary<string, string> dic = new Dictionary<string, string>();
        //    string[] args = s.SplitTrim('|');
        //    foreach (string arg in args)
        //    {
        //        string[] kv = arg.SplitTrim('=');
        //        if (kv.Length == 2)
        //            dic.Add(kv[0], kv[1]);
        //    }

        //    return dic;
        //}

        //public static NameValueCollection CommaPipeParse(string s)
        //{
        //    NameValueCollection dic = new NameValueCollection();
        //    string[] args = s.SplitTrim('|');
        //    foreach (string arg in args)
        //    {
        //        string[] kv = arg.SplitTrim('=');
        //        if (kv.Length == 2)
        //            dic.Add(kv[0], kv[1]);
        //    }

        //    return dic;
        //}
    }
}
