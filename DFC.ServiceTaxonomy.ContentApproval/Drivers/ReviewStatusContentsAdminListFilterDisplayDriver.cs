using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Extensions;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using DFC.ServiceTaxonomy.ContentApproval.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.ContentApproval.Drivers
{
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

        public override IDisplayResult Display(ContentOptionsViewModel model)
            => View("ContentsAdminFilters_Thumbnail__ContentApproval", model).Location("Thumbnail", "Content:20.1");

        public override IDisplayResult Edit(ContentOptionsViewModel model, IUpdateModel updater)
        {
            return Initialize<ContentApprovalContentsAdminListFilterViewModel>("ContentsAdminList__ReviewStatusFilter", m =>
            {
                model.FilterResult.MapTo(m);
                var reviewStatuses = new List<SelectListItem>
                    {
                        new SelectListItem { Text = S["All review statuses"], Value = "", Selected = string.IsNullOrEmpty(m.SelectedReviewStatus?.ToString())},
                    };

                reviewStatuses.AddRange(EnumExtensions.GetSelectList(typeof(ReviewStatusFilterOptions), m.SelectedReviewStatus?.ToString()));
                m.ReviewStatuses = reviewStatuses;
            }).Location("Actions:20");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            var viewModel = new ContentApprovalContentsAdminListFilterViewModel();
            if(await updater.TryUpdateModelAsync(viewModel, Constants.ContentApprovalPartPrefix))
            {
                model?.FilterResult?.MapFrom(viewModel);
            }
#pragma warning disable AsyncFixer02 // Long-running or blocking operations inside an async method
#pragma warning disable CS8604 // Possible null reference argument.
            return Edit(model, updater);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore AsyncFixer02 // Long-running or blocking operations inside an async method
        }
    }
}
