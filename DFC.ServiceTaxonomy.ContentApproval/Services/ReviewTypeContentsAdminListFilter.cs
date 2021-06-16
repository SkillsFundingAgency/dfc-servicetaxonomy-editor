//using System.Threading.Tasks;
//using DFC.ServiceTaxonomy.ContentApproval.Indexes;
//using DFC.ServiceTaxonomy.ContentApproval.Models;
//using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
//using OrchardCore.ContentManagement;
//using OrchardCore.Contents.Services;
//using OrchardCore.Contents.ViewModels;
//using OrchardCore.DisplayManagement.ModelBinding;
//using OrchardCore.DisplayManagemen
//using YesSql;

//namespace DFC.ServiceTaxonomy.ContentApproval.Services
//{
//    public class ReviewTypeContentsAdminListFilter : IContentsAdminListFilter
//    {
//        public async Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
//        {
//            var viewModel = new ReviewTypeContentsAdminListFilterViewModel();

//            if (await updater.TryUpdateModelAsync(viewModel, Constants.ContentApprovalPartPrefix)
//                && viewModel.SelectedReviewType.HasValue)
//            {
//                query.With<ContentApprovalPartIndex>(i => i.ReviewType == (int?)viewModel.SelectedReviewType);
//            }
//        }
//    }
//}
