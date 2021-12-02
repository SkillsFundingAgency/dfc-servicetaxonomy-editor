using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.JobProfiles.Module
{
    [Feature("DFC.ServiceTaxonomy.JobProfiles")]
    public class Migrations : DataMigration
    {
        private readonly IRecipeMigrator _recipeMigrator;

        public Migrations(IRecipeMigrator recipeMigrator)
        {
            _recipeMigrator = recipeMigrator;
        }

        public async Task<int> CreateAsync()
        {
            await _recipeMigrator.ExecuteAsync("job-profiles-content-types.recipe.json", this);

            return 1;
        }
    }
}
