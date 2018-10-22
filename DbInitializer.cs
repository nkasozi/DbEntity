using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace DbEntity
{
    public class DbInitializer:DbInitializerBase
    {
        
        //Initialize Db connection only...no updates done
        public static DbResult Initialize()
        {
            DbResult apiResult = new DbResult();

            try
            {
                AutoFindTypesToKeepTrackOf();
                SetConnectionStringInDatabaseHandler();

                apiResult = CreateStoredProcedures();

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                IConfigurationSource source = ConfigurationManager.GetSection(ACTIVE_RECORD_SECTION_NAME) as IConfigurationSource;
                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
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



        //Creates the Db if it doesnt exist,updates the db schema and initializes the db connection
        public static DbResult CreateDbIfNotExistsAndUpdateSchema()
        {
            DbResult apiResult = new DbResult();

            try
            {
                AutoFindTypesToKeepTrackOf();

                apiResult = ExecuteCreateDatabaseSQLIfNotExists();

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                apiResult = CreateStoredProcedures();

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                IConfigurationSource source = ConfigurationManager.GetSection(ACTIVE_RECORD_SECTION_NAME) as IConfigurationSource;
                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                ActiveRecordStarter.UpdateSchema();
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
        public static DbResult InitializeAndUpdateSchema()
        {
            DbResult apiResult = new DbResult();

            try
            {
                AutoFindTypesToKeepTrackOf();

                SetConnectionStringInDatabaseHandler();

                apiResult = CreateStoredProcedures();

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                IConfigurationSource source = ConfigurationManager.GetSection(ACTIVE_RECORD_SECTION_NAME) as IConfigurationSource;

                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                ActiveRecordStarter.UpdateSchema();
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

        //Drops the database if its there, creates the database, updates the db schema and initializes the db connection
        public static DbResult DropAndRecreateDb()
        {
            DbResult apiResult = new DbResult();

            try
            {
                AutoFindTypesToKeepTrackOf();

                SetConnectionStringInDatabaseHandler();

                apiResult = ExecuteDropDatabaseSQLIfExists();

                if (apiResult.StatusCode != DbGlobals.SUCCESS_STATUS_CODE)
                    return apiResult;

                return CreateDbIfNotExistsAndUpdateSchema();
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

        //checks if the db connection has ever been set
        public static bool ThrowExceptionIfNoSuccessfullInit()
        {
            if (!_is_init_successfull)
            {
                throw new Exception($"Db not Initialized. Please Use [{nameof(Initialize)}] or any other Initialize Methods at App Start");
            }

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
        internal const string StoredProcForGettingParameterNames = "GetStoredProcParametersInOrder";
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
