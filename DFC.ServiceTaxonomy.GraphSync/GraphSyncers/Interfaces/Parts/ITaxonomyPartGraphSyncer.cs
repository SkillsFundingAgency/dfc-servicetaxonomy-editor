using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Parts
{
    public interface ITaxonomyPartGraphSyncer : IContentPartGraphSyncer
    {
        Task AddSyncComponentsForNonLeafEmbeddedTerm(JObject content, IGraphMergeContext context);
        Task DeleteComponentsForNonLeafEmbeddedTerm(JObject content, IGraphDeleteContext context);
    }
}
