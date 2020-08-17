namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface IGraphSyncContext : IGraphOperationContext
    {
        IGraphSyncContext? ParentContext { get; }
    }
}
