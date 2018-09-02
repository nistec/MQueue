using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Channels.Tcp;
using Nistec.Channels;
using System.IO.Pipes;

namespace Nistec.Messaging.Config
{
    /// <summary>
    /// Represent a tcp client settings.
    /// </summary>
    public class TcpClientQueueSettings
    {
        static readonly Dictionary<string, TcpSettings> ClientSettingsQueue = new Dictionary<string, TcpSettings>();

        /// <summary>
        /// Get Tcp Client Settings
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static TcpSettings GetTcpClientSettings(string hostName)
        {
            TcpSettings settings = null;
            if (ClientSettingsQueue.TryGetValue(hostName, out settings))
            {
                return settings;
            }
            settings = LoadTcpConfigClient(hostName);
            if (settings == null)
            {
                throw new Exception("Invalid configuration for tcp cache client settings with host name:" + hostName);
            }
            ClientSettingsQueue[hostName] = settings;
            return settings;
        }
        /// <summary>
        /// LoadTcpConfigClient
        /// </summary>
        /// <param name="configHost"></param>
        /// <returns></returns>
        public static TcpSettings LoadTcpConfigClient(string configHost)
        {
            if (string.IsNullOrEmpty(configHost))
            {
                throw new ArgumentNullException("TcpQueueSettings.LoadTcpConfigClient name");
            }

            var config = QueueConfigClient.GetConfig();
            //TODO
            return null;
            //var settings = config.FindTcpClient(configHost);
            //if (settings == null)
            //{
            //    throw new ArgumentException("Invalid TcpQueueSettings with TcpName:" + configHost);
            //}

            //return new TcpSettings()
            //{
            //    HostName = settings.HostName,
            //    Address = TcpSettings.EnsureHostAddress(settings.Address),
            //    Port = settings.Port,
            //    IsAsync = settings.IsAsync,
            //    ReceiveBufferSize = settings.ReceiveBufferSize,
            //    SendBufferSize = settings.SendBufferSize,
            //    SendTimeout = settings.SendTimeout,
            //    ReadTimeout = settings.ReadTimeout,
            //};
        }
    }
    /// <summary>
    /// Represent a pipe client settings.
    /// </summary>
    public class PipeClientQueueSettings
    {
        static readonly Dictionary<string, PipeSettings> ClientSettingsQueue = new Dictionary<string, PipeSettings>();
        /// <summary>
        /// Get pipe client settings
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static PipeSettings GetPipeClientSettings(string hostName)
        {
            PipeSettings settings = null;
            if (ClientSettingsQueue.TryGetValue(hostName, out settings))
            {
                return settings;
            }
            settings = LoadPipeConfigClient(hostName);
            if (settings == null)
            {
                throw new Exception("Invalid configuration for pipe cache client settings with host name:" + hostName);
            }
            ClientSettingsQueue[hostName] = settings;
            return settings;
        }

        /// <summary>
        /// LoadPipeConfigClient
        /// </summary>
        /// <param name="configPipe"></param>
        /// <returns></returns>
        public static PipeSettings LoadPipeConfigClient(string configPipe)
        {
            if (string.IsNullOrEmpty(configPipe))
            {
                throw new ArgumentNullException("PipeQueueSettings.LoadPipeConfigClient name");
            }

            var config = QueueConfigClient.GetConfig();

            var settings = config.FindPipeClient(configPipe);
            if (settings == null)
            {
                throw new ArgumentException("Invalid PipeQueueSettings with PipeName:" + configPipe);
            }
            return new PipeSettings()
            {
                PipeName = settings.PipeName,
                PipeDirection = EnumExtension.Parse<PipeDirection>(settings.PipeDirection, PipeDirection.InOut),
                PipeOptions = EnumExtension.Parse<PipeOptions>(settings.PipeOptions, PipeOptions.None),
                VerifyPipe = settings.VerifyPipe,
                ConnectTimeout = (uint)settings.ConnectTimeout,
                InBufferSize = settings.InBufferSize
            };

        }

    }


    /*
    public class ServerQueueSettings
    {
        /// <summary>
        /// LoadPipeConfigServer
        /// </summary>
        /// <param name="configPipe"></param>
        /// <returns></returns>
        public static PipeSettings LoadPipeConfigServer(string configPipe)
        {
            if (string.IsNullOrEmpty(configPipe))
            {
                throw new ArgumentNullException("PipeQueueSettings.LoadPipeConfigServer name");
            }

            var config = QueueServerConfig.GetConfig();

            var settings = config.FindPipeServer(configPipe);
            if (settings == null)
            {
                throw new ArgumentException("Invalid PipeQueueSettings with PipeName:" + configPipe);
            }
            return new PipeSettings()
           {
               PipeName = settings.PipeName,
               PipeDirection = EnumExtension.Parse<PipeDirection>(settings.PipeDirection, PipeDirection.InOut),
               PipeOptions = EnumExtension.Parse<PipeOptions>(settings.PipeOptions, PipeOptions.None),
               VerifyPipe = settings.VerifyPipe,
               ConnectTimeout = (uint)settings.ConnectTimeout,
               InBufferSize = settings.InBufferSize,
               OutBufferSize = settings.OutBufferSize,
               MaxServerConnections = settings.MaxServerConnections,
               MaxAllowedServerInstances = settings.MaxAllowedServerInstances
           };
        }


        /// <summary>
        /// LoadTcpConfigServer
        /// </summary>
        /// <param name="configHost"></param>
        /// <returns></returns>
        public static TcpSettings LoadTcpConfigServer(string configHost)
        {
            if (string.IsNullOrEmpty(configHost))
            {
                throw new ArgumentNullException("TcpQueueSettings.LoadTcpConfigServer name");
            }
            //TODO
            return null;
            //var config = QueueServerConfig.GetConfig();

            //var settings = config.FindTcpServer(configHost);
            //if (settings == null)
            //{
            //    throw new ArgumentException("Invalid TcpQueueSettings with TcpName:" + configHost);
            //}

            //return new TcpSettings()
            //{
            //    HostName = settings.HostName,
            //    Address = TcpSettings.EnsureHostAddress(settings.Address),
            //    Port = settings.Port,
            //    IsAsync = settings.IsAsync,
            //    ReceiveBufferSize = settings.ReceiveBufferSize,
            //    SendBufferSize = settings.SendBufferSize,
            //    SendTimeout = settings.SendTimeout,
            //    ProcessTimeout = settings.ProcessTimeout,
            //    ReadTimeout = settings.ReadTimeout,
            //    MaxSocketError = settings.MaxSocketError,
            //    MaxServerConnections = Math.Max(1, settings.MaxServerConnections)
            //};

        }
    }
    */
}
