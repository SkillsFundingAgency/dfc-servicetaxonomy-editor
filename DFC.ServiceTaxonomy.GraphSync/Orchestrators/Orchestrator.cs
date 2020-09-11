using DFC.ServiceTaxonomy.GraphSync.Notifications;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.Orchestrators
{
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

        //todo: todo temporarily protected
        protected string GetContentTypeDisplayName(ContentItem contentItem)
        {
            return _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType).DisplayName;
        }
    }
}
