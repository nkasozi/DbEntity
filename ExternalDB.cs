using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Data;
using System.Data.Common;


public class ExternalDB
{
    private Database DB;

    public ExternalDB(string ConnectionString)
    {
        try
        {
            DB = new Microsoft.Practices.EnterpriseLibrary.Data.Sql.SqlDatabase(ConnectionString);
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }

    public DataTable ExecuteStoredProc(string StoredProc, params object[] parameters)
    {
        DataTable dt = new DataTable();
        try
        {
            DbCommand procommand = DB.GetStoredProcCommand(StoredProc, parameters);
            dt = DB.ExecuteDataSet(procommand).Tables[0];

            return dt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public DataSet ExecuteSqlQuery(string sqlQuery)
    {
        DataSet dt = new DataSet();
        try
        {
            DbCommand procommand = DB.GetSqlStringCommand(sqlQuery);
            dt = DB.ExecuteDataSet(procommand);

            return dt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public int ExecuteNonQuery(string sqlQuery)
    {
        int dt = 0;
        try
        {
            DbCommand procommand = DB.GetSqlStringCommand(sqlQuery);
            dt = DB.ExecuteNonQuery(procommand);

            return dt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public DataSet ExecuteDataSet(string storedProc, object[] parameters)
    {
        DataSet ds = new DataSet();
        try
        {
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

