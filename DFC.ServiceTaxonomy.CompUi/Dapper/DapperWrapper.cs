using System.Data.Common;
using Dapper;

namespace DFC.ServiceTaxonomy.CompUi.Dapper
{
    internal class DapperWrapper : IDapperWrapper
    {
        async public Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql)
        {
            return await connection.QueryAsync<T>(sql);
        }
    }
}
