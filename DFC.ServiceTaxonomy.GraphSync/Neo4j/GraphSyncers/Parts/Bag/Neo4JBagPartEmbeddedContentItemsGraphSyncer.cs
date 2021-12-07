using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.GraphSyncers.Helpers;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.GraphSyncers.Parts.Bag
{
    public class Neo4JBagPartEmbeddedContentItemsGraphSyncer : Neo4JEmbeddedContentItemsGraphSyncer, IBagPartEmbeddedContentItemsGraphSyncer
    {
        public Neo4JBagPartEmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            ISyncNameProvider statelessSyncNameProvider,
            IServiceProvider serviceProvider,
            ILogger<Neo4JBagPartEmbeddedContentItemsGraphSyncer> logger)
            : base(contentDefinitionManager, statelessSyncNameProvider, serviceProvider, logger)
        {
        }

        protected override IEnumerable<string> GetEmbeddableContentTypes(IGraphSyncContext context)
        {
            BagPartSettings bagPartSettings = context.ContentTypePartDefinition.GetSettings<BagPartSettings>();
            return bagPartSettings.ContainedContentTypes;
        }

        protected override async Task<string> RelationshipType(string contentType)
        {
            //todo: what if want different relationships for same contenttype in different bags!
            string? relationshipType = _statelessSyncNameProvider
                .GetGraphSyncPartSettings(contentType)
                .BagPartContentItemRelationshipType;

            if (string.IsNullOrEmpty(relationshipType))
                relationshipType = await base.RelationshipType(contentType);

            return relationshipType;
        }
    }
}
