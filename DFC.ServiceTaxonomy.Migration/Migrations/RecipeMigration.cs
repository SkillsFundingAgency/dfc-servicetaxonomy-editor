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
        public async Task<int> UpdateFrom2()
        {
            try
            {
                logger.LogInformation($" Started UpdateFrom2 Migration from DFC.ServiceTaxonomy.Migration");

                var recipes = new string[]
                {
                    $"{Constants.ContentTypesLocation}AddBasicCardContentType.recipe.json",
                };

                foreach (var recipe in recipes)
                {
                    await recipeMigrator.ExecuteAsync(recipe, this);
                }
            }
            catch (Exception exception)
            {
                logger.LogError($"UpdateFrom2 Migration failed {exception.Message}", exception);

                throw;
            }

            logger.LogInformation($"Completed UpdateFrom2 Migration from DFC.ServiceTaxonomy.Migration");

            return 3;
        }
        public async Task<int> UpdateFrom3()
        {
            try
            {
                logger.LogInformation($" Started UpdateFrom3 Migration from DFC.ServiceTaxonomy.Migration");

                var recipes = new string[]
                {
                    $"{Constants.PlacementsLocation}AllPlacements.recipe.json",
                };

                foreach (var recipe in recipes)
                {
                    await recipeMigrator.ExecuteAsync(recipe, this);
                }
            }
            catch (Exception exception)
            {
                logger.LogError($"UpdateFrom3 Migration failed {exception.Message}", exception);

                throw;
            }

            logger.LogInformation($"Completed UpdateFrom3 Migration from DFC.ServiceTaxonomy.Migration");

            return 4;
        }
        public async Task<int> UpdateFrom4()
        {
            try
            {
                logger.LogInformation($" Started UpdateFrom4 Migration from DFC.ServiceTaxonomy.Migration");

                var recipes = new string[]
                {
                    $"{Constants.TemplatesLocation}AllTemplates.recipe.json",
                };

                foreach (var recipe in recipes)
                {
                    await recipeMigrator.ExecuteAsync(recipe, this);
                }
            }
            catch (Exception exception)
            {
                logger.LogError($"UpdateFrom4 Migration failed {exception.Message}", exception);

                throw;
            }

            logger.LogInformation($"Completed UpdateFrom4 Migration from DFC.ServiceTaxonomy.Migration");

            return 5;
        }
        public async Task<int> UpdateFrom5()
        {
            try
            {
                logger.LogInformation($" Started UpdateFrom5 Migration from DFC.ServiceTaxonomy.Migration");

                var recipes = new string[]
                {
                    $"{Constants.ContentTypesLocation}AddProductCardTypeContentType.recipe.json",
                    $"{Constants.ContentItemsLocation}AddProductCardTypeContentItem.recipe.json",
                    $"{Constants.ContentTypesLocation}AddProductCardContentType.recipe.json",
                    $"{Constants.ContentTypesLocation}AddCardContainerContentType.recipe.json",
                    $"{Constants.ContentTypesLocation}UpdatePageContentType.recipe.json",
                    $"{Constants.TemplatesLocation}AllTemplates.recipe.json",
                    $"{Constants.PlacementsLocation}AllPlacements.recipe.json",
                    $"{Constants.ContentItemsLocation}AddCardsContentItem.recipe.json",
                    $"{Constants.ContentItemsLocation}AddFacHomeAndCoursePageContentItem.recipe.json"
                };

                foreach (var recipe in recipes)
                {
                    await recipeMigrator.ExecuteAsync(recipe, this);
                }
            }
            catch (Exception exception)
            {
                logger.LogError($"UpdateFrom5 Migration failed {exception.Message}", exception);

                throw;
            }

            logger.LogInformation($"Completed UpdateFrom5 Migration from DFC.ServiceTaxonomy.Migration");

            return 6;
        }
        public async Task<int> UpdateFrom6()
        {
            try
            {
                logger.LogInformation($" Started UpdateFrom6 Migration from DFC.ServiceTaxonomy.Migration");

                var recipes = new string[]
                {
                    $"{Constants.TemplatesLocation}AllTemplates.recipe.json",
                    $"{Constants.ContentItemsLocation}AddFacHomeAndCoursePageContentItem.recipe.json"
                };

                foreach (var recipe in recipes)
                {
                    await recipeMigrator.ExecuteAsync(recipe, this);
                }
            }
            catch (Exception exception)
            {
                logger.LogError($"UpdateFrom6 Migration failed {exception.Message}", exception);

                throw;
            }

            logger.LogInformation($"Completed UpdateFrom6 Migration from DFC.ServiceTaxonomy.Migration");

            return 7;
        }

        public async Task<int> UpdateFrom7()
        {
            try
            {
                logger.LogInformation($" Started UpdateFrom7 Migration from DFC.ServiceTaxonomy.Migration");

                var recipes = new string[]
                {
                    $"{Constants.ContentTypesLocation}AddCardContainerContentType.recipe.json",
                    $"{Constants.ContentItemsLocation}AddCardsContentItem.recipe.json",
                    $"{Constants.ContentItemsLocation}AddFacHomeAndCoursePageContentItem.recipe.json"
                };

                foreach (var recipe in recipes)
                {
                    await recipeMigrator.ExecuteAsync(recipe, this);
                }
            }
            catch (Exception exception)
            {
                logger.LogError($"UpdateFrom7 Migration failed {exception.Message}", exception);

                throw;
            }

            logger.LogInformation($"Completed UpdateFrom7 Migration from DFC.ServiceTaxonomy.Migration");

            return 8;
        }
    }
}
