using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentItemGraphSyncer
    {
        int Priority { get; }

        bool CanHandle(ContentItem contentItem);

        Task AddSyncComponents(ContentItem contentItem, IGraphMergeContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            ContentItem contentItem,
            IValidateAndRepairContext context);
    }
}
