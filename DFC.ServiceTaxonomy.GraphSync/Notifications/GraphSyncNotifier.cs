using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Services;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.Notifications
{
    public class GraphSyncNotifier : IGraphSyncNotifier
    {
        private readonly INodeContentItemLookup _nodeContentItemLookup;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GraphSyncNotifier> _logger;
        private readonly IList<NotifyEntry> _entries;

        public GraphSyncNotifier(
            INodeContentItemLookup nodeContentItemLookup,
            IContentDefinitionManager contentDefinitionManager,
            LinkGenerator linkGenerator,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GraphSyncNotifier> logger)
        {
            _nodeContentItemLookup = nodeContentItemLookup;
            _contentDefinitionManager = contentDefinitionManager;
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _entries = new List<NotifyEntry>();
        }

        public async Task AddBlocked(
            SyncOperation syncOperation,
            ContentItem contentItem,
            IEnumerable<(string GraphReplicaSetName, IAllowSyncResult AllowSyncResult)> graphBlockers)
        {
            string contentType = GetContentTypeDisplayName(contentItem);

            _logger.LogWarning("{Operation} the '{ContentItem}' {ContentType} has been cancelled.",
                syncOperation, contentItem.DisplayText, contentType);

            StringBuilder technicalMessage = new StringBuilder();
            StringBuilder technicalHtmlMessage = new StringBuilder();

            technicalMessage.AppendLine($"{syncOperation} has been blocked by");
            technicalHtmlMessage.AppendLine($"<h5 class=\"card-title\">{syncOperation} has been blocked by</h5>");

            foreach (var graphBlocker in graphBlockers)
            {
                _logger.LogWarning($"{graphBlocker.GraphReplicaSetName} graph blockers: {graphBlocker.AllowSyncResult}.");
                await AddSyncBlockers(technicalMessage, technicalHtmlMessage, graphBlocker.GraphReplicaSetName, graphBlocker.AllowSyncResult);
            }

            //todo: need details of the content item with incoming relationships
            Add($"{syncOperation} the '{contentItem.DisplayText}' {contentType} has been cancelled, due to an issue with graph syncing.",
                technicalMessage.ToString(),
                technicalHtmlMessage: new HtmlString(technicalHtmlMessage.ToString()));
        }

        private async Task AddSyncBlockers(StringBuilder technicalMessage, StringBuilder technicalHtmlMessage, string graphReplicaSetName, IAllowSyncResult allowSyncResult)
        {
            technicalHtmlMessage.AppendLine($"<div class=\"card mt-3\"><div class=\"card-header\">{graphReplicaSetName} graph</div><div class=\"card-body\">");

            technicalHtmlMessage.AppendLine("<ul class=\"list-group list-group-flush\">");
            foreach (var syncBlocker in allowSyncResult.SyncBlockers)
            {
                string? contentItemId = await _nodeContentItemLookup.GetContentItemId((string)syncBlocker.Id, graphReplicaSetName);
                // return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = taxonomyContentItemId });

                string title;
                if (contentItemId != null)
                {
                    string editContentItemUrl = _linkGenerator.GetUriByAction(
                        _httpContextAccessor.HttpContext,
                        "Edit", "Admin",
                        new {area = "OrchardCore.Contents", contentItemId});

                    title = $"<a href=\"{editContentItemUrl}\">'{syncBlocker.Title}'</a>";
                }
                else
                {
                    title = syncBlocker.Title != null ? $"'{syncBlocker.Title}'" : "[Missing Title]";
                }

                technicalHtmlMessage.AppendLine($"<li class=\"list-group-item\">'{title}' {syncBlocker.ContentType}</li>");
            }

            technicalHtmlMessage.AppendLine("</ul></div></div>");

            technicalMessage.AppendLine($"{graphReplicaSetName} graph: {allowSyncResult}");
        }

        private string GetContentTypeDisplayName(ContentItem contentItem)
        {
            return _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType).DisplayName;
        }

        //todo: add custom styles via scss?
        public void Add(
            string userMessage,
            string technicalMessage = "",
            Exception? exception = null,
            HtmlString? technicalHtmlMessage = null,
            HtmlString? userHtmlMessage = null,
            NotifyType type = NotifyType.Error)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(exception, "Notification '{NotificationType}' with user message '{NotificationUserMessage}' and technical message '{NotificationTechnicalMessage}' and exception '{Exception}'}.",
                    type, userMessage, technicalMessage, exception?.ToString() ?? "None");
            }

            HtmlContentBuilder htmlContentBuilder = new HtmlContentBuilder();

            string uniqueId = $"id{Guid.NewGuid():N}";
            string onClickFunction = $"click_{uniqueId}";
            string clipboardCopy = TechnicalClipboardCopy(
                Activity.Current.TraceId.ToString(),
                userMessage,
                technicalMessage,
                exception);

            //fa-angle-double-down, hat-wizard, oil-can, fa-wrench?
            htmlContentBuilder
                .AppendHtml(TechnicalScript(onClickFunction, clipboardCopy))
                .AppendHtml(userHtmlMessage ?? new HtmlString(userMessage))
//                .AppendHtml($"<button class=\"close\" style=\"right: 1.25em;\" type=\"button\" data-toggle=\"collapse\" data-target=\"#{uniqueId}\" aria-expanded=\"false\" aria-controls=\"{uniqueId}\"><i class=\"fas fa-wrench\"></i></button>")
                .AppendHtml($"<a class=\"close\" style=\"right: 1.25em;\" data-toggle=\"collapse\" href=\"#{uniqueId}\" role=\"button\" aria-expanded=\"false\" aria-controls=\"{uniqueId}\"><i class=\"fas fa-wrench\"></i></a>")
                .AppendHtml($"<div class=\"collapse\" id=\"{uniqueId}\">")
                .AppendHtml($"<div class=\"card mt-2\"><div class=\"card-header\">Technical Details <button onclick=\"{onClickFunction}()\" style=\"float: right;\" type=\"button\"><i class=\"fas fa-copy\"></i></button></div><div class=\"card-body\">")
                .AppendHtml($"<h5 class=\"card-title\">Trace ID</h5><h6 class=\"card-subtitle text-muted\">{Activity.Current.TraceId}</h6>")
                .AppendHtml("<div class=\"mt-3\">")
                .AppendHtml(technicalHtmlMessage ?? new HtmlString(technicalMessage))
                .AppendHtml("</div>");

            if (exception != null)
            {
                htmlContentBuilder.AppendHtml($"<div class=\"card mt-3\"><div class=\"card-header\">Exception</div><div class=\"card-body\"><pre><code>{exception}</code></pre></div></div>");
            }

            htmlContentBuilder.AppendHtml("</div></div></div>");

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
            // we replace any ` (backticks) in the  clipboard text to stop them breaking the js
            return new HtmlString(
@$"<script type=""text/javascript"">
function {onClickFunction}() {{
    navigator.clipboard.writeText(`{clipboardText.Replace('`', '\'')}`).then(function() {{
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
