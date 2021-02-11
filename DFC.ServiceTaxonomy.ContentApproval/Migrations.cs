using OrchardCore.Data.Migration;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.ContentApproval
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        // private readonly IRecipeMigrator _recipeMigrator;

        public Migrations(IContentDefinitionManager contentDefinitionManager, IRecipeMigrator recipeMigrator)
        {
            _contentDefinitionManager = contentDefinitionManager;
            // _recipeMigrator = recipeMigrator;
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

        //todo: move recipe into this module
        //todo: is there any reason it can't be moved into Create()?
        // public async Task<int> UpdateFrom1()
        // {
        //     await _recipeMigrator.ExecuteAsync("ft2-82-content-approval-module.recipe.json", this);
        //
        //     return 2;
        // }
    }
}
