using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content;
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

        //todo: do we need this? will events fire anyway?
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

                    _session.Save(page);
                }

                try
                {
                    if (pages.Count > 1)
                    {
                        var publishedPage = pages.Single(x => x.Published);
                        var draftPage = pages.Single(x => x.Latest);

                        if (!await _syncOrchestrator.Update(publishedPage, draftPage))
                        {
                            _session.Cancel();
                        }
                    }
                    else
                    {
                        var page = pages.Single();

                        if (page.Published && !await _syncOrchestrator.Publish(page))
                        {
                            _session.Cancel();
                        }
                        else if (!page.Published && !await _syncOrchestrator.SaveDraft(page))
                        {
                            _session.Cancel();
                        }
                    }
                }
                catch (Exception)
                {
                    _session.Cancel();
                    return false;
                }
            }

            return true;
        }
    }
}
