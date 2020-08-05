using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Queries.Interfaces
{
    //don't have to worry about twoWay, as content picker doesn't support yet
    public interface IGetIncomingContentPickerRelationships : IQuery<INodeWithOutgoingRelationships?>
    {
        string? ContentType { get; set; }
        object? IdPropertyValue { get; set; }
    }
}
