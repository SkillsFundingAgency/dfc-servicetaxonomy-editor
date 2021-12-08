namespace DFC.ServiceTaxonomy.GraphSync.Interfaces
{
    public interface IDeleteRelationshipsCommand : INodeWithOutgoingRelationshipsCommand
    {
        bool DeleteDestinationNodes { get; set; }
    }
}
