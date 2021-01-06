using System;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Indexes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

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

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateMapIndexTable<GraphSyncPartIndex>( table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("NodeId")
            );

            SchemaBuilder.AlterTable(nameof(GraphSyncPartIndex), table => table
                .CreateIndex("IDX_GraphSyncPartIndex_NodeId", "NodeId")
            );

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.DropTable(nameof(AuditSyncLog));

            return 4;
        }
    }
}
