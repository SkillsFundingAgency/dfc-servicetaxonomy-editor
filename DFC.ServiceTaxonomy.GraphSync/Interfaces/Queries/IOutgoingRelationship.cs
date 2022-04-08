namespace DFC.ServiceTaxonomy.GraphSync.Interfaces.Queries
{
    public interface IOutgoingRelationship
    {
        IRelationship Relationship { get; set; }
        INode DestinationNode { get; set; }
    }
}
