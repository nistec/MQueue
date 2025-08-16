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
            //IQueueMessage qi = null;
            TransStream ts = null;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {

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
                                QueueRequest message = new QueueRequest(QueueCmd.LoadFromBackup)
                                {
                                    Host = key,
                                    //QCommand = QueueCmd.LoadFromBackup,
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
                        case "clearqueue":
                            ts = api.OperateQueue(QueueCmdOperation.ClearQueue, key);
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
                        case "querylabels":
                            ts = api.Report(QueueCmdReport.QueryLabels, key);
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
                        case "reportqueuecounters":
                            ts = api.Report(QueueCmdReport.ReportQueueCounters, key);
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
                        case "reportcountersall":
                            if (key == "/start" || value == "/start")
                            {
                                StartMonitor(() => {
                                    ts = api.Report(QueueCmdReport.ReportCountersAll);
                                    Display(cmd, ts);
                                });
                            }
                            else
                            {
                                ts = api.Report(QueueCmdReport.ReportCountersAll);
                                Display(cmd, ts);
                            }
                            break;
                        //case "queueitemscount":
                        //    if (value == "/start")
                        //    {
                        //        StartMonitor(() => {
                        //            ts = api.Report(QueueCmdReport.QueueCount, key);
                        //            Display(cmd, ts);
                        //        });
                        //    }
                        //    else
                        //    {
                        //        ts = api.Report(QueueCmdReport.QueueCount, key);
                        //        Display(cmd, ts);
                        //    }
                        //    break;
                        //case "queueitemscountall":
                        //    if (key == "/start" || value == "/start")
                        //    {
                        //        StartMonitor(() => {
                        //            ts = api.Report(QueueCmdReport.QueueCountAll);
                        //            Display(cmd, ts);
                        //        });
                        //    }
                        //    else
                        //    {
                        //        ts = api.Report(QueueCmdReport.QueueCountAll);
                        //        Display(cmd, ts);
                        //    }
                        //    break;
                        case "dbqueuereport":
                                ts = api.Report(QueueCmdReport.DbQueueReport, key);
                                Display(cmd, ts);
                            break;
                        case "dbqueueclear":
                            ts = api.Report(QueueCmdReport.DbQueueClear, key);
                            Display(cmd, ts);
                            break;
                        case "dbqueueclearitem":
                            ts = api.Report(QueueCmdReport.DbQueueClearItem, key);
                            Display(cmd, ts);
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
            Console.WriteLine("report-queue-statistic");
            Console.WriteLine("report-queue-counters [/start monitor]");
            Console.WriteLine("performance-counter");
            Console.WriteLine("queue-count  [/start monitor]");
            Console.WriteLine("queue-count-all  [/start monitor]");
            Console.WriteLine("report-counters-all  [/start monitor]");

            Console.WriteLine("dbqueuereport");
            Console.WriteLine("dbqueueclear");
            Console.WriteLine("dbqueueclearitem  [/key]");
            
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


    }

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
                            api.Enqueue(item, 5000, (ack)=> {
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
