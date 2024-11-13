using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Contexts;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Notifications;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators.Interfaces;
using DFC.ServiceTaxonomy.PageLocation.Constants;
using DFC.ServiceTaxonomy.PageLocation.Models;
using DFC.ServiceTaxonomy.Taxonomies.Handlers;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using YesSql;

namespace DFC.ServiceTaxonomy.PageLocation.Handlers
{
    public class PageLocationTaxonomyTermHandler : ITaxonomyTermHandler
    {
        private readonly ISession _session;
        private readonly ITaxonomyHelper _taxonomyHelper;
        private readonly ISyncOrchestrator _syncOrchestrator;
        private readonly IContentItemsService _contentItemsService;
        private readonly IGraphSyncNotifier _notifier;
        private readonly IEnumerable<IContentOrchestrationHandler> _contentOrchestrationHandlers;

        public PageLocationTaxonomyTermHandler(
            ISession session,
            ITaxonomyHelper taxonomyHelper,
            ISyncOrchestrator syncOrchestrator,
            IContentItemsService contentItemsService,
            IGraphSyncNotifier notifier,
            IEnumerable<IContentOrchestrationHandler> contentOrchestrationHandlers)
        {
            _session = session;
            _taxonomyHelper = taxonomyHelper;
            _syncOrchestrator = syncOrchestrator;
            _contentItemsService = contentItemsService;
            _notifier = notifier;
            _contentOrchestrationHandlers = contentOrchestrationHandlers;
        }

        public async Task PublishedAsync(ContentItem term)
        {
            if (term.ContentType != ContentTypes.PageLocation)
                return;

            var context = new OrchestrationContext(term, _notifier);

            foreach (var orchestrator in _contentOrchestrationHandlers)
            {
                await orchestrator.Published(context);
            }
        }

        public async Task<bool> UpdatedAsync(ContentItem term, ContentItem taxonomy)
        {
            if (term.ContentType != ContentTypes.PageLocation)
                return true;

            List<ContentItem> allPages = await _contentItemsService.GetActive(ContentTypes.Page);
            List<ContentItem> associatedPages = allPages.Where(x => x.Content.Page.PageLocations.TermContentItemIds[0] == term.ContentItemId).ToList();

            var groups = associatedPages.GroupBy(x => x.ContentItemId);

            foreach (var group in groups)
            {
                var pages = group.ToList();

                foreach (var page in pages)
                {
                    //rebuild the Full URL to ensure it matches the current state of the term
                    var pageUrlName = page.As<PageLocationPart>().UrlName;
                    var termUrl = _taxonomyHelper.BuildTermUrl(JObject.FromObject(term), JObject.FromObject(taxonomy));

                    var fullUrl = string.IsNullOrWhiteSpace(termUrl)
                        ? $"/{pageUrlName}"
                        : $"/{termUrl}/{pageUrlName}";

                    page.Alter<PageLocationPart>(part => part.FullUrl = fullUrl);

                    await _session.SaveAsync(page);
                }

                try
                {
                    if (pages.Count > 1)
                    {
                        pages = pages.OrderByDescending(p => p.ModifiedUtc).ToList();
                    }

                    if (pages.Count > 1 && pages.Any(p => p.Published))
                    {
                        // Currently there is an issue in STAX with there occasionally being more that one version of a content item being flagged as 'Latest'.
                        // The route cause of this is currently unknown however it results in multiple versions of the same content item being shown in the editor
                        // and causes the following code to fail silently when amending Page locations as is is only expecting one published and one latest version.
                        // As such defensive code to fix this is required until the route cause is identified and fixed.
                        if (pages.All(p => p.Latest))
                        {
                            // If all pages are flagged as published then this needs to be corrected so that only the latest one is.
                            var removePublishFlag = pages.All(p => p.Published);
                            // If none of the pages are published flag the oldest is flagged as published.
                            if(pages.All(p=> !p.Published))
                            {
                                pages.Last().Published = true;
                            }
                            // We need to flag all pages other than the most recently modified as Latest = false
                            var incorrectPages = pages.OrderByDescending(p => p.ModifiedUtc).Skip(1).ToList();
                            foreach (var page in incorrectPages)
                            {
                                page.Latest = false;
                                if(removePublishFlag)
                                {
                                    page.Published = false;
                                }
                                await _session.SaveAsync(page);
                            }
                        }

                        var publishedPage = pages.Single(x => x.Published);
                        var draftPage = pages.Single(x => x.Latest);

                        if (!await _syncOrchestrator.Update(publishedPage, draftPage))
                        {
                            await _session.CancelAsync();
                        }
                    }
                    else
                    {
                        // We have multiple pages flagged as 'Latest' and none are flagged as 'Published' so we need to assert that only the most
                        // recently modified page is 'Latest' and remove the others.
                        if (pages.Count > 1)
                        {
                            var incorrectPages = pages.OrderByDescending(p => p.ModifiedUtc).Skip(1).ToList();
                            foreach (var incorrectPage in incorrectPages)
                            {
                                incorrectPage.Latest = false;
                                await _session.SaveAsync(incorrectPage);
                            }
                            pages = pages.Take(1).ToList();
                        }

                        var page = pages.Single();

                        if (page.Published && !await _syncOrchestrator.Publish(page))
                        {
                            await _session.CancelAsync();
                        }
                        else if (!page.Published && !await _syncOrchestrator.SaveDraft(page))
                        {
                            await _session.CancelAsync();
                        }
                    }
                }
                catch (Exception)
                {
                    await _session.CancelAsync();
                    return false;
                }
            }

            return true;
        }
    }
}
