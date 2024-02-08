using DFC.ServiceTaxonomy.CompUi.Model;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface ICacheHandler
    {
        Task ProcessPublishedAsync(PublishContentContext context);

        Task ProcessDraftSavedAsync(SaveDraftContentContext context);

        Task ProcessRemovedAsync(RemoveContentContext context);

        Task ProcessUnpublishedAsync(PublishContentContext context);

        string ResolvePublishNodeId(string nodeId, string content, string contentType);

        string ResolveDraftNodeId(NodeItem nodeItem, string contentType);
    }
}
