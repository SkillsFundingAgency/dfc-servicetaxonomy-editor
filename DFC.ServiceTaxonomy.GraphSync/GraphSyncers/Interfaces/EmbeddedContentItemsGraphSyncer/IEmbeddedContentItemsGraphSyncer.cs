using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer
{
    public interface IEmbeddedContentItemsGraphSyncer
    {
        Task AllowSync(JsonArray? contentItems, IGraphMergeContext context, IAllowSync allowSync);
        Task AddSyncComponents(JsonArray? contentItems, IGraphMergeContext context);

        Task AllowSyncDetaching(IGraphMergeContext context, IAllowSync allowSync);
        Task AddSyncComponentsDetaching(IGraphMergeContext context);

        Task AllowDelete(JsonArray? contentItems, IGraphDeleteContext context, IAllowSync allowSync);
        Task DeleteComponents(JsonArray? contentItems, IGraphDeleteContext context);

        Task<ContentItem[]> MutateOnClone(JsonArray? contentItems, ICloneContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JsonArray? contentItems,
            IValidateAndRepairContext context);
        Task AddRelationship(JsonArray? jArray, IDescribeRelationshipsContext context);
    }
}
