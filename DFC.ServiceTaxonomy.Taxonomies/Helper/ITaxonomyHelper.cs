#nullable enable

using Newtonsoft.Json.Linq;

namespace DFC.ServiceTaxonomy.Taxonomies.Helper
{
    public interface ITaxonomyHelper
    {
        JArray GetAllTerms(JObject taxonomy);

        JArray? GetTerms(JObject contentItem);

        JObject? FindParentTaxonomyTerm(JObject termContentItem, JObject taxonomyContentItem);

        string BuildTermUrl(JObject term, JObject taxonomy);
    }
}
