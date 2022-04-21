using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.EmbeddedContentItemsDataSyncer;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers
{
    public abstract class EmbeddingContentPartDataSyncer : ContentPartDataSyncer
    {
        protected abstract string ContainerName { get; }
        protected readonly IEmbeddedContentItemsDataSyncer _embeddedContentItemsDataSyncer;

        protected EmbeddingContentPartDataSyncer(IEmbeddedContentItemsDataSyncer embeddedContentItemsDataSyncer)
        {
            _embeddedContentItemsDataSyncer = embeddedContentItemsDataSyncer;
        }

        public override Task AllowSync(JObject content, IDataMergeContext context, IAllowSync allowSync)
        {
            return _embeddedContentItemsDataSyncer.AllowSync((JArray?)content[ContainerName], context, allowSync);
        }

        public override Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            return _embeddedContentItemsDataSyncer.AddSyncComponents((JArray?)content[ContainerName], context);
        }

        public override Task AllowSyncDetaching(IDataMergeContext context, IAllowSync allowSync)
        {
            return _embeddedContentItemsDataSyncer.AllowSyncDetaching(context, allowSync);
        }

        public override Task AddSyncComponentsDetaching(IDataMergeContext context)
        {
            return _embeddedContentItemsDataSyncer.AddSyncComponentsDetaching(context);
        }

        public override Task AllowDelete(JObject content, IDataDeleteContext context, IAllowSync allowSync)
        {
            return _embeddedContentItemsDataSyncer.AllowDelete((JArray?)content[ContainerName], context, allowSync);
        }

        public override Task DeleteComponents(JObject content, IDataDeleteContext context)
        {
            return _embeddedContentItemsDataSyncer.DeleteComponents((JArray?)content[ContainerName], context);
        }

        public override async Task MutateOnClone(JObject content, ICloneContext context)
        {
            var mutatedContentItems = await _embeddedContentItemsDataSyncer.MutateOnClone((JArray?)content[ContainerName], context);
            var mutatedContentItemsJArray = JArray.FromObject(mutatedContentItems);
            content[ContainerName] = mutatedContentItemsJArray;

            // we could return the mutated contentitems from this method and the derived class could use Alter with the appropriate part
            // context.ContentItem.Alter<FlowPart>(p => p.Widgets = mutatedContentItems.ToList());
        }

        public override Task AddRelationship(JObject content, IDescribeRelationshipsContext context)
        {
            return _embeddedContentItemsDataSyncer.AddRelationship((JArray?)content[ContainerName], context);
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            return _embeddedContentItemsDataSyncer.ValidateSyncComponent(
                (JArray?)content[ContainerName], context);
        }
    }
}
