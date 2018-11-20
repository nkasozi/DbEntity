using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Framework.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace DbEntity
{
    public class DbInitializer : DbInitializerBase
    {

        //Initialize Db connection only...no updates done
        public static DbResult Initialize(string ConnectionString = null)
        {
            DbResult dbResult = new DbResult();

            try
            {
                //attempt to determine the types to keep track of automatically
                //by reflection (op result doesnt really matter)
                AutoFindTypesToKeepTrackOf();

                dbResult = SetConnectionStringInDatabaseHandler(ConnectionString);

                //failed to set con string
                //we stop here since setting the con string is necessary to contine
                if (dbResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return dbResult;

                //try to create stored procedures for fetching 
                //parameters for any other stored proc
                //this makes calls to any AutoParams method much faster
                //if its successfull otherwise we default to executing raw sql
                CreateStoredProcedures();

                var source = GetConfigurationSource(ConnectionString);

                //initialize active record
                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                _is_init_successfull = true;

                //all is good
                dbResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                //they have called the initialize method again
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
                    _is_init_successfull = true;
                    dbResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                    dbResult.StatusDesc = "SUSPECTED DOUBLE INITIALIZE: " + ex.Message;
                }
                //some other failure
                else
                {
                    dbResult.SetFailuresAsStatusInResponseFields(EXCEPTION_LEAD_STRING + ex.Message);
                }
            }

            return dbResult;
        }

        private static IConfigurationSource GetConfigurationSource(string ConnectionString = null)
        {
            //read from the app config file if
            //no connection string specified
            if (string.IsNullOrEmpty(ConnectionString))
                return ConfigurationManager.GetSection(ACTIVE_RECORD_SECTION_NAME) as IConfigurationSource;

            //use inplace config for active record
            //to create a connection string
            var properties = new Dictionary<string, string>
                {
                    { "connection.driver_class", "NHibernate.Driver.SqlClientDriver" },
                    { "dialect", "NHibernate.Dialect.MsSql2000Dialect" },
                    { "connection.provider", "NHibernate.Connection.DriverConnectionProvider" },
                    { "connection.connection_string", ConnectionString }
                };


            var source = new InPlaceConfigurationSource();
            source.Add(typeof(ActiveRecordBase), properties);
            return source;
        }



        //Creates the Db if it doesnt exist, updates the db schema and initializes the db connection
        public static DbResult CreateDbIfNotExistsAndUpdateSchema(string DbConnectionString = null)
        {
            DbResult apiResult = new DbResult();

            try
            {
                AutoFindTypesToKeepTrackOf();

                //create the db 
                apiResult = ExecuteCreateDatabaseSQLIfNotExists(DbConnectionString);

                //we failed to create the db
                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                //try to create stored procedures for fetching 
                //parameters for any other stored proc
                //this makes calls to any AutoParams method much faster
                //if its successfull otherwise we default to executing raw sql
                CreateStoredProcedures();

                IConfigurationSource source = GetConfigurationSource(DbConnectionString);

                //initialize active record
                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                ActiveRecordStarter.UpdateSchema();

                //we are all good
                _is_init_successfull = true;
                apiResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
                    _is_init_successfull = true;
                    apiResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                    apiResult.StatusDesc = "SUSPECTED DOUBLE INITIALIZE: " + ex.Message;
                }
                else
                {
                    apiResult.SetFailuresAsStatusInResponseFields(EXCEPTION_LEAD_STRING + ex.Message);
                }
            }

            return apiResult;
        }

        //Updates the db schema and initializes the db connection
        public static DbResult InitializeAndUpdateSchema(string DbConnectionString = null)
        {
            DbResult apiResult = new DbResult();

            try
            {
                AutoFindTypesToKeepTrackOf();

                apiResult = SetConnectionStringInDatabaseHandler(DbConnectionString);

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                //try to create stored procedures for fetching 
                //parameters for any other stored proc
                //this makes calls to any AutoParams method much faster
                //if its successfull otherwise we default to executing raw sql
                CreateStoredProcedures();

                IConfigurationSource source = GetConfigurationSource(DbConnectionString);

                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                ActiveRecordStarter.UpdateSchema();

                //we are all good
                _is_init_successfull = true;
                apiResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                //double call on the init method of Active Record
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
                    _is_init_successfull = true;
                    apiResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                    apiResult.StatusDesc = "SUSPECTED DOUBLE INITIALIZE: " + ex.Message;
                }
                else
                {
                    apiResult.SetFailuresAsStatusInResponseFields(EXCEPTION_LEAD_STRING + ex.Message);
                }
            }

            return apiResult;
        }

        //Drops the database if its there, creates the database, updates the db schema and initializes the db connection
        public static DbResult DropAndRecreateDb(string DbConnectionString = null)
        {
            DbResult dbResult = new DbResult();

            try
            {
                AutoFindTypesToKeepTrackOf();

                dbResult = SetConnectionStringInDatabaseHandler(DbConnectionString);

                if (dbResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return dbResult;

                dbResult = ExecuteDropDatabaseSQLIfExists(DbConnectionString);

                if (dbResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return dbResult;

                return CreateDbIfNotExistsAndUpdateSchema(DbConnectionString);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
                    _is_init_successfull = true;
                    dbResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                    dbResult.StatusDesc = "SUSPECTED DOUBLE INITIALIZE: " + ex.Message;
                }
                else
                {
                    dbResult.SetFailuresAsStatusInResponseFields(EXCEPTION_LEAD_STRING + ex.Message);
                }
            }

            return dbResult;
        }

        //checks if one of the initialize methods has ever been called successfully
        public static bool ThrowExceptionIfInitailizationWasNotSuccessfull()
        {
            //this flag is set to false if no
            //successfull call to any init method fails
            if (!_is_init_successfull)
                throw new Exception($"Db not Initialized. Please Use [{nameof(Initialize)}] or any other Initialize Methods at App Start");

            return true;
        }
    }

    //constants class
    public static class DbGlobals
    {
        //publics
        public const string SUCCESS_STATUS_CODE = "0";
        public const string FAILURE_STATUS_CODE = "100";
        public const string SUCCESS_STATUS_TEXT = "SUCCESS";

        //internals...not be visibile outside this project
        internal const string NameOfStoredProcToGetParameterNames = "GetStoredProcParametersInOrder";
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
