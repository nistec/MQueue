using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Logging;

namespace Nistec.Messaging
{
    public class QLog //: ILogger
    {
        static readonly ILogger logger = new Logger(true);


        public static void Debug(string message)
        {
            FilteredLog(LoggerLevel.Debug, message, null);
        }
        public static void Info(string message)
        {
            FilteredLog(LoggerLevel.Info, message, null);
        }
        public static void Warn(string message)
        {
            FilteredLog(LoggerLevel.Warn, message, null);
        }
        public static void Error(string message)
        {
            FilteredLog(LoggerLevel.Error, message, null);
        }
        public static void Fatal(string message)
        {
            FilteredLog(LoggerLevel.Fatal, message, null);
        }
        public static void Debug(string format, params object[] args)
        {
            FilteredLog(LoggerLevel.Debug, format, args);
        }
        public static void Debug(string format, bool consoleAsWell, params object[] args)
        {
            FilteredLog(LoggerLevel.Debug, format, args);
        }
        public static void Info(string format, params object[] args)
        {
            FilteredLog(LoggerLevel.Info, format, args);
        }
        public static void Warn(string format, params object[] args)
        {
            FilteredLog(LoggerLevel.Warn, format, args);
        }
        public static void Error(string format, params object[] args)
        {
            FilteredLog(LoggerLevel.Error, format, args);
        }
        public static void Fatal(string format, params object[] args)
        {
            FilteredLog(LoggerLevel.Fatal, format, args);
        }
        public static void Trace(string method, bool begin)
        {
            if (logger.IsEnabled(LoggerLevel.Trace))
            {
                logger.Trace(method, begin);
            }
        }
        public static void Exception(string message, Exception exception)
        {
            if (logger.IsEnabled(LoggerLevel.Error))
            {
                logger.Exception(message, exception, false, false);
            }
        }
        public static void Exception(string message, Exception exception, bool innerException, bool addStackTrace)
        {
            if (logger.IsEnabled(LoggerLevel.Error))
            {
                logger.Exception(message, exception, innerException, addStackTrace);
            }
        }
        private static void FilteredLog(LoggerLevel level, string format, object[] objects)
        {
            if (logger.IsEnabled(level))
            {
                logger.Log(level, format, objects);
            }
        }




        //ILogger log;

        //public QLog()
        //{
        //    log = new Logger(true);
        //}

        //public QLog(NetlogSettings settings)
        //{
        //    log = new Logger(settings);
        //}

        //public bool IsEnabled(LoggerLevel level)
        //{
        //    return log.IsEnabled(level);
        //}
        //public void Log(LoggerLevel level, string format, params object[] args)
        //{
        //    log.Log(level, format, args);
        //}
        //public void Exception(string message, Exception exception, bool innerException, bool addStackTrace)
        //{
        //    log.Exception(message, exception, innerException, addStackTrace);
        //}
        //public void Trace(string method, bool begin)
        //{
        //    log.Trace(method, begin);
        //}
    }
}
