using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Legacy
{
    /// <summary>
    /// Receive Item Callback delegate
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public delegate IQueueItem ReceiveItemCallback(TimeSpan timeout, object state);

    #region ErrorOcurredEventArgs

    public delegate void ErrorOcurredEventHandler(object sender, ErrorOcurredEventArgs e);

    public class ErrorOcurredEventArgs : EventArgs
    {
        private string m_Message = "";

        #region Constructors

        public ErrorOcurredEventArgs(string message)
        {
            if (message == null)
                message = "";
            m_Message = message;
        }

        #endregion

        #region Properties

        public string Message
        {
            get { return m_Message; }
            set
            {
                if (value == null)
                    value = "";
                m_Message = value;
            }
        }

        #endregion
    }

    #endregion

    #region MessageEventsArgs

    public delegate void MessageEventHandler(object sender, MessageEventArgs e);

    public class MessageEventArgs : EventArgs
    {
        private System.Messaging.Message msg;
        private string state;

        public MessageEventArgs(System.Messaging.Message message, string state)
        {
            this.msg = message;
            this.state = state;
        }

        #region Properties Implementation

        public System.Messaging.Message WindowsMessage
        {
            get { return this.msg; }
        }

        public string State
        {
            get { return this.state; }
        }

        public string FormatMessage()
        {
            return System.Web.HttpUtility.UrlDecode(msg.Body.ToString(), new System.Text.UTF8Encoding());
        }
        public string FormatMessage(System.Text.Encoding en)
        {
            return System.Web.HttpUtility.UrlDecode(msg.Body.ToString(), en);
        }

        #endregion

    }

    #endregion

    #region PropertyItemChangedEventArgs

    public delegate void PropertyItemChangedEventHandler(object sender, PropertyItemChangedEventArgs e);

    public class PropertyItemChangedEventArgs : EventArgs
    {
        private string m_PropertyName = "";
        private object m_PropertyValue = null;
        public PropertyItemChangedEventArgs(string propertyName, object popertyValue)
        {
            m_PropertyName = propertyName;
            m_PropertyValue = popertyValue;
        }

        public string PropertyName
        {
            get { return m_PropertyName; }
        }

        public object PropertyValue
        {
            get { return m_PropertyValue; }
        }
    }

    #endregion

    public delegate void ReceiveCompletedEventHandler(object sender, ReceiveCompletedEventArgs e);

    public class ReceiveCompletedEventArgs : EventArgs
    {
        // Fields
        private IQueueItem item;
        private IAsyncResult result;
        private IReceiveCompleted sender;
        //private object state;

        // Methods
        internal ReceiveCompletedEventArgs(IReceiveCompleted sender, IAsyncResult result)
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


        public IQueueItem Item
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
                            sender.Completed(item.ItemId, (int)Messaging.ItemState.Commit, item.HasAttach);

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
}
