using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Nistec.WinForms;
using Nistec;
using System.Data;
using Nistec.Data;
using Nistec.Win;
using Nistec.Messaging.Remote;
using Nistec.Messaging.Server;


namespace Nistec.Messaging.UI
{

 

	/// <summary>
	/// Summary description for SmsSettings.
	/// </summary>
	public class AddItemDlg : Nistec.WinForms.McForm
    {
        private McLabel ctlLabel1;
        private McTextBox txtKey;
        private McButton btnOk;
        private McButton btnCancel;
        private McLabel ctlLabel2;
        private McComboBox cbProvider;
        private McLabel mcLabel2;
        private McMultiBox txtConnection;
        private McLabel mcLabel1;
        private McComboBox cbCoverMode;
        private McCheckBox isTrans;
        private McSpinEdit numMaxRetry;
        private McLabel mcLabel3;
		private System.ComponentModel.IContainer components=null;

        public AddItemDlg()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.ctlLabel1 = new Nistec.WinForms.McLabel();
            this.txtKey = new Nistec.WinForms.McTextBox();
            this.btnOk = new Nistec.WinForms.McButton();
            this.btnCancel = new Nistec.WinForms.McButton();
            this.ctlLabel2 = new Nistec.WinForms.McLabel();
            this.cbProvider = new Nistec.WinForms.McComboBox();
            this.mcLabel2 = new Nistec.WinForms.McLabel();
            this.txtConnection = new Nistec.WinForms.McMultiBox();
            this.mcLabel1 = new Nistec.WinForms.McLabel();
            this.cbCoverMode = new Nistec.WinForms.McComboBox();
            this.isTrans = new Nistec.WinForms.McCheckBox();
            this.numMaxRetry = new Nistec.WinForms.McSpinEdit();
            this.mcLabel3 = new Nistec.WinForms.McLabel();
            this.SuspendLayout();
            // 
            // StyleGuideBase
            // 
            this.StyleGuideBase.AlternatingColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(225)))), ((int)(((byte)(239)))));
            this.StyleGuideBase.BorderColor = System.Drawing.Color.SteelBlue;
            this.StyleGuideBase.BorderHotColor = System.Drawing.Color.Blue;
            this.StyleGuideBase.CaptionColor = System.Drawing.Color.SteelBlue;
            this.StyleGuideBase.ColorBrush1 = System.Drawing.Color.LightSteelBlue;
            this.StyleGuideBase.ColorBrush2 = System.Drawing.Color.AliceBlue;
            this.StyleGuideBase.ColorBrushLower = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(174)))), ((int)(((byte)(237)))));
            this.StyleGuideBase.ColorBrushUpper = System.Drawing.Color.AliceBlue;
            this.StyleGuideBase.BackgroundColor = System.Drawing.Color.AliceBlue;
            this.StyleGuideBase.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.StyleGuideBase.FormColor = System.Drawing.Color.AliceBlue;
            this.StyleGuideBase.StylePlan = Nistec.WinForms.Styles.SteelBlue;
            // 
            // ctlLabel1
            // 
            this.ctlLabel1.BackColor = System.Drawing.Color.AliceBlue;
            this.ctlLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ctlLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.ctlLabel1.ForeColor = System.Drawing.Color.Black;
            this.ctlLabel1.Location = new System.Drawing.Point(17, 49);
            this.ctlLabel1.Name = "ctlLabel1";
            this.ctlLabel1.Size = new System.Drawing.Size(72, 13);
            this.ctlLabel1.Text = "Queue Name";
            this.ctlLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtKey
            // 
            this.txtKey.BackColor = System.Drawing.Color.White;
            this.txtKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txtKey.ForeColor = System.Drawing.Color.Black;
            this.txtKey.Location = new System.Drawing.Point(17, 64);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(130, 20);
            this.txtKey.StylePainter = this.StyleGuideBase;
            this.txtKey.TabIndex = 113;
            // 
            // btnOk
            // 
            this.btnOk.ControlLayout = Nistec.WinForms.ControlLayout.VistaLayout;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnOk.Location = new System.Drawing.Point(201, 53);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(84, 25);
            this.btnOk.StylePainter = this.StyleGuideBase;
            this.btnOk.TabIndex = 120;
            this.btnOk.Text = "Ok";
            this.btnOk.ToolTipText = "Ok";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.ControlLayout = Nistec.WinForms.ControlLayout.VistaLayout;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnCancel.Location = new System.Drawing.Point(201, 85);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 25);
            this.btnCancel.StylePainter = this.StyleGuideBase;
            this.btnCancel.TabIndex = 119;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.ToolTipText = "Check Credit";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ctlLabel2
            // 
            this.ctlLabel2.BackColor = System.Drawing.Color.AliceBlue;
            this.ctlLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ctlLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.ctlLabel2.ForeColor = System.Drawing.Color.Black;
            this.ctlLabel2.Location = new System.Drawing.Point(17, 85);
            this.ctlLabel2.Name = "ctlLabel2";
            this.ctlLabel2.Size = new System.Drawing.Size(104, 13);
            this.ctlLabel2.Text = "Queue Provider";
            this.ctlLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbProvider
            // 
            this.cbProvider.BackColor = System.Drawing.Color.White;
            this.cbProvider.ButtonToolTip = "";
            this.cbProvider.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.cbProvider.ForeColor = System.Drawing.Color.Black;
            this.cbProvider.IntegralHeight = false;
            this.cbProvider.Location = new System.Drawing.Point(17, 101);
            this.cbProvider.Name = "cbProvider";
            this.cbProvider.Size = new System.Drawing.Size(130, 20);
            this.cbProvider.StylePainter = this.StyleGuideBase;
            this.cbProvider.TabIndex = 115;
            // 
            // mcLabel2
            // 
            this.mcLabel2.BackColor = System.Drawing.Color.AliceBlue;
            this.mcLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mcLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.mcLabel2.ForeColor = System.Drawing.Color.Black;
            this.mcLabel2.Location = new System.Drawing.Point(17, 204);
            this.mcLabel2.Name = "mcLabel2";
            this.mcLabel2.Size = new System.Drawing.Size(164, 13);
            this.mcLabel2.Text = "Connection String";
            this.mcLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtConnection
            // 
            this.txtConnection.BackColor = System.Drawing.Color.White;
            this.txtConnection.ButtonToolTip = "";
            this.txtConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.txtConnection.ForeColor = System.Drawing.Color.Black;
            this.txtConnection.Location = new System.Drawing.Point(17, 219);
            this.txtConnection.MultiType = Nistec.WinForms.MultiType.Custom;
            this.txtConnection.Name = "txtConnection";
            this.txtConnection.Size = new System.Drawing.Size(268, 20);
            this.txtConnection.StylePainter = this.StyleGuideBase;
            this.txtConnection.TabIndex = 129;
            //this.txtConnection.ButtonClick += new Nistec.WinForms.ButtonClickEventHandler(this.txtConnection_ButtonClick);
            // 
            // mcLabel1
            // 
            this.mcLabel1.BackColor = System.Drawing.Color.AliceBlue;
            this.mcLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mcLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.mcLabel1.ForeColor = System.Drawing.Color.Black;
            this.mcLabel1.Location = new System.Drawing.Point(17, 123);
            this.mcLabel1.Name = "mcLabel1";
            this.mcLabel1.Size = new System.Drawing.Size(104, 13);
            this.mcLabel1.Text = "Cover Mode";
            this.mcLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbCoverMode
            // 
            this.cbCoverMode.BackColor = System.Drawing.Color.White;
            this.cbCoverMode.ButtonToolTip = "";
            this.cbCoverMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.cbCoverMode.ForeColor = System.Drawing.Color.Black;
            this.cbCoverMode.IntegralHeight = false;
            this.cbCoverMode.Location = new System.Drawing.Point(17, 139);
            this.cbCoverMode.Name = "cbCoverMode";
            this.cbCoverMode.Size = new System.Drawing.Size(130, 20);
            this.cbCoverMode.StylePainter = this.StyleGuideBase;
            this.cbCoverMode.TabIndex = 10005;
            this.cbCoverMode.SelectedIndexChanged += new System.EventHandler(this.cbCoverMode_SelectedIndexChanged);
            // 
            // isTrans
            // 
            this.isTrans.BackColor = System.Drawing.Color.AliceBlue;
            this.isTrans.ForeColor = System.Drawing.SystemColors.ControlText;
            this.isTrans.Location = new System.Drawing.Point(86, 185);
            this.isTrans.Name = "isTrans";
            this.isTrans.Size = new System.Drawing.Size(61, 13);
            this.isTrans.TabIndex = 10007;
            this.isTrans.Text = "Is Trans";
            // 
            // numMaxRetry
            // 
            this.numMaxRetry.BackColor = System.Drawing.Color.White;
            this.numMaxRetry.ButtonAlign = Nistec.WinForms.ButtonAlign.Right;
            this.numMaxRetry.DecimalPlaces = 0;
            this.numMaxRetry.DefaultValue = "3";
            this.numMaxRetry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.numMaxRetry.ForeColor = System.Drawing.Color.Black;
            this.numMaxRetry.Format = "N";
            this.numMaxRetry.FormatType = NumberFormats.StandadNumber;
            this.numMaxRetry.Location = new System.Drawing.Point(17, 178);
            this.numMaxRetry.MaxValue = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMaxRetry.MinValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numMaxRetry.Name = "numMaxRetry";
            this.numMaxRetry.Size = new System.Drawing.Size(60, 20);
            this.numMaxRetry.StylePainter = this.StyleGuideBase;
            this.numMaxRetry.TabIndex = 10005;
            this.numMaxRetry.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // mcLabel3
            // 
            this.mcLabel3.BackColor = System.Drawing.Color.AliceBlue;
            this.mcLabel3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.mcLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.mcLabel3.ForeColor = System.Drawing.Color.Black;
            this.mcLabel3.Location = new System.Drawing.Point(17, 163);
            this.mcLabel3.Name = "mcLabel3";
            this.mcLabel3.Size = new System.Drawing.Size(104, 13);
            this.mcLabel3.Text = "Max Retry";
            this.mcLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AddItemDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.CaptionVisible = true;
            this.ClientSize = new System.Drawing.Size(304, 261);
            this.ControlLayout = Nistec.WinForms.ControlLayout.Visual;
            this.Controls.Add(this.numMaxRetry);
            this.Controls.Add(this.mcLabel3);
            this.Controls.Add(this.isTrans);
            this.Controls.Add(this.mcLabel1);
            this.Controls.Add(this.cbCoverMode);
            this.Controls.Add(this.mcLabel2);
            this.Controls.Add(this.txtConnection);
            this.Controls.Add(this.ctlLabel1);
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.ctlLabel2);
            this.Controls.Add(this.cbProvider);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "AddItemDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add Queue Item";
            this.Controls.SetChildIndex(this.cbProvider, 0);
            this.Controls.SetChildIndex(this.ctlLabel2, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.Controls.SetChildIndex(this.txtKey, 0);
            this.Controls.SetChildIndex(this.ctlLabel1, 0);
            this.Controls.SetChildIndex(this.txtConnection, 0);
            this.Controls.SetChildIndex(this.mcLabel2, 0);
            this.Controls.SetChildIndex(this.cbCoverMode, 0);
            this.Controls.SetChildIndex(this.mcLabel1, 0);
            this.Controls.SetChildIndex(this.isTrans, 0);
            this.Controls.SetChildIndex(this.mcLabel3, 0);
            this.Controls.SetChildIndex(this.numMaxRetry, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

 
		#endregion

 
	    protected override bool Initialize(object[] args)
		{
			return true;
		}
        

        #region IOfficeControl
        //internal MainWindow owner;

        //public void InitOwner(MainWindow owner)
        //{
        //    this.owner = owner;
        //    SetIStyle();
        //}
        //public void SetIStyle()
        //{
        //    if (owner != null)
        //    {
        //        this.StylePainter = owner.CurrentStylePainter;
        //        foreach (Control c in this.Controls)
        //        {
        //            if (c is Nistec.WinForms.ILayout)
        //            {
        //                ((Nistec.WinForms.ILayout)c).StylePainter = owner.CurrentStylePainter;
        //            }
        //        }
        //    }
        //}
        #endregion

        private QProperties m_QueueItem;

        public static bool Open()
        {
            bool ok = false;
            AddItemDlg frm = new AddItemDlg();
            //frm.LoadSettings(AddItemDlg);
            DialogResult dr= frm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                ok = true;
                frm.Close();
            }
            return ok;
        }

        public QProperties QueueItem
        {
            get { return m_QueueItem; }
        }

        private CoverMode CoverMode
        {
            get
            {
                try
                {
                    return (CoverMode)Enum.Parse(typeof(CoverMode), this.cbCoverMode.Text, true);
                }
                catch
                {
                    return CoverMode.Memory;//.ItemsOnly;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                //this.cbProvider.Items.AddRange(Enum.GetNames(typeof(QueueProvider)));
                this.cbCoverMode.Items.AddRange(Enum.GetNames(typeof(CoverMode)));
                // this.StyleGuideBase.StylePlan = Cache.IStyle.StylePlan;
            }
        }

        public void StyleGuidChanged(Nistec.WinForms.Styles style)
        {
            this.StylePainter.StylePlan = style;
            foreach (Control c in this.Controls)
            {
                if (c is ILayout) ((ILayout)c).StylePainter = this.StyleGuideBase;
            }
        }

        protected override void OnStylePropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnStylePropertyChanged(e);
            this.BackColor = this.StyleGuideBase.BackgroundColor;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!ValidateItem())
                return;
            try
            {
                CreateItem();
                if (!(m_QueueItem==null))//.IsEmpty)
                {
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Nistec.WinForms.MsgDlg.ShowDialog(ex.Message, "ERROR");
            }

        }

        private void CreateItem()
        {
            m_QueueItem = new QProperties(this.txtKey.Text);

            //m_QueueItem.ConnectionString = this.txtConnection.Text;
            m_QueueItem.Mode = CoverMode;
            //m_QueueItem.Server = 0;
            //m_QueueItem.Provider = (QueueProvider)this.cbProvider.SelectedIndex;

            AgentManager.Queue.AddQueue(m_QueueItem);

 
            Close();
        }

        private bool ValidateItem()
        {
            string errorMessage = "";
            bool isValid = true;
            CoverMode type = CoverMode;

            if (txtKey.TextLength == 0)
            {
                isValid = false;
                errorMessage += "\r\nInvalid Queue name";
            }
            //switch (type)
            //{
            //    case CoverMode.ItemsOnly:
            //    case CoverMode.ItemsAndLog:
            //        if (cbProvider.Text.Length == 0)
            //        {
            //            isValid = false;
            //            errorMessage += "\r\nInvalid Provider";
            //        }
            //        if (txtConnection.Text.Length == 0)
            //        {
            //            isValid = false;
            //            errorMessage += "\r\nInvalid Connection";
            //        }
            //        break;
            //}

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MsgBox.ShowError(errorMessage);
            }
            return isValid;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbCoverMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (CoverMode)
            {
                //case CoverMode.ItemsOnly:
                //case CoverMode.ItemsAndLog:
                //    this.cbProvider.Enabled= true;
                //    this.txtConnection.Enabled = true;
                //    break;
                default:
                    this.cbProvider.Enabled = false;
                    this.txtConnection.Enabled = false;
                    break;
  
            }
        }

        //void txtConnection_ButtonClick(object sender, ButtonClickEventArgs e)
        //{
        //    Nistec.Ado.UI.ConnectionWizardForm form = new Nistec.Ado.UI.ConnectionWizardForm();
        //    DialogResult dr= form.ShowDialog();
        //    if (dr == DialogResult.Yes)
        //    {
        //      this.txtConnection.Text=  form.Connection.ConnectionString;
        //      DBProvider provider = form.Connection.Provider;
        //      if (provider == DBProvider.Firebird)
        //          this.cbProvider.SelectedItem = QueueProvider.Embedded;
        //        else
        //          this.cbProvider.SelectedItem = QueueProvider.SqlServer;

        //    }
        //}

	}
}

