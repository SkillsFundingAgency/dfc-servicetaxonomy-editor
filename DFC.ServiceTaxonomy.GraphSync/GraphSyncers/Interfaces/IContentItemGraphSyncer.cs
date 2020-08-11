using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentItemGraphSyncer
    {
        int Priority { get; }

        bool CanSync(ContentItem contentItem);

        Task AllowSync(IGraphMergeItemSyncContext context, IAllowSyncResult allowSyncResult);
        Task AddSyncComponents(IGraphMergeItemSyncContext context);

        Task AllowDelete(IGraphDeleteItemSyncContext context, IAllowSyncResult allowSyncResult);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(IValidateAndRepairItemSyncContext context);
    }
}
