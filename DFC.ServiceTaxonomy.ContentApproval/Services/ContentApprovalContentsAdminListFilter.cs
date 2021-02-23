using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Indexes;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using OrchardCore.ContentManagement;
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
            //todo: do we need to set Prefix?

            //todo: common const for prefix
            if (await updater.TryUpdateModelAsync(viewModel, "ContentApproval")
                && viewModel.SelectedApprovalStatus.HasValue)
            {
                query.With<ContentApprovalPartIndex>(i => i.ReviewStatus == viewModel.SelectedApprovalStatus.ToString());
            }
        }
    }
}
