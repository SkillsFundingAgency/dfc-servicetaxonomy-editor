
using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
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

        public async Task<int> GetManageContentItemCount(DashboardItemsStatusCard card)
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

            return await query.CountAsync();
        }
    }
}
