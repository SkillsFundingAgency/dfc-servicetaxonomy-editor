using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        {
            return Combine(
                View("ContentsAdminFilters_Thumbnail__ReviewStatus", model).Location("Thumbnail", "Content:50"),
                View("ContentsAdminFilters_Thumbnail__ReviewType", model).Location("Thumbnail", "Content:60"));
        }

        public override IDisplayResult Edit(ContentOptionsViewModel model, IUpdateModel updater)
        {
            return Initialize<ContentApprovalContentsAdminListFilterViewModel>("ContentsAdminList__ContentApprovalFilter", m =>
            {
                model.FilterResult.MapTo(m);

                var reviewStatuses = new List<SelectListItem>
                    {
                        new SelectListItem { Text = S["All review statuses"], Value = "", Selected = string.IsNullOrEmpty(m.SelectedReviewStatus?.ToString())},
                    };

                reviewStatuses.AddRange(GetSelectList(typeof(ReviewStatusFilterOptions), m.SelectedReviewStatus?.ToString()));
                m.ReviewStatuses = reviewStatuses;

                var reviewTypes = new List<SelectListItem>
                    {
                        new SelectListItem { Text = S["All reviewer types"], Value = "" , Selected = string.IsNullOrEmpty(m.SelectedReviewType?.ToString())},
                    };

                reviewTypes.AddRange(GetSelectList(typeof(ReviewType), m.SelectedReviewType?.ToString()));
                m.ReviewTypes = reviewTypes;

            }).Location("Actions:20");
        }

        private static IEnumerable<SelectListItem> GetSelectList(Type enumType, string? selectedValue)
        {
            FieldInfo[] enumFields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            return enumFields.Select(fi => (fi, da: fi.GetCustomAttribute<DisplayAttribute>()))
                .OrderBy(v => v.da?.Order ?? -1)
                .Select(v => new SelectListItem(v.da?.Name ?? v.fi.Name, v.fi.Name.ToLower(), !string.IsNullOrEmpty(selectedValue) && v.fi.Name.Equals(selectedValue, StringComparison.InvariantCultureIgnoreCase)));
        }


        public override async Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            var viewModel = new ContentApprovalContentsAdminListFilterViewModel();
            if(await updater.TryUpdateModelAsync(viewModel, Constants.ContentApprovalPartPrefix))
            {
                model?.FilterResult?.MapFrom(viewModel);
            }
            // Code conventions in STAX clashing with Orchard Core approach
#pragma warning disable AsyncFixer02 // Long-running or blocking operations inside an async method
#pragma warning disable CS8604 // Possible null reference argument.
            return Edit(model, updater);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore AsyncFixer02 // Long-running or blocking operations inside an async method
        }
    }
}
