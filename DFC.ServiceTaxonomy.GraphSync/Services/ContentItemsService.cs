using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public interface IContentItemsService
    {
        Task<List<ContentItem>> GetPublishedOnly(string contentType);
        Task<List<ContentItem>> GetPublishedWithDraftVersion(string contentType);
        Task<List<ContentItem>> GetDraft(string contentType);
    }

    //todo: better name
    public class ContentItemsService : IContentItemsService
    {
        private readonly ISession _session;

        public ContentItemsService(ISession session)
        {
            _session = session;
        }

        public async Task<List<ContentItem>> GetPublishedOnly(string contentType)
        {
            return await Get(contentType, true, true);
        }

        public async Task<List<ContentItem>> GetPublishedWithDraftVersion(string contentType)
        {
            return await Get(contentType, false, true);
        }

        public async Task<List<ContentItem>> GetDraft(string contentType)
        {
            return await Get(contentType, true, false);
        }

        private async Task<List<ContentItem>> Get(string contentType, bool latest, bool published)
        {
            return (await _session
                .Query<ContentItem, ContentItemIndex>()
                .Where(x => contentType == x.ContentType && x.Latest == latest && x.Published == published)
                .ListAsync()).ToList();
        }
    }
}
