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

        Task<bool> HasExistingPublishedVersion(string contentItemId);
    }
}
