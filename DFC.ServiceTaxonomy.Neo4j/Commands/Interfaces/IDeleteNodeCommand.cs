
namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IDeleteNodeCommand : ICommand
    {
        string? ContentType { get; set; }
        string? Uri { get; set; }
    }
}
