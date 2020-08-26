#nullable enable

using System;
using System.Linq;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.Taxonomies.Helper
{
    public class TaxonomyHelper : ITaxonomyHelper
    {
        public JArray? GetTerms(JObject contentItem)
        {
            //make sure we always check for terms at the root of the object first, or the taxonomy reshuffle validation won't work.
            if (contentItem.ContainsKey(nameof(TaxonomyPart.Terms)))
                return contentItem[nameof(TaxonomyPart.Terms)] as JArray;

            return contentItem[nameof(TaxonomyPart)]?[nameof(TaxonomyPart.Terms)] as JArray;
        }

        public JObject? FindParentTaxonomyTerm(JObject termContentItem, JObject taxonomyContentItem)
        {
            JArray? terms = GetTerms(taxonomyContentItem);

            if (terms == null)
                return null;

            if (terms.Any(x => (string?)x["ContentItemId"] == (string?)termContentItem["ContentItemId"]))
                return taxonomyContentItem;

            JObject? result = null;

            foreach (JObject term in terms)
            {
                result = FindParentTaxonomyTerm(termContentItem, term);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public string BuildTermUrl(JObject term, JObject taxonomy)
        {
            string? url = term["DisplayText"]?.Value<string?>();

            if (url == null)
                throw new InvalidOperationException($"No DisplayText property found on: {term}");

            JObject? parent = FindParentTaxonomyTerm(term, taxonomy);

            while (parent != null && (string?)parent!["ContentType"] != "Taxonomy")
            {
                url = $"{parent!["DisplayText"]}/{url}";
                parent = FindParentTaxonomyTerm(parent, taxonomy);
            }

            return url.Trim('/');
        }

        public JArray GetAllTerms(JObject taxonomy)
        {
            var results = new JArray();
            return GetAllTermsInternal(taxonomy, results);
        }

        private JArray GetAllTermsInternal(JObject taxonomy, JArray results)
        {
            var terms = GetTerms(taxonomy);

            if (terms != null)
            {
                results.Merge(terms);

                foreach (dynamic term in terms)
                {
                    GetAllTermsInternal(term, results);
                }
            }

            return results;
        }
    }
}
