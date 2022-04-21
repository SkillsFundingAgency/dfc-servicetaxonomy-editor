
namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts
{
    public interface ICloneContext : IDataSyncContext
    {
        ICloneDataSync CloneDataSync { get; }
    }
}
