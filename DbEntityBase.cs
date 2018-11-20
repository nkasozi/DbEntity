using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class DbEntityBase<T> : ActiveRecordBase<T> where T : new()
    {
        public string StatusCode { get; set; }
        public string StatusDesc { get; set; }

        //Simple implementation of a Last Recently Used (LRU) cache to hold the last stored paramaters picked from DB 
        //(prevent too many calls to the Db to pick parameters of stored proc)
        protected Dictionary<string, DataSet> _stored_proc_params_dictionary = new Dictionary<string, DataSet>();
        protected string _stored_Proc_Params_that_were_Passed { get; set; }

        protected DbResult CheckIfAllRequiredAttributesAreSet()
        {
            DbResult result = new DbResult();
            try
            {

                var objProperties = GetType().GetProperties();

                //loop thru all obj properties
                foreach (var objProperty in objProperties)
                {
                    try
                    {
                        //get all custom attributes first
                        object[] customAttributes = objProperty.GetCustomAttributes(false);


                        //loop thru for the custom attributes first
                        foreach (object customAttribute in customAttributes)
                        {

                            //primary key attribute
                            //e.g [PrimaryKey(PrimaryKeyType.Identity, "RecordId")]
                            if (customAttribute is RequiredAttribute pkAttribute)
                            {
                                if (objProperty == null)
                                {
                                    result.StatusCode = DbGlobals.FAILURE_STATUS_CODE;
                                    result.StatusDesc = $"{objProperty.Name} is Required";
                                    return result;
                                }

                                if (objProperty.PropertyType == typeof(string) && string.IsNullOrEmpty(objProperty.GetValue(this) as string))
                                {
                                    result.StatusCode = DbGlobals.FAILURE_STATUS_CODE;
                                    result.StatusDesc = $"{objProperty.Name} is Required";
                                    return result;
                                }
                            }
                        }

                    }
                    catch (Exception)
                    {

                    }
                }

                result.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
                result.StatusDesc = DbGlobals.SUCCESS_STATUS_TEXT;
                return result;
            }
            catch (Exception ex)
            {
                result.StatusCode = DbGlobals.FAILURE_STATUS_CODE;
                result.StatusDesc = $"ERROR: {ex.Message}";
                return result;
            }
        }

        protected static void CopyDataRowValuesToObjectProperties(DataRow parent, T obj)
        {
            var objProperties = obj.GetType().GetProperties();

            //loop thru all obj properties
            foreach (var objProperty in objProperties)
            {
                try
                {
                    //get all custom attributes first
                    object[] customAttributes = objProperty.GetCustomAttributes(false);

                    //flag to be set when a property has been set
                    bool hasBeenSet = false;

                    //loop thru for the custom attributes first
                    foreach (object customAttribute in customAttributes)
                    {

                        //primary key attribute
                        //e.g [PrimaryKey(PrimaryKeyType.Identity, "RecordId")]
                        if (customAttribute is PrimaryKeyAttribute pkAttribute)
                        {
                            string column = pkAttribute.Column;
                            if (column != null)
                            {
                                objProperty.SetValue(obj, parent[column], new object[] { });
                                hasBeenSet = true;
                                break;
                            }
                        }

                        //property attribute
                        //e.g [Property(Length = 50, Column="MyColumn")]
                        if (customAttribute is PropertyAttribute propertyAttribute)
                        {
                            string column = propertyAttribute.Column;
                            if (column != null)
                            {
                                objProperty.SetValue(obj, parent[column], new object[] { });
                                hasBeenSet = true;
                                break;
                            }
                        }
                    }

                    //this is a custom attribute...it has been set in the for loop
                    if (hasBeenSet)
                        continue;

                    //normal obj property
                    objProperty.SetValue(obj, parent[objProperty.Name], new object[] { });
                }
                catch (Exception ex)
                {
                    //db value is null so we cant convert it to .net value
                    if (ex.Message.ToUpper().Contains("DBNull".ToUpper()))
                        continue;

                    //property in object not found in row returned
                    if (ex.Message.ToUpper().Contains("does not belong to table Table".ToUpper()))
                        continue;

                    //set the status code and failure desc values 
                    TryToSetObjectProperty(obj, nameof(StatusCode), DbGlobals.FAILURE_STATUS_CODE);
                    TryToSetObjectProperty(obj, nameof(StatusDesc), $"FAILED: UNABLE TO SET VALUE FOR PROPERTY [{objProperty.Name}]. REASON: {ex.Message}");
                    return;
                }
            }

            //set the status code and status desc values to success
            TryToSetObjectProperty(obj, nameof(StatusCode), DbGlobals.SUCCESS_STATUS_CODE);
            TryToSetObjectProperty(obj, nameof(StatusDesc), DbGlobals.SUCCESS_STATUS_TEXT);
        }

        protected static void TryToSetObjectProperty(object theObject, string propertyName, object value)
        {
            try
            {
                Type type = theObject.GetType();
                var property = type.GetProperty(propertyName);
                var setter = property.SetMethod;
                setter.Invoke(theObject, new object[] { value });
            }
            catch (Exception)
            {

            }
        }

        protected object[] GetStoredProcParameters(string storedProc)
        {
            List<object> allParameters = new List<object>();

            DataSet ds = null;

            //the parameters for this stored proc have aleady been fetched
            //so we just use the same
            if (_stored_proc_params_dictionary.ContainsKey(storedProc))
            {
                ds = _stored_proc_params_dictionary[storedProc];
            }

            //no parameters ever found..
            //we go to the db and pick them
            else
            {
                ds = FetchStoredProcParametersFromDb(storedProc);
            }

            if (ds?.Tables.Count <= 0)
            {
                if (!_stored_proc_params_dictionary.ContainsKey(storedProc))
                    _stored_proc_params_dictionary.Add(storedProc, ds);
            }

            DataTable dataTable = ds.Tables[0];

            if (dataTable?.Rows.Count <= 0)
            {
                if (!_stored_proc_params_dictionary.ContainsKey(storedProc))
                    _stored_proc_params_dictionary.Add(storedProc, ds);
            }

            T obj = new T();
            _stored_Proc_Params_that_were_Passed = "";

            //aim here is simple
            //for each stored procedure parameter found,
            //we find the object property with the same name,
            //we then get that propertys value and save that in the array of parameters to pass
            //since sql is not Case sensitive, we find first matching parameter

            foreach (DataRow storedProcParameter in dataTable.Rows)
            {

                var objProperties = obj.GetType().GetProperties();
                bool propertyWasFound = false;
                string storedProcParamaterName = storedProcParameter["Parameter_name"].ToString().Replace("@", string.Empty);

                foreach (var objProperty in objProperties)
                {
                    try
                    {

                        if (objProperty.Name.ToUpper() == storedProcParamaterName.ToUpper())
                        {

                            object objValue = objProperty.GetValue(this, null);
                            _stored_Proc_Params_that_were_Passed += $"{storedProcParamaterName}: {objValue},";
                            allParameters.Add(objValue);
                            propertyWasFound = true;
                            break;
                        }

                    }
                    catch (Exception)
                    {
                    }
                }

                //if after looping thru all the obj properties
                //we still cant find a property with the same name
                //just set that stored proc paramater value as null

                if (!propertyWasFound)
                {
                    _stored_Proc_Params_that_were_Passed += $"{storedProcParamaterName}: null,";
                    allParameters.Add(null);
                }

            }

            if (!_stored_proc_params_dictionary.ContainsKey(storedProc))
                _stored_proc_params_dictionary.Add(storedProc, ds);

            _stored_Proc_Params_that_were_Passed.Trim(',');
            return allParameters.ToArray();
        }

        protected DataSet FetchStoredProcParametersFromDb(string storedProc)
        {
            DataSet ds = new DataSet();

            //if we managed to create the stored proc we can use to 
            //get parameters of other stored procs
            if (DbInitializerBase.WasInitOfStoredProcSuccessfull())
            {
                ds = DbEntityDbHandler.ExecuteDataSet(DbGlobals.NameOfStoredProcToGetParameterNames, storedProc);
                return ds;
            }

            //we should just run a normal sql query to get the parameters for the 
            //stored proc being called
            string sql = "select" +
                             "'Parameter_name' = name," +
                             "'Type' = type_name(user_type_id)," +
                             "'Param_order' = parameter_id" +
                             "from sys.parameters where object_id = object_id(@StoredProcName)" +
                             "order by Param_order asc";

            ds = DbEntityDbHandler.ExecuteSqlQuery(sql);
            return ds;
        }

        public string GetStoredProcedureParametersPassed()
        {
            return _stored_Proc_Params_that_were_Passed;
        }

        public virtual bool IsValid()
        {

            return true;
        }

        protected int ExecuteNonQueryStoredProcAutoParams(string storedProc)
        {
            object[] storedProcParameters = GetStoredProcParameters(storedProc);
            return ExecuteNonQueryUsingStoredProc(storedProc, storedProcParameters);
        }

        //update/insert/delete from db rows using a stored proc
        protected static int ExecuteNonQueryUsingStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public virtual bool SetSuccessAsStatusInResponseFields()
        {
            StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
            StatusDesc = DbGlobals.SUCCESS_STATUS_TEXT;
            return true;
        }

        public virtual bool SetFailuresAsStatusInResponseFields(string Message)
        {
            StatusCode = DbGlobals.FAILURE_STATUS_CODE;
            StatusDesc = Message;
            return true;
        }
    }
}
