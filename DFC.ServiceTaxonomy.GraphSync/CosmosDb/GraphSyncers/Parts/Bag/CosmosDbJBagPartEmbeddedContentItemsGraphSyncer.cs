using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CosmosDb.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.CosmosDb.GraphSyncers.Parts.Bag
{
    public class CosmosDbBagPartEmbeddedContentItemsGraphSyncer : CosmosDbEmbeddedContentItemsGraphSyncer, IBagPartEmbeddedContentItemsGraphSyncer
    {
        public CosmosDbBagPartEmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            ISyncNameProvider statelessSyncNameProvider,
            IServiceProvider serviceProvider,
            ILogger<CosmosDbBagPartEmbeddedContentItemsGraphSyncer> logger)
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
