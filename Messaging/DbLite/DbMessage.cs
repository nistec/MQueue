using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Data.Entities;
using Nistec.IO;
using Nistec.Data;


namespace Nistec.Messaging.DbSLite
{

  
    /// <summary>
    /// Represent an MessageEntity entity.
    /// </summary>
    [Serializable]
    public class DbLiteMessage : Nistec.Messaging.Db.DbMessage,IEntityItem
    {

        //#region db properties

        //[EntityProperty(EntityPropertyType.Key, Order=0)]
        //public long UniqueId{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=1)]
        //public string Host{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=2)]
        //public byte MessageState{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=3)]
        //public byte Command{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=4)]
        //public byte Priority{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=5)]
        //public byte Retry{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=6)]
        //public DateTime ArrivedTime{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=7)]
        //public DateTime SentTime{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=8)]
        //public DateTime Modified{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=9)]
        //public int Expiration{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=10)]
        //public int MessageId{ get; set; }

        //[EntityProperty(EntityPropertyType.Default, Order=11)]
        //public byte[] BodyStream { get; set; }

        //#endregion

        //public static DbMessage GetEntity(Message msg)
        //{
        //    return new DbMessage()
        //    {
        //        ArrivedTime = msg.ArrivedTime,
        //        BodyStream = msg.BodyStream.ToArray(),
        //        Expiration = msg.Expiration,
        //        Host = msg.Destination,
        //        MessageId = msg.MessageId,
        //        MessageState = (byte)msg.MessageState,
        //        Command = (byte)msg.Command,
        //        Modified = msg.Modified,
        //        Priority = (byte)msg.Priority,
        //        Retry = (byte)msg.Retry,
        //        SentTime = msg.SentTime,
        //        UniqueId =0// msg.UniqueId
        //    };
        //}

        public string QueueName { get; set; }

        internal object[] ItemArray()
        {

            return new object[]{  QueueName,
            MessageState,
            Command,
            Priority,
            UniqueId,
            Retry,
            ArrivedTime,
            Creation,
            Modified,
            Expiration,
            Identifier,
            BodyStream
            };
        }

    }
}
