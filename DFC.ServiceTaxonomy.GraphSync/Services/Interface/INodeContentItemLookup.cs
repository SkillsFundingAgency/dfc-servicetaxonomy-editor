using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface INodeContentItemLookup
    {
        Task<string?> GetContentItemId(string nodeId, string graphReplicaSetName);
    }
}
