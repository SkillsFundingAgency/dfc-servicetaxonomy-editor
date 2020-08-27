
namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Contexts
{
    public interface ICloneContext : IGraphSyncContext
    {
        public ICloneGraphSync CloneGraphSync { get; }
    }
}
