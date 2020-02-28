using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public interface IDeleteGraphSyncer
    {
        Task DeleteFromGraph(ContentItem contentItem);
    }
}
