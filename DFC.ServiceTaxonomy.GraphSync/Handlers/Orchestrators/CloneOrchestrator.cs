using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators
{
    public class CloneOrchestrator : Orchestrator, ICloneOrchestrator
    {
        private readonly ICloneGraphSync _cloneGraphSync;

        protected CloneOrchestrator(
            ICloneGraphSync cloneGraphSync,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            ILogger<CloneOrchestrator> logger)
            : base(contentDefinitionManager, notifier, logger)
        {
            _cloneGraphSync = cloneGraphSync;
        }

        public async Task<bool> Clone(ContentItem contentItem)
        {
            await _cloneGraphSync.MutateOnClone(contentItem);
            return true;
        }
    }
}
