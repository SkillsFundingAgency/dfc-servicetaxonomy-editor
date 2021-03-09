using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
    //todo: one method to return all count types?
    public class ContentItemsApprovalService : IContentItemsApprovalService
    {
        private readonly ISession _session;
        private readonly DefaultContentsAdminListFilter _defaultContentsAdminListFilter;

        public ContentItemsApprovalService(
            ISession session,
            DefaultContentsAdminListFilter defaultContentsAdminListFilter)
        {
            _session = session;
            _defaultContentsAdminListFilter = defaultContentsAdminListFilter;
        }

        public async Task<ContentItemsApprovalCounts> GetManageContentItemCount(DashboardItemsStatusCard card)
        {
            var filterOptions = new ContentOptionsViewModel();
            var query = _session.Query<ContentItem>();

            switch (card)
            {
                case DashboardItemsStatusCard.InDraft:
                    filterOptions.ContentsStatus = ContentsStatus.Draft;
                    break;
                case DashboardItemsStatusCard.Published:
                    filterOptions.ContentsStatus = ContentsStatus.Published;
                    break;
                case DashboardItemsStatusCard.WaitingForReview:
                    filterOptions.ContentsStatus = ContentsStatus.AllVersions;
                    query.With<ContentApprovalPartIndex>(i => i.ReviewStatus == (int?)ContentReviewStatus.ReadyForReview);
                    break;
                case DashboardItemsStatusCard.InReview:
                    //todo: we'll have to filter 'will need review' items according to if there's a draft version
                    // can we do that using With<> ? or do apply custom c# filtering after FilterAsync on the items before count??
                    // or generate a sql query, but then can't reuse _defaultContentsAdminListFilter (so easily)
                    filterOptions.ContentsStatus = ContentsStatus.AllVersions;
                    query.With<ContentApprovalPartIndex>(i => i.ReviewStatus == (int?)ContentReviewStatus.InReview);
                    break;
                default:
                    throw new NotImplementedException();
            }

            // we have the option to c&p and change _defaultContentsAdminListFilter if necessary
            await _defaultContentsAdminListFilter.FilterAsync(filterOptions, query, null);

            ContentItemsApprovalCounts counts = new ContentItemsApprovalCounts();

            //todo: if we always show the review cards with the draft & published cards
            // it would make sense to have a running count for all types, and cache it, so it's only done once per refresh

            if (card != DashboardItemsStatusCard.WaitingForReview
                && card != DashboardItemsStatusCard.InReview)
            {
                counts.Count = await query.CountAsync();
                return counts;
            }

            //what's faster? do we run 5 separate queries for the 2 reviews, and all the review types,
            //or 1 query, and count all the items after?

            int lastReviewType = (int)Enum.GetValues(typeof(ReviewType)).Cast<ReviewType>().Max();

            // assumes non-sparse enum values starting at 0!
            counts.ReviewTypeCounts = new int[lastReviewType + 1];
            Array.Fill(counts.ReviewTypeCounts, 0);

            //todo: we only need to know for each type if there is >0 items, so we could use bools
            // and stop counting when we encounter 1 of each

            //todo: will need to check responsiveness when 100k+ items, and maybe even make the count async??
            var contentItems = query.ToAsyncEnumerable();
            await foreach (ContentItem contentItem in contentItems)
            {
                ++counts.Count;
                ReviewType? reviewStatus = contentItem.As<ContentApprovalPart>()?.ReviewType;
                if (reviewStatus != null)
                {
                    //todo: bounds check?
                    ++counts.ReviewTypeCounts[(int)reviewStatus];
                }
            }

            return counts;
        }
    }
}
