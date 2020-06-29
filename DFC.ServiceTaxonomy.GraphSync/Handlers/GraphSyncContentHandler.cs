using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    //todo: add eventgrid content handler too
    public class GraphSyncContentHandler : ContentHandlerBase
    {
        private readonly IMergeGraphSyncer _mergeGraphSyncer;
        private readonly INotifier _notifier;
        private readonly ILogger<GraphSyncContentHandler> _logger;

        public GraphSyncContentHandler(
            IMergeGraphSyncer mergeGraphSyncer,
            INotifier notifier,
            ILogger<GraphSyncContentHandler> logger)
        {
            _mergeGraphSyncer = mergeGraphSyncer;
            _notifier = notifier;
            _logger = logger;
        }

        public override Task DraftSavedAsync(SaveDraftContentContext context)
        {
            return Task.CompletedTask;
        }

        //todo: context contains cancel! (might have to use publishing though)
        //todo: how to add notifications?
        public override async Task PublishedAsync(PublishContentContext context)
        {
            try
            {
                await _mergeGraphSyncer.SyncToGraph(context.ContentItem);
            }
            catch (Exception exception)
            {
                string message = $"Unable to sync {context.ContentItem.ContentType} to graph.";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));
            }
        }
    }
}
