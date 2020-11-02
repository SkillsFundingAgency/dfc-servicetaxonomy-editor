using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.Neo4j.Queries.Model;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IVisualiseGraphSyncer
    {
        Task<Subgraph> GetVisualisationSubgraph(string contentItemId, string graphName, IContentItemVersion contentItemVersion);
    }
}
