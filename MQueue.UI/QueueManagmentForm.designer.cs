namespace Nistec.Messaging.UI
{
    partial class QueueManagmentForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueueManagmentForm));
            this.ctlLabel2 = new Nistec.WinForms.McLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.mcManagment = new Nistec.Wizards.McManagment();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tbHelp = new Nistec.WinForms.McToolButton();
            this.sp7 = new Nistec.WinForms.McToolButton();
            this.tbCommand = new Nistec.WinForms.McToolButton();
            this.sp6 = new Nistec.WinForms.McToolButton();
            this.tbLoadXml = new Nistec.WinForms.McToolButton();
            this.tbSaveXml = new Nistec.WinForms.McToolButton();
            this.sp5 = new Nistec.WinForms.McToolButton();
            this.tbStatistic = new Nistec.WinForms.McToolButton();
            this.tbUsage = new Nistec.WinForms.McToolButton();
            this.tbItems = new Nistec.WinForms.McToolButton();
            this.sp4 = new Nistec.WinForms.McToolButton();
            this.tbDelete = new Nistec.WinForms.McToolButton();
            this.tbAddItem = new Nistec.WinForms.McToolButton();
            this.sp3 = new Nistec.WinForms.McToolButton();
            this.tbRestart = new Nistec.WinForms.McToolButton();
            this.tbPause = new Nistec.WinForms.McToolButton();
            this.tbStop = new Nistec.WinForms.McToolButton();
            this.tbStart = new Nistec.WinForms.McToolButton();
            this.sp2 = new Nistec.WinForms.McToolButton();
            this.tbRefresh = new Nistec.WinForms.McToolButton();
            this.tbActions = new Nistec.WinForms.McToolButton();
            this.pServices = new Nistec.WinForms.PopUpItem();
            this.pItems = new Nistec.WinForms.PopUpItem();
            this.pData = new Nistec.WinForms.PopUpItem();
            this.sp1 = new Nistec.WinForms.McToolButton();
            this.tbForward = new Nistec.WinForms.McToolButton();
            this.tbBack = new Nistec.WinForms.McToolButton();
            this.pageDetails = new Nistec.WinForms.McTabPage();
            this.listView = new Nistec.WinForms.McListView();
            this.colDisplayName = new System.Windows.Forms.ColumnHeader();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.pgItems = new Nistec.WinForms.McTabPage();
            this.gridItems = new Nistec.GridView.Grid();
            this.pgSource = new Nistec.WinForms.McTabPage();
            this.txtBody = new System.Windows.Forms.RichTextBox();
            this.txtHeader = new System.Windows.Forms.TextBox();
            this.pgImage = new Nistec.WinForms.McTabPage();
            this.imgSource = new System.Windows.Forms.PictureBox();
            this.txtImageHeader = new System.Windows.Forms.TextBox();
            this.pgClass = new Nistec.WinForms.McTabPage();
            this.vgrid = new Nistec.GridView.VGrid();
            this.pgBrowser = new Nistec.WinForms.McTabPage();
            this.ctlBrowser = new System.Windows.Forms.WebBrowser();
            this.txtHeaderBrowser = new System.Windows.Forms.TextBox();
            this.pgChart = new Nistec.WinForms.McTabPage();
            this.lblUsage = new Nistec.WinForms.McLabel();
            this.lblUsageHistory = new Nistec.WinForms.McLabel();
            this.mcUsageHistory1 = new Nistec.Charts.McUsageHistory();
            this.mcUsage1 = new Nistec.Charts.McUsage();
            this.pnlChart = new Nistec.WinForms.McPanel();
            this.pnlUsage = new Nistec.WinForms.McPanel();
            this.ctlPieChart1 = new Nistec.Charts.McPieChart();
            this.mcToolButton1 = new Nistec.WinForms.McToolButton();
            this.mcToolButton2 = new Nistec.WinForms.McToolButton();
            this.mcToolButton3 = new Nistec.WinForms.McToolButton();
            this.mcToolButton5 = new Nistec.WinForms.McToolButton();
            this.mcToolButton4 = new Nistec.WinForms.McToolButton();
            this.mcToolButton6 = new Nistec.WinForms.McToolButton();
            this.tbProperty = new Nistec.WinForms.McToolButton();
            this.mcManagment.ToolBar.SuspendLayout();
            this.pageDetails.SuspendLayout();
            this.pgItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridItems)).BeginInit();
            this.pgSource.SuspendLayout();
            this.pgImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSource)).BeginInit();
            this.pgClass.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vgrid)).BeginInit();
            this.pgBrowser.SuspendLayout();
            this.pgChart.SuspendLayout();
            this.pnlChart.SuspendLayout();
            this.pnlUsage.SuspendLayout();
            this.SuspendLayout();
            // 
            // StyleGuideBase
            // 
            this.StyleGuideBase.BorderColor = System.Drawing.Color.DarkGray;// SlateGray;
            this.StyleGuideBase.StylePlan = WinForms.Styles.SteelBlue;
            //this.gridItems.LayoutManager.Layout.StylePlan = WinForms.Styles.SteelBlue;
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
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // mcManagment
            // 
            this.mcManagment.CaptionFont = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mcManagment.CaptionImage = ((System.Drawing.Image)(resources.GetObject("mcManagment.CaptionImage")));
            this.mcManagment.CaptionSubText = "";
            this.mcManagment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mcManagment.HideTabs = true;
            this.mcManagment.ImageList = this.imageList1;
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
            this.mcManagment.Size = new System.Drawing.Size(849, 509);
            this.mcManagment.TabIndex = 0;
            // 
            // 
            // 
            this.mcManagment.ToolBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(203)))), ((int)(((byte)(183)))));
            this.mcManagment.ToolBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mcManagment.ToolBar.ControlLayout = Nistec.WinForms.ControlLayout.Visual;
            this.mcManagment.ToolBar.Controls.Add(this.tbHelp);
            this.mcManagment.ToolBar.Controls.Add(this.sp7);
            this.mcManagment.ToolBar.Controls.Add(this.tbCommand);
            this.mcManagment.ToolBar.Controls.Add(this.sp6);
            this.mcManagment.ToolBar.Controls.Add(this.tbLoadXml);
            this.mcManagment.ToolBar.Controls.Add(this.tbSaveXml);
            this.mcManagment.ToolBar.Controls.Add(this.sp5);
            this.mcManagment.ToolBar.Controls.Add(this.tbStatistic);
            this.mcManagment.ToolBar.Controls.Add(this.tbUsage);
            this.mcManagment.ToolBar.Controls.Add(this.tbItems);
            this.mcManagment.ToolBar.Controls.Add(this.sp4);
            this.mcManagment.ToolBar.Controls.Add(this.tbDelete);
            this.mcManagment.ToolBar.Controls.Add(this.tbAddItem);
            this.mcManagment.ToolBar.Controls.Add(this.tbProperty);
            this.mcManagment.ToolBar.Controls.Add(this.sp3);
            this.mcManagment.ToolBar.Controls.Add(this.tbRestart);
            this.mcManagment.ToolBar.Controls.Add(this.tbPause);
            this.mcManagment.ToolBar.Controls.Add(this.tbStop);
            this.mcManagment.ToolBar.Controls.Add(this.tbStart);
            this.mcManagment.ToolBar.Controls.Add(this.sp2);
            this.mcManagment.ToolBar.Controls.Add(this.tbRefresh);
            this.mcManagment.ToolBar.Controls.Add(this.tbActions);
            this.mcManagment.ToolBar.Controls.Add(this.sp1);
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
            this.mcManagment.ToolBar.Size = new System.Drawing.Size(849, 26);
            this.mcManagment.ToolBar.TabIndex = 8;
            this.mcManagment.WizardPages.AddRange(new Nistec.WinForms.McTabPage[] {
            this.pageDetails,
            this.pgItems,
            this.pgSource,
            this.pgImage,
            this.pgClass,
            this.pgBrowser,
            this.pgChart});
            this.mcManagment.SelectionNodeChanged += new System.Windows.Forms.TreeViewEventHandler(this.mcManagment_SelectionNodeChanged);
            this.mcManagment.ToolButtonClick += new Nistec.WinForms.ToolButtonClickEventHandler(this.mcManagment_ToolButtonClick);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Default.png");
            this.imageList1.Images.SetKeyName(1, "Data.gif");
            this.imageList1.Images.SetKeyName(2, "File.gif");
            this.imageList1.Images.SetKeyName(3, "Binary.gif");
            this.imageList1.Images.SetKeyName(4, "Image.gif");
            this.imageList1.Images.SetKeyName(5, "XMLFileHS.png");
            this.imageList1.Images.SetKeyName(6, "Class.gif");
            this.imageList1.Images.SetKeyName(7, "htmlicon.gif");
            this.imageList1.Images.SetKeyName(8, "gears.gif");
            this.imageList1.Images.SetKeyName(9, "NewFolderHS.png");
            this.imageList1.Images.SetKeyName(10, "openfolderHS.png");
            // 
            // tbHelp
            // 
            this.tbHelp.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbHelp.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbHelp.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbHelp.Image = global::Nistec.Messaging.UI.Properties.Resources.Help;
            this.tbHelp.Location = new System.Drawing.Point(551, 3);
            this.tbHelp.Name = "tbHelp";
            this.tbHelp.Size = new System.Drawing.Size(22, 20);
            this.tbHelp.TabIndex = 7;
            this.tbHelp.ToolTipText = "Help";
            // 
            // sp6
            // 
            this.sp6.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.sp6.DialogResult = System.Windows.Forms.DialogResult.None;
            this.sp6.Dock = System.Windows.Forms.DockStyle.Left;
            this.sp6.Location = new System.Drawing.Point(529, 3);
            this.sp6.Name = "sp6";
            this.sp6.Size = new System.Drawing.Size(14, 20);
            this.sp6.TabIndex = 21;
            this.sp6.ToolTipText = "";
            // 
            // tbCommand
            // 
            this.tbCommand.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbCommand.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbCommand.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbCommand.Image = global::Nistec.Messaging.UI.Properties.Resources.record;
            this.tbCommand.Location = new System.Drawing.Point(521, 3);
            this.tbCommand.Name = "tbCommand";
            this.tbCommand.Size = new System.Drawing.Size(22, 20);
            this.tbCommand.TabIndex = 7;
            this.tbCommand.ToolTipText = "Command";
            // 
            // sp7
            // 
            this.sp7.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.sp7.DialogResult = System.Windows.Forms.DialogResult.None;
            this.sp7.Dock = System.Windows.Forms.DockStyle.Left;
            this.sp7.Location = new System.Drawing.Point(507, 3);
            this.sp7.Name = "sp7";
            this.sp7.Size = new System.Drawing.Size(14, 20);
            this.sp7.TabIndex = 21;
            this.sp7.ToolTipText = "";
            // 
            // tbLoadXml
            // 
            this.tbLoadXml.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbLoadXml.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbLoadXml.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbLoadXml.Image = global::Nistec.Messaging.UI.Properties.Resources.paste;
            this.tbLoadXml.Location = new System.Drawing.Point(485, 3);
            this.tbLoadXml.Name = "tbLoadXml";
            this.tbLoadXml.Size = new System.Drawing.Size(22, 20);
            this.tbLoadXml.TabIndex = 20;
            this.tbLoadXml.ToolTipText = "Load Queue from file";
            // 
            // tbSaveXml
            // 
            this.tbSaveXml.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbSaveXml.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbSaveXml.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbSaveXml.Image = global::Nistec.Messaging.UI.Properties.Resources.saveHS;
            this.tbSaveXml.Location = new System.Drawing.Point(463, 3);
            this.tbSaveXml.Name = "tbSaveXml";
            this.tbSaveXml.Size = new System.Drawing.Size(22, 20);
            this.tbSaveXml.TabIndex = 19;
            this.tbSaveXml.ToolTipText = "Save Queue Items to file";
            // 
            // sp5
            // 
            this.sp5.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.sp5.DialogResult = System.Windows.Forms.DialogResult.None;
            this.sp5.Dock = System.Windows.Forms.DockStyle.Left;
            this.sp5.Location = new System.Drawing.Point(447, 3);
            this.sp5.Name = "sp5";
            this.sp5.Size = new System.Drawing.Size(16, 20);
            this.sp5.TabIndex = 13;
            this.sp5.ToolTipText = "";
            // 
            // tbStatistic
            // 
            this.tbStatistic.ButtonStyle = Nistec.WinForms.ToolButtonStyle.CheckButton;
            this.tbStatistic.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbStatistic.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbStatistic.OptionGroup = "ItemsOption";
            this.tbStatistic.Image = global::Nistec.Messaging.UI.Properties.Resources.barcha2;
            this.tbStatistic.Location = new System.Drawing.Point(425, 3);
            this.tbStatistic.Name = "tbStatistic";
            this.tbStatistic.Size = new System.Drawing.Size(22, 20);
            this.tbStatistic.TabIndex = 17;
            this.tbStatistic.ToolTipText = "Remote Queue statistic";
            // 
            // tbUsage
            // 
            this.tbUsage.ButtonStyle = Nistec.WinForms.ToolButtonStyle.CheckButton;
            this.tbUsage.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbUsage.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbUsage.OptionGroup = "ItemsOption";
            this.tbUsage.Image = global::Nistec.Messaging.UI.Properties.Resources.algorithm;
            this.tbUsage.Location = new System.Drawing.Point(403, 3);
            this.tbUsage.Name = "tbUsage";
            this.tbUsage.Size = new System.Drawing.Size(22, 20);
            this.tbUsage.TabIndex = 9;
            this.tbUsage.ToolTipText = "Show statistic";
            // 
            // tbItems
            // 
            this.tbItems.AllowAllUp = true;
            this.tbItems.ButtonStyle = Nistec.WinForms.ToolButtonStyle.CheckButton;
            this.tbItems.OptionGroup = "ItemsOption";
            this.tbItems.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbItems.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbItems.Image = global::Nistec.Messaging.UI.Properties.Resources.summar1;
            this.tbItems.Location = new System.Drawing.Point(381, 3);
            this.tbItems.Name = "tbItems";
            this.tbItems.Size = new System.Drawing.Size(22, 20);
            this.tbItems.TabIndex = 14;
            this.tbItems.ToolTipText = "Show Details";
            // 
            // sp4
            // 
            this.sp4.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.sp4.DialogResult = System.Windows.Forms.DialogResult.None;
            this.sp4.Dock = System.Windows.Forms.DockStyle.Left;
            this.sp4.Location = new System.Drawing.Point(366, 3);
            this.sp4.Name = "sp4";
            this.sp4.Size = new System.Drawing.Size(15, 20);
            this.sp4.TabIndex = 18;
            this.sp4.ToolTipText = "";
            // 
            // tbDelete
            // 
            this.tbDelete.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbDelete.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbDelete.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbDelete.Image = global::Nistec.Messaging.UI.Properties.Resources.logoff_small;
            this.tbDelete.Location = new System.Drawing.Point(344, 3);
            this.tbDelete.Name = "tbDelete";
            this.tbDelete.Size = new System.Drawing.Size(22, 20);
            this.tbDelete.TabIndex = 16;
            this.tbDelete.ToolTipText = "Remove item from Queue";
            // 
            // tbAddItem
            // 
            this.tbAddItem.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbAddItem.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbAddItem.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbAddItem.Image = global::Nistec.Messaging.UI.Properties.Resources.newfile_wiz;
            this.tbAddItem.Location = new System.Drawing.Point(322, 3);
            this.tbAddItem.Name = "tbAddItem";
            this.tbAddItem.Size = new System.Drawing.Size(22, 20);
            this.tbAddItem.TabIndex = 15;
            this.tbAddItem.ToolTipText = "Add new item";
            // 
            // sp3
            // 
            this.sp3.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.sp3.DialogResult = System.Windows.Forms.DialogResult.None;
            this.sp3.Dock = System.Windows.Forms.DockStyle.Left;
            this.sp3.Location = new System.Drawing.Point(284, 3);
            this.sp3.Name = "sp3";
            this.sp3.Size = new System.Drawing.Size(16, 20);
            this.sp3.TabIndex = 12;
            this.sp3.ToolTipText = "";
            // 
            // tbRestart
            // 
            this.tbRestart.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbRestart.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbRestart.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbRestart.Image = global::Nistec.Messaging.UI.Properties.Resources.RestartHS;
            this.tbRestart.Location = new System.Drawing.Point(262, 3);
            this.tbRestart.Name = "tbRestart";
            this.tbRestart.Size = new System.Drawing.Size(22, 20);
            this.tbRestart.TabIndex = 6;
            this.tbRestart.ToolTipText = "Restart service";
            // 
            // tbPause
            // 
            this.tbPause.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbPause.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbPause.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbPause.Image = global::Nistec.Messaging.UI.Properties.Resources.PauseHS;
            this.tbPause.Location = new System.Drawing.Point(240, 3);
            this.tbPause.Name = "tbPause";
            this.tbPause.Size = new System.Drawing.Size(22, 20);
            this.tbPause.TabIndex = 5;
            this.tbPause.ToolTipText = "Pause service";
            // 
            // tbStop
            // 
            this.tbStop.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbStop.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbStop.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbStop.Image = global::Nistec.Messaging.UI.Properties.Resources.StopHS;
            this.tbStop.Location = new System.Drawing.Point(218, 3);
            this.tbStop.Name = "tbStop";
            this.tbStop.Size = new System.Drawing.Size(22, 20);
            this.tbStop.TabIndex = 4;
            this.tbStop.ToolTipText = "Stop Service";
            // 
            // tbStart
            // 
            this.tbStart.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbStart.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbStart.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbStart.Image = global::Nistec.Messaging.UI.Properties.Resources.PlayHS;
            this.tbStart.Location = new System.Drawing.Point(196, 3);
            this.tbStart.Name = "tbStart";
            this.tbStart.Size = new System.Drawing.Size(22, 20);
            this.tbStart.TabIndex = 3;
            this.tbStart.ToolTipText = "Start service";
            // 
            // sp2
            // 
            this.sp2.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.sp2.DialogResult = System.Windows.Forms.DialogResult.None;
            this.sp2.Dock = System.Windows.Forms.DockStyle.Left;
            this.sp2.Location = new System.Drawing.Point(180, 3);
            this.sp2.Name = "sp2";
            this.sp2.Size = new System.Drawing.Size(16, 20);
            this.sp2.TabIndex = 11;
            this.sp2.ToolTipText = "";
            // 
            // tbRefresh
            // 
            this.tbRefresh.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbRefresh.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbRefresh.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbRefresh.Image = global::Nistec.Messaging.UI.Properties.Resources.RefreshDocViewHS;
            this.tbRefresh.Location = new System.Drawing.Point(158, 3);
            this.tbRefresh.Name = "tbRefresh";
            this.tbRefresh.Size = new System.Drawing.Size(22, 20);
            this.tbRefresh.TabIndex = 2;
            this.tbRefresh.ToolTipText = "Refresh";
            // 
            // tbActions
            // 
            this.tbActions.ButtonStyle = Nistec.WinForms.ToolButtonStyle.DropDownButton;
            this.tbActions.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbActions.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbActions.Image = global::Nistec.Messaging.UI.Properties.Resources.settings;
            this.tbActions.Location = new System.Drawing.Point(72, 3);
            this.tbActions.MenuItems.AddRange(new Nistec.WinForms.PopUpItem[] {
            this.pServices,
            this.pItems});
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
            // pItems
            // 
            this.pItems.Text = "Items";
            // 
            // pData
            // 
            this.pData.Text = "Data";
            // 
            // sp1
            // 
            this.sp1.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.sp1.DialogResult = System.Windows.Forms.DialogResult.None;
            this.sp1.Dock = System.Windows.Forms.DockStyle.Left;
            this.sp1.Location = new System.Drawing.Point(56, 3);
            this.sp1.Name = "sp1";
            this.sp1.Size = new System.Drawing.Size(16, 20);
            this.sp1.TabIndex = 10;
            this.sp1.ToolTipText = "";
            // 
            // tbForward
            // 
            this.tbForward.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbForward.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbForward.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbForward.Image = global::Nistec.Messaging.UI.Properties.Resources.GoToNextHS;
            this.tbForward.Location = new System.Drawing.Point(34, 3);
            this.tbForward.Name = "tbForward";
            this.tbForward.Size = new System.Drawing.Size(22, 20);
            this.tbForward.TabIndex = 1;
            this.tbForward.ToolTipText = "Next";
            // 
            // tbBack
            // 
            this.tbBack.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbBack.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbBack.Image = global::Nistec.Messaging.UI.Properties.Resources.GoToPrevious;
            this.tbBack.Location = new System.Drawing.Point(12, 3);
            this.tbBack.Name = "tbBack";
            this.tbBack.Size = new System.Drawing.Size(22, 20);
            this.tbBack.TabIndex = 0;
            this.tbBack.ToolTipText = "Prev";
            // 
            // pageDetails
            // 
            this.pageDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(245)))), ((int)(((byte)(232)))));
            this.pageDetails.Controls.Add(this.listView);
            this.pageDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.pageDetails.Location = new System.Drawing.Point(4, 4);
            this.pageDetails.Name = "pageDetails";
            this.pageDetails.Size = new System.Drawing.Size(644, 448);
            this.pageDetails.Text = "Nistec Service Managment";
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
            this.listView.Size = new System.Drawing.Size(644, 448);
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
            this.pgItems.Size = new System.Drawing.Size(644, 448);
            this.pgItems.Text = "pgItems";
            // 
            // gridItems
            // 
            this.gridItems.AllowAdd = false;
            this.gridItems.AllowRemove = false;
            this.gridItems.BackColor = System.Drawing.Color.White;
            this.gridItems.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridItems.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.gridItems.DataMember = "";
            this.gridItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.gridItems.ForeColor = System.Drawing.Color.Black;
            this.gridItems.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.gridItems.Location = new System.Drawing.Point(0, 0);
            this.gridItems.Name = "gridItems";
            this.gridItems.ReadOnly = true;
            this.gridItems.Size = new System.Drawing.Size(644, 448);
            this.gridItems.TabIndex = 15;
            // 
            // pgSource
            // 
            this.pgSource.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.pgSource.Controls.Add(this.txtBody);
            this.pgSource.Controls.Add(this.txtHeader);
            this.pgSource.Location = new System.Drawing.Point(4, 4);
            this.pgSource.Name = "pgSource";
            this.pgSource.Size = new System.Drawing.Size(644, 448);
            this.pgSource.Text = "pgSource";
            // 
            // txtBody
            // 
            this.txtBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBody.Location = new System.Drawing.Point(0, 20);
            this.txtBody.Name = "txtBody";
            this.txtBody.ReadOnly = true;
            this.txtBody.Size = new System.Drawing.Size(644, 428);
            this.txtBody.TabIndex = 1;
            this.txtBody.Text = "";
            // 
            // txtHeader
            // 
            this.txtHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtHeader.Location = new System.Drawing.Point(0, 0);
            this.txtHeader.Name = "txtHeader";
            this.txtHeader.ReadOnly = true;
            this.txtHeader.Size = new System.Drawing.Size(644, 20);
            this.txtHeader.TabIndex = 0;
            // 
            // pgImage
            // 
            this.pgImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.pgImage.Controls.Add(this.imgSource);
            this.pgImage.Controls.Add(this.txtImageHeader);
            this.pgImage.Location = new System.Drawing.Point(4, 4);
            this.pgImage.Name = "pgImage";
            this.pgImage.Size = new System.Drawing.Size(644, 448);
            this.pgImage.Text = "pgImage";
            // 
            // imgSource
            // 
            this.imgSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgSource.Location = new System.Drawing.Point(0, 20);
            this.imgSource.Name = "imgSource";
            this.imgSource.Size = new System.Drawing.Size(644, 428);
            this.imgSource.TabIndex = 1;
            this.imgSource.TabStop = false;
            // 
            // txtImageHeader
            // 
            this.txtImageHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtImageHeader.Location = new System.Drawing.Point(0, 0);
            this.txtImageHeader.Name = "txtImageHeader";
            this.txtImageHeader.ReadOnly = true;
            this.txtImageHeader.Size = new System.Drawing.Size(644, 20);
            this.txtImageHeader.TabIndex = 0;
            // 
            // pgClass
            // 
            this.pgClass.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.pgClass.Controls.Add(this.vgrid);
            this.pgClass.Location = new System.Drawing.Point(4, 4);
            this.pgClass.Name = "pgClass";
            this.pgClass.Size = new System.Drawing.Size(644, 448);
            this.pgClass.Text = "pgClass";
            // 
            // pgBrowser
            // 
            this.pgBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.pgBrowser.Controls.Add(this.ctlBrowser);
            this.pgBrowser.Controls.Add(this.txtHeaderBrowser);
            this.pgBrowser.Location = new System.Drawing.Point(4, 4);
            this.pgBrowser.Name = "pgBrowser";
            this.pgBrowser.Size = new System.Drawing.Size(644, 448);
            this.pgBrowser.Text = "pgBrowser";
            // 
            // ctlBrowser
            // 
            this.ctlBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlBrowser.Location = new System.Drawing.Point(0, 20);
            this.ctlBrowser.Name = "ctlBrowser";
            this.ctlBrowser.Size = new System.Drawing.Size(644, 428);
            this.ctlBrowser.TabIndex = 0;
            // 
            // txtHeaderBrowser
            // 
            this.txtHeaderBrowser.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtHeaderBrowser.Location = new System.Drawing.Point(0, 0);
            this.txtHeaderBrowser.Name = "txtHeaderBrowser";
            this.txtHeaderBrowser.ReadOnly = true;
            this.txtHeaderBrowser.Size = new System.Drawing.Size(644, 20);
            this.txtHeaderBrowser.TabIndex = 0;
            // 
            // pgChart
            // 
            this.pgChart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.pgChart.Controls.Add(this.pnlChart);
            this.pgChart.Controls.Add(this.pnlUsage);
            this.pgChart.Location = new System.Drawing.Point(4, 4);
            this.pgChart.Name = "pgChart";
            this.pgChart.Size = new System.Drawing.Size(644, 448);
            this.pgChart.Text = "pgChart";
            // 
            // lblUsage
            // 
            this.lblUsage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.lblUsage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblUsage.ControlLayout = Nistec.WinForms.ControlLayout.System;
            this.lblUsage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsage.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblUsage.Location = new System.Drawing.Point(20, 22);
            this.lblUsage.Name = "lblUsage";
            this.lblUsage.Size = new System.Drawing.Size(41, 13);
            this.lblUsage.Text = "0";
            // 
            // lblUsageHistory
            // 
            this.lblUsageHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.lblUsageHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblUsageHistory.ControlLayout = Nistec.WinForms.ControlLayout.System;
            this.lblUsageHistory.AutoSize = true;
            this.lblUsageHistory.Location = new System.Drawing.Point(85, 22);
            this.lblUsageHistory.Name = "lblUsageHistory";
            this.lblUsageHistory.Size = new System.Drawing.Size(279, 13);
            this.lblUsageHistory.TabIndex = 21;
            this.lblUsageHistory.TextAlign= System.Drawing.ContentAlignment.MiddleLeft;
            this.lblUsageHistory.Text = "Queue Usage History";
            // 
            // mcUsageHistory1
            // 
            this.mcUsageHistory1.Anchor =System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.mcUsageHistory1.BackColor = System.Drawing.Color.Black;
            this.mcUsageHistory1.Location = new System.Drawing.Point(82, 40);
            this.mcUsageHistory1.Maximum = 100;
            this.mcUsageHistory1.Name = "mcUsageHistory1";
            this.mcUsageHistory1.Size = new System.Drawing.Size(480, 124);
            this.mcUsageHistory1.TabIndex = 20;
            // 
            // mcUsage1
            // 
            this.mcUsage1.BackColor = System.Drawing.Color.Black;
            this.mcUsage1.Location = new System.Drawing.Point(20, 40);
            this.mcUsage1.Maximum = 100;
            this.mcUsage1.Name = "mcUsage1";
            this.mcUsage1.Size = new System.Drawing.Size(41, 124);
            this.mcUsage1.TabIndex = 19;
            this.mcUsage1.Value1 = 100;
            this.mcUsage1.Value2 = 1;
            // 
            // pnlChart
            // 
            this.pnlChart.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
            this.pnlChart.BackColor = System.Drawing.Color.White;
            this.pnlChart.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlChart.ControlLayout = Nistec.WinForms.ControlLayout.Flat;
            this.pnlChart.Controls.Add(this.ctlPieChart1);
            this.pnlChart.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pnlChart.Location = new System.Drawing.Point(20, 23);
            this.pnlChart.Name = "pnlChart";
            this.pnlChart.Padding = new System.Windows.Forms.Padding(4);
            this.pnlChart.Size = new System.Drawing.Size(569, 180);
            this.pnlChart.TabIndex = 22;
            // 
            // pnlUsage
            // 
            this.pnlUsage.Anchor =  System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
            this.pnlUsage.BackColor = System.Drawing.Color.White;
            this.pnlUsage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlUsage.ControlLayout = Nistec.WinForms.ControlLayout.System;
            this.pnlUsage.Controls.Add(this.lblUsage);
            this.pnlUsage.Controls.Add(this.lblUsageHistory);
            this.pnlUsage.Controls.Add(this.mcUsageHistory1);
            this.pnlUsage.Controls.Add(this.mcUsage1);
            this.pnlUsage.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pnlUsage.Location = new System.Drawing.Point(20, 240);
            this.pnlUsage.Name = "pnlUsage";
            this.pnlUsage.Padding = new System.Windows.Forms.Padding(4);
            this.pnlUsage.Size = new System.Drawing.Size(569, 180);
            this.pnlUsage.TabIndex = 22;
            // 
            // ctlPieChart1
            // 
            this.ctlPieChart1.BackColor = System.Drawing.Color.White;
            this.ctlPieChart1.Depth = 50F;
            this.ctlPieChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlPieChart1.Location = new System.Drawing.Point(4, 4);
            this.ctlPieChart1.Name = "ctlPieChart1";
            this.ctlPieChart1.Radius = 180F;
            this.ctlPieChart1.Size = new System.Drawing.Size(561, 172);
            this.ctlPieChart1.TabIndex = 2;
            this.ctlPieChart1.AutoSizePie = true;
            this.ctlPieChart1.Text = "ctlPieChart1";
            // 
            // mcToolButton1
            // 
            this.mcToolButton1.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
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
            this.mcToolButton2.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
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
            this.mcToolButton3.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.mcToolButton3.DialogResult = System.Windows.Forms.DialogResult.None;
            this.mcToolButton3.Dock = System.Windows.Forms.DockStyle.Left;
            this.mcToolButton3.Location = new System.Drawing.Point(284, 3);
            this.mcToolButton3.Name = "mcToolButton3";
            this.mcToolButton3.Size = new System.Drawing.Size(16, 20);
            this.mcToolButton3.TabIndex = 12;
            this.mcToolButton3.ToolTipText = "";
            // 
            // mcToolButton5
            // 
            this.mcToolButton5.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.mcToolButton5.DialogResult = System.Windows.Forms.DialogResult.None;
            this.mcToolButton5.Dock = System.Windows.Forms.DockStyle.Left;
            this.mcToolButton5.Location = new System.Drawing.Point(344, 3);
            this.mcToolButton5.Name = "mcToolButton5";
            this.mcToolButton5.Size = new System.Drawing.Size(15, 20);
            this.mcToolButton5.TabIndex = 18;
            this.mcToolButton5.ToolTipText = "";
            // 
            // mcToolButton4
            // 
            this.mcToolButton4.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.mcToolButton4.DialogResult = System.Windows.Forms.DialogResult.None;
            this.mcToolButton4.Dock = System.Windows.Forms.DockStyle.Left;
            this.mcToolButton4.Location = new System.Drawing.Point(425, 3);
            this.mcToolButton4.Name = "mcToolButton4";
            this.mcToolButton4.Size = new System.Drawing.Size(16, 20);
            this.mcToolButton4.TabIndex = 13;
            this.mcToolButton4.ToolTipText = "";
            // 
            // mcToolButton6
            // 
            this.mcToolButton6.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Separator;
            this.mcToolButton6.DialogResult = System.Windows.Forms.DialogResult.None;
            this.mcToolButton6.Dock = System.Windows.Forms.DockStyle.Left;
            this.mcToolButton6.Location = new System.Drawing.Point(507, 3);
            this.mcToolButton6.Name = "mcToolButton6";
            this.mcToolButton6.Size = new System.Drawing.Size(14, 20);
            this.mcToolButton6.TabIndex = 21;
            this.mcToolButton6.ToolTipText = "";
            // 
            // tbProperty
            // 
            this.tbProperty.ButtonStyle = Nistec.WinForms.ToolButtonStyle.Button;
            this.tbProperty.DialogResult = System.Windows.Forms.DialogResult.None;
            this.tbProperty.Dock = System.Windows.Forms.DockStyle.Left;
            this.tbProperty.Image = global::Nistec.Messaging.UI.Properties.Resources.PropertiesHS;
            this.tbProperty.Location = new System.Drawing.Point(300, 3);
            this.tbProperty.Name = "tbProperty";
            this.tbProperty.Size = new System.Drawing.Size(22, 20);
            this.tbProperty.TabIndex = 22;
            this.tbProperty.ToolTipText = "";
            // 
            // QueueManagmentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CaptionVisible = true;
            this.ClientSize = new System.Drawing.Size(853, 549);
            this.ControlLayout = Nistec.WinForms.ControlLayout.Visual;
            this.Controls.Add(this.mcManagment);
            this.Name = "QueueManagmentForm";
            this.Text = "Nistec Service Managment";
            this.Controls.SetChildIndex(this.mcManagment, 0);
            this.mcManagment.ToolBar.ResumeLayout(false);
            this.pageDetails.ResumeLayout(false);
            this.pgItems.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridItems)).EndInit();
            this.pgSource.ResumeLayout(false);
            this.pgSource.PerformLayout();
            this.pgImage.ResumeLayout(false);
            this.pgImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgSource)).EndInit();
            this.pgClass.ResumeLayout(false);
            this.pgBrowser.ResumeLayout(false);
            this.pgBrowser.PerformLayout();
            this.pgChart.ResumeLayout(false);
            this.pgChart.PerformLayout();
            this.pnlChart.ResumeLayout(false);
            this.pnlUsage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Nistec.Wizards.McManagment mcManagment;
        private Nistec.WinForms.McToolButton tbCommand;
        private Nistec.WinForms.McToolButton tbHelp;
        private Nistec.WinForms.McToolButton tbRestart;
        private Nistec.WinForms.McToolButton tbPause;
        private Nistec.WinForms.McToolButton tbStop;
        private Nistec.WinForms.McToolButton tbStart;
        private Nistec.WinForms.McToolButton tbRefresh;
        private Nistec.WinForms.McToolButton tbForward;
        private Nistec.WinForms.McToolButton tbBack;
        private Nistec.WinForms.McTabPage pageDetails;
        private Nistec.WinForms.McListView listView;
        private System.Windows.Forms.ColumnHeader colDisplayName;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colName;
        private Nistec.WinForms.McTabPage pgItems;
        private Nistec.WinForms.McTabPage pgSource;
        private Nistec.WinForms.McTabPage pgImage;
        private Nistec.WinForms.McTabPage pgChart;
        private Nistec.WinForms.McTabPage pgClass;
        private Nistec.WinForms.McTabPage pgBrowser;

        private System.Windows.Forms.WebBrowser ctlBrowser;
        private System.Windows.Forms.Timer timer1;
        private Nistec.GridView.Grid gridItems;
        private Nistec.GridView.VGrid vgrid;
        private Nistec.WinForms.McLabel ctlLabel2;
        private Nistec.WinForms.McToolButton tbActions;
        private Nistec.WinForms.PopUpItem pServices;
        private Nistec.WinForms.PopUpItem pItems;
        private Nistec.WinForms.PopUpItem pData;
        private Nistec.WinForms.McToolButton tbUsage;
        private Nistec.WinForms.McToolButton sp7;
        private Nistec.WinForms.McToolButton sp5;
        private Nistec.WinForms.McToolButton sp3;
        private Nistec.WinForms.McToolButton sp2;
        private Nistec.WinForms.McToolButton sp1;
        private Nistec.WinForms.McToolButton tbItems;
        internal System.Windows.Forms.RichTextBox txtBody;
        private System.Windows.Forms.TextBox txtHeader;
        private System.Windows.Forms.TextBox txtImageHeader;
        internal System.Windows.Forms.PictureBox imgSource;
        private Nistec.WinForms.McToolButton tbAddItem;
        private System.Windows.Forms.TextBox txtHeaderBrowser;
        private Nistec.WinForms.McToolButton tbDelete;
        private System.Windows.Forms.ImageList imageList1;
        private Nistec.WinForms.McToolButton sp4;
        private Nistec.WinForms.McToolButton tbStatistic;
        private Nistec.Charts.McPieChart ctlPieChart1;
        private Nistec.WinForms.McLabel lblUsage;
        private Nistec.WinForms.McLabel lblUsageHistory;
        private Nistec.Charts.McUsageHistory mcUsageHistory1;
        private Nistec.Charts.McUsage mcUsage1;
        private Nistec.WinForms.McPanel pnlChart;
        private Nistec.WinForms.McPanel pnlUsage;
        private Nistec.WinForms.McToolButton sp6;
        private Nistec.WinForms.McToolButton tbLoadXml;
        private Nistec.WinForms.McToolButton tbSaveXml;
        private Nistec.WinForms.McToolButton mcToolButton1;
        private Nistec.WinForms.McToolButton mcToolButton2;
        private Nistec.WinForms.McToolButton mcToolButton3;
        private Nistec.WinForms.McToolButton mcToolButton5;
        private Nistec.WinForms.McToolButton mcToolButton4;
        private Nistec.WinForms.McToolButton mcToolButton6;
        private Nistec.WinForms.McToolButton tbProperty;


    }
}