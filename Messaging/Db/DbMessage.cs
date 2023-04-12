  using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Data.Entities;
using Nistec.IO;
using Nistec.Data;


namespace Nistec.Messaging.Db
{

  
    /// <summary>
    /// Represent an MessageEntity entity.
    /// </summary>
    [Serializable]
    public class DbMessage : IEntityItem
    {

        #region db properties

        [EntityProperty(EntityPropertyType.Key, Order=0)]
        public long UniqueId{ get; set; }
       
        [EntityProperty(EntityPropertyType.Default, Order=1)]
        public string Host{ get; set; }
       
        [EntityProperty(EntityPropertyType.Default, Order=2)]
        public byte MessageState{ get; set; }
       
        [EntityProperty(EntityPropertyType.Default, Order=3)]
        public byte QCommand{ get; set; }
        
        [EntityProperty(EntityPropertyType.Default, Order=4)]
        public byte Priority{ get; set; }
        
        [EntityProperty(EntityPropertyType.Default, Order=5)]
        public byte Retry{ get; set; }

        [EntityProperty(EntityPropertyType.Default, Order=6)]
        public DateTime ArrivedTime{ get; set; }
        
        [EntityProperty(EntityPropertyType.Default, Order=7)]
        public DateTime Creation{ get; set; }
        
        [EntityProperty(EntityPropertyType.Default, Order=8)]
        public DateTime Modified{ get; set; }

        [EntityProperty(EntityPropertyType.Default, Order=9)]
        public int Expiration{ get; set; }
        
        [EntityProperty(EntityPropertyType.Default, Order=10)]
        public string Identifier{ get; set; }
       
        [EntityProperty(EntityPropertyType.Default, Order=11)]
        public byte[] BodyStream { get; set; }

        #endregion

        public static DbMessage GetEntity(QueueMessage msg)
        {
            return new DbMessage()
            {
                ArrivedTime = msg.ArrivedTime,
                BodyStream = msg.BodyStream.ToArray(),
                Expiration = msg.Expiration,
                Host = msg.Host,
                Identifier = msg.Identifier,
                MessageState = (byte)msg.MessageState,
                QCommand = (byte)msg.QCommand,
                Modified = msg.Modified,
                Priority = (byte)msg.Priority,
                Retry = (byte)msg.Retry,
                Creation = msg.Creation,
                UniqueId =0// msg.UniqueId
            };
        }

    }
}
