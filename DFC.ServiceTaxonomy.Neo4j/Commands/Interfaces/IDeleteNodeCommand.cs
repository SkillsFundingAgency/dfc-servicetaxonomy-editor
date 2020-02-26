using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IDeleteNodeCommand
    {
        string ContentType { get; set; }
        string Uri { get; set; }
        Query Query { get; }
    }
}
