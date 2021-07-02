using DFC.ServiceTaxonomy.Banners.Indexes;
using DFC.ServiceTaxonomy.Banners.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Recipes.Services;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.Banners
{
    [Feature("DFC.ServiceTaxonomy.Banners")]
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager, IRecipeMigrator recipeMigrator)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(BannerPart), part => part
                .Attachable()
                .WithDescription("Adds banner related properties to a content item.")
            );
            SchemaBuilder.CreateMapIndexTable<BannerPartIndex>(table => table
                .Column<string>(nameof(BannerPartIndex.ContentItemId))
                .Column<string>(nameof(BannerPartIndex.WebPageName))
                .Column<string>(nameof(BannerPartIndex.WebPageURL))
                .Column<bool>(nameof(BannerPartIndex.Latest))
                .Column<bool>(nameof(BannerPartIndex.Published)));

            SchemaBuilder.AlterIndexTable<BannerPartIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(BannerPartIndex)}_{nameof(BannerPartIndex.ContentItemId)}",
                    "DocumentId",
                    nameof(BannerPartIndex.WebPageName),
                    nameof(BannerPartIndex.WebPageURL),
                    nameof(BannerPartIndex.Latest),
                    nameof(BannerPartIndex.Published)));

            return 1;
        }

    }
}
