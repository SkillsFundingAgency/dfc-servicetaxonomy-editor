using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.Content.Services
{
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

        public async Task<List<ContentItem>> GetActive(string contentType)
        {
            return (await _session
                .Query<ContentItem, ContentItemIndex>()
                .Where(x => contentType == x.ContentType && (x.Latest || x.Published))
                .ListAsync()).ToList();
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

        public async Task<IEnumerable<ContentItem>> Get(
            string contentType,
            DateTime since,
            bool? latest = null,
            bool? published = null)
        {
            // this works when using sqlite, but it seems there's a bug in yessql when executing the query against azure sql

            // return await _session
            //     .Query<ContentItem, ContentItemIndex>(x =>
            //         x.ContentType == contentTypeDefinition.Name
            //         && (latest == null || x.Latest == latest)
            //         && (published == null || x.Published == published)
            //         && (x.CreatedUtc >= lastSynced || x.ModifiedUtc >= lastSynced))
            //     .ListAsync();

            // so instead we pick one of 4 different queries depending on whether latest or published is null

            if (latest != null && published != null)
            {
                return await _session
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentType == contentType
                        && x.Latest == latest && x.Published == published
                        && (x.CreatedUtc >= since || x.ModifiedUtc >= since))
                    .ListAsync();
            }

            if (latest == null && published != null)
            {
                return await _session
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentType == contentType
                        && x.Published == published
                        && (x.CreatedUtc >= since || x.ModifiedUtc >= since))
                    .ListAsync();
            }

            if (latest != null && published == null)
            {
                return await _session
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentType == contentType
                        && x.Latest == latest
                        && (x.CreatedUtc >= since || x.ModifiedUtc >= since))
                    .ListAsync();
            }

            // latest == null && published == null

            return await _session
                .Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType == contentType
                    && (x.CreatedUtc >= since || x.ModifiedUtc >= since))
                .ListAsync();
        }

        //todo: return count
        public async Task<IEnumerable<ContentItem>> GetCount(
            bool? latest = null,
            bool? published = null)
        {
            // this works when using sqlite, but it seems there's a bug in yessql when executing the query against azure sql
            // if this worked we could have contentType and since as optional too

            // return await _session
            //     .Query<ContentItem, ContentItemIndex>(x =>
            //         x.ContentType == contentTypeDefinition.Name
            //         && (latest == null || x.Latest == latest)
            //         && (published == null || x.Published == published)
            //         && (x.CreatedUtc >= lastSynced || x.ModifiedUtc >= lastSynced))
            //     .ListAsync();

            // so instead we pick one of 4 different queries depending on whether latest or published is null

            if (latest != null && published != null)
            {
                return await _session
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.Latest == latest && x.Published == published)
                    .ListAsync();
            }

            if (latest == null && published != null)
            {
                return await _session
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.Published == published)
                    .ListAsync();
            }

            if (latest != null && published == null)
            {
                return await _session
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.Latest == latest)
                    .ListAsync();
            }

            // latest == null && published == null

            //todo:
            // return await _session
            //     .Query<ContentItem, ContentItemIndex>(x => { })
            //     .ListAsync();
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ContentItem>> GetDeleted(
            string contentType,
            DateTime since)
        {
            return (await _session
                    .Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentType == contentType &&
                        x.ModifiedUtc >= since)
                    .ListAsync())
                .GroupBy(x => x.ContentItemId)
                .Select(grp => grp.OrderByDescending(x => x.ModifiedUtc).First())
                .Where(x => !x.Latest && !x.Published);
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
