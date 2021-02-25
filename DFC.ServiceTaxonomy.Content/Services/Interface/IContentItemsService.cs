using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.Content.Services.Interface
{
    public interface IContentItemsService
    {
        Task<List<ContentItem>> GetPublishedOnly(string contentType);
        Task<List<ContentItem>> GetPublishedWithDraftVersion(string contentType);
        Task<List<ContentItem>> GetDraft(string contentType);
        Task<List<ContentItem>> GetActive(string contentType);

        Task<int> GetDraftCount();
        Task<int> GetPublishedCount();

        Task<bool> HasExistingPublishedVersion(string contentItemId);

        Task<IEnumerable<ContentItem>> Get(string contentType,
            DateTime since,
            bool? latest = null,
            bool? published = null);

        Task<int> GetCount(
            bool? latest = null,
            bool? published = null);

        Task<IEnumerable<ContentItem>> GetDeleted(
            string contentType,
            DateTime since);
    }
}
