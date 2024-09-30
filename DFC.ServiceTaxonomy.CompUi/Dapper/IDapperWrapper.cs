using System.Data.Common;

namespace DFC.ServiceTaxonomy.CompUi.Dapper
{
    public interface IDapperWrapper
    {
        Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql);

        Task<string> QueryAsync(DbConnection connection, string sql);      
    }
}
