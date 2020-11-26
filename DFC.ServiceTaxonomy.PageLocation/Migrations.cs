using DFC.ServiceTaxonomy.PageLocation.Indexes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.PageLocation
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
            _contentDefinitionManager.AlterPartDefinition("PageLocationPart", builder => builder
                .Attachable()
                .WithDescription("Adds the page location part."));

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTable<PageLocationPartIndex>(table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("Url")
            );

            SchemaBuilder.AlterTable(nameof(PageLocationPartIndex), table => table
                .CreateIndex("IDX_PageLocationPartIndex_ContentItemId", "ContentItemId")
            );

            return 2;
        }
    }
}
