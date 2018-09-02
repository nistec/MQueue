using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
//using Nistec.Threading;
using Nistec.WinForms;
using System.Windows.Forms;
using Nistec.Win;
using Nistec.Messaging.Server;
using Nistec.Messaging.Remote;
using Extension.Nistec.Threading;

namespace Nistec.Messaging.UI
{
    public class AsyncLoaderForm:Nistec.WinForms.ProgressBox
    {
        string filename;
        string queueName;
        McForm owner;
        DataTable dt;

        public AsyncLoaderForm(McForm owner,string filename, string queueName)
        {
            this.owner = owner;
            this.filename = filename;
            this.queueName = queueName;
            base.ProgressText = "Load xml";
            base.ProgressBar1.Visible = true;
            base.ProgressBar1.BarColor=StyleLayout.DefaultLayout.CaptionColor;
            base.Text = "Queue Items Loader";
        }

        public DialogResult InvokeLoader(object args)
        {
            base.AsyncBeginInvoke(args);
            return this.ShowDialog();
        }
  
        protected override void OnAsyncCompleted(AsyncCallEventArgs e)
        {
            base.OnAsyncCompleted(e);

            try
            {

            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
            finally
            {
                //Closewindow();
            }
        }

        protected override void OnAsyncExecutingProgress(AsyncProgressEventArgs e)
        {
            base.OnAsyncExecutingProgress(e);
            this.ProgressText = e.Message;
        }

        protected override void OnAsyncExecutingWorker(AsyncCallEventArgs e)
        {
            base.OnAsyncExecutingWorker(e);
            try
            {
                DataSet ds = new DataSet("RemoteQueue");
                ds.ReadXml(filename);
                dt = ds.Tables[0];
                int count = dt.Rows.Count;
                this.ProgressBar1.Maximum = count;

                var hostPipe = QueueHost.Parse("ipc:.:nistec_queue_manager?" + queueName);

                QueueApi Client = new QueueApi(hostPipe);

                foreach (DataRow dr in dt.Rows)
                {
                    Client.Send(QueueItem.Create(dr));
                    this.ProgressBar1.Increment(1);
                }

            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex.Message);
            }
            finally
            {
                //Closewindow();
            }
        }
        protected override void OnAsyncCancelExecuting(EventArgs e)
        {
            base.OnAsyncCancelExecuting(e);

            Closewindow();
        }

        private delegate void CloseCallBack();

        private void Closewindow()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new CloseCallBack(Closewindow), null);
            }
            else
            {
                this.Close();
            }
        }


     
    }

}
