using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces
{
    public interface IGetDraftRelationships : IQuery<INodeWithOutgoingRelationships?>
    {
        string? ContentType { get; set; }
        object? IdPropertyValue { get; set; }
    }
}
