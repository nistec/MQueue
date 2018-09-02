//licHeader
//===============================================================================================================
// System  : Nistec.Queue - Nistec.Queue Class Library
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
using Nistec.Channels.RemoteCache;
using Nistec.Serialization;
using Nistec.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nistec.Channels;

namespace Nistec
{

    class Controller
    {
        public static void Run(string[] args)
        {

            ServiceManager manager = new ServiceManager();

            NetProtocol cmdProtocol = NetProtocol.Tcp;
            string protocol = "tcp";
            string cmd = "";
            string operationType = "service";
            string cmdName = "";
            string cmdArg1 = "";

            SetCommands();

            manager.ShowServiceDetails();

            DisplayOperationType(operationType);

            DisplayMenu("menu", "", "");
            //DisplayOperationMessage();
            //operationType = GetOperationType(Console.ReadLine().ToLower(), operationType);

            //if (operationType == "quit")
            //{
            //    return;
            //}
            //Console.WriteLine("Current operation type : {0}.", operationType);


            while (cmd != "quit")
            {
                Console.WriteLine("Enter command :");

                cmd = Console.ReadLine();

                try
                {

                    string[] cmdargs = SplitCmd(cmd);
                    cmdName = GetCommandType(cmdargs[0], cmdName);
                    cmdArg1 = GetCommandType(cmdargs[1], cmdArg1);

                    switch (cmdName.ToLower())
                    {
                        case "menu":
                            DisplayMenu("menu", "", "");
                            break;
                        case "menu-items":
                            DisplayMenu("menu-items", operationType, "");
                            break;
                        case "operation-type":
                            DisplayOperationMessage();
                            operationType = GetOperationType(Console.ReadLine().ToLower(), operationType);
                            Console.WriteLine("Current cache type : {0}.", operationType);
                            break;
                        case "protocol":
                            Console.WriteLine("Choose protocol : tcp , pipe, http");
                            protocol = EnsureProtocol(Console.ReadLine().ToLower(), protocol);
                            cmdProtocol = GetProtocol(protocol, cmdProtocol);
                            Console.WriteLine("Current protocol : {0}.", protocol);
                            break;
                        case "args":
                            DisplayMenu("args", operationType, cmdArg1);
                            break;
                        case "quit":

                            break;
                        default:
                            switch (operationType)
                            {
                                case "service":

                                    switch (cmdName.ToLower())
                                    {
                                        case "status":
                                            manager.DispalyServiceStatus();
                                            break;
                                        case "details":
                                            manager.ShowServiceDetails();
                                            break;
                                        case "install":
                                            manager.DoServiceCommand(ServiceCmd.Install);
                                            break;
                                        case "uninstall":
                                            manager.DoServiceCommand(ServiceCmd.Uninstall);
                                            break;
                                        case "start":
                                            manager.DoServiceCommand(ServiceCmd.Start);
                                            break;
                                        case "stop":
                                            manager.DoServiceCommand(ServiceCmd.Stop);
                                            break;
                                        case "restart":
                                            manager.DoServiceCommand(ServiceCmd.Restart);
                                            break;
                                        case "paus":
                                            manager.DoServiceCommand(ServiceCmd.Pause);
                                            break;
                                    }
                                    //CmdController.DoCommandCache(cmdProtocol,cmdName, cmdArg1, cmdargs[2]);
                                    break;
                            }
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

        static Dictionary<string, string> serviceController = new Dictionary<string, string>();
        static void SetCommands()
        {
            serviceController.Add("status", "no args");
            serviceController.Add("details", "no args");
            serviceController.Add("install", "no args");
            serviceController.Add("uninstall", "no args");
            serviceController.Add("start", "no args");
            serviceController.Add("stop", "no args");
            serviceController.Add("restart", "no args");
            serviceController.Add("paus", "no args");
        }

        static string EnsureArg(string arg)
        {
            if (arg == null)
                return "";
            return arg.Replace("/", "").ToLower();
        }
        static void DisplayOperationMessage()
        {
            Console.WriteLine("Choose operation : service, or quit");
        }
        static void DisplayOperationType(string operationType)
        {
            Console.WriteLine("Current operation type : {0}.", operationType);
        }
        
        static void DisplayArgs(string cmdType, string arg)
        {
            string a = EnsureArg(arg);
            KeyValuePair<string, string> kv = new KeyValuePair<string, string>();
            switch (cmdType)
            {
                case "service":
                    kv = serviceController.Where(p => p.Key.ToLower() == a).FirstOrDefault();
                    break;
            }

            if (kv.Key != null)
                Console.WriteLine("commands: {0} Arguments: {1}.", kv.Key, kv.Value);
            else
                Console.WriteLine("Bad commands: {0} Arguments: {1}.", cmdType, arg);
        }

        static void DisplayCommands(string cmdType, string prefix)
        {
            switch (cmdType)
            {
                case "service":
                    string cmd = "";
                    foreach (string s in serviceController.Keys)
                    {
                        cmd += s + " ";
                    }
                    Console.WriteLine("{0}{1}.", prefix, cmd);
                    break;
            }

        }

        static void DisplayMenu(string cmdType, string operationType, string arg)
        {
            //string menu = "cache-type: remote-cache, remote-sync, remote-session";
            //Console.WriteLine(menu);

            switch (cmdType)
            {
                case "menu":
                    Console.WriteLine("Enter: operation-type, To change operation type");
                    Console.WriteLine("Enter: protocol, To change protocol (tcp, pipe, http)");
                    Console.WriteLine("Enter: menu, To display menu");
                    Console.WriteLine("Enter: menu-items, To display menu items for current cache-type");
                    Console.WriteLine("Enter: args, and /command to display command argument");
                    break;
                case "menu-items":
                    switch (operationType)
                    {
                        case "service":
                            DisplayCommands(operationType, "service commands: ");
                            break;
                        default:
                            Console.Write("Bad commands: Invalid operation-type");
                            break;
                    }
                    break;
                case "args":
                    if (arg != null && arg.StartsWith("/"))
                    {
                        DisplayArgs(operationType, arg);
                    }
                    break;
            }
            Console.WriteLine("");

        }
        static string[] SplitCmd(string cmd)
        {
            string[] args = new string[4] { "", "", "", "" };

            string[] cmdargs = cmd.SplitTrim(' ');
            if (cmdargs.Length > 0)
                args[0] = cmdargs[0];
            if (cmdargs.Length > 1)
                args[1] = cmdargs[1];
            if (cmdargs.Length > 2)
                args[2] = cmdargs[2];
            if (cmdargs.Length > 3)
                args[3] = cmdargs[3];
            return args;
        }

        static string GetOperationType(string cmd, string curItem)
        {
            switch (cmd.ToLower())
            {
                case "service":
                    return cmd.ToLower();
                default:
                    Console.WriteLine("Invalid operation-type {0}", cmd);
                    return curItem;
            }
        }
        static string GetCommandType(string cmd, string curItem)
        {
            if (cmd == "..")
                return curItem;
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

        public static int GetUsage(string procName)
        {

            System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName(procName);
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
    /*
    class CmdController
    {
        public static void DoCommandCache(NetProtocol cmdProtocol,string cmd, string key, string value)
        {
            bool ok = false;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                switch (cmd.ToLower())
                {
                    case "getvalue":
                        var json = CacheApi.Get(cmdProtocol).GetJson(key, JsonFormat.Indented);
                        Display(cmd,json);
                        break;
                    case "additem":
                        var res = CacheApi.Get(cmdProtocol).AddItem(key, value, 0);
                        Display(cmd, res.ToString());
                        break;
                    case "removeitem":
                        CacheApi.Get(cmdProtocol).RemoveItem(key);
                        Display(cmd, "Cache item will remove.");
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
        public static void DoCommandCacheSync(NetProtocol cmdProtocol,string cmd, string name, string keys)
        {
            Stopwatch watch = Stopwatch.StartNew();
            bool ok = false;
            try
            {
                switch (cmd.ToLower())
                {
                    case "getjson":
                        {
                            var json = SyncCacheApi.Get(cmdProtocol).GetJson(name, keys.Split(';'), JsonFormat.Indented);
                            Display(cmd, json);
                        }
                        break;
                    case "getrecord":
                        {
                            //var record = CacheApi.Sync.GetEntity<ContactEntity>(name, keys.Split(';'));

                            var record = SyncCacheApi.Get(cmdProtocol).GetRecord(name, keys.Split(';'));
                            var json = JsonSerializer.ToJson(record, null, JsonFormat.Indented);
                            Display(cmd, json);
                        }
                        break;
                    case "getallentitynames":
                        var names = SyncCacheApi.Get(cmdProtocol).GetAllEntityNames();
                        DisplayArray(cmd,names);
                        break;
                    case "getentitykeys":
                        var ks = SyncCacheApi.Get(cmdProtocol).GetEntityKeys(name);
                        DisplayArray(cmd, ks);
                        break;
                    case "refreshitem":
                        SyncCacheApi.Get(cmdProtocol).Refresh(name);
                        Display(cmd, "Refresh sync cache item started");
                        break;
                    case "refresh":
                        SyncCacheApi.Get(cmdProtocol).Refresh();
                        Display(cmd, "Refresh all sync cache items started");
                        break;
                    case "reset":
                        SyncCacheApi.Get(cmdProtocol).Reset();
                        Display(cmd, "Sync cache restart");
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
        public static void DoCommandSession(NetProtocol cmdProtocol,string cmd, string sessionId, string key)
        {

            bool ok = false;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                switch (cmd.ToLower())
                {
                    case "getsessionitem":
                        var json = SessionCacheApi.Get(cmdProtocol).GetJson(sessionId, key, JsonFormat.Indented);
                        Display(cmd, json);
                        break;
                    case "getallsessionskeys":
                        var ks = SessionCacheApi.Get(cmdProtocol).GetAllSessionsKeys();
                        DisplayArray(cmd, ks);
                        break;
                    case "removesession":
                        SessionCacheApi.Get(cmdProtocol).RemoveSession(sessionId);
                        Display(cmd, "session {0} will remove", sessionId);
                        break;
                    case "removeitem":
                        SessionCacheApi.Get(cmdProtocol).Remove(sessionId, key);
                        Display(cmd, "session item {0},{1} will remove", sessionId, key);
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

        static void Display(string cmd, string val)
        {
            Console.WriteLine("command - {0} :", cmd);
            Console.WriteLine(val);
        }
        static void Display(string cmd, string val, params string[] args)
        {
            Console.WriteLine("command - {0} :", cmd);
            Console.WriteLine(val, args);
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
    }
    */
    #endregion
}
