using System.Collections.Generic;
using System.Text;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators
{
    // public enum OperationDirection
    // {
    //     From,
    //     To
    // }

    public class Orchestrator
    {
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly ICustomNotifier _notifier;
        protected readonly ILogger _logger;

        protected Orchestrator(
            IContentDefinitionManager contentDefinitionManager,
            ICustomNotifier notifier,
            ILogger logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _logger = logger;
        }

        protected void AddBlockedNotifier(
            string operationDescription,
            //OperationDirection operationDirection,
            ContentItem contentItem,
            IEnumerable<(string GraphReplicaSetName, IAllowSyncResult AllowSyncResult)> graphBlockers)
        {
            string contentType = GetContentTypeDisplayName(contentItem);

            //{OperationDirection} the {GraphReplicaSetName} graphs
            //operationDirection, graphReplicaSetName,
            _logger.LogWarning("{OperationDescription} the '{ContentItem}' {ContentType} has been cancelled.",
                operationDescription, contentItem.DisplayText, contentType);

            //todo: technical message on clipboard contains html

            StringBuilder technicalMessage = new StringBuilder();
            technicalMessage.AppendLine("The operation has been blocked by the following items.");
            foreach (var graphBlocker in graphBlockers)
            {
                _logger.LogWarning($"{graphBlocker.GraphReplicaSetName} graph blockers: {graphBlocker.AllowSyncResult}.");
                AddSyncBlockersLine(technicalMessage, graphBlocker.GraphReplicaSetName, graphBlocker.AllowSyncResult);
            }

            //todo: need details of the content item with incoming relationships
            _notifier.Add(new HtmlString(
                $"{operationDescription} the '{contentItem.DisplayText}' {contentType} has been cancelled."),
                new HtmlString(technicalMessage.ToString()));
        }

        private void AddSyncBlockersLine(StringBuilder technicalMessage, string graphReplicaSetName, IAllowSyncResult allowSyncResult)
        {
            technicalMessage.AppendLine($"<div class=\"card\"><div class=\"card-header\">{graphReplicaSetName}</div><div class=\"card-body\">{allowSyncResult}</div></div>");
        }

        //todo: todo temporarily protected
        protected string GetContentTypeDisplayName(ContentItem contentItem)
        {
            return _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType).DisplayName;
        }
    }
}
