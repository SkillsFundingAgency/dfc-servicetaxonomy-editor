#nullable enable

using System;
using System.Linq;
using System.Text.Json.Nodes;
using DFC.ServiceTaxonomy.Taxonomies.Models;
using Json.More;

namespace DFC.ServiceTaxonomy.Taxonomies.Helper
{
    public class TaxonomyHelper : ITaxonomyHelper
    {
        public JsonArray? GetTerms(JsonObject contentItem)
        {
            //make sure we always check for terms at the root of the object first, or the taxonomy reshuffle validation won't work.
            if (contentItem.ContainsKey(nameof(TaxonomyPart.Terms)))
                return contentItem[nameof(TaxonomyPart.Terms)] as JsonArray;

            return contentItem[nameof(TaxonomyPart)]?[nameof(TaxonomyPart.Terms)] as JsonArray;
        }

        public JsonObject? FindParentTaxonomyTerm(JsonObject termContentItem, JsonObject taxonomyContentItem)
        {
            JsonArray? terms = GetTerms(taxonomyContentItem);

            if (terms == null)
                return null;

            if (terms.Any(x => (string?)x!["ContentItemId"] == (string?)termContentItem["ContentItemId"]))
                return taxonomyContentItem;

            JsonObject? result = null;
             
            foreach (var term in terms!)
            {
                result = FindParentTaxonomyTerm(termContentItem, term!.AsObject());

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public string BuildTermUrl(JsonObject term, JsonObject taxonomy)
        {
            string? url = term["DisplayText"]?.Value<string?>();

            if (url == null)
                throw new InvalidOperationException($"No DisplayText property found on: {term}");

            JsonObject? parent = FindParentTaxonomyTerm(term, taxonomy);

            while (parent != null && (string?)parent!["ContentType"] != "Taxonomy")
            {
                url = $"{parent!["DisplayText"]}/{url}";
                parent = FindParentTaxonomyTerm(parent, taxonomy);
            }

            return url.Trim('/');
        }

        public JsonArray GetAllTerms(JsonObject taxonomy)
        {
            var results = new JsonArray();
            return GetAllTermsInternal(taxonomy, results);
        }

        private JsonArray GetAllTermsInternal(JsonObject taxonomy, JsonArray results)
        {
            var terms = GetTerms(taxonomy);

            if (terms != null)
            {
                
                JObject.Parse(results.ToJsonString()).Merge(terms);

                foreach (var term in terms!)
                {
                    GetAllTermsInternal(term!.AsObject(), results);
                }
            }

            return results;
        }
    }
}
