using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces
{
    public interface ISubgraph
    {
        INode? SourceNode { get; }
        HashSet<INode> Nodes { get; }
        HashSet<IRelationship> Relationships  { get; }

        IEnumerable<IRelationship> OutgoingRelationships { get; }
        IEnumerable<IRelationship> IncomingRelationships { get; }
    }
}
