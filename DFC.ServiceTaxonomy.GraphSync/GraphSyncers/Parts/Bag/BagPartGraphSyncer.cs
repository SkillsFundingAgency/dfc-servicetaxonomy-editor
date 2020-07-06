using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using Newtonsoft.Json.Linq;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Bag
{
    public class BagPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IBagPartEmbeddedContentItemsGraphSyncer _bagPartEmbeddedContentItemsGraphSyncer;
        public string PartName => nameof(BagPart);

        private const string ContainerName = "ContentItems";

        public BagPartGraphSyncer(IBagPartEmbeddedContentItemsGraphSyncer bagPartEmbeddedContentItemsGraphSyncer)
        {
            _bagPartEmbeddedContentItemsGraphSyncer = bagPartEmbeddedContentItemsGraphSyncer;
        }

        public async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            await _bagPartEmbeddedContentItemsGraphSyncer.AddSyncComponents((JArray?)content[ContainerName], context);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context)
        {
            return await _bagPartEmbeddedContentItemsGraphSyncer.ValidateSyncComponent(
                (JArray?)content[ContainerName], context);
        }
    }
}
