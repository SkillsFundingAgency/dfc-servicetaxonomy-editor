﻿using System;
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
            _contentDefinitionManager.AlterPartDefinitionAsync("GraphSyncPart", builder => builder
                .Attachable()
                .WithDescription("Enables the content type to be synced to a graph."));

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateTableAsync(nameof(AuditSyncLog), table => table
                .Column<DateTime?>("LastSynced")
            );

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateMapIndexTableAsync<GraphSyncPartIndex>(table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("NodeId")
            );

            SchemaBuilder.AlterTableAsync(nameof(GraphSyncPartIndex), table => table
                .CreateIndex("IDX_GraphSyncPartIndex_NodeId", "NodeId")
            );

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.DropTableAsync(nameof(AuditSyncLog));

            return 4;
        }
    }
}
