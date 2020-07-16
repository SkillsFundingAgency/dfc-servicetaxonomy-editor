using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentItemGraphSyncer
    {
        int Priority { get; }

        bool CanSync(ContentItem contentItem);

        Task AddSyncComponents(IGraphMergeItemSyncContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            ContentItem contentItem,
            IValidateAndRepairItemSyncContext context);
    }
}
