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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;

namespace Nistec.Services
{
    /// <summary>
    /// Settings
    /// </summary>
    public class Settings
    {

        public const string ServiceName = "Nistec.Queue.Agent";
        public const string DisplayName = "Nistec.Queue";
        public const string ServiceDescription = "Nistec Remote Queue Agent";
        public static string[] ServicesDependedOn { get { return null /*new string[] { "Nistec.Cache", "Nistec.Queue" }*/; } }
        public static ServiceAccount ServiceAccount { get { return ServiceAccount.LocalSystem; } }
        public static ServiceStartMode ServiceStartMode { get { return ServiceStartMode.Automatic; } }


        public const string ServiceProcess = "Nistec.Queue.Agent.exe";
        public const string WindowsAppProcess = "Nistec.Queue.Server.exe";
        public const string TrayAppProcess = "";
        public const string ManagerAppProcess = "";

        /// <summary>
        /// Gets if server service is installed.
        /// </summary>
        /// <returns></returns>
        public static bool IsServiceInstalled()
        {
            foreach (ServiceController service in ServiceController.GetServices())
            {
                if (service.ServiceName == Settings.ServiceName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets server service installed.
        /// </summary>
        /// <param name="createIfNotInstalled"></param>
        /// <returns></returns>
        public static AgentController GetServiceInstalled()//bool createIfNotInstalled)
        {
            foreach (ServiceController service in ServiceController.GetServices())
            {
                if (service.ServiceName == Settings.ServiceName)
                {
                    return new AgentController(service);
                }
            }

            return new AgentController(null);
        }
    }

    public class AgentController
    {
        public AgentController(ServiceController sc)
        {
            ServiceController = sc;

            ServiceName = Settings.ServiceName;
            DisplayName = Settings.DisplayName;
            Installed = (sc != null);
        }

        public ServiceController ServiceController { get; set; }
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public bool Installed { get; set; }
    }

}
