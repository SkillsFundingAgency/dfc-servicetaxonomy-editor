using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Filters.Query.Services;
using ISession = YesSql.ISession;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
    public class ContentItemsApprovalService : IContentItemsApprovalService
    {
        private readonly ISession _session;
        private readonly IContentsAdminListQueryService _contentsAdminListQueryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentItemsApprovalService(
            ISession session,
            IContentsAdminListFilter defaultContentsAdminListFilter,
            IEnumerable<IContentsAdminListFilter> contentsAdminListFilters,
            IContentsAdminListQueryService contentsAdminListQueryService,
            IHttpContextAccessor httpContextAccessor)
        {
            _session = session;
            _httpContextAccessor = httpContextAccessor;
            _contentsAdminListQueryService = contentsAdminListQueryService;
        }
        public async Task<ContentItemsApprovalCounts> GetManageContentItemCounts(DashboardItemsStatusCard card, IUpdateModel updater)
        {
            ContentItemsApprovalCounts counts = new ContentItemsApprovalCounts();

            switch (card)
            {
                case DashboardItemsStatusCard.InDraft:
                    counts.Count = await GetManageContentItemCount(card, updater);
                    break;
                case DashboardItemsStatusCard.Published:
                    counts.Count = await GetManageContentItemCount(card, updater);
                    var forcePublishedCount = await GetManageContentItemCount(card, updater, null, true);
                    counts.SubCounts = new[] {counts.Count, forcePublishedCount };
                    break;
                case DashboardItemsStatusCard.WaitingForReview:
                case DashboardItemsStatusCard.InReview:
                    var reviewTypes = Enum.GetValues(typeof(ReviewType)).Cast<ReviewType>();
                    var reviewTypeCountTasks = reviewTypes.Select(rt => GetManageContentItemCount(card, updater, rt==ReviewType.None?(ReviewType?)null:rt));

                    int[] reviewTypeCounts = await Task.WhenAll(reviewTypeCountTasks);
                    counts.Count = reviewTypeCounts[0];
                    counts.SubCounts = reviewTypeCounts.ToArray();
                    break;
                case DashboardItemsStatusCard.MyWorkItems:
                    counts.MyItems = await GetMyContent();
                    break;
            }

            return counts;
        }

        private async Task<List<ContentItem>> GetMyContent()
        {
            var myContent = new List<ContentItem>();
            var user = _httpContextAccessor.HttpContext?.User.Identity.Name ?? string.Empty;

            var query = _session.Query<ContentItem>().With<ContentItemIndex>(i => i.Author == user && i.Latest);
            await foreach (var item in  query.OrderByDescending(c => c.ModifiedUtc).ToAsyncEnumerable())
            {
                // For some reason can't filter DisplayText in the query so having to check here and enforce the count limit
                if (!string.IsNullOrWhiteSpace(item.DisplayText))
                {
                    myContent.Add(item);
                }

                if (myContent.Count == 10)
                {
                    break;
                }
            }
            return myContent;
        }

        // if we need to count 'will need review' items, we'll have to also check for a draft version
        // can we do that using With<> ? or do apply custom c# filtering after FilterAsync on the items before count??
        // or generate a sql query, but then can't reuse _defaultContentsAdminListFilter (so easily)

        private async Task<int> GetManageContentItemCount(
            DashboardItemsStatusCard card, IUpdateModel updater,
            ReviewType? reviewType = null, bool isForcePublished = false)
        {
            //var querySession = _session.Query<ContentItem>();

            var filterOptions = new ContentOptionsViewModel()
            {
                FilterResult = new QueryFilterResult<ContentItem>(new ConcurrentDictionary<string, QueryTermOption<ContentItem>>())
            };

            switch (card)
            {
                case DashboardItemsStatusCard.InDraft:
                    filterOptions.ContentsStatus = ContentsStatus.Draft;
                    //querySession.With<ContentItemIndex>(i => i.Latest && !i.Published);
                    break;
                case DashboardItemsStatusCard.Published:
                    filterOptions.ContentsStatus = ContentsStatus.Published;
                    //querySession.With<ContentItemIndex>(i => i.Latest && !i.Published);
                    break;
                case DashboardItemsStatusCard.WaitingForReview:
                    filterOptions.ContentsStatus = ContentsStatus.AllVersions;
                    break;
                case DashboardItemsStatusCard.InReview:
                    filterOptions.ContentsStatus = ContentsStatus.AllVersions;
                    break;
                default:
                    throw new NotImplementedException();
            }

            var query = await _contentsAdminListQueryService.QueryAsync(filterOptions, updater);

            switch (card)
            {
                case DashboardItemsStatusCard.Published:
                    if (isForcePublished)
                    {
                        query.With<ContentApprovalPartIndex>(i => i.IsForcePublished);
                    }
                    break;
                case DashboardItemsStatusCard.WaitingForReview:
                    query.With<ContentApprovalPartIndex>(i => i.ReviewStatus == (int?)ReviewStatus.ReadyForReview);
                    if (reviewType != null)
                    {
                        query.With<ContentApprovalPartIndex>(i => i.ReviewType == (int)reviewType);
                    }
                    break;
                case DashboardItemsStatusCard.InReview:
                    filterOptions.ContentsStatus = ContentsStatus.AllVersions;
                    query.With<ContentApprovalPartIndex>(i => i.ReviewStatus == (int?)ReviewStatus.InReview);
                    if (reviewType != null)
                    {
                        query.With<ContentApprovalPartIndex>(i => i.ReviewType == (int)reviewType);
                    }
                    break;
            }
            return await query.CountAsync();
        }
    }
}
