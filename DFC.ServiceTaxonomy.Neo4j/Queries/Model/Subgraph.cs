using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Queries.Model
{
    public class Subgraph : ISubgraph
    {
        //todo: change to reference to INode
        public long SelectedNodeId { get; set; }
        //todo: enumerable/hashset
        public IEnumerable<INode>? Nodes { get; set; }
        public HashSet<IRelationship>? Relationships  { get; set; }
    }
}
