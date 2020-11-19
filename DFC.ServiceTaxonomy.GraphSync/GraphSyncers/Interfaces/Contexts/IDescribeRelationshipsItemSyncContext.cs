
namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IDescribeRelationshipsItemSyncContext : IItemSyncContext, IDescribeRelationshipsContext
    {
        //todo: sort once we swap to c#9
        //new IEnumerable<IDescribeRelationshipsContext> ChildContexts { get; }
    }
}
