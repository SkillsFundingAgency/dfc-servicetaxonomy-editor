using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Recipes.Services;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.ContentApproval
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IRecipeMigrator _recipeMigrator;

        public Migrations(IContentDefinitionManager contentDefinitionManager, IRecipeMigrator recipeMigrator)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _recipeMigrator = recipeMigrator;
        }

        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<ContentApprovalPartIndex>(table => table
                .Column<string>(nameof(ContentApprovalPartIndex.ReviewStatus)));

            SchemaBuilder.AlterIndexTable<ContentApprovalPartIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(ContentApprovalPartIndex)}_{nameof(ContentApprovalPartIndex.ReviewStatus)}",
                    "DocumentId",
                    nameof(ContentApprovalPartIndex.ReviewStatus)));

            _contentDefinitionManager.AlterPartDefinition(nameof(ContentApprovalPart), part => part
                .Attachable()
                .WithDescription("Adds publishing status workflow properties to content items.")
            );

            _contentDefinitionManager.AlterPartDefinition(nameof(ContentApprovalItemStatusDashboardPart), builder => builder
                .Attachable()
                .WithDescription("Adds content approval dashboard cards.")
            );

            return 1;
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
                .Column<string>(nameof(ContentApprovalPartIndex.ReviewStatus)));

            SchemaBuilder.AlterIndexTable<ContentApprovalPartIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(ContentApprovalPartIndex)}_{nameof(ContentApprovalPartIndex.ReviewStatus)}",
                    "DocumentId",
                    nameof(ContentApprovalPartIndex.ReviewStatus)));

            _contentDefinitionManager.AlterPartDefinition(nameof(ContentApprovalPart), part => part
                .Attachable()
                .WithDescription("Adds publishing status workflow properties to content items.")
            );

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTable<ContentApprovalPartIndex>(table => table
                .AddColumn<string>(nameof(ContentApprovalPartIndex.ReviewType)));

            return 4;
        }
    }
}
