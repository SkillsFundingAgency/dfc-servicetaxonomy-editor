using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.Flows.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.Bag
{
    public class BagPartGraphSyncer : IContentPartGraphSyncer
    {
        private readonly IBagPartEmbeddedContentItemsGraphSyncer _bagPartEmbeddedContentItemsGraphSyncer;
        public string PartName => nameof(BagPart);

        private const string ContainerName = "ContentItems";

        public async Task AllowSync(JObject content, IGraphMergeContext context, IAllowSyncResult allowSyncResult)
        {
            await _bagPartEmbeddedContentItemsGraphSyncer.AllowSync((JArray?)content[ContainerName], context, allowSyncResult);
        }

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
