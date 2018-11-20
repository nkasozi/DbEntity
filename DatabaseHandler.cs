
using DbEntity;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;


public static class DbEntityDbHandler
{
    private const string _DB_ENITY_CONSTRING_NAME = "DbEntityConnectionString";
    private static string _connectionString = "";
    private static Database _database = null;

    public static DataTable ExecuteStoredProc(string StoredProc, params object[] parameters)
    {
        DataTable dt = new DataTable();
        try
        {
            DbInitializer.ThrowExceptionIfInitailizationWasNotSuccessfull();
            DbCommand procommand = _database.GetStoredProcCommand(StoredProc, parameters);
            dt = _database.ExecuteDataSet(procommand).Tables[0];

            return dt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static dynamic[] ExecuteStoredProcDynamically(string StoredProc, params object[] parameters)
    {
        List<dynamic> objects = new List<dynamic>();
        DataTable dt = new DataTable();
        try
        {
            DbInitializer.ThrowExceptionIfInitailizationWasNotSuccessfull();
            DbCommand procommand = _database.GetStoredProcCommand(StoredProc, parameters);
            dt = _database.ExecuteDataSet(procommand).Tables[0];
            foreach(DataRow row in dt.Rows)
            {
                dynamic drow = new DynamicDataRow(row);
                objects.Add(drow);
            }
            return objects.ToArray();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private static bool InitDB()
    {
        //no connection string was set prior to calling this guy
        if (string.IsNullOrEmpty(_connectionString)) { throw new Exception($"Connection string {_connectionString} cant be NULL or EMPTY"); }

        //if the database is null then, create a new connection
        _database = _database ?? new Microsoft.Practices.EnterpriseLibrary.Data.Sql.SqlDatabase(_connectionString);
        return true;
    }

    public static DataSet ExecuteSqlQuery(string sqlQuery)
    {
        DataSet dt = new DataSet();
        try
        {
            //DbInitializer.ThrowExceptionIfNoSuccessfullInit();
            DbCommand procommand = _database.GetSqlStringCommand(sqlQuery);
            dt = _database.ExecuteDataSet(procommand);

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
            //DbInitializer.ThrowExceptionIfNoSuccessfullInit();
            DbCommand procommand = _database.GetSqlStringCommand(sqlQuery);
            dt = _database.ExecuteNonQuery(procommand);

            return dt;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static int ExecuteNonQuery(string storedProc, params object[] parameters)
    {
        int rowsAffected = 0;
        try
        {
            DbInitializer.ThrowExceptionIfInitailizationWasNotSuccessfull();
            DbCommand procommand = _database.GetStoredProcCommand(storedProc, parameters);
            rowsAffected = _database.ExecuteNonQuery(procommand);
            return rowsAffected;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static bool SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        _database = null;
        return InitDB();
    }

    public static string GetConnectionString()
    {
        return _connectionString;
    }

    public static DataSet ExecuteDataSet(string storedProc,params object[] parameters)
    {
        DataSet ds = new DataSet();
        try
        {
            DbInitializer.ThrowExceptionIfInitailizationWasNotSuccessfull();
            DbCommand procommand = _database.GetStoredProcCommand(storedProc, parameters);
            ds = _database.ExecuteDataSet(procommand);
            return ds;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


}

