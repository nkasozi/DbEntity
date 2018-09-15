using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntity
{
    public class DbEntity<T> : ActiveRecordBase<T> where T : new()
    {

        public string StatusCode { get; set; }
        public string StatusDesc { get; set; }

        private Dictionary<string, DataSet> StoredProcParameters = new Dictionary<string, DataSet>();

        public DbEntity()
        {
        }

        public virtual bool IsValid()
        {
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

        public static T[] QueryWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            List<T> all = new List<T>();

            DataTable dt = DbEntityDbHandler.ExecuteStoredProc(storedProc, storedProcParameters);

            foreach (DataRow dr in dt.Rows)
            {
                T obj = new T();
                CopyParentArrayToChildProperty(dr, obj);
                all.Add(obj);
            }

            return all.ToArray();
        }

        public virtual T[] QueryWithStoredProcAutoParams(string storedProc)
        {
            List<T> all = new List<T>();

            object[] storedProcParameters = GetStoredProcParameters(storedProc);

            DataTable dt = DbEntityDbHandler.ExecuteStoredProc(storedProc, storedProcParameters);

            foreach (DataRow dr in dt.Rows)
            {
                T obj = new T();
                CopyParentArrayToChildProperty(dr, obj);
                all.Add(obj);
            }

            this.StatusCode = DbGlobals.SUCCESS_STATUS_CODE;
            this.StatusDesc = DbGlobals.SUCCESS_STATUS_TEXT;
            return all.ToArray();
        }

        private object[] GetStoredProcParameters(string storedProc)
        {
            List<object> allParameters = new List<object>();

            DataSet ds = null;

            if (StoredProcParameters.ContainsKey(storedProc))
                ds = StoredProcParameters[storedProc];
            else
                ds = DbEntityDbHandler.ExecuteDataSet(DbInitializer.StoredProcForParameterNames, storedProc);

            if (ds?.Tables.Count <= 0)
            {
                if (!StoredProcParameters.ContainsKey(storedProc))
                    StoredProcParameters.Add(storedProc, ds);
                return allParameters.ToArray();
            }

            DataTable dataTable = ds.Tables[0];

            if (dataTable?.Rows.Count <= 0)
            {
                if (!StoredProcParameters.ContainsKey(storedProc))
                    StoredProcParameters.Add(storedProc, ds);
                return allParameters.ToArray();
            }

            T obj = new T();

            //aim here is simple
            //for each stored procedure parameter found,
            //we find the object property with the same name,
            //we then get that propertys value and save that in the array of parameters to pass
            foreach (DataRow storedProcParameter in dataTable.Rows)
            {
                var objProperties = obj.GetType().GetProperties();
                bool propertyFound = false;

                foreach (var objProperty in objProperties)
                {
                    try
                    {
                        string storedProcParamaterName = storedProcParameter["Parameter_name"].ToString().Replace("@", string.Empty);

                        if (objProperty.Name.ToUpper() == storedProcParamaterName.ToUpper())
                        {
                            allParameters.Add(objProperty.GetValue(this, null));
                            propertyFound = true;
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
                if (!propertyFound)
                {
                    allParameters.Add(null);
                }
            }

            if(!StoredProcParameters.ContainsKey(storedProc))
                StoredProcParameters.Add(storedProc, ds);

            return allParameters.ToArray();
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

        public static Task QueryWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => QueryWithStoredProc(storedProc, storedProcParameters));
        }

        private static void CopyParentArrayToChildProperty(DataRow parent, T obj)
        {
            var objProperties = obj.GetType().GetProperties();

            foreach (var objProperty in objProperties)
            {
                try
                {
                    object[] customAttributes = objProperty.GetCustomAttributes(false);
                    bool hasBeenSet = false;

                    foreach (object customAttribute in customAttributes)
                    {
                        PrimaryKeyAttribute pkAttribute = customAttribute as PrimaryKeyAttribute;

                        if (pkAttribute != null)
                        {
                            string column = pkAttribute.Column;
                            if (column != null)
                            {
                                objProperty.SetValue(obj, parent[column], new object[] { });
                                hasBeenSet = true;
                                break;
                            }
                        }

                        PropertyAttribute propertyAttribute = customAttribute as PropertyAttribute;

                        if (propertyAttribute != null)
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

                    if (hasBeenSet)
                        continue;

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
                    TryToSetObjectProperty(obj, nameof(StatusDesc), $"FAILED: UNABLE TO SET VALUE FOR {objProperty.Name}. REASON: {ex.Message}");
                    return;
                }
            }

            //set the status code and status desc values
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
