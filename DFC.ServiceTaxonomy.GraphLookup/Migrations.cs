using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace DFC.ServiceTaxonomy.GraphLookup
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("GraphLookupPart", builder => builder
                .Attachable()
                .WithDescription("Lookup node(s) from the graph and create relationships and/or set a property based on the selection.")
            );

            return 1;
        }
    }
}
