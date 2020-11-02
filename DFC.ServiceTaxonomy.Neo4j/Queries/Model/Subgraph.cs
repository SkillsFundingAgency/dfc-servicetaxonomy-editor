using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Queries.Model
{
    public class Subgraph : ISubgraph
    {
        public INode? SourceNode { get; set; }
        public HashSet<INode> Nodes { get; set; }
        public HashSet<IRelationship> Relationships { get; set; }

        public Subgraph()
        {
            SourceNode = null;
            Nodes = new HashSet<INode>();
            Relationships = new HashSet<IRelationship>();
        }

        public Subgraph(IEnumerable<INode> nodes, IEnumerable<IRelationship> relationships, INode? sourceNode = null)
        {
            Nodes = new HashSet<INode>(nodes);
            Relationships = new HashSet<IRelationship>(relationships);
            SourceNode = sourceNode;
        }

        public void Add(ISubgraph subgraph)
        {
            Nodes.UnionWith(subgraph.Nodes);
            Relationships.UnionWith(subgraph.Relationships);
            // options:
            // use SourceNode from new Subgraph
            // keep original SourceNode
            // validate both same (& throw if not)
            // set to null if different
            SourceNode = subgraph.SourceNode;
        }
    }
}
