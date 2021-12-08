using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries;

namespace DFC.ServiceTaxonomy.GraphSync.Models
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
