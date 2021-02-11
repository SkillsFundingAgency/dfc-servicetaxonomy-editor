using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.ContentApproval
{
    //todo: DashboardPartIndex isn't being created, even though it's in the dashboard migration
    // it works when enabled as a feature through the ui however!
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
            // SchemaBuilder.CreateMapIndexTable<DashboardPartIndex>(table => table
            //     .Column<double>("Position")
            // );

            _contentDefinitionManager.AlterPartDefinition("ContentApprovalDashboardPart", builder => builder
                .Attachable()
                .WithDescription("Adds content approval dashboard cards.")
            );

            return 1;
        }

        //todo: do we need to migrate from roles?

        public async Task<int> UpdateFrom1()
        {
            // this conceptually should live in Create(), but Create() isn't async, so we have it as a update instead
            await _recipeMigrator.ExecuteAsync("stax-content-approval.recipe.json", this);

            return 2;
        }
    }
}
