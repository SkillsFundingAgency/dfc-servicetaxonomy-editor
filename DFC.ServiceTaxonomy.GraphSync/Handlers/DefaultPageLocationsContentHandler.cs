using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers
{
    public class DefaultPageLocationsContentHandler : ContentHandlerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;

        public DefaultPageLocationsContentHandler(IServiceProvider serviceProvider, ISession session)
        {
            _serviceProvider = serviceProvider;
            _session = session;
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

                foreach (var page in pages.Where(x => x.ContentItemId != context.ContentItem.ContentItemId && x.Content.Page.PageLocations.TagNames[0] == context.ContentItem.Content.Page.PageLocations.TagNames[0]))
                {
                    var contentItemVersion = new ContentItemVersion(GraphReplicaSetNames.Preview);
                    var latestPreview = await contentItemVersion.GetContentItemAsync(contentManager, page.ContentItemId);

                    if (latestPreview != null)
                    {
                        latestPreview.Content.Page.DefaultPageAtLocation.Value = false;
                        //latestDraft.Published = false;
                        await contentManager.SaveDraftAsync(latestPreview);
                    }
                }
            }
        }
    }
}
