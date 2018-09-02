using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Xml;
using MControl.Messaging;

namespace MControl.Queue.Service
{
    public sealed class ServiceConfig
    {

        //Remote cache
        public static string RemoteQueueName;
        public static long MaxSize;
        public static string SyncOption;
        public static int TcpPort = 9009;
        public static bool SecureChannel = true;

        //Data cache
        public static string DataQueueName;
        public static string XmlConfigFile;
        public static bool LoadOnStart=false;
        private static bool QueueLoaded = false;
 
        //mailer
        public static bool EnableMailer = false;

        static ServiceConfig()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            SecureChannel = Convert.ToBoolean(appSettings["SecureChannel"]);
            TcpPort = Convert.ToInt32(appSettings["TcpPort"]);
            MaxSize = Convert.ToInt32(appSettings["MaxSize"]);
            RemoteQueueName = appSettings["RemoteName"];
            SyncOption = appSettings["SyncOption"];
            XmlConfigFile = appSettings["XmlDataConfig"];
            LoadOnStart = Types.ToBool(appSettings["LoadOnStart"], false);

            EnableMailer = Types.ToBool(appSettings["EnableMailer"], false);

            if (LoadOnStart)
            {
                LoadQueues();
            }
        }
        internal static void LoadConfig()
        {

        }

        internal static void LoadQueues()
        {

            if (!QueueLoaded)
            {
             System.Configuration.Configuration config =
         ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                XmlDocument doc = new XmlDocument();
                doc.Load(config.FilePath);

                Console.WriteLine("Load Config: " + config.FilePath);

                XmlNode root = doc.SelectSingleNode("//remoteSettings");
                XmlNodeList list = root.ChildNodes;

                for(int i=0;i<list.Count;i++)
               {
                    //n.FirstChild.ChildNodes[1].InnerText
                    McQueueProperties prop =
                        new McQueueProperties(list[i]);
                    //prop.ConnectionString = ConnectionString;
                    //prop.Provider = Provider;
                    Console.WriteLine("Load: " + prop.QueueName);

                    RemoteQueueManager.AddQueue(prop);
                }
                QueueLoaded = true;
            }
        }
    }
}
