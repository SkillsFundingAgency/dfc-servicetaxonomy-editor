using System;
using System.Collections.Generic;
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

        public string ErrorMessage => "The generated URL for this Page Location has already been used as a Page URL.";

        public async Task<bool> Validate(ContentItem term, ContentItem taxonomy)
        {
            if (term.Content.PageLocation == null)
            {
                return true;
            }

            string url = _taxonomyHelper.BuildTermUrl(JObject.FromObject(term), JObject.FromObject(taxonomy));
            //TODO: check whether or not we only care about published pages, but I think we care about both
            IEnumerable<ContentItem> pages = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Page" && x.Latest).ListAsync();
            
            foreach (var page in pages)
            {
                //TODO: use nameof, but doing so would introduce a circular dependency between the page location and taxonomies projects
                string? draftUrl = ((string?)(await _contentManager.GetAsync(page.ContentItemId, VersionOptions.Draft))?.Content.PageLocationPart.FullUrl)?.Trim('/') ?? null;
                string? pubUrl = ((string?)(await _contentManager.GetAsync(page.ContentItemId, VersionOptions.Published))?.Content.PageLocationPart.FullUrl)?.Trim('/') ?? null;

                if ((draftUrl?.Equals(url, StringComparison.OrdinalIgnoreCase) ?? false) || (pubUrl?.Equals(url, StringComparison.OrdinalIgnoreCase) ?? false)) return false;
            }

            return true;
        }
    }
}
