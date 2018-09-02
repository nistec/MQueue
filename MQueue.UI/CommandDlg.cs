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
using Nistec.Messaging.Server;


namespace Nistec.Messaging.UI
{

	/// <summary>
	/// Summary description for SmsSettings.
	/// </summary>
	public class CommandDlg : Nistec.WinForms.McForm
    {
        private McLabel ctlLabel1;
        private McTextBox txtKey;
        private McButton btnOk;
        private McButton btnCancel;
        private McLabel ctlLabel2;
        private McComboBox cbCommand;
		private System.ComponentModel.IContainer components=null;

        public CommandDlg()
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
            this.cbCommand = new Nistec.WinForms.McComboBox();
            this.SuspendLayout();
            // 
            // StyleGuideBase
            // 
            this.StyleGuideBase.AlternatingColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(225)))), ((int)(((byte)(239)))));
            this.StyleGuideBase.BackgroundColor = System.Drawing.Color.AliceBlue;
            this.StyleGuideBase.BorderColor = System.Drawing.Color.SteelBlue;
            this.StyleGuideBase.BorderHotColor = System.Drawing.Color.Blue;
            this.StyleGuideBase.CaptionColor = System.Drawing.Color.SteelBlue;
            this.StyleGuideBase.ColorBrush1 = System.Drawing.Color.LightSteelBlue;
            this.StyleGuideBase.ColorBrush2 = System.Drawing.Color.AliceBlue;
            this.StyleGuideBase.ColorBrushLower = System.Drawing.Color.FromArgb(((int)(((byte)(137)))), ((int)(((byte)(174)))), ((int)(((byte)(237)))));
            this.StyleGuideBase.ColorBrushUpper = System.Drawing.Color.AliceBlue;
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
            this.btnOk.Location = new System.Drawing.Point(205, 59);
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
            this.btnCancel.Location = new System.Drawing.Point(205, 99);
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
            this.ctlLabel2.Text = "Command Name";
            this.ctlLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbCommand
            // 
            this.cbCommand.BackColor = System.Drawing.Color.White;
            this.cbCommand.ButtonToolTip = "";
            this.cbCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.cbCommand.ForeColor = System.Drawing.Color.Black;
            this.cbCommand.IntegralHeight = false;
            this.cbCommand.Items.AddRange(new object[] {
            "ReEnqueueLog",
            "ClearAllItems",
            "TruncateDB"});
            this.cbCommand.Location = new System.Drawing.Point(17, 104);
            this.cbCommand.Name = "cbCommand";
            this.cbCommand.Size = new System.Drawing.Size(130, 20);
            this.cbCommand.StylePainter = this.StyleGuideBase;
            this.cbCommand.TabIndex = 10012;
            // 
            // CommandDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.CaptionVisible = true;
            this.ClientSize = new System.Drawing.Size(308, 156);
            this.ControlLayout = Nistec.WinForms.ControlLayout.Visual;
            this.Controls.Add(this.cbCommand);
            this.Controls.Add(this.ctlLabel1);
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.ctlLabel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "CommandDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Queue Command";
            this.Controls.SetChildIndex(this.ctlLabel2, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.btnOk, 0);
            this.Controls.SetChildIndex(this.txtKey, 0);
            this.Controls.SetChildIndex(this.ctlLabel1, 0);
            this.Controls.SetChildIndex(this.cbCommand, 0);
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

       

        public static bool Open()
        {
            bool ok = false;
            CommandDlg frm = new CommandDlg();
            //frm.LoadSettings(AddItemDlg);
            DialogResult dr= frm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                ok = true;
                frm.Close();
            }
            return ok;
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
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                Nistec.WinForms.MsgDlg.ShowDialog(ex.Message, "ERROR");
            }

        }

        private void ExecuteCommand()
        {
            //int res = 0;
            switch (cbCommand.Text)
            {
                //case "ReEnqueueLog":
                //    RemoteManager.ReEnqueueLog(txtKey.Text);
                //    if (res > 0)
                //    {
                //        MsgBox.ShowInfo("ReEnqueued: " + res.ToString());
                //    }
                //    break;
                case "ClearAllItems":

                    string name = txtKey.Text;
                    if (MsgBox.ShowQuestion("Clear All items Queue " + name + "?", "Nistec", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                       AgentManager.Queue.ClearAllItems(name);
                    }


                    break;
                //case "TruncateDB":

                //    if (MsgBox.ShowQuestion("Truncate DB ?", "Nistec", MessageBoxButtons.YesNo) == DialogResult.Yes)
                //    {
                //        RemoteManager.ReEnqueueLog(txtKey.Text);
                //        if (res > 0)
                //        {
                //            MsgBox.ShowInfo("ReEnqueued: " + res.ToString());
                //        }
                //    }
                //    break;
            }

            Close();
        }

        private bool ValidateItem()
        {
            string errorMessage = "";
            bool isValid = true;

            if (txtKey.TextLength == 0)
            {
                isValid = false;
                errorMessage += "\r\nInvalid Queue name";
            }
           
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

   

	}
}

