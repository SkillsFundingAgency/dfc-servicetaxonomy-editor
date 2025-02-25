using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Constants;
using DFC.ServiceTaxonomy.PageLocation.Indexes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using YesSql;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.PageLocation
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly YesSql.ISession _session;

        public Migrations(IContentDefinitionManager contentDefinitionManager, YesSql.ISession session)
        {
            _session = session;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinitionAsync("PageLocationPart", builder => builder
                .Attachable()
                .WithDescription("Adds the page location part."));

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTableAsync<PageLocationPartIndex>(table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("Url")
            );

            SchemaBuilder.AlterTableAsync(nameof(PageLocationPartIndex), table => table
                .CreateIndex("IDX_PageLocationPartIndex_ContentItemId", "ContentItemId")
            );

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterIndexTableAsync<PageLocationPartIndex>(table => table
            .AddColumn<string>("UrlName")
            );

            return 3;
        }


        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTableAsync<PageLocationPartIndex>(table => table
            .AddColumn<bool>("UseInTriageTool")
            );

            return 4;
        }

        public async Task<int> UpdateFrom4Async()
        {
            var contentItems = await _session
                .Query<ContentItem, ContentItemIndex>()
                .Where(x => (x.ContentType == ContentTypes.Page || x.ContentType == ContentTypes.TriageToolFilters) && x.Published && x.Latest)
                .ListAsync();

            foreach (var contentItem in contentItems)
            {
                await _session.SaveAsync(contentItem, checkConcurrency: true);
            }
            return 5;
        }
    }
}
