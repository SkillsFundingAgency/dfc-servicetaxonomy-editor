using DFC.ServiceTaxonomy.PageLocation.GraphQL;
using DFC.ServiceTaxonomy.PageLocation.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using YesSql;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.TriageTool
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int CreateAsync()
        {
            _contentDefinitionManager.AlterPartDefinition("PagePart", builder => builder
                .Attachable()
                .WithDescription("Enables the content type to be synced to a graph."));

            return 1;
        }

        public int UpdateFrom1Async()
        {

            SchemaBuilder.CreateMapIndexTable<PageItemsPartIndex>(table => table
            .Column<string>("ContentItemId")
            .Column<bool>("UseInTriageTool")
            );

            SchemaBuilder.AlterTable(nameof(PageItemsPartIndex), table => table
                .CreateIndex($"IDX_PageItemsPartIndex_ContentItemId", "ContentItemId"
               ));

            return 2;
        }

       
    }
}
