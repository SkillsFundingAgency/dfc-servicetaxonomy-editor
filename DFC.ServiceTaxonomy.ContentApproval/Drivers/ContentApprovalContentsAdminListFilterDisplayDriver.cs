using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Extensions;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.ContentApproval.Drivers
{
    //todo: rename to ContentReviewStatus...
    public class ContentApprovalContentsAdminListFilterDisplayDriver : DisplayDriver<ContentOptionsViewModel>
    {
        private readonly IStringLocalizer S;

        public ContentApprovalContentsAdminListFilterDisplayDriver(
            IStringLocalizer<ContentApprovalContentsAdminListFilterDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        protected override void BuildPrefix(ContentOptionsViewModel model, string htmlFieldPrefix)
        {
            Prefix = Constants.ContentApprovalPartPrefix;
        }

        public override IDisplayResult Edit(ContentOptionsViewModel model, IUpdateModel updater)
        {
            return Initialize<ContentApprovalContentsAdminListFilterViewModel>("ContentsAdminList__ContentApprovalPartFilter", m =>
                {
                    var approvalStatuses = new List<SelectListItem>
                    {
                        new SelectListItem { Text = S["All review statuses"], Value = "" },
                    };

                    approvalStatuses.AddRange(EnumExtensions.GetSelectList(typeof(ContentReviewStatus)));

                    m.ApprovalStatuses = approvalStatuses;
                }).Location("Actions:25");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            var viewModel = new ContentApprovalContentsAdminListFilterViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Constants.ContentApprovalPartPrefix)
                && viewModel.SelectedApprovalStatus != null)
            {
                model.RouteValues.TryAdd($"{Constants.ContentApprovalPartPrefix}.SelectedApprovalStatus", viewModel.SelectedApprovalStatus);
            }

            return await EditAsync(model, updater);
        }
    }
}
