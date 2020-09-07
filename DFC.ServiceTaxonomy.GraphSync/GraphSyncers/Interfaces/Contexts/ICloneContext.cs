
namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface ICloneContext : IGraphSyncContext
    {
        ICloneGraphSync CloneGraphSync { get; }
    }
}
