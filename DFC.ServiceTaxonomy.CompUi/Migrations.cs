using DFC.ServiceTaxonomy.CompUi.Indexes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.CompUi
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
            _contentDefinitionManager.AlterPartDefinition("RelatedContentItemIndex", builder => builder
                .Attachable()
                .WithDescription("Adds the related content item Ids."));

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTable<RelatedContentItemIndex>(table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("ContentType")
                .Column<string>("RelatedContentIds")
            );

            SchemaBuilder.AlterTable(nameof(RelatedContentItemIndex), table => table
                .CreateIndex("IDX_RelatedContentItemIndex_ContentItemId", "ContentItemId")
            );

            return 2;
        }
    }
}
