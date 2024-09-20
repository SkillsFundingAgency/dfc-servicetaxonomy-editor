using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.ExploreCareersMigration.Migrations
{
    public class ExploreCareersMigrations : DataMigration
    {
        private readonly IRecipeMigrator _recipeMigrator;
        private readonly ILogger<ExploreCareersMigrations> _logger;

        public ExploreCareersMigrations(IRecipeMigrator recipeMigrator, ILogger<ExploreCareersMigrations> logger)
        {
            _recipeMigrator = recipeMigrator;
            _logger = logger;
        }

        public async Task<int> CreateAsync()
        {
            try
            {
                _logger.LogInformation($"Starting step 1 of DFC.ServiceTaxonomy.ExploreCareersMigration");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/explore-careers-content-types-migration.recipe.json", this);
                _logger.LogInformation($"Completed step 1 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 1 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 1;
        }

        public async Task<int> UpdateFrom1Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 2 of DFC.ServiceTaxonomy.ExploreCareersMigration");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/explore-careers-content-types-migration-amendment-01.recipe.json", this);
                _logger.LogInformation($"Completed step 2 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 2 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 2;
        }

        public async Task<int> UpdateFrom2Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 3 of DFC.ServiceTaxonomy.ExploreCareersMigration");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/explore-careers-content-types-migration-amendment-02.recipe.json", this);
                _logger.LogInformation($"Completed step 3 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 3 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 3;
        }

        public async Task<int> UpdateFrom3Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 4 of DFC.ServiceTaxonomy.ExploreCareersMigration");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/explore-careers-content-types-migration-amendment-03.recipe.json", this);
                _logger.LogInformation($"Completed step 4 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 4 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 4;
        }

        public async Task<int> UpdateFrom4Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 5 of DFC.ServiceTaxonomy.ExploreCareersMigration");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/explore-careers-content-types-migration-amendment-04.recipe.json", this);
                _logger.LogInformation($"Completed step 5 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 5 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 5;
        }

        public async Task<int> UpdateFrom5Async()
        {
            try
            {
                _logger.LogInformation($"Starting step 6 of DFC.ServiceTaxonomy.ExploreCareersMigration");
                await _recipeMigrator.ExecuteAsync("MigrationRecipes/explore-careers-content-types-migration-amendment-05.recipe.json", this);
                _logger.LogInformation($"Completed step 6 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with step 6 of DFC.ServiceTaxonomy.ExploreCareersMigration");
            }

            return 6;
        }
    }
}
