using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces
{
    public interface ISyncOrchestrator
    {
        Task<bool> SaveDraft(ContentItem contentItem);
        Task<bool> Publish(ContentItem contentItem);
        Task<bool> Update(ContentItem publishedContentItem, ContentItem previewContentItem);
        Task<bool> DiscardDraft(ContentItem contentItem);
        Task<bool> Clone(ContentItem contentItem);
    }
}
