using System;
using System.Threading.Tasks;
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
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                await _mergeGraphSyncer.SyncToGraph(context.ContentItem, contentManager);
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
