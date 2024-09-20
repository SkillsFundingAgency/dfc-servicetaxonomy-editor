using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.ExploreCareersMigration.Migrations
{
    public class ExploreCareersMigrations : DataMigration
    {
        private readonly IRecipeMigrator _recipeMigrator;

        public ExploreCareersMigrations(IRecipeMigrator recipeMigrator)
        {
            _recipeMigrator = recipeMigrator;
        }

        public async Task<int> CreateAsync()
        {
            await _recipeMigrator.ExecuteAsync("MigrationRecipes/SectorLandingPageContentType.json", this);

            return 1;
        }
    }
}
