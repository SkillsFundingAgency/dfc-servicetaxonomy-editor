using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
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

        public bool CanSync(ContentItem contentItem)
        {
            // this check means a 'Terms' content type using a hierarchical taxonomy won't sync,
            // but I think orchard core would blow up first anyway ;)
            return ((JObject)contentItem.Content).ContainsKey(Terms)
                   && contentItem.ContentType != Terms;
        }

        public TaxonomyTermContentItemGraphSyncer(
            ITaxonomyPartGraphSyncer taxonomyPartGraphSyncer)
            //ITermPartGraphSyncer termPartGraphSyncer)
        {
            _taxonomyPartGraphSyncer = taxonomyPartGraphSyncer;
            //_termPartGraphSyncer = termPartGraphSyncer;
        }

        public async Task AddSyncComponents(IGraphMergeItemSyncContext context)
        {
            //todo: concurrent?
            await _taxonomyPartGraphSyncer.AddSyncComponentsForNonRoot(context.ContentItem.Content, context);
            //todo: taxonomy isn't there yet, need to order
            //await _termPartGraphSyncer.AddSyncComponents(context.ContentItem.Content[_termPartGraphSyncer.PartName], context);
        }

        public async Task<(bool validated, string failureReason)> ValidateSyncComponent(
            ContentItem contentItem,
            IValidateAndRepairItemSyncContext context)
        {
            return await _taxonomyPartGraphSyncer.ValidateSyncComponent((JObject)contentItem.Content, context);
        }
    }
}
