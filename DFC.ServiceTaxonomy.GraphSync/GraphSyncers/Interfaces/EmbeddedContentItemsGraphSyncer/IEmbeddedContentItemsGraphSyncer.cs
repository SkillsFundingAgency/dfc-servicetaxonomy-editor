using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer
{
    public interface IEmbeddedContentItemsGraphSyncer
    {
        Task AllowSync(JArray? contentItems, IGraphMergeContext context, IAllowSync allowSync);
        Task AddSyncComponents(JArray? contentItems, IGraphMergeContext context);

        Task AllowSyncDetaching(IGraphMergeContext context, IAllowSync allowSync);
        Task AddSyncComponentsDetaching(IGraphMergeContext context);

        Task AllowDelete(JArray? contentItems, IGraphDeleteContext context, IAllowSync allowSync);
        Task DeleteComponents(JArray? contentItems, IGraphDeleteContext context);

        Task<ContentItem[]> MutateOnClone(JArray? contentItems, ICloneContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JArray? contentItems,
            IValidateAndRepairContext context);
        Task AddRelationship(JArray? jArray, IDescribeRelationshipsContext context);
    }
}
