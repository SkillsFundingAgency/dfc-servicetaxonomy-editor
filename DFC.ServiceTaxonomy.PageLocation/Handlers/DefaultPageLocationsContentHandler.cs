using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using DFC.ServiceTaxonomy.PageLocation.Indexes;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using YesSql;

namespace DFC.ServiceTaxonomy.PageLocation.Handlers
{
    //todo: IContentPartHandler now has support for DraftSavedAsync, so we should switch to using it
    public class DefaultPageLocationsContentHandler : ContentHandlerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;
        private readonly IGraphCluster _graphCluster;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly INotifier _notifier;
        private readonly ILogger<DefaultPageLocationsContentHandler> _logger;

        public DefaultPageLocationsContentHandler(IServiceProvider serviceProvider, ISession session, IGraphCluster graphCluster, IPreviewContentItemVersion previewContentItemVersion, INotifier notifier, ILogger<DefaultPageLocationsContentHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _session = session;
            _graphCluster = graphCluster;
            _previewContentItemVersion = previewContentItemVersion;
            _notifier = notifier;
            _logger = logger;
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            if (context.ContentItem.ContentType == "Page" && Convert.ToBoolean(context.ContentItem.Content.PageLocationPart.DefaultPageForLocation.Value))
            {
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                //TODO : find out how to query for only pages marked as default - probably need to make a new index
                var pages = await _session.Query<ContentItem, PageLocationPartIndex>(x => x.ContentItemId != context.ContentItem.ContentItemId).ListAsync();

                foreach (var page in pages.Where(x => x.Content.Page.PageLocations.TermContentItemIds[0] == context.ContentItem.Content.Page.PageLocations.TermContentItemIds[0]))
                {
                    var latestPublished = await contentManager.GetAsync(page.ContentItemId, VersionOptions.Published);

                    if (latestPublished != null && Convert.ToBoolean(latestPublished.Content.PageLocationPart.DefaultPageForLocation.Value))
                    {
                        latestPublished!.Content.PageLocationPart.DefaultPageForLocation.Value = false;
                        latestPublished.Published = false;
                        await contentManager.PublishAsync(latestPublished);
                    }
                }
            }
        }

        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            if (context.ContentItem.ContentType == "Page" && Convert.ToBoolean(context.ContentItem.Content.PageLocationPart.DefaultPageForLocation.Value))
            {
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                //TODO : find out how to query for only pages marked as default - probably need to make a new index
                var pages = await _session.Query<ContentItem, PageLocationPartIndex>(x => x.ContentItemId != context.ContentItem.ContentItemId).ListAsync();

                foreach (var page in pages.Where(x => x.Content.Page.PageLocations.TermContentItemIds[0] == context.ContentItem.Content.Page.PageLocations.TermContentItemIds[0]))
                {
                    var latestPreview = await _previewContentItemVersion.GetContentItem(contentManager, page.ContentItemId);

                    if (latestPreview != null && Convert.ToBoolean(latestPreview.Content.PageLocationPart.DefaultPageForLocation.Value))
                    {
                        latestPreview!.Content.PageLocationPart.DefaultPageForLocation.Value = false;

                        await contentManager.SaveDraftAsync(latestPreview);

                        if (latestPreview.Published)
                        {
                            await SyncToPreviewGraph(latestPreview);
                        }
                    }
                }
            }
        }

        private async Task SyncToPreviewGraph(ContentItem contentItem)
        {
            SyncStatus syncStatus = SyncStatus.Blocked;
            string message = $"Unable to sync '{contentItem.DisplayText}' Page to {GraphReplicaSetNames.Preview} graph(s).";

            try
            {
                IMergeGraphSyncer mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                IAllowSyncResult allowSyncResult = await mergeGraphSyncer.SyncToGraphReplicaSetIfAllowed(
                    _graphCluster.GetGraphReplicaSet(GraphReplicaSetNames.Preview), contentItem, contentManager);
                syncStatus = allowSyncResult.AllowSync;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, message);
            }

            if (syncStatus == SyncStatus.Blocked)
            {
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DefaultPageLocationsContentHandler), message));
            }
        }
    }
}
