using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Taxonomies.Helper;
using DFC.ServiceTaxonomy.Taxonomies.Validation;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.PageLocation.Validators
{
    public class PageLocationUrlValidator : ITaxonomyTermValidator
    {
        private readonly ISession _session;

        public PageLocationUrlValidator(ISession session)
        {
            _session = session;
        }

        public string ErrorMessage => "The generated URL for this Page Location has already been used as a Page URL.";

        public async Task<bool> Validate(ContentItem term, ContentItem taxonomy)
        {
            if (term.Content.PageLocation == null)
            {
                return true;
            }

            string url = TaxonomyHelpers.BuildTermUrl(term, taxonomy);
            //TODO: check whether or not we only care about published pages, but I think we care about both
            IEnumerable<ContentItem> pages = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Page" && x.Latest).ListAsync();
            //TODO: use nameof, but doing so would introduce a circular dependency between the page location and taxonomies projects
            return pages.All(x => ((string)x.Content.PageLocationPart.FullUrl).Trim('/') != url);
        }
    }
}
