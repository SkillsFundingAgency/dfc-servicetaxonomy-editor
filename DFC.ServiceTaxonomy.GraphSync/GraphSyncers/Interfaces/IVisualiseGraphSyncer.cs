using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IVisualiseGraphSyncer
    {
        // public string? SourceNodeId { get; }
        // public IEnumerable<string>? SourceNodeLabels { get; }
        // public string? SourceNodeIdPropertyName { get; }

        //Task<IEnumerable<IQuery<INodeAndOutRelationshipsAndTheirInRelationships>>> BuildVisualisationCommands(string contentItemId, IContentItemVersion contentItemVersion);
        //Task<IEnumerable<IQuery<object?>>> BuildVisualisationCommands(string contentItemId, IContentItemVersion contentItemVersion);

        Task<GraphDataset> GetData(string contentItemId, string graphName, IContentItemVersion contentItemVersion);
    }
}
