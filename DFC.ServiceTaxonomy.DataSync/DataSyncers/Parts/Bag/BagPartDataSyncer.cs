using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.EmbeddedContentItemsDataSyncer;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts.Bag
{
    public class BagPartDataSyncer : EmbeddingContentPartDataSyncer
    {
        public override string PartName => nameof(BagPart);
        protected override string ContainerName => "ContentItems";

        public BagPartDataSyncer(IBagPartEmbeddedContentItemsDataSyncer embeddedContentItemsDataSyncer)
            : base(embeddedContentItemsDataSyncer)
        {
        }
    }
}
