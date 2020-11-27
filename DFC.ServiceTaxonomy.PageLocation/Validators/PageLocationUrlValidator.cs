using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Indexes;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Utilities;
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

        public async Task<(bool, string)> ValidateCreate(JObject term, JObject taxonomy)
        {
            if (!term.ContainsKey("PageLocation"))
            {
                return (true, string.Empty);
            }

            string url = _taxonomyHelper.BuildTermUrl(term, taxonomy);
            //TODO: check whether or not we only care about published pages, but I think we care about both
            IEnumerable<ContentItem> contentItems = await _session.Query<ContentItem, PageLocationPartIndex>().ListAsync();

            foreach (var contentItem in contentItems)
            {
                ContentItem? draftContentItem = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Draft);
                ContentItem? publishedContentItem = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Published);

                //TODO: use nameof, but doing so would introduce a circular dependency between the page location and taxonomies projects
                string? draftUrl = ((string?)draftContentItem?.Content.PageLocationPart.FullUrl)?.Trim('/') ?? null;
                string? pubUrl = ((string?)publishedContentItem?.Content.PageLocationPart.FullUrl)?.Trim('/') ?? null;

                string[]? draftRedirectLocations = draftContentItem?.Content.PageLocationPart.RedirectLocations?.ToObject<string?>()?.Split("\r\n");
                string[]? publishedRedirectLocations = publishedContentItem?.Content.PageLocationPart.RedirectLocations?.ToObject<string?>()?.Split("\r\n");

                if ((draftUrl?.Equals(url, StringComparison.OrdinalIgnoreCase) ?? false) || (pubUrl?.Equals(url, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    return (false, $"The generated URL for '{term["DisplayText"]}' has already been used as a {draftContentItem?.ContentType.CamelFriendly() ?? publishedContentItem!.ContentType.CamelFriendly()} URL.");
                }

                if ((draftRedirectLocations?.Any(x => x.Trim('/').Equals(url, StringComparison.OrdinalIgnoreCase)) ?? false) || (publishedRedirectLocations?.Any(x => x.Trim('/').Equals(url, StringComparison.OrdinalIgnoreCase)) ?? false))
                {
                    return (false, $"The generated URL for '{term["DisplayText"]}' has already been used as a {draftContentItem?.ContentType.CamelFriendly() ?? publishedContentItem!.ContentType.CamelFriendly()} Redirect Location");
                }
            }

            return (true, string.Empty);
        }

        public Task<(bool, string)> ValidateUpdate(JObject term, JObject taxonomy)
        {
            return ValidateCreate(term, taxonomy);
        }

        public Task<(bool, string)> ValidateDelete(JObject term, JObject taxonomy)
        {
            return Task.FromResult((true, string.Empty));
        }
    }
}
