using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Items
{
    public interface IContentItemGraphSyncer
    {
        int Priority { get; }

        bool CanSync(ContentItem contentItem);

        Task AllowSync(IGraphMergeItemSyncContext context, IAllowSyncResult allowSyncResult);
        Task AddSyncComponents(IGraphMergeItemSyncContext context);

        Task AllowDelete(IGraphDeleteItemSyncContext context, IAllowSyncResult allowSyncResult);
        Task DeleteComponents(IGraphDeleteItemSyncContext context);

        Task MutateOnClone(ICloneItemSyncContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(IValidateAndRepairItemSyncContext context);
        Task AddRelationship(IDescribeRelationshipsContext context);
    }
}
