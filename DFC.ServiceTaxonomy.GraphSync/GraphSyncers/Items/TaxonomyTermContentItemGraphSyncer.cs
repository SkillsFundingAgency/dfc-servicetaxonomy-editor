using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items
{
    public class TaxonomyTermContentItemGraphSyncer : IContentItemGraphSyncer
    {
        private const string Terms = "Terms";

        public int Priority => 0;

        public bool CanSync(ContentItem contentItem)
        {
            // this check means a 'Terms' content type using a hierarchical taxonomy won't sync,
            // but I think orchard core would blow up first anyway ;)
            return ((JObject)contentItem.Content).ContainsKey(Terms)
                   && contentItem.ContentType != Terms;
        }

        public Task AddSyncComponents(IGraphMergeItemSyncContext context)
        {
            //todo: taxonomy type has all its part definitions returned by GetTypeDefinition :+1:
            // the embedded PageLocation content type, only has some of its part definitions returned by GetTypeDefinition
            // or actually, it returns all the part definitions for the parts its designed with,
            // but it returns a "TermPart" content and a "Terms" content with no corresponding part definition
            // so currently they don't get synced

            /*
             *     "TermPart": {
                      "TaxonomyContentItemId": "4eembshqzx66drajtdten34tc8"
                    },
                    "Terms": [
                      {
                        "ContentItemId": "4pksnz9106ngbwq74w66snan5x",
                        "ContentItemVersionId": null,
             */

            // terms contains recursive content items, (in a similar manner to under TaxonomyPart => Terms in the taxonomy content type)
            // and we'll need to recurse to handle the hierarchy

            // can we tell if the content-type is used as a taxonomy from querying all taxonomies?
            // if so, we can look for TermPart and Terms even though there's no part definition
            // do we fake the definition or take it from the taxonomy?

            return Task.CompletedTask;
        }

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(ContentItem contentItem, IValidateAndRepairContext context) => throw new System.NotImplementedException();
    }
}
