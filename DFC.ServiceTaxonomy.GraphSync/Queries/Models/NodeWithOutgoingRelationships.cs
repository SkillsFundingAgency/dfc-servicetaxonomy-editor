using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Queries.Models
{
    public class NodeWithOutgoingRelationships : INodeWithOutgoingRelationships
    {
        public INode SourceNode { get; set; }
        public IEnumerable<IOutgoingRelationship> OutgoingRelationships { get; set; }

        public NodeWithOutgoingRelationships(INode sourceNode, IEnumerable<(IRelationship relationship, INode destinationNode)>? outgoingRelationships)
        {
            SourceNode = sourceNode;
            //todo: can we get null?
            //OutgoingRelationships = outgoingRelationships ?? new List<IOutgoingRelationship>();
            OutgoingRelationships =
                outgoingRelationships.Select(or => new OutgoingRelationship(or.relationship, or.destinationNode));
        }
    }
}
