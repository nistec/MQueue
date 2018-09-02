using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MControl.Util;
using MControl.Data;
using MControl.Data.SqlClient;
using MControl.Charts;
using MControl.GridView;
using MControl.WinForms;

namespace MControl.Messaging.UI
{
    public partial class MessagingMonitor : McForm
    {
        public MessagingMonitor()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Config();
            LoadDal();
            InitChart();
            this.timer1.Interval = interval;
            this.timer1.Enabled = true;
        }

        #region members

        private bool useChannels = false;

        private int maxUsage = 1000;
        private int interval = 1000;

        private string QueueName1 = "";
        private string QueueName2 = "";
        private string QueueName3 = "";
        private string QueueName4 = "";

        private string ChannelName1 = "";
        private string ChannelName2 = "";
        private string ChannelName3 = "";
        private string ChannelName4 = "";

        private int QueueItem1Count;
        private int QueueItem2Count;
        private int QueueItem3Count;
        private int QueueItem4Count;
        private int QueueItemTotalCount;

        private int ChannelItems1Count;
        private int ChannelItems2Count;
        private int ChannelItems3Count;
        private int ChannelItems4Count;
        private int ChannelItemsTotalCount;

        private DataTable itemsSchema;

        private DataTable itemsSource;
        private DataTable summarizeSource;
        private DataTable valuesSource;
        private DataTable channelsSource;

        private bool refreshQueueItems = true;
        private bool refreshQueueSummarize = true;
        private int tickInterval = 5;

        private bool doRefresh = false;
        private bool isActivated = true;
        private int intervalCount=0;
        private bool autoScale = false;
        private bool initilaized = false;
        private bool sqlOK = false;
        private bool useDataReader = true;
        private int fastFirstRows = 100;

        private int meterScaleInterval=100;
        private int meterScaleMax=1000;
        private int ledScaleCount=40;
        private int ledScaleMax=5000;


        #endregion

        #region properties
 
        public bool PropInitilaized
        {
            get 
            {
                if (string.IsNullOrEmpty(sqlItems) || string.IsNullOrEmpty(sqlBySender) || string.IsNullOrEmpty(sqlValues))
                    return false;
                return initilaized; 
            }
        }

        public bool PropAutoScale
        {
            get { return autoScale; }
            set { autoScale = value; }
        }

        public bool PropUseChannels
        {
            get { return useChannels; }
            set { useChannels = value; }
        }

        public bool PropUseDataReader
        {
            get { return useDataReader; }
            set { useDataReader = value; }
        }

        public string PropConnectionString
        {
            get { return cnn; }
            set { cnn = value; }
        }

        public string PropItemsSource
        {
            get { return sqlItems; }
            set { sqlItems = value; }
        }
        public string PropItemsBySenderSource
        {
            get { return sqlBySender; }
            set { sqlBySender = value; }
        }
        public string PropItemsSummarizeSource
        {
            get { return sqlValues; }
            set { sqlValues = value; }
        }
        public string PropChannelsSource
        {
            get { return sqlChannels; }
            set { sqlChannels = value; }
        }

        public int PropInterval //= 1000
        {
            get { return interval; }
            set { if (value >= 1000) { interval = value; } }
        }

        public int PropMaxUsage //= 1000
        {
            get { return maxUsage; }
            set 
            {
                if ((value >= 1) && maxUsage != value)
                {
                    maxUsage = value;
                    OnUsageChanges();
                }
            }
      }
        public int PropTickInterval //= 50
        {
            get { return tickInterval; }
            set
            {
                if ((value >= 2) && tickInterval != value)
                {
                    tickInterval = value;
                }
            }
        }
       
        public int PropFastFirstRows //= 100
        {
            get { return fastFirstRows; }
            set
            {
                if ((value >= 10) && fastFirstRows != value)
                {
                    fastFirstRows = value;
                }
            }
        }
        public string PropQueueName1
        {
            get { return QueueName1; }
            set { QueueName1 = value; lblMeter1.Text = value; lblUsage1.Text = value; }
        }
        public string PropQueueName2
        {
            get { return QueueName2; }
            set { QueueName2 = value; lblMeter2.Text = value; lblUsage2.Text = value; }
        }
        public string PropQueueName3
        {
            get { return QueueName3; }
            set { QueueName3 = value; lblMeter3.Text = value; lblUsage3.Text = value; }
        }
        public string PropQueueName4
        {
            get { return QueueName4; }
            set { QueueName4 = value; lblMeter4.Text = value; lblUsage4.Text = value; }
        }

        public string PropChannelName1
        {
            get { return ChannelName1; }
            set { ChannelName1 = value; lblChannel1.Text = value; }
        }
        public string PropChannelName2
        {
            get { return ChannelName2; }
            set { ChannelName2 = value; lblChannel2.Text = value; }
        }
        public string PropChannelName3
        {
            get { return ChannelName3; }
            set { ChannelName3 = value; lblChannel3.Text = value; }
        }
        public string PropChannelName4
        {
            get { return ChannelName4; }
            set { ChannelName4 = value; lblChannel4.Text = value; }
        }

        public int PropMeterScaleInterval
        {
            get { return meterScaleInterval; }
            set 
            {
                if (meterScaleInterval != value)
                {
                    meterScaleInterval = value;
                    OnMeterChanges();
                }
            }
        }

        public int PropMeterScaleMax
        {
            get { return meterScaleMax; }
            set 
            {
                if (meterScaleMax != value)
                {
                    meterScaleMax = value;
                    OnMeterChanges();
                }
            }
        }

        public int PropLedScaleCount
        {
            get { return ledScaleCount; }
            set 
            {
                if (ledScaleCount != value)
                {
                    ledScaleCount = value;
                    OnLedChanges();
                }
            }
       }

        public int PropLedScaleMax
        {
            get { return ledScaleMax; }
            set 
            {
                if (ledScaleMax != value)
                {
                    ledScaleMax = value;
                    OnLedChanges();
                }
            }
      }

        

        #endregion

        #region Async Dal

        string cnn ="";// "Data Source=IL-TLV-NTRUJMAN; Initial Catalog=FrameworkDB; Integrated Security=SSPI; Connection Timeout=30";
        MControl.Data.IDalBase DalBase;

        private void LoadDal()
        {
            try
            {
                DalBase = new MControl.Data.Common.DalProvider(MControl.Data.DBProvider.SqlServer, cnn);
                DalBase.Init(cnn, true);
                sqlOK = true;
            }
            catch
            {
                sqlOK = false;
            }
        }

        private string sqlItems = "vw_QueueItemsMonitor";
        private string sqlBySender = "vw_QueueItemsAllBySender";
        private string sqlValues = "vw_QueueItemsSummarize";
        private string sqlChannels = "vw_ChannelsSummarize";

        private Data.SqlClient.DalAsync dalAsync;

        void Async_AsyncComplited(object sender, EventArgs e)
        {
            itemsSource = dalAsync.AsyncResult_DataTable;
            itemsSource.TableName = "QueueItems";
            SetGridItems();
            this.statusStrip.Text= "Total count:" + this.gridItems.RowCount;
        }

        private void AsyncDalDispose()
        {
            if (dalAsync != null)
            {
                dalAsync.AsyncCompleted -= new EventHandler(Async_AsyncComplited);
            }
        }

        protected void AsyncHandleCallback(IAsyncResult result)
        {
            IDataReader reader =null;
            try
            {
                if (useDataReader)
                {
                    reader = dalAsync.AsyncExecuteEnd(result);
                    if (reader == null)
                    {
                        return;
                    }
                    if (itemsSchema == null)
                    {
                        itemsSchema = MControl.Data.DataUtil.GetTableSchema(reader, "QueueItems");
                    }

                    //dalAsync.FillDataSourceSchema(reader, "QueueItems");
                    //this.gridItems.InvokeDataSource(null);

                    this.gridItems.InvokeDataSource(reader, itemsSchema.Clone() /*dalAsync.AsyncResult_DataTable*/, "QueueItems", fastFirstRows, 0);
                }
                else
                {
                    AsyncDataFill del = new AsyncDataFill(dalAsync.AsyncFillDataSource);
                    this.Invoke(del, dalAsync.AsyncExecuteEnd(result));
                }
            }
            catch (Exception ex)
            {
                if (useDataReader)
                {
                    if (reader != null && !reader.IsClosed)
                    {
                        reader.Close();
                    }
                    return;
                }
                if (IsHandleCreated)
                {
                    this.Invoke(new AsyncDisplayStatus(dalAsync.SetAsyncStatus), "Error: " + ex.Message, MControl.Data.StatusPriority.Error);
                }
            }
        }

        void gridItems_LoadDataSourceEnd(object sender, EventArgs e)
        {
            SetGridStatus( this.gridItems.RowCount);
        }

        private delegate void SetGridStatusCallBack(int count);
        private void SetGridStatus(int count)
        {
            if (this.statusStrip.InvokeRequired)
            {
                statusStrip.Invoke(new SetGridStatusCallBack(SetGridStatus), count);
            }
            else
            {
                this.statusStrip.Text = "Total Items: " + count.ToString();
            }
        }

        private void AsyncDalStart()
        {
            if (!initilaized || !sqlOK )
                return;
            try
            {
                if (dalAsync == null)
                {
                    dalAsync = new Data.SqlClient.DalAsync(DalBase);// DalAsync();
                    dalAsync.AsyncCompleted += new EventHandler(Async_AsyncComplited);
                }
                dalAsync.AsyncExecuteBegin(new AsyncCallback(AsyncHandleCallback), "SELECT * FROM " + sqlItems, null, 0, 0);
            }
            catch(Exception ex)
            {
                MsgBox.ShowError(ex.Message);
                initilaized = false;
            }
        }

 

        private void AsyncDalReStart()
        {
            if (!initilaized || !sqlOK)
                return;
            try
            {

                if (dalAsync != null)
                {
                    dalAsync.AsyncCompleted -= new EventHandler(Async_AsyncComplited);
                    dalAsync.Dispose();
                    dalAsync = null;
                }
                dalAsync = new Data.SqlClient.DalAsync(DalBase);// DalAsync();
                dalAsync.AsyncCompleted += new EventHandler(Async_AsyncComplited);

            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
                initilaized = false;
            }
        }

        #endregion

  
        #region override

        protected virtual void SetGridItems()
        {
            //itemsSource = dt;
            if (this.gridItems.DataSource == null)
            {
                this.gridItems.Init(itemsSource, "", "QueueItems");
            }
            else
            {
                this.gridItems.ReBinding(itemsSource);
            }
            refreshQueueItems = false;
        }

        protected virtual void SetGridSummarise()
        {
            //summarizeSource = dt;
            if (this.gridSummarize.DataSource == null)
            {
                this.gridSummarize.Init(summarizeSource, "", "QueueItemsBySender");
            }
            else
            {
                this.gridSummarize.ReBinding(summarizeSource);
            }
            refreshQueueSummarize = false;
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

        protected override void OnAsyncExecutingWorker(MControl.Threading.AsyncCallEventArgs e)
        {
            base.OnAsyncExecutingWorker(e);
            try
            {
                using (Dal cmd = new Dal(DalBase))
                {
                    if (refreshQueueSummarize)
                    {
                        summarizeSource = cmd.ExecuteDataTable("SELECT * FROM " + sqlBySender);
                    }
                    valuesSource = cmd.ExecuteDataTable("SELECT * FROM " + sqlValues);
                    if (useChannels)
                    {
                        channelsSource = cmd.ExecuteDataTable("SELECT * FROM " + sqlChannels);
                    }
                }
            }
            catch { }
            finally
            {
                //doRefresh = true;
            }

        }

        protected override void OnAsyncCompleted(MControl.Threading.AsyncCallEventArgs e)
        {
            base.OnAsyncCompleted(e);

            try
            {
                if (refreshQueueSummarize)
                {
                    SetGridSummarise();
                }
                FillQueueValues();
                FillChannelValues();
                FillControls();
            }
            catch(Exception ex) 
            {
                string s = ex.Message;
            }
            finally
            {
                base.AsyncDispose();
            }


        }

        #endregion

        #region private methods

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                intervalCount++;
                doRefresh = intervalCount >= tickInterval;
                if (refreshQueueItems)
                {
                    AsyncDalStart();
                    refreshQueueItems = false;
                }
                if (isActivated)
                {
                    if (doRefresh)
                    {
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

        private void FillControls()
        {

            if (autoScale)
            {
                int valInterval = meterScaleInterval;
                if (QueueItem1Count > 0)
                {
                    valInterval = QueueItem1Count / 10;
                    ctlMeter1.SetAutoScale(QueueItem1Count, Math.Max(meterScaleMax, QueueItem1Count), Math.Max(meterScaleInterval, valInterval));
                }
                if (QueueItem2Count > 0)
                {
                    valInterval = QueueItem2Count / 10;
                    ctlMeter2.SetAutoScale(QueueItem2Count, Math.Max(meterScaleMax, QueueItem2Count), Math.Max(meterScaleInterval, valInterval));
                }
                if (QueueItem3Count > 0)
                {
                    valInterval = QueueItem3Count / 10;
                    ctlMeter3.SetAutoScale(QueueItem3Count, Math.Max(meterScaleMax, QueueItem3Count), Math.Max(meterScaleInterval, valInterval));
                }
                if (QueueItem4Count > 0)
                {
                    valInterval = QueueItem4Count / 10;
                    ctlMeter4.SetAutoScale(QueueItem4Count, Math.Max(meterScaleMax, QueueItem4Count), Math.Max(meterScaleInterval, valInterval));
                }

                ctlLedAll.ScaleValue = QueueItemTotalCount;
            }
            else
            {
                this.ctlMeter1.ScaleValue = QueueItem1Count;
                this.ctlMeter2.ScaleValue = QueueItem2Count;
                this.ctlMeter3.ScaleValue = QueueItem3Count;
                this.ctlMeter4.ScaleValue = QueueItem4Count;

            }

            if (QueueItemTotalCount > maxUsage)
                OnUsageChanges();
            this.ctlUsage1.Value1 = QueueItem1Count;
            this.ctlUsage1.Value2 = QueueItemTotalCount;
            this.ctlUsage2.Value1 = QueueItem2Count;
            this.ctlUsage2.Value2 = QueueItemTotalCount;
            this.ctlUsage3.Value1 = QueueItem3Count;
            this.ctlUsage3.Value2 = QueueItemTotalCount;
            this.ctlUsage4.Value1 = QueueItem4Count;
            this.ctlUsage4.Value2 = QueueItemTotalCount;

            this.ctlUsageHistory1.AddValues(QueueItem1Count, QueueItemTotalCount);
            this.ctlUsageHistory2.AddValues(QueueItem2Count, QueueItemTotalCount);
            this.ctlUsageHistory3.AddValues(QueueItem3Count, QueueItemTotalCount);
            this.ctlUsageHistory4.AddValues(QueueItem4Count, QueueItemTotalCount);


            this.ctlLedAll.ScaleValue = QueueItemTotalCount;

            this.ctlLedChannel1.ScaleValue = ChannelItems1Count;
            this.ctlLedChannel2.ScaleValue = ChannelItems2Count;
            this.ctlLedChannel3.ScaleValue = ChannelItems3Count;
            this.ctlLedChannel4.ScaleValue = ChannelItems4Count;


            ctlPieChart1.Items[0].Weight = (double)QueueItem1Count;
            ctlPieChart1.Items[1].Weight = (double)QueueItem2Count;
            ctlPieChart1.Items[2].Weight = (double)QueueItem3Count;
            ctlPieChart1.Items[3].Weight = (double)QueueItem4Count;

            ctlPieChart1.Items[0].ToolTipText = QueueName1 + ":" + QueueItem1Count.ToString();
            ctlPieChart1.Items[1].ToolTipText = QueueName2 + ":" + QueueItem2Count.ToString();
            ctlPieChart1.Items[2].ToolTipText = QueueName3 + ":" + QueueItem3Count.ToString();
            ctlPieChart1.Items[3].ToolTipText = QueueName4 + ":" + QueueItem4Count.ToString();

            ctlPieChart1.Items[0].PanelText = QueueName1 + ":" + QueueItem1Count.ToString();
            ctlPieChart1.Items[1].PanelText = QueueName2 + ":" + QueueItem2Count.ToString();
            ctlPieChart1.Items[2].PanelText = QueueName3 + ":" + QueueItem3Count.ToString();
            ctlPieChart1.Items[3].PanelText = QueueName4 + ":" + QueueItem4Count.ToString();

             ctlPieChart1.AddChartDescription();


            this.lblUsageValue1.Text = QueueItem1Count.ToString();
            this.lblUsageValue2.Text = QueueItem2Count.ToString();
            this.lblUsageValue3.Text = QueueItem3Count.ToString();
            this.lblUsageValue4.Text = QueueItem4Count.ToString();
            this.lblMeterValue1.Text = QueueItem1Count.ToString();
            this.lblMeterValue2.Text = QueueItem2Count.ToString();
            this.lblMeterValue3.Text = QueueItem3Count.ToString();
            this.lblMeterValue4.Text = QueueItem4Count.ToString();
            this.lblLedValue1.Text = ChannelItems1Count.ToString();
            this.lblLedValue2.Text = ChannelItems2Count.ToString();
            this.lblLedValue3.Text = ChannelItems3Count.ToString();
            this.lblLedValue4.Text = ChannelItems4Count.ToString();
            this.lblTotalQueue.Text = QueueItemTotalCount.ToString();
        }

        private void FillMeterControls()
        {

            if (autoScale)
            {
                int valInterval = meterScaleInterval;
                if (QueueItem1Count > 0)
                {
                    valInterval = QueueItem1Count / 10;
                    ctlMeter1.SetAutoScale(QueueItem1Count, Math.Max(meterScaleMax, QueueItem1Count), Math.Max(meterScaleInterval, valInterval));
                }
                if (QueueItem2Count > 0)
                {
                    valInterval = QueueItem2Count / 10;
                    ctlMeter2.SetAutoScale(QueueItem2Count, Math.Max(meterScaleMax, QueueItem2Count), Math.Max(meterScaleInterval, valInterval));
                }
                if (QueueItem3Count > 0)
                {
                    valInterval = QueueItem3Count / 10;
                    ctlMeter3.SetAutoScale(QueueItem3Count, Math.Max(meterScaleMax, QueueItem3Count), Math.Max(meterScaleInterval, valInterval));
                }
                if (QueueItem4Count > 0)
                {
                    valInterval = QueueItem4Count / 10;
                    ctlMeter4.SetAutoScale(QueueItem4Count, Math.Max(meterScaleMax, QueueItem4Count), Math.Max(meterScaleInterval, valInterval));
                }

                ctlLedAll.ScaleValue = QueueItemTotalCount;
            }
            else
            {
                this.ctlMeter1.ScaleValue = QueueItem1Count;
                this.ctlMeter2.ScaleValue = QueueItem2Count;
                this.ctlMeter3.ScaleValue = QueueItem3Count;
                this.ctlMeter4.ScaleValue = QueueItem4Count;

            }

            this.lblMeterValue1.Text = QueueItem1Count.ToString();
            this.lblMeterValue2.Text = QueueItem2Count.ToString();
            this.lblMeterValue3.Text = QueueItem3Count.ToString();
            this.lblMeterValue4.Text = QueueItem4Count.ToString();
            this.lblTotalQueue.Text = QueueItemTotalCount.ToString();
        }

        private void FillUsageControls()
        {

            if (QueueItemTotalCount > maxUsage)
                OnUsageChanges();
            
            this.ctlUsage1.Value1 = QueueItem1Count;
            this.ctlUsage1.Value2 = QueueItemTotalCount;
            this.ctlUsage2.Value1 = QueueItem2Count;
            this.ctlUsage2.Value2 = QueueItemTotalCount;
            this.ctlUsage3.Value1 = QueueItem3Count;
            this.ctlUsage3.Value2 = QueueItemTotalCount;
            this.ctlUsage4.Value1 = QueueItem4Count;
            this.ctlUsage4.Value2 = QueueItemTotalCount;

            this.ctlUsageHistory1.AddValues(QueueItem1Count, QueueItemTotalCount);
            this.ctlUsageHistory2.AddValues(QueueItem2Count, QueueItemTotalCount);
            this.ctlUsageHistory3.AddValues(QueueItem3Count, QueueItemTotalCount);
            this.ctlUsageHistory4.AddValues(QueueItem4Count, QueueItemTotalCount);


            this.lblUsageValue1.Text = QueueItem1Count.ToString();
            this.lblUsageValue2.Text = QueueItem2Count.ToString();
            this.lblUsageValue3.Text = QueueItem3Count.ToString();
            this.lblUsageValue4.Text = QueueItem4Count.ToString();
      
        }

        private void InitChart()
        {
            if (useChannels)
            {
                this.useChannels = !string.IsNullOrEmpty(sqlChannels);
            }

            OnUsageChanges();
            OnLedChanges();
            OnMeterChanges();
            ctlPieChart1.Items.Clear();
            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Blue, QueueName1, "Queue: " + QueueName1, 0));
            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Gold, QueueName2, "Queue: " + QueueName2, 0));
            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Green, QueueName3, "Queue: " + QueueName3, 0));
            ctlPieChart1.Items.Add(new PieChartItem(0, Color.Red, QueueName4, "Queue: " + QueueName4, 0));

            this.ctlPieChart1.Padding = new System.Windows.Forms.Padding(60, 0, 0, 0);
            this.ctlPieChart1.ItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceTransparency = 0.75F;
            this.ctlPieChart1.FocusedItemStyle.SurfaceBrightness = 0.3F;
            this.ctlPieChart1.AddChartDescription();
            this.ctlPieChart1.Leaning = (float)(40 * Math.PI / 180);
            this.ctlPieChart1.Depth = 50;
            this.ctlPieChart1.Radius = 240F;

            initilaized = true;
        }

        private void OnLedChanges()
        {
            this.SuspendLayout();

            this.ctlLedChannel1.ScaleMax = ledScaleMax;
            this.ctlLedChannel2.ScaleMax = ledScaleMax;
            this.ctlLedChannel3.ScaleMax = ledScaleMax;
            this.ctlLedChannel4.ScaleMax = ledScaleMax;
            
            this.ctlLedChannel1.ScaleLedCount = ledScaleCount;
            this.ctlLedChannel2.ScaleLedCount = ledScaleCount;
            this.ctlLedChannel3.ScaleLedCount = ledScaleCount;
            this.ctlLedChannel4.ScaleLedCount = ledScaleCount;

            int ledRed = Types.ToInt(ledScaleMax * 0.9, ledScaleMax);

            this.ctlLedChannel1.ScaleLedRed = ledRed;
            this.ctlLedChannel2.ScaleLedRed = ledRed;
            this.ctlLedChannel3.ScaleLedRed = ledRed;
            this.ctlLedChannel4.ScaleLedRed = ledRed;

            int ledYellow = Types.ToInt(ledScaleMax * 0.7, ledScaleMax);

            this.ctlLedChannel1.ScaleLedYellow = ledYellow;
            this.ctlLedChannel2.ScaleLedYellow = ledYellow;
            this.ctlLedChannel3.ScaleLedYellow = ledYellow;
            this.ctlLedChannel4.ScaleLedYellow = ledYellow;
            this.ResumeLayout(false);

        }

        private void OnMeterChanges()
        {

            int max = Math.Max(meterScaleMax, (int)((float)QueueItem1Count * 1.2F));

            this.SuspendLayout();

            this.ctlMeter1.ScaleMax = Math.Max(meterScaleMax, (int)((float)QueueItem1Count * 1.2F));
            this.ctlMeter2.ScaleMax = Math.Max(meterScaleMax, (int)((float)QueueItem2Count * 1.2F));
            this.ctlMeter3.ScaleMax = Math.Max(meterScaleMax, (int)((float)QueueItem3Count * 1.2F));
            this.ctlMeter4.ScaleMax = Math.Max(meterScaleMax, (int)((float)QueueItem4Count * 1.2F));

            this.ctlMeter1.ScaleInterval = meterScaleInterval;
            this.ctlMeter2.ScaleInterval = meterScaleInterval;
            this.ctlMeter3.ScaleInterval = meterScaleInterval;
            this.ctlMeter4.ScaleInterval = meterScaleInterval;

            int meterRed = Types.ToInt(meterScaleMax * 0.9, meterScaleMax);

            this.ctlMeter1.ScaleLedRed = meterRed;
            this.ctlMeter2.ScaleLedRed = meterRed;
            this.ctlMeter3.ScaleLedRed = meterRed;
            this.ctlMeter4.ScaleLedRed = meterRed;

            int meterYellow = Types.ToInt(meterScaleMax * 0.7, meterScaleMax);

            this.ctlMeter1.ScaleLedYellow = meterYellow;
            this.ctlMeter2.ScaleLedYellow = meterYellow;
            this.ctlMeter3.ScaleLedYellow = meterYellow;
            this.ctlMeter4.ScaleLedYellow = meterYellow;
 
            int total = meterScaleMax * 4;
            int ledRed = Types.ToInt(total * 0.9, total);
            int ledYellow = Types.ToInt(total * 0.7, total);

            this.ctlLedAll.ScaleMax = total;
            this.ctlLedAll.ScaleLedRed = ledRed;
            this.ctlLedAll.ScaleLedYellow = ledYellow;

            this.ResumeLayout(false);

        }
        private void OnUsageChanges()
        {
            int max = Math.Max(maxUsage, (int)((float)QueueItemTotalCount*1.2F));
            this.SuspendLayout();
            this.ctlUsage1.Maximum = max;
            this.ctlUsage2.Maximum = max;
            this.ctlUsage3.Maximum = max;
            this.ctlUsage4.Maximum = max;

            this.ctlUsageHistory1.Maximum = max;
            this.ctlUsageHistory2.Maximum = max;
            this.ctlUsageHistory3.Maximum = max;
            this.ctlUsageHistory4.Maximum = max;
            this.ResumeLayout(false);
        }

        private void FillQueueValues()
        {
            if (valuesSource == null || valuesSource.Rows.Count == 0)
            {
                QueueItem1Count = 0;
                QueueItem2Count = 0;
                QueueItem3Count = 0;
                QueueItem4Count = 0;
            }
            else
            {
                foreach (DataRow dr in valuesSource.Rows)
                {

                    if (dr["QueueName"].ToString().Equals(QueueName1))
                        QueueItem1Count = Types.ToInt(dr["Total"], 0);
                    else if (dr["QueueName"].ToString().Equals(QueueName2))
                        QueueItem2Count = Types.ToInt(dr["Total"], 0);
                    else if (dr["QueueName"].ToString().Equals(QueueName3))
                        QueueItem3Count = Types.ToInt(dr["Total"], 0);
                    else if (dr["QueueName"].ToString().Equals(QueueName4))
                        QueueItem4Count = Types.ToInt(dr["Total"], 0);
                }
            }
            //test
            //QueueItem1Count = 1200;
            //QueueItem2Count = 200;
            //QueueItem3Count = 700;
            //QueueItem4Count = 500;

            QueueItemTotalCount = QueueItem1Count + QueueItem2Count + QueueItem3Count + QueueItem4Count;
        }

        private void FillChannelValues()
        {
            if (channelsSource == null || channelsSource.Rows.Count == 0)
            {
                    ChannelItems1Count = 0;
                    ChannelItems2Count = 0;
                    ChannelItems3Count = 0;
                    ChannelItems4Count = 0;
            }
            else
            {
                foreach (DataRow dr in channelsSource.Rows)
                {

                    if (dr["ChannelName"].ToString().Equals(ChannelName1))
                        ChannelItems1Count = Types.ToInt(dr["Total"], 0);
                    else if (dr["ChannelName"].ToString().Equals(ChannelName2))
                        ChannelItems2Count = Types.ToInt(dr["Total"], 0);
                    else if (dr["ChannelName"].ToString().Equals(ChannelName3))
                        ChannelItems3Count = Types.ToInt(dr["Total"], 0);
                    else if (dr["ChannelName"].ToString().Equals(ChannelName4))
                        ChannelItems4Count = Types.ToInt(dr["Total"], 0);
                }
            }
            //test
            //ChannelItems1Count = 3200;
            //ChannelItems2Count = 5200;
            //ChannelItems3Count = 200;
            //ChannelItems4Count = 1200;

            ChannelItemsTotalCount = ChannelItems1Count + ChannelItems2Count + ChannelItems3Count + ChannelItems4Count;

        }
        #endregion

        #region Config


        private void Config()
        {
            GridField[] fields = new GridField[24];
            fields[0] = new GridField("QueueName1", QueueName1);
            fields[1] = new GridField("QueueName2", QueueName2);
            fields[2] = new GridField("QueueName3", QueueName3);
            fields[3] = new GridField("QueueName4", QueueName4);

            fields[4] = new GridField("ChannelName1", ChannelName1);
            fields[5] = new GridField("ChannelName2", ChannelName2);
            fields[6] = new GridField("ChannelName3", ChannelName3);
            fields[7] = new GridField("ChannelName4", ChannelName4);

            fields[8] = new GridField("UseChannels", useChannels);
            fields[9] = new GridField("MaxUsage", maxUsage);
            fields[10] = new GridField("ConnectionString", cnn);
            fields[11] = new GridField("Interval", interval);
            fields[12] = new GridField("MeterScaleInterval", meterScaleInterval);
            fields[13] = new GridField("MeterScaleMax", meterScaleMax);
            fields[14] = new GridField("LedScaleCount", ledScaleCount);
            fields[15] = new GridField("LedScaleMax", ledScaleMax);
            fields[16] = new GridField("AutoScale", autoScale);
            fields[17] = new GridField("ItemsSource", sqlItems);
            fields[18] = new GridField("ItemsBySenderSource", sqlBySender);
            fields[19] = new GridField("ItemsSummarizeSource", sqlValues);
            fields[20] = new GridField("ChannelsSource", sqlChannels);
            fields[21] = new GridField("TickInterval", tickInterval);
            fields[22] = new GridField("UseDataReader", useDataReader);
            fields[23] = new GridField("FastFirstRows", fastFirstRows);

 
            fields[0].Description = "Queue Name #1";
            fields[1].Description = "Queue Name #2";
            fields[2].Description = "Queue Name #3";
            fields[3].Description = "Queue Name #4";

            fields[4].Description = "Channel Name #1";
            fields[5].Description = "Channel Name #2";
            fields[6].Description = "Channel Name #3";
            fields[7].Description = "Channel Name #4";

            fields[8].Description = "Use Channels";
            fields[9].Description = "Max Usage value for each Queue";
            fields[10].Description = "Connection String to database";
            fields[11].Description = "Interval in millisecondes for Refresh";
            fields[12].Description = "Meter Scale Interval ";
            fields[13].Description = "Meter Scale Max Value";
            fields[14].Description = "Led Scale items Count";
            fields[15].Description = "Led Scale Max value";
            fields[16].Description = "Auto Scale properies";

            fields[17].Description ="Items Source";
            fields[18].Description ="Items By Sender Source";
            fields[19].Description ="Items Summarize Source";
            fields[20].Description = "Channels Source";
            fields[21].Description = "Tick Interval Refreshing";
            fields[22].Description = "Use DataReader to fetch Queue items";
            fields[23].Description = "Number of First row to fetch when use data reader";

            VGridDlg dlg = new VGridDlg();
            dlg.VGrid.SetDataBinding(fields, "Monitor");
            dlg.Width = 400;
            DialogResult dr = dlg.ShowDialog();

            QueueName1 = fields[0].Text;
            QueueName2 = fields[1].Text;
            QueueName3 = fields[2].Text;
            QueueName4 = fields[3].Text;
            ChannelName1 = fields[4].Text;
            ChannelName2 = fields[5].Text;
            ChannelName3 = fields[6].Text;
            ChannelName4 = fields[7].Text;
            useChannels = Types.ToBool(fields[8].Text, false);
            maxUsage = Types.ToInt(fields[9].Value, maxUsage);
            cnn = fields[10].Text;
            interval = Types.ToInt(fields[11].Value, interval);
            meterScaleInterval = Types.ToInt(fields[12].Value, meterScaleInterval);
            meterScaleMax = Types.ToInt(fields[13].Value, meterScaleMax);
            ledScaleCount = Types.ToInt(fields[14].Value, ledScaleCount);
            ledScaleMax = Types.ToInt(fields[15].Value, ledScaleMax);
            autoScale = Types.ToBool(fields[16].Text, true);
            sqlItems = fields[17].Text;
            sqlBySender = fields[18].Text;
            sqlValues = fields[19].Text;
            sqlChannels = fields[20].Text;
            tickInterval = Types.ToInt(fields[21].Value, meterScaleMax);
            useDataReader = Types.ToBool(fields[22].Text, true);
            fastFirstRows = Types.ToInt(fields[22].Value, fastFirstRows);

        }

        #endregion

        private void ctlToolBar_ButtonClick(object sender, MControl.WinForms.ToolButtonClickEventArgs e)
        {
            switch(e.Button.Name)
            {

                case "tbRefresh":
                    if (tabControl.SelectedIndex == 0)
                    {
                        refreshQueueItems = true;
                    }
                    if (tabControl.SelectedIndex == 1)
                    {
                        refreshQueueSummarize = true;
                    }

                    break;
                case "tbClose":
                    this.Close();
                    break;
                 case "tbConfig":
                    Config();
                    LoadDal();
                    AsyncDalReStart();
                    InitChart();
                    break;
           }
        }

        private void tbOffset_SelectedItemClick(object sender, MControl.WinForms.SelectedPopUpItemEvent e)
        {
            float offset = Types.ToFloat(e.Item.Text, 0F);
            for (int i = 0; i < ctlPieChart1.Items.Count; i++)
            {
                ctlPieChart1.Items[i].Offset = offset;
            }

        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.tbOffset.Enabled = this.tabControl.SelectedTab == pgChart;
            this.tbRefresh.Enabled = this.tabControl.SelectedIndex <= 1;
        }
    }
}