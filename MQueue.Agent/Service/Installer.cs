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
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Nistec.Services
{
	[RunInstaller(true)]
	public class Installer : System.Configuration.Install.Installer
	{
		private ServiceInstaller serviceInstaller1;
		private ServiceProcessInstaller processInstaller;

		public Installer()
		{
			// Instantiate installers for process and services.
			processInstaller = new ServiceProcessInstaller();
			serviceInstaller1 = new ServiceInstaller();

			// The services run under the system account.
            processInstaller.Account = Settings.ServiceAccount;

			// The services are started manually.
            serviceInstaller1.StartType = Settings.ServiceStartMode;

			// ServiceName must equal those on ServiceBase derived classes.            

            serviceInstaller1.ServiceName = Settings.ServiceName;
            serviceInstaller1.DisplayName = Settings.DisplayName;
            serviceInstaller1.Description = Settings.ServiceDescription;

            string[] ServicesDependedOn = Settings.ServicesDependedOn;

            if (ServicesDependedOn != null && ServicesDependedOn.Length > 0)
            {
                serviceInstaller1.ServicesDependedOn = ServicesDependedOn;
            }

			// Add installers to collection. Order is not important.
			Installers.Add(serviceInstaller1);
			Installers.Add(processInstaller);
		}

    }
}
