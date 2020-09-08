using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;
using System.Linq;
using Microsoft.AspNetCore.Html;

namespace DFC.ServiceTaxonomy.GraphSync.Notifications
{
    public class CustomNotifier : ICustomNotifier
    {
        private readonly ILogger<CustomNotifier> _logger;
        private readonly IList<NotifyEntry> _entries;

        public CustomNotifier(ILogger<CustomNotifier> logger)
        {
            _logger = logger;
            _entries = new List<NotifyEntry>();
        }

        public void Add(NotifyType type, HtmlString userMessage, HtmlString technicalMessage)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Notification '{NotificationType}' with user message '{NotificationUserMessage}' and technical message {NotificationTechnicalMessage}.",
                    type, userMessage, technicalMessage);
            }

            HtmlContentBuilder htmlContentBuilder = new HtmlContentBuilder();

            htmlContentBuilder
                .AppendHtml(userMessage)
                .AppendHtml("<ul class=\"list-group mb-3\"><li class=\"list-group-item\">")
                .AppendHtml(technicalMessage)
                .AppendHtml("</li></ul>");

            _entries.Add(new NotifyEntry { Type = type, Message = htmlContentBuilder });
        }

        public void Add(NotifyType type, LocalizedHtmlString message)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Notification '{NotificationType}' with message '{NotificationMessage}'.", type, message);
            }

            _entries.Add(new NotifyEntry { Type = type, Message = message });
        }

        public IList<NotifyEntry> List()
        {
            if (_entries.Any(x => x.Type == NotifyType.Error))
            {
                return _entries.Where(x => x.Type != NotifyType.Success).ToList();
            }

            return _entries;
        }
    }
}
