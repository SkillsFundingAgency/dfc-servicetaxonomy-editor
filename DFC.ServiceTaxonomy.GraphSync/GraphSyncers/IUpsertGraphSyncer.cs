using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public interface IUpsertGraphSyncer
    {
        Task SyncToGraph(ContentItem contentItem);
    }
}
