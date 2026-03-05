using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields
{
    public interface IContentFieldGraphSyncer
    {
        string FieldTypeName {get;}

        Task AddSyncComponents(JsonObject contentItemField, IGraphMergeContext context);

        Task AddSyncComponentsDetaching(IGraphMergeContext context)
        {
            return Task.CompletedTask;
        }

        Task AddRelationship(JsonObject contentItemField, IDescribeRelationshipsContext itemContext)
        {
            return Task.CompletedTask;
        }

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JsonObject contentItemField,
            IValidateAndRepairContext context);
    }
}
