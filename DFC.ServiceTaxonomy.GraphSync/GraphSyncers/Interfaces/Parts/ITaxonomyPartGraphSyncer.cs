using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts
{
    public interface ITaxonomyPartGraphSyncer : IContentPartGraphSyncer
    {
        Task AddSyncComponentsForNonLeafEmbeddedTerm(JsonObject content, IGraphMergeContext context);
        Task DeleteComponentsForNonLeafEmbeddedTerm(JsonObject content, IGraphDeleteContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponentForNonLeafEmbeddedTerm(
            JsonObject content,
            IValidateAndRepairContext context);
    }
}
