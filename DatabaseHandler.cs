
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class DbEntityDbHandler
{
    private const string _DB_ENITY_CONSTRING_NAME = "DbEntityConnectionString";
    private static string _connectionString = "";
    private static Database _database = null;

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
            //InitDB();
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

        //try to update the config file with the new connection string
        //bool isConfigUpdated = CreateConstringInConfig(_connectionString);

        ////error on changing the config file
        //if (!isConfigUpdated) { return isConfigUpdated; }

        //if the database is null then, create a new connection
        _database = _database ?? new Microsoft.Practices.EnterpriseLibrary.Data.Sql.SqlDatabase(_connectionString);
        return true;
    }

    private static bool CreateConstringInConfig(string DbConString)
    {
        // Get the application configuration file.
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        // Create a connection string element and
        // save it to the configuration file.

        // Create a connection string element.
        ConnectionStringSettings csSettings = new ConnectionStringSettings();

        //make sure these guys are set
        csSettings.Name = "DbEntityConnectionString";
        csSettings.ConnectionString = DbConString;
        csSettings.ProviderName = "System.Data.SqlClient";

        // Get the connection strings section.
        ConnectionStringsSection csSection = config.ConnectionStrings;
        

        // Add the new element.
        try
        {
            csSection.ConnectionStrings.Remove(csSettings);
            csSection.ConnectionStrings.Add(csSettings);
        }
        catch (Exception ex)
        {
            return false;
        }

        // Save the configuration file.
        config.Save(ConfigurationSaveMode.Modified);
       
        return true;
    }

    public static DataSet ExecuteSqlQuery(string sqlQuery)
    {
        DataSet dt = new DataSet();
        try
        {
            //InitDB();
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
            //InitDB();
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
            //InitDB();
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
            //InitDB();
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

