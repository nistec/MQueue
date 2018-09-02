//licHeader
//===============================================================================================================
// System  : Nistec.Queue - Nistec.Queue Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of cache core.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|


//#define ActiveQueueLog

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Linq;
using Nistec.Generic;
using Nistec.Threading;
using Nistec.Logging;

namespace Nistec.Messaging
{

    #region  Log delegate
    /*
    /// <summary>
    /// Log Message EventHandler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void LogMessageEventHandler(object sender, LogMessageEventArgs e);
    
    /// <summary>
    /// Represent log message event arguments.
    /// </summary>
    public class LogMessageEventArgs : EventArgs
    {
        // Fields
        private string message;
        private IAsyncResult result;
        private QLogger sender;

        // Methods
        internal LogMessageEventArgs(QLogger sender, IAsyncResult result)
        {
            this.result = result;
            this.sender = sender;
        }

        /// <summary>
        /// Get message.
        /// </summary>
        public string Message
        {
            get
            {
                if (this.message == null)
                {
                    try
                    {
                        this.message = this.sender.EndLog(this.result);
                    }
                    catch
                    {
                        throw;
                    }
                }
                return this.message;
            }
        }
    }
    */
    #endregion

    /// <summary>
    /// Represent cache logger.
    /// </summary>
    public class QLogger : System.ComponentModel.Component
    {

        #region  members
        internal static bool DebugEnabled = true;
        
        //int logCapacity = 1000;
  
        //static LogService LogService = new LogService(true);

        static QLogger()
        {

        }
       


        #endregion

        #region Logger

        ILogger _ILogger = Nistec.Logging.Logger.Instance;
        /// <summary>
        /// Get or Set Logger that implements <see cref="ILogger"/> interface.
        /// </summary>
        public ILogger ILog { get { return _ILogger; } set { if (value != null)_ILogger = value; } }

        bool EnableLog { get; set; }

        static QLogger _Logger;

        public static QLogger SetLogger(bool enableLog, bool enableDebug)
        {
            if (_Logger == null)
            {
                _Logger = new QLogger(enableLog, enableDebug);
            }
            return _Logger;
        }


        /// <summary>
        /// Get <see cref="CacheLogger"/> instance.
        /// </summary>
        static QLogger Logger
        {
            get
            {
                if (_Logger == null)
                {
                    _Logger = new QLogger(true, true);
                }
                return _Logger;
            }
        }




#if (ActiveQueueLog)

        static readonly Queue<string> logQueue = new Queue<string>();

        private string LogItemWorker(string text)
        {
            string msg = string.Format("{0}: {1}", DateTime.Now, text);
            lock (((ICollection)logQueue).SyncRoot)
            {
                if (logQueue.Count > logCapacity)
                {
                    logQueue.Dequeue();
                }
                logQueue.Enqueue(msg);
            }
            return msg;
        }
        /// <summary>
        /// Clear log.
        /// </summary>
        public void Clear()
        {
            lock (((ICollection)logQueue).SyncRoot)
            {
                logQueue.Clear();
            }
        }

         /// <summary>
        /// Read log as string array.
        /// </summary>
        /// <returns></returns>
        public static string[] ReadLog()
        {
            List<string> copy = null;
            lock (((ICollection)logQueue).SyncRoot)
            {
                copy = logQueue.ToList<string>();
            }
            if (copy == null)
                return new string[] {""};
            copy.Reverse();
            return copy.ToArray();
        }

        /// <summary>
        /// Get log as long string.
        /// </summary>
        /// <returns></returns>
        public string QLog()
        {
            string[] array = ReadLog();
            StringBuilder sb = new StringBuilder();

            foreach (string s in array)
            {
                sb.AppendLine(s);
            }
            return sb.ToString();
        }

        #region write aysnc

        private AsyncCallback onRequestCompleted;
        private ManualResetEvent resetEvent;
        /// <summary>
        /// Log Item Callback delegate
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public delegate string LogItemCallback(LoggerLevel level, string text);
        /// <summary>
        /// Log Completed event
        /// </summary>
        public event LogMessageEventHandler LogMessage;

        /// <summary>
        /// OnLogCompleted
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnLogMessage(LogMessageEventArgs e)
        {
            if (LogMessage != null)
                LogMessage(this, e);
        }

        private string LogItemWorker(LoggerLevel level, string text)
        {
            //string msg = string.Format("{0}: {1}", DateTime.Now, text);
            string msg = string.Format("{0}: {1}", DateTime.Now, level.ToString() + "-" + text);

            ILog.Log(level, text);

            return msg;
        }

        /// <summary>
        /// AsyncLog
        /// </summary>
        /// <returns></returns>
        internal void LogAsync(LoggerLevel level, string text)
        {
            LogItemCallback caller = new LogItemCallback(LogItemWorker);

            // Initiate the asychronous call.
            IAsyncResult result = caller.BeginInvoke(level,text, CreateCallBack(), caller);
            result.AsyncWaitHandle.WaitOne();

            //while (!result.IsCompleted)
            //{
            //    Thread.Sleep(10);
            //}
            // Call EndInvoke to wait for the asynchronous call to complete,
            // and to retrieve the results.
            caller.EndInvoke(result);
        }

        /// <summary>
        /// Begin write to cache logger async.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public IAsyncResult BeginLog(LoggerLevel level, string text)
        {
            return BeginLog(level,text);
        }
        /// <summary>
        /// Begin write to cache logger async.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="callback"></param>
        /// <param name="level"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public IAsyncResult BeginLog(object state, AsyncCallback callback, LoggerLevel level, string text)
        {

            LogItemCallback caller = new LogItemCallback(LogItemWorker);

            if (callback == null)
            {
                callback = CreateCallBack();
            }

            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke(level,text, callback, caller);
            this.resetEvent.Set();
            return result;
        }


        /// <summary>Completes the specified asynchronous receive operation.</summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public string EndLog(IAsyncResult asyncResult)
        {
            // Retrieve the delegate.
            LogItemCallback caller = (LogItemCallback)asyncResult.AsyncState;

            // Call EndInvoke to retrieve the results.
            string msg = (string)caller.EndInvoke(asyncResult);

            return msg;
        }

        private AsyncCallback CreateCallBack()
        {
            if (this.onRequestCompleted == null)
            {
                this.onRequestCompleted = new AsyncCallback(this.OnRequestCompleted);
            }
            return this.onRequestCompleted;
        }


        private void OnRequestCompleted(IAsyncResult asyncResult)
        {
            if (LogMessage != null)
                OnLogMessage(new LogMessageEventArgs(this, asyncResult));
        }

        #endregion
        
#endif

        #region ctor

        /// <summary>
        /// Initialize a new instance of cache logger.
        /// </summary>
        public QLogger(bool enableLog,bool enableDebug)
        {
            DebugEnabled = enableDebug;
            EnableLog = enableLog;
            //resetEvent = new ManualResetEvent(false);
        }
        /// <summary>
        /// Release all resource fro cache logger.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        #endregion

       

        #region write level

        /// <summary>
        /// Write new line to cache logger as info.
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            Logger.WriteLog(LoggerLevel.Info, message);
        }
        /// <summary>
        /// Write new line to cache logger as info.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void InfoFormat(string message, params object[] args)
        {
            Logger.WriteLog(LoggerLevel.Info, message, args);
        }
        /// <summary>
        /// Write new line to cache logger as debug.
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            Logger.WriteLog(LoggerLevel.Debug, message);
        }
        /// <summary>
        ///  Write new line to cache logger as debug.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void DebugFormat(string message, params object[] args)
        {
            Logger.WriteLog(LoggerLevel.Debug, message, args);
        }
        /// <summary>
        ///  Write new line to cache logger as error.
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            Logger.WriteLog(LoggerLevel.Error, message);
        }
        /// <summary>
        ///  Write new line to cache logger as error.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void ErrorFormat(string message, params object[] args)
        {
            Logger.WriteLog(LoggerLevel.Error, message, args);
        }

        public static void Exception(string message, Exception exception)
        {
            Exception(message, exception, false, false);
        }

        public static void Exception(string message, Exception exception, bool addStackTrace = false)
        {
            Exception(message, exception, false, addStackTrace);
        }
        public static void Exception(string message, Exception e, bool innerException, bool addStackTrace)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message + " " + e.Message);


            if (innerException)
            {
                Exception innerEx = e == null ? null : e.InnerException;
                while (innerEx != null)
                {
                    sb.Append(innerEx.Message);
                    innerEx = innerEx.InnerException;
                }
            }
            if (addStackTrace)
            {
                sb.AppendLine();
                sb.AppendFormat("StackTrace:{0}", e.StackTrace);
            }
            Logger.WriteLog(LoggerLevel.Error, sb.ToString());
        }
        #endregion

        void WriteLog(LoggerLevel level, string message, params object[] args)
        {
            string msg = args == null ? message : string.Format(message, args);

            if (EnableLog)
            {
                //LogService.Write(DateTime.Now,level, msg);

                switch (level)
                {
                    case LoggerLevel.Error:
                        ILog.Log(level, msg);
                        break;
                    case LoggerLevel.Debug:
                        if (DebugEnabled)
                            ILog.Log(level, msg);
                        break;
                    case LoggerLevel.Info:
                        ILog.Log(level, msg);
                        break;
                    case LoggerLevel.Warn:
                        ILog.Log(level, msg);
                        break;
                }
            }
            //Log(level.ToString() + "-" + msg);

            //if (QueueSettings.EnableLog)
            //{
            //switch (level)
            //{
            //    case LoggerLevel.Error:
            //        ILog.Error(msg); break;
            //    case LoggerLevel.Debug:
            //        ILog.Debug(msg); break;
            //    case LoggerLevel.Info:
            //        ILog.Info(msg); break;
            //    case LoggerLevel.Warn:
            //        ILog.Warn(msg); break;
            //    //case LoggerLevel.Trace:
            //    //    Netlog.Trace(msg); break;
            //}
            //}

            //Console.WriteLine(msg);
        }
   
#endregion
    }

}
