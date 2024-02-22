using System.Data.Common;
using Dapper;

namespace DFC.ServiceTaxonomy.CompUi.Dapper
{
    internal class DapperWrapper : IDapperWrapper
    {
        public async Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql)
        {
            return await connection.QueryAsync<T>(sql);
        }

        public async Task<string> QueryAsync(DbConnection connection, string sql)
        {
            return await connection.QuerySingle(sql);
        }


       
        //public async Task<string> QuerySingleOrDefaultAsync(DbConnection connection, string sql)
        //{
        //    return await connection.QuerySingleOrDefaultAsync(sql);
        //}
    }
}
