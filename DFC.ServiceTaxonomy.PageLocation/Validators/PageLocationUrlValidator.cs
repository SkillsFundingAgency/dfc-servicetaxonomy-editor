using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.PageLocation.Validators
{
    public class PageLocationUrlValidator : ITaxonomyTermValidator
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyHelper _taxonomyHelper;

        public PageLocationUrlValidator(ISession session, IContentManager contentManager, ITaxonomyHelper taxonomyHelper)
        {
            _session = session;
            _contentManager = contentManager;
            _taxonomyHelper = taxonomyHelper;
        }

        public string? ErrorMessage { get; private set; }

        public async Task<bool> Validate(JObject term, JObject taxonomy)
        {
            if (!term.ContainsKey("PageLocation"))
            {
                return true;
            }

            string url = _taxonomyHelper.BuildTermUrl(term, taxonomy);
            //TODO: check whether or not we only care about published pages, but I think we care about both
            IEnumerable<ContentItem> pages = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Page" && x.Latest).ListAsync();
            
            foreach (var page in pages)
            {
                ContentItem? draftPage = await _contentManager.GetAsync(page.ContentItemId, VersionOptions.Draft);
                ContentItem? publishedPage = await _contentManager.GetAsync(page.ContentItemId, VersionOptions.Published);

                //TODO: use nameof, but doing so would introduce a circular dependency between the page location and taxonomies projects
                string? draftUrl = ((string?)draftPage?.Content.PageLocationPart.FullUrl)?.Trim('/') ?? null;
                string? pubUrl = ((string?)publishedPage?.Content.PageLocationPart.FullUrl)?.Trim('/') ?? null;

                string[]? draftRedirectLocations = draftPage?.Content.PageLocationPart.RedirectLocations?.ToObject<string?>()?.Split("\r\n");
                string[]? publishedRedirectLocations = publishedPage?.Content.PageLocationPart.RedirectLocations?.ToObject<string?>()?.Split("\r\n");

                if ((draftUrl?.Equals(url, StringComparison.OrdinalIgnoreCase) ?? false) || (pubUrl?.Equals(url, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    ErrorMessage = $"The generated URL for '{term["DisplayText"]}' has already been used as a Page URL.";
                    return false;
                }

                if ((draftRedirectLocations?.Any(x => x.Trim('/').Equals(url, StringComparison.OrdinalIgnoreCase)) ?? false) || (publishedRedirectLocations?.Any(x => x.Trim('/').Equals(url, StringComparison.OrdinalIgnoreCase)) ?? false))
                {
                    ErrorMessage = $"The generated URL for '{term["DisplayText"]}' has already been used as a Page Redirect Location";
                    return false;
                }
            }

            return true;
        }
    }
}
