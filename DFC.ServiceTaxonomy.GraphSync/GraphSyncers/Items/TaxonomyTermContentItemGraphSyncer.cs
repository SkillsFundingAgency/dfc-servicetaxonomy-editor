using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items
{
    //todo: location as /x/y/z : syncer with contenttypes to sync for. like filter call highest pri with chained nexts?
    public class TaxonomyTermContentItemGraphSyncer : IContentItemGraphSyncer
    {
        private readonly ITaxonomyPartGraphSyncer _taxonomyPartGraphSyncer;
        //private readonly ITermPartGraphSyncer _termPartGraphSyncer;
        private const string Terms = "Terms";

        public int Priority => 0;

        public TaxonomyTermContentItemGraphSyncer(ITaxonomyPartGraphSyncer taxonomyPartGraphSyncer)
        {
            _taxonomyPartGraphSyncer = taxonomyPartGraphSyncer;
        }

        public bool CanSync(ContentItem contentItem)
        {
            // this check means a 'Terms' content type using a hierarchical taxonomy won't sync,
            // but I think orchard core would blow up first anyway ;)
            return ((JObject)contentItem.Content).ContainsKey(Terms)
                   && contentItem.ContentType != Terms;
        }

        public async Task AllowSync(IGraphMergeItemSyncContext context, IAllowSyncResult allowSyncResult)
        {
            await _taxonomyPartGraphSyncer.AllowSync(context.ContentItem.Content, context, allowSyncResult);
        }

        public async Task AddSyncComponents(IGraphMergeItemSyncContext context)
        {
            //todo: concurrent?
            await _taxonomyPartGraphSyncer.AddSyncComponentsForNonLeafEmbeddedTerm(context.ContentItem.Content, context);
            //todo: taxonomy isn't there yet, need to order
            //await _termPartGraphSyncer.AddSyncComponents(context.ContentItem.Content[_termPartGraphSyncer.PartName], context);
        }

        public async Task AllowDelete(IGraphDeleteItemSyncContext context, IAllowSyncResult allowSyncResult)
        {
            await _taxonomyPartGraphSyncer.AllowDelete(context.ContentItem.Content, context, allowSyncResult);
        }

        public async Task DeleteComponents(IGraphDeleteItemSyncContext context)
        {
            await _taxonomyPartGraphSyncer.DeleteComponentsForNonLeafEmbeddedTerm(context.ContentItem.Content, context);
        }

        public Task MutateOnClone(ICloneItemSyncContext context)
        {
            return Task.CompletedTask;
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            IValidateAndRepairItemSyncContext context)
        {
            return await _taxonomyPartGraphSyncer.ValidateSyncComponent((JObject)context.ContentItem.Content, context);
        }
    }
}
