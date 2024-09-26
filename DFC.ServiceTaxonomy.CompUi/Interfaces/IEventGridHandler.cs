using DFC.ServiceTaxonomy.CompUi.Models;
using DfE.NCS.Framework.Event.Model;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IEventGridHandler
    {
        ContentEventData? CreateEventMessageAsync(Processing processing);
        ContentEventData? CreateEventMessageAsync(RelatedContentData contentData);
        Task SendEventMessageAsync(Processing processing, ContentEventType eventType);
        Task SendEventMessageAsync(RelatedContentData contentData, ContentEventType eventType);
    }
}
