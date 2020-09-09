using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.GraphSync.Handlers.Interfaces;
using DFC.ServiceTaxonomy.PageLocation.Models;
using DFC.ServiceTaxonomy.Taxonomies.Handlers;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.PageLocation.Handlers
{
    public class PageLocationTaxonomyTermHandler : ITaxonomyTermHandler
    {
        private readonly ISession _session;
        private readonly ITaxonomyHelper _taxonomyHelper;
        private readonly IContentManager _contentManager;
        private readonly ISyncOrchestrator _syncOrchestrator;
        private readonly IContentItemsService _contentItemsService;

        public PageLocationTaxonomyTermHandler(ISession session, ITaxonomyHelper taxonomyHelper, IContentManager contentManager, ISyncOrchestrator syncOrchestrator, IContentItemsService contentItemsService)
        {
            _session = session;
            _taxonomyHelper = taxonomyHelper;
            _contentManager = contentManager;
            _syncOrchestrator = syncOrchestrator;
            _contentItemsService = contentItemsService;
        }

        public async Task UpdatedAsync(ContentItem term, ContentItem taxonomy)
        {
            if (term.ContentType != ContentTypes.PageLocation)
                return;

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

                if (pages.Count > 1)
                {
                    var publishedPage = pages.Single(x => x.Published);
                    var draftPage = pages.Single(x => x.Latest);

                    //TODO (CHECK) : will this also fire events to the event store?
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
        }
    }
}
