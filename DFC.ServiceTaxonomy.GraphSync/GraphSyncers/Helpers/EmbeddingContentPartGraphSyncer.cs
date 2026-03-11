using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts;

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

        public override Task AllowSync(JsonObject content, IGraphMergeContext context, IAllowSync allowSync)
        {
            return _embeddedContentItemsGraphSyncer.AllowSync((JsonArray?)content[ContainerName], context, allowSync);
        }

        public override Task AddSyncComponents(JsonObject content, IGraphMergeContext context)
        {
            return _embeddedContentItemsGraphSyncer.AddSyncComponents((JsonArray?)content[ContainerName], context);
        }

        public override Task AllowSyncDetaching(IGraphMergeContext context, IAllowSync allowSync)
        {
            return _embeddedContentItemsGraphSyncer.AllowSyncDetaching(context, allowSync);
        }

        public override Task AddSyncComponentsDetaching(IGraphMergeContext context)
        {
            return _embeddedContentItemsGraphSyncer.AddSyncComponentsDetaching(context);
        }

        public override Task AllowDelete(JsonObject content, IGraphDeleteContext context, IAllowSync allowSync)
        {
            return _embeddedContentItemsGraphSyncer.AllowDelete((JsonArray?)content[ContainerName], context, allowSync);
        }

        public override Task DeleteComponents(JsonObject content, IGraphDeleteContext context)
        {
            return _embeddedContentItemsGraphSyncer.DeleteComponents((JsonArray?)content[ContainerName], context);
        }

        public override async Task MutateOnClone(JsonObject content, ICloneContext context)
        {
            var mutatedContentItems = await _embeddedContentItemsGraphSyncer.MutateOnClone((JsonArray?)content[ContainerName], context);
            var mutatedContentItemsJArray = JArray.FromObject(mutatedContentItems);
            content[ContainerName] = mutatedContentItemsJArray;

            // we could return the mutated contentitems from this method and the derived class could use Alter with the appropriate part
            // context.ContentItem.Alter<FlowPart>(p => p.Widgets = mutatedContentItems.ToList());
        }

        public override Task AddRelationship(JsonObject content, IDescribeRelationshipsContext context)
        {
            return _embeddedContentItemsGraphSyncer.AddRelationship((JsonArray?)content[ContainerName], context);
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JsonObject content,
            IValidateAndRepairContext context)
        {
            return _embeddedContentItemsGraphSyncer.ValidateSyncComponent(
                (JsonArray?)content[ContainerName], context);
        }
    }
}
