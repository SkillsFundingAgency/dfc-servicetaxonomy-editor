﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.ContentApproval.Drivers
{
    public class ContentApprovalPartDisplayDriver : ContentPartDisplayDriver<ContentApprovalPart>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<ContentApprovalPartDisplayDriver> H;

        public ContentApprovalPartDisplayDriver(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor, INotifier notifier, IHtmlLocalizer<ContentApprovalPartDisplayDriver> htmlLocalizer)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public override async Task<IDisplayResult?> DisplayAsync(ContentApprovalPart part, BuildPartDisplayContext context)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            var results = new List<IDisplayResult>();

            if (part.InDraft && part.ApprovalStatus == null && await _authorizationService.AuthorizeAsync(currentUser, Permissions.RequestReviewPermissions.RequestReviewPermission, part))
            {
                results.Add(Initialize<ContentApprovalPartViewModel>(
                        "ContentApprovalPart_Admin_RequestReview",
                        viewModel => PopulateViewModel(part, viewModel))
                    .Location("SummaryAdmin", "Actions:First"));
            }

            if (part.ApprovalStatus == ContentApprovalStatus.ReadyForReview && await _authorizationService.AuthorizeAsync(currentUser,
                Permissions.CanPerformReviewPermissions.CanPerformReviewPermission, part))
            {
                results.Add(Initialize<ContentApprovalPartViewModel>(
                        "ContentApprovalPart_Admin_InReview",
                        viewModel => PopulateViewModel(part, viewModel))
                    .Location("SummaryAdmin", "Actions:First"));
            }

            return Combine(results.ToArray());
        }

        public override async Task<IDisplayResult?> EditAsync(ContentApprovalPart part, BuildPartEditorContext context)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser == null)
            {
                return null;
            }

            var editorShape = GetEditorShapeType(context);
            var reviewStatuses = new[] {ContentApprovalStatus.ReadyForReview, ContentApprovalStatus.InReview};

            // Show Request review option
            if (part.ApprovalStatus != ContentApprovalStatus.InReview &&
                await _authorizationService.AuthorizeAsync(currentUser, Permissions.RequestReviewPermissions.RequestReviewPermission))
            {
                return Initialize<ContentApprovalPartViewModel>(
                        $"{editorShape}_RequestReview",
                        viewModel => PopulateViewModel(part, viewModel))
                    .Location("Actions:First");
            }

            // Show Approval option
            if (reviewStatuses.Any(r => part.ApprovalStatus == r) && await _authorizationService.AuthorizeAsync(currentUser,Permissions.CanPerformReviewPermissions.CanPerformReviewPermission))
            {
                return Initialize<ContentApprovalPartViewModel>(
                        $"{editorShape}_ApprovalResult",
                        viewModel => PopulateViewModel(part, viewModel))
                    .Location("Actions:First");
            }

            return null;
        }

        public override async Task<IDisplayResult?> UpdateAsync(ContentApprovalPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ContentApprovalPartViewModel();
            
            await updater.TryUpdateModelAsync(viewModel, Prefix);

            var keys = updater.ModelState.Keys;

            if (keys.Contains("submit.Save"))
            {
                var saveType = updater.ModelState["submit.Save"];
                if (saveType.AttemptedValue.Contains("Save"))
                {
                    part.ApprovalStatus = null;
                }
                else if (saveType.AttemptedValue.Contains("RequestApproval"))
                {
                    part.ApprovalStatus = ContentApprovalStatus.ReadyForReview;
                    _notifier.Success(H[$"{0} is now ready to be reviewed.", part.ContentItem.DisplayText]);
                }
            }
            else if (keys.Contains("submit.Publish"))
            {
                part.ApprovalStatus = null;
            }

            return await EditAsync(part, context);
        }
        private static void PopulateViewModel(ContentApprovalPart part, ContentApprovalPartViewModel viewModel)
        {
            viewModel.ContentItemId = part.ContentItem.ContentItemId;
            viewModel.ApprovalStatus = part.ApprovalStatus;
            viewModel.Comment = part.Comment;
        }
    }
}
