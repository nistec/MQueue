using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Nistec.Queue.Service
{
	[RunInstaller(true)]
	public class Installer : System.Configuration.Install.Installer
	{

        /// Required designer variable.
        private System.ComponentModel.Container components = null;

		private ServiceInstaller serviceInstaller1;
		private ServiceProcessInstaller processInstaller;
        //private ServiceConfig serviceConfig;

		public Installer()
		{
            // This call is required by the Designer.
            InitializeComponent();

            //serviceConfig = new ServiceConfig();

			// Instantiate installers for process and services.
			processInstaller = new ServiceProcessInstaller();
			serviceInstaller1 = new ServiceInstaller();

			// The services run under the system account.
			processInstaller.Account = ServiceAccount.LocalSystem;

			// The services are started manually.
			serviceInstaller1.StartType = ServiceStartMode.Automatic;

			// ServiceName must equal those on ServiceBase derived classes.            

            serviceInstaller1.ServiceName = "Nistec.Queue";
            serviceInstaller1.DisplayName = "Nistec.Queue";
            serviceInstaller1.Description = "Nistec RemoteQueue Service";

			// Add installers to collection. Order is not important.
			Installers.Add(serviceInstaller1);
			Installers.Add(processInstaller);
		}

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

        /*
        
		public override void Install(IDictionary stateServer)
		{
			Microsoft.Win32.RegistryKey system,
				currentControlSet,
				services,
				service,
				config; //config is where I'll be putting service-specific configuration
			

			try
			{
				//Let the project installer do its job
				base.Install(stateServer);

				//Open the HKEY_LOCAL_MACHINE\SYSTEM key
				system = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("System");
				//Open CurrentControlSet
				currentControlSet = system.OpenSubKey("CurrentControlSet");
				//Go to the services key
				services = currentControlSet.OpenSubKey("Services");
				//Open the key for your service, and allow writing
				service = services.OpenSubKey(this.serviceInstaller1.ServiceName, true);
				//Add your service's description as a REG_SZ value named "Description"
                service.SetValue("Description", this.serviceInstaller1.Description);
				//(Optional) Add some other information your service will be looking
				
				config = service.CreateSubKey("Parameters");
			}
			catch(Exception e)
			{
				Console.WriteLine("An exception was thrown during service installation:\n" + e.ToString());
			}
		}

      
		public override void Uninstall(IDictionary stateServer)
		{
			Microsoft.Win32.RegistryKey system,
				currentControlSet,
				services,
				service;

			try
			{
				//Drill down to the service key and open it with write permission
				system = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("System");
				currentControlSet = system.OpenSubKey("CurrentControlSet");
				services = currentControlSet.OpenSubKey("Services");
				service = services.OpenSubKey(this.serviceInstaller1.ServiceName, true);
				//Delete any keys you created during installation (or that your service	created)
				service.DeleteSubKeyTree("Parameters");
				//...
			}
			catch(Exception e)
			{
				Console.WriteLine("Exception encountered while uninstalling service:\n"	+ e.ToString());
			}
			finally
			{
				//Let the project installer do its job
				base.Uninstall(stateServer);
			}
		}

        */
	}
}
