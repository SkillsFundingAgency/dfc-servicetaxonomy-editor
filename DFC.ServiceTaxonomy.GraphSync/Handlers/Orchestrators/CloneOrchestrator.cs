using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Orchestrators
{
    public class CloneOrchestrator : Orchestrator, ICloneOrchestrator
    {
        private readonly ICloneGraphSync _cloneGraphSync;
        private readonly ISyncOrchestrator _syncOrchestrator;
        private readonly IServiceProvider _serviceProvider;

        public CloneOrchestrator(
            ICloneGraphSync cloneGraphSync,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            ISyncOrchestrator syncOrchestrator,
            IServiceProvider serviceProvider,
            ILogger<CloneOrchestrator> logger)
            : base(contentDefinitionManager, notifier, logger)
        {
            _cloneGraphSync = cloneGraphSync;
            _syncOrchestrator = syncOrchestrator;
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> Clone(ContentItem contentItem)
        {
            IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            await _cloneGraphSync.MutateOnClone(contentItem, contentManager);

            return await _syncOrchestrator.SaveDraft(contentItem);
        }
    }
}
