using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IVisualiseGraphSyncer
    {
        public string? SourceNodeId { get; }
        public IEnumerable<string>? SourceNodeLabels { get; }
        public string? SourceNodeIdPropertyName { get; }

        Task<IEnumerable<IQuery<INodeAndOutRelationshipsAndTheirInRelationships>>> BuildVisualisationCommands(string contentItemId, IContentItemVersion contentItemVersion);
    }
}
