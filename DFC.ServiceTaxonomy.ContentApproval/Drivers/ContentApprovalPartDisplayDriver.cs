using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Extensions;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentPreview;
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

            // Todo: removed this for the time being due to the requirement for commenting which needs to be defined in this view first.
            //if (part.IsInDraft() && part.ReviewStatus == ReviewStatus.WillNeedReview && await _authorizationService.AuthorizeAsync(currentUser, Permissions.RequestReviewPermissions.RequestReviewPermission, part))
            //{
            //    results.Add(Initialize<ContentApprovalPartViewModel>(
            //            "ContentApprovalPart_Admin_RequestReview",
            //            viewModel => PopulateViewModel(part, viewModel))
            //        .Location("SummaryAdmin", "Actions:First"));
            //}

            if (part.ReviewStatus != ReviewStatus.NotInReview && await _authorizationService.AuthorizeAsync(currentUser, part.ReviewType.GetRelatedPermission(), part))
            {
                results.Add(Initialize<ContentApprovalPartViewModel>(
                        "ContentApprovalPart_Admin_InReview",
                        viewModel => PopulateViewModel(part, viewModel))
                    .Location("SummaryAdmin", "Actions:First"));
            }

            // Todo: see above, hidden for time being.
            // Show comment field
            //results.Add(Initialize<ContentApprovalPartViewModel>(
            //        $"ContentApprovalPart_Edit_Comment",
            //        viewModel => PopulateViewModel(part, viewModel))
            //    .Location("SummaryAdmin", "Content"));

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
            if (part.ReviewStatus == ReviewStatus.InReview && (!context.Updater.ModelState.IsValid || !await _authorizationService.AuthorizeAsync(currentUser, reviewPermission)))
            {
                _notifier.Warning(H["This content item is now under review and should not be modified."]);
            }

            var editorShape = GetEditorShapeType(context);
            var reviewStatuses = new[] { ReviewStatus.ReadyForReview, ReviewStatus.InReview };

            var results = new List<IDisplayResult>();
            // Show force publish
            if (part.ReviewStatus == ReviewStatus.NotInReview && await _authorizationService.AuthorizeAsync(currentUser, Permissions.ForcePublishPermissions.ForcePublishPermission))
            {
                results.Add(Initialize<ContentApprovalPartViewModel>(
                        $"{editorShape}_ForcePublish",
                        viewModel => PopulateViewModel(part, viewModel))
                    .Location("Actions:15"));
            }

            // Show Request review option
            if (await _authorizationService.AuthorizeAsync(currentUser, Permissions.RequestReviewPermissions.RequestReviewPermission))
            {
                results.Add(Initialize<ContentApprovalPartViewModel>(
                        $"{editorShape}_RequestReview",
                        viewModel => PopulateViewModel(part, viewModel))
                    .Location("Actions:16"));
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
            var isPreview = _httpContextAccessor.HttpContext?.Features.Get<ContentPreviewFeature>()?.Previewing ?? false;
            if (isPreview)
            {
                return await EditAsync(part, context);
            }

            var viewModel = new ContentApprovalPartViewModel();
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (part.ReviewStatus == ReviewStatus.InReview && !await _authorizationService.AuthorizeAsync(currentUser, part.ReviewType.GetRelatedPermission()))
            {
                updater.ModelState.AddModelError("ReviewStatus", "This item is currently under review and cannot be modified at this time.");
                return await EditAsync(part, context);
            }

            await updater.TryUpdateModelAsync(viewModel, Prefix);
            var keys = updater.ModelState.Keys;

            // For Publish, Force-Publish, Save Draft & Exit and Send Back actions the user must enter a comment for the audit trail
            if (!IsAuditTrailCommentValid(part, updater))
            {
                updater.ModelState.AddModelError("Comment", "Please update the comment field before submitting.");
                return await EditAsync(part, context);
            }

            if (keys.Contains(Constants.SubmitSaveKey))
            {
                var saveType = updater.ModelState[Constants.SubmitSaveKey].AttemptedValue;
                if (saveType.StartsWith(Constants.SubmitRequestApprovalValuePrefix))
                {
                    part.ReviewStatus = ReviewStatus.ReadyForReview;
                    part.ReviewType = Enum.Parse<ReviewType>(saveType.Replace(Constants.SubmitRequestApprovalValuePrefix, ""));
                    _notifier.Success(H["{0} is now ready to be reviewed.", part.ContentItem.DisplayText]);
                }
                else // implies a draft save or send back for revision
                {
                    part.ReviewStatus = ReviewStatus.NotInReview;
                    part.ReviewType = ReviewType.None;
                }
            }
            else if (keys.Contains(Constants.SubmitPublishKey))
            {
                part.ReviewStatus = ReviewStatus.NotInReview;
                var publishType = updater.ModelState[Constants.SubmitPublishKey].AttemptedValue;
                if (publishType.StartsWith(Constants.SubmitRequestApprovalValuePrefix))
                {
                    part.IsForcePublished = true;
                    part.ReviewType = Enum.Parse<ReviewType>(publishType.Replace(Constants.SubmitRequestApprovalValuePrefix, ""));
                }
                else
                {
                    part.IsForcePublished = false;
                    part.ReviewType = ReviewType.None;
                }
            }

            // save the updateModel in the content approval part for later use in customising the audit event logging
            part.updateModel = updater;

            return await EditAsync(part, context);
        }

        private static void PopulateViewModel(ContentApprovalPart part, ContentApprovalPartViewModel viewModel)
        {
            viewModel.ContentItemId = part.ContentItem.ContentItemId;
            viewModel.ReviewStatus = part.ReviewStatus;
            viewModel.ReviewTypes = EnumExtensions.GetEnumNameAndDisplayNameDictionary(typeof(ReviewType)).Where(rt => rt.Key != ReviewType.None.ToString());
        }

        private static bool IsAuditTrailCommentValid(ContentApprovalPart part, IUpdateModel updateModel)
        {
            bool commentValid = true;

            var isCommentRequired = IsCommentRequired(updateModel);
            if(isCommentRequired)
            {
                var auditTrailPartExists = part.ContentItem.Has<OrchardCore.Contents.AuditTrail.Models.AuditTrailPart>();
                if (auditTrailPartExists)
                {
                    var auditTrailPartComment = part.ContentItem.As<OrchardCore.Contents.AuditTrail.Models.AuditTrailPart>().Comment;
                    if (string.IsNullOrEmpty(auditTrailPartComment))
                    {
                        commentValid = false;
                    }
                }
            }

            return commentValid;
        }

        private static bool IsCommentRequired(IUpdateModel updateModel)
        {
            string actionType = string.Empty;
            bool isCommentRequired = false;

            /*
             *  Button/Action matrix         Key/Action                                                                     Comment
             *  --------------------         ---------------------------------------------------------------------------    ---------------------
             *  Publish                     - constants.SubmitPublishKey action = submit.Publish                            Required
             *  Publish and continue        - constants.SubmitPublishKey action = submit.PublishAndContinue                 Required
             *  
             *  Force Publish
             *      Content Design          - constants.SubmitPublishKey action = submit.RequestApproval - ContentDesign    Required
             *      Stakeholder             - constants.SubmitPublishKey action = submit.RequestApproval - Stakeholder      Required
             *      SME                     - constants.SubmitPublishKey action = submit.RequestApproval - SME              Required
             *      UX                      - constants.SubmitPublishKey action = submit.RequestApproval - UX               Required
             *      
             *  Save Draft and continue     - constants.SubmitSaveKey    action = submit.SaveAndContinue                    Optional
             *  Save Draft and exit         - constants.SubmitSaveKey    action = submit.Save                               Required
             *  
             *  Request review
             *      Content Design          - constants.SubmitSaveKey    action = submit.RequestApproval - ContentDesign    Optional
             *      Stakeholder             - constants.SubmitSaveKey    action = submit.RequestApproval - Stakeholder      Optional
             *      SME                     - constants.SubmitSaveKey    action = submit.RequestApproval - SME              Optional
             *      UK                      - constants.SubmitSaveKey    action = submit.RequestApproval - UX               Optional
             *      
             *  Send back                   - constants.SubmitSaveKey    action = submit.RequiresRevision                   Required
             *               
             *  Preview draft               - N/A (opens new tab)
             *  
             *  Visualise draft graph       - N/A (opens new tab)
             *  Visualise published graph   - N/A (opens new tab)
             *  
             *  Cancel                      - N/A (exits page)
             */

            if (updateModel != null && updateModel.ModelState != null && updateModel.ModelState?.Keys != null)
            {
                var keys = updateModel.ModelState.Keys;
                if (keys.Any())
                {
                    //----------------------------------------------
                    // Publish/Force-Publish button
                    //----------------------------------------------
                    if (keys.Contains(Constants.SubmitPublishKey))
                    {
                            isCommentRequired = true;
                    }
                    //----------------------------------------------
                    // Save Draft/Save Draft & Exit button
                    //----------------------------------------------
                    else if (keys.Contains(Constants.SubmitSaveKey))
                    {
                        actionType = updateModel.ModelState[Constants.SubmitSaveKey].AttemptedValue;
                        switch(actionType)
                        {
                            case "submit.Save":
                            case "submit.RequiresRevision":
                                isCommentRequired = true;
                                break;

                            case "submit.SaveAndContinue":
                            case "submit.RequestApproval-ContentDesign":
                            case "submit.RequestApproval-Stakeholder":
                            case "submit.RequestApproval-SME":
                            case "submit.RequestApproval-UX":
                                isCommentRequired = false;
                                break;

                            default:
                                isCommentRequired = true;
                                break;
                        }
                    }
                    //----------------------------------------------
                    // Request review button
                    //----------------------------------------------
                    else if (keys.Contains(Constants.SubmitRequestApprovalValuePrefix))
                    {
                        isCommentRequired = false;
                    }
                    //----------------------------------------------
                    // Send back button
                    //----------------------------------------------
                    else if (keys.Contains(Constants.SubmitRequiresRevisionValue))
                    {
                        isCommentRequired = true;
                    }
                }
            }

            return isCommentRequired;
        }
    }
}
