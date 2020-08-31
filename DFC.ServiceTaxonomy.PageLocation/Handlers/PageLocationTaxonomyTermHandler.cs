using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public PageLocationTaxonomyTermHandler(ISession session, ITaxonomyHelper taxonomyHelper, IContentManager contentManager, ISyncOrchestrator syncOrchestrator)
        {
            _session = session;
            _taxonomyHelper = taxonomyHelper;
            _contentManager = contentManager;
            _syncOrchestrator = syncOrchestrator;
        }

        public async Task UpdatedAsync(ContentItem term, ContentItem taxonomy)
        {
            if (term.Content.PageLocation == null)
                return;

            IEnumerable<ContentItem> allPages = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Page" && x.Latest).ListAsync();
            IEnumerable<ContentItem> associatedPages = allPages.Where(x => x.Content.Page.PageLocations.TermContentItemIds[0] == term.ContentItemId);

            foreach (var page in associatedPages)
            {
                //rebuild the Full URL to ensure it matches the current state of the term
                var pageUrlName = page.As<PageLocationPart>().UrlName;
                var termUrl = _taxonomyHelper.BuildTermUrl(JObject.FromObject(term), JObject.FromObject(taxonomy));

                var fullUrl = string.IsNullOrWhiteSpace(termUrl)
                    ? $"/{pageUrlName}"
                    : $"/{termUrl}/{pageUrlName}";

                //load and alter the draft page first
                ContentItem draftPage = await _contentManager.GetAsync(page.ContentItemId, VersionOptions.Draft);
                if (draftPage != null)
                {
                    draftPage.Alter<PageLocationPart>(part => part.FullUrl = fullUrl);
                }

                //load and alter the published page
                ContentItem publishedPage = await _contentManager.GetAsync(page.ContentItemId, VersionOptions.Published);

                if (publishedPage != null)
                {
                    publishedPage.Alter<PageLocationPart>(part => part.FullUrl = fullUrl);
                }

                if (publishedPage != null)
                {
                    _session.Save(publishedPage);
                }

                if (draftPage != null)
                {
                    _session.Save(draftPage);
                }

                if (publishedPage != null && draftPage != null)
                {
                    //TODO (CHECK) : will this also fire events to the event store?
                    if (!await _syncOrchestrator.Update(publishedPage, draftPage))
                    {
                        _session.Cancel();
                    }
                }
                else if (publishedPage != null)
                {
                    if (!await _syncOrchestrator.Publish(publishedPage))
                    {
                        _session.Cancel();
                    }
                }
                else if (draftPage != null)
                {
                    if (!await _syncOrchestrator.SaveDraft(draftPage))
                    {
                        _session.Cancel();
                    }
                }
            }
        }
    }
}
