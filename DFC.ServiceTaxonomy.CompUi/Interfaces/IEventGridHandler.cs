using DFC.ServiceTaxonomy.CompUi.Models;
using DfE.NCS.Framework.Event.Model;

namespace DFC.ServiceTaxonomy.CompUi.Interfaces
{
    public interface IEventGridHandler
    {
        ContentEventData? CreateEventMessageAsync(RelatedContentData contentData);
        Task SendEventMessageAsync(RelatedContentData contentData, ContentEventType eventType);
    }
}
