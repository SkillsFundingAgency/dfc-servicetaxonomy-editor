using DFC.ServiceTaxonomy.Dysac.Indexes;
using DFC.ServiceTaxonomy.Dysac.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using YesSql;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.Dysac
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
            _contentDefinitionManager.AlterPartDefinition(nameof(JobProfileCategoriesPart), builder => builder
                .Attachable()
                .WithDescription("Enables the content type to be synced to a graph."));

            return 1;
        }

        public int UpdateFrom1()
        {

            SchemaBuilder.CreateMapIndexTable<JobProfileCategoriesPartIndex>(table => table
            .Column<string>(nameof(JobProfileCategoriesPartIndex.ContentItemId))
            .Column<string>(nameof(JobProfileCategoriesPartIndex.RelatedJobProfileContentItemIds), c => c.WithLength(5000))
            );

            SchemaBuilder.AlterTable(nameof(JobProfileCategoriesPartIndex), table => table
                .CreateIndex($"IDX_{nameof(JobProfileCategoriesPartIndex)}_{nameof(JobProfileCategoriesPartIndex.ContentItemId)}",
                nameof(JobProfileCategoriesPartIndex.ContentItemId)
               ));

            return 2;
        }

       
    }
}
