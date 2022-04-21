namespace DFC.ServiceTaxonomy.DataSync.Interfaces.Queries
{
    public interface IOutgoingRelationship
    {
        IRelationship Relationship { get; set; }
        INode DestinationNode { get; set; }
    }
}
