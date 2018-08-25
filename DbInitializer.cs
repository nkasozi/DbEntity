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
        private const string ExceptionLeadString = "EXCEPTION:";
        private const string SectionName = "activerecord";
        private const string SubSectionName = "config";
        private const string ConStringKeyName = "connection.connection_string";
        public static List<Type> TypesToKeepTrackOf = new List<Type>();

        public static DbResult Initialize()
        {
            DbResult apiResult = new DbResult();

            try
            {
                var conString = ConfigurationManager.AppSettings;
                SetConnectionStringInDatabaseHandler();
                IConfigurationSource source = ConfigurationManager.GetSection(SectionName) as IConfigurationSource;
                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                apiResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
                    apiResult.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                    apiResult.StatusDesc = "SUSPECTED DOUBLE INITIALIZE: "+ex.Message;
                }
                else
                {
                    apiResult.SetFailuresAsStatusInResponseFields(ExceptionLeadString + ex.Message);
                }
            }

            return apiResult;
        }

        public static DbResult InitializeAndUpdateSchema()
        {
            DbResult apiResult = new DbResult();

            try
            {
                var conString = ConfigurationManager.AppSettings;//.ConnectionStrings["connection.connection_string"].ConnectionString;
                SetConnectionStringInDatabaseHandler();
                IConfigurationSource source = ConfigurationManager.GetSection(SectionName) as IConfigurationSource;

                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                ActiveRecordStarter.UpdateSchema();

                apiResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
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

        public static void SetConnectionStringInDatabaseHandler()
        {
            //get config file path
            //open and read file path to the section containing active record
            //read the connection string value in "connection.connection_string"
            string connectionStringKey = "connection.connection_string";
            string pathToActiveAppConfig = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            //read the config file using linq
            //config file has tree structure like Configuration(Root)=>activerecord=>config=>*add
            //the add element with the connection string value is the one we wany
            XDocument doc = XDocument.Load(pathToActiveAppConfig);
            string dbConnectionString = doc.Root?.Elements("activerecord")?.
                         Elements("config")?.
                         Elements("add")?.
                         Where(i => i.HasAttributes && i.Attribute("key").Value == connectionStringKey).FirstOrDefault()?.
                         Attribute("value")?.Value;
            
            dbConnectionString = dbConnectionString ?? throw new Exception ($"No Connection String Value {connectionStringKey} found Defined in config file {pathToActiveAppConfig}");
            DatabaseHandler.SetConnectionString(dbConnectionString);
        }

        public static DbResult DropAndRecreate()
        {
            DbResult apiResult = new DbResult();

            try
            {

                SetConnectionStringInDatabaseHandler();
                IConfigurationSource source = System.Configuration.ConfigurationManager.GetSection(SectionName) as IConfigurationSource;
                ActiveRecordStarter.Initialize(source, TypesToKeepTrackOf.ToArray());
                ActiveRecordStarter.DropSchema();
                ActiveRecordStarter.UpdateSchema();
                apiResult.SetSuccessAsStatusInResponseFields();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("MORE THAN ONCE"))
                {
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
    }

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
