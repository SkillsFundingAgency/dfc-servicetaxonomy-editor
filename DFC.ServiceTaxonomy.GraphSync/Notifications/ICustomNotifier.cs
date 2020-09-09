using System;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Notifications
{
    public interface ICustomNotifier : INotifier
    {
        void Add(
            HtmlString userMessage,
            HtmlString technicalMessage,
            Exception? exception = null,
            NotifyType type = NotifyType.Error);
    }
}
