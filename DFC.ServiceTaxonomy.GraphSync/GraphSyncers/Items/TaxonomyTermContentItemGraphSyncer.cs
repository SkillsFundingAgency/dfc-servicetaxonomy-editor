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

        public bool CanHandle(ContentItem contentItem)
        {
            // this check means a 'Terms' content type using a hierarchical taxonomy won't sync,
            // but I think orchard core would blow up first anyway ;)
            return ((JObject)contentItem.Content).ContainsKey(Terms)
                   && contentItem.ContentType != Terms;
        }

        public Task AddSyncComponents(ContentItem contentItem, IGraphMergeContext context) => throw new System.NotImplementedException();

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(ContentItem contentItem, IValidateAndRepairContext context) => throw new System.NotImplementedException();
    }
}
