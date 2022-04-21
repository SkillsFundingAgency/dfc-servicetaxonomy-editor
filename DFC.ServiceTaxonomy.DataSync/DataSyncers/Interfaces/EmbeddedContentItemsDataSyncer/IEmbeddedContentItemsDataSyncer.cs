using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.EmbeddedContentItemsDataSyncer
{
    public interface IEmbeddedContentItemsDataSyncer
    {
        Task AllowSync(JArray? contentItems, IDataMergeContext context, IAllowSync allowSync);
        Task AddSyncComponents(JArray? contentItems, IDataMergeContext context);

        Task AllowSyncDetaching(IDataMergeContext context, IAllowSync allowSync);
        Task AddSyncComponentsDetaching(IDataMergeContext context);

        Task AllowDelete(JArray? contentItems, IDataDeleteContext context, IAllowSync allowSync);
        Task DeleteComponents(JArray? contentItems, IDataDeleteContext context);

        Task<ContentItem[]> MutateOnClone(JArray? contentItems, ICloneContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JArray? contentItems,
            IValidateAndRepairContext context);
        Task AddRelationship(JArray? jArray, IDescribeRelationshipsContext context);
    }
}
