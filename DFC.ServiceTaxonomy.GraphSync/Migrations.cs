using System;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ValidateAndRepair;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;

namespace DFC.ServiceTaxonomy.GraphSync
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
            _contentDefinitionManager.AlterPartDefinition("GraphSyncPart", builder => builder
                .Attachable()
                .WithDescription("Enables the content type to be synced to a graph."));

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateTable(nameof(AuditSyncLog), table => table
                .Column<DateTime?>("LastSynced")
            );

            return 2;
        }
    }
}
