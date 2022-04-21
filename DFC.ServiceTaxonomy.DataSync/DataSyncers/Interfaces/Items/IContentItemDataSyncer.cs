using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Items
{
    public interface IContentItemDataSyncer
    {
        int Priority { get; }

        bool CanSync(ContentItem contentItem);

        Task AllowSync(IDataMergeItemSyncContext context, IAllowSync allowSync);
        Task AddSyncComponents(IDataMergeItemSyncContext context);

        Task AllowDelete(IDataDeleteItemSyncContext context, IAllowSync allowSync);
        Task DeleteComponents(IDataDeleteItemSyncContext context);

        Task MutateOnClone(ICloneItemSyncContext context);

        Task AddRelationship(IDescribeRelationshipsItemSyncContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(IValidateAndRepairItemSyncContext context);
    }
}
