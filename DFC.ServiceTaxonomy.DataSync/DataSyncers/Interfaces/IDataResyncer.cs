using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces
{
    public interface IDataResyncer
    {
        Task ResyncContentItems(string contentType);
    }
}
