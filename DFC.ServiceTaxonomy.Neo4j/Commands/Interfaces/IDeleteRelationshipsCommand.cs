
namespace DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces
{
    public interface IDeleteRelationshipsCommand : INodeWithOutgoingRelationshipsCommand
    {
        bool DeleteDestinationNodes { get; set; }
    }
}
