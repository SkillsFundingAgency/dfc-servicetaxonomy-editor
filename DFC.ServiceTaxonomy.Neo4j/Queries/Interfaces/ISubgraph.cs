using System.Collections.Generic;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces
{
    public interface ISubgraph
    {
        //todo: change to reference to INode
        long SelectedNodeId { get; }
        //todo: enumerable/hashset
        HashSet<INode> Nodes { get; }
        HashSet<IRelationship> Relationships  { get; }
    }
}
