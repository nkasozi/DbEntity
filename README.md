# Why
Stored Proc support in most popular ORMs is terrible. ORM's are great...but the stored proc stuff is terrible. I know it, You know it, You are tired.
- Tired of hustling with the database. Translating database objects into yours.
- You use an ORM like Entity framework but it writes terrible SQL.
- You understand that many times your stored procedures are way faster than any SQL that orm will spill out and invoking the stored  proc using ORM is a pain.

Db Entity is for those people (SQL Server users) who are tired of ORMs with terrible stored proc support. Basically you are the sort of engineer who wants fine grained
control with all the speed that ORMs give you. You value speed and clarity over other stuff. DbEntity rides on top of the popular
Castle Active Record ORM (which also rides on top on Nhibernate)

# Do you write such code over and over again (I hate this stuff)
```
var command =   CbDatabase.GetStoredProcCommand("Accounts_SelectRow", objectId,BankCode);
datatable = CbDatabase.ExecuteDataSet(command).Tables[0];

if (datatable.Rows.Count > 0)
{
   DataRow dr = datatable.Rows[0];
   string IsActive = dr["IsActive"].ToString();
   account.AccountBalance = dr["AccBalance"].ToString();
   account.AccountId = dr["AccountId"].ToString();
   account.BankCode = dr["BankCode"].ToString();
   account.AccountNumber = dr["AccNumber"].ToString();
   account.AccountType = dr["AccType"].ToString();
   account.IsActive = dr["IsActive"].ToString();
   account.BranchCode = dr["BranchCode"].ToString();
   account.ModifiedBy = dr["ModifiedBy"].ToString();
   account.CurrencyCode = dr["CurrencyCode"].ToString();
   account.ApprovedBy = dr["ApprovedBy"].ToString();
   account.AccountName = dr["AccountName"].ToString();
   account.MinimumBalance = dr["MinimumBalance"].ToString();
   account.InterestRate = dr["InterestRate"].ToString();
   account.InterestType = dr["InterestType"].ToString();
}
```
# Or this stuff (Entity Framework!!!)
```
using (var ctx = new SchoolDBEntities())
{
   var idParam = new SqlParameter {
            ParameterName = "StudentID",
            Value = 1
   };
   //Get student name of string type
   var courseList = ctx.Database.SqlQuery<Course>("exec GetCoursesByStudentId @StudentId ", idParam).ToList<Course>();

   //Or can call SP by following way
   //var courseList = ctx.Courses.SqlQuery("exec GetCoursesByStudentId @StudentId ", idParam).ToList<Course>();

   foreach (Course cs in courseList)
       Console.WriteLine("Course Name: {0}",cs.CourseName);
}
```
# With Dbentity you can replace all that stuff with this 1 line
```
Item item = Item.QueryWithStoredProc("GetItemById",3).FirstOrDefault;
```
You take this further you can also do this
```
Item item = new Item
 {
     ItemCode = "ITEM-" + DateTime.Now.Ticks.ToString(),
     ItemName = "Shoes"
 };

int rowsAffected = item.SaveWithStoredProc("SaveItem",item.ItemCode,item.ItemName);
```
Simplicity!!

Now check this out...DbEntity can auto fill, the stored procedure parameters in the correct order all on its own. 
It does that by matching the expected stored proc parameters to the objects properties by reflection. So you can do something cool like this..

```
Item item = new Item
{
        ItemCode = "ITEM-132441",
        ItemName = "Shoes"
};
```
int rowsAffected = item.SaveWithStoredProcAutoParams("SaveItem");

NB: If a stored procedure expects a parameter e.g ItemId and there is no equivalent property in the object then null will be passed as the parameter value
You can echo out the Values passed to the stored proc by doing something like this just after making the call to the stored proc

```

//will have the exact stored proc name and the value sent to the db
Console.WriteLine(item.GetStoredProcedureParametersPassed());

```
# How about a complex Type...one which is not mapped to any object in your project
```
dynamic[] all = DbEntityDbHandler.ExecuteStoredProcDynamically("GetAllItems");
string itemName = all.FirstOrDefault()?.ItemName;
//dynamic stuff is resolved at run time so intellisense wont work
Console.WriteLine(itemName); 
```
That's it, We are done. If you have read this far then you might just see the need.

# Ok So how do you get it in your app. Well Nuget of course
```
Install-Package DbEntity
```
# Config file changes. Remember to change the app.config or web.config file to point to your Database
```
<activerecord>

    <config>
      <add key="connection.driver_class" value="NHibernate.Driver.SqlClientDriver"/>
      <add key="dialect" value="NHibernate.Dialect.MsSql2000Dialect"/>
      <add key="show_sql" value="true"/>
      <add key="format_sql" value="true"/>
      <add key="connection.provider" value="NHibernate.Connection.DriverConnectionProvider"/>
      <add key="connection.connection_string" value="UID=YourUsername;Password=YourPassword;Initial Catalog=YourDatabaseName;Data Source=(local)"/>
      <add value="NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle" key="proxyfactory.factory_class"/>\
    </config>

  </activerecord>
 ```

# Initialization. To Use DbEntity's powers, you need to call the Initialize method at the start of the app(global.asax or something)

```
// pass the array of items you want Active Record to keep track of so you can enjoy stuff like auto create tables
//ability to use the good old Item.Save() sql stuff (which is slow by the way)
 DbInitializer.TypesToKeepTrackOf.AddRange(new Type[] { typeof(Item) });

//just initialize this...nothing fancy
DbResult result = DbInitializer.Initialize();

//will auto update the schema
DbResult result = DbInitializer.InitializeAndUpdateSchema(); 

//will auto create database
DbResult result = DbInitializer.CreateDbIfNotExistsAndUpdateSchema(); 

//will drop the existing db, recreate it and auto update the schema
DbResult result = DbInitializer.DropAndRecreateDb(); 

```
# Class setup follows the castle active record style with a twist...you simply inherit from DbEntity. For a more comprehensive explanation on how to use the full power of castle active record check out http://www.castleproject.org/projects/activerecord/

``` 
[ActiveRecord("Items")]
public class Item : DbEntity<Item>

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
}
```
# What if you can not inherit...maybe your classes are already set in stone. No problem you can use DbEntity like this
```
Item[] items = DbEntity<Item>.QueryWithStoredProc("GetAllItems");
```

