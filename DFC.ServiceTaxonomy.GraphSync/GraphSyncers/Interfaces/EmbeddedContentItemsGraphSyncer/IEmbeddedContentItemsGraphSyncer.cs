using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Contexts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.EmbeddedContentItemsGraphSyncer
{
    public interface IEmbeddedContentItemsGraphSyncer
    {
        Task AddSyncComponents(JArray? contentItems, IGraphMergeContext context);

        Task<(bool validated, string failureReason)> ValidateSyncComponent(JArray? contentItems, ValidateAndRepairContext context);
    }
}
