using DFC.ServiceTaxonomy.CompUi.Models;
using DfE.NCS.Framework.Event.Model;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface ICacheHandler
    {
        Task ProcessPublishedAsync(PublishContentContext context);

        Task ProcessDraftSavedAsync(SaveDraftContentContext context);

        Task ProcessRemovedAsync(RemoveContentContext context);

        Task ProcessUnpublishedAsync(PublishContentContext context);

        Task ProcessEventGridMessage(Processing processing, ContentEventType contentEventType);
    }
}
