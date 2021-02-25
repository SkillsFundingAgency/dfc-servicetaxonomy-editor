﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Extensions;
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

            if (part.InDraft && part.ReviewStatus == ContentReviewStatus.NotInReview && await _authorizationService.AuthorizeAsync(currentUser, Permissions.RequestReviewPermissions.RequestReviewPermission, part))
            {
                results.Add(Initialize<ContentApprovalPartViewModel>(
                        "ContentApprovalPart_Admin_RequestReview",
                        viewModel => PopulateViewModel(part, viewModel))
                    .Location("SummaryAdmin", "Actions:First"));
            }

            if (part.ReviewStatus != ContentReviewStatus.NotInReview && await _authorizationService.AuthorizeAsync(currentUser, part.ReviewType.GetRelatedPermission(), part))
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

            var reviewPermission = part.ReviewType.GetRelatedPermission();
            if (part.ReviewStatus == ContentReviewStatus.InReview && (!context.Updater.ModelState.IsValid || !await _authorizationService.AuthorizeAsync(currentUser, reviewPermission)))
            {
                _notifier.Warning(H["This content item is now under review and should not be modified."]);
            }

            var editorShape = GetEditorShapeType(context);
            var reviewStatuses = new[] {ContentReviewStatus.ReadyForReview, ContentReviewStatus.InReview};

            var results = new List<IDisplayResult>();
            // Show Request review option
            if (part.ReviewStatus != ContentReviewStatus.InReview && await _authorizationService.AuthorizeAsync(currentUser, Permissions.RequestReviewPermissions.RequestReviewPermission))
            {
                results.Add(Initialize<ContentApprovalPartViewModel>(
                        $"{editorShape}_RequestReview",
                        viewModel => PopulateViewModel(part, viewModel))
                    .Location("Actions:20"));
            }
            else if (part.ReviewStatus == ContentReviewStatus.InReview && await _authorizationService.AuthorizeAsync(currentUser, reviewPermission))
            {
                results.Add(Initialize<ContentApprovalPartViewModel>(
                        $"{editorShape}_RequestReview",
                        viewModel => PopulateViewModel(part, viewModel, true))
                    .Location("Actions:20"));
            }

            // Show Request revision option
            if (reviewStatuses.Any(r => part.ReviewStatus == r) && await _authorizationService.AuthorizeAsync(currentUser, reviewPermission))
            {
                results.Add(Initialize<ContentApprovalPartViewModel>(
                        $"{editorShape}_RequestRevision",
                        viewModel => PopulateViewModel(part, viewModel))
                    .Location("Actions:First"));
            }

            return Combine(results.ToArray());
        }


        public override async Task<IDisplayResult?> UpdateAsync(ContentApprovalPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new ContentApprovalPartViewModel();
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (part.ReviewStatus == ContentReviewStatus.InReview && !await _authorizationService.AuthorizeAsync(currentUser, part.ReviewType.GetRelatedPermission()))
            {
                updater.ModelState.AddModelError("ReviewStatus", "This item is currently under review and cannot be modified at this time.");
                return await EditAsync(part, context);
            }

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            var keys = updater.ModelState.Keys;

            if (keys.Contains(Constants.SubmitSaveKey))
            {
                var saveType = updater.ModelState[Constants.SubmitSaveKey].AttemptedValue;
                if (saveType.StartsWith(Constants.SubmitRequestApprovalValuePrefix))
                {
                    part.ReviewStatus = ContentReviewStatus.ReadyForReview;
                    part.ReviewType = Enum.Parse<ReviewType>(saveType.Replace(Constants.SubmitRequestApprovalValuePrefix, ""));
                    _notifier.Success(H["{0} is now ready to be reviewed.", part.ContentItem.DisplayText]);
                }
                else // implies a draft save or send back for revision
                {
                    part.ReviewStatus = ContentReviewStatus.NotInReview;
                    part.ReviewType = ReviewType.None;
                }
            }
            else if (keys.Contains(Constants.SubmitPublishKey))
            {
                part.ReviewStatus = ContentReviewStatus.NotInReview;
                part.ReviewType = ReviewType.None;
            }

            return await EditAsync(part, context);
        }
        private static void PopulateViewModel(ContentApprovalPart part, ContentApprovalPartViewModel viewModel, bool isInReviewView = false)
        {
            viewModel.ContentItemId = part.ContentItem.ContentItemId;
            viewModel.ReviewStatus = part.ReviewStatus;
            viewModel.Comment = part.Comment;
            var reviewTypes = EnumExtensions.GetEnumNameAndDisplayNameDictionary(typeof(ReviewType)).Where(rt => rt.Key != ReviewType.None.ToString());
            viewModel.ReviewTypes = isInReviewView ? reviewTypes.Where(rt => rt.Key != part.ReviewType.ToString()) : reviewTypes;
        }
    }
}
