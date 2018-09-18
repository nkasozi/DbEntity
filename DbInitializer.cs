using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;

namespace DbEntity
{
    public class DbInitializer
    {
        //constants
        private const string ExceptionLeadString = "EXCEPTION:";
        private const string SectionName = "activerecord";
        private const string SubSectionName = "config";
        private const string ConStringKeyName = "connection.connection_string";

        //variables
        private static bool IsInitSuccessfull = false;
        internal static bool IsInitOfStoredProcSuccessfull = false;
        internal const string StoredProcForGettingParameterNames = "GetStoredProcParametersInOrder";

        //public variables
        public static List<Type> TypesToKeepTrackOf = new List<Type>();

        //Initialize Db connection only...no updates done
        public static DbResult Initialize()
        {
            DbResult apiResult = new DbResult();

            try
            {
                SetConnectionStringInDatabaseHandler();

                apiResult = CreateStoredProcedures();

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                IConfigurationSource source = ConfigurationManager.GetSection(SectionName) as IConfigurationSource;
                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                IsInitSuccessfull = true;
                apiResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
                    IsInitSuccessfull = true;
                    apiResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                    apiResult.StatusDesc = "SUSPECTED DOUBLE INITIALIZE: " + ex.Message;
                }
                else
                {
                    apiResult.SetFailuresAsStatusInResponseFields(ExceptionLeadString + ex.Message);
                }
            }

            return apiResult;
        }

        //Creates the Db if it doesnt exist,updates the db schema and initializes the db connection
        public static DbResult CreateDbIfNotExistsAndUpdateSchema()
        {
            DbResult apiResult = new DbResult();

            try
            {
                apiResult = CreateDatabaseIfNotExists();

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                apiResult = CreateStoredProcedures();

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                IConfigurationSource source = ConfigurationManager.GetSection(SectionName) as IConfigurationSource;
                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                ActiveRecordStarter.UpdateSchema();
                IsInitSuccessfull = true;
                apiResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
                    IsInitSuccessfull = true;
                    apiResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                    apiResult.StatusDesc = "SUSPECTED DOUBLE INITIALIZE: " + ex.Message;
                }
                else
                {
                    apiResult.SetFailuresAsStatusInResponseFields(ExceptionLeadString + ex.Message);
                }
            }

            return apiResult;
        }

        //Updates the db schema and initializes the db connection
        public static DbResult InitializeAndUpdateSchema()
        {
            DbResult apiResult = new DbResult();

            try
            {
                SetConnectionStringInDatabaseHandler();

                apiResult = CreateStoredProcedures();

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                IConfigurationSource source = ConfigurationManager.GetSection(SectionName) as IConfigurationSource;

                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                ActiveRecordStarter.UpdateSchema();
                IsInitSuccessfull = true;
                apiResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
                    IsInitSuccessfull = true;
                    apiResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                    apiResult.StatusDesc = "SUSPECTED DOUBLE INITIALIZE: " + ex.Message;
                }
                else
                {
                    apiResult.SetFailuresAsStatusInResponseFields(ExceptionLeadString + ex.Message);
                }
            }

            return apiResult;
        }

        //Drops the database if its there, creates the database, updates the db schema and initializes the db connection
        public static DbResult DropAndRecreateDb()
        {
            DbResult apiResult = new DbResult();

            try
            {
                SetConnectionStringInDatabaseHandler();

                apiResult = DropDatabaseIfExists();

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                return CreateDbIfNotExistsAndUpdateSchema();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
                    IsInitSuccessfull = true;
                    apiResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                    apiResult.StatusDesc = "SUSPECTED DOUBLE INITIALIZE: " + ex.Message;
                }
                else
                {
                    apiResult.SetFailuresAsStatusInResponseFields(ExceptionLeadString + ex.Message);
                }
            }

            return apiResult;
        }

        //checks if the db connection has ever been set
        public static bool ThrowExceptionIfNoSuccessfullInit()
        {
            if (!IsInitSuccessfull)
            {
                throw new Exception($"Db not Initialized. Please Use [{nameof(DbInitializer.Initialize)}] or any other Initialize Methods at App Start");
            }

            return true;
        }

        //sets the constring to whatever is read from the config file 
        private static void SetConnectionStringInDatabaseHandler()
        {
            string dbConnectionString = ReadConnectionFromConfig();
            DbEntityDbHandler.SetConnectionString(dbConnectionString);
        }

        //creates any initial stored procedures necessary
        private static DbResult CreateStoredProcedures()
        {
            DbResult apiResult = new DbResult();

            try
            {
                string createSql = $"create proc {StoredProcForGettingParameterNames}" +
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
                IsInitOfStoredProcSuccessfull = true;
                apiResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                string msg = ex.Message.ToUpper();
                if (msg.Contains("ALREADY") || msg.Contains("EXISTS"))
                {
                    IsInitOfStoredProcSuccessfull = true;
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
        private static DbResult CreateDatabaseIfNotExists()
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
                        IsInitOfStoredProcSuccessfull = true;
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
        private static DbResult DropDatabaseIfExists()
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
        private static string ReadConnectionFromConfig()
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

    //constants class
    public class DbGlobals
    {
        public const string SUCCESS_STATUS_CODE = "0";
        public const string FAILURE_STATUS_CODE = "100";
        public const string SUCCESS_STATUS_TEXT = "SUCCESS";
    }

    //response class
    public class DbResult
    {
        public string StatusCode { get; set; }
        public string StatusDesc { get; set; }
        public string PegPayID { get; set; }

        public virtual bool SetSuccessAsStatusInResponseFields()
        {
            StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
            StatusDesc = DbGlobals.SUCCESS_STATUS_TEXT;
            return true;
        }

        public virtual bool SetFailuresAsStatusInResponseFields(string Message)
        {
            StatusCode = DbGlobals.FAILURE_STATUS_CODE;
            StatusDesc = "FAILED: " + Message;
            return true;
        }
    }
}
