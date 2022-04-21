using DFC.ServiceTaxonomy.DataSync.Interfaces;
using DFC.ServiceTaxonomy.DataSync.Interfaces.Queries;

namespace DFC.ServiceTaxonomy.DataSync.Models
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
