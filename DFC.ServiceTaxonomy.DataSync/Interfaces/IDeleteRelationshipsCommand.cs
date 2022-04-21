namespace DFC.ServiceTaxonomy.DataSync.Interfaces
{
    public interface IDeleteRelationshipsCommand : INodeWithOutgoingRelationshipsCommand
    {
        bool DeleteDestinationNodes { get; set; }
    }
}
