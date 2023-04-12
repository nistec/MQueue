using System;
//using System.Drawing;
using System.ComponentModel;
//using System.Windows.Forms;
using System.Messaging;
using Nistec.IO;
using Nistec.Messaging.Remote;



namespace Nistec.Messaging
{

    #region  ReceiveMessageCompletedEventArgs

    /// <summary>
    /// Receive Item Callback delegate
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public delegate IQueueMessage ReceiveMessageCallback(TimeSpan timeout, object state);

    public delegate void ReceiveMessageCompletedEventHandler(object sender, ReceiveMessageCompletedEventArgs e);

    public class ReceiveMessageCompletedEventArgs : EventArgs
    {
        // Fields
        private IQueueMessage item;
        private IAsyncResult result;
        private IQueueClient sender;
        //private object state;

        // Methods
        public ReceiveMessageCompletedEventArgs(IQueueClient sender, IAsyncResult result)
        {
            this.result = result;
            this.sender = sender;
            //this.state = state;// result.AsyncState;
        }

        // Properties
        public IAsyncResult AsyncResult
        {
            get
            {
                return this.result;
            }
            set
            {
                this.result = value;
            }
        }


        public IQueueMessage Item
        {
            get
            {
                if (this.item == null)
                {
                    try
                    {
                        this.item = this.sender.EndReceive(this.result);
                        if (item != null && this.sender.IsCoverable)//&& state!= ItemState.Abort)
                        {
                            //this.state = ItemState.Commit;
                            sender.Commit(item.GetPtr());//.Completed(item.ItemId, (int)ItemState.Commit);//, item.HasAttach);

                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
                return this.item;
            }
        }
        
    }

    #endregion

    #region  ReceiveItemCompletedEventArgs
    /*
    /// <summary>
    /// Receive Item Callback delegate
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public delegate IQueueMessage ReceiveItemCallback(TimeSpan timeout, object state);

    public delegate void ReceiveItemCompletedEventHandler(object sender, ReceiveItemCompletedEventArgs e);

    public class ReceiveItemCompletedEventArgs : EventArgs
    {
        // Fields
        private IQueueMessage item;
        private IAsyncResult result;
        private IReceiveCompleted sender;
        //private object state;

        // Methods
        public ReceiveItemCompletedEventArgs(IReceiveCompleted sender, IAsyncResult result)
        {
            this.result = result;
            this.sender = sender;
            //this.state = state;// result.AsyncState;
        }

        // Properties
        public IAsyncResult AsyncResult
        {
            get
            {
                return this.result;
            }
            set
            {
                this.result = value;
            }
        }


        public IQueueMessage Item
        {
            get
            {
                if (this.item == null)
                {
                    try
                    {
                        this.item = this.sender.EndReceive(this.result);
                        if (item != null && !this.sender.IsTrans)//&& state!= ItemState.Abort)
                        {
                            //this.state = ItemState.Commit;
                            sender.Completed(item.ItemId, (int)ItemState.Commit);//, item.HasAttach);

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }
                return this.item;
            }
        }

    }
    */
    #endregion


    #region QueueItemEventsArgs

    public delegate void QueueItemEventHandler(object sender, QueueItemEventArgs e);

    public class QueueItemEventArgs : EventArgs
    {
        private IQueueMessage item;
        private MessageState state;
        private string message;

        public QueueItemEventArgs(IQueueMessage itm, MessageState state)
        {
            this.item = itm;
            this.state = state;
        }

        public QueueItemEventArgs(IQueueMessage itm, MessageState state, string message)
        {
            this.item = itm;
            this.state = state;
            this.message = message;
        }

        #region Properties 

        /// <summary>
        /// Get QueueMessage
        /// </summary>
        public IQueueMessage Item
        {
            get { return this.item; }
        }
        /// <summary>
        /// Get Queue item state
        /// </summary>
        public MessageState State
        {
            get { return this.state; }
            //set { this.state = value; }
        }
        /// <summary>
        /// Get Queue item state message
        /// </summary>
        public string Message
        {
            get { return this.message; }
            //set { this.state = value; }
        }
        public void Commit()
        {
            this.state = MessageState.TransCommited;
        }
        public void Abort()
        {
            this.state = MessageState.TransAborted;
        }

        #endregion

    }

    #endregion

   

    #region IntPtrItemEventsArgs

    public delegate void PtrItemEventHandler(object sender, PtrItemEventArgs e);

    public class PtrItemEventArgs : EventArgs
    {
        private Ptr ptr;
        private ItemState state;

        public PtrItemEventArgs(Ptr ptr, ItemState state)
        {
            this.ptr = ptr;
            this.state = state;
        }

        #region Properties 

        /// <summary>
        /// Get QueueMessage
        /// </summary>
        public Ptr Item
        {
            get { return this.ptr; }
        }
        /// <summary>
        /// Get Queue item state
        /// </summary>
        public ItemState State
        {
            get { return this.state; }
            //set { this.state = value; }
        }

        public void Commit()
        {
            this.state = ItemState.Commit;
        }
        public void Abort()
        {
            this.state = ItemState.Abort;
        }

         #endregion

    }

    #endregion

    #region IntPtrItemEventsArgs

    public delegate void TItemEventHandler<T>(object sender, TItemEventArgs<T> e);

    public class TItemEventArgs<T> : EventArgs
    {
        private T ptr;
        private ItemState state;

        public TItemEventArgs(T ptr, ItemState state)
        {
            this.ptr = ptr;
            this.state = state;
        }

        #region Properties 

        /// <summary>
        /// Get QueueMessage
        /// </summary>
        public T Item
        {
            get { return this.ptr; }
        }
        /// <summary>
        /// Get Queue item state
        /// </summary>
        public ItemState State
        {
            get { return this.state; }
            //set { this.state = value; }
        }

        public void Commit()
        {
            this.state = ItemState.Commit;
        }
        public void Abort()
        {
            this.state = ItemState.Abort;
        }

        #endregion

    }

    #endregion
}


