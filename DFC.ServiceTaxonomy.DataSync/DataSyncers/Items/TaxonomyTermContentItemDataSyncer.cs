using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Items;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Parts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Items
{
    //todo: location as /x/y/z : syncer with contenttypes to sync for. like filter call highest pri with chained nexts?
    public class TaxonomyTermContentItemDataSyncer : IContentItemDataSyncer
    {
        private readonly ITaxonomyPartDataSyncer _taxonomyPartDataSyncer;
        private const string Terms = "Terms";

        public int Priority => 0;

        public TaxonomyTermContentItemDataSyncer(ITaxonomyPartDataSyncer taxonomyPartDataSyncer)
        {
            _taxonomyPartDataSyncer = taxonomyPartDataSyncer;
        }

        public bool CanSync(ContentItem contentItem)
        {
            // this check means a 'Terms' content type using a hierarchical taxonomy won't sync,
            // but I think orchard core would blow up first anyway ;)
            return ((JObject)contentItem.Content).ContainsKey(Terms)
                   && contentItem.ContentType != Terms;
        }

        public Task AllowSync(IDataMergeItemSyncContext context, IAllowSync allowSync)
        {
            return _taxonomyPartDataSyncer.AllowSync(context.ContentItem.Content, context, allowSync);
        }

        public Task AddSyncComponents(IDataMergeItemSyncContext context)
        {
            //todo: concurrent?
            return _taxonomyPartDataSyncer.AddSyncComponentsForNonLeafEmbeddedTerm(context.ContentItem.Content, context);
            //todo: taxonomy isn't there yet, need to order
        }

        public Task AllowDelete(IDataDeleteItemSyncContext context, IAllowSync allowSync)
        {
            return _taxonomyPartDataSyncer.AllowDelete(context.ContentItem.Content, context, allowSync);
        }

        public Task DeleteComponents(IDataDeleteItemSyncContext context)
        {
            return _taxonomyPartDataSyncer.DeleteComponentsForNonLeafEmbeddedTerm(context.ContentItem.Content, context);
        }

        public Task MutateOnClone(ICloneItemSyncContext context)
        {
            return _taxonomyPartDataSyncer.MutateOnClone(context.ContentItem.Content, context);
        }

        public Task AddRelationship(IDescribeRelationshipsItemSyncContext context)
        {
            return _taxonomyPartDataSyncer.AddRelationship(context.ContentItem.Content, context);
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(
            IValidateAndRepairItemSyncContext context)
        {
            return _taxonomyPartDataSyncer.ValidateSyncComponentForNonLeafEmbeddedTerm((JObject)context.ContentItem.Content, context);
        }
    }
}
