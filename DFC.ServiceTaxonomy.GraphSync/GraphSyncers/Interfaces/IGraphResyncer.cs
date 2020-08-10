using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IGraphResyncer
    {
        Task ResyncContentItems(string contentType);
    }
}
