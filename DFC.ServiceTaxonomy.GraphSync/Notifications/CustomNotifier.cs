using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        //todo: add custom styles via scss
        //todo: why when there are 2 notifiers do only one (sometimes neither) of the collapses work? and not always the same one!!!!
        public void Add(HtmlString userMessage, HtmlString technicalMessage, Exception? exception = null, NotifyType type = NotifyType.Error)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Notification '{NotificationType}' with user message '{NotificationUserMessage}' and technical message '{NotificationTechnicalMessage}' and exception '{Exception}'}.",
                    type, userMessage, technicalMessage, exception?.ToString() ?? "None");
            }

            HtmlContentBuilder htmlContentBuilder = new HtmlContentBuilder();

            string uniqueId = Guid.NewGuid().ToString("N");
            string onClickFunction = $"click_{uniqueId}";
            string clipboardCopy = TechnicalClipboardCopy(
                Activity.Current.TraceId.ToString(),
                userMessage.ToString(),
                technicalMessage.ToString(),
                exception);

            //fa-angle-double-down, hat-wizard, oil-can, fa-wrench?
            htmlContentBuilder
                .AppendHtml(TechnicalScript(onClickFunction, clipboardCopy))
                .AppendHtml(userMessage)
                .AppendHtml($"<button class=\"close\" style=\"right: 1.25em;\" type=\"button\" data-toggle=\"collapse\" data-target=\"#{uniqueId}\" aria-expanded=\"false\" aria-controls=\"{uniqueId}\"><i class=\"fas fa-wrench\"></i></button>")
                .AppendHtml($"<div class=\"collapse\" style=\"margin-top: 1em;\" id=\"{uniqueId}\">")
                .AppendHtml($"<button onclick=\"{onClickFunction}()\" style=\"float: right; position: relative; left: 3em;\" type=\"button\"><i class=\"fas fa-copy\"></i></button>")
                .AppendHtml($"<p>Trace ID: {Activity.Current.TraceId}</p><p>")
                .AppendHtml(technicalMessage)
                .AppendHtml("</p></div>");

            if (exception != null)
            {
                //todo: monospace
                htmlContentBuilder.AppendHtml($"<div>{exception}</div>");
            }

            _entries.Add(new NotifyEntry { Type = type, Message = htmlContentBuilder });
        }

        //todo: add Trace Id to std notifications?
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

        //todo: one function in ncs.js and call that?
        private IHtmlContent TechnicalScript(string onClickFunction, string clipboardText)
        {
            return new HtmlString(
@$"<script type=""text/javascript"">
function {onClickFunction}() {{
    navigator.clipboard.writeText(`{clipboardText}`).then(function() {{
        console.log(""Copied technical error details to clipboard successfully!"");
    }}, function(err) {{
        console.error(""Unable to write technical error details to clipboard. :-("");
    }});
}}
</script>"
            );
        }

        private string TechnicalClipboardCopy(string traceId, string userMessage, string technicalMessage, Exception? exception = null)
        {
            return
@$"Trace ID          : {traceId}
User Message      : {userMessage}
Technical Message : {technicalMessage}
Exception         : {exception}";
        }
    }
}
