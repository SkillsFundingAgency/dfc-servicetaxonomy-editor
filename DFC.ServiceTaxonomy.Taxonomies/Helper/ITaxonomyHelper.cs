#nullable enable


using System.Text.Json.Nodes;

namespace DFC.ServiceTaxonomy.Taxonomies.Helper
{
    public interface ITaxonomyHelper
    {
        JsonArray GetAllTerms(JsonObject taxonomy);

        JsonArray? GetTerms(JsonObject contentItem);

        JsonObject? FindParentTaxonomyTerm(JsonObject termContentItem, JsonObject taxonomyContentItem);

        string BuildTermUrl(JsonObject term, JsonObject taxonomy);
    }
}
