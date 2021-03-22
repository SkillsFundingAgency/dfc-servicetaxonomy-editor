using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Recipes.Services;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using OrchardCore.Modules;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.ContentApproval
{
    [Feature("DFC.ServiceTaxonomy.ContentApproval")]
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IRecipeMigrator _recipeMigrator;

        public Migrations(IContentDefinitionManager contentDefinitionManager, IRecipeMigrator recipeMigrator)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _recipeMigrator = recipeMigrator;
        }

        public async Task<int> CreateAsync()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(ContentApprovalItemStatusDashboardPart), builder => builder
                .Attachable()
                .WithDescription("Adds content approval dashboard cards.")
            );

            _contentDefinitionManager.AlterPartDefinition(nameof(ContentApprovalPart), part => part
                .Attachable()
                .WithDescription("Adds publishing status workflow properties to content items.")
            );

            SchemaBuilder.DropMapIndexTable<ContentApprovalPartIndex>();

            SchemaBuilder.CreateMapIndexTable<ContentApprovalPartIndex>(table => table
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewStatus))
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewType))
                .Column<bool>(nameof(ContentApprovalPart.IsForcePublished)));

            SchemaBuilder.AlterIndexTable<ContentApprovalPartIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(ContentApprovalPartIndex)}_{nameof(ContentApprovalPartIndex.ReviewStatus)}",
                    "DocumentId",
                    nameof(ContentApprovalPartIndex.ReviewStatus),
                    nameof(ContentApprovalPartIndex.ReviewType),
                    nameof(ContentApprovalPart.IsForcePublished)));


            await _recipeMigrator.ExecuteAsync("stax-content-approval.recipe.json", this);

            return await Task.FromResult(6);
        }

        public async Task<int> UpdateFrom1Async()
        {
            await _recipeMigrator.ExecuteAsync("stax-content-approval.recipe.json", this);

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.DropMapIndexTable<ContentApprovalPartIndex>();

            _contentDefinitionManager.DeletePartDefinition(nameof(ContentApprovalPart));

            SchemaBuilder.CreateMapIndexTable<ContentApprovalPartIndex>(table => table
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewStatus))
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewType)));

            SchemaBuilder.AlterIndexTable<ContentApprovalPartIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(ContentApprovalPartIndex)}_{nameof(ContentApprovalPartIndex.ReviewStatus)}",
                    "DocumentId",
                    nameof(ContentApprovalPartIndex.ReviewStatus),
                    nameof(ContentApprovalPartIndex.ReviewType)));

            _contentDefinitionManager.AlterPartDefinition(nameof(ContentApprovalPart), part => part
                .Attachable()
                .WithDescription("Adds publishing status workflow properties to content items.")
            );

            return 3;
        }

        public async Task<int> UpdateFrom3Async()
        {
            SchemaBuilder.DropMapIndexTable<ContentApprovalPartIndex>();

            SchemaBuilder.CreateMapIndexTable<ContentApprovalPartIndex>(table => table
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewStatus))
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewType))
                .Column<bool>(nameof(ContentApprovalPart.IsForcePublished)));

            SchemaBuilder.AlterIndexTable<ContentApprovalPartIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(ContentApprovalPartIndex)}_{nameof(ContentApprovalPartIndex.ReviewStatus)}",
                    "DocumentId",
                    nameof(ContentApprovalPartIndex.ReviewStatus),
                    nameof(ContentApprovalPartIndex.ReviewType),
                    nameof(ContentApprovalPart.IsForcePublished)));

            await _recipeMigrator.ExecuteAsync("stax-content-approval-amendment-01.recipe.json", this);

            return 4;
        }

        public async Task<int> UpdateFrom4Async()
        {
            await _recipeMigrator.ExecuteAsync("stax-content-approval-amendment-02.recipe.json", this);

            return 5;
        }

        public async Task<int> UpdateFrom5Async()
        {
            await _recipeMigrator.ExecuteAsync("stax-content-approval-amendment-03.recipe.json", this);

            return 6;
        }
    }
}
