using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nistec.Generic;

namespace Nistec
{
    public class EntityDemo
    {
        public EntityDemo()
        {
        }
        public EntityDemo(int index)
        {
            EntityId = UUID.UniqueId();
            EntityName = "demo";
            EntityAddress = "New york";
            EntityDate = DateTime.Now;
            Index = index;
        }

        public long EntityId { get; set; }
        public string EntityName { get; set; }
        public string EntityAddress { get; set; }
        public DateTime EntityDate { get; set; }
        public int Index { get; set; }

    }
}
