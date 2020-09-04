using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Fields
{
    public interface IContentFieldGraphSyncer
    {
        string FieldTypeName {get;}

        Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context);

        Task AddSyncComponentsDetaching(IGraphMergeContext context)
        {
            return Task.CompletedTask;
        }

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject contentItemField,
            IValidateAndRepairContext context);

        Task AddRelationship(IDescribeRelationshipsContext itemContext)
        {
            return Task.CompletedTask;
        }
    }
}
