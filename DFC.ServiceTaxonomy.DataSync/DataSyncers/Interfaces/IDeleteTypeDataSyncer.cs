using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces
{
    public interface IDeleteTypeDataSyncer
    {
        Task DeleteNodesByType(string dataSyncReplicaSetName, string contentType);
    }
}
