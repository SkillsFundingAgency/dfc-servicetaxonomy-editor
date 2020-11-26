using DFC.ServiceTaxonomy.UnpublishLater.Indexes;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.UnpublishLater
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
            _contentDefinitionManager.AlterPartDefinition(nameof(UnpublishLaterPart), builder => builder
                .Attachable()
                .WithDescription("Adds the ability to schedule content items to be unpublished at a given future date and time."));

            SchemaBuilder.CreateMapIndexTable<UnpublishLaterPartIndex>(table => table
                .Column<string>(nameof(UnpublishLaterPartIndex.ScheduledUnpublishUtc))
            );

            SchemaBuilder.AlterTable(nameof(UnpublishLaterPartIndex), table => table
                .CreateIndex(
                    $"IDX_{nameof(UnpublishLaterPartIndex)}_{nameof(UnpublishLaterPartIndex.ScheduledUnpublishUtc)}",
                    nameof(UnpublishLaterPartIndex.ScheduledUnpublishUtc))
            );

            return 1;
        }
    }
}
