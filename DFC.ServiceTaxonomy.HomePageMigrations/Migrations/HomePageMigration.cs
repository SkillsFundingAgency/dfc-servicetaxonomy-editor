using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.HomePageMigrations.Migrations
{
    public class HomePageMigration : DataMigration
    {
        private readonly IRecipeMigrator _recipeMigrator;
        private readonly ILogger<HomePageMigration> _logger;

        public HomePageMigration(IRecipeMigrator recipeMigrator, ILogger<HomePageMigration> logger)
        {
            _recipeMigrator = recipeMigrator;
            _logger = logger;
        }

        public async Task<int> CreateAsync()
        {
            try
            {
                _logger.LogInformation($"Starting step 1 of DFC.ServiceTaxonomy.HomePageMigration");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/home-page.recipe.json", this);
                _logger.LogInformation($"Completed step 1 of DFC.ServiceTaxonomy.HomePageMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 1 of DFC.ServiceTaxonomy.HomePageMigration");
            }

            return 1;
        }

        public async Task<int> UpdateFrom1Async()
        {

            try
            {
                _logger.LogInformation($"Starting step 2 of DFC.ServiceTaxonomy.HomePageMigration");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/speak-to-an-adviser.recipe.json", this);
                _logger.LogInformation($"Completed step 2 of DFC.ServiceTaxonomy.HomePageMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 2 of DFC.ServiceTaxonomy.HomePageMigration");
            }

            return 2;
        }
        public async Task<int> UpdateFrom2Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 3 of DFC.ServiceTaxonomy.HomePageMigration");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/triage-result-tile.recipe.json", this);
                _logger.LogInformation($"Completed step 3 of DFC.ServiceTaxonomy.HomePageMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 3 of DFC.ServiceTaxonomy.HomePageMigration");
            }

            return 3;
        }
        public async Task<int> UpdateFrom3Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 3 of DFC.ServiceTaxonomy.HomePageMigration");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/keep-in-touch.recipe.json", this);
                _logger.LogInformation($"Completed step 3 of DFC.ServiceTaxonomy.HomePageMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 3 of DFC.ServiceTaxonomy.HomePageMigration");
            }

            return 4;
        }
    }
}
