using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentItemGraphSyncer
    {
        int Priority { get; }

        bool CanSync(ContentItem contentItem);

        Task<bool> AllowSync(IGraphMergeItemSyncContext context) => Task.FromResult(true);
//        Task<bool> AllowSync(ContentItem contentItem, IGraphMergeContext context) => Task.FromResult(true);

        Task AddSyncComponents(IGraphMergeItemSyncContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            ContentItem contentItem,
            IValidateAndRepairItemSyncContext context);
    }
}
