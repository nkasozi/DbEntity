using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DbEntity
{
    public class DbEntity<T> : DbEntityBase<T> where T : new()
    {
        public DbEntity()
        {
        }

        /**DB Select and Query Methods**/
        //method implemented for Async 
        //similar to user.QueryAsync()
        public static Task<T[]> QueryWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => QueryWithStoredProc(storedProc, storedProcParameters));
        }

        //method implemented for readability 
        //similar to user.Query()
        //query db for rows matching object using a stored proc
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

        //query for results from DB
        //auto populate the stored proc parameters based on the obj properties
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

        //query for results from DB
        //auto populate the stored proc parameters based on the obj properties
        public virtual Task<T[]> QueryWithStoredProcAutoParamsAysnc(string storedProc)
        {
            return Task.Factory.StartNew(() => QueryWithStoredProcAutoParams(storedProc));
        }



        /**DB Delete Methods**/
        //method implemented for readability
        //similar to user.DeleteAsync()
        //auto populate the stored proc parameters based on the obj properties
        public virtual Task<int> DeleteWithStoredProcAutoParamsAsync(string storedProc)
        {
            return Task.Factory.StartNew(() => ExecuteNonQueryStoredProcAutoParams(storedProc));
        }

        //method implemented for readability
        //similar to user.DeleteAsync()
        //auto populate the stored proc parameters based on the obj properties
        public virtual int DeleteWithStoredProcAutoParams(string storedProc)
        {
            return ExecuteNonQueryStoredProcAutoParams(storedProc);
        }

        //method implemented for readability
        //similar to user.DeleteAsync()
        public virtual Task<int> DeleteWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => DeleteWithStoredProc(storedProc, storedProcParameters));
        }

        //method implemented for readability
        //similar to user.Delete()
        public virtual int DeleteWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            return ExecuteNonQueryUsingStoredProc(storedProc, storedProcParameters);
        }



        /**DB Update Methods**/
        //method implemented for readability
        //similar to user.UpdateAsync()
        //auto populate the stored proc parameters based on the obj properties
        public virtual Task<int> UpdateWithStoredProcAutoParamsAsync(string storedProc)
        {
            return Task.Factory.StartNew(() => ExecuteNonQueryStoredProcAutoParams(storedProc));
        }

        //method implemented for readability
        //similar to user.UpdateAsync()
        public virtual Task<int> UpdateWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => UpdateWithStoredProc(storedProc, storedProcParameters));
        }

        //method implemented for readability
        //similar to user.Update()
        public virtual int UpdateWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            return ExecuteNonQueryUsingStoredProc(storedProc, storedProcParameters);
        }

        //method implemented for readability
        //similar to user.UpdateAsync()
        //auto populate the stored proc parameters based on the obj properties
        public virtual int UpdateWithStoredProcAutoParams(string storedProc)
        {
            return ExecuteNonQueryStoredProcAutoParams(storedProc);
        }



        /**DB Insert Methods**/
        //method implemented for readability
        //similar to user.InsertAsync()
        //auto populate the stored proc parameters based on the obj properties
        public virtual Task<int> InsertWithStoredProcAutoParamsAsync(string storedProc)
        {
            return Task.Factory.StartNew(() => ExecuteNonQueryStoredProcAutoParams(storedProc));
        }

        //method implemented for readability
        //similar to user.InsertAsync()
        public virtual Task InsertWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => InsertWithStoredProc(storedProc, storedProcParameters));
        }

        //method implemented for readability
        //similar to user.Insert()
        public virtual int InsertWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            return ExecuteNonQueryUsingStoredProc(storedProc, storedProcParameters);
        }

        //method implemented for readability
        //similar to user.InsertAsync()
        //auto populate the stored proc parameters based on the obj properties
        public virtual int InsertWithStoredProcAutoParams(string storedProc)
        {
            return ExecuteNonQueryStoredProcAutoParams(storedProc);
        }



        /**DB Save (Insert or Update if exists) Methods**/
        //method implemented for readability
        //similar to user.SaveAsync()
        //auto populate the stored proc parameters based on the obj properties
        public virtual Task<int> SaveWithStoredProcAutoParamsAsync(string storedProc)
        {
            return Task.Factory.StartNew(() => ExecuteNonQueryStoredProcAutoParams(storedProc));
        }

        //method implemented for readability
        //similar to user.SaveAsync()
        public virtual Task<int> SaveWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(() => SaveWithStoredProc(storedProc, storedProcParameters));
        }

        //method implemented for readability
        //similar to user.Save()
        public virtual int SaveWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            return ExecuteNonQueryUsingStoredProc(storedProc, storedProcParameters);
        }

        //method implemented for readability
        //similar to user.Save()
        //auto populate the stored proc parameters based on the obj properties
        public virtual int SaveWithStoredProcAutoParams(string storedProc)
        {
            return ExecuteNonQueryStoredProcAutoParams(storedProc);
        }

        public virtual Task SaveAsync()
        {
           return Task.Factory.StartNew(() => Save());
        }

        //method implemented for readability
        //similar to user.Save()
        //auto populate the stored proc parameters based on the obj properties
        public static int ExecuteNonQueryWithStoredProc(string storedProc, params object[] storedProcParameters)
        {
            return ExecuteNonQueryUsingStoredProc(storedProc, storedProcParameters);
        }

        //method implemented for readability
        //similar to user.Save()
        //auto populate the stored proc parameters based on the obj properties
        public static Task<int> ExecuteNonQueryWithStoredProcAsync(string storedProc, params object[] storedProcParameters)
        {
            return Task.Factory.StartNew(()=> ExecuteNonQueryUsingStoredProc(storedProc, storedProcParameters));
        }

    }
}
