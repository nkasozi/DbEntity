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

            return all.ToArray();
        }

        private object[] GetStoredProcParameters(string storedProc)
        {
            List<object> all = new List<object>();

            DataSet ds = DbEntityDbHandler.ExecuteDataSet(DbInitializer.StoredProcForParameterNames, storedProc);

            if (ds?.Tables.Count <= 0)
                return all.ToArray();

            DataTable dataTable = ds.Tables[0];

            if (dataTable?.Rows.Count <= 0)
                return all.ToArray();

            T obj = new T();

            //aim here is simple
            //for each stored procedure parameter found,
            //we find the object property with the same name,
            //we then get that propertys value and save that in the array of parameters to pass
            foreach (DataRow row in dataTable.Rows)
            {
                var objProperties = obj.GetType().GetProperties();
                bool propertyFound = false;

                foreach (var objProperty in objProperties)
                {
                    try
                    {
                        string storedProcParamaterName = row["Parameter_name"].ToString().Replace("@",string.Empty);
                        if (objProperty.Name.ToUpper() == storedProcParamaterName.ToUpper())
                        {
                            all.Add(objProperty.GetValue(this, null));
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
                    all.Add(null);
                }
            }


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

        private static void CopyParentArrayToChildProperty(DataRow parent, object child)
        {
            var childProperties = child.GetType().GetProperties();

            foreach (var childProperty in childProperties)
            {
                try
                {
                    object[] attrs = childProperty.GetCustomAttributes(false);
                    bool hasBeenSet = false;

                    foreach (object attr in attrs)
                    {
                        PrimaryKeyAttribute pkAttribute = attr as PrimaryKeyAttribute;
                        if (pkAttribute != null)
                        {
                            string column = pkAttribute.Column;
                            if (column != null)
                            {
                                childProperty.SetValue(child, parent[column], new object[] { });
                                hasBeenSet = true;
                                break;
                            }
                        }

                        PropertyAttribute propertyAttribute = attr as PropertyAttribute;
                        if (propertyAttribute != null)
                        {
                            string column = propertyAttribute.Column;
                            if (column != null)
                            {
                                childProperty.SetValue(child, parent[column], new object[] { });
                                hasBeenSet = true;
                                break;
                            }
                        }
                    }
                    if (hasBeenSet) { continue; }
                    childProperty.SetValue(child, parent[childProperty.Name], new object[] { });
                }
                catch (Exception)
                {

                }
            }
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
