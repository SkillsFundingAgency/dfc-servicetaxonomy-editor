#nullable enable

using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.Taxonomies.Helper
{
    public interface ITaxonomyHelper
    {
        List<dynamic>? GetTerms(dynamic contentItem);

        dynamic? FindParentTaxonomyTerm(dynamic termContentItem, dynamic taxonomyContentItem);

        string BuildTermUrl(dynamic term, dynamic taxonomy);
    }
}
