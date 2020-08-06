using DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Models
{
    public class OutgoingRelationship : IOutgoingRelationship
    {
        public IRelationship Relationship { get; set; }
        public INode DestinationNode { get; set; }

        public OutgoingRelationship(IRelationship relationship, INode destinationNode)
        {
            Relationship = relationship;
            DestinationNode = destinationNode;
        }
    }
}
