using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Queries.Models
{
    public interface IOutgoingRelationship
    {
        IRelationship Relationship { get; set; }
        INode DestinationNode { get; set; }
    }
}
