﻿using System;
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
            Prefix = "ContentApproval";
        }

        public override IDisplayResult Edit(ContentOptionsViewModel model, IUpdateModel updater)
        {
            //todo: do we have 1 filter with eg waiting for ux approval, waiting for content design approval etc?
            // do we have in draft and published in there too?
            // do we have 2 filters, one for waiting/in review and one for the review type?
            // what if have 2 and only one filter applied?
            // waiting + not selected -> all waiting (of any type)
            // not selected + UX -> all in ux waiting/ready
            // leave draft and published out?
            // have filter dropdown with combos in there .. eg. waiting for ux review? that selects both
            // if we update the top left filter, do we hide the custom filters, or leave them visible?

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
            //todo: common const
            if (await updater.TryUpdateModelAsync(viewModel, "ContentApproval")
                && viewModel.SelectedApprovalStatus != null)
            {
                //todo: common const
                model.RouteValues.TryAdd("ContentApproval.SelectedApprovalStatus", viewModel.SelectedApprovalStatus);
            }

            return await EditAsync(model, updater);
        }
    }
}
