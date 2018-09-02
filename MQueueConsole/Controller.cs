using Nistec.Channels.RemoteCache;
using Nistec.Serialization;
using Nistec.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nistec.Channels;
using Nistec.Caching.Remote;
using Nistec.Caching;
using System.Data;
using API = Nistec.Caching.Remote;
using Nistec.Logging;
using Nistec.Caching.Config;

namespace Nistec
{
    internal class CacheType
    {
        public const string cache = "remote-cache";
        public const string sync = "remote-sync";
        public const string session = "remote-session";
        public const string data = "remote-data";
    }

    class Controller
    {
        internal static bool EnableLog = false;
        internal static bool EnableJsonController = false;
        public static void Run(string[] args)
        {
            NetProtocol cmdProtocol = NetProtocol.Pipe;
            string protocol = "pipe";
            string cmd = "";
            string cacheType = CacheType.cache;
            string transform = "binary";
            string cmdName = "";
            string cmdArg1 = "";
            string cmdArg2 = "";
            string cmdArg3 = "";
            string cmdArg4 = "";

            DisplayMenu("menu", "", "");
            DisplayCacheTypeMenu();
            cacheType = GetCacheType(Console.ReadLine().ToLower(), cacheType);

            if (cacheType == "quit")
            {
                return;
            }
            Console.WriteLine("Current cache type : {0}.", cacheType);
            SetCommands();

            while (cmd != "quit")
            {
                Console.WriteLine("Enter command :");

                cmd = Console.ReadLine();

                try
                {

                    string[] cmdargs = SplitCmd(cmd);
                    cmdName = GetCommandType(cmdargs, cmdName, 0);
                    cmdArg1 = GetCommandType(cmdargs, cmdArg1, 1);
                    cmdArg2 = GetCommandType(cmdargs, cmdArg2, 2);
                    cmdArg3 = GetCommandType(cmdargs, cmdArg3, 3);
                    cmdArg4 = GetCommandType(cmdargs, cmdArg4, 4);

                    switch (cmdName.ToLower())
                    {
                        case "menu":
                            DisplayMenu("menu", "", "");
                            break;
                        case "menu-items":
                            DisplayMenu("menu-items", cacheType, "");
                            break;
                        case "cache-type":
                            DisplayCacheTypeMenu();
                            cacheType = GetCacheType(Console.ReadLine().ToLower(), cacheType);
                            Console.WriteLine("Current cache type : {0}.", cacheType);
                            break;
                        case "transform":
                            DisplayTransformTypeMenu();
                            transform= GetTransformType(Console.ReadLine().ToLower(), transform);
                            Console.WriteLine("Current transform type : {0}.", transform);
                            break;
                        case "protocol":
                            Console.WriteLine("Choose protocol : tcp , pipe, http");
                            protocol = EnsureProtocol(Console.ReadLine().ToLower(), protocol);
                            cmdProtocol = GetProtocol(protocol, cmdProtocol);
                            Console.WriteLine("Current protocol : {0}.", protocol);
                            break;
                        case "args":
                            DisplayMenu("args", cacheType, cmdArg1);
                            break;
                        case "report":
                            //CmdController.DoCommandManager(cmdArg1, cacheType);
                            break;
                        case "quit":

                            break;
                        default:
                            if (EnableJsonController)
                            {
                                switch (cacheType)
                                {
                                    case CacheType.cache:
                                        CmdController.DoCommandCacheJson(cmdProtocol, cmdName, cmdArg1, cmdArg2);
                                        break;
                                    case CacheType.sync:
                                        CmdController.DoCommandSyncJson(cmdProtocol, cmdName, cmdArg1, cmdArg2, cmdArg3);
                                        break;
                                    case CacheType.session:
                                        CmdController.DoCommandSessionJson(cmdProtocol, cmdName, cmdArg1, cmdArg2, cmdArg3);
                                        break;
                                    default:
                                        Console.WriteLine("Unknown command!");
                                        break;
                                }
                            }
                            else
                            {
                                switch (cacheType)
                                {
                                    case CacheType.cache:
                                        CmdController.DoCommandCache(cmdProtocol, transform,cmdName, cmdArg1, cmdArg2);
                                        break;
                                    case CacheType.sync:
                                        CmdController.DoCommandSync(cmdProtocol, transform, cmdName, cmdArg1, cmdArg2, cmdArg3);
                                        break;
                                    case CacheType.session:
                                        CmdController.DoCommandSession(cmdProtocol, transform, cmdName, cmdArg1, cmdArg2, cmdArg3);
                                        break;
                                    case CacheType.data:
                                        CmdController.DoCommandData(cmdProtocol, transform, cmdName, cmdArg1, cmdArg2, cmdArg3, cmdArg4);
                                        break;
                                    default:
                                        Console.WriteLine("Unknown command!");
                                        break;
                                }
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

        static Dictionary<string, string> cmdCache = new Dictionary<string, string>();
        static Dictionary<string, string> cmdSyncCache = new Dictionary<string, string>();
        static Dictionary<string, string> cmdSessionCache = new Dictionary<string, string>();
        static Dictionary<string, string> cmdDataCache = new Dictionary<string, string>();
        static Dictionary<string, string> cmdManager = new Dictionary<string, string>();
        static void SetCommands()
        {
            cmdCache.Add("Get", "key");
            cmdCache.Add("GetRecord", "key");
            cmdCache.Add("Add", "key, value, expiration");
            cmdCache.Add("Set", "key, value, expiration");
            cmdCache.Add("Remove", "key");
            cmdCache.Add("Reply", "text");

            cmdSyncCache.Add("GetRecord", "tableName, key");
            cmdSyncCache.Add("GetEntity", "tableName, key");
            cmdSyncCache.Add("Get", "tableName, key, field");
            cmdSyncCache.Add("GetAllEntityNames", "");
            cmdSyncCache.Add("GetEntityKeys", "tableName");
            cmdSyncCache.Add("GetEntityItemsCount", "tableName");
            cmdSyncCache.Add("PrintEntityValues", "tableName");
            cmdSyncCache.Add("Remove", "tableName");
            cmdSyncCache.Add("RefreshAll", "");
            cmdSyncCache.Add("Refresh", "tableName");
            cmdSyncCache.Add("Reset", "");
            cmdSyncCache.Add("Reply", "text");

            cmdSessionCache.Add("CreateSession", "sessionId, expiration");
            cmdSessionCache.Add("Add", "sessionId, key, value");
            cmdSessionCache.Add("Set", "sessionId, key, value");
            cmdSessionCache.Add("Get", "sessionId, key");
            cmdSessionCache.Add("Remove", "sessionId, key");
            cmdSessionCache.Add("GetSessionItems", "sessionId");
            cmdSessionCache.Add("GetAllSessionsKeys", "sessionId");
            cmdSessionCache.Add("RemoveSession", "sessionId");
            cmdSessionCache.Add("Reply", "text");

            
            cmdDataCache.Add("GetRecord", "db, tableName, key");
            cmdDataCache.Add("Get", "db, tableName, key, field");
            cmdDataCache.Add("GetAllEntityNames", "");
            cmdDataCache.Add("GetEntityKeys", "db, tableName");
            cmdDataCache.Add("GetEntityItemsCount", "db, tableName");
            cmdDataCache.Add("GetTable", "db, tableName");
            cmdDataCache.Add("RemoveTable", "db, tableName");
            cmdDataCache.Add("Refresh", "db, tableName");
            cmdDataCache.Add("Reset", "");
            cmdDataCache.Add("Reply", "text");

            cmdManager.Add("Statistic", "cache-type");
            cmdManager.Add("CounterState", "cache-type");
            cmdManager.Add("CounterReset", "");
            cmdManager.Add("PrintLog", "");
        }

        static string EnsureArg(string arg)
        {
            if (arg == null)
                return "";
            return arg.Replace("/", "").ToLower();
        }
        static void DisplayCacheTypeMenu()
        {
            Console.WriteLine("Choose cache type : remote-cache, remote-sync, remote-session, remote-data");
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
                case CacheType.cache:
                    kv = cmdCache.Where(p => p.Key.ToLower() == a).FirstOrDefault();
                    break;
                case CacheType.sync:
                    kv = cmdSyncCache.Where(p => p.Key.ToLower() == a).FirstOrDefault();
                    break;
                case CacheType.session:
                    kv = cmdSessionCache.Where(p => p.Key.ToLower() == a).FirstOrDefault();
                    break;
                case CacheType.data:
                    kv = cmdDataCache.Where(p => p.Key.ToLower() == a).FirstOrDefault();
                    break;
            }

            if (kv.Key != null)
                Console.WriteLine("commands: {0} Arguments: {1}.", kv.Key, kv.Value);
            else
                Console.WriteLine("Bad commands: {0} Arguments: {1}.", cmdType, arg);
        }

        static void DisplayMenu(string cmdType, string cacheType, string arg)
        {
            //string menu = "cache-type: remote-cache, remote-sync, remote-session";
            //Console.WriteLine(menu);

            switch (cmdType)
            {
                case "menu":
                    Console.WriteLine("Enter: cache-type, To change cache type");
                    Console.WriteLine("Enter: protocol, To change protocol (tcp, pipe, http)");
                    Console.WriteLine("Enter: transform, To change transform type (binary, json)");
                    Console.WriteLine("Enter: menu, To display menu");
                    Console.WriteLine("Enter: menu-items, To display menu items for current cache-type");
                    Console.WriteLine("Enter: args, and /command to display command argument");
                    Console.WriteLine("Enter: report, and /command to display cache report");

                    break;
                case "menu-items":
                    DisplayMenuItems(cmdType, cacheType);
                    break;
                case "args":
                    if (arg != null && arg.StartsWith("/"))
                    {
                        DisplayArgs(cacheType, arg);
                    }
                    break;
            }
            Console.WriteLine("");

        }

        static void DisplayMenuItems(string cmdType, string cacheType)
        {
            switch (cacheType)
            {
                case CacheType.cache:
                    Console.Write("{0} commands: ", cacheType);
                    foreach (var entry in cmdCache)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                case CacheType.sync:
                    Console.Write("{0} commands: ", cacheType);
                    foreach (var entry in cmdSyncCache)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                case CacheType.session:
                    Console.Write("{0} commands: ", cacheType);
                    foreach (var entry in cmdSessionCache)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                case CacheType.data:
                    Console.Write("{0} commands: ", cacheType);
                    foreach (var entry in cmdDataCache)
                    {
                        Console.Write("{0} ,", entry.Key);
                    }
                    break;
                default:
                    Console.Write("Bad commands: Invalid cache-type");
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

        static string GetCacheType(string cmd, string curItem)
        {
            switch (cmd.ToLower())
            {
                case CacheType.cache:
                case CacheType.sync:
                case CacheType.session:
                case CacheType.data:
                    return cmd.ToLower();
                default:
                    Console.WriteLine("Invalid cache-type {0}", cmd);
                    return curItem;
            }
        }
        static string GetTransformType(string cmd, string curItem)
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
        //static string GetCommandType(string cmd, string curItem)
        //{
        //    if (cmd == "..")
        //        return curItem;
        //    return cmd;
        //}
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

            System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("NistecCache");
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
        public static void DoCommandCache(NetProtocol cmdProtocol, string transform, string cmd, string key, string value)
        {
            var api = API.CacheApi.Get(cmdProtocol);
            bool ok = true;
            string json = null;
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
                    switch (cmd.ToLower())
                    {
                        case "custom":
                            {
                                var args = KeySet.SplitTrim(key);
                                var res = api.DoCustom(cmd, KeySet.ReadArgs(args, 0), KeySet.ReadArgs(args, 1), value, 0);
                                json = api.ToJson(res, true);
                                Display(cmd, json);
                            }
                            break;
                        case "get":
                            {
                                json = api.GetJson(key, JsonFormat.Indented);
                                Display(cmd, json);
                            }
                            break;
                        case "getrecord":
                            {
                                var record = api.GetRecord(key);
                                json = api.ToJson(record);
                                Display(cmd, json);
                            }
                            break;

                        case "add":
                            {
                                var res = api.Add(key, value, 0);
                                Display(cmd, res.ToString());
                            }
                            break;
                        case "set":
                            {
                                var res = api.Set(key, value, 0);
                                Display(cmd, res.ToString());
                            }
                            break;
                        case "remove":
                            api.Remove(key);
                            Display(cmd, "Cache item will remove.");
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
        public static void DoCommandSync(NetProtocol cmdProtocol, string transform, string cmd, string name, string keys, string field)
        {
            var api = API.SyncCacheApi.Get(cmdProtocol);
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
                            Display(cmd, "Refresh sync cache item started");
                            break;
                        case "refreshall":
                            api.RefreshAll();
                            Display(cmd, "Refresh all sync cache items started");
                            break;
                        case "reset":
                            api.Reset();
                            Display(cmd, "Sync cache restart");
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
            var api = API.SessionCacheApi.Get(cmdProtocol);
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
                            Display(cmd, RemoteCacheApi.ToJson(session, JsonFormat.Indented));
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
            var api = API.DataCacheApi.Get(cmdProtocol);
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

        public static void DoCommandCacheJson(NetProtocol cmdProtocol, string cmd, string key, string value)
        {
            //string RemoteHostName = CacheDefaults.DefaultBundleHostName;
            var api = API.CacheApi.Get(cmdProtocol);

            bool ok = true;
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                switch (cmd.ToLower())
                {
                    case "get":
                        {
                            var json = api.DoHttpJson(command: "Get", key: key, pretty: true);

                            //var json = api.SendJsonDuplex(API.CacheCmd.Get, key, value, true);
                            Display(cmd, json);
                        }
                        break;
                    case "add":
                        {
                            var json = api.DoHttpJson(command: "Add", key: key, value: value, pretty: true);
                            Display(cmd, json);
                            //var res = api.SendJsonDuplex(API.CacheCmd.Add, key, value, true);
                            //Display(cmd, res.ToString());
                        }
                        break;
                    case "set":
                        {
                            var json = api.DoHttpJson(command: "Set", key: key, value: value, pretty: true);
                            Display(cmd, json);
                            //var res = api.SendJsonDuplex(API.CacheCmd.Set,key, value, 0);
                            //Display(cmd, res.ToString());
                        }
                        break;
                    case "remove":
                        {
                            var json = api.DoHttpJson(command: "Remove", key: key, pretty: true);
                            Display(cmd, json);

                            //api.SendJsonDuplex(API.CacheCmd.Remove, key, value, true);
                            //Display(cmd, "Cache item will remove.");
                        }
                        break;
                    case "reply":
                        {
                            var json = api.DoHttpJson(command: "Reply", key: key, pretty: true);
                            Display(cmd, json);

                            //var r = api.SendJsonDuplex(API.CacheCmd.Reply, key, value, true);
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
            //string RemoteHostName = CacheDefaults.DefaultBundleHostName;
            var api=API.SyncCacheApi.Get(cmdProtocol);

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
                            //json = api.SendJsonDuplex(API.SyncCacheCmd.GetAs, key, true);
                            Display(cmd, json);
                        }
                        break;
                    case "getentity":
                        {
                            json = api.DoHttpJson(command: "GetEntity", entityName: name, primaryKey: keys, pretty: true);

                            //key = string.Format("{0}:{1}", name, string.Join("_", keys));
                            //json = api.SendJsonDuplex(API.SyncCacheCmd.GetEntity, key, true);
                            Display(cmd, json);
                        }
                        break;
                    case "get":
                        {
                            json = api.DoHttpJson(command: "Get", entityName: name, primaryKey: keys,field: field, pretty: true);

                            //key = string.Format("{0}:{1}", name, string.Join("_", keys));
                            //json = api.SendJsonDuplex(API.SyncCacheCmd.Get, key, true);
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
                            //json = api.SendJsonDuplex(API.SyncCacheCmd.GetRecord, key, true);
                            Display(cmd, json);
                        }
                        break;
                    case "remove":
                        {
                            json = api.DoHttpJson(command: "Remove", entityName: name, primaryKey: keys, pretty: true);

                            //json = api.SendJsonDuplex(API.SyncCacheCmd.Remove, name, true);
                            Display(cmd, json);
                        }
                        break;
                    case "getallentitynames":
                        {
                            json = api.DoHttpJson(command: "GetAllEntityNames", entityName: name, primaryKey: keys, pretty: true);

                            //json = api.SendJsonDuplex(API.SyncCacheCmd.GetAllEntityNames, name, true);
                            Display(cmd, json);
                        }
                        break;
                    case "getentitykeys":
                        {
                            json = api.DoHttpJson(command: "GetEntityKeys", entityName: name, primaryKey: keys, pretty: true);

                            //json = api.SendJsonDuplex(API.SyncCacheCmd.GetEntityKeys, name, true);
                            string[] arrky = JsonSerializer.Deserialize<string[]>(json);
                            DisplayArray(cmd, arrky);
                        }
                        break;
                    case "printentityvalues":
                        {

                            var ks = api.DoHttpJson(command: "GetEntityKeys", entityName: name, primaryKey: keys);


                            //var ks = api.SendJsonDuplex(API.SyncCacheCmd.GetEntityKeys, name);
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
                                    //json = api.SendJsonDuplex(API.SyncCacheCmd.GetRecord, recordkey, true);

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
                            //json = api.SendJsonDuplex(API.SyncCacheCmd.GetEntityItemsCount, name, true);
                            Display(cmd, json);
                        }
                        break;
                    case "refresh":
                        json = api.DoHttpJson(command: "Refresh", entityName: name, primaryKey: keys, pretty: true);
                        Display(cmd, json);
                        //api.SendJsonDuplex(API.SyncCacheCmd.Refresh, name);
                        //Display(cmd, "Refresh sync cache item started");
                        break;
                    case "refreshall":
                        json = api.DoHttpJson(command: "RefreshAll", entityName: name, primaryKey: keys, pretty: true);
                        Display(cmd, json);

                        //api.SendJsonDuplex(API.SyncCacheCmd.RefreshAll, name);
                        //Display(cmd, "Refresh all sync cache items started");
                        break;
                    case "reset":
                        json = api.DoHttpJson(command: "Reset", entityName: name, primaryKey: keys, pretty: true);
                        Display(cmd, json);
                        //api.SendJsonDuplex(API.SyncCacheCmd.Reset, name);
                        //Display(cmd, "Sync cache restart");
                        break;
                    case "reply":
                        json = api.DoHttpJson(command: "Reply", entityName: name, primaryKey: keys, pretty: true);
                        Display(cmd, json);

                        //json = api.SendJsonDuplex(API.SyncCacheCmd.Reply, name, true);
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
            //string RemoteHostName = CacheDefaults.DefaultBundleHostName;
            var api = API.SessionCacheApi.Get(cmdProtocol);
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
                        //json = api.SendJsonDuplex(new CacheMessage() { Command= API.SessionCmd.ViewAllSessionsKeys },true);// API.SessionCmd.ViewAllSessionsKeys, null, true);
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
            ICachePerformanceReport report = null;
            if (cmdType == "all" || cmdType == null)
            {
                report = ManagerApi.GetPerformanceReport();
            }
            else
            {
                var agentType = GetCacheAgentType(cmdType);
                report = ManagerApi.GetAgentPerformanceReport(agentType);
            }
                        
            if (report == null)
            {
                Console.WriteLine("Invalid cache performance property");
            }
            else
            {
                Console.WriteLine("Cache Performance Report");
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
                report = ManagerApi.GetStateCounterReport();
            }
            else
            {
                var agentType = GetCacheAgentType(cmdType);
                report = ManagerApi.GetStateCounterReport(agentType);
            }

            if (report == null)
            {
                Console.WriteLine("Invalid cache performance property");
            }
            else
            {
                Console.WriteLine("Cache State Report");
                var json = Nistec.Serialization.JsonSerializer.Serialize(report, null, JsonFormat.Indented);
                Console.WriteLine(json);
            }
            Console.WriteLine();
        }

        static void DoResetPerformanceCounter()
        {
            ManagerApi.ResetPerformanceCounter();
        }

        static void DoLog()
        {
            string log = ManagerApi.CacheLog();
            Console.WriteLine("Cache Log");
            Console.WriteLine(log);
            Console.WriteLine();
        }

        static CacheAgentType GetCacheAgentType(string cmdType)
        {
            switch (cmdType)
            {
                case CacheType.cache:
                    return CacheAgentType.Cache;
                case CacheType.sync:
                    return CacheAgentType.SyncCache;
                case CacheType.session:
                    return CacheAgentType.SessionCache;
                default:
                    return CacheAgentType.Cache;
            }
        }
    }

    //class JsonCmdController
    //{
    //    public static void DoCommandCacheJson(NetProtocol cmdProtocol, string cmd, string key, string value)
    //    {
    //        string RemoteHostName = CacheApiSettings.RemoteCacheHostName;
    //        //EnableRemoteException = CacheApiSettings.EnableRemoteException;

    //        bool ok = true;
    //        Stopwatch watch = Stopwatch.StartNew();
    //        try
    //        {
    //            switch (cmd.ToLower())
    //            {
    //                case "getvalue":
    //                    var json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.CacheCmd.GetValue, key, value);
    //                    Display(cmd, json);
    //                    break;
    //                case "additem":
    //                    var res = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName,0, API.CacheCmd.AddItem,key, value);
    //                    Display(cmd, res.ToString());
    //                    break;
    //                case "removeitem":
    //                    RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.CacheCmd.RemoveItem, key, value);
    //                    Display(cmd, "Cache item will remove.");
    //                    break;
    //                case "reply":
    //                    var r = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.CacheCmd.Reply, key, value);
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
    //    public static void DoCommandCacheSyncJson(NetProtocol cmdProtocol, string cmd, string name, string keys)
    //    {
    //        string RemoteHostName = CacheApiSettings.RemoteSyncCacheHostName;

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
    //                        json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.GetAs, key);
    //                        Display(cmd, json);
    //                    }
    //                    break;
    //                case "getrecord":
    //                    {
    //                        key = string.Format("{0}:{1}", name, string.Join("_", keys));
    //                        json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.GetRecord, key);
    //                        Display(cmd, json);
    //                    }
    //                    break;
    //                case "getallentitynames":
    //                    {
    //                        key = string.Format("{0}:{1}", name, string.Join("_", keys));
    //                        json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.GetAllEntityNames, key);
    //                        Display(cmd, json);
    //                    }
    //                    break;
    //                case "getentitykeys":
    //                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.GetEntityKeys, name);
    //                    string[] arrky = JsonSerializer.Deserialize<string[]>(json);
    //                    DisplayArray(cmd, arrky);
    //                    break;
    //                case "printentityvalues":
    //                    {
    //                        var ks = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.GetEntityKeys, name);
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
    //                                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.GetRecord, recordkey);
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
    //                        json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.GetEntityItemsCount, name);
    //                        Display(cmd, json);
    //                    }
    //                    break;
    //                case "refreshitem":
    //                    RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.RefreshItem, name);
    //                    Display(cmd, "Refresh sync cache item started");
    //                    break;
    //                case "refresh":
    //                    RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.Refresh, name);
    //                    Display(cmd, "Refresh all sync cache items started");
    //                    break;
    //                case "reset":
    //                    RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.Reset, name);
    //                    Display(cmd, "Sync cache restart");
    //                    break;
    //                case "reply":
    //                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SyncCacheCmd.Reply, name);
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
    //        string RemoteHostName = CacheApiSettings.RemoteSessionHostName;
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
    //                    var ks = API.SessionCacheApi.Get(cmdProtocol).GetAllSessionsKeys().ToArray();
    //                    DisplayArray(cmd, ks);
    //                    break;
    //                case "removesession":
    //                    json = RemoteApi.JsonRequest(cmdProtocol, RemoteHostName, 0, API.SessionCmd.RemoveSession, null, null, 0, sessionId);
    //                    API.SessionCacheApi.Get(cmdProtocol).RemoveSession(sessionId);
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
    //    //    ICachePerformanceReport report = null;
    //    //    if (cmdType == "all" || cmdType == null)
    //    //    {
    //    //        report = ManagerApi.GetPerformanceReport();
    //    //    }
    //    //    else
    //    //    {
    //    //        var agentType = GetCacheAgentType(cmdType);
    //    //        report = ManagerApi.GetAgentPerformanceReport(agentType);
    //    //    }

    //    //    if (report == null)
    //    //    {
    //    //        Console.WriteLine("Invalid cache performance property");
    //    //    }
    //    //    else
    //    //    {
    //    //        Console.WriteLine("Cache Performance Report");
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
    //    //        var agentType = GetCacheAgentType(cmdType);
    //    //        report = ManagerApi.GetStateCounterReport(agentType);
    //    //    }

    //    //    if (report == null)
    //    //    {
    //    //        Console.WriteLine("Invalid cache performance property");
    //    //    }
    //    //    else
    //    //    {
    //    //        Console.WriteLine("Cache State Report");
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
    //    //    string log = ManagerApi.CacheLog();
    //    //    Console.WriteLine("Cache Log");
    //    //    Console.WriteLine(log);
    //    //    Console.WriteLine();
    //    //}

    //    //static CacheAgentType GetCacheAgentType(string cmdType)
    //    //{
    //    //    switch (cmdType)
    //    //    {
    //    //        case CacheType.cache:
    //    //            return CacheAgentType.Cache;
    //    //        case CacheType.sync:
    //    //            return CacheAgentType.SyncCache;
    //    //        case CacheType.session:
    //    //            return CacheAgentType.SessionCache;
    //    //        default:
    //    //            return CacheAgentType.Cache;
    //    //    }
    //    //}
    //}
    #endregion
}
