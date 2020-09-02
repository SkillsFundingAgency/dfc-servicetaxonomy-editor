using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers
{
    public abstract class EmbeddingContentPartGraphSyncer : ContentPartGraphSyncer
    {
        protected abstract string ContainerName { get; }
        protected readonly IEmbeddedContentItemsGraphSyncer _embeddedContentItemsGraphSyncer;

        protected EmbeddingContentPartGraphSyncer(IEmbeddedContentItemsGraphSyncer embeddedContentItemsGraphSyncer)
        {
            _embeddedContentItemsGraphSyncer = embeddedContentItemsGraphSyncer;
        }

        public override async Task AllowSync(JObject content, IGraphMergeContext context, IAllowSyncResult allowSyncResult)
        {
            await _embeddedContentItemsGraphSyncer.AllowSync((JArray?)content[ContainerName], context, allowSyncResult);
        }

        public override async Task AddSyncComponents(JObject content, IGraphMergeContext context)
        {
            await _embeddedContentItemsGraphSyncer.AddSyncComponents((JArray?)content[ContainerName], context);
        }

        public override async Task AddSyncComponentsDetaching(IGraphMergeContext context)
        {
            await _embeddedContentItemsGraphSyncer.AddSyncComponentsDetaching(context);
        }

        public override async Task AllowDelete(JObject content, IGraphDeleteContext context, IAllowSyncResult allowSyncResult)
        {
            await _embeddedContentItemsGraphSyncer.AllowDelete((JArray?)content[ContainerName], context, allowSyncResult);
        }

        public override async Task DeleteComponents(JObject content, IGraphDeleteContext context)
        {
            await _embeddedContentItemsGraphSyncer.DeleteComponents((JArray?)content[ContainerName], context);
        }

        public override async Task MutateOnClone(JObject content, ICloneContext context)
        {
            var mutatedContentItems = await _embeddedContentItemsGraphSyncer.MutateOnClone((JArray?)content[ContainerName], context);
            var mutatedContentItemsJArray = JArray.FromObject(mutatedContentItems);
            content[ContainerName] = mutatedContentItemsJArray;

            // we could return the mutated contentitems from this method and the derived class could use Alter with the appropriate part
            // context.ContentItem.Alter<FlowPart>(p => p.Widgets = mutatedContentItems.ToList());
        }

        public override async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            return await _embeddedContentItemsGraphSyncer.ValidateSyncComponent(
                (JArray?)content[ContainerName], context);
        }
    }
}
