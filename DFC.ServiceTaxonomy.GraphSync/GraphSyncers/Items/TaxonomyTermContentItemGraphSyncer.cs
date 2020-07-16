using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Models;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items
{
    //todo: set up locations
    //todo: location as /x/y/z : syncer with contenttypes to sync for. like filter call highest pri with chained nexts?
    //todo: validation
    public class TaxonomyTermContentItemGraphSyncer : IContentItemGraphSyncer
    {
        private readonly ITaxonomyPartGraphSyncer _taxonomyPartGraphSyncer;
        private const string Terms = "Terms";

        public int Priority => 0;

        // we need separate CanSync & CanValidate methods, because CanSync shouldn't select leaf terms, but CanValidate needs to

        public bool CanSync(ContentItem contentItem)
        {
            // this check means a 'Terms' content type using a hierarchical taxonomy won't sync,
            // but I think orchard core would blow up first anyway ;)
            return ((JObject)contentItem.Content).ContainsKey(Terms)
                   && contentItem.ContentType != Terms;
        }

        public bool CanValidate(ContentItem contentItem)
        {
            // this check means a 'TermPart' content type using a hierarchical taxonomy won't sync,
            // but I think orchard core would blow up first anyway ;)
            return ((JObject)contentItem.Content).ContainsKey(nameof(TermPart))
                   && contentItem.ContentType != nameof(TermPart);
        }

        public TaxonomyTermContentItemGraphSyncer(ITaxonomyPartGraphSyncer taxonomyPartGraphSyncer)
        {
            _taxonomyPartGraphSyncer = taxonomyPartGraphSyncer;
        }

        public async Task AddSyncComponents(IGraphMergeItemSyncContext context)
        {
            await _taxonomyPartGraphSyncer.AddSyncComponents(context.ContentItem.Content, context);
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(
            ContentItem contentItem,
            IValidateAndRepairItemSyncContext context)
        {
            return Task.FromResult((true, ""));
        }
    }
}
