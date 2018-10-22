using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace DbEntity
{
    public class DbInitializerBase
    {
        //constants
        protected const string EXCEPTION_LEAD_STRING = "EXCEPTION:";
        protected const string ACTIVE_RECORD_SECTION_NAME = "activerecord";
        protected const string ACTIVE_RECORD_SUB_SECTION_NAME = "config";
        protected const string CON_STRING_KEY_NAME = "connection.connection_string";

        //variables
        protected static bool _is_init_successfull = false;
        protected static bool _is_init_of_storedProcs_successfull = false;


        //public variables
        public static List<Type> TypesToKeepTrackOf = new List<Type>();

        public static bool WasInitOfStoredProcSuccessfull()
        {
            return _is_init_of_storedProcs_successfull;
        }

        //find all types that inherit from ActiveRecordBase
        //using reflection
        public static DbResult LoadTypesToKeepTrackOfFromAssembly(Assembly assembly)
        {
            DbResult dbResult = new DbResult();
            try
            {
                List<Type> types_that_inherit_from_activeRecord = FindDerivedTypes(assembly, typeof(ActiveRecordBase)).ToList();
                TypesToKeepTrackOf.AddRange(types_that_inherit_from_activeRecord);
                dbResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                dbResult.StatusDesc = DbGlobals.SUCCESS_STATUS_TEXT;
            }
            catch (Exception ex)
            {
                dbResult.StatusCode = DbGlobals.FAILURE_STATUS_CODE;
                dbResult.StatusDesc = $"EXCEPTION: {ex.Message}";
            }
            return dbResult;
        }


        //find all types that inherit from ActiveRecordBase
        //using reflection
        protected static DbResult AutoFindTypesToKeepTrackOf()
        {
            DbResult dbResult = new DbResult();
            try
            {
                List<Type> types_that_inherit_from_activeRecord = FindDerivedTypes(Assembly.GetEntryAssembly(), typeof(ActiveRecordBase)).ToList();
                TypesToKeepTrackOf.AddRange(types_that_inherit_from_activeRecord);
                dbResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                dbResult.StatusDesc = DbGlobals.SUCCESS_STATUS_TEXT;
            }
            catch (Exception ex)
            {
                dbResult.StatusCode = DbGlobals.FAILURE_STATUS_CODE;
                dbResult.StatusDesc = $"EXCEPTION: {ex.Message}";
            }
            return dbResult;
        }

        protected static IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
        }

        //sets the constring to whatever is read from the config file 
        protected static DbResult SetConnectionStringInDatabaseHandler()
        {
            DbResult dbResult = new DbResult();
            string dbConnectionString = ReadConnectionFromConfig();

            bool con_string_was_set = DbEntityDbHandler.SetConnectionString(dbConnectionString);

            if (con_string_was_set)
            {
                dbResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                dbResult.StatusDesc = DbGlobals.SUCCESS_STATUS_TEXT;
                return dbResult;
            }

            dbResult.StatusCode = DbGlobals.FAILURE_STATUS_CODE;
            dbResult.StatusDesc = "FAILED TO SET CONNECTION STRING";
            return dbResult;
        }

        //creates any initial stored procedures necessary
        protected static DbResult CreateStoredProcedures()
        {
            DbResult apiResult = new DbResult();

            try
            {
                string createSql = $"create proc {DbGlobals.StoredProcForGettingParameterNames}" +
                                    " @StoredProcName varchar(200)" +
                                    " as" +
                                    " Begin" +
                                       " select" +
                                       " 'Parameter_name' = name," +
                                       " 'Type' = type_name(user_type_id)," +
                                       " 'Param_order' = parameter_id" +
                                       " from sys.parameters where object_id = object_id(@StoredProcName)" +
                                       " order by Param_order asc" +
                                    " End";
                int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(createSql);
                _is_init_of_storedProcs_successfull = true;
                apiResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                string msg = ex.Message.ToUpper();
                if (msg.Contains("ALREADY") || msg.Contains("EXISTS"))
                {
                    _is_init_of_storedProcs_successfull = true;
                    apiResult.SetSuccessAsStatusInResponseFields();
                }
                else
                {
                    apiResult.SetSuccessAsStatusInResponseFields();//($"ERROR: UNABLE TO CREATE NECESSARY STORED PROC's: {msg}");
                }
            }
            return apiResult;
        }

        //creates the Db if it doesnt exists
        protected static DbResult ExecuteCreateDatabaseSQLIfNotExists()
        {
            DbResult apiResult = new DbResult();

            try
            {
                string connectionString = ReadConnectionFromConfig();
                try
                {
                    string databaseName = connectionString?.Split(';')?.Where(i => i.ToUpper().Contains("CATALOG"))?.FirstOrDefault()?.Split('=')?[1];
                    string newConnectionString = connectionString.Replace(databaseName, "master");

                    bool isSet = DbEntityDbHandler.SetConnectionString(newConnectionString);

                    if (!isSet)
                    {
                        apiResult.SetFailuresAsStatusInResponseFields($"ERROR: UNABLE TO SET NEW CONNECTION STRING IN CONFIG FILE INORDER TO CREATE DATABASE");
                        return apiResult;
                    }

                    string createSQL = $"Create Database {databaseName}";
                    int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(createSQL);
                    apiResult.SetSuccessAsStatusInResponseFields();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message.ToUpper();
                    if (msg.Contains("ALREADY") || msg.Contains("EXISTS"))
                    {
                        _is_init_of_storedProcs_successfull = true;
                        apiResult.SetSuccessAsStatusInResponseFields();
                    }
                    else
                    {
                        apiResult.SetFailuresAsStatusInResponseFields($"ERROR: {msg}");
                    }
                }

                DbEntityDbHandler.SetConnectionString(connectionString);
                return apiResult;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                apiResult.SetFailuresAsStatusInResponseFields($"ERROR: {msg}");
            }
            return apiResult;
        }

        //drops the Db if it exists
        protected static DbResult ExecuteDropDatabaseSQLIfExists()
        {
            DbResult apiResult = new DbResult();

            try
            {
                string connectionString = ReadConnectionFromConfig();

                try
                {
                    string databaseName = connectionString?.Split(';')?.Where(i => i.ToUpper().Contains("CATALOG"))?.FirstOrDefault()?.Split('=')?[1];
                    string newConnectionString = connectionString.Replace(databaseName, "master");

                    bool isSet = DbEntityDbHandler.SetConnectionString(newConnectionString);

                    if (!isSet)
                    {
                        apiResult.SetFailuresAsStatusInResponseFields($"ERROR: UNABLE TO SET NEW CONNECTION STRING IN CONFIG FILE INORDER TO CREATE DATABASE");
                        return apiResult;
                    }

                    //switch to the master db first ie. u cant drop a db if u are using it
                    //change the db to single use ie. close existing connections
                    //then we can drop it
                    string useMasterSQL = "use master";
                    int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(useMasterSQL);
                    string alterSQL = $"ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    rowsAffected = DbEntityDbHandler.ExecuteNonQuery(alterSQL);
                    string dropSQL = $"Drop Database {databaseName}";
                    rowsAffected = DbEntityDbHandler.ExecuteNonQuery(dropSQL);
                    apiResult.SetSuccessAsStatusInResponseFields();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    apiResult.SetFailuresAsStatusInResponseFields($"ERROR: {msg}");
                }

                //rollback stuff
                DbEntityDbHandler.SetConnectionString(connectionString);
                return apiResult;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                apiResult.SetFailuresAsStatusInResponseFields($"ERROR: {msg}");
            }
            return apiResult;
        }

        //read the constring from the config file supplied
        protected static string ReadConnectionFromConfig()
        {

            //get config file path
            //open and read file path to the section containing active record
            //read the connection string value in "connection.connection_string"
            string connectionStringKey = "connection.connection_string";
            string pathToActiveAppConfig = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            //read the config file using linq
            //config file has tree structure like Configuration(Root)=>activerecord=>config=>*add
            //the add element with the connection string value is the one we want
            XDocument doc = XDocument.Load(pathToActiveAppConfig);
            string dbConnectionString = doc.Root?.Elements("activerecord")?.
                         Elements("config")?.
                         Elements("add")?.
                         Where(i => i.HasAttributes && i.Attribute("key").Value == connectionStringKey).FirstOrDefault()?.
                         Attribute("value")?.Value;

            dbConnectionString = dbConnectionString ?? throw new Exception($"No Connection String Value {connectionStringKey} found Defined in config file {pathToActiveAppConfig}");
            return dbConnectionString;
        }
    }
}
