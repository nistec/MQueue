using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using MControl.Messaging;

public partial class _Default : System.Web.UI.Page 
{
    const string cnn = "Data Source=mcontrol; Initial Catalog=McQueueDB; uid=sa;password=tishma; Connection Timeout=30";

    static int counter = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
             RunRemoteQueue();
    }

    private void RunRemoteQueue()
    {

        //AsyncManagerWorker();


        McQueueProperties prop = new McQueueProperties("NC_Quick");

        prop.ConnectionString = cnn;
        prop.CoverMode = CoverMode.ItemsAndLog;
        prop.Server = 0;
        prop.Provider = QueueProvider.SqlServer;
        prop.ReloadOnStart = true;
        Console.WriteLine("create remote queue");
        RemoteQueue rqc = RemoteManager.Create(prop);

        string[] list = RemoteManager.QueueList;

        Console.Write(list);
        //rqc.ReceiveCompleted += new ReceiveCompletedEventHandler(rq_ReceiveCompleted);


        //RemoteQueue.AddQueue(prop);
        //Console.WriteLine("rempote queue created");
        //rq.MessageArraived += new QueueItemEventHandler(rq_MessageArraived);
        //rq.MessageReceived += new QueueItemEventHandler(rq_MessageReceived);
        //Console.WriteLine(rqc.Reply("test"));

        AsyncManagerWorker();
        //RunThreads(new ThreadStart(AsyncRemoteDequeue), 10);
        //RunThreads(new ThreadStart(AsyncManagerWorker),10);

        while (true)
        {
            //IAsyncResult result = rqc.BeginReceive();
            //Console.WriteLine("Count: {0}", rqc.Count);
            //Thread.Sleep(10);
        }

    }
    void rq_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
    {
        IReceiveCompleted mq = (IReceiveCompleted)sender;

        // End the asynchronous receive operation.
        IQueueItem item = mq.EndReceive(e.AsyncResult);

        Console.WriteLine(item.ItemId.ToString());
    }

    private void AsyncManagerWorker()
    {
        RemoteQueue r = new RemoteQueue("NC_Quick");
        int count = 1000;
        for (int i = 0; i <= count; i++)
        {
            Priority p = (i % 5 == 0) ? Priority.High : (i % 2 == 0) ? Priority.Medium : Priority.Normal;
            QueueItem item = new QueueItem(p);
            item.MessageId = i;
            item.Body = "abc this is a test  אכן זוהי דוגמא";
            item.Subject = "test";
            item.Sender = "ibm";
            item.Destination = "nissim";
            //RemoteQueueClient q = new RemoteQueueClient("Cellcom");
            r.Enqueue(item);
            //logger.WriteLoge("Enqueue: " + ItemToString(item), Mode.INFO);
            counter = i;
        }
    }

}
