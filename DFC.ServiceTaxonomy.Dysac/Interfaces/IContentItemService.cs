using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Dysac.Services
{
    /// <summary>
    /// Helper for working with OrchardCore content items.
    /// </summary>
    public interface IContentItemService
    {
        /// <summary>
        /// Get content items that reference <paramref name="contentItem"/>.
        /// </summary>
        /// <param name="contentItem">The item to search for.</param>
        /// <returns>The referencing content items.</returns>
        Task<IEnumerable<ContentItem>> GetReferencingContentItems(string contentItemId);
    }
}
