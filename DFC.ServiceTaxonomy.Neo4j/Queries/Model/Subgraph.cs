using System.Collections.Generic;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Queries.Model
{
    public class Subgraph : ISubgraph
    {
        //todo: change to reference to INode
        public long SelectedNodeId { get; set; }
        public HashSet<INode> Nodes { get; set; }
        public HashSet<IRelationship> Relationships { get; set; }

        public Subgraph()
        {
            Nodes = new HashSet<INode>();
            Relationships = new HashSet<IRelationship>();
        }

        public Subgraph(IEnumerable<INode> nodes, IEnumerable<IRelationship> relationships)
        {
            Nodes = new HashSet<INode>(nodes);
            Relationships = new HashSet<IRelationship>(relationships);
        }

        public void Add(ISubgraph subgraph)
        {
            //todo: check both have same sourcenode
            // if different/ null?????
            Nodes.UnionWith(subgraph.Nodes);
            Relationships.UnionWith(subgraph.Relationships);
        }
    }
}
