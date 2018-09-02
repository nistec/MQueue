using System;
//using System.Drawing;
using System.ComponentModel;
//using System.Windows.Forms;
using System.Messaging;



namespace Nistec.Messaging
{

    /// <summary>
    /// Exception that framework  can raise for message
    /// </summary>
    [Serializable]
    public class MessageException : ApplicationException
    {
        MessageState _AcknowledgeStatus;
        /// <summary>
        /// MessageException
        /// </summary>
        /// <param name="ack"></param>
        /// <param name="msg"></param>
        public MessageException(MessageState ack, string msg)
            : base(msg)
        {
            _AcknowledgeStatus = ack;
        }
        /// <summary>
        /// AcknowledgStatus
        /// </summary>
        public MessageState MessageState
        {
            get { return _AcknowledgeStatus; }
        }
    }
   
}


