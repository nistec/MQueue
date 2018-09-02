using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Configuration;
using Nistec.Messaging.Server;
using Nistec.Services;

namespace Nistec.Queue.Service
{
	public class Service1 : System.ServiceProcess.ServiceBase
	{
        ServiceManager Svc;
        
        protected override void OnStart(string[] args)
		{
            Svc = new ServiceManager();
 			Svc.Start(); 
		}
 
        
		protected override void OnStop()
		{
            Svc.Stop();
            //RemoteQueueManager.WriteXmlBackup();
		}
	}

}
