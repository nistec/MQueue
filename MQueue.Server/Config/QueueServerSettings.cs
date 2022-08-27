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
using Nistec.Channels.Http;

namespace Nistec.Messaging.Config
{

    public class QueueServerSettings
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
               PipeName = settings.VerifyPipe,//settings.PipeName,
               PipeDirection = EnumExtension.Parse<PipeDirection>(settings.PipeDirection, PipeDirection.InOut),
               PipeOptions = EnumExtension.Parse<PipeOptions>(settings.PipeOptions, PipeOptions.None),
               VerifyPipe = settings.VerifyPipe,
               ConnectTimeout = settings.ConnectTimeout,
                ReceiveBufferSize = settings.ReceiveBufferSize,
                SendBufferSize = settings.SendBufferSize,
               MaxServerConnections = settings.MaxServerConnections,
               MaxAllowedServerInstances = settings.MaxAllowedServerInstances,
               IsAsync=settings.IsAsync
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
            //return null;
            var config = QueueServerConfig.GetConfig();

            var settings = config.FindTcpServer(configHost);
            if (settings == null)
            {
                throw new ArgumentException("Invalid TcpQueueSettings with TcpName:" + configHost);
            }

            return new TcpSettings()
            {
                HostName = settings.HostName,
                Address = TcpSettings.EnsureHostAddress(settings.Address),
                Port = settings.Port,
                IsAsync = settings.IsAsync,
                ReceiveBufferSize = settings.ReceiveBufferSize,
                SendBufferSize = settings.SendBufferSize,
                ConnectTimeout = settings.ConnectTimeout,
                //ReceiveTimeout = settings.ReceiveTimeout,
                ReadTimeout = settings.ReadTimeout,
                //ProcessTimeout = settings.ProcessTimeout,
                MaxSocketError = settings.MaxSocketError,
                MaxServerConnections = Math.Max(1, settings.MaxServerConnections)
            };

        }

        /// <summary>
        /// LoadHttpConfigServer
        /// </summary>
        /// <param name="configHost"></param>
        /// <returns></returns>
        public static HttpSettings LoadHttpConfigServer(string configHost)
        {
            if (string.IsNullOrEmpty(configHost))
            {
                throw new ArgumentNullException("HttpQueueSettings.LoadTcpConfigServer name");
            }
            //TODO
            //return null;
            var config = QueueServerConfig.GetConfig();

            var settings = config.FindHttpServer(configHost);
            if (settings == null)
            {
                throw new ArgumentException("Invalid HttpQueueSettings with HostName:" + configHost);
            }

            return new HttpSettings()
            {
                HostName = settings.HostName,
                Address = HttpSettings.EnsureHostAddress(settings.Address),
                Method=settings.Method,

                Port = settings.Port,
                //IsAsync = settings.IsAsync,
                //ReceiveBufferSize = settings.ReceiveBufferSize,
                //SendBufferSize = settings.SendBufferSize,
                ConnectTimeout = settings.ConnectTimeout,
                ReadTimeout = settings.ReadTimeout,
                //ProcessTimeout = settings.ProcessTimeout,
                MaxErrors = settings.MaxErrors,
                MaxThreads = Math.Max(1, settings.MaxServerConnections),
                MaxServerConnections = Math.Max(1, settings.MaxServerConnections)
            };

        }
    }

}
