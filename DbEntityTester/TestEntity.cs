using Castle.ActiveRecord;
using DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntityTester
{
    [ActiveRecord("Items")]
    public class Item : DbEntity<Item>
    {
        [PrimaryKey(PrimaryKeyType.Identity, "RecordId")]
        public int Id { get; set; }

        [Property(Length = 50)]
        public string ItemCode { get; set; }

        [Property(Length = 50)]
        public string ItemName { get; set; }

        [Property(Length = 50)]
        public int ItemPrice { get; set; }

        [Property(Length = 7500)]
        public string ItemImage { get; set; }

        [Property]
        public int ItemCount { get; set; }

        [Property(Length = 50)]
        public string CreatedBy { get; set; }

        [Property(Length = 50)]
        public string ModifiedBy { get; set; }

        [Property(Length = 50)]
        public DateTime CreatedOn { get; set; }

        [Property(Length = 50)]
        public DateTime ModifiedOn { get; set; }
    }
}
