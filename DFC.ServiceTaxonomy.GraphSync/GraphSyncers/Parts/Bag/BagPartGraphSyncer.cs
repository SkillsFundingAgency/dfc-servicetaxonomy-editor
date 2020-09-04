using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Bag
{
    public class BagPartGraphSyncer : EmbeddingContentPartGraphSyncer
    {
        public override string PartName => nameof(BagPart);
        protected override string ContainerName => "ContentItems";

        public BagPartGraphSyncer(IBagPartEmbeddedContentItemsGraphSyncer embeddedContentItemsGraphSyncer)
            : base(embeddedContentItemsGraphSyncer)
        {
        }
    }
}
