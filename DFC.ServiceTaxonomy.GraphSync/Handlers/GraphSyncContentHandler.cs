using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    //todo: add eventgrid content handler too
    public class GraphSyncContentHandler : ContentHandlerBase
    {
        private readonly IMergeGraphSyncer _mergeGraphSyncer;
        private readonly IServiceProvider _serviceProvider;
        private readonly INotifier _notifier;
        private readonly ILogger<GraphSyncContentHandler> _logger;

        public GraphSyncContentHandler(
            IMergeGraphSyncer mergeGraphSyncer,
            IServiceProvider serviceProvider,
            INotifier notifier,
            ILogger<GraphSyncContentHandler> logger)
        {
            _mergeGraphSyncer = mergeGraphSyncer;
            _serviceProvider = serviceProvider;
            _notifier = notifier;
            _logger = logger;
        }

        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            await SyncToGraphReplicaSet(GraphReplicaSetNames.Draft, context.ContentItem);
        }

        //todo: context contains cancel! (might have to use publishing though)
        public override async Task PublishedAsync(PublishContentContext context)
        {
            await SyncToGraphReplicaSet(GraphReplicaSetNames.Published, context.ContentItem);
        }

        private async Task SyncToGraphReplicaSet(string replicaSetName, ContentItem contentItem)
        {
            try
            {
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                await _mergeGraphSyncer.SyncToGraphReplicaSet(replicaSetName, contentItem, contentManager);
            }
            catch (Exception exception)
            {
                string message = $"Unable to sync {contentItem.DisplayText} {contentItem.ContentType} to {replicaSetName} graphs.";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));
            }
        }

    }
}
