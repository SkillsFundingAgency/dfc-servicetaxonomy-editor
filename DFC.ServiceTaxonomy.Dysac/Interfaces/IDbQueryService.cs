namespace DFC.ServiceTaxonomy.Dysac.Interfaces
{
    /// <summary>
    /// Helper service to query the database.
    /// </summary>
    public interface IDbQueryService
    {
        /// <summary>
        /// Execute query with parameters.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="sql">The sql query.</param>
        /// <param name="param">The parameters.</param>
        /// <returns>The results for the query.</returns>
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, object? param = null);
    }
}
