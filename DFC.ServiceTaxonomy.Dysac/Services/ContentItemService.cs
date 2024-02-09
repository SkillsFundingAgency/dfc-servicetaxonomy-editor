using DFC.ServiceTaxonomy.Dysac.Indexes;
using DFC.ServiceTaxonomy.Dysac.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using YesSql;

namespace DFC.ServiceTaxonomy.Dysac.Services
{
    /// <inheritdoc/>
    public class ContentItemService : IContentItemService
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IDbQueryService dbQueryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentItemService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="dbQueryService">The <see cref="IDbQueryService"/>.</param>
        public ContentItemService(
            ILogger<ContentItemService> logger,
            IServiceProvider serviceProvider,
            IDbQueryService dbQueryService)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.dbQueryService = dbQueryService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ContentItem>> GetReferencingContentItems(string contentItemId)
        {
            InitialiseScopedServices(out var store, out var contentManager);

            // build query
            var dialect = store.Configuration.SqlDialect;
            var sqlBuilder = dialect.CreateBuilder(store.Configuration.TablePrefix);

            sqlBuilder.Select();
            sqlBuilder.Table("JobProfileIndex", alias: null, store.Configuration.Schema);
            sqlBuilder.Selector(nameof(JobProfileCategoriesPartIndex.ContentItemId));
            sqlBuilder.WhereAnd($"JobProfileCategory = @Id");

            // execute query to get referencing content items' ids
            var x = sqlBuilder.ToSqlString();
            var relatedContentItems = await dbQueryService.ExecuteQueryAsync<string>(sqlBuilder.ToSqlString(), new { Id = contentItemId });

            // get referencing content items by their ids
            return await contentManager.GetAsync(relatedContentItems);
        }

        private void InitialiseScopedServices(out IStore store, out IContentManager contentManager)
        {
            var scope = serviceProvider.CreateScope();
            store = scope.ServiceProvider.GetRequiredService<IStore>();
            contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
        }
    }
}
