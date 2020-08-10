using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface IContentItemsService
    {
        Task<List<ContentItem>> GetPublishedOnly(string contentType);
        Task<List<ContentItem>> GetPublishedWithDraftVersion(string contentType);
        Task<List<ContentItem>> GetDraft(string contentType);
    }
}
