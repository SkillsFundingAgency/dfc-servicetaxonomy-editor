using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.CosmosDb.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.EmbeddedContentItemsDataSyncer;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.DataSync.CosmosDb.DataSyncers.Parts.Bag
{
    public class CosmosDbBagPartEmbeddedContentItemsDataSyncer : CosmosDbEmbeddedContentItemsDataSyncer, IBagPartEmbeddedContentItemsDataSyncer
    {
        public CosmosDbBagPartEmbeddedContentItemsDataSyncer(
            IContentDefinitionManager contentDefinitionManager,
            ISyncNameProvider statelessSyncNameProvider,
            IServiceProvider serviceProvider,
            ILogger<CosmosDbBagPartEmbeddedContentItemsDataSyncer> logger)
            : base(contentDefinitionManager, statelessSyncNameProvider, serviceProvider, logger)
        {
        }

        protected override IEnumerable<string> GetEmbeddableContentTypes(IDataSyncContext context)
        {
            BagPartSettings bagPartSettings = context.ContentTypePartDefinition.GetSettings<BagPartSettings>();
            return bagPartSettings.ContainedContentTypes;
        }

        protected override async Task<string> RelationshipType(string contentType)
        {
            //todo: what if want different relationships for same contenttype in different bags!
            string? relationshipType = _statelessSyncNameProvider
                .GetDataSyncPartSettings(contentType)
                .BagPartContentItemRelationshipType;

            if (string.IsNullOrEmpty(relationshipType))
                relationshipType = await base.RelationshipType(contentType);

            return relationshipType;
        }
    }
}
