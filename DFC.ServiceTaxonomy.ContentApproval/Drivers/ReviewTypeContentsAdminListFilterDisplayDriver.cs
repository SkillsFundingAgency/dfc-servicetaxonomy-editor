using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Extensions;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.ContentApproval.Drivers
{
    public class ReviewTypeContentsAdminListFilterDisplayDriver : DisplayDriver<ContentOptionsViewModel>
    {
        private readonly IStringLocalizer S;

        public ReviewTypeContentsAdminListFilterDisplayDriver(
            IStringLocalizer<ReviewStatusContentsAdminListFilterDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        protected override void BuildPrefix(ContentOptionsViewModel model, string htmlFieldPrefix)
        {
            Prefix = Constants.ContentApprovalPartPrefix;
        }

        public override IDisplayResult Edit(ContentOptionsViewModel model, IUpdateModel updater)
        {
            return Initialize<ReviewTypeContentsAdminListFilterViewModel>("ContentsAdminList__ReviewTypeFilter", m =>
                {
                    var reviewTypes = new List<SelectListItem>
                    {
                        new SelectListItem { Text = S["All review types"], Value = "" },
                    };

                    reviewTypes.AddRange(EnumExtensions.GetSelectList(typeof(ReviewType)));

                    m.ReviewTypes = reviewTypes;
                }).Location("Actions:20");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            var viewModel = new ReviewTypeContentsAdminListFilterViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Constants.ContentApprovalPartPrefix)
                && viewModel.SelectedReviewType != null)
            {
                model.RouteValues.TryAdd($"{Constants.ContentApprovalPartPrefix}.SelectedReviewType", viewModel.SelectedReviewType);
            }

            return await EditAsync(model, updater);
        }
    }
}
