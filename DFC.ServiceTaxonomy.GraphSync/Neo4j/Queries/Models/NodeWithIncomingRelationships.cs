using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models
{
    public class NodeWithIncomingRelationships : INodeWithIncomingRelationships
    {
        public INode SourceNode { get; set; }
        public IEnumerable<IOutgoingRelationship> IncomingRelationships { get; set; }

        public NodeWithIncomingRelationships(INode sourceNode, IEnumerable<(IRelationship relationship, INode destinationNode)> incomingRelationships)
        {
            SourceNode = sourceNode;
            //todo: can we get null?
            IncomingRelationships =
                incomingRelationships.Select(or => new OutgoingRelationship(or.relationship, or.destinationNode));
        }
    }
}
