namespace MControl.Messaging.UI
{
    partial class QueuManagmentForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueuManagmentForm));
            this.mcManagment = new MControl.Wizards.McManagment();
            this.tbHelp = new MControl.WinForms.McToolButton();
            this.tbRestart = new MControl.WinForms.McToolButton();
            this.tbPause = new MControl.WinForms.McToolButton();
            this.tbStop = new MControl.WinForms.McToolButton();
            this.tbStart = new MControl.WinForms.McToolButton();
            this.tbRefresh = new MControl.WinForms.McToolButton();
            this.tbActions = new MControl.WinForms.McToolButton();
            this.pServices = new MControl.WinForms.PopUpItem();
            this.pQueues = new MControl.WinForms.PopUpItem();
            this.tbForward = new MControl.WinForms.McToolButton();
            this.tbBack = new MControl.WinForms.McToolButton();
            this.pageDetails = new MControl.WinForms.McTabPage();
            this.listView = new MControl.WinForms.McListView();
            this.colDisplayName = new System.Windows.Forms.ColumnHeader();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.pgItems = new MControl.WinForms.McTabPage();
            this.gridItems = new MControl.GridView.Grid();
            this.pgUsage = new MControl.WinForms.McTabPage();
            this.lblUsageValue2 = new MControl.WinForms.McLabel();
            this.lblUsageValue1 = new MControl.WinForms.McLabel();
            this.lblUsage2 = new System.Windows.Forms.Label();
            this.lblUsage1 = new System.Windows.Forms.Label();
            this.ctlUsageHistory2 = new MControl.Charts.McUsageHistory();
            this.ctlUsage2 = new MControl.Charts.McUsage();
            this.ctlUsageHistory1 = new MControl.Charts.McUsageHistory();
            this.ctlUsage1 = new MControl.Charts.McUsage();
            this.pgMeter = new MControl.WinForms.McTabPage();
            this.pgChannels = new MControl.WinForms.McTabPage();
            this.pgChart = new MControl.WinForms.McTabPage();
            this.ctlLabel2 = new MControl.WinForms.McLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tbUsage = new MControl.WinForms.McToolButton();
            this.mcToolButton1 = new MControl.WinForms.McToolButton();
            this.mcToolButton2 = new MControl.WinForms.McToolButton();
            this.mcToolButton3 = new MControl.WinForms.McToolButton();
            this.mcToolButton4 = new MControl.WinForms.McToolButton();
            this.tbItems = new MControl.WinForms.McToolButton();
            this.mcManagment.ToolBar.SuspendLayout();
            this.pageDetails.SuspendLayout();
            this.pgItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridItems)).BeginInit();
            this.pgUsage.SuspendLayout();
            this.SuspendLayout();
            // 
            // mcManagment
            // 
            this.mcManagment.CaptionFont = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mcManagment.CaptionImage = ((System.Drawing.Image)(resources.GetObject("mcManagment.CaptionImage")));
            this.mcManagment.CaptionSubText = "";
            this.mcManagment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mcManagment.HideTabs = true;
            this.mcManagment.ListCaption = "";
            this.mcManagment.ListCaptionVisible = true;
            this.mcManagment.ListWidth = 194;
            this.mcManagment.Location = new System.Drawing.Point(2, 38);
            this.mcManagment.Name = "mcManagment";
            this.mcManagment.Padding = new System.Windows.Forms.Padding(2);
            this.mcManagment.SelectedIndex = 0;
            this.mcManagment.SelectedNode = null;
            this.mcManagment.SelectedToolBar = -1;
            this.mcManagment.ShowCaption = false;
            this.mcManagment.Size = new System.Drawing.Size(828, 488);
            this.mcManagment.TabIndex = 0;
            // 
            // 
            // 
            this.mcManagment.ToolBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(203)))), ((int)(((byte)(183)))));
            this.mcManagment.ToolBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mcManagment.ToolBar.ControlLayout = MControl.WinForms.ControlLayout.Visual;
            this.mcManagment.ToolBar.Controls.Add(this.tbHelp);
            this.mcManagment.ToolBar.Controls.Add(this.mcToolButton4);
            this.mcManagment.ToolBar.Controls.Add(this.tbUsage);
            this.mcManagment.ToolBar.Controls.Add(this.tbItems);
            this.mcManagment.ToolBar.Controls.Add(this.mcToolButton3);
            this.mcManagment.ToolBar.Controls.Add(this.tbRestart);
            this.mcManagment.ToolBar.Controls.Add(this.tbPause);
            this.mcManagment.ToolBar.Controls.Add(this.tbStop);
            this.mcManagment.ToolBar.Controls.Add(this.tbStart);
            this.mcManagment.ToolBar.Controls.Add(this.mcToolButton2);
            this.mcManagment.ToolBar.Controls.Add(this.tbRefresh);
            this.mcManagment.ToolBar.Controls.Add(this.tbActions);
            this.mcManagment.ToolBar.Controls.Add(this.mcToolButton1);
            this.mcManagment.ToolBar.Controls.Add(this.tbForward);
            this.mcManagment.ToolBar.Controls.Add(this.tbBack);
            this.mcManagment.ToolBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.mcManagment.ToolBar.FixSize = false;
            this.mcManagment.ToolBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.mcManagment.ToolBar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mcManagment.ToolBar.Location = new System.Drawing.Point(0, 0);
            this.mcManagment.ToolBar.Name = "toolBar";
            this.mcManagment.ToolBar.Padding = new System.Windows.Forms.Padding(12, 3, 3, 3);
            this.mcManagment.ToolBar.SelectedGroup = -1;
            this.mcManagment.ToolBar.Size = new System.Drawing.Size(828, 26);
            this.mcManagment.ToolBar.TabIndex = 8;
            this.mcManagment.WizardPages.AddRange(new MControl.WinForms.McTabPage[] {
            this.pageDetails,
            this.pgItems,
            this.pgUsage,
            this.pgMeter,
            this.pgChannels,
            this.pgChart});
            this.mcManagment.SelectionNodeChanged += new System.Windows.Forms.TreeViewEventHandler(this.mcManagment_SelectionNodeChanged);
            this.mcManagment.ToolButtonClick += new MControl.WinForms.ToolButtonClickEventHandler(this.mcManagment_ToolButtonClick);
            // 
            // tbHelp
            // 
            this.tbHelp.ButtonStyle = MControl.WinForms.ToolButtonStyle.Button;
            this.tbHelp.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbHelp.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbHelp.Image = global::MControl.Messaging.UI.Properties.Resources.Help;
            this.tbHelp.Location = new System.Drawing.Point(360, 3);
            this.tbHelp.Name = "tbHelp";
            this.tbHelp.Size = new System.Drawing.Size(22, 20);
            this.tbHelp.TabIndex = 7;
            this.tbHelp.ToolTipText = "";
            // 
            // tbRestart
            // 
            this.tbRestart.ButtonStyle = MControl.WinForms.ToolButtonStyle.Button;
            this.tbRestart.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbRestart.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbRestart.Image = global::MControl.Messaging.UI.Properties.Resources.RestartHS;
            this.tbRestart.Location = new System.Drawing.Point(262, 3);
            this.tbRestart.Name = "tbRestart";
            this.tbRestart.Size = new System.Drawing.Size(22, 20);
            this.tbRestart.TabIndex = 6;
            this.tbRestart.ToolTipText = "";
            // 
            // tbPause
            // 
            this.tbPause.ButtonStyle = MControl.WinForms.ToolButtonStyle.Button;
            this.tbPause.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbPause.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbPause.Image = global::MControl.Messaging.UI.Properties.Resources.PauseHS;
            this.tbPause.Location = new System.Drawing.Point(240, 3);
            this.tbPause.Name = "tbPause";
            this.tbPause.Size = new System.Drawing.Size(22, 20);
            this.tbPause.TabIndex = 5;
            this.tbPause.ToolTipText = "";
            // 
            // tbStop
            // 
            this.tbStop.ButtonStyle = MControl.WinForms.ToolButtonStyle.Button;
            this.tbStop.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbStop.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbStop.Image = global::MControl.Messaging.UI.Properties.Resources.StopHS;
            this.tbStop.Location = new System.Drawing.Point(218, 3);
            this.tbStop.Name = "tbStop";
            this.tbStop.Size = new System.Drawing.Size(22, 20);
            this.tbStop.TabIndex = 4;
            this.tbStop.ToolTipText = "";
            // 
            // tbStart
            // 
            this.tbStart.ButtonStyle = MControl.WinForms.ToolButtonStyle.Button;
            this.tbStart.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbStart.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbStart.Image = global::MControl.Messaging.UI.Properties.Resources.PlayHS;
            this.tbStart.Location = new System.Drawing.Point(196, 3);
            this.tbStart.Name = "tbStart";
            this.tbStart.Size = new System.Drawing.Size(22, 20);
            this.tbStart.TabIndex = 3;
            this.tbStart.ToolTipText = "";
            // 
            // tbRefresh
            // 
            this.tbRefresh.ButtonStyle = MControl.WinForms.ToolButtonStyle.Button;
            this.tbRefresh.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbRefresh.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbRefresh.Image = global::MControl.Messaging.UI.Properties.Resources.RefreshDocViewHS;
            this.tbRefresh.Location = new System.Drawing.Point(158, 3);
            this.tbRefresh.Name = "tbRefresh";
            this.tbRefresh.Size = new System.Drawing.Size(22, 20);
            this.tbRefresh.TabIndex = 2;
            this.tbRefresh.ToolTipText = "";
            // 
            // tbActions
            // 
            this.tbActions.ButtonStyle = MControl.WinForms.ToolButtonStyle.DropDownButton;
            this.tbActions.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbActions.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbActions.Image = global::MControl.Messaging.UI.Properties.Resources.settings;
            this.tbActions.Location = new System.Drawing.Point(72, 3);
            this.tbActions.MenuItems.AddRange(new MControl.WinForms.PopUpItem[] {
            this.pServices,
            this.pQueues});
            this.tbActions.Name = "tbActions";
            this.tbActions.Size = new System.Drawing.Size(86, 20);
            this.tbActions.TabIndex = 8;
            this.tbActions.Text = "Action";
            this.tbActions.ToolTipText = "Action";
            // 
            // pServices
            // 
            this.pServices.Text = "Services";
            // 
            // pQueues
            // 
            this.pQueues.Text = "Queues";
            // 
            // tbForward
            // 
            this.tbForward.ButtonStyle = MControl.WinForms.ToolButtonStyle.Button;
            this.tbForward.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbForward.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbForward.Image = global::MControl.Messaging.UI.Properties.Resources.GoToNextHS;
            this.tbForward.Location = new System.Drawing.Point(34, 3);
            this.tbForward.Name = "tbForward";
            this.tbForward.Size = new System.Drawing.Size(22, 20);
            this.tbForward.TabIndex = 1;
            this.tbForward.ToolTipText = "";
            // 
            // tbBack
            // 
            this.tbBack.ButtonStyle = MControl.WinForms.ToolButtonStyle.Button;
            this.tbBack.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbBack.Image = global::MControl.Messaging.UI.Properties.Resources.GoToPrevious;
            this.tbBack.Location = new System.Drawing.Point(12, 3);
            this.tbBack.Name = "tbBack";
            this.tbBack.Size = new System.Drawing.Size(22, 20);
            this.tbBack.TabIndex = 0;
            this.tbBack.ToolTipText = "";
            // 
            // pageDetails
            // 
            this.pageDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(245)))), ((int)(((byte)(232)))));
            this.pageDetails.Controls.Add(this.listView);
            this.pageDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.pageDetails.Location = new System.Drawing.Point(4, 4);
            this.pageDetails.Name = "pageDetails";
            this.pageDetails.Size = new System.Drawing.Size(623, 427);
            this.pageDetails.Text = "MControl Service Managment";
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDisplayName,
            this.colName,
            this.colType,
            this.colStatus});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(623, 427);
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // colDisplayName
            // 
            this.colDisplayName.Text = "Display name";
            this.colDisplayName.Width = 200;
            // 
            // colName
            // 
            this.colName.DisplayIndex = 3;
            this.colName.Text = "Service name";
            this.colName.Width = 200;
            // 
            // colType
            // 
            this.colType.DisplayIndex = 1;
            this.colType.Text = "Service type";
            this.colType.Width = 80;
            // 
            // colStatus
            // 
            this.colStatus.DisplayIndex = 2;
            this.colStatus.Text = "Status";
            this.colStatus.Width = 80;
            // 
            // pgItems
            // 
            this.pgItems.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(245)))), ((int)(((byte)(232)))));
            this.pgItems.Controls.Add(this.gridItems);
            this.pgItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.pgItems.Location = new System.Drawing.Point(4, 4);
            this.pgItems.Name = "pgItems";
            this.pgItems.Size = new System.Drawing.Size(623, 427);
            this.pgItems.Text = "pgItems";
            // 
            // gridItems
            // 
            this.gridItems.AllowAdd = false;
            this.gridItems.AllowRemove = false;
            this.gridItems.BackColor = System.Drawing.Color.White;
            this.gridItems.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridItems.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.gridItems.CaptionVisible = false;
            this.gridItems.DataMember = "";
            this.gridItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.gridItems.ForeColor = System.Drawing.Color.Black;
            this.gridItems.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.gridItems.Location = new System.Drawing.Point(0, 0);
            this.gridItems.Name = "gridItems";
            this.gridItems.ReadOnly = true;
            this.gridItems.Size = new System.Drawing.Size(623, 427);
            this.gridItems.TabIndex = 15;
            // 
            // pgUsage
            // 
            this.pgUsage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(245)))), ((int)(((byte)(232)))));
            this.pgUsage.Controls.Add(this.lblUsageValue2);
            this.pgUsage.Controls.Add(this.lblUsageValue1);
            this.pgUsage.Controls.Add(this.lblUsage2);
            this.pgUsage.Controls.Add(this.lblUsage1);
            this.pgUsage.Controls.Add(this.ctlUsageHistory2);
            this.pgUsage.Controls.Add(this.ctlUsage2);
            this.pgUsage.Controls.Add(this.ctlUsageHistory1);
            this.pgUsage.Controls.Add(this.ctlUsage1);
            this.pgUsage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.pgUsage.Location = new System.Drawing.Point(4, 4);
            this.pgUsage.Name = "pgUsage";
            this.pgUsage.Size = new System.Drawing.Size(623, 427);
            this.pgUsage.Text = "pgUsage";
            // 
            // lblUsageValue2
            // 
            this.lblUsageValue2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lblUsageValue2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(245)))), ((int)(((byte)(232)))));
            this.lblUsageValue2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblUsageValue2.ControlLayout = MControl.WinForms.ControlLayout.System;
            this.lblUsageValue2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsageValue2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblUsageValue2.Location = new System.Drawing.Point(23, 113);
            this.lblUsageValue2.Name = "lblUsageValue2";
            this.lblUsageValue2.Size = new System.Drawing.Size(41, 13);
            this.lblUsageValue2.Text = "0";
            // 
            // lblUsageValue1
            // 
            this.lblUsageValue1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(245)))), ((int)(((byte)(232)))));
            this.lblUsageValue1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblUsageValue1.ControlLayout = MControl.WinForms.ControlLayout.System;
            this.lblUsageValue1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsageValue1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblUsageValue1.Location = new System.Drawing.Point(23, 10);
            this.lblUsageValue1.Name = "lblUsageValue1";
            this.lblUsageValue1.Size = new System.Drawing.Size(41, 13);
            this.lblUsageValue1.Text = "0";
            // 
            // lblUsage2
            // 
            this.lblUsage2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lblUsage2.AutoSize = true;
            this.lblUsage2.Location = new System.Drawing.Point(82, 113);
            this.lblUsage2.Name = "lblUsage2";
            this.lblUsage2.Size = new System.Drawing.Size(13, 13);
            this.lblUsage2.TabIndex = 11;
            this.lblUsage2.Text = "2";
            // 
            // lblUsage1
            // 
            this.lblUsage1.AutoSize = true;
            this.lblUsage1.Location = new System.Drawing.Point(82, 10);
            this.lblUsage1.Name = "lblUsage1";
            this.lblUsage1.Size = new System.Drawing.Size(13, 13);
            this.lblUsage1.TabIndex = 10;
            this.lblUsage1.Text = "1";
            // 
            // ctlUsageHistory2
            // 
            this.ctlUsageHistory2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ctlUsageHistory2.BackColor = System.Drawing.Color.Black;
            this.ctlUsageHistory2.Location = new System.Drawing.Point(85, 129);
            this.ctlUsageHistory2.Maximum = 100;
            this.ctlUsageHistory2.Name = "ctlUsageHistory2";
            this.ctlUsageHistory2.Size = new System.Drawing.Size(523, 82);
            this.ctlUsageHistory2.TabIndex = 5;
            // 
            // ctlUsage2
            // 
            this.ctlUsage2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ctlUsage2.BackColor = System.Drawing.Color.Black;
            this.ctlUsage2.Location = new System.Drawing.Point(23, 129);
            this.ctlUsage2.Maximum = 100;
            this.ctlUsage2.Name = "ctlUsage2";
            this.ctlUsage2.Size = new System.Drawing.Size(41, 82);
            this.ctlUsage2.TabIndex = 4;
            this.ctlUsage2.Value1 = 100;
            this.ctlUsage2.Value2 = 1;
            // 
            // ctlUsageHistory1
            // 
            this.ctlUsageHistory1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ctlUsageHistory1.BackColor = System.Drawing.Color.Black;
            this.ctlUsageHistory1.Location = new System.Drawing.Point(85, 26);
            this.ctlUsageHistory1.Maximum = 100;
            this.ctlUsageHistory1.Name = "ctlUsageHistory1";
            this.ctlUsageHistory1.Size = new System.Drawing.Size(523, 82);
            this.ctlUsageHistory1.TabIndex = 3;
            // 
            // ctlUsage1
            // 
            this.ctlUsage1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ctlUsage1.BackColor = System.Drawing.Color.Black;
            this.ctlUsage1.Location = new System.Drawing.Point(23, 26);
            this.ctlUsage1.Maximum = 100;
            this.ctlUsage1.Name = "ctlUsage1";
            this.ctlUsage1.Size = new System.Drawing.Size(41, 82);
            this.ctlUsage1.TabIndex = 2;
            this.ctlUsage1.Value1 = 100;
            this.ctlUsage1.Value2 = 1;
            // 
            // pgMeter
            // 
            this.pgMeter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.pgMeter.Location = new System.Drawing.Point(4, 4);
            this.pgMeter.Name = "pgMeter";
            this.pgMeter.Size = new System.Drawing.Size(623, 427);
            this.pgMeter.Text = "pgMeter";
            // 
            // pgChannels
            // 
            this.pgChannels.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.pgChannels.Location = new System.Drawing.Point(4, 4);
            this.pgChannels.Name = "pgChannels";
            this.pgChannels.Size = new System.Drawing.Size(623, 427);
            this.pgChannels.Text = "pgChannels";
            // 
            // pgChart
            // 
            this.pgChart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.pgChart.Location = new System.Drawing.Point(4, 4);
            this.pgChart.Name = "pgChart";
            this.pgChart.Size = new System.Drawing.Size(623, 427);
            this.pgChart.Text = "pgChart";
            // 
            // ctlLabel2
            // 
            this.ctlLabel2.BackColor = System.Drawing.SystemColors.Control;
            this.ctlLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ctlLabel2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ctlLabel2.Location = new System.Drawing.Point(0, 0);
            this.ctlLabel2.Name = "ctlLabel2";
            this.ctlLabel2.Size = new System.Drawing.Size(80, 20);
            // 
            // tbUsage
            // 
            this.tbUsage.ButtonStyle = MControl.WinForms.ToolButtonStyle.Button;
            this.tbUsage.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbUsage.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbUsage.Image = global::MControl.Messaging.UI.Properties.Resources.algorithm;
            this.tbUsage.Location = new System.Drawing.Point(322, 3);
            this.tbUsage.Name = "tbUsage";
            this.tbUsage.Size = new System.Drawing.Size(22, 20);
            this.tbUsage.TabIndex = 9;
            this.tbUsage.ToolTipText = "Usage";
            // 
            // mcToolButton1
            // 
            this.mcToolButton1.ButtonStyle = MControl.WinForms.ToolButtonStyle.Separator;
            this.mcToolButton1.DialogResult = System.Windows.Forms.DialogResult.None;
            this.mcToolButton1.Dock = System.Windows.Forms.DockStyle.Left;
            this.mcToolButton1.Location = new System.Drawing.Point(56, 3);
            this.mcToolButton1.Name = "mcToolButton1";
            this.mcToolButton1.Size = new System.Drawing.Size(16, 20);
            this.mcToolButton1.TabIndex = 10;
            this.mcToolButton1.ToolTipText = "";
            // 
            // mcToolButton2
            // 
            this.mcToolButton2.ButtonStyle = MControl.WinForms.ToolButtonStyle.Separator;
            this.mcToolButton2.DialogResult = System.Windows.Forms.DialogResult.None;
            this.mcToolButton2.Dock = System.Windows.Forms.DockStyle.Left;
            this.mcToolButton2.Location = new System.Drawing.Point(180, 3);
            this.mcToolButton2.Name = "mcToolButton2";
            this.mcToolButton2.Size = new System.Drawing.Size(16, 20);
            this.mcToolButton2.TabIndex = 11;
            this.mcToolButton2.ToolTipText = "";
            // 
            // mcToolButton3
            // 
            this.mcToolButton3.ButtonStyle = MControl.WinForms.ToolButtonStyle.Separator;
            this.mcToolButton3.DialogResult = System.Windows.Forms.DialogResult.None;
            this.mcToolButton3.Dock = System.Windows.Forms.DockStyle.Left;
            this.mcToolButton3.Location = new System.Drawing.Point(284, 3);
            this.mcToolButton3.Name = "mcToolButton3";
            this.mcToolButton3.Size = new System.Drawing.Size(16, 20);
            this.mcToolButton3.TabIndex = 12;
            this.mcToolButton3.ToolTipText = "";
            // 
            // mcToolButton4
            // 
            this.mcToolButton4.ButtonStyle = MControl.WinForms.ToolButtonStyle.Separator;
            this.mcToolButton4.DialogResult = System.Windows.Forms.DialogResult.None;
            this.mcToolButton4.Dock = System.Windows.Forms.DockStyle.Left;
            this.mcToolButton4.Location = new System.Drawing.Point(344, 3);
            this.mcToolButton4.Name = "mcToolButton4";
            this.mcToolButton4.Size = new System.Drawing.Size(16, 20);
            this.mcToolButton4.TabIndex = 13;
            this.mcToolButton4.ToolTipText = "";
            // 
            // tbItems
            // 
            this.tbItems.AllowAllUp = true;
            this.tbItems.ButtonStyle = MControl.WinForms.ToolButtonStyle.Button;
            this.tbItems.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbItems.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbItems.Image = global::MControl.Messaging.UI.Properties.Resources.summar1;
            this.tbItems.Location = new System.Drawing.Point(300, 3);
            this.tbItems.Name = "tbItems";
            this.tbItems.Size = new System.Drawing.Size(22, 20);
            this.tbItems.TabIndex = 14;
            this.tbItems.ToolTipText = "";
            // 
            // QueuManagmentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CaptionVisible = true;
            this.ClientSize = new System.Drawing.Size(832, 528);
            this.ControlLayout = MControl.WinForms.ControlLayout.Visual;
            this.Controls.Add(this.mcManagment);
            this.Name = "QueuManagmentForm";
            this.Text = "MControl Service Managment";
            this.Controls.SetChildIndex(this.mcManagment, 0);
            this.mcManagment.ToolBar.ResumeLayout(false);
            this.pageDetails.ResumeLayout(false);
            this.pgItems.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridItems)).EndInit();
            this.pgUsage.ResumeLayout(false);
            this.pgUsage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MControl.Wizards.McManagment mcManagment;
        private MControl.WinForms.McToolButton tbHelp;
        private MControl.WinForms.McToolButton tbRestart;
        private MControl.WinForms.McToolButton tbPause;
        private MControl.WinForms.McToolButton tbStop;
        private MControl.WinForms.McToolButton tbStart;
        private MControl.WinForms.McToolButton tbRefresh;
        private MControl.WinForms.McToolButton tbForward;
        private MControl.WinForms.McToolButton tbBack;
        private MControl.WinForms.McTabPage pageDetails;
        private MControl.WinForms.McListView listView;
        private System.Windows.Forms.ColumnHeader colDisplayName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colName;
        private MControl.WinForms.McTabPage pgItems;
        private MControl.WinForms.McTabPage pgUsage;
        private MControl.WinForms.McTabPage pgMeter;
        private MControl.WinForms.McTabPage pgChannels;
        private MControl.WinForms.McTabPage pgChart;

        private MControl.Charts.McUsageHistory ctlUsageHistory1;
        private MControl.Charts.McUsage ctlUsage1;
        private MControl.Charts.McUsageHistory ctlUsageHistory2;
        private MControl.Charts.McUsage ctlUsage2;
        private System.Windows.Forms.Label lblUsage1;
        private System.Windows.Forms.Label lblUsage2;
        private System.Windows.Forms.Timer timer1;
        private MControl.GridView.Grid gridItems;
        private MControl.WinForms.McLabel lblUsageValue2;
        private MControl.WinForms.McLabel lblUsageValue1;
        private MControl.WinForms.McLabel lblMeterValue2;
        private MControl.WinForms.McLabel lblMeterValue1;
        private MControl.WinForms.McLabel ctlLabel2;
        private MControl.WinForms.PopUpItem itmOffset0;
        private MControl.WinForms.PopUpItem itmOffset15;
        private MControl.WinForms.McToolButton tbActions;
        private MControl.WinForms.PopUpItem pServices;
        private MControl.WinForms.PopUpItem pQueues;
        private MControl.WinForms.McToolButton tbUsage;
        private MControl.WinForms.McToolButton mcToolButton4;
        private MControl.WinForms.McToolButton mcToolButton3;
        private MControl.WinForms.McToolButton mcToolButton2;
        private MControl.WinForms.McToolButton mcToolButton1;
        private MControl.WinForms.McToolButton tbItems;
    }
}