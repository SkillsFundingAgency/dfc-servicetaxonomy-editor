using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items
{
    public class TaxonomyTermContentItemGraphSyncer : IContentItemGraphSyncer
    {
        public bool CanHandle(ContentItem contentItem) => throw new System.NotImplementedException();

        public Task AddSyncComponents(ContentItem contentItem, IGraphMergeContext context) => throw new System.NotImplementedException();

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(ContentItem contentItem, IValidateAndRepairContext context) => throw new System.NotImplementedException();
    }
}
