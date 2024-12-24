using DFC.ServiceTaxonomy.Title.Indexes;
using DFC.ServiceTaxonomy.Title.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Recipes.Services;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.Title
{
    [Feature("DFC.ServiceTaxonomy.Title")]
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager, IRecipeMigrator recipeMigrator)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(UniqueTitlePart), part => part
                .Attachable()
                .WithDescription("Adds Title related properties to a content item.")
            );
            SchemaBuilder.CreateMapIndexTable<UniqueTitlePartIndex>(table => table
                .Column<string>(nameof(UniqueTitlePartIndex.ContentItemId))
                .Column<string>(nameof(UniqueTitlePartIndex.Title))
                .Column<bool>(nameof(UniqueTitlePartIndex.Latest))
                .Column<bool>(nameof(UniqueTitlePartIndex.Published)));

            SchemaBuilder.AlterIndexTable<UniqueTitlePartIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(UniqueTitlePartIndex)}_{nameof(UniqueTitlePartIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(UniqueTitlePartIndex.Title),
                    nameof(UniqueTitlePartIndex.Latest),
                    nameof(UniqueTitlePartIndex.Published)));

            return 1;
        }

        public int UpdateFrom1()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(UniqueTitlePart), part => part
                .Attachable()
                .WithDescription("Adds Title related properties to a content item.")
            );

            //SchemaBuilder.DropMapIndexTable<UniqueTitlePartIndex>();

            SchemaBuilder.AlterIndexTable<UniqueTitlePartIndex>(table => table.AddColumn<string>(nameof(UniqueTitlePartIndex.ContentType)));
            //SchemaBuilder.CreateMapIndexTable<UniqueTitlePartIndex>(table => table
            //    .Column<string>(nameof(UniqueTitlePartIndex.ContentItemId))
            //    .Column<string>(nameof(UniqueTitlePartIndex.Title))
            //    .Column<bool>(nameof(UniqueTitlePartIndex.Latest))
            //    .Column<bool>(nameof(UniqueTitlePartIndex.Published))
            //    .Column<bool>(nameof(UniqueTitlePartIndex.ContentType)));

            SchemaBuilder.AlterIndexTable<UniqueTitlePartIndex>(table => table
     .DropIndex($"IDX_{nameof(UniqueTitlePartIndex)}_{nameof(UniqueTitlePartIndex.ContentItemId)}"));

            SchemaBuilder.AlterIndexTable<UniqueTitlePartIndex>(table => table
                           .CreateIndex(
                               $"IDX_{nameof(UniqueTitlePartIndex)}_{nameof(UniqueTitlePartIndex.ContentItemId)}",
                               "DocumentId",
                               nameof(UniqueTitlePartIndex.Title),
                               nameof(UniqueTitlePartIndex.Latest),
                               nameof(UniqueTitlePartIndex.Published),
                               nameof(UniqueTitlePartIndex.ContentType)));

            return 2;
        }
    }
}
