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
//#define SERVICE

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Nistec.Generic;
using Nistec.Logging;

namespace Nistec.Services
{
    public class Program
    {

#if (SERVICE)

        static void Main(string[] args)
        {

            try
            {

                if (args.Length > 0)
                {
                    //Install service
                    if (args[0].Trim().ToLower() == "/i")
                    { 
                        System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/i", Assembly.GetExecutingAssembly().Location }); 
                    }

                    //Uninstall service                 
                    else if (args[0].Trim().ToLower() == "/u")
                    { 
                        System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location }); 
                    }
                    //run as console application
                    else if (args[0].Trim().ToLower() == "/c")
                    {
                        ServiceManager SVC = new ServiceManager();
                        SVC.Start();
                        Console.WriteLine("Server Started as console");
                        Console.ReadKey();
                    }
                }
                else
                {

                    Console.WriteLine("Server Started...");

                    System.ServiceProcess.ServiceBase[] ServicesToRun;
                    ServicesToRun = new System.ServiceProcess.ServiceBase[] { new Service1() };
                    System.ServiceProcess.ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception ex)
            {
                Netlog.Exception(Settings.ServiceName, ex, true, true);
            }
        }

#else
        static void Main(string[] args)
        {
            try
            {
                ServiceManager SVC = new ServiceManager();
                SVC.Start();

                //ManagerConfig config = Config.CreateManagerConfig();
                //manager.Start(McLock.Lock.ValidateLock(),true);

                Console.WriteLine("Server Started...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {

                Console.Write("*****************ERROR**************");
                Console.Write(ex.Message);
                Console.Write(ex.StackTrace);
                Console.ReadLine();


            }

        }

#endif

    }
}
