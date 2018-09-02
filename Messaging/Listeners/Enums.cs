using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging.Listeners
{
    ///// <summary>
    ///// Enumerator for adapter operations.
    ///// </summary>
    //public enum AdapterOperations
    //{
    //    ///// <summary>
    //    ///// Async method.
    //    ///// </summary>
    //    //Async,
    //    ///// <summary>
    //    ///// Sync method.
    //    ///// </summary>
    //    //Sync,
    //    Recieve,
    //    //ReceiveRound,
    //    /// <summary>
    //    /// Transfer method.
    //    /// </summary>
    //    Transfer
    //}


    /// <summary>
    /// Enumerator for adapter protocols.
    /// </summary>
    public enum AdapterProtocols
    {
        /// <summary>
        /// Ipc (NamedPipe) protocol
        /// </summary>
        NamedPipe = 1,
        /// <summary>
        /// Tcp protocol
        /// </summary>
        Tcp = 2,
        /// <summary>
        /// Http protocol
        /// </summary>
        Http = 3,
        /// <summary>
        /// File stream protocol.
        /// </summary>
        File = 4,
        /// <summary>
        /// Db connection protocol.
        /// </summary>
        Db = 5
    }
    /// <summary>
    /// Enumerator for File Order Types.
    /// </summary>
    public enum FileOrderTypes
    {
        /// <summary>
        /// Order files by file name.
        /// </summary>
        ByName = 0,
        /// <summary>
        /// Order files by creation time.
        /// </summary>
        ByCreation = 1
    }
}
