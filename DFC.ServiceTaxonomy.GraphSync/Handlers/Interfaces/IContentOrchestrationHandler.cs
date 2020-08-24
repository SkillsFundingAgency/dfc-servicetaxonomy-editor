using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces
{
    public interface IContentOrchestrationHandler
    {
        Task DraftSaved(ContentItem contentItem);
        Task Published(ContentItem contentItem);
        Task Unpublished(ContentItem contentItem);
        Task Cloned(ContentItem contentItem);
        Task Deleted(ContentItem contentItem);
        Task DraftDiscarded(ContentItem contentItem);
    }
}
