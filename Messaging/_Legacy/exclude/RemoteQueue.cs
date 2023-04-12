using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Threading;
using System.Runtime.Remoting;  
using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Permissions;
//using Nistec.Win;


namespace Nistec.Legacy
{
  
 
    [SecurityPermission(SecurityAction.Assert)]
    public class RemoteQueue : NetComponent,IAsyncQueue, IReceiveCompleted
    {

		#region <Members>
        internal bool m_UseMessageQueueListner = false;
        internal bool m_IsTrans = false;
        string m_QueueName;

        const string host = "ipc://portMQueue/RemoteQueueServer.rem";

        IRemoteQueue manager;

		#endregion

		#region <Ctor>

        public RemoteQueue(string queueName)
            :this(queueName,false)
        {

        }
         public RemoteQueue(string queueName, bool isTrans)
         {
             m_QueueName = queueName;
             m_IsTrans = isTrans;
            try
            {
                manager = (IRemoteQueue)Activator.GetObject
                (
                typeof(IRemoteQueue),
                host
                );

                //Console.WriteLine(manager.GetType().ToString());
                //Console.WriteLine(manager.GetType().IsAssignableFrom(typeof(IRemoteQueueManager)));
                //Console.WriteLine(typeof(IRemoteQueueManager).IsAssignableFrom(manager.GetType()));

                if (manager == null)
                    Console.WriteLine("cannot locate remote queue server");
                else
                {
                    Console.WriteLine(manager.Reply("Remote queue activated"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Remote queue Error: "+ex.Message);
                throw ex;
            }
         }

         public static RemoteQueue Get(string name)
         {
             return new RemoteQueue(name);
         }

		#endregion

        #region message events

         /// <summary>
         /// ErrorOcurred
         /// </summary>
         public event ErrorOcurredEventHandler ErrorOcurred;

        /// <summary>
        /// Message Received
        /// </summary>
        public event Messaging.QueueItemEventHandler MessageReceived;
        /// <summary>
        /// Message Arraived
        /// </summary>
        public event Messaging.QueueItemEventHandler MessageArraived;
        /// <summary>
        /// PropertyChanged
        /// </summary>
        public event PropertyItemChangedEventHandler PropertyChanged;

       

        protected virtual void OnMessageArraived(Messaging.QueueItemEventArgs e)
        {
            if (MessageArraived != null)
                MessageArraived(this, e);
        }

        protected virtual void OnMessageReceived(Messaging.QueueItemEventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }
        protected virtual void OnErrorOcurred(ErrorOcurredEventArgs e)
        {
            if (ErrorOcurred != null)
                ErrorOcurred(this, e);
        }

        /// <summary>
        /// OnErrorOcurred
        /// </summary>
        /// <param name="msg"></param>
        private void OnErrorOcurred(string msg)
        {
            Console.WriteLine("ErrorOcurred: " + msg);
            OnErrorOcurred(new ErrorOcurredEventArgs(msg));
        }


        /// <summary>
        /// OnPropertyChanged
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(PropertyItemChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        internal void OnPropertyChanged(string propertyName, object propertyValue)
        {
            OnPropertyChanged(new PropertyItemChangedEventArgs(propertyName, propertyValue));
        }
        #endregion

        #region properties members

   
        public string QueueName 
        { 
            get { return m_QueueName; } 
        }
        
        public bool Enabled 
        { 
            get { return (bool)manager.GetProperty(m_QueueName,"Enabled"); } 
            set { SetProperty("Enabled", value); } 
        }
        public int MaxItemsPerSecond
        {
            get { return (int)manager.GetProperty(m_QueueName, "MaxItemsPerSecond"); }
            set { SetProperty("MaxItemsPerSecond", value); }
        }
        public int Server
        {
            get { return (int)manager.GetProperty(m_QueueName, "Server"); }
            //set { SetProperty( "Server", value); }
        }
        public bool IsTrans
        {
            get { return (bool)manager.GetProperty(m_QueueName, "IsTrans"); }
            //set { SetProperty( "IsTrans", value); }
        }
        public string DateFormat
        {
            get { return (string)manager.GetProperty(m_QueueName, "DateFormat"); }
            set { SetProperty( "DateFormat", value); }
        }
        public bool HoldDequeue
        {
            get { return (bool)manager.GetProperty(m_QueueName, "HoldDequeue"); }
            set { SetProperty( "HoldDequeue", value); }
        }

        #endregion

        #region IRemoteQueueManager members

        public bool Initilaized
        {
            get { return manager.Initilaized(m_QueueName); }
        }
        public string Reply(string text)
         {
             return manager.Reply(text);
         }

        public bool CanQueue(uint count)
        {
            return manager.CanQueue(m_QueueName,count);
        }

        //public object ExecuteCommand(string commandName, string command, params string[] param)
        //{
        //    return manager.ExecuteCommand(m_QueueName, commandName, command, param);
        //}

        //protected void OnPropertyChanged(PropertyItemChangedEventArgs e)
        //{
        //    //base.OnPropertyChanged(e);
        //    SetProperty(e.PropertyName, e.PropertyValue);
        //}

        public void SetProperty(string propertyName, object propertyValue)
        {
            manager.SetProperty(m_QueueName, propertyName, propertyValue);
            OnPropertyChanged(propertyName, propertyValue);
        }

        public void ValidateCapacity()
        {
            manager.ValidateCapacity(m_QueueName);
        }

  
       
        #endregion

        #region  asyncInvoke

        private AsyncCallback onRequestCompleted;
        private ManualResetEvent resetEvent;

        //public delegate IQueueItem ReceiveItemCallback(TimeSpan timeout,ref object state);
        public event ReceiveCompletedEventHandler ReceiveCompleted;
        //public event ReceiveResultEventHandler ReceiveResult;

        private  ManualResetEvent ResetEvent
        {
            get
            {
                if (resetEvent == null)
                    resetEvent = new ManualResetEvent(false);
                return resetEvent;
            }
        }

        protected virtual void OnReceiveCompleted(ReceiveCompletedEventArgs e)
        {
            if (ReceiveCompleted != null)
                ReceiveCompleted(this, e);
        }

        //public void RequestCompleted(IAsyncResult asyncResult)
        //{
        //    OnReceiveCompleted(new ReceiveCompletedEventArgs(this, asyncResult));
        //}
      

        private IQueueItem ReceiveItemWorker(TimeSpan timeout,object state)
        {
            IQueueItem item = null;
            Messaging.TimeOut to = new Messaging.TimeOut(timeout);
            while (item == null)
            {
                if (to.IsTimeOut())
                {
                    //OnErrorOcurred("ReceiveItem Timeout");
                    state = (int)Messaging.ReceiveState.Timeout; 
                    break;
                }
                item = this.Dequeue();
                if (item == null)
                {
                    Thread.Sleep(100);
                }
            }
            if (item != null)
            {
                state = (int)Messaging.ReceiveState.Success;
                Console.WriteLine("Dequeue item :{0}", item.ItemId);
            }
            return item;//.Copy() as IQueueItem;
        }

        public IQueueItem AsyncReceive()
        {
            return AsyncReceive(null);
        }
        public IQueueItem AsyncReceive(object state)
        {
            if (state == null)
            {
                state = new object();
            }
            TimeSpan timeout=McQueue.DefaultMaxTimeout;
            ReceiveItemCallback caller = new ReceiveItemCallback(this.ReceiveItemWorker);

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(timeout, state,CreateCallBack(), caller);

            result.AsyncWaitHandle.WaitOne();

            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
            IQueueItem item = caller.EndInvoke(result);
            AsyncCompleted(item);
            return item;

        }

        public IAsyncResult BeginReceive(object state)
        {
            return BeginReceive(McQueue.DefaultMaxTimeout, state, null);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, object state)
        {
            return BeginReceive(timeout, state, null);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, object state, AsyncCallback callback)
        {

            //AsyncRequest request = new AsyncRequest(this, timeout, state, callback);
            //request.BeginRequest();
            //return request;

            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if ((totalMilliseconds < 0L) || (totalMilliseconds > 4294967295L))
            {
                throw new ArgumentException("InvalidParameter", "timeout");
            }
            ReceiveItemCallback caller = new ReceiveItemCallback(ReceiveItemWorker);

            if (callback == null)
            {
                callback = CreateCallBack();
            }
            if (state == null)
            {
                state = new object();
            }
            state =(int) ReceiveState.Wait;

            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke(timeout,state, callback, caller);

            this.ResetEvent.Set();
            //OnReceiveCompleted(new ReceiveResultEventArgs(this, result));
            return result;
        }


        // Callback method must have the same signature as the
        // AsyncCallback delegate.
        public IQueueItem EndReceive(IAsyncResult asyncResult)
        {

            //if (asyncResult == null)
            //{
            //    throw new ArgumentNullException("asyncResult");
            //}
            //if (!(asyncResult is AsyncRequest))
            //{
            //    throw new ArgumentException("Invalid Async Result");
            //}
            //AsyncRequest request = (AsyncRequest)asyncResult;
            
            //return request.EndRequest(asyncResult);


            // Retrieve the delegate.
            ReceiveItemCallback caller = (ReceiveItemCallback)asyncResult.AsyncState;

            //if (((ReceiveState)asyncResult.AsyncState) != ReceiveState.Success)
            //{
            //    caller.EndInvoke(asyncResult);
            //    this.ResetEvent.WaitOne();
            //    return null;
            //}

            // Call EndInvoke to retrieve the results.
            IQueueItem item = (IQueueItem)caller.EndInvoke(asyncResult);

            AsyncCompleted(item);
            this.ResetEvent.WaitOne();
            return item;
        }

        private AsyncCallback CreateCallBack()
        {
            if (this.onRequestCompleted == null)
            {
                this.onRequestCompleted = new AsyncCallback(this.OnRequestCompleted);
            }
            return this.onRequestCompleted;
        }

        private void AsyncCompleted(IQueueItem item)
        {
            if (item != null)
            {
                if (item != null && IsTrans)
                {
                    //this.TransBegin(item);
                }
                else
                {
                    this.Completed(item.ItemId, (int)ItemState.Commit, item.HasAttach);
                }
            }
        }

        private void OnRequestCompleted(IAsyncResult asyncResult)
        {
            OnReceiveCompleted(new ReceiveCompletedEventArgs(this, asyncResult));
        }

        #endregion
 
        #region override

        public void AbortTrans(Guid ItemId, bool hasAttach)
        {
            manager.AbortTrans(m_QueueName, ItemId, hasAttach);
        }

        public void CommitTrans(Guid ItemId, bool hasAttach)
        {
            manager.CommitTrans(m_QueueName, ItemId, hasAttach);
        }

        /// <summary>
        /// Get or Set MaxCapacity of queue
        /// </summary>
        public int MaxCapacity
        {
            get { return manager.MaxCapacity(m_QueueName); }
            set { SetProperty("MaxCapacity", value); }
        }

        public int Count
        {
            get { return manager.Count(m_QueueName); }
        }

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Peek()
        {
            string xml = manager.Peek(m_QueueName);
            return DeserializeItem(xml,false);
        }

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public IQueueItem Peek(Priority priority)
        {
            string xml = manager.Peek(m_QueueName, priority);
            return DeserializeItem(xml,false);
        }

        /// <summary>
        /// Peek Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Peek(Guid ptr)
        {
            string xml = manager.Peek(m_QueueName, ptr);
            return DeserializeItem(xml,false);
        }


        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public IQueueItem Dequeue(Priority priority)
        {
            string xml = manager.Dequeue(m_QueueName, priority);
            return DeserializeItem(xml, m_UseMessageQueueListner);
        }

        /// <summary>
        /// Dequeue Message
        /// </summary>
        /// <returns></returns>
        public IQueueItem Dequeue(Guid ptr)
        {
            string xml = manager.Dequeue(m_QueueName,ptr);
            return DeserializeItem(xml, m_UseMessageQueueListner);

        }


        public IQueueItem Dequeue()
        {
            string serItem = manager.Dequeue(m_QueueName);
            return DeserializeItem(serItem, m_UseMessageQueueListner);
        }

        internal IQueueItem RemoveItem()
        {
            string serItem = manager.RemoveItem(m_QueueName);
            return DeserializeItem(serItem, m_UseMessageQueueListner);
        }

        internal void Clear()
        {
           manager.Clear(m_QueueName);
        }

        private IQueueItem DeserializeItem(string serItem,bool raisEvent)
        {
            if (string.IsNullOrEmpty(serItem))
            {
                return null;
            }
            IQueueItem item = QueueItem.Deserialize(serItem) as IQueueItem;
            if (raisEvent)
            {
                OnMessageReceived(new QueueItemEventArgs(item, ItemState.Dequeue));
                if(!m_IsTrans)
                    Completed(item.ItemId, (int)ItemState.Commit, item.HasAttach);
            }
            return item;
        }

        public void Enqueue(IQueueItem item)
        {
            manager.Enqueue(m_QueueName, item.Serialize());
            OnMessageArraived(new QueueItemEventArgs(item, ItemState.Enqueue));
        }

        public void Completed(Guid ItemId, int status, bool hasAttach)
        {
            manager.Completed(m_QueueName, ItemId, status, hasAttach);

        }

        public void ReEnqueue(IQueueItem item)
        {
            manager.ReEnqueue(m_QueueName, item.Serialize());
            OnMessageArraived(new QueueItemEventArgs(item, ItemState.Enqueue));
        }

        /*
        public IQueueItem[] GetQueueCoverItems()
        {
            DataTable dt = (DataTable)  ExecuteCommand("GetQueueItemsTable", null, null);

            if (dt != null)
            {
                return DataTableToQueueItems(dt,false);
                //List<IQueueItem> items = DataTableToQueueItems(dt);
                //items.ToArray();
            }
            return null;
        }
        */

        //public DataTable GetQueueCoverItemsTable()
        //{
        //  return (DataTable)  ExecuteCommand("GetQueueItemsTable", null, null);
        //}

        public DataTable GetQueueItemsTable()
        {
            try
            {
                return (DataTable)manager.GetQueueItemsTable(m_QueueName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public IQueueItem[] GetQueueItems()
        {
            try
            {
                return manager.GetQueueItems(m_QueueName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

      #endregion

        #region Cover items
        /*
        internal IQueueItem[] DataTableToQueueItems(DataTable dt, bool SerializeBody)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return null;
            }
            List<IQueueItem> list = new List<IQueueItem>();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add(DataRowToQueueItem(dr, SerializeBody));
            }
            return list.ToArray();
        }



                internal IQueueItem DataRowToQueueItem(DataRow dr, bool SerializeBody)
                {
                    if (dr == null)
                        return null;
                    QueueItem item = new QueueItem(dr, SerializeBody);
                    if (item.HasAttach)
                    {
                        item.AddAttachment(GetQueueAttachData(item.ItemId, m_QueueName));
                    }
                    return item;
                }

                /// <summary>
                /// Get GetQueueAttachData in DataRow array
                /// </summary>
                /// <param name="attchId"></param>
                /// <param name="m_QueueName"></param>
                /// <returns></returns>
                private DataRow[] GetQueueAttachData(Guid attchId, string m_QueueName)
                {
                    DataTable dt = (DataTable)ExecuteCommand("ExecuteDatatable", string.Format(SQLCMD.SqlSelectRowAttach, attchId, m_QueueName), null);
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        return null;
                    }
                    return dt.Select();
                }

         */
        //public void ClearQueueItems(QueueItemType type)
        //{
        //    string cmdText = null;
        //    switch (type)
        //    {
        //        case QueueItemType.QueueItems:
        //            cmdText = "ClearQueueItems";
        //            break;
        //        case QueueItemType.AttachItems:
        //            cmdText = "ClearQueueAttachItems";
        //            break;
        //        case QueueItemType.HoldItems:
        //            cmdText ="ClearQueueHoldItems";
        //            break;
        //        case QueueItemType.AllItems:
        //            cmdText ="ClearQueueAllItems";
        //            break;
        //        default:
        //            return;
        //    }
        //    ExecuteCommand(cmdText, null,null);


        //}

        //public void ClearFinallItems(QueueItemType type)
        //{
        //    ExecuteCommand("ClearFinallItems", null, type.ToString());
        //}


        //public void RemoveQueue(string queueName)
        //{
        //    ExecuteCommand("RemoveQueue", null, queueName);
        //}


        //public void ClearTimeOutItems()
        //{
        //    ExecuteCommand("ClearTimeOutItems", null, null);
        //}


        //public int ReEnqueueLog(string queueName)
        //{
        //    return manager.ReEnqueueLog(queueName);

        //}

        #endregion

        #region Hold items

        internal bool enqueueHoldItems = false;

        /// <summary>
        /// Re Enqueue all Hold Items remain in DB by interval minute 
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="intervalMinute"></param>
        public void HoldItemsEnqueue(int capacity, int intervalMinute)
        {
            if (enqueueHoldItems)
                return;
            try
            {
                enqueueHoldItems = true;
                //ExecuteCommand("HoldItemsEnqueue", null, capacity.ToString(), intervalMinute.ToString());
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            finally
            {
                enqueueHoldItems = false;
            }
        }

/*
        /// <summary>
        /// HoldItemsClear
        /// </summary>
        public void HoldItemsClear()
        {
            if (enqueueHoldItems)
                return;
            try
            {
                ExecuteCommand("ExecuteCmd", string.Format(SQLCMD.SqlRemoveItemByStatus, m_QueueName, (int)ItemState.Hold), null);
            }
            catch { }
            finally
            {
                enqueueHoldItems = false;
            }
        }
*/
        ///// <summary>
        ///// Clear HoldItems by interval minute
        ///// </summary>
        ///// <param name="intervalMinute"></param>
        //public void HoldItemsClear(int intervalMinute)
        //{
        //    if (HoldItemsCount(intervalMinute) == 0)
        //    {
        //        HoldItemsClear();
        //    }
        //}
/*
        /// <summary>
        /// Get HoldItemsCount by interval minute
        /// </summary>
        /// <param name="intervalMinute"></param>
        /// <returns></returns>
        public int HoldItemsCount()//int intervalMinute)
        {
            try
            {
                //DateTime dateFrom = DateTime.Now.AddMinutes(Math.Abs(intervalMinute) * -1);
                object o = ExecuteCommand("ExecuteCmdScalar", string.Format(SQLCMD.SqlSelectHoldItemsCount, m_QueueName,  Server), null);
                if (o == null)
                    return 0;
                return Types.ToInt(o, 0);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return 0;
            }
        }
*/
        #endregion

        

    }
}

    
