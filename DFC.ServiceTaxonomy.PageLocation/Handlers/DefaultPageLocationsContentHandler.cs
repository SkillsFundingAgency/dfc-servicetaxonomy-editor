using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Results.AllowSync;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
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
        private readonly IDataSyncCluster _dataSyncCluster;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly INotifier _notifier;
        private readonly ILogger<DefaultPageLocationsContentHandler> _logger;

        public DefaultPageLocationsContentHandler(IServiceProvider serviceProvider, ISession session, IDataSyncCluster dataSyncCluster, IPreviewContentItemVersion previewContentItemVersion, INotifier notifier, ILogger<DefaultPageLocationsContentHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _session = session;
            _dataSyncCluster = dataSyncCluster;
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
            // sonar can't see that the set value could be used in the event of an exception
            #pragma warning disable S1854
            AllowSyncResult allowSyncResult = AllowSyncResult.Blocked;
            string message = $"Unable to sync '{contentItem.DisplayText}' Page to {DataSyncReplicaSetNames.Preview} graph(s).";

            try
            {
                IMergeDataSyncer mergeDataSyncer = _serviceProvider.GetRequiredService<IMergeDataSyncer>();
                IContentManager contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                IAllowSync allowSync = await mergeDataSyncer.SyncToDataSyncReplicaSetIfAllowed(
                    _dataSyncCluster.GetDataSyncReplicaSet(DataSyncReplicaSetNames.Preview), contentItem, contentManager);
                allowSyncResult = allowSync.Result;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to sync '{ContentItemDisplayText}' Page to {DataSyncReplicaSetName} graph(s).",
                    contentItem.DisplayText, DataSyncReplicaSetNames.Preview);
            }

            if (allowSyncResult == AllowSyncResult.Blocked)
            {
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DefaultPageLocationsContentHandler), message));
            }
            #pragma warning restore S1854
        }
    }
}
