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
        public async Task<int> UpdateFrom2Async()
        {

            try
            {
                _logger.LogInformation($"Starting step 1 of DFC.ServiceTaxonomy.TriageToolMigrations");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/triage-tool-lookup.recipe.json", this);
                _logger.LogInformation($"Completed step 1 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 1 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }

            return 3;
        }
        public async Task<int> UpdateFrom3Async()
        {

            try
            {
                _logger.LogInformation($"Starting step 2 of DFC.ServiceTaxonomy.TriageToolMigrations");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/triage-tool-resulttypes.recipe.json", this);
                _logger.LogInformation($"Completed step 2 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 2 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }

            return 4;
        }
        public async Task<int> UpdateFrom4Async()
        {

            try
            {
                _logger.LogInformation($"Starting step 3 of DFC.ServiceTaxonomy.TriageToolMigrations");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/triage-tool-lookup-result-content.recipe.json", this);
                _logger.LogInformation($"Completed step 3 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 3 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }

            return 5;
        }
        public async Task<int> UpdateFrom5sync()
        {

            try
            {
                _logger.LogInformation($"Starting step 4 of DFC.ServiceTaxonomy.TriageToolMigrations");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/speak-to-an-adviser.recipe.json", this);
                _logger.LogInformation($"Completed step 4 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 4 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }

            return 6;
        }
        public async Task<int> UpdateFrom6sync()
        {

            try
            {
                _logger.LogInformation($"Starting step 5 of DFC.ServiceTaxonomy.TriageToolMigrations");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/home-page.recipe.json", this);
                _logger.LogInformation($"Completed step 4 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 5 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }

            return 7;
        }
        public async Task<int> UpdateFrom7sync()
        {

            try
            {
                _logger.LogInformation($"Starting step 6 of DFC.ServiceTaxonomy.TriageToolMigrations");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/triagetool-becomeanapprentice.recipe.json", this);
                _logger.LogInformation($"Completed step 4 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 6 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }

            return 8;
        }
        public async Task<int> UpdateFrom8sync()
        {

            try
            {
                _logger.LogInformation($"Starting step 6 of DFC.ServiceTaxonomy.TriageToolMigrations");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/triagetool-becomeanapprentice.recipe.json", this);
                _logger.LogInformation($"Completed step 4 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 6 of DFC.ServiceTaxonomy.TriageToolMigrations");
            }

            return 9;
        }
    }
}
