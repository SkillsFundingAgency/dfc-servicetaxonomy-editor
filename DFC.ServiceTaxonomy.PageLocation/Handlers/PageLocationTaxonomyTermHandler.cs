using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public PageLocationTaxonomyTermHandler(ISession session, ITaxonomyHelper taxonomyHelper, IContentManager contentManager)
        {
            _session = session;
            _taxonomyHelper = taxonomyHelper;
            _contentManager = contentManager;
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

                page.Alter<PageLocationPart>(part => part.FullUrl = fullUrl);

                if (page.Published)
                {
                    page.Published = false;
                    await _contentManager.PublishAsync(page);
                    return;
                }

                await _contentManager.SaveDraftAsync(page);
            }
        }
    }
}
