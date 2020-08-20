#nullable enable

using System.Collections.Generic;
using System.Linq;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using Microsoft.CSharp.RuntimeBinder;

namespace DFC.ServiceTaxonomy.Taxonomies.Helper
{
    public class TaxonomyHelper : ITaxonomyHelper
    {
        public List<dynamic>? GetTerms(dynamic contentItem)
        {
            try
            {
                return contentItem.Content?[nameof(TaxonomyPart)]?[nameof(TaxonomyPart.Terms)]?.ToObject<List<dynamic>?>() ??
                       contentItem.Content?[nameof(TaxonomyPart.Terms)]?.ToObject<List<dynamic>?>() ??
                       contentItem[nameof(TaxonomyPart.Terms)]?.ToObject<List<dynamic>?>();
            }
            catch (RuntimeBinderException)
            {
                return null;
            }
        }

        public dynamic? FindParentTaxonomyTerm(dynamic termContentItem, dynamic taxonomyContentItem)
        {
            List<dynamic>? terms = GetTerms(taxonomyContentItem);

            if (terms == null)
                return null;

            if (terms.Any(x => x.ContentItemId == termContentItem.ContentItemId))
                return taxonomyContentItem;

            dynamic? result = null;

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

        public string BuildTermUrl(dynamic term, dynamic taxonomy)
        {
            string url = term.DisplayText;

            dynamic? parent = FindParentTaxonomyTerm(term, taxonomy);

            while (parent != null && parent!.ContentType != "Taxonomy")
            {
                url = $"{parent!.DisplayText}/{url}";
                parent = FindParentTaxonomyTerm(parent, taxonomy);
            }

            return url.Trim('/');
        }
    }
}
