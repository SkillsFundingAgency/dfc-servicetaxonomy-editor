using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces
{
    public interface IOutgoingRelationship
    {
        IRelationship Relationship { get; set; }
        INode DestinationNode { get; set; }
    }
}
