using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Extensions;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.ContentApproval.Controllers
{
    public class ContentApprovalController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<ContentApprovalController> H;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public ContentApprovalController(IAuthorizationService authorizationService, IContentManager contentManager, INotifier notifier, IHtmlLocalizer<ContentApprovalController> htmlLocalizer, IUrlHelperFactory urlHelperFactory)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            _notifier = notifier;
            H = htmlLocalizer;
            _urlHelperFactory = urlHelperFactory;
        }
        public async Task<IActionResult> RequestApproval(string contentItemId, string returnUrl, string reviewType)
        {
            if(string.IsNullOrWhiteSpace(contentItemId) || string.IsNullOrWhiteSpace(reviewType))
            {
                return BadRequest();
            }

            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.RequestReviewPermissions.RequestReviewPermission, contentItem))
            {
                return Forbid();
            }

            contentItem.Alter<ContentApprovalPart>(p => p.ReviewStatus = ReviewStatus.ReadyForReview);
            contentItem.Alter<ContentApprovalPart>(p => p.ReviewType = Enum.Parse<ReviewType>(reviewType));
            await _contentManager.SaveDraftAsync(contentItem);
            _notifier.Success(H["{0} is now ready to be reviewed.", contentItem.DisplayText]);

            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Review(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
            var contentApprovalPart = contentItem?.As<ContentApprovalPart>();
            if (contentItem == null || contentApprovalPart == null)
            {
                return NotFound();
            }

            if (contentApprovalPart.ReviewStatus == ReviewStatus.ReadyForReview && contentApprovalPart.ReviewType == ReviewType.None)
            {
                _notifier.Warning(H["An appropriate review type hasn't been selected. Please resubmit for review."]);
                contentItem.Alter<ContentApprovalPart>(p => p.ReviewStatus = ReviewStatus.NotInReview);
                await _contentManager.SaveDraftAsync(contentItem);
                return Redirect(returnUrl);
            }

            if (!await _authorizationService.AuthorizeAsync(User, contentApprovalPart.ReviewType.GetRelatedPermission(), contentItem))
            { 
                return Forbid();
            }

            if (contentApprovalPart.ReviewStatus == ReviewStatus.NotInReview)
            {
                _notifier.Warning(H["This item is no longer available for review."]);
                return Redirect(returnUrl);
            }

            if (contentApprovalPart.ReviewStatus == ReviewStatus.ReadyForReview)
            {
                contentItem.Alter<ContentApprovalPart>(p => p.ReviewStatus = ReviewStatus.InReview);
                contentItem.Author = User.Identity.Name;
                await _contentManager.SaveDraftAsync(contentItem);
            }
            else if (contentApprovalPart.ReviewStatus == ReviewStatus.InReview)
            {
                _notifier.Warning(H["This content item has already been selected for review."]);
            }

            return await RedirectToEdit(returnUrl, contentItem);
        }

        private async Task<IActionResult> RedirectToEdit(string returnUrl, ContentItem contentItem)
        {
            var metadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
            if (metadata.EditorRouteValues == null)
            {
                _notifier.Error(H["Could not redirect for review: {0}", contentItem.DisplayText]);
                return Redirect(returnUrl);
            }

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                metadata.EditorRouteValues.Add("returnUrl", returnUrl);
            }

            var editLink = _urlHelperFactory.GetUrlHelper(Url.ActionContext).Action(
                metadata.EditorRouteValues["action"].ToString(),
                metadata.EditorRouteValues);
            return Redirect(editLink);
        }

        public async Task<IActionResult> Edit(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
            if (contentItem == null)
            {
                return NotFound();
            }
            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var reviewStatus = contentItem.As<ContentApprovalPart>()?.ReviewStatus ?? ReviewStatus.NotInReview;
            if(reviewStatus == ReviewStatus.InReview)
            {
                _notifier.Warning(H["This item is already in review with {0} and therefore cannot be edited.", contentItem.Author]);
                return Redirect(returnUrl);
            }
            return await RedirectToEdit(returnUrl, contentItem);
        }
    }
}
