using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;
using MControl.Util;
using MControl.Data;
using MControl.Data.SqlClient;
using MControl.Charts;
using MControl.GridView;
using MControl.WinForms;
using MControl.Messaging;

namespace MControl.Messaging.UI
{
    public partial class QueuManagmentForm : McForm
    {

        private enum Actions
        {
            Services,
            Queues
        }

        private Actions curAction = Actions.Services;
        private bool shouldRefresh=true;
        private System.ServiceProcess.ServiceController[] services;

        const string channelManager = "";
        const string channelServer = "";

        TreeNode nodeRoot;
        //RemoteQueueClient client;
        //RemoteQueue remote;


        public QueuManagmentForm()
        {
            InitializeComponent();
            this.tbBack.Enabled = false;
            this.tbForward.Enabled = false;
            //CreateNodeList();
            //remote = new RemoteQueueClient("");// RemoteQueue.Instance;

        }

        #region servise


        private void CreateServicesNodeList()
        {
            if (!shouldRefresh && curAction == Actions.Services)
                return;

            RefreshServiceList();

           shouldRefresh=false;
           curAction = Actions.Services;
         
        }


        private void RefreshServiceList()
        {
            services = ServiceController.GetServices();
            mcManagment.TreeView.Nodes.Clear();
            mcManagment.ListCaption = "Services";
            this.mcManagment.SelectedPage = pageDetails;

            foreach (ServiceController s in services)
            {
                if (s.ServiceName.ToLower().StartsWith("mcontrol"))
                {
                    TreeNode t = new TreeNode(s.DisplayName);
                    t.Tag = s;
                    this.mcManagment.Items.Add(t);
                }
            }
            mcManagment.TreeView.Sort();
            //listBoxServices.DisplayMember = "DisplayName";
            //listBoxServices.DataSource = services;

        }

        protected string GetServiceTypeName(ServiceType type)
        {
            string serviceType = "";
            if ((type & ServiceType.InteractiveProcess) != 0)
            {
                serviceType = "Interactive ";
                type -= ServiceType.InteractiveProcess;
            }
            switch (type)
            {
                case ServiceType.Adapter:
                    serviceType += "Adapter";
                    break;
                case ServiceType.FileSystemDriver:
                case ServiceType.KernelDriver:
                case ServiceType.RecognizerDriver:
                    serviceType += "Driver";
                    break;
                case ServiceType.Win32OwnProcess:
                    serviceType += "Win32 Service Process";
                    break;
                case ServiceType.Win32ShareProcess:
                    serviceType += "Win32 Shared Process";
                    break;
                default:
                    serviceType += "unknown type " + type.ToString();
                    break;
            }
            return serviceType;
        }

        protected void SetServiceStatus(ServiceController controller)
        {
            tbStart.Enabled = true;
            tbStop.Enabled = true;
            tbPause.Enabled = true;
            tbRestart.Enabled = true;
            if (!controller.CanPauseAndContinue)
            {
                tbPause.Enabled = false;
                //tbRestart.Enabled = false;
            }
            if (!controller.CanStop)
            {
                tbStop.Enabled = false;
            }
            ServiceControllerStatus status = controller.Status;
            switch (status)
            {
                case ServiceControllerStatus.ContinuePending:
                    //textServiceStatus.Text = "Continue Pending";
                    tbPause.Enabled = false;
                    break;
                case ServiceControllerStatus.Paused:
                    //textServiceStatus.Text = "Paused";
                    tbPause.Enabled = false;
                    tbStart.Enabled = false;
                    break;
                case ServiceControllerStatus.PausePending:
                    //textServiceStatus.Text = "Pause Pending";
                    tbPause.Enabled = false;
                    tbStart.Enabled = false;
                    break;
                case ServiceControllerStatus.StartPending:
                    //textServiceStatus.Text = "Start Pending";
                    tbStart.Enabled = false;
                    break;
                case ServiceControllerStatus.Running:
                    //textServiceStatus.Text = "Running";
                    tbStart.Enabled = false;
                    break;
                case ServiceControllerStatus.Stopped:
                    //textServiceStatus.Text = "Stopped";
                    tbStop.Enabled = false;
                    tbRestart.Enabled = false;
                    break;
                case ServiceControllerStatus.StopPending:
                    //textServiceStatus.Text = "Stop Pending";
                    tbStop.Enabled = false;
                    tbRestart.Enabled = false;
                    break;
                default:
                    //textServiceStatus.Text = "Unknown status";
                    break;
            }

        }
        #endregion

        #region Queues

        private void CreateNodeList()
        {
            if (!shouldRefresh && curAction == Actions.Queues)
                return;

            try
            {
                //RemoteQueue q = new RemoteQueue(null);
                string[] list = RemoteQueue.QueueList;
                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Queue List";
                //nodeRoot = new TreeNode("Queue List");

                //mcManagment.TreeView.Nodes.Add(nodeRoot);

                // GetQueueItemsTable
                //GetQueueItems

                if (list == null)
                    goto Label_Exit;

                foreach (string s in list)
                {
                    TreeNode t = new TreeNode(s);
                    t.Tag = s;
                    this.mcManagment.Items.Add(t);
                }
                mcManagment.TreeView.Sort();
                //listBoxServices.DisplayMember = "DisplayName";
                //listBoxServices.DataSource = services;
                shouldRefresh = false;
                
                Label_Exit:
                curAction = Actions.Queues;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }

        }

        private void ShowGridItems()
        {
            string text = GetSelectedItem();
            if (text != null)
            {
                ShowGridItems(text);
            }
        }

        private void ShowGridItems(string name)
        {
            this.mcManagment.SelectedPage = pgItems;
            DataTable dt = RemoteQueue.Client().GetQueueItemsTable(name);
            this.gridItems.DataSource = dt;
        }


        #endregion

        private string GetSelectedItem()
        {
            TreeNode node = mcManagment.TreeView.SelectedNode;
            if (node == null)
                return null;
            return node.Text;
        }

    
    
        private void mcManagment_ToolButtonClick(object sender, MControl.WinForms.ToolButtonClickEventArgs e)
        {

            ServiceController controller = null;
            int index = 0;

            //TreeNode node = mcManagment.TreeView.SelectedNode;

            //if (node == null)
            //    return;


            try
            {

                if (curAction == Actions.Services)
                {
                    TreeNode node = mcManagment.TreeView.SelectedNode;
                    if (node != null)
                    {
                        controller = (ServiceController)node.Tag;
                        index = node.Index;
                    }
                }
                Cursor.Current = Cursors.WaitCursor;


                switch (e.Button.Name)
                {
                    case "tbActions":
                        {
                            index = 0;
                            switch (e.Button.SelectedPopUpItem.Text)
                            {
                                case "Services":
                                    //tbActions.Text = "Services";
                                    tbUsage.Enabled = false;
                                    tbItems.Enabled = false;
                                    this.mcManagment.SelectedPage = pageDetails;
                                    CreateServicesNodeList();
                                    RefreshServiceList();
                                    break;
                                case "Queues":
                                    //tbActions.Text = "Queues";
                                    tbUsage.Enabled = true;
                                    tbItems.Enabled = true;
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeList();
                                    return;
                            }
                        }
                        break;
                    case "tbItems":
                        this.mcManagment.SelectedPage = pgItems;
                        return;
                    case "tbUsage":
                        this.mcManagment.SelectedPage = pgUsage;
                        return;
                    case "tbBack":
                        //
                        return;
                    case "tbForward":
                        //
                        return;
                    case "tbRefresh":
                        if (curAction == Actions.Queues)
                        {
                            ShowGridItems();
                            return;
                        }
                        else if (curAction == Actions.Services)
                        {
                            RefreshServiceList();
                        }
                        return;
                    case "tbStart":
                        if (controller == null)
                            return;
                        controller.Start();
                        controller.WaitForStatus(ServiceControllerStatus.Running);
                        break;
                    case "tbStop":
                        if (controller == null)
                            return;
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped);
                        break;
                    case "tbPause":
                        if (controller == null)
                            return;
                        if (controller.Status == ServiceControllerStatus.Paused || controller.Status == ServiceControllerStatus.PausePending)
                        {
                            controller.Continue();
                            controller.WaitForStatus(ServiceControllerStatus.Running);
                        }
                        else
                        {
                            controller.Pause();
                            controller.WaitForStatus(ServiceControllerStatus.Paused);
                        }
                        break;
                    case "tbRestart":
                        if (controller == null)
                            return;
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped);
                        controller.Start();
                        controller.WaitForStatus(ServiceControllerStatus.Running);
                        break;
                    case "tbHelp":

                        return;

                }

                //int index=mcManagment.TreeView.se
                //RefreshServiceList();
                mcManagment.TreeView.SelectedNode = mcManagment.TreeView.Nodes[index];
            //SetServiceStatus(controller);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void mcManagment_SelectionNodeChanged(object sender, TreeViewEventArgs e)
        {
            if (curAction != Actions.Services)
                return;

            ServiceController controller =
                     (ServiceController)e.Node.Tag;
            this.listView.Items.Clear();
            ListViewItem item = this.listView.Items.Add(controller.DisplayName);
            item.SubItems.Add(controller.ServiceName);
            item.SubItems.Add(controller.ServiceType.ToString());
            item.SubItems.Add(controller.Status.ToString());
            SetServiceStatus(controller);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CreateServicesNodeList();
            //Config();
            //LoadDal();
            //InitChart();
            //this.timer1.Interval = interval;
            //this.timer1.Enabled = true;
        }

   
  
        private void tbOffset_SelectedItemClick(object sender, MControl.WinForms.SelectedPopUpItemEvent e)
        {
            //float offset = Types.ToFloat(e.Item.Text, 0F);
            //for (int i = 0; i < ctlPieChart1.Items.Count; i++)
            //{
            //    ctlPieChart1.Items[i].Offset = offset;
            //}

        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            //this.tbOffset.Enabled = this.tabControl.SelectedTab == pgChart;
            //this.tbRefresh.Enabled = this.tabControl.SelectedIndex <= 1;
        }
    }
}