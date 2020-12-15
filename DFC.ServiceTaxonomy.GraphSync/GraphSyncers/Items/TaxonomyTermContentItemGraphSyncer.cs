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

        public Task AllowSync(IGraphMergeItemSyncContext context, IAllowSync allowSync)
        {
            return _taxonomyPartGraphSyncer.AllowSync(context.ContentItem.Content, context, allowSync);
        }

        public Task AddSyncComponents(IGraphMergeItemSyncContext context)
        {
            //todo: concurrent?
            return _taxonomyPartGraphSyncer.AddSyncComponentsForNonLeafEmbeddedTerm(context.ContentItem.Content, context);
            //todo: taxonomy isn't there yet, need to order
            //await _termPartGraphSyncer.AddSyncComponents(context.ContentItem.Content[_termPartGraphSyncer.PartName], context);
        }

        public Task AllowDelete(IGraphDeleteItemSyncContext context, IAllowSync allowSync)
        {
            return _taxonomyPartGraphSyncer.AllowDelete(context.ContentItem.Content, context, allowSync);
        }

        public Task DeleteComponents(IGraphDeleteItemSyncContext context)
        {
            return _taxonomyPartGraphSyncer.DeleteComponentsForNonLeafEmbeddedTerm(context.ContentItem.Content, context);
        }

        public Task MutateOnClone(ICloneItemSyncContext context)
        {
            return _taxonomyPartGraphSyncer.MutateOnClone(context.ContentItem.Content, context);
        }

        public Task AddRelationship(IDescribeRelationshipsItemSyncContext context)
        {
            return _taxonomyPartGraphSyncer.AddRelationship(context.ContentItem.Content, context);
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(
            IValidateAndRepairItemSyncContext context)
        {
            return _taxonomyPartGraphSyncer.ValidateSyncComponentForNonLeafEmbeddedTerm((JObject)context.ContentItem.Content, context);
        }
    }
}
