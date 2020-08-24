using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators
{
    public class CloneOrchestrator : Orchestrator, ICloneOrchestrator
    {
        protected CloneOrchestrator(
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            ILogger<CloneOrchestrator> logger)
            : base(contentDefinitionManager, notifier, logger)
        {
        }
    }
}
