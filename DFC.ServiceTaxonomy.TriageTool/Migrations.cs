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

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(PagePart), builder => builder
                .Attachable()
                .WithDescription("Enables the content type to be synced to a graph."));

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTable<PageItemsPartIndex>(table => table
         .Column<string>(nameof(PageItemsPartIndex.ContentItemId))
         .Column<bool>(nameof(PageItemsPartIndex.UseInTriageTool))
         );

            SchemaBuilder.AlterTable(nameof(PageItemsPartIndex), table => table
                .CreateIndex($"IDX_PageItemsPartIndex_ContentItemId", "ContentItemId"));  

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterIndexTable<PageItemsPartIndex>(table => table
            .AddColumn<bool>("useInTest")
            );

            return 3;
        }


    }
}
