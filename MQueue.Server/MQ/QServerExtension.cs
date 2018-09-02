using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Messaging
{
    public static class QServerExtension
    {

        internal static PriorityQueue Factory(this IQProperties prop)
        {
            switch (prop.Mode)
            {
                //case QueueMode.Transactional:
                //    return new PriorityTransQueue(prop.QueueName);
                //case CoverMode.Db:
                //    Assists.Exception_QueueDbNotSupported();
                //    return null;
                //case CoverMode.File:
                //    return new PriorityFileQueue(prop.QueueName);
                case CoverMode.Persistent:
                    return new PriorityPersistQueue(prop);
                case CoverMode.Memory:
                default:
                    return new PriorityMemQueue(prop.QueueName);
            }
        }


        //public static void DoRetry(this QueueItem item)
        //{
        //    item.Retry++;
        //    item.Modified = DateTime.Now;
        //    //item.Header = null;
        //    //m_stream.Replace(Retry, offset + 24);
        //    //m_stream.Replace(Modified.Ticks, offset + 44);
        //}
    }
}
