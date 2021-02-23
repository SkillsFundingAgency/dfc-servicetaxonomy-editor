using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ReviewTypeContentsAdminListFilterDisplayDriver : DisplayDriver<ContentOptionsViewModel>
    {
        private readonly IStringLocalizer S;

        public ReviewTypeContentsAdminListFilterDisplayDriver(
            IStringLocalizer<ContentApprovalContentsAdminListFilterDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        protected override void BuildPrefix(ContentOptionsViewModel model, string htmlFieldPrefix)
        {
            Prefix = ContentApprovalPart.Prefix;
        }

        public override IDisplayResult Edit(ContentOptionsViewModel model, IUpdateModel updater)
        {
            return Initialize<ReviewTypeContentsAdminListFilterViewModel>("ContentsAdminList__ReviewTypeFilter", m =>
                {
                    var reviewTypes = new List<SelectListItem>
                    {
                        new SelectListItem { Text = S["All review types"], Value = "" },
                    };

                    string[] displayNames = EnumExtensions.GetDisplayNames(typeof(ReviewType));
                    string[] names = Enum.GetNames(typeof(ReviewType));

                    reviewTypes.AddRange(displayNames.Zip(names, (displayName, name) => new SelectListItem(displayName, name) ));

                    m.ReviewTypes = reviewTypes;
                }).Location("Actions:25");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            var viewModel = new ReviewTypeContentsAdminListFilterViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, ContentApprovalPart.Prefix)
                && viewModel.SelectedReviewType != null)
            {
                model.RouteValues.TryAdd($"{ContentApprovalPart.Prefix}.SelectedReviewType", viewModel.SelectedReviewType);
            }

            return await EditAsync(model, updater);
        }
    }
}
