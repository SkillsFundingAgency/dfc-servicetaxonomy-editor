using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.ValidateAndRepair;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentFieldGraphSyncer
    {
        string FieldTypeName {get;}

        Task AddSyncComponents(JObject contentItemField, IGraphMergeContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject contentItemField, ValidateAndRepairContext context);
    }
}
