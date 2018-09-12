using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Collections;

public class NormalDatabaseHandler
{
    private Database MMoneyDb;
    private DbCommand procommand;
    private DataTable datatable;
    public static string MoMoQueue = @".\private$\MomoTestQueue3";
    private string ConString = "DbEntityConnectionString";
    // private string ConString = "LiveMMoneyDb";

    public NormalDatabaseHandler()
    {
        MMoneyDb = DatabaseFactory.CreateDatabase(ConString);
        if (ConString == "LiveMMoneyDb")
        {
            MoMoQueue = @".\private$\livemobilemoneyqueue";
        }
        else
        {
            MoMoQueue = @".\private$\MomoTestQueue3";
        }

        //MMoneyDb = DatabaseFactory.CreateDatabase("LiveMMoneyDb");
    }
}
