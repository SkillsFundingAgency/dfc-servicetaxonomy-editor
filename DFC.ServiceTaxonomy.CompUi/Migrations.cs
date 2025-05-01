using DFC.ServiceTaxonomy.CompUi.Models;
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
            _contentDefinitionManager.AlterPartDefinitionAsync(nameof(RelatedContentItemIndex), builder => builder
                .Attachable()
                .WithDescription("Adds the related content item Ids."));

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTableAsync<RelatedContentItemIndex>(table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("ContentType")
                .Column<string>("RelatedContentIds", c => c.WithLength(2048))
            );

            SchemaBuilder.AlterTableAsync(nameof(RelatedContentItemIndex), table => table
                .CreateIndex("IDX_RelatedContentItemIndex_ContentItemId", "ContentItemId")
            );

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterTableAsync(nameof(RelatedContentItemIndex), table => table
                .DropColumn("ContentType")
            );

            SchemaBuilder.AlterTableAsync(nameof(RelatedContentItemIndex), table => table
                .DropColumn("RelatedContentIds")
            );

            SchemaBuilder.AlterTableAsync(nameof(RelatedContentItemIndex), table => table
                .AddColumn<string>("ContentType", c => c.WithLength(128))
            );

            SchemaBuilder.AlterTableAsync(nameof(RelatedContentItemIndex), table => table
                .AddColumn<string>("RelatedContentIds", c => c.WithLength(1024))
            );

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterTableAsync(nameof(RelatedContentItemIndex), table => table
                .DropColumn("RelatedContentIds")
            );

            SchemaBuilder.AlterTableAsync(nameof(RelatedContentItemIndex), table => table
                .AddColumn<string>("RelatedContentIds", c => c.WithLength(2048))
            );

            return 4;
        }

        public int UpdateFrom4()
        {
            SchemaBuilder.AlterTableAsync(nameof(RelatedContentItemIndex), table => table
                .AlterColumn(nameof(RelatedContentItemIndex.RelatedContentIds), c => c.WithType(typeof(string), int.MaxValue - 1))
            );

            return 5;
        }
    }
}
