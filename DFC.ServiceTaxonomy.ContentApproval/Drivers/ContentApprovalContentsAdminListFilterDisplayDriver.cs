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
            Prefix = ContentApprovalPart.Prefix;
        }

        public override IDisplayResult Edit(ContentOptionsViewModel model, IUpdateModel updater)
        {
            return Initialize<ContentApprovalContentsAdminListFilterViewModel>("ContentsAdminList__ContentApprovalPartFilter", m =>
                {
                    var approvalStatuses = new List<SelectListItem>
                    {
                        new SelectListItem { Text = S["All approval statuses"], Value = "" },
                    };

                    string[] displayNames = EnumExtensions.GetDisplayNames(typeof(ContentReviewStatus));
                    string[] names = Enum.GetNames(typeof(ContentReviewStatus));

                    approvalStatuses.AddRange(displayNames.Zip(names, (displayName, name) => new SelectListItem(displayName, name) ));

                    m.ApprovalStatuses = approvalStatuses;
                }).Location("Actions:20");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            var viewModel = new ContentApprovalContentsAdminListFilterViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, ContentApprovalPart.Prefix)
                && viewModel.SelectedApprovalStatus != null)
            {
                model.RouteValues.TryAdd($"{ContentApprovalPart.Prefix}.SelectedApprovalStatus", viewModel.SelectedApprovalStatus);
            }

            return await EditAsync(model, updater);
        }
    }
}
