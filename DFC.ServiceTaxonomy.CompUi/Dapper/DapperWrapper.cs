using System.Data.Common;
using Dapper;

namespace DFC.ServiceTaxonomy.CompUi.Dapper
{
    internal class DapperWrapper : IDapperWrapper
    {
        public async Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql)
            => await QueryAsync<T>(connection, sql, null);

        public async Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql, object? param)
            => await connection.QueryAsync<T>(sql, param);

        public async Task<string> QueryAsync(DbConnection connection, string sql)
            => await QueryAsync(connection, sql, null);

        public async Task<string> QueryAsync(DbConnection connection, string sql, object? param)
            => await connection.QuerySingleAsync(sql, param);
    }
}
