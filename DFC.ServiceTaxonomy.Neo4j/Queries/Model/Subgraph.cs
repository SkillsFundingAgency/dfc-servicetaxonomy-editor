using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Queries.Model
{
    /// <inheritdoc/>
    public class Subgraph : ISubgraph
    {
        //todo: immutable, with non-nullable sourcenode?
        public INode? SourceNode { get; set; }
        public HashSet<INode> Nodes { get; }
        public HashSet<IRelationship> Relationships { get; }

        private IRelationship[]? _outgoingRelationships;
        private IRelationship[]? _incomingRelationships;

        //todo: can we remove this ctor?
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

        public IEnumerable<IRelationship> OutgoingRelationships
        {
            get
            {
                if (_outgoingRelationships != null)
                    return _outgoingRelationships;

                //todo: null SourceNode
                return _outgoingRelationships = Relationships
                    .Where(r => r.StartNodeId == SourceNode!.Id)
                    .ToArray();
            }
        }

        public IEnumerable<IRelationship> IncomingRelationships
        {
            get
            {
                if (_incomingRelationships != null)
                    return _incomingRelationships;

                //todo: null SourceNode
                return _incomingRelationships = Relationships
                    .Where(r => r.EndNodeId == SourceNode!.Id)
                    .ToArray();
            }
        }
    }
}
