using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading; 
using System.Collections;
using System.Collections.Generic;

namespace MControl.Queue.Service
{

    public class RemoteServer  
	{

        //private ServiceConfig serviceConfig;
        
        //static readonly RemoteQueueManager manager = new RemoteQueueManager();

        public RemoteServer()
		{
            //serviceConfig = new ServiceConfig();
        }
		
 
		/// <summary>
        /// Initilaize the queue remoting.
		/// </summary>
        public void Start()
        {

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteQueueServer),
                "RemoteQueueServer.rem", WellKnownObjectMode.Singleton);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteQueueManager),
                "RemoteQueueManager.rem", WellKnownObjectMode.Singleton);

            //RemotingConfiguration.RegisterWellKnownServiceType(typeof(ImplementationClass<int>), "IntRemoteObject.rem", WellKnownObjectMode.SingleCall);

            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add("authorizedGroup", "Everyone");
            props.Add("portName", "portMQueue");
            props.Add("exclusiveAddressUse", "false");
            //        prop["name"] = "RemoteServer";
            //        prop["tokenImpersonationLevel"] = System.Security.Principal.TokenImpersonationLevel.Delegation;
            //        prop["includeVersions"] = false;
            //        prop["strictBinding"] = false;
            //        prop["secure"] = true;

            IpcChannel channel = new IpcChannel(props, null, null);// ("test");
            ChannelServices.RegisterChannel(channel, false /*ensureSecurity*/);

            //// Show the URIs associated with the channel.
            System.Runtime.Remoting.Channels.ChannelDataStore channelData =
                (System.Runtime.Remoting.Channels.ChannelDataStore)
                channel.ChannelData;
            foreach (string uri in channelData.ChannelUris)
            {
                Console.WriteLine("The channel URI is {0}.", uri);
            }

          

            Console.WriteLine("Channel:{0} Waiting for connections...", channel.ChannelName);

        }

        public void Stop()
        {
          
        }

     
	}
}
