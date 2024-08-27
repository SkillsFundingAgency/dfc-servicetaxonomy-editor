using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.Migration.Migrations
{
    public class RecipeMigration : DataMigration
    {
        private readonly IRecipeMigrator recipeMigrator;
        private readonly ILogger<RecipeMigration> logger;

        public RecipeMigration(IRecipeMigrator recipeMigrator, ILogger<RecipeMigration> logger)
        {
            this.recipeMigrator = recipeMigrator;
            this.logger = logger;
        }

        public async Task<int> CreateAsync()
        {
            try
            {
                logger.LogInformation($" Started CreateAsync Migration from DFC.ServiceTaxonomy.Migration");

                var recipes = new string[]
                {
                    $"{Constants.ContentItemsLocation}AddHeaderContentItem.recipe.json",
                    $"{Constants.ContentTypesLocation}AddHeaderContentType.recipe.json",
                };

                foreach (var recipe in recipes)
                {
                    await recipeMigrator.ExecuteAsync(recipe, this);
                }
            }
            catch (Exception exception)
            {
                logger.LogError($"CreateAsync Migration failed {exception.Message}", exception);

                throw;
            }

            logger.LogInformation($"Completed CreateAsync Migration from DFC.ServiceTaxonomy.Migration");

             return 1;
        }

        public async Task<int> UpdateFrom1()
        {
            try
            {
                logger.LogInformation($" Started UpdateFrom1 Migration from DFC.ServiceTaxonomy.Migration");

                var recipes = new string[]
                {
                    $"{Constants.ContentItemsLocation}AddFooterContentItem.recipe.json",
                    $"{Constants.ContentTypesLocation}AddFooterContentType.recipe.json",
                };

                foreach (var recipe in recipes)
                {
                    await recipeMigrator.ExecuteAsync(recipe, this);
                }
            }
            catch (Exception exception)
            {
                logger.LogError($"UpdateFrom1 Migration failed {exception.Message}", exception);

                throw;
            }

            logger.LogInformation($"Completed UpdateFrom1 Migration from DFC.ServiceTaxonomy.Migration");

            return 2;
        }
    }
}
