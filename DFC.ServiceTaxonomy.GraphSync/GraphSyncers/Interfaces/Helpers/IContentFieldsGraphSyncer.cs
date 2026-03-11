using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers
{
    public interface IContentFieldsGraphSyncer
    {
        Task AllowSync(JsonObject content, IGraphMergeContext context, IAllowSync allowSync)
            => Task.CompletedTask;

        Task AddSyncComponents(JsonObject content, IGraphMergeContext context);

        Task AddRelationship(JsonObject content, IDescribeRelationshipsContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JsonObject content,
            IValidateAndRepairContext context);
    }
}
