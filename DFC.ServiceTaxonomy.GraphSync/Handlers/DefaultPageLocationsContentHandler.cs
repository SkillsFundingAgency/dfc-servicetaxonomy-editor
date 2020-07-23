using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.Notify;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
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
            if (context.ContentItem.ContentType == "Page" && Convert.ToBoolean(context.ContentItem.Content.Page.DefaultPageAtLocation.Value))
            {
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                //TODO : find out how to query for only pages marked as default - probably need to make a new index
                var pages = await _session.Query<ContentItem, ContentItemIndex>(x => x.Latest && x.ContentType == "Page").ListAsync();

                foreach (var page in pages.Where(x => x.ContentItemId != context.ContentItem.ContentItemId && x.Content.Page.PageLocations.TagNames[0] == context.ContentItem.Content.Page.PageLocations.TagNames[0]))
                {
                    var latestPublished = await contentManager.GetAsync(page.ContentItemId, VersionOptions.Published);

                    if (latestPublished != null)
                    {
                        latestPublished.Content.Page.DefaultPageAtLocation.Value = false;
                        latestPublished.Published = false;
                        await contentManager.PublishAsync(latestPublished);
                    }
                }
            }
        }

        public override async Task DraftSavedAsync(SaveDraftContentContext context)
        {
            if (context.ContentItem.ContentType == "Page" && Convert.ToBoolean(context.ContentItem.Content.Page.DefaultPageAtLocation.Value))
            {
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();

                //TODO : find out how to query for only pages marked as default - probably need to make a new index
                var pages = await _session.Query<ContentItem, ContentItemIndex>(x => x.Latest && x.ContentType == "Page").ListAsync();

                foreach (var page in pages.Where(x => x.ContentItemId != context.ContentItem.ContentItemId && x.Content.Page.PageLocations.TermContentItemIds[0] == context.ContentItem.Content.Page.PageLocations.TermContentItemIds[0]))
                {
                    var latestPreview = await _previewContentItemVersion.GetContentItem(contentManager, page.ContentItemId);

                    if (latestPreview != null)
                    {
                        latestPreview.Content.Page.DefaultPageAtLocation.Value = false;

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
            try
            {
                IMergeGraphSyncer mergeGraphSyncer = _serviceProvider.GetRequiredService<IMergeGraphSyncer>();
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                await mergeGraphSyncer.SyncToGraphReplicaSet(_graphCluster.GetGraphReplicaSet(GraphReplicaSetNames.Preview), contentItem, contentManager);
            }
            catch (Exception exception)
            {
                string message = $"Unable to sync {contentItem.DisplayText} Page to {GraphReplicaSetNames.Preview} graph(s).";
                _logger.LogError(exception, message);
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(GraphSyncContentHandler), message));
            }
        }
    }
}
