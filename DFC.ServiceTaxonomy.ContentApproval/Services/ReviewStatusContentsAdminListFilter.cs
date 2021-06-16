//using System.Threading.Tasks;
//using DFC.ServiceTaxonomy.ContentApproval.Indexes;
//using DFC.ServiceTaxonomy.ContentApproval.Models;
//using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
//using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
//using OrchardCore.ContentManagement;
//using OrchardCore.Contents.Services;
//using OrchardCore.Contents.ViewModels;
//using OrchardCore.DisplayManagement.ModelBinding;
//using YesSql;

//namespace DFC.ServiceTaxonomy.ContentApproval.Services
//{
//    public class ReviewStatusContentsAdminListFilter : IContentsAdminListFilter
//    {
//        public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
//        {
//            var viewModel = new ReviewStatusContentsAdminListFilterViewModel();

//            if (await updater.TryUpdateModelAsync(viewModel, Constants.ContentApprovalPartPrefix)
//                && viewModel.SelectedApprovalStatus.HasValue)
//            {
//                if (viewModel.SelectedApprovalStatus == ReviewStatusFilterOptions.WillNeedReview)
//                {
//                    query.With<ContentItemIndex>(x => x.Latest && !x.Published);
//                }

//                if (viewModel.SelectedApprovalStatus == ReviewStatusFilterOptions.ForcePublished)
//                {
//                    query.With<ContentApprovalPartIndex>(i => i.IsForcePublished);
//                }
//                else
//                {
//                    query.With<ContentApprovalPartIndex>(i => i.ReviewStatus == (int?)viewModel.SelectedApprovalStatus);
//                }
//            }
//        }
//    }
//}
