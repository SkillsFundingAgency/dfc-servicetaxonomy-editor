using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentFieldsGraphSyncer
    {
        Task<bool> AllowSync(JArray? contentItems, IGraphMergeContext context) => Task.FromResult(true);

        Task AddSyncComponents(JObject content, IGraphMergeContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(JObject content,
            IValidateAndRepairContext context);
    }
}
