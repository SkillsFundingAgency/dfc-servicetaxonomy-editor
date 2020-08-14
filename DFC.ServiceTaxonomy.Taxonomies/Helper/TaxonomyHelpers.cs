using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Taxonomies.Helper
{
    public static class TaxonomyHelpers
    {
        public static List<ContentItem> GetTerms(ContentItem contentItem)
        {
            return contentItem.As<TaxonomyPart>()?.Terms ??
                   contentItem.Content.Terms?.ToObject<List<ContentItem>>();
        }

        public static ContentItem FindParentTaxonomyTerm(ContentItem termContentItem, ContentItem taxonomyContentItem)
        {
            List<ContentItem> terms = GetTerms(taxonomyContentItem);

            if (terms == null)
                return null;

            if (terms.Any(x => x.ContentItemId == termContentItem.ContentItemId))
                return taxonomyContentItem;

            ContentItem result = null;

            foreach (var term in terms)
            {
                result = FindParentTaxonomyTerm(termContentItem, term);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public static string BuildTermUrl(ContentItem term, ContentItem taxonomy)
        {
            string url = term.DisplayText;

            ContentItem parent = FindParentTaxonomyTerm(term, taxonomy);

            while (parent != null && parent.ContentType != "Taxonomy")
            {
                url = $"{parent.DisplayText}/{url}";
                parent = FindParentTaxonomyTerm(parent, taxonomy);
            }

            return url.Trim('/');
        }
    }
}
