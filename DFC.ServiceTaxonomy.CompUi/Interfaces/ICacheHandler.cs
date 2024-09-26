using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface ICacheHandler
    {
        Task ProcessPublishedAsync(PublishContentContext context);

        Task ProcessDraftSavedAsync(SaveDraftContentContext context);

        Task ProcessRemovedAsync(RemoveContentContext context);

        Task ProcessUnpublishedAsync(PublishContentContext context);

        Task ProcessCreatedAsync(CreateContentContext context);
    }
}
