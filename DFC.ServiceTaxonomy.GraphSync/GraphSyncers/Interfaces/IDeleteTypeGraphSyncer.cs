using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IDeleteTypeGraphSyncer
    {
        Task DeleteNodesByType(string graphReplicaSetName, string contentType);
    }
}
