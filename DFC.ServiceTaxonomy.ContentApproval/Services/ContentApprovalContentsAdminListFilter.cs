using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentApproval.Services
{
    public class ContentApprovalContentsAdminListFilter : IContentsAdminListFilter
    {
        public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
        {
            var viewModel = new ContentApprovalContentsAdminListFilterViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Constants.ContentApprovalPartPrefix)
                && viewModel.SelectedApprovalStatus.HasValue)
            {
                if (viewModel.SelectedApprovalStatus == ContentReviewStatus.NotInReview)
                {
                    query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                }
                query.With<ContentApprovalPartIndex>(i => i.ReviewStatus == (int?)viewModel.SelectedApprovalStatus);
            }
        }
    }
}
