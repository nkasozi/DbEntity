I know it, You know it, You are tired. After 50 years or so, Database Integration with mainstream code just sucks\
- Tired of hustling with the database. Translating database objects into yours.\
- You use an ORM like Entity framework but it writes terrible SQL.\
- You understand that many times your stored procedures are way faster than any SQL that orm will spill out and invoking the stored  proc using ORM is a pain.

Db Entity is for those people who are tired of shitty Complex ORMs. Basically you are the sort of engineer who wants fine grained\
control with all the speed that ORMs give you. You value speed and clarity over other shit. DbEntity rides on top of the popular\
Castle Active Record ORM (which also rides on top on Nhibernate)

Do you write such code over and over again (I hate this shit)

var command =   CbDatabase.GetStoredProcCommand("Accounts_SelectRow", objectId,BankCode);\
datatable = CbDatabase.ExecuteDataSet(command).Tables[0];\

if (datatable.Rows.Count > 0)\
{\
   DataRow dr = datatable.Rows[0];\
   string IsActive = dr["IsActive"].ToString();\
   account.AccountBalance = dr["AccBalance"].ToString();\
   account.AccountId = dr["AccountId"].ToString();\
   account.BankCode = dr["BankCode"].ToString();\
   account.AccountNumber = dr["AccNumber"].ToString();\
   account.AccountType = dr["AccType"].ToString();\
   account.IsActive = dr["IsActive"].ToString();\
   account.BranchCode = dr["BranchCode"].ToString();\
   account.ModifiedBy = dr["ModifiedBy"].ToString();\
   account.CurrencyCode = dr["CurrencyCode"].ToString();\
   account.ApprovedBy = dr["ApprovedBy"].ToString();\
   account.AccountName = dr["AccountName"].ToString();\
   account.MinimumBalance = dr["MinimumBalance"].ToString();\
   account.InterestRate = dr["InterestRate"].ToString();\
   account.InterestType = dr["InterestType"].ToString();\
}

Or this shit (Entity Framework...FuckKKK!!!)

using (var ctx = new SchoolDBEntities())\
{\
   var idParam = new SqlParameter {\
            ParameterName = "StudentID",\
            Value = 1\
   };\
   //Get student name of string type\
   var courseList = ctx.Database.SqlQuery<Course>("exec GetCoursesByStudentId @StudentId ", idParam).ToList<Course>();

   //Or can call SP by following way\
   //var courseList = ctx.Courses.SqlQuery("exec GetCoursesByStudentId @StudentId ", idParam).ToList<Course>();

   foreach (Course cs in courseList)\
       Console.WriteLine("Course Name: {0}",cs.CourseName);\
}

With Dbentity you can replace all that bull shit with this 1 line

Item item = Item.QueryWithStoredProc("GetItemById",3).FirstOrDefault;

You take this further you can also do this

Item item = new Item\
 {\
     ItemCode = "ITEM-" + DateTime.Now.Ticks.ToString(),\
     ItemName = "Shoes"\
 };

int rowsAffected = item.SaveWithStoredProc("SaveItem",item.ItemCode,item.ItemName);

Simplicity!!

Now check this out...Db Entity can auto fill, the stored procedure parameters in the correct order all on its own. It does that by matching the expected stored proc parameters to the objects properties. So you can do something cool like this..

Item item = new Item\
{\
        ItemCode = "ITEM-" + DateTime.Now.Ticks.ToString(),\
        ItemName = "Shoes"\
};

int rowsAffected = item.SaveWithStoredProcAutoParams("SaveItem");

How about a complex Type...one which is not mapped to any object in your project

dynamic[] all = DbEntityDbHandler.ExecuteStoredProcDynamically("GetAllItems");\
 string itemName = all.FirstOrDefault()?.ItemCode;\
Console.WriteLine(itemName); //dynamic shit is resolved at run time

That's it, We are done. If you have read this far then you might just see the need.

Ok So how do you get it in your app. Well nuget of course

Install-Package DbEntity
