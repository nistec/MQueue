using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;

using Nistec.Data;
using Nistec.Data.SqlClient;
using Nistec.Charts;
using Nistec.GridView;
using Nistec.WinForms;
using Nistec.Win;
using Nistec.Messaging.Remote;
using Nistec.Messaging.Server;
using Extension.Nistec.Threading;
using Nistec.Channels;
using Nistec.Serialization;
using Nistec.Generic;
using Nistec.Data.Sqlite;
using Nistec.Data.Persistance;

namespace Nistec.Messaging.UI
{
    public partial class QueueManagmentForm : McForm
    {
        #region members

        private enum Actions
        {
            Services,
            Items
        }

        private int curIndex;
        private Actions curAction = Actions.Services;
        private bool shouldRefresh=true;
        private System.ServiceProcess.ServiceController[] services;

        const string channelManager = "";
        const string channelServer = "";

        //TreeNode nodeRoot;
        //RemoteQueueClient client;
        //RemoteQueue remote;

        #endregion

        #region ctor

        public QueueManagmentForm()
        {
            InitializeComponent();
            this.tbBack.Enabled = false;
            this.tbForward.Enabled = false;
            this.mcManagment.TreeView.ImageList = this.imageList1;
            this.mcManagment.TreeView.HideSelection = false;
            //CreateNodeList();
            //remote = new RemoteQueueClient("");// RemoteQueue.Instance;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CreateServicesNodeList();
            LoadUsage();
            //Config();
            //LoadDal();
            
            //InitChart();
            //this.timer1.Interval = interval;
            //this.timer1.Enabled = true;
        }

        #endregion

        #region Items

        private void CreateNodeItems(bool shouldRefresh)
        {
            this.shouldRefresh = shouldRefresh;
            CreateNodeItems();
        }

        private void CreateNodeItems()
        {
            if (!shouldRefresh && curAction == Actions.Items)
                return;

            try
            {
                mcManagment.TreeView.Nodes.Clear();
                mcManagment.ListCaption = "Queue Items";

                //string hostAddress = "nistec_queue_manager";
                //if (cmdProtocol == NetProtocol.Tcp)
                //{
                //    var tcpSettings = TcpClientSettings.GetTcpClientSettings("nistec_queue_manager");
                //    hostAddress = tcpSettings.Address;
                //}

                var ts = ManagementApi.Get().Report(QueueCmdReport.ReportQueueList,null);
                //string[] list = TransStream.ReadValue<string[]>(ts);

                if (ts == null)
                    goto Label_Exit;
                string[] list = ts.ReadValue<string[]>();

                //string[] list = AgentManager.Queue.GetQueueList();
                if (list == null)
                    goto Label_Exit;

                //TreeNode parent = TreeNode("Data Queue", 9, 10);
                //this.mcManagment.Items.AddRange(parents);

                foreach (string s in list)
                {
                    int icon = 1;
                    string name = s;
                    TreeNode t = new TreeNode(name);
                    t.Tag = s;
                    t.ImageIndex = icon;
                    t.SelectedImageIndex = icon;
                    //parent.Nodes.Add(t);
                    this.mcManagment.Items.Add(t);
                }
                mcManagment.TreeView.Sort();
                //listBoxServices.DisplayMember = "DisplayName";
                //listBoxServices.DataSource = services;
                shouldRefresh = false;
                //LoadUsage();
                InitChart();
            Label_Exit:
                curAction = Actions.Items;
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

            try
            {
                var ts = ManagementApi.Get(ManagementApi.HostName,name,NetProtocol.Pipe).Report(QueueCmdReport.ReportQueueItems, null);
                //var dt = TransStream.ReadValue(ts);
                var dt = (ts != null) ? ts.ReadValue() : null;
                //var q= AgentManager.Queue.Get(name);
                //DataTable dt = null;

                //RemoteQueue client = new RemoteQueue(name);
                //DataTable item = client.GetQueueItemsTable();
                if (dt != null)
                {
                    this.mcManagment.SelectedPage = pgItems;
                    this.gridItems.CaptionText = name;// item.TableName;
                    this.gridItems.DataSource = dt;
                    DoStatusBar("Record Count: " + gridItems.RowCount);

                    WiredGridDropDown(1);

                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }


        #endregion

        #region Grid box

        bool EditBoxFlag = false;
        private void WiredGridDropDown(int column)
        {
            if (EditBoxFlag)
                UnWiredGridDropDown(column);
            if (gridItems.Columns.Count > column && gridItems.Columns[column].ColumnType == GridColumnType.MemoColumn)
            {
                ((GridMemoColumn)gridItems.Columns[column]).EditBox.DropDown += EditBox_DropDown;
                ((GridMemoColumn)gridItems.Columns[column]).EditBox.DropUp += EditBox_DropUp;
                EditBoxFlag = true;
            }
        }

        private void UnWiredGridDropDown(int column)
        {
            if (EditBoxFlag)
            {
                if (gridItems.Columns.Count > column && gridItems.Columns[column].ColumnType == GridColumnType.MemoColumn)
                {
                    ((GridMemoColumn)gridItems.Columns[column]).EditBox.DropDown -= EditBox_DropDown;
                    ((GridMemoColumn)gridItems.Columns[column]).EditBox.DropUp -= EditBox_DropUp;
                }
                EditBoxFlag = false;
            }
        }

        private void EditBox_DropUp(object sender, EventArgs e)
        {

        }
        private void EditBox_DropDown(object sender, EventArgs e)
        {
            DropDownColumnBody((GridMemoBox)sender);
        }
        private void DropDownColumnUp(GridMemoBox box)
        {

        }
        private void DropDownColumnBody(GridMemoBox box)
        {
            try
            {

                //DataRowView record = GetCurrentGridRow();

                object record=GetCurrentRowObject();
                if (record == null)
                {
                    return;
                }
                //string key = Types.NZ(record.Row[0], null);
                //if (key == null)
                //{
                //    return;
                //}

                //object val= Types.NZ(record.Row[1], null);
                //if (val == null)
                //{
                //    return;
                //}

                Type type = record.GetType();

                if(type==typeof(PersistItem))
                {
                    PersistItem pi = (PersistItem)record;
                    if (pi.body.GetType() == typeof(byte[]))
                    {
                        var item = BinarySerializer.Deserialize<IQueueItem>((byte[])pi.body);
                        box.Text = item.ToJson();
                    }
                    else if (pi.body.GetType() == typeof(IQueueItem))
                    {
                        box.Text = ((IQueueItem)pi.body).ToJson();
                    }
                }
                else if (type == typeof(IQueueItem))
                {
                    box.Text = ((IQueueItem)record).ToJson();
                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message, "Queue Management");
            }
        }

        void RefreshGrid()
        {
            if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
            {
                return;
            }
            gridItems.Refresh();

        }

        object GetCurrentRowObject()
        {
            if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
            {
                return null;
            }
            if (gridItems.CurrentRowIndex < 0)
            {
                return null;
            }
            return gridItems.Rows[gridItems.CurrentRowIndex];
        }
        DataRowView GetCurrentGridRow()
        {
            if (gridItems == null || gridItems.Rows == null || gridItems.Rows.Count == 0)
            {
                return null;
            }
            if (gridItems.CurrentRowIndex < 0)
            {
                return null;
            }
            return gridItems.GetCurrentDataRow();
        }

        string GetSelectedQueueKey()
        {
            DataRowView record = GetCurrentGridRow();
            if (record == null)
            {
                return null;
            }
            string key = Types.NZ(record.Row[0], null);
            return key;

        }
        string GetSelectedSessionId()
        {
            DataRowView record = GetCurrentGridRow();
            if (record == null)
            {
                return null;
            }
            string id = Types.NZ(record.Row[4], null);
            return id;

        }
        string GetSelectedSessionKey()
        {

            DataRowView record = GetCurrentGridRow();
            if (record == null)
            {
                return null;
            }
            string key = Types.NZ(record.Row[0], null);
            if (key == null)
            {
                return null;
            }

            string id = Types.NZ(record.Row[4], null);
            key = key.Replace(id + KeySet.Separator, "");
            return key;
        }

        #endregion

        #region servise


        private void CreateServicesNodeList()
        {
            if (!shouldRefresh && curAction == Actions.Services)
                return;

            RefreshServiceList();
            ToolBarSettings(Actions.Services); 
            shouldRefresh = false;
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
                if (s.ServiceName.ToLower().StartsWith("nistec"))
                {
                    TreeNode t = new TreeNode(s.DisplayName);
                    t.Tag = s;
                    t.ImageIndex = 8;
                    t.SelectedImageIndex = 8;
                    //t.StateImageIndex = 8;
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

        #region Service controller

        private ServiceController GetServiceController()
        {
            ServiceController controller = null;
            if (curAction == Actions.Services)
            {
                TreeNode node = mcManagment.TreeView.SelectedNode;
                if (node != null)
                {
                    controller = (ServiceController)node.Tag;
                    curIndex = node.Index;
                }
            }
            return controller;
        }

        private void ShowServiceDetails()
        {
            ServiceController controller = GetServiceController();
            if (controller == null)
                return;

            this.listView.Items.Clear();
            ListViewItem item = this.listView.Items.Add(controller.DisplayName);
            item.SubItems.Add(controller.ServiceName);
            item.SubItems.Add(controller.ServiceType.ToString());
            item.SubItems.Add(controller.Status.ToString());
            SetServiceStatus(controller);
        }

        private void DoRefresh()
        {
            if (curAction == Actions.Items)
            {
                if (tbItems.Checked)
                    ShowGridItems();
                else if (tbUsage.Checked)
                    RefreshStatistic();
                else if (tbStatistic.Checked)
                    DoStatistic();
                else
                    ShowGridItems();
            }
            else if (curAction == Actions.Services)
            {
                RefreshServiceList();
            }
        }
        private void DoPause()
        {
            try
            {
            ServiceController controller = GetServiceController();
            if (controller == null)
                return;
            WaitDlg.RunProgress("Pause...");
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
            System.Threading.Thread.Sleep(1000);
            SetServiceStatus(controller);
            ShowServiceDetails();
        }
        finally
        {
            WaitDlg.EndProgress();
        }
    }
        private void DoRestart()
        {
            try{
            ServiceController controller = GetServiceController();
            if (controller == null)
                return;
            WaitDlg.RunProgress("Stop...");
            controller.Stop();
            controller.WaitForStatus(ServiceControllerStatus.Stopped);
            System.Threading.Thread.Sleep(1000);
            WaitDlg.RunProgress("Start...");
            controller.Start();
            controller.WaitForStatus(ServiceControllerStatus.Running);
            System.Threading.Thread.Sleep(1000);
            SetServiceStatus(controller);
            ShowServiceDetails();
        }
        finally
        {
            WaitDlg.EndProgress();
        }

        }

        private void DoStart()
        {
            try
            {
            ServiceController controller = GetServiceController();
            if (controller == null)
                return;
            WaitDlg.RunProgress("Start...");
            controller.Start();
            controller.WaitForStatus(ServiceControllerStatus.Running);
            System.Threading.Thread.Sleep(1000);
            SetServiceStatus(controller);
            ShowServiceDetails();
        }
        finally
        {
            WaitDlg.EndProgress();
        }
    }

        private void DoStop()
        {
            try
            {
                ServiceController controller = GetServiceController();
                if (controller == null)
                    return;
                WaitDlg.RunProgress("Stop...");
                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped);
                System.Threading.Thread.Sleep(1000);
                SetServiceStatus(controller);
                ShowServiceDetails();
            }
            finally
            {
                WaitDlg.EndProgress();
            }
        }
        #endregion

        #region tool bar
        private string GetSelectedItem()
        {
            
            TreeNode node = mcManagment.TreeView.SelectedNode;
            if (node == null)
                return "";
            return node.Text;
        }

        private void ToolBarSettings(Actions mode)
        {
            bool isItems = (mode == Actions.Items);
            bool isService = (mode == Actions.Services);

            tbUsage.Enabled = !isService;
            tbItems.Enabled = !isService;
            tbProperty.Enabled = !isService;

            tbStatistic.Enabled = !isService;
            tbAddItem.Enabled = !isService;
            tbDelete.Enabled = !isService;
            tbSaveXml.Enabled = !isService;
            tbLoadXml.Enabled = !isService;
            tbCommand.Enabled = !isService;

            tbRefresh.Enabled = true;

            tbBack.Enabled = false;
            tbForward.Enabled = false;

            tbPause.Enabled = isService;
            tbRestart.Enabled = isService;
            tbStart.Enabled = isService;
            tbStop.Enabled = isService;

        }

        private void mcManagment_ToolButtonClick(object sender, Nistec.WinForms.ToolButtonClickEventArgs e)
        {

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                DoStatusBar("");

                switch (e.Button.Name)
                {
                    case "tbActions":
                        {
                            switch (e.Button.SelectedPopUpItem.Text)
                            {
                                case "Services":
                                    ToolBarSettings(Actions.Services);
                                    this.mcManagment.SelectedPage = pageDetails;
                                    CreateServicesNodeList();
                                    RefreshServiceList();
                                    break;
                                case "Items":
                                    ToolBarSettings(Actions.Items);
                                    this.mcManagment.SelectedPage = pgItems;
                                    CreateNodeItems();
                                    return;
                            }
                        }
                        break;
                    case "tbItems":
                        ShowGridItems();
                        //this.mcManagment.SelectedPage = pgItems;
                        //DoRefresh();
                        return;
                    case "tbUsage":
                        //LoadUsage();
                        this.mcManagment.SelectedPage = pgChart;
                        return;
                    case "tbBack":
                        //
                        return;
                    case "tbForward":
                        //
                        return;
                    case "tbRefresh":
                        DoRefresh();
                        return;
                    case "tbStart":
                        DoStart();
                        break;
                    case "tbStop":
                        DoStop();
                        break;
                    case "tbPause":
                        DoPause();
                        break;
                    case "tbRestart":
                        DoRestart();
                        break;
                    case "tbHelp":

                        return;
                    case "tbProperty":
                        DoProperty();
                        break;
                    case "tbSaveXml":
                        DoSaveXml();
                        return;
                    case "tbLoadXml":
                        DoLoadXml();
                        return;
                    case "tbStatistic":
                        DoStatistic();
                        return;
                    case "tbDelete":
                        DoRemoveItem();
                        break;
                    case "tbClear":
                        ClearAllItem();
                        break;
                    case "tbCommand":
                        DoCommand();
                        break;
                    case "tbAddItem":
                        DoAddItem();
                        break;

                }
                //mcManagment.TreeView.SelectedNode = mcManagment.TreeView.Nodes[index];
                //SetServiceStatus(controller);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void DoStatusBar(string text)
        {
            this.mcManagment.StatusBar.Text = text;  
        }

        private void DoAddItem()
        {
            if (AddItemDlg.Open())
            {
                CreateNodeItems(true);
            }
        }
        private void DoCommand()
        {
            if (CommandDlg.Open())
            {
                CreateNodeItems(true);
            }
        }
        private void DoRemoveItem()
        {
            string name = GetSelectedItem();
            if (MsgBox.ShowQuestion("Delete Queue " + name + "?", "Nistec", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var ts = ManagementApi.Get(ManagementApi.HostName,name, NetProtocol.Pipe).Command(QueueCmd.RemoveQueue);
                object val = (ts != null) ? ts.ReadValue():null;
                //var val = TransStream.ReadValue(ts);

                //AgentManager.Queue.RemoveQueue(name);
                CreateNodeItems(true);
            }
        }

        private void ClearAllItem()
        {
            string name = GetSelectedItem();
            if (MsgBox.ShowQuestion("Clear All items Queue " + name + "?", "Nistec", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                TransStream ts = ManagementApi.Get(ManagementApi.HostName,name, NetProtocol.Pipe).Command(QueueCmd.ClearQueue);
                object val = (ts != null) ? ts.ReadValue() : null;
                //var val = TransStream.ReadValue(ts);

                //AgentManager.Queue.ClearAllItems(name);
                CreateNodeItems(true);
            }
        }

        private void DoProperty()
        {
            string name = GetSelectedItem();
            if (string.IsNullOrEmpty(name))
                return;

            //object obj = null;
            string itemName = "";

            TransStream ts = ManagementApi.Get(ManagementApi.HostName,name, NetProtocol.Pipe).Command(QueueCmd.QueueProperty);
            //var obj = TransStream.ReadValue(ts);
            var obj = (ts != null) ? ts.ReadValue() : null;

            //var obj = AgentManager.Queue.Get(name);

            //RemoteQueue Client = new RemoteQueue(name);
            //obj = Client;
            itemName = "RemoteQueue";


            if (obj != null)
            {
                PropertyForm dlg = new PropertyForm(name);//, obj, itemName);
                dlg.Text = "Queue Item Property";
                dlg.ControlLayout = ControlLayout.Visual;
                //dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.VGrid.SetDataBinding(obj, itemName);
                dlg.ShowDialog();

            }
        }
        private void DoSaveXml()
        {
            try
            {
                string filename = CommonDlg.SaveAs("(*.xml)|*.xml", Environment.CurrentDirectory);
                if (string.IsNullOrEmpty(filename))
                    return;
                 string name = GetSelectedItem();
                 //RemoteQueue Client = new RemoteQueue(name);

                 //DataTable dt= Client.GetQueueItemsTable();
                 //DataSet ds = new DataSet("RemoteQueue");
                 //ds.Tables.Add(dt.Copy());
                 //ds.WriteXml(filename);
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void DoLoadXml()
        {
            try
            {
                string filename = CommonDlg.FileDialog("(*.xml)|*.xml", Environment.CurrentDirectory);
                if (string.IsNullOrEmpty(filename))
                    return;

                string name = GetSelectedItem();
                AsyncLoaderForm f = new AsyncLoaderForm(this, filename, name);
                f.InvokeLoader(null);

                this.DoRefresh();
                
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

       
       
        
        private void DoStatistic()
        {
            try
            {

                //RemoteQueue client=new RemoteQueue(GetSelectedItem());
                //IQueueItem item= client.Dequeue();
                //if (item != null)
                //{
                //    Nistec.GridView.VGridDlg dlg = new VGridDlg();
                //    dlg.Text = "Queue Item Property";
                //    dlg.ControlLayout = ControlLayout.Visual;
                //    dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                //    dlg.VGrid.SetDataBinding(item, client.QueueName);
                //    dlg.ShowDialog();

                //}

                DataTable dt= AgentManager.Queue.GetStatistic();

                this.mcManagment.SelectedPage = pgItems;
                this.gridItems.CaptionText = "Queue Statistic";
                this.gridItems.DataSource = dt;

            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
        }

        private void mcManagment_SelectionNodeChanged(object sender, TreeViewEventArgs e)
        {
            //DoStatusBar("");

            if (curAction == Actions.Items)
            {
                //ShowGridItems();
                string name=GetSelectedItem();
                this.lblUsageHistory.Text = name;
                this.gridItems.CaptionText = name;
                if (this.mcManagment.SelectedPage == pgItems)
                {
                    ShowGridItems();
                }
                else if (this.mcManagment.SelectedPage == pgChart)
                {
                    SetStatistic();
                }
            }
            else if (curAction == Actions.Services)
            {
                ShowServiceDetails();
            }
        }
  
        private void tbOffset_SelectedItemClick(object sender, Nistec.WinForms.SelectedPopUpItemEvent e)
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

        #endregion

        #region usage

        const int maxCapcity = 100000;
        private int maxUsage = 0;
        private int interval = 1000;
        private int tickInterval = 5;
        private bool refreshStatistic = false;
        private bool isActivated = true;
        private int intervalCount = 0;
        
        long curUsage = 0;
        int queueCount = 1;

        private void SetStatistic()
        {
            string name = GetSelectedItem();
            if (string.IsNullOrEmpty(name))
                return;

            QueuePerformanceReport qp = new QueuePerformanceReport();
            curUsage = qp.ItemsCount;
            
            //queueCount = RemoteManager.QueueList.Length;
            //if (queueCount <= 0)
            //    queueCount = 1;
            //RemoteQueue client = new RemoteQueue(name);
            //curUsage = client.Count;// Client.RemoteClient(name).Count;
            //maxUsage = client.MaxCapacity;// / queueCount;
            if (maxUsage <= 0)
                maxUsage = maxCapcity;// / queueCount;
        }

        protected  void LoadUsage()
        {
            try
            {
                if (this.timer1.Enabled)
                    return;
                //SetStatistic();
            }
            catch (Exception ex)
            {
                //this.statusStrip.Text = ex.Message;
                Nistec.WinForms.MsgDlg.ShowMsg(ex.Message, "Statistic.QueueView");
            }
            //InitChart();
            this.timer1.Interval = interval;
            this.timer1.Enabled = true;
        }

    
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            isActivated = false;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            isActivated = true;
        }

        private void RefreshStatistic()
        {
            intervalCount = 0;
            base.AsyncBeginInvoke(null);
        }

        protected override void OnAsyncExecutingWorker(AsyncCallEventArgs e)
        {
            base.OnAsyncExecutingWorker(e);
            try
            {
                SetStatistic();
            }
            catch { }
        }

        protected override void OnAsyncCompleted(AsyncCallEventArgs e)
        {
            base.OnAsyncCompleted(e);

            try
            {
                FillControls();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            finally
            {
                base.AsyncDispose();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                intervalCount++;
                refreshStatistic = intervalCount >= tickInterval;
                if (isActivated)
                {
                    if (refreshStatistic)
                    {
                        //this.statusStrip.Text = "";
                        base.AsyncBeginInvoke(null);
                        intervalCount = 0;
                    }
                    else
                    {
                        FillUsageControls();
                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        private void FillUsageControls()
        {
            if (maxUsage <= 0)
            {
                if (queueCount <= 0) queueCount = 1;
                maxUsage = maxCapcity / queueCount;
            }

            this.mcUsage1.Maximum = maxUsage;
            this.mcUsage1.Value1 = (int)curUsage;
            this.mcUsage1.Value2 = (int)0;

            this.mcUsageHistory1.Maximum = maxUsage;
            this.mcUsageHistory1.AddValues((int)curUsage, (int)0);
            this.lblUsage.Text = curUsage.ToString();

        }

        private void FillControls()
        {

            //if (Statistic == null)
            //    return;
  
            //this.ctlLedAll.ScaleValue = QueueItemTotalCount;
            if (curAction != Actions.Items)
                return;
            int count = this.mcManagment.Items.Count;
            if (count <= 0)
                return;
            if (ctlPieChart1.Items.Count != count)
            {
                InitChart();
            }

            for (int i = 0; i < count; i++)
            {
                string name = this.mcManagment.Items[i].Text;

                var client= AgentManager.Queue.Get(name);

                //RemoteQueue Client = new RemoteQueue(name);
                int queueCount = client.Count;
                //ctlPieChart1.Items.Add(new PieChartItem((double)queueCount, GetColor(i), name, name + ": " + queueCount.ToString(), 0));
                ctlPieChart1.Items[i].Weight = (double)queueCount;
                ctlPieChart1.Items[i].ToolTipText = name+":" + queueCount.ToString();
                ctlPieChart1.Items[i].PanelText = name + ":" + queueCount.ToString();

            }

            ctlPieChart1.AddChartDescription();
        }

 
        private void InitChart()
        {
            if (curAction != Actions.Items)
                return;
            ctlPieChart1.Items.Clear();

            int count = this.mcManagment.Items.Count;
            if (count <= 0)
                return;
            for (int i = 0; i < count; i++)
            {
                string name = this.mcManagment.Items[i].Text;
                var Client=AgentManager.Queue.Get(name);

                //RemoteQueue Client = new RemoteQueue(name);
                queueCount = Client.Count;
                ctlPieChart1.Items.Add(new PieChartItem(queueCount, GetColor(i), name, name+": " + queueCount.ToString(), 0));
            }

            //ctlPieChart1.Items.Add(new PieChartItem(0, Color.Blue, "Usage", "Usage: " + curItemsUsage.ToString(), 0));

            this.ctlPieChart1.Padding = new System.Windows.Forms.Padding(60, 0, 0, 0);
            this.ctlPieChart1.ItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceBrightness = 0.3F;
            this.ctlPieChart1.AddChartDescription();
            this.ctlPieChart1.Leaning = (float)(40 * Math.PI / 180);
            this.ctlPieChart1.Depth = 50;
            this.ctlPieChart1.Radius = 90F;

            //initilaized = true;
        }

        private Color GetColor(int index)
        {

            Color[] colors=new Color[]{Color.Blue,Color.Gold,Color.Green,Color.Red,Color.LightSalmon,Color.YellowGreen,Color.Turquoise,Color.Silver};
            return colors[index % 7];
           
        }

        #endregion
    }
}