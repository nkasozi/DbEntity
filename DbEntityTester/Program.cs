using DbEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntityTester
{
    class Program
    {
        static void Main(string[] args)
        {

            DbResult result = DbInitializer.Initialize();
            //Item[] items3 = Item.QueryWithStoredProc("GetAllItems");
            //DbInitializer.TypesToKeepTrackOf.AddRange(new Type[] { typeof(Item) });
            //DbResult result = DbInitializer.DropAndRecreateDb();
            //DbResult result = DbInitializer.InitializeAndUpdateSchema();
            //result = DbInitializer.Initialize();
            //Item item = new Item
            //{
            //    ItemCode = "ITEM-" + DateTime.Now.Ticks.ToString(),
            //    CreatedBy = "admin",
            //    ItemCount = 10,
            //    ItemName = "Shoes",
            //    ItemPrice = 2000,
            //    ModifiedBy = "admin"
            //};

            //int rowsAffected = item.SaveWithStoredProcAutoParams("SaveItem");
            //dynamic[] all = DbEntityDbHandler.ExecuteStoredProcDynamically("GetAllItems");
            //string itemName = all.FirstOrDefault()?.ItemCode;

            //Item[] items = item.QueryWithStoredProcAutoParams("GetAllItems");
            //Console.WriteLine(item.GetStoredProcedureParametersPassed());
            //Item[] items2 = DbEntity<Item>.QueryWithStoredProc("GetAllItems");
            //NormalDatabaseHandler dh = new NormalDatabaseHandler();
        }
    }
}
