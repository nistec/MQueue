using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Config
{
    public class ActiveConfig
    {

        #region Load xml config
        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        public void LoadSyncConfig()
        {

            string file = QueueSettings.SyncConfigFile;
            LoadSyncConfigFile(file, 3);
        }

        internal void LoadSyncConfigFile(string file, int retrys)
        {

            int counter = 0;
            bool reloaded = false;
            while (!reloaded && counter < retrys)
            {
                reloaded = LoadSyncConfigFile(file);
                counter++;
                if (!reloaded)
                {
                    CacheLogger.Logger.LogAction(CacheAction.LoadItem, CacheActionState.Failed, "LoadSyncConfigFile retry: " + counter);
                }
            }
            if (reloaded)
            {
                OnSyncReload(new GenericEventArgs<string>(file));
            }
        }

        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        /// <param name="file"></param>
        public bool LoadSyncConfigFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                return true;
            Thread.Sleep(1000);
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);

                LoadSyncConfig(doc);
                return true;
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.LoadItem, CacheActionState.Error, "LoadSyncConfigFile error: " + ex.Message);
                OnError("LoadSyncConfigFile error " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Load sync cache from xml string argument.
        /// </summary>
        /// <param name="xml"></param>
        public bool LoadSyncConfig(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return true;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                LoadSyncConfig(doc);
                return true;
            }
            catch (Exception ex)
            {
                CacheLogger.Logger.LogAction(CacheAction.LoadItem, CacheActionState.Error, "LoadSyncConfig error: " + ex.Message);
                OnError("LoadSyncConfig error " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Load sync cache from <see cref="XmlDocument"/> document.
        /// </summary>
        /// <param name="doc"></param>
        public void LoadSyncConfig(XmlDocument doc)
        {
            if (doc == null)
                return;

            XmlNode items = doc.SelectSingleNode("//SyncCache");
            if (items == null)
                return;
            LoadSyncItems(items, CacheSettings.EnableAsyncTask);
        }

        internal abstract void LoadSyncItems(XmlNode node, bool EnableAsyncTask);

        #endregion load xml config
    }
}
