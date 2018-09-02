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
using Nistec.Generic;
using Nistec.Logging;
using Nistec.Runtime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nistec.Messaging.Config
{
    public class ConfigFileWatcher
    {

        SysFileWatcher _configFileWatcher;
        bool initilaized = false;
        void Init()
        {
            if (initilaized)
                return;
            string filnenmae = Path.Combine(Environment.CurrentDirectory, "Nistec.Queue.Agent.exe.config");

            _configFileWatcher = new SysFileWatcher(filnenmae, true);
            _configFileWatcher.FileChanged += new FileSystemEventHandler(_ConfigFileWatcher_FileChanged);
            initilaized = true;
        }
        void _ConfigFileWatcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            ConfigurationManager.RefreshSection("appSettings");
            ConfigurationManager.RefreshSection("connectionStrings");
            Netlog.Info("ConfigFileWatcher FileChanged");
        }

        bool _IsListen;
        void Listen()
        {
            while(_IsListen)
            {
                Thread.Sleep(120000);
            }
        }

        public void Start(bool useListener)
        {
            if (_IsListen)
                return;
            if (!initilaized)
                Init();
            if (useListener)
            {
                _IsListen = true;
                Thread th = new Thread(new ThreadStart(Listen));
                th.IsBackground = true;
                th.Start();
            }
            Netlog.Debug("ConfigFileWatcher started...");
        }

        public void Stop()
        {
            _IsListen = false;
            if (initilaized)
                _configFileWatcher.FileChanged -= new FileSystemEventHandler(_ConfigFileWatcher_FileChanged);
            initilaized = false;
            Netlog.Debug("ConfigFileWatcher stoped...");

        }

    }
}
