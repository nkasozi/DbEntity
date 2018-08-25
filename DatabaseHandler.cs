
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class DatabaseHandler
{
    private static string ConnectionString = "";
    private static Database DB = null;

    public static void LogError(string ID, string Message)
    {
        try
        {

        }
        catch (Exception ex)
        {
            throw ex;
        }
        return;
    }

    public static DataTable ExecuteStoredProc(string StoredProc, params object[] parameters)
    {
        DataTable dt = new DataTable();
        try
        {
            InitDB();
            DbCommand procommand = DB.GetStoredProcCommand(StoredProc, parameters);
            dt = DB.ExecuteDataSet(procommand).Tables[0];

            return dt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private static bool InitDB()
    {
        if (string.IsNullOrEmpty(ConnectionString)) { throw new Exception($"Connection string {ConnectionString} cant be NULL or EMPTY"); }
        DB = DB ?? new Microsoft.Practices.EnterpriseLibrary.Data.Sql.SqlDatabase(ConnectionString);
        return true;
    }

    public static DataSet ExecuteSqlQuery(string sqlQuery)
    {
        DataSet dt = new DataSet();
        try
        {
            InitDB();
            DbCommand procommand = DB.GetSqlStringCommand(sqlQuery);
            dt = DB.ExecuteDataSet(procommand);

            return dt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static int ExecuteNonQuery(string sqlQuery)
    {
        int dt = 0;
        try
        {
            InitDB();
            DbCommand procommand = DB.GetSqlStringCommand(sqlQuery);
            dt = DB.ExecuteNonQuery(procommand);

            return dt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static int ExecuteNonQuery(string storedProc, object[] parameters)
    {
        int rowsAffected = 0;
        try
        {
            InitDB();
            DbCommand procommand = DB.GetStoredProcCommand(storedProc, parameters);
            rowsAffected = DB.ExecuteNonQuery(procommand);
            return rowsAffected;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static bool SetConnectionString(string connectionString)
    {
        ConnectionString = connectionString;
        DB = null;
        return true;
    }

    public static DataSet ExecuteDataSet(string storedProc, object[] parameters)
    {
        DataSet ds = new DataSet();
        try
        {
            InitDB();
            DbCommand procommand = DB.GetStoredProcCommand(storedProc, parameters);
            ds = DB.ExecuteDataSet(procommand);
            return ds;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


}

