using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.TriageToolMigrations.Migrations
{
    public class TriageToolMigration : DataMigration
    {
        private readonly IRecipeMigrator _recipeMigrator;
        private readonly ILogger<TriageToolMigration> _logger;

        public TriageToolMigration(IRecipeMigrator recipeMigrator, ILogger<TriageToolMigration> logger)
        {
            _recipeMigrator = recipeMigrator;
            _logger = logger;
        }

        public async Task<int> CreateAsync()
        {
            try
            {
                _logger.LogInformation($"Starting step 1 of DFC.ServiceTaxonomy.TriageToolMigrations");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/triagetool-speak-to-an-adviser.recipe.json", this);
                _logger.LogInformation($"Completed step 1 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 1 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }

            return 1;
        }

        public async Task<int> UpdateFrom1Async()
        {

            try
            {
                _logger.LogInformation($"Starting step 2 of DFC.ServiceTaxonomy.TriageToolMigrations");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/triagetool-speak-to-an-adviser-update.recipe.json", this);
                _logger.LogInformation($"Completed step 2 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 2 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }

            return 2;
        }
    }
}
