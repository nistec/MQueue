using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nistec.Win;
using Nistec.Messaging.Remote;
using Nistec.Messaging.Server;

namespace Nistec.Messaging.UI
{
    public partial class PropertyForm : Nistec.GridView.VGridDlg
    {
        string queueName;
        public PropertyForm()
        {
            InitializeComponent();
        }
        public PropertyForm(string queueName)
        {
            this.queueName = queueName;
            //SelectObject(o, name);
        }

       

        protected override void OnPropertyItemChanged(PropertyItemChangedEventArgs e)
        {
            base.OnPropertyItemChanged(e);
            if (string.IsNullOrEmpty(queueName))
                return;
            var Client = AgentManager.Queue.Get(queueName);
            //RemoteQueue Client = new RemoteQueue(queueName);
            Client.SetProperty(e.PropertyName, e.PropertyValue);

        }

    }
}