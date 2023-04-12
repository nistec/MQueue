using Nistec.Serialization;
using Nistec.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nistec.Channels;
using System.Data;
using Nistec.Logging;
using Nistec.Messaging.Remote;
using Nistec.Messaging;
using Nistec.Channels.Tcp;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace Nistec.QueueConsole
{
    internal class SectionType
    {
        public const string queue = "remote-queue";
        public const string operation = "remote-operation";
        public const string report = "remote-report";
        public const string manager = "remote-manager";
    }

    class ConsoleController
    {
        internal static bool EnableLog = false;
        internal static bool EnableJsonController = false;
        public static void Run(string[] args)
        {
            NetProtocol cmdProtocol = NetProtocol.Tcp;
            string protocol = "tcp";
            string cmd = "";
            string sectionType = SectionType.queue;
            string transform = "binary";
            string cmdName = "";
            string cmdKey = "";
            string cmdValue = "";
            //string cmdArg3 = "";
            //string cmdArg4 = "";

            //"QueueName=|ServerPath=|Mode=[Memory,Persistent,FileStream,Db,Rout]|IsTrans=false|MaxRetry=3|ReloadOnStart=true|ConnectTimeout=0|TargetPath=|IsTopic=false|CommitMode=|[OnDisk,OnMemory1,None]"

            //"[Memory,Persistent,FileStream,Db,Rout]"


            //DisplayMenu("menu", "", "");
            //DisplaySectionTypeMenu();
            //sectionType = GetSectionType(Console.ReadLine().ToLower(), sectionType);

            if (sectionType == "quit")
            {
                return;
            }
            //Console.WriteLine("Current section type : {0}.", sectionType);
            //SetCommands();
            Console.WriteLine("Welcome to MQueue cli");


            while (cmd != "quit")
            {
                Console.WriteLine("Enter command :");

                cmd = Console.ReadLine();

                try
                {

                    //string[] cmdargs = SplitCmd(cmd);
                    string[] cmdargs = SplitCommand(cmd);
                    cmdName = GetCommandType(cmdargs, cmdName, 0);
                    cmdKey = GetCommandType(cmdargs, cmdKey, 1);
                    cmdValue = GetCommandType(cmdargs, cmdValue, 2);
                    //cmdArg3 = GetCommandType(cmdargs, cmdArg3, 3);
                    //cmdArg4 = GetCommandType(cmdargs, cmdArg4, 4);

                    switch (cmdName.ToLower())
                    {
                        case "menu":
                            CmdController.DisplayMenu();
                            //DisplayMenu("menu", "", "");
                            break;
                        //case "menu-items":
                        //    DisplayMenu("menu-items", sectionType, "");
                        //    break;
                        //case "section-type":
                        //    DisplaySectionTypeMenu();
                        //    sectionType = GetSectionType(Console.ReadLine().ToLower(), sectionType);
                        //    Console.WriteLine("Current section type : {0}.", sectionType);
                        //    break;
                        case "transform":
                            if (cmdKey == "binary" || cmdKey == "json")
                                transform = cmdKey;
                            else
                                Console.WriteLine("Wrong command");
                            //DisplayTransformTypeMenu();
                            //transform= GetTransformType(Console.ReadLine().ToLower(), transform);
                            Console.WriteLine("Current transform type : {0}.", transform);
                            break;
                        case "protocol":
                            if (cmdKey == "tcp" || cmdKey == "pipe" || cmdKey == "http")
                                protocol = cmdKey;
                            else
                                Console.WriteLine("Wrong command");
                            //Console.WriteLine("Choose protocol : tcp , pipe, http");
                            //protocol = EnsureProtocol(Console.ReadLine().ToLower(), protocol);
                            //cmdProtocol = GetProtocol(protocol, cmdProtocol);
                            Console.WriteLine("Current protocol : {0}.", protocol);
                            break;
                        case "commands":
                        case "?":
                            CmdController.DisplayCommands();
                            break;
                        case "all":
                            CmdController.DisplayMenu();
                            CmdController.DisplayCommands();
                            break;
                        case "args":
                            CmdController.DisplayArgs();
                            //DisplayMenu("args", sectionType, cmdKey);
                            break;
                        case "report":
                            //CmdController.DoCommandManager(cmdArg1, sectionType);
                            break;
                        case "stop":
                        case "/stop":
                            CmdController.MonitorState = false;
                            break;
                        case "quit":

                            break;
                        default:
                            CmdController.DoCommand(cmdProtocol, transform, cmdName, cmdKey, cmdValue);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                }
                Console.WriteLine();
            }
        }
        static string EnsureArg(string arg)
        {
            if (arg == null)
                return "";
            return arg.Replace("/", "");//.ToLower();
        }
        static string[] SplitCommand(string cmd)
        {
            //string[] cmdargs = cmd.SplitTrim('/');
            string[] cmdargs = cmd.SplitTrim(' ');
            for (int i = 0; i < cmdargs.Length; i++)
                cmdargs[i] = cmdargs[i].Trim();

            return cmdargs;
        }
        /*
        static Dictionary<string, string> cmdQueue = new Dictionary<string, string>();
        static Dictionary<string, string> cmdOperation = new Dictionary<string, string>();
        //static Dictionary<string, string> cmdSession = new Dictionary<string, string>();
        static Dictionary<string, string> cmdReport = new Dictionary<string, string>();
        static Dictionary<string, string> cmdManager = new Dictionary<string, string>();
        static void SetCommands()
        {
            cmdQueue.Add("AddQueue", "key, value:QueueName =| ServerPath =| Mode =[Memory, Persistent, FileStream, Db, Rout] | IsTrans = false | MaxRetry = 3 | ReloadOnStart = true | ConnectTimeout = 0 | TargetPath =| IsTopic = false | CommitMode =|[OnDisk, OnMemory1, None]");
            cmdQueue.Add("RemoveQueue", "key, value");
            cmdQueue.Add("QueueExists", "key, value");
            cmdQueue.Add("Reply", "text");

            cmdOperation.Add("HoldEnqueue", "key, value");
            cmdOperation.Add("ReleaseHoldEnqueue", "key, value");
            cmdOperation.Add("HoldDequeue", "key, value");
            cmdOperation.Add("ReleaseHoldDequeue", "key, value");
            cmdOperation.Add("EnableQueue", "key, value");
            cmdOperation.Add("DisableQueue", "key, value");
            cmdOperation.Add("TopicAdd", "key, value");
            cmdOperation.Add("TopicRemove", "key, value");
            cmdOperation.Add("TopicPublish", "key, value");
            cmdOperation.Add("TopicSubscribe", "key, value");
            cmdOperation.Add("TopicRemoveItem", "key, value");
            cmdOperation.Add("TopicHold", "key, value");
            cmdOperation.Add("TopicHoldRelease", "key, value");
            cmdOperation.Add("TopicSubscribeHold", "key, value");
            cmdOperation.Add("TopicSubscribeRelease", "key, value");
            cmdOperation.Add("TopicSubscribeAdd", "key, value");
            cmdOperation.Add("TopicSubscribeRemove", "key, value");

            cmdReport.Add("Exists", "key, value");
            cmdReport.Add("QueueProperty", "key, value");
            cmdReport.Add("ReportQueueList", "key, value");
            cmdReport.Add("ReportQueueItems", "key, value");
            cmdReport.Add("ReportQueueStatistic", "key, value");
            cmdReport.Add("PerformanceCounter", "key, value");
            cmdReport.Add("QueueCount", "key, value");


            //cmdOperation.Add("GetRecord", "tableName, key");
            //cmdOperation.Add("GetEntity", "tableName, key");
            //cmdOperation.Add("Get", "tableName, key, field");
            //cmdOperation.Add("GetAllEntityNames", "");
            //cmdOperation.Add("GetEntityKeys", "tableName");
            //cmdOperation.Add("GetEntityItemsCount", "tableName");
            //cmdOperation.Add("PrintEntityValues", "tableName");
            //cmdOperation.Add("Remove", "tableName");
            //cmdOperation.Add("RefreshAll", "");
            //cmdOperation.Add("Refresh", "tableName");
            //cmdOperation.Add("Reset", "");
            //cmdOperation.Add("Reply", "text");

            //cmdSession.Add("CreateSession", "sessionId, expiration");
            //cmdSession.Add("Add", "sessionId, key, value");
            //cmdSession.Add("Set", "sessionId, key, value");
            //cmdSession.Add("Get", "sessionId, key");
            //cmdSession.Add("Remove", "sessionId, key");
            //cmdSession.Add("GetSessionItems", "sessionId");
            //cmdSession.Add("GetAllSessionsKeys", "sessionId");
            //cmdSession.Add("RemoveSession", "sessionId");
            //cmdSession.Add("Reply", "text");


            //cmdReport.Add("GetRecord", "db, tableName, key");
            //cmdReport.Add("Get", "db, tableName, key, field");
            //cmdReport.Add("GetAllEntityNames", "");
            //cmdReport.Add("GetEntityKeys", "db, tableName");
            //cmdReport.Add("GetEntityItemsCount", "db, tableName");
            //cmdReport.Add("GetTable", "db, tableName");
            //cmdReport.Add("RemoveTable", "db, tableName");
            //cmdReport.Add("Refresh", "db, tableName");
            //cmdReport.Add("Reset", "");
            //cmdReport.Add("Reply", "text");

            cmdManager.Add("Statistic", "queue-type");
            cmdManager.Add("CounterState", "queue-type");
            cmdManager.Add("CounterReset", "");
            cmdManager.Add("PrintLog", "");
        }

        static void DisplaySectionTypeMenu()
        {
            Console.WriteLine("Choose section type : remote-queue, remote-operation, remote-report, remote-manager");
        }
        static void DisplayTransformTypeMenu()
        {
            Console.WriteLine("Choose transform type : binary,json");
        }
        static void DisplayArgs(string cmdType, string arg)
        {
            string a = EnsureArg(arg);
            KeyValuePair<string, string> kv = new KeyValuePair<string, string>();
            switch (cmdType)
            {
                case SectionType.queue:
                    kv = cmdQueue.Where(p => p.Key == a).FirstOrDefault();
                    break;
                case SectionType.operation:
                    kv = cmdOperation.Where(p => p.Key == a).FirstOrDefault();
                    break;
                case SectionType.report:
                    kv = cmdReport.Where(p => p.Key == a).FirstOrDefault();
                    break;
                case SectionType.manager:
                    kv = cmdReport.Where(p => p.Key == a).FirstOrDefault();
                    break;
            }

            if (kv.Key != null)
                Console.WriteLine("commands: {0} Arguments: {1}.", kv.Key, kv.Value);
            else
                Console.WriteLine("Bad commands: {0} Arguments: {1}.", cmdType, arg);
        }

        static void DisplayMenu(string cmdType, string sectionType, string arg)
        {
            //string menu = "queue-type: remote-queue, remote-sync, remote-session";
            //Console.WriteLine(menu);

            switch (cmdType)
            {
                case "menu":
                    Console.WriteLine("Enter: section-type, To change section type");
                    Console.WriteLine("Enter: protocol, To change protocol (tcp, pipe, http)");
                    Console.WriteLine("Enter: transform, To change transform type (binary, json)");
                    Console.WriteLine("Enter: menu, To display menu");
                    Console.WriteLine("Enter: menu-items, To display menu items for current queue-type");
                    Console.WriteLine("Enter: all, to display all commands");
                    Console.WriteLine("Enter: args, and /command to display command argument");
                    Console.WriteLine("Enter: report, and /command to display queue report");

                    break;
                case "menu-items":
                    DisplayMenuItems(cmdType, sectionType);
                    break;
                case "args":
                    if (arg != null && arg.StartsWith("/"))
                    {
                        DisplayArgs(sectionType, arg);
                    }
                    break;
            }
            Console.WriteLine("");

        }

        static void DisplayMenuItems(string cmdType, string sectionType)
        {
            switch (sectionType)
            {
                case SectionType.queue:
                    Console.Write("{0} commands: ", sectionType);
                    foreach (var entry in cmdQueue)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                case SectionType.operation:
                    Console.Write("{0} commands: ", sectionType);
                    foreach (var entry in cmdOperation)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                //case SectionType.session:
                //    Console.Write("{0} commands: ", sectionType);
                //    foreach (var entry in cmdSession)
                //    {
                //        Console.Write("{0} ,", entry.Key);
                //    }
                //    break;
                case SectionType.report:
                    Console.Write("{0} commands: ", sectionType);
                    foreach (var entry in cmdReport)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                case SectionType.manager:
                    Console.Write("{0} commands: ", sectionType);
                    foreach (var entry in cmdManager)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                default:
                    Console.Write("Bad commands: Invalid section-type");
                    break;
            }
            Console.WriteLine();

        }
 
        static string[] SplitCmd(string cmd, int maxArgs = 0)
        {
            string[] cmdargs = cmd.SplitTrim(' ');
            List<string> list = new List<string>();
            int i = 0;
            int length = cmdargs.Length;
            foreach (string s in cmdargs)
            {
                if (cmdargs[i].StartsWith("\""))
                {
                    StringBuilder sb = new StringBuilder();
                    bool completed = false;
                    do
                    {
                        if (cmdargs[i].EndsWith("\""))
                        {
                            sb.Append(cmdargs[i].TrimEnd('"'));
                            completed = true;
                        }
                        else
                        {
                            sb.Append(cmdargs[i].TrimStart('"') + " ");
                            i++;
                        }

                    } while (!completed && i < length);
                    list.Add(sb.ToString());
                }
                else
                {
                    list.Add(cmdargs[i]);
                }
                i++;
                if (i >= length)
                    break;
            }
            if ((maxArgs > 0 && list.Count > maxArgs) || list.Count == 0)
            {
                throw new Exception("Incorrect command!");
            }
            return list.ToArray();
        }
        */
        static string GetSectionType(string cmd, string curItem)
        {
            switch (cmd.ToLower())
            {
                case SectionType.queue:
                case SectionType.operation:
                case SectionType.report:
                case SectionType.manager:
                    return cmd.ToLower();
                default:
                    Console.WriteLine("Invalid section-type {0}", cmd);
                    return curItem;
            }
        }
        static string GetCommandType(string[] args, string curItem, int i)
        {
            string cmd = curItem;
            if (args.Length > i)
            {
                cmd = args[i];
                if (cmd == "..")
                    return curItem;
            }
            return cmd;
        }
        static string EnsureProtocol(string protocol, string curProtocol)
        {
            switch (protocol.ToLower())
            {
                case "tcp":
                case "pipe":
                case "http":
                    return protocol.ToLower();
                default:
                    return curProtocol;
            }
        }
        static string EnsureTransform(string cmd, string curItem)
        {
            switch (cmd.ToLower())
            {
                case "json":
                case "binary":
                    return cmd.ToLower();
                default:
                    Console.WriteLine("Invalid transform-type {0}", cmd);
                    return curItem;
            }
        }

        static NetProtocol GetProtocol(string protocol, NetProtocol curProtocol)
        {
            switch (protocol.ToLower())
            {
                case "tcp":
                    return NetProtocol.Tcp;
                case "pipe":
                    return NetProtocol.Pipe;
                case "http":
                    return NetProtocol.Http;
                default:
                    return curProtocol;
            }
        }

        public static int GetUsage()
        {

            System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("NistecQueue");
            int usage = 0;
            if (process == null)
                return 0;
            for (int i = 0; i < process.Length; i++)
            {
                usage += (int)((int)process[i].WorkingSet64) / 1024;
            }

            return usage;
        }

    }

    #region Cmd Controller

    

    class CmdController
    {
        public static bool MonitorState = false;

        public static void DoCommand(NetProtocol cmdProtocol, string transform, string cmd, string key, string value)
        {

            string hostAddress = "nistec_queue_manager";
            if (cmdProtocol == NetProtocol.Tcp)
            {
                var tcpSettings = TcpClientSettings.GetTcpClientSettings("nistec_queue_manager");
                hostAddress = string.Format("{0}:{1}", tcpSettings.Address, tcpSettings.Port) ;
            }
            var api = ManagementApi.Get(hostAddress,cmdProtocol);
            bool ok = true;
            string json = null;
            IQueueMessage qi = null;
            TransStream ts = null;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {

                //if (transform == "json")
                //{
                //    json = api.DoHttpJson(command: cmd, key: key, value: value, pretty: true);
                //    Display(cmd, json);
                //}
                //else
                //{
                    
                //}

                if (transform == "json")
                {
                    json = api.DoHttpJson(command: cmd, key: key, value: value, pretty: true);
                    Display(cmd, json);
                }
                else
                {
                    
                    switch (cmd.ToLower().Replace("-", ""))
                    {
                        case "addqueue":
                            ts = api.AddQueue(QProperties.ByCommaPipe(value));
                            Display(cmd, ts);
                            break;
                        case "removequeue":
                            ts = api.RemoveQueue(key);
                            Display(cmd, ts);
                            break;
                        case "queueexists":
                            ts = api.QueueExists(key);
                            Display(cmd, ts);
                            break;
                        case "backupqueue":
                            ts = api.OperateQueue(QueueCmdOperation.BackupQueue, key);
                            Display(cmd, ts);
                            break;
                        case "backupall":
                            ts = api.OperateQueue(QueueCmdOperation.BackupAll);
                            Display(cmd, ts);
                            break;
                        case "loadfrombackup":
                            {
                                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                                {
                                    Console.WriteLine("Queue name or path missing");
                                    return;
                                }
                                QueueRequest message = new QueueRequest()
                                {
                                    Host = key,
                                    QCommand = QueueCmd.LoadFromBackup,
                                    Args = NameValueArgs.Create("path", value)
                                };
                                ts = api.OperateQueue(message);
                                Display(cmd, ts);
                            }
                            break;
                        case "holdenqueue":
                            ts = api.OperateQueue(QueueCmdOperation.HoldEnqueue, key);
                            Display(cmd, ts);
                            break;
                        case "releaseholdenqueue":
                            ts = api.OperateQueue(QueueCmdOperation.ReleaseHoldEnqueue, key);
                            Display(cmd, ts);
                            break;
                        case "holddequeue":
                            ts = api.OperateQueue(QueueCmdOperation.HoldDequeue, key);
                            Display(cmd, ts);
                            break;
                        case "releaseholddequeue":
                            ts = api.OperateQueue(QueueCmdOperation.ReleaseHoldDequeue, key);
                            Display(cmd, ts);
                            break;
                        case "enablequeue":
                            ts = api.OperateQueue(QueueCmdOperation.EnableQueue, key);
                            Display(cmd, ts);
                            break;
                        case "disablequeue":
                            ts = api.OperateQueue(QueueCmdOperation.DisableQueue, key);
                            Display(cmd, ts);
                            break;
                        case "topicadd":
                            ts = api.OperateQueue(QueueCmdOperation.TopicAdd, key);
                            Display(cmd, ts);
                            break;
                        case "topicremove":
                            ts = api.OperateQueue(QueueCmdOperation.TopicRemove, key);
                            Display(cmd, ts);
                            break;
                        case "topicpublish":
                            ts = api.OperateQueue(QueueCmdOperation.TopicPublish, key);
                            Display(cmd, ts);
                            break;
                        case "topicsubscribe":
                            ts = api.OperateQueue(QueueCmdOperation.TopicSubscribe, key);
                            Display(cmd, ts);
                            break;
                        case "topicremoveItem":
                            ts = api.OperateQueue(QueueCmdOperation.TopicRemoveItem, key);
                            Display(cmd, ts);
                            break;
                        case "topichold":
                            ts = api.OperateQueue(QueueCmdOperation.TopicHold, key);
                            Display(cmd, ts);
                            break;
                        case "topicholdrelease":
                            ts = api.OperateQueue(QueueCmdOperation.TopicHoldRelease, key);
                            Display(cmd, ts);
                            break;
                        case "topicsubscribehold":
                            ts = api.OperateQueue(QueueCmdOperation.TopicSubscribeHold, key);
                            Display(cmd, ts);
                            break;
                        case "topicsubscriberelease":
                            ts = api.OperateQueue(QueueCmdOperation.TopicSubscribeRelease, key);
                            Display(cmd, ts);
                            break;
                        case "topicsubscribeadd":
                            ts = api.OperateQueue(QueueCmdOperation.TopicSubscribeAdd, key);
                            Display(cmd, ts);
                            break;
                        case "topicsubscriberemove":
                            ts = api.OperateQueue(QueueCmdOperation.TopicSubscribeRemove, key);
                            Display(cmd, ts);
                            break;
                        case "exists":
                            ts = api.Report(QueueCmdReport.Exists, key);
                            Display(cmd, ts);
                            break;
                        case "queueproperty":
                            ts = api.Report(QueueCmdReport.QueueProperty, key);
                            Display(cmd, ts);
                            break;
                        case "reportqueuelist":
                            ts = api.Report(QueueCmdReport.ReportQueueList, key);
                            Display(cmd, ts);
                            break;
                        case "reportqueueitems":
                            ts = api.Report(QueueCmdReport.ReportQueueItems, key);
                            Display(cmd, ts);
                            break;
                        case "reportqueuestatistic":
                            ts = api.Report(QueueCmdReport.ReportQueueStatistic, key);
                            Display(cmd, ts);
                            break;
                        case "performancecounter":
                            ts = api.Report(QueueCmdReport.PerformanceCounter, key);
                            Display(cmd, ts);
                            break;
                        case "queuecount":
                            if (value == "/start")
                            {
                                StartMonitor(() => {
                                    ts = api.Report(QueueCmdReport.QueueCount, key);
                                    Display(cmd, ts);
                                });
                            }
                            else
                            {
                                ts = api.Report(QueueCmdReport.QueueCount, key);
                                Display(cmd, ts);
                            }
                            break;
                        case "queuecountall":
                            if (key == "/start" || value == "/start")
                            {
                                StartMonitor(() => {
                                    ts = api.Report(QueueCmdReport.QueueCountAll);
                                    Display(cmd, ts);
                                });
                            }
                            else
                            {
                                ts = api.Report(QueueCmdReport.QueueCountAll);
                                Display(cmd, ts);
                            }
                            break;
                        case "usage":
                            ConsoleController.GetUsage();
                            break;
                        case "reply":
                            ts = api.Reply();
                            Display(cmd, ts);
                            break;
                        default:
                            ok = false;
                            Display(cmd, "Unknown command");
                            break;
                    }
                    /*
                    switch (cmd.ToLower().Replace("",""))
                    {
                        case "AddQueue":
                                ts = api.AddQueue(QProperties.ByCommaPipe(value));
                                Display(cmd, ts);
                            break;
                        case "RemoveQueue":
                            ts = api.RemoveQueue(key);
                                Display(cmd, ts);
                            break;
                        case "QueueExists":
                            ts = api.QueueExists(key);
                                Display(cmd, ts);
                            break;
                        case "HoldEnqueue":
                        case "ReleaseHoldEnqueue":
                        case "HoldDequeue":
                        case "ReleaseHoldDequeue":
                        case "EnableQueue":
                        case "DisableQueue":
                        case "TopicAdd":
                        case "TopicRemove":
                        case "TopicPublish":
                        case "TopicSubscribe":
                        case "TopicRemoveItem":
                        case "TopicHold":
                        case "TopicHoldRelease":
                        case "TopicSubscribeHold":
                        case "TopicSubscribeRelease":
                        case "TopicSubscribeAdd":
                        case "TopicSubscribeRemove":
                            ts = api.OperateQueue(EnumExtension.Parse<QueueCmdOperation>(cmd), key);
                                Display(cmd, ts);
                            break;
                        case "Exists":
                        case "QueueProperty":
                        case "ReportQueueList":
                        case "ReportQueueItems":
                        case "ReportQueueStatistic":
                        case "PerformanceCounter":
                        case "QueueCount":
                            ts = api.Report(EnumExtension.Parse<QueueCmdReport>(cmd), key);
                            Display(cmd, ts);
                            break;
                        case "Reply":
                            ts = api.Reply();
                            Display(cmd, ts);
                            break;
                        default:
                            ok = false;
                            Display(cmd, "Unknown command");
                            break;
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                ok = false;
                Console.WriteLine("Error: {0}", ex.Message);
            }
            finally
            {
                watch.Stop();
                if (ok)
                    Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
            }
        }


        #region Display
        static void Display(string cmd, IQueueMessage item)
        {
            if (item == null)
            {
                Console.WriteLine("command - {0} not responsed", cmd);
                return;
            }
            Console.WriteLine("command - {0} response - :", cmd);
            Console.WriteLine(item.ToJson());
            if (ConsoleController.EnableLog)
            {
                Netlog.InfoFormat("command - {0} counter - {1} :", cmd, item.ToJson());
            }
        }
        static void Display(string cmd, TransStream item)
        {
            if (item == null)
            {
                Console.WriteLine("command - {0} not responsed", cmd);
                return;
            }
            Console.WriteLine("command - {0} response - :", cmd);

            Console.WriteLine(Strings.ReflatJson(item.ReadToJson(),true));//.ReadJson());
            if (ConsoleController.EnableLog)
            {
                Netlog.InfoFormat("command - {0} counter - {1} :", cmd, item.ReadToJson());
            }

        }
        static void Display(string cmd, string val, int counter)
        {
            Console.WriteLine("command - {0} counter - {1} :", cmd, counter);
            Console.WriteLine(val);
            if (ConsoleController.EnableLog)
            {
                Netlog.InfoFormat("command - {0} counter - {1} :", cmd, counter);
                Netlog.Info(val);
            }
        }
        static void Display(string cmd, string val)
        {
            Console.WriteLine("command - {0} :", cmd);
            Console.WriteLine(val);
            if (ConsoleController.EnableLog)
            {
                Netlog.InfoFormat("command - {0} :", cmd);
                Netlog.Info(val);
            }
        }
        static void Display(string cmd, string val, params string[] args)
        {
            Console.WriteLine("command - {0} :", cmd);
            Console.WriteLine(val, args);
            if (ConsoleController.EnableLog)
            {
                Netlog.InfoFormat("command - {0} :", cmd);
                Netlog.InfoFormat(val, args);
            }
        }
        static void DisplayArray(string cmd, string[] arr)
        {
            if (arr == null)
                Console.WriteLine("{0} not found", cmd);
            else
            {
                Console.WriteLine("command - {0} :", cmd);
                foreach (string s in arr)
                {
                    Console.WriteLine(s);
                }
            }

        }

        internal static void DisplayArgs()
        {
            Console.WriteLine("add-queue        value: QueueName =| ServerPath =| Mode =[Memory, Persistent, FileStream, Db, Rout] | IsTrans = false | MaxRetry = 3 | ReloadOnStart = true | ConnectTimeout = 0 | TargetPath =| IsTopic = false | CommitMode =|[OnDisk, OnMemory1, None]");
            Console.WriteLine("transform        value:(binary, json)");
            Console.WriteLine("protocol         value:(tcp , pipe, http)");

        }

        internal static void DisplayMenu()
        {
            Console.WriteLine("?");
            Console.WriteLine("all");
            Console.WriteLine("commands");
            Console.WriteLine("quit");
            Console.WriteLine("transform        (binary, json)");
            Console.WriteLine("protocol         (tcp , pipe, http)");

        }
        internal static void DisplayCommands()
        {

            Console.WriteLine("QueueCmdOperation");
            Console.WriteLine("=================");
            Console.WriteLine("add-queue");
            Console.WriteLine("remove-queue");
            Console.WriteLine("queue-exists");

            Console.WriteLine("QueueCmdOperation");
            Console.WriteLine("=================");
            Console.WriteLine("backup-queue");
            Console.WriteLine("backup-all");
            Console.WriteLine("load-from-backup");
            Console.WriteLine("hold-enqueue");
            Console.WriteLine("release-hold-enqueue");
            Console.WriteLine("hold-dequeue");
            Console.WriteLine("release-hold-dequeue");
            Console.WriteLine("enable-queue");
            Console.WriteLine("disable-queue");
            Console.WriteLine("topic-add");
            Console.WriteLine("topic-remove");
            Console.WriteLine("topic-publish");
            Console.WriteLine("topic-subscribe");
            Console.WriteLine("topic-removeItem");
            Console.WriteLine("topic-hold");
            Console.WriteLine("topic-hold-release");
            Console.WriteLine("topic-subscribe-hold");
            Console.WriteLine("topic-subscribe-release");
            Console.WriteLine("topic-subscribe-add");
            Console.WriteLine("topic-subscribe-remove");

            Console.WriteLine("QueueCmdReport");
            Console.WriteLine("==============");
            Console.WriteLine("exists");
            Console.WriteLine("queue-property");
            Console.WriteLine("report-queuelist");
            Console.WriteLine("report-queueitems");
            Console.WriteLine("report-queuestatistic");
            Console.WriteLine("performance-counter");
            Console.WriteLine("queue-count  [/start monitor]");
            Console.WriteLine("queue-count-all  [/start monitor]");
            Console.WriteLine("usage    [/start monitor]");
            Console.WriteLine("reply");
        }

        #endregion

        #region monitor

        static void StartMonitor(Action action)
        {
            Console.WriteLine("Monitor start... to stop enter /stop");
            MonitorState = true;
            int count = 0;
            do
            {
                if (count > 5)
                {
                    Console.WriteLine("continue ? y\\n");
                    if(Console.ReadKey().Key == ConsoleKey.N)
                    {
                        MonitorState = false;
                        break;
                    }
                    count=0;
                }
                count++;
                action();
                Thread.Sleep(7000);
            } while (MonitorState);
            Console.WriteLine("Monitor stoped");
        }

        #endregion

        /*
                public static void DoCommandSync(NetProtocol cmdProtocol, string transform, string cmd, string name, string keys, string field)
                {
                    var api = ManagementApi.Get(cmdProtocol);
                    Stopwatch watch = Stopwatch.StartNew();
                    string json = null;
                    bool ok = true;
                    try
                    {

                        if (transform == "json")
                        {
                            switch (cmd.ToLower())
                            {
                                case "printentityvalues":
                                    {
                                        var ks = api.DoHttpJson(command: "GetEntityKeys", entityName: name, primaryKey: keys);

                                        if (string.IsNullOrWhiteSpace(ks))
                                        {
                                            throw new Exception("Entity keys not found");
                                        }
                                        string[] arr = JsonSerializer.Deserialize<string[]>(ks);

                                        if (arr == null || arr.Length == 0)
                                        {
                                            Display(cmd, "items not found!");
                                        }
                                        else
                                        {
                                            int counter = 0;
                                            foreach (var k in arr)
                                            {
                                                json = api.DoHttpJson(command: "GetRecord", entityName: name, primaryKey: k);
                                                counter++;
                                                Display(cmd, json, counter);
                                            }

                                            Display(cmd, "finished items: " + arr.Length.ToString());
                                        }
                                    }
                                    break;
                                default:

                                    json = api.DoHttpJson(command: cmd, entityName: name, primaryKey: keys, field: field, pretty: true);
                                    Display(cmd, json);
                                    break;
                            }
                        }
                        else
                        {
                            switch (cmd.ToLower())
                            {
                                case "getjson":
                                    {
                                        json = api.GetJson(name, keys.Split(';'), JsonFormat.Indented);
                                        Display(cmd, json);
                                    }
                                    break;
                                case "getentity":
                                    {
                                        var entity = api.GetEntity<GenericRecord>(name, keys.Split(';'));
                                        json = JsonSerializer.Serialize(entity, null, JsonFormat.Indented);
                                        Display(cmd, json);
                                    }
                                    break;
                                case "get":
                                    {
                                        var item = api.Get(name, keys.Split(';'), field);
                                        json = JsonSerializer.Serialize(item, null, JsonFormat.Indented);
                                        Display(cmd, json);
                                    }
                                    break;
                                case "getrecord":
                                    {
                                        var record = api.GetRecord(name, keys.Split(';'));
                                        json = JsonSerializer.Serialize(record, null, JsonFormat.Indented);
                                        Display(cmd, json);
                                    }
                                    break;
                                case "remove":
                                    {
                                        api.Remove(name);
                                        Display(cmd, "Sync item: " + name + " was removed");
                                    }
                                    break;
                                case "getallentitynames":
                                    var names = api.GetAllEntityNames().ToArray();
                                    DisplayArray(cmd, names);
                                    break;
                                case "getentitykeys":
                                    var ks = api.GetEntityKeys(name).ToArray();
                                    DisplayArray(cmd, ks);
                                    break;
                                case "printentityvalues":
                                    {
                                        var arr = api.GetEntityKeys(name).ToArray();
                                        if (arr == null || arr.Length == 0)
                                        {
                                            Display(cmd, "items not found!");
                                        }
                                        else
                                        {
                                            int counter = 0;
                                            foreach (var k in arr)
                                            {
                                                var record = api.GetRecord(name, k.Split(';'));
                                                json = JsonSerializer.Serialize(record, null, JsonFormat.Indented);
                                                counter++;
                                                Display(cmd, json, counter);
                                            }

                                            Display(cmd, "finished items: " + arr.Length.ToString());
                                        }
                                    }
                                    break;
                                case "getentityitemscount":
                                    {
                                        var count = api.GetEntityItemsCount(name);
                                        Display(cmd, count.ToString());
                                    }
                                    break;
                                case "refresh":
                                    api.Refresh(name);
                                    Display(cmd, "Refresh sync queue item started");
                                    break;
                                case "refreshall":
                                    api.RefreshAll();
                                    Display(cmd, "Refresh all sync queue items started");
                                    break;
                                case "reset":
                                    api.Reset();
                                    Display(cmd, "Sync queue restart");
                                    break;
                                case "reply":
                                    var r = api.Reply(name);
                                    Display(cmd, name);
                                    break;

                                default:
                                    ok = false;
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        Console.WriteLine("Error: {0}", ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        if (ok)
                            Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
                    }
                }
                public static void DoCommandSession(NetProtocol cmdProtocol, string transform, string cmd, string sessionId, string key, string value)
                {
                    int expiration = 60;
                    bool ok = true;
                    string json = null;
                    var api = ManagementApi.Get(cmdProtocol);
                    Stopwatch watch = Stopwatch.StartNew();
                    try
                    {
                        if (transform == "json")
                        {
                            json = api.DoHttpJson(command: cmd, groupId: sessionId, id: key, value: value, pretty: true);
                            Display(cmd, json);
                        }
                        else
                        {
                            switch (cmd.ToLower())
                            {
                                case "createsession":
                                    expiration = Types.ToInt(key);
                                    api.CreateSession(sessionId, "*", expiration, null);
                                    Display(cmd, "Session created");
                                    break;
                                case "add":
                                    {
                                        var ack = api.Add(sessionId, key, value, expiration);
                                        Display(cmd, ack.ToString());//, "Session item {0}", res > 0 ? "not added" : "added");
                                    }
                                    break;
                                case "set":
                                    {
                                        var ack = api.Set(sessionId, key, value, expiration);
                                        Display(cmd, ack.ToString());//, "Session item {0}", res > 0 ? "not added" : "added");
                                    }
                                    break;
                                case "get":
                                    json = api.GetJson(sessionId, key, JsonFormat.Indented);
                                    Display(cmd, json);
                                    break;
                                case "getsessionitems":
                                    var session = api.GetSessionItems(sessionId);
                                    Display(cmd, RemoteQueueApi.ToJson(session, JsonFormat.Indented));
                                    break;
                                case "getallsessionskeys":
                                    var ks = api.ViewAllSessionsKeys().ToArray();
                                    DisplayArray(cmd, ks);
                                    break;
                                case "removesession":
                                    api.RemoveSession(sessionId);
                                    Display(cmd, "session {0} will remove", sessionId);
                                    break;
                                case "remove":
                                    api.Remove(sessionId, key);
                                    Display(cmd, "session item {0},{1} will remove", sessionId, key);
                                    break;
                                case "reply":
                                    var r = api.Reply(key);
                                    Display(cmd, key);
                                    break;
                                default:
                                    ok = false;
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        Console.WriteLine("Error: {0}", ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        if (ok)
                            Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
                    }
                }
                public static void DoCommandData(NetProtocol cmdProtocol, string transform, string cmd, string db, string tableName, string keys, string field)
                {
                    var api = ManagementApi.Get(cmdProtocol);
                    Stopwatch watch = Stopwatch.StartNew();
                    string json = null;
                    bool ok = true;
                    try
                    {

                        if (transform == "json")
                        {
                            switch (cmd.ToLower())
                            {
                                default:
                                    json = api.DoHttpJson(command: cmd, db: db, tableName: tableName, primaryKey:keys, field: field, pretty: true);
                                    Display(cmd, json);
                                    break;
                            }
                        }
                        else
                        {
                            switch (cmd.ToLower())
                            {
                                default:
                                    var o=api.DoCustom(command: cmd, db: db, tableName: tableName, primaryKey: keys, field: field);
                                    json = JsonSerializer.Serialize(o, null, JsonFormat.Indented);
                                    Display(cmd, json);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        Console.WriteLine("Error: {0}", ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        if (ok)
                            Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
                    }
                }

                public static void DoCommandQueueJson(NetProtocol cmdProtocol, string cmd, string key, string value)
                {
                    //string RemoteHostName = QueueDefaults.DefaultBundleHostName;
                    var api = ManagementApi.Get(cmdProtocol);

                    bool ok = true;
                    Stopwatch watch = Stopwatch.StartNew();
                    try
                    {
                        switch (cmd.ToLower())
                        {
                            case "get":
                                {
                                    var json = api.DoHttpJson(command: "Get", key: key, pretty: true);

                                    //var json = api.SendJsonDuplex(API.QueueCmd.Get, key, value, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "add":
                                {
                                    var json = api.Enqueue(command: "Add", key: key, value: value, pretty: true);
                                    Display(cmd, json);
                                    //var res = api.SendJsonDuplex(API.QueueCmd.Add, key, value, true);
                                    //Display(cmd, res.ToString());
                                }
                                break;
                            case "set":
                                {
                                    var json = api.DoHttpJson(command: "Set", key: key, value: value, pretty: true);
                                    Display(cmd, json);
                                    //var res = api.SendJsonDuplex(API.QueueCmd.Set,key, value, 0);
                                    //Display(cmd, res.ToString());
                                }
                                break;
                            case "remove":
                                {
                                    var json = api.DoHttpJson(command: "Remove", key: key, pretty: true);
                                    Display(cmd, json);

                                    //api.SendJsonDuplex(API.QueueCmd.Remove, key, value, true);
                                    //Display(cmd, "Queue item will remove.");
                                }
                                break;
                            case "reply":
                                {
                                    var json = api.DoHttpJson(command: "Reply", key: key, pretty: true);
                                    Display(cmd, json);

                                    //var r = api.SendJsonDuplex(API.QueueCmd.Reply, key, value, true);
                                    //Display(cmd, key);
                                }
                                break;
                            default:
                                ok = false;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        Console.WriteLine("Error: {0}", ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        if (ok)
                            Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
                    }
                }
                public static void DoCommandSyncJson(NetProtocol cmdProtocol, string cmd, string name, string keys, string field)
                {
                    //string RemoteHostName = QueueDefaults.DefaultBundleHostName;
                    var api= ManagementApi.Get(cmdProtocol);

                    Stopwatch watch = Stopwatch.StartNew();
                    bool ok = true;
                    try
                    {
                        string json = null;
                        string key = null;
                        switch (cmd.ToLower())
                        {
                            case "getjson":
                                {
                                    if (name==null || keys == null)
                                    {
                                        throw new ArgumentNullException("name|keys");
                                    }
                                    json = api.DoHttpJson(command: "GetAs", entityName: name, primaryKey: keys, pretty: true);

                                    //key = string.Format("{0}:{1}", name, string.Join("_", keys));
                                    //json = api.SendJsonDuplex(API.SyncQueueCmd.GetAs, key, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "getentity":
                                {
                                    json = api.DoHttpJson(command: "GetEntity", entityName: name, primaryKey: keys, pretty: true);

                                    //key = string.Format("{0}:{1}", name, string.Join("_", keys));
                                    //json = api.SendJsonDuplex(API.SyncQueueCmd.GetEntity, key, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "get":
                                {
                                    json = api.DoHttpJson(command: "Get", entityName: name, primaryKey: keys,field: field, pretty: true);

                                    //key = string.Format("{0}:{1}", name, string.Join("_", keys));
                                    //json = api.SendJsonDuplex(API.SyncQueueCmd.Get, key, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "getrecord":
                                {
                                    if(keys == null)
                                    {
                                        throw new ArgumentNullException("keys");
                                    }
                                    json = api.DoHttpJson(command:"GetRecord",entityName: name,primaryKey: keys.Replace(",",KeySet.Separator),pretty:true);

                                    //key = string.Format("{0}:{1}", name, string.Join("_", keys));
                                    //json = api.SendJsonDuplex(API.SyncQueueCmd.GetRecord, key, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "remove":
                                {
                                    json = api.DoHttpJson(command: "Remove", entityName: name, primaryKey: keys, pretty: true);

                                    //json = api.SendJsonDuplex(API.SyncQueueCmd.Remove, name, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "getallentitynames":
                                {
                                    json = api.DoHttpJson(command: "GetAllEntityNames", entityName: name, primaryKey: keys, pretty: true);

                                    //json = api.SendJsonDuplex(API.SyncQueueCmd.GetAllEntityNames, name, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "getentitykeys":
                                {
                                    json = api.DoHttpJson(command: "GetEntityKeys", entityName: name, primaryKey: keys, pretty: true);

                                    //json = api.SendJsonDuplex(API.SyncQueueCmd.GetEntityKeys, name, true);
                                    string[] arrky = JsonSerializer.Deserialize<string[]>(json);
                                    DisplayArray(cmd, arrky);
                                }
                                break;
                            case "printentityvalues":
                                {

                                    var ks = api.DoHttpJson(command: "GetEntityKeys", entityName: name, primaryKey: keys);


                                    //var ks = api.SendJsonDuplex(API.SyncQueueCmd.GetEntityKeys, name);
                                    if (string.IsNullOrWhiteSpace(ks))
                                    {
                                        throw new Exception("Entity keys not found");
                                    }
                                    //var arr = api.GetEntityKeys(name).ToArray();
                                    string[] arr = JsonSerializer.Deserialize<string[]>(ks);

                                    //Display(cmd, "finished items: " + arr.Length.ToString());

                                    if (arr == null || arr.Length == 0)
                                    {
                                        Display(cmd, "items not found!");
                                    }
                                    else
                                    {
                                        int counter = 0;
                                        foreach (var k in arr)
                                        {
                                            //string recordkey =  string.Join(KeySet.Separator, k.Split(',',KeySet.SeparatorCh));
                                            json = api.DoHttpJson(command: "GetRecord", entityName: name, primaryKey: k);

                                            //string recordkey = string.Format("{0}:{1}", name, string.Join("_", k.Split(';')));
                                            //json = api.SendJsonDuplex(API.SyncQueueCmd.GetRecord, recordkey, true);

                                            counter++;
                                            Display(cmd, json, counter);
                                        }

                                        Display(cmd, "finished items: " + arr.Length.ToString());
                                    }
                                }
                                break;
                            case "getentityitemscount":
                                {
                                    json = api.DoHttpJson(command: "GetEntityItemsCount", entityName: name, primaryKey: keys, pretty: true);
                                    //json = api.SendJsonDuplex(API.SyncQueueCmd.GetEntityItemsCount, name, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "refresh":
                                json = api.DoHttpJson(command: "Refresh", entityName: name, primaryKey: keys, pretty: true);
                                Display(cmd, json);
                                //api.SendJsonDuplex(API.SyncQueueCmd.Refresh, name);
                                //Display(cmd, "Refresh sync queue item started");
                                break;
                            case "refreshall":
                                json = api.DoHttpJson(command: "RefreshAll", entityName: name, primaryKey: keys, pretty: true);
                                Display(cmd, json);

                                //api.SendJsonDuplex(API.SyncQueueCmd.RefreshAll, name);
                                //Display(cmd, "Refresh all sync queue items started");
                                break;
                            case "reset":
                                json = api.DoHttpJson(command: "Reset", entityName: name, primaryKey: keys, pretty: true);
                                Display(cmd, json);
                                //api.SendJsonDuplex(API.SyncQueueCmd.Reset, name);
                                //Display(cmd, "Sync queue restart");
                                break;
                            case "reply":
                                json = api.DoHttpJson(command: "Reply", entityName: name, primaryKey: keys, pretty: true);
                                Display(cmd, json);

                                //json = api.SendJsonDuplex(API.SyncQueueCmd.Reply, name, true);
                                //Display(cmd, json);
                                break;
                            default:
                                ok = false;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        Console.WriteLine("Error: {0}", ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        if (ok)
                            Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
                    }
                }
                public static void DoCommandSessionJson(NetProtocol cmdProtocol, string cmd, string sessionId, string key, string value)
                {
                    //string RemoteHostName = QueueDefaults.DefaultBundleHostName;
                    var api = ManagementApi.Get(cmdProtocol);
                    bool ok = true;
                    int expiration = 60;
                    Stopwatch watch = Stopwatch.StartNew();
                    try
                    {
                        string json = null;

                        switch (cmd.ToLower())
                        {
                            case "createsession":
                                {
                                    json = api.DoHttpJson(command: "CreateSession",  groupId:sessionId, id: key, value: value, pretty: true);
                                    Display(cmd, json);

                                    //json = api.SendJsonDuplex(API.SessionCmd.CreateSession, "*", null, expiration, sessionId, true);
                                    //Display(cmd, "Session created");
                                }
                                break;
                            case "add":
                                {
                                    json = api.DoHttpJson(command: "Add", groupId: sessionId, id: key, value: value,expiration: expiration, pretty: true);

                                    //json = api.SendJsonDuplex(API.SessionCmd.Add, key, value, expiration, sessionId, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "set":
                                {
                                    json = api.DoHttpJson(command: "Set", groupId: sessionId, id: key, value: value, expiration: expiration, pretty: true);
                                    //json = api.SendJsonDuplex(API.SessionCmd.Set, key, value, expiration, sessionId, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "get":
                                {
                                    json = api.DoHttpJson(command: "Get", groupId: sessionId, id: key, pretty: true);

                                    //json = api.SendJsonDuplex(API.SessionCmd.Get, key, null, 0, sessionId, true);
                                    Display(cmd, json);
                                }
                                break;
                            case "getsessionitems":
                                json = api.DoHttpJson(command: "GetSessionItems", groupId: sessionId, id: key, pretty: true);
                                //json = api.SendJsonDuplex(API.SessionCmd.GetSessionItems, null, null, 0, sessionId, true);
                                Display(cmd, json);
                                break;
                            case "getallsessionskeys":
                                json = api.DoHttpJson(command: "ViewAllSessionsKeys", groupId: sessionId, id: key, pretty: true);
                                //json = api.SendJsonDuplex(new QueueMessage() { Command= API.SessionCmd.ViewAllSessionsKeys },true);// API.SessionCmd.ViewAllSessionsKeys, null, true);
                                Display(cmd, json);
                                break;
                            case "removesession":
                                json = api.DoHttpJson(command: "RemoveSession", groupId: sessionId, id: key, pretty: true);
                                //json = api.SendJsonDuplex(API.SessionCmd.RemoveSession, null, null, 0, sessionId, true);
                                Display(cmd, "session {0} will remove", sessionId);
                                break;
                            case "remove":
                                json = api.DoHttpJson(command: "Remove", groupId: sessionId, id: key, pretty: true);
                                //json = api.SendJsonDuplex(API.SessionCmd.Remove, key, null, 0, sessionId, true);
                                Display(cmd, "session item {0},{1} will remove", sessionId, key);
                                break;
                            case "reply":
                                json = api.DoHttpJson(command: "Reply", groupId: sessionId, id: key, pretty: true);
                                //json = api.SendJsonDuplex(API.SessionCmd.Reply, key);
                                Display(cmd, json);
                                break;
                            default:
                                ok = false;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        Console.WriteLine("Error: {0}", ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        if (ok)
                            Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
                    }
                }

                public static void DoCommandManager(string cmd, string key)
                {

                    bool ok = true;
                    Stopwatch watch = Stopwatch.StartNew();
                    try
                    {
                        switch (cmd.ToLower())
                        {
                            case "statistic":
                                DoPerformanceReport(key);
                                break;
                            case "counterstate":
                                DoPerformanceStateReport(key);
                                break;
                            case "counterreset":
                                DoResetPerformanceCounter();
                                break;
                            case "printlog":
                                DoLog();
                                break;
                            default:
                                ok = false;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        Console.WriteLine("Error: {0}", ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        if (ok)
                            Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
                    }
                }

                #region Display
                static void Display(string cmd, string val, int counter)
                {
                    Console.WriteLine("command - {0} counter - {1} :", cmd,counter);
                    Console.WriteLine(val);
                    if(Controller.EnableLog)
                    {
                        Netlog.InfoFormat("command - {0} counter - {1} :", cmd, counter);
                        Netlog.Info(val);
                    }
                }
                static void Display(string cmd, string val)
                {
                    Console.WriteLine("command - {0} :", cmd);
                    Console.WriteLine(val);
                    if (Controller.EnableLog)
                    {
                        Netlog.InfoFormat("command - {0} :", cmd);
                        Netlog.Info(val);
                    }
                }
                static void Display(string cmd, string val, params string[] args)
                {
                    Console.WriteLine("command - {0} :", cmd);
                    Console.WriteLine(val, args);
                    if (Controller.EnableLog)
                    {
                        Netlog.InfoFormat("command - {0} :", cmd);
                        Netlog.InfoFormat(val, args);
                    }
                }
                static void DisplayArray(string cmd, string[] arr)
                {
                    if (arr == null)
                        Console.WriteLine("{0} not found", cmd);
                    else
                    {
                        Console.WriteLine("command - {0} :", cmd);
                        foreach (string s in arr)
                        {
                            Console.WriteLine(s);
                        }
                    }

                }
                #endregion
                static void DoPerformanceReport(string cmdType)
                {
                    IQueuePerformanceReport report = null;
                    if (cmdType == "all" || cmdType == null)
                    {
                        report = ManagementApi.GetPerformanceReport();
                    }
                    else
                    {
                        var agentType = GetQueueAgentType(cmdType);
                        report = ManagementApi.GetAgentPerformanceReport(agentType);
                    }

                    if (report == null)
                    {
                        Console.WriteLine("Invalid queue performance property");
                    }
                    else
                    {
                        Console.WriteLine("Queue Performance Report");
                        var dt = report.PerformanceReport;

                        var json = Nistec.Serialization.JsonSerializer.Serialize(dt, null, JsonFormat.Indented);
                        Console.WriteLine(json);
                    }
                    Console.WriteLine();
                }


                static void DoPerformanceStateReport(string cmdType)
                {
                    DataTable report = null;
                    if (cmdType == "all" || cmdType == null)
                    {
                        report = ManagementApi.GetStateCounterReport();
                    }
                    else
                    {
                        var agentType = GetQueueAgentType(cmdType);
                        report = ManagementApi.GetStateCounterReport(agentType);
                    }

                    if (report == null)
                    {
                        Console.WriteLine("Invalid queue performance property");
                    }
                    else
                    {
                        Console.WriteLine("Queue State Report");
                        var json = Nistec.Serialization.JsonSerializer.Serialize(report, null, JsonFormat.Indented);
                        Console.WriteLine(json);
                    }
                    Console.WriteLine();
                }

                static void DoResetPerformanceCounter()
                {
                    ManagementApi.ResetPerformanceCounter();
                }

                static void DoLog()
                {
                    string log = ManagementApi.QueueLog();
                    Console.WriteLine("Queue Log");
                    Console.WriteLine(log);
                    Console.WriteLine();
                }

                static QueueAgentType GetQueueAgentType(string cmdType)
                {
                    switch (cmdType)
                    {
                        case SectionType.queue:
                            return QueueAgentType.Queue;
                        case SectionType.sync:
                            return QueueAgentType.SyncQueue;
                        case SectionType.session:
                            return QueueAgentType.SessionQueue;
                        default:
                            return QueueAgentType.Queue;
                    }
                }
        */
    }

    //class JsonCmdController
    //{
    //    public static void DoCommandQueueJson(NetProtocol cmdProtocol, string cmd, string key, string value)
    //    {
    //        string RemoteHostName = QueueApiSettings.RemoteQueueHostName;
    //        //EnableRemoteException = QueueApiSettings.EnableRemoteException;

    //        bool ok = true;
    //        Stopwatch watch = Stopwatch.StartNew();
    //        try
    //        {
    //            switch (cmd.ToLower())
    //            {
    //                case "getvalue":
    //                    var json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.QueueCmd.GetValue, key, value);
    //                    Display(cmd, json);
    //                    break;
    //                case "additem":
    //                    var res = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName,0, API.QueueCmd.AddItem,key, value);
    //                    Display(cmd, res.ToString());
    //                    break;
    //                case "removeitem":
    //                    RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.QueueCmd.RemoveItem, key, value);
    //                    Display(cmd, "Queue item will remove.");
    //                    break;
    //                case "reply":
    //                    var r = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.QueueCmd.Reply, key, value);
    //                    Display(cmd, key);
    //                    break;
    //                default:
    //                    ok = false;
    //                    break;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            ok = false;
    //            Console.WriteLine("Error: {0}", ex.Message);
    //        }
    //        finally
    //        {
    //            watch.Stop();
    //            if (ok)
    //                Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
    //        }
    //    }
    //    public static void DoCommandQueueSyncJson(NetProtocol cmdProtocol, string cmd, string name, string keys)
    //    {
    //        string RemoteHostName = QueueApiSettings.RemoteSyncQueueHostName;

    //        Stopwatch watch = Stopwatch.StartNew();
    //        bool ok = true;
    //        try
    //        {
    //            string json = null;
    //            string key = null;
    //            switch (cmd.ToLower())
    //            {
    //                case "getjson":
    //                    {
    //                        key = string.Format("{0}:{1}", name, string.Join("_", keys));
    //                        json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.GetAs, key);
    //                        Display(cmd, json);
    //                    }
    //                    break;
    //                case "getrecord":
    //                    {
    //                        key = string.Format("{0}:{1}", name, string.Join("_", keys));
    //                        json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.GetRecord, key);
    //                        Display(cmd, json);
    //                    }
    //                    break;
    //                case "getallentitynames":
    //                    {
    //                        key = string.Format("{0}:{1}", name, string.Join("_", keys));
    //                        json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.GetAllEntityNames, key);
    //                        Display(cmd, json);
    //                    }
    //                    break;
    //                case "getentitykeys":
    //                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.GetEntityKeys, name);
    //                    string[] arrky = JsonSerializer.Deserialize<string[]>(json);
    //                    DisplayArray(cmd, arrky);
    //                    break;
    //                case "printentityvalues":
    //                    {
    //                        var ks = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.GetEntityKeys, name);
    //                        if (string.IsNullOrWhiteSpace(ks))
    //                        {
    //                            throw new Exception("Entity keys not found");
    //                        }
    //                        //var arr = api.GetEntityKeys(name).ToArray();
    //                        string[] arr= JsonSerializer.Deserialize<string[]>(ks);

    //                        if (arr == null || arr.Length == 0)
    //                        {
    //                            Display(cmd, "items not found!");
    //                        }
    //                        else
    //                        {
    //                            int count = Types.ToInt(keys);
    //                            if (count <= 0)
    //                                count = 1;
    //                            int counter = 0;
    //                            for (int i = 0; i < count; i++)
    //                            {
    //                                foreach (var k in arr)
    //                                {
    //                                    string recordkey = string.Format("{0}:{1}", name, string.Join("_", k.Split(';')));
    //                                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.GetRecord, recordkey);
    //                                    counter++;
    //                                    Display(cmd, json, counter);
    //                                }

    //                                Display(cmd, "finished items: " + arr.Length.ToString());
    //                            }
    //                        }
    //                    }
    //                    break;
    //                case "getentityitemscount":
    //                    {
    //                        json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.GetEntityItemsCount, name);
    //                        Display(cmd, json);
    //                    }
    //                    break;
    //                case "refreshitem":
    //                    RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.RefreshItem, name);
    //                    Display(cmd, "Refresh sync queue item started");
    //                    break;
    //                case "refresh":
    //                    RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.Refresh, name);
    //                    Display(cmd, "Refresh all sync queue items started");
    //                    break;
    //                case "reset":
    //                    RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.Reset, name);
    //                    Display(cmd, "Sync queue restart");
    //                    break;
    //                case "reply":
    //                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncQueueCmd.Reply, name);
    //                    Display(cmd, json);
    //                    break;

    //                default:
    //                    ok = false;
    //                    break;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            ok = false;
    //            Console.WriteLine("Error: {0}", ex.Message);
    //        }
    //        finally
    //        {
    //            watch.Stop();
    //            if (ok)
    //                Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
    //        }
    //    }
    //    public static void DoCommandSessionJson(NetProtocol cmdProtocol, string cmd, string sessionId, string key)
    //    {
    //        string RemoteHostName = QueueApiSettings.RemoteSessionHostName;
    //        bool ok = true;
    //        Stopwatch watch = Stopwatch.StartNew();
    //        try
    //        {
    //            string json = null;

    //            switch (cmd.ToLower())
    //            {
    //                case "getsessionitem":
    //                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SessionCmd.GetSessionItem, key,null,0,sessionId);
    //                    Display(cmd, json);
    //                    break;
    //                case "getallsessionskeys":
    //                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SessionCmd.GetAllSessionsKeys, null);
    //                    var ks = API.SessionQueueApi.Get(cmdProtocol).GetAllSessionsKeys().ToArray();
    //                    DisplayArray(cmd, ks);
    //                    break;
    //                case "removesession":
    //                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SessionCmd.RemoveSession, null, null, 0, sessionId);
    //                    API.SessionQueueApi.Get(cmdProtocol).RemoveSession(sessionId);
    //                    Display(cmd, "session {0} will remove", sessionId);
    //                    break;
    //                case "removeitem":
    //                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SessionCmd.RemoveSessionItem, key, null, 0, sessionId);
    //                    Display(cmd, "session item {0},{1} will remove", sessionId, key);
    //                    break;
    //                case "reply":
    //                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SessionCmd.Reply, key);
    //                    Display(cmd, json);
    //                    break;
    //                default:
    //                    ok = false;
    //                    break;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            ok = false;
    //            Console.WriteLine("Error: {0}", ex.Message);
    //        }
    //        finally
    //        {
    //            watch.Stop();
    //            if (ok)
    //                Console.WriteLine("Elapsed Milliseconds : " + watch.ElapsedMilliseconds);
    //        }
    //    }

    //    static void Display(string cmd, string val, int counter)
    //    {
    //        Console.WriteLine("command - {0} counter - {1} :", cmd, counter);
    //        Console.WriteLine(val);
    //        if (Controller.EnableLog)
    //        {
    //            Netlog.InfoFormat("command - {0} counter - {1} :", cmd, counter);
    //            Netlog.Info(val);
    //        }
    //    }
    //    static void Display(string cmd, string val)
    //    {
    //        Console.WriteLine("command - {0} :", cmd);
    //        Console.WriteLine(val);
    //        if (Controller.EnableLog)
    //        {
    //            Netlog.InfoFormat("command - {0} :", cmd);
    //            Netlog.Info(val);
    //        }
    //    }
    //    static void Display(string cmd, string val, params string[] args)
    //    {
    //        Console.WriteLine("command - {0} :", cmd);
    //        Console.WriteLine(val, args);
    //        if (Controller.EnableLog)
    //        {
    //            Netlog.InfoFormat("command - {0} :", cmd);
    //            Netlog.InfoFormat(val, args);
    //        }
    //    }
    //    static void DisplayArray(string cmd, string[] arr)
    //    {
    //        if (arr == null)
    //            Console.WriteLine("{0} not found", cmd);
    //        else
    //        {
    //            Console.WriteLine("command - {0} :", cmd);
    //            foreach (string s in arr)
    //            {
    //                Console.WriteLine(s);
    //            }
    //        }

    //    }

    //    //static void DoPerformanceReport(string cmdType)
    //    //{
    //    //    IQueuePerformanceReport report = null;
    //    //    if (cmdType == "all" || cmdType == null)
    //    //    {
    //    //        report = ManagerApi.GetPerformanceReport();
    //    //    }
    //    //    else
    //    //    {
    //    //        var agentType = GetQueueAgentType(cmdType);
    //    //        report = ManagerApi.GetAgentPerformanceReport(agentType);
    //    //    }

    //    //    if (report == null)
    //    //    {
    //    //        Console.WriteLine("Invalid queue performance property");
    //    //    }
    //    //    else
    //    //    {
    //    //        Console.WriteLine("Queue Performance Report");
    //    //        var dt = report.PerformanceReport;

    //    //        var json = Nistec.Serialization.JsonSerializer.Serialize(dt, null, JsonFormat.Indented);
    //    //        Console.WriteLine(json);
    //    //    }
    //    //    Console.WriteLine();
    //    //}


    //    //static void DoPerformanceStateReport(string cmdType)
    //    //{
    //    //    DataTable report = null;
    //    //    if (cmdType == "all" || cmdType == null)
    //    //    {
    //    //        report = ManagerApi.GetStateCounterReport();
    //    //    }
    //    //    else
    //    //    {
    //    //        var agentType = GetQueueAgentType(cmdType);
    //    //        report = ManagerApi.GetStateCounterReport(agentType);
    //    //    }

    //    //    if (report == null)
    //    //    {
    //    //        Console.WriteLine("Invalid queue performance property");
    //    //    }
    //    //    else
    //    //    {
    //    //        Console.WriteLine("Queue State Report");
    //    //        var json = Nistec.Serialization.JsonSerializer.Serialize(report, null, JsonFormat.Indented);
    //    //        Console.WriteLine(json);
    //    //    }
    //    //    Console.WriteLine();
    //    //}

    //    //static void DoResetPerformanceCounter()
    //    //{
    //    //    ManagerApi.ResetPerformanceCounter();
    //    //}

    //    //static void DoLog()
    //    //{
    //    //    string log = ManagerApi.QueueLog();
    //    //    Console.WriteLine("Queue Log");
    //    //    Console.WriteLine(log);
    //    //    Console.WriteLine();
    //    //}

    //    //static QueueAgentType GetQueueAgentType(string cmdType)
    //    //{
    //    //    switch (cmdType)
    //    //    {
    //    //        case SectionType.queue:
    //    //            return QueueAgentType.Queue;
    //    //        case SectionType.sync:
    //    //            return QueueAgentType.SyncQueue;
    //    //        case SectionType.session:
    //    //            return QueueAgentType.SessionQueue;
    //    //        default:
    //    //            return QueueAgentType.Queue;
    //    //    }
    //    //}
    //}
    #endregion


    #region Backup

    class BackupHandler
    {
        bool onBackgroundProcess=false;

        QueueApi api;
        public BackupHandler(string hostAddress)
        {
            var host = QueueHost.Parse(hostAddress);// ("tcp:127.0.0.1:15000?Netcell");
            api =  new QueueApi(host);
        }
              
        public void LoadFromBackup(string path, Action report)
        {
            if (onBackgroundProcess)
                return;
            try
            {
                Console.WriteLine("Start LoadFromBackup");

                onBackgroundProcess = true;

                //string path = GetRelayPath();

                if (Directory.Exists(path))
                {
                    string[] messages = Directory.GetFiles(path, "*.mcq", SearchOption.AllDirectories);
                    if (messages == null || messages.Length == 0)
                    {
                        return;
                    }

                    Console.WriteLine("{0} items found to ReEnqueue", messages.Length);

                    Netlog.InfoFormat("LoadFromBackup: {0} ", messages.Length);


                    foreach (string message in messages)
                    {
                        //while (this.Count > 1000)
                        //{

                        //    Thread.Sleep(1000);
                        //}

                        QueueMessage item = QueueMessage.ReadFile(message);
                        if (item != null)
                        {
                            api.EnqueueAsync(item, 5000, (ack)=> {
                                Console.WriteLine(ack.Print());
                            });
                        }
                        DeleteFile(message);
                        Thread.Sleep(100);
                    }
                    Netlog.Info("LoadFromBackup finished. ");
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

        public void AsyncLoadFromBackup(string path, Action report)
        {
            Task.Factory.StartNew(() => LoadFromBackup(path, report));
        }

        #region IO

        public static void DeleteFile(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    File.Delete(filename);
                }
                catch (Exception ex)
                {
                    Netlog.ErrorFormat("Error DeleteFile: {0}, {1} ", filename, ex.Message);
                }
            }
        }

        #endregion
    }

    #endregion
}
