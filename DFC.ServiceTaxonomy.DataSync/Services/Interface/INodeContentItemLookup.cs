using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.DataSync.Services.Interface
{
    public interface INodeContentItemLookup
    {
        Task<string?> GetContentItemId(string nodeId, string graphReplicaSetName);
    }
}
