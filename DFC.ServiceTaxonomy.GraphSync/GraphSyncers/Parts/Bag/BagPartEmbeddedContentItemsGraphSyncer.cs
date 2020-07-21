using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Bag
{
    public class BagPartEmbeddedContentItemsGraphSyncer : EmbeddedContentItemsGraphSyncer, IBagPartEmbeddedContentItemsGraphSyncer
    {
        public BagPartEmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
            : base(contentDefinitionManager, serviceProvider)
        {
        }

        protected override IEnumerable<string> GetEmbeddableContentTypes(
            ContentItem contentItem,
            ContentTypePartDefinition contentTypePartDefinition)
        {
            BagPartSettings bagPartSettings = contentTypePartDefinition.GetSettings<BagPartSettings>();
            return bagPartSettings.ContainedContentTypes;
        }

        protected override async Task<string> RelationshipType(IGraphSyncHelper embeddedContentGraphSyncHelper)
        {
            //todo: what if want different relationships for same contenttype in different bags!
            string? relationshipType = embeddedContentGraphSyncHelper.GraphSyncPartSettings.BagPartContentItemRelationshipType;
            if (string.IsNullOrEmpty(relationshipType))
                relationshipType = await base.RelationshipType(embeddedContentGraphSyncHelper);

            return relationshipType;
        }
    }
}
