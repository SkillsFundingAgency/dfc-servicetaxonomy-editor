using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Notifications
{
    public interface ICustomNotifier : INotifier
    {
        void Add(NotifyType type, HtmlString userMessage, HtmlString technicalMessage);
    }
}
