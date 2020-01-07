using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public interface IGraphSyncer
    {
        Task SyncToGraph(ContentItem contentItem);
    }
}
