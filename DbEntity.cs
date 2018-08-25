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


        public Task SaveWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => SaveWithStoredProc(storedProc, storedProcParameters));
        }

        public Task UpdateWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => UpdateWithStoredProc(storedProc, storedProcParameters));
        }

        public Task InsertWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => InsertWithStoredProc(storedProc, storedProcParameters));
        }

        public Task DeleteWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => DeleteWithStoredProc(storedProc, storedProcParameters));
        }

        public static T[] QueryWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            List<T> all = new List<T>();
            DataTable dt = DatabaseHandler.ExecuteStoredProc(storedProc, storedProcParameters);

            foreach (DataRow dr in dt.Rows)
            {
                T obj = new T();
                CopyParentArrayToChildProperty(dr, obj);
                all.Add(obj);
            }

            return all.ToArray();
        }

        public int SaveWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DatabaseHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public int InsertWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DatabaseHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public int UpdateWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DatabaseHandler.ExecuteNonQuery(storedProc, storedProcParameters);
            return rowsAffected;
        }

        public int DeleteWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            int rowsAffected = DatabaseHandler.ExecuteNonQuery(storedProc, storedProcParameters);
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
                                childProperty.SetValue(child, parent[column],new object[] { });
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
