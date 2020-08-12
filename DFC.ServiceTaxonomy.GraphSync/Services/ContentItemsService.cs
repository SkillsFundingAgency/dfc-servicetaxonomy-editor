using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    //todo: better name
    public class ContentItemsService : IContentItemsService
    {
        private readonly ISession _session;
        private readonly IContentManagerSession _contentManagerSession;

        public ContentItemsService(
            ISession session,
            IContentManagerSession contentManagerSession)
        {
            _session = session;
            _contentManagerSession = contentManagerSession;
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

        public async Task<bool> HasExistingPublishedVersion(string contentItemId)
        {
            // check if one exists, without loading it and running handlers etc.

            // this might be valid optimisation, but would need to dig deeper before enabling it
            // if (_contentManagerSession.RecallPublishedItemId(contentItemId, out ContentItem _))
            //     return true;

            return (await _session
                .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentItemId == contentItemId && x.Published)
                .FirstOrDefaultAsync())
                != default;
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
