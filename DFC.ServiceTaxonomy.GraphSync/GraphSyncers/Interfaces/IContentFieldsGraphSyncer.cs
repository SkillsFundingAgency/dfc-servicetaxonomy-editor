using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentFieldsGraphSyncer
    {
        Task AddSyncComponents(JObject content, IGraphMergeContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content, ValidateAndRepairContext context);
    }
}
