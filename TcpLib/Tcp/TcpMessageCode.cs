using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Tcp
{
    public enum TcpMessageCode
    {
        /// <summary>
        /// 250
        /// </summary>
        Ok = 250,
        /// <summary>
        /// 354
        /// </summary>
        StartMessageInput = 354,//; end with <CRLF>.<CRLF>
        /// <summary>
        /// 401
        /// </summary>
        UnAuthrized = 401,
        /// <summary>
        /// 421
        /// </summary>
        IdleTimeoutClosingConnection = 421,
        /// <summary>
        /// 500
        /// </summary>
        InternalServerError = 500,
        /// <summary>
        /// 501
        /// </summary>
        SyntaxError = 501,
        /// <summary>
        /// 502
        /// </summary>
        ErrorCommand = 502,
        /// <summary>
        /// 503
        /// </summary>
        BadSequenceCommands = 503,
        /// <summary>
        /// 552
        /// </summary>
        MessageSizeExceeds = 552,

    };
}
