using System;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.FlowPart
{
    public class FlowPartEmbeddedContentItemsGraphSyncer : EmbeddedContentItemsGraphSyncer, IFlowPartEmbeddedContentItemsGraphSyncer
    {
        public FlowPartEmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
            : base(contentDefinitionManager, serviceProvider)
        {
        }
    }
}
