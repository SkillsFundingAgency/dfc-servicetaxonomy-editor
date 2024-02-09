using System.Diagnostics.CodeAnalysis;
using Dapper;
using DFC.ServiceTaxonomy.Dysac.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;

namespace DFC.ServiceTaxonomy.Dysac.Services
{
    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class DbQueryService : IDbQueryService
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbQueryService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public DbQueryService(
            ILogger<DbQueryService> logger,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string sql, object? param = null)
        {
            using var scope = serviceProvider.CreateScope();
            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();

            using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();

            return await connection.QueryAsync<T>(sql, param);
        }
    }
}
