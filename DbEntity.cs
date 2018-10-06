using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DbEntity
{
    public class DbEntity<T> : ActiveRecordBase<T> where T : new()
    {

        public string StatusCode { get; set; }
        public string StatusDesc { get; set; }
        

        //Simple implementation of a Last Recently Used (LRU) cache to hold the last stored paramaters picked from DB 
        //(prevent too many calls to the Db to pick parameters of stored proc)
        private Dictionary<string, DataSet> _storedProcParameters = new Dictionary<string, DataSet>();
        private string _storedProcedureParametersPassed { get; set; }

        public DbEntity()
        {
        }

        private static void CopyDataRowValuesToObjectProperties(DataRow parent, T obj)
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

        private static void TryToSetObjectProperty(object theObject, string propertyName, object value)
        {
            try
            {
                Type type = theObject.GetType();
                var property = type.GetProperty(propertyName);
                var setter = property.SetMethod;
                setter.Invoke(theObject, new object[] { value });
            }
            catch (Exception) { }
        }

        private object[] GetStoredProcParameters(string storedProc)
        {
            List<object> allParameters = new List<object>();

            DataSet ds = null;

            if (_storedProcParameters.ContainsKey(storedProc))
            {
                ds = _storedProcParameters[storedProc];
            }
            else
            {
                ds = FetchStoredProcParametersFromDb(storedProc);
            }

            if (ds?.Tables.Count <= 0)
            {
                if (!_storedProcParameters.ContainsKey(storedProc))
                    _storedProcParameters.Add(storedProc, ds);
            }

            DataTable dataTable = ds.Tables[0];

            if (dataTable?.Rows.Count <= 0)
            {
                if (!_storedProcParameters.ContainsKey(storedProc))
                    _storedProcParameters.Add(storedProc, ds);
            }

            T obj = new T();
            _storedProcedureParametersPassed = "";

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
                            _storedProcedureParametersPassed += $"{storedProcParamaterName}: {objValue},";
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
                    _storedProcedureParametersPassed += $"{storedProcParamaterName}: null,";
                    allParameters.Add(null);
                }

            }

            if (!_storedProcParameters.ContainsKey(storedProc))
                _storedProcParameters.Add(storedProc, ds);

            _storedProcedureParametersPassed.Trim(',');
            return allParameters.ToArray();
        }

        private DataSet FetchStoredProcParametersFromDb(string storedProc)
        {
            DataSet ds = new DataSet();
            if (!DbInitializer.IsInitOfStoredProcSuccessfull)
            {
                string sql = "select" +
                             "'Parameter_name' = name," +
                             "'Type' = type_name(user_type_id)," +
                             "'Param_order' = parameter_id" +
                             "from sys.parameters where object_id = object_id(@StoredProcName)" +
                             "order by Param_order asc";
                ds = DbEntityDbHandler.ExecuteSqlQuery(sql);
            }
            else
            {
                ds = DbEntityDbHandler.ExecuteDataSet(DbInitializer.StoredProcForGettingParameterNames, storedProc);
            }
            return ds;
        }
    
        public string GetStoredProcedureParametersPassed()
        {
            return _storedProcedureParametersPassed;
        }

        public virtual bool IsValid()
        {
            return true;
        }

        public virtual int SaveWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public virtual int InsertWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public virtual int UpdateWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public virtual int DeleteWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public virtual T[] QueryWithStoredProcAutoParams(string storedProc)
        {
            List<T> all = new List<T>();

            object[] storedProcParameters = GetStoredProcParameters(storedProc);

            DataTable dt = DbEntityDbHandler.ExecuteStoredProc(storedProc, storedProcParameters);

            foreach (DataRow dr in dt.Rows)
            {
                T obj = new T();
                CopyDataRowValuesToObjectProperties(dr, obj);
                all.Add(obj);
            }

            this.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
            this.StatusDesc = DbGlobals.SUCCESS_STATUS_TEXT;
            return all.ToArray();
        }

        public virtual int SaveWithStoredProcAutoParams(string storedProc)
        {
            object[] storedProcParameters = GetStoredProcParameters(storedProc);
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public virtual int InsertWithStoredProcAutoParams(string storedProc)
        {
            object[] storedProcParameters = GetStoredProcParameters(storedProc);
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public virtual int UpdateWithStoredProcAutoParams(string storedProc)
        {
            object[] storedProcParameters = GetStoredProcParameters(storedProc);
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public virtual int DeleteWithStoredProcAutoParams(string storedProc)
        {
            object[] storedProcParameters = GetStoredProcParameters(storedProc);
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
    
        public virtual Task SaveWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => SaveWithStoredProc(storedProc, storedProcParameters));
        }

        public virtual Task UpdateWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => UpdateWithStoredProc(storedProc, storedProcParameters));
        }

        public virtual Task InsertWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => InsertWithStoredProc(storedProc, storedProcParameters));
        }

        public virtual Task DeleteWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => DeleteWithStoredProc(storedProc, storedProcParameters));
        }

        public static Task QueryWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => QueryWithStoredProc(storedProc, storedProcParameters));
        }

        public static T[] QueryWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            List<T> all = new List<T>();

            DataTable dt = DbEntityDbHandler.ExecuteStoredProc(storedProc, storedProcParameters);

            foreach (DataRow dr in dt.Rows)
            {
                T obj = new T();
                CopyDataRowValuesToObjectProperties(dr, obj);
                all.Add(obj);
            }

            return all.ToArray();
        }

        public static int SaveUsingStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public static int InsertUsingStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public static int UpdateUsingStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public static int DeleteUsingStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DbEntityDbHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        

    }
}
