using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Items
{
    public class ContentItemGraphSyncer : IContentItemGraphSyncer
    {
        public int Priority => int.MinValue;

        public bool CanHandle(ContentItem contentItem) => true;

        public Task AddSyncComponents(ContentItem contentItem, IGraphMergeContext context) => throw new System.NotImplementedException();

        public Task<(bool validated, string failureReason)> ValidateSyncComponent(ContentItem contentItem, IValidateAndRepairContext context) => throw new System.NotImplementedException();
    }
}
