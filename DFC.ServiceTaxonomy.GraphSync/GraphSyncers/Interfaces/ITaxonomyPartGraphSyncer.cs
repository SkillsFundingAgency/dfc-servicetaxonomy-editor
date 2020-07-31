using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface ITaxonomyPartGraphSyncer : IContentPartGraphSyncer
    {
        Task AddSyncComponentsForNonRoot(JObject content, IGraphMergeContext context);
    }
}
