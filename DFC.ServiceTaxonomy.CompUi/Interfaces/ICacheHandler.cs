using DFC.ServiceTaxonomy.CompUi.Model;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface ICacheHandler
    {
        Task ProcessPublishedAsync(PublishContentContext context);

        Task ProcessDraftSavedAsync(SaveDraftContentContext context);

        string ResolvePublishNodeId(NodeItem nodeItem, string contentType);

        string ResolveDraftNodeId(NodeItem nodeItem, string contentType);
    }
}
