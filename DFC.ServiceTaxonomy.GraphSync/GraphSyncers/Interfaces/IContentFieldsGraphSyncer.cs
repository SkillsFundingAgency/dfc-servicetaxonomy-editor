using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IContentFieldsGraphSyncer
    {
        Task AllowSync(JObject content, IGraphMergeContext context, IAllowSyncResult allowSyncResult)
            => Task.CompletedTask;

        Task AddSyncComponents(JObject content, IGraphMergeContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context);
    }
}
