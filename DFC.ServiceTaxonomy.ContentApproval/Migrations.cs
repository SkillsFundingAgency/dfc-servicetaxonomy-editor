using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Recipes.Services;
using YesSql.Sql;

namespace DFC.ServiceTaxonomy.ContentApproval
{
    [Feature("DFC.ServiceTaxonomy.ContentApproval")]
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IRecipeMigrator _recipeMigrator;
        private readonly ILogger<Migrations> _logger;

        public Migrations(IContentDefinitionManager contentDefinitionManager, IRecipeMigrator recipeMigrator, ILogger<Migrations> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _recipeMigrator = recipeMigrator;
            _logger = logger;
        }

        public async Task<int> CreateAsync()
        {
            await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(ContentApprovalItemStatusDashboardPart), builder => builder
                .Attachable()
                .WithDescription("Adds content approval dashboard cards.")
            );

            await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(ContentApprovalPart), part => part
                .Attachable()
                .WithDescription("Adds publishing status workflow properties to content items.")
            );


            try
            {
                await SchemaBuilder.DropMapIndexTableAsync<ContentApprovalPartIndex>();
            }
            catch (Exception e)
            {
                // Not required by SQLLite as no issue if index doesn't exist but maybe a problem in SQL
                _logger.LogWarning(e, "ContentApprovalPartIndex could not be deleted");
            }

            await SchemaBuilder.CreateMapIndexTableAsync<ContentApprovalPartIndex>(table => table
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewStatus))
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewType))
                .Column<bool>(nameof(ContentApprovalPart.IsForcePublished)));

            await SchemaBuilder.AlterIndexTableAsync<ContentApprovalPartIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(ContentApprovalPartIndex)}_{nameof(ContentApprovalPartIndex.ReviewStatus)}",
                    "DocumentId",
                    nameof(ContentApprovalPartIndex.ReviewStatus),
                    nameof(ContentApprovalPartIndex.ReviewType),
                    nameof(ContentApprovalPart.IsForcePublished)));

            await _recipeMigrator.ExecuteAsync("stax-content-approval.recipe.json", this);

            return 9;
        }

        public async Task<int> UpdateFrom1Async()
        {
            await _recipeMigrator.ExecuteAsync("stax-content-approval.recipe.json", this);

            return 2;
        }

        public int UpdateFrom2()
        {
            try
            {
                SchemaBuilder.DropMapIndexTableAsync<ContentApprovalPartIndex>();
            }
            catch (Exception e)
            {
                // Not required by SQLLite as no issue if index doesn't exist but maybe a problem in SQL
                _logger.LogWarning(e, "ContentApprovalPartIndex could not be deleted");
            }

            _contentDefinitionManager.DeletePartDefinitionAsync(nameof(ContentApprovalPart));

            SchemaBuilder.CreateMapIndexTableAsync<ContentApprovalPartIndex>(table => table
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewStatus))
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewType)));

            SchemaBuilder.AlterIndexTableAsync<ContentApprovalPartIndex>(table => table
                .CreateIndex(
                    $"IDX_{nameof(ContentApprovalPartIndex)}_{nameof(ContentApprovalPartIndex.ReviewStatus)}",
                    "DocumentId",
                    nameof(ContentApprovalPartIndex.ReviewStatus),
                    nameof(ContentApprovalPartIndex.ReviewType)));

            _contentDefinitionManager.AlterPartDefinitionAsync(nameof(ContentApprovalPart), part => part
                .Attachable()
                .WithDescription("Adds publishing status workflow properties to content items.")
            );

            return 3;
        }

        public async Task<int> UpdateFrom3Async()
        {
            try
            {
                await SchemaBuilder.DropMapIndexTableAsync<ContentApprovalPartIndex>();
            }
            catch (Exception e)
            {
                // Not required by SQLLite as no issue if index doesn't exist but maybe a problem in SQL
                _logger.LogWarning(e, "ContentApprovalPartIndex could not be deleted");
            }

            await SchemaBuilder.CreateMapIndexTableAsync<ContentApprovalPartIndex>(table => table
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewStatus))
                .Column<int>(nameof(ContentApprovalPartIndex.ReviewType))
                .Column<bool>(nameof(ContentApprovalPart.IsForcePublished)));

            await SchemaBuilder.AlterIndexTableAsync<ContentApprovalPartIndex>(table => table
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
        public async Task<int> UpdateFrom6Async()
        {
            await _recipeMigrator.ExecuteAsync("stax-content-approval-amendment-04.recipe.json", this);

            return 7;
        }

        public async Task<int> UpdateFrom7Async()
        {
            await _recipeMigrator.ExecuteAsync("stax-content-approval-amendment-05.recipe.json", this);

            return 8;
        }

        public async Task<int> UpdateFrom8Async()
        {
            await _recipeMigrator.ExecuteAsync("stax-content-approval-amendment-06.recipe.json", this);

            return 9;
        }

    }
}
