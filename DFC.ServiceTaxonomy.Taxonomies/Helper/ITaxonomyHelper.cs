#nullable enable

using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Taxonomies.Helper
{
    public interface ITaxonomyHelper
    {
        List<ContentItem>? GetTerms(ContentItem contentItem);

        ContentItem? FindParentTaxonomyTerm(ContentItem termContentItem, ContentItem taxonomyContentItem);

        string BuildTermUrl(ContentItem term, ContentItem taxonomy);
    }
}
