using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.ContentManagement;
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
        public async Task<IActionResult> RequestApproval(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.RequestReviewPermissions.RequestReviewPermission, contentItem))
            {
                return Forbid();
            }

            contentItem.Alter<ContentApprovalPart>(p => p.ReviewStatus = ContentReviewStatus.ReadyForReview);
            await _contentManager.SaveDraftAsync(contentItem);
            _notifier.Success(H[$"{0} is now ready to be reviewed.", contentItem.DisplayText]);

            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Review(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
            if (contentItem == null)
            {
                return NotFound();
            }
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.CanPerformReviewPermissions.CanPerformReviewPermission, contentItem))
            {
                return Forbid();
            }

            var reviewStatus = contentItem.As<ContentApprovalPart>().ReviewStatus;

            if (reviewStatus == null)
            {
                _notifier.Warning(H[$"This content item is not currently ready for review: {0}", contentItem.DisplayText]);
                return Redirect(returnUrl);
            }

            if (reviewStatus == ContentReviewStatus.ReadyForReview)
            {
                contentItem.Alter<ContentApprovalPart>(p => p.ReviewStatus = ContentReviewStatus.InReview);
                await _contentManager.SaveDraftAsync(contentItem);
            }

            if (reviewStatus == ContentReviewStatus.InReview)
            {
                _notifier.Warning(H[$"This content item has already been selected for review."]);
            }

            var metadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
            if (metadata.EditorRouteValues == null)
            {
                _notifier.Error(H[$"Could not redirect for review: {0}", contentItem.DisplayText]);
                return Redirect(returnUrl);
            }

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                metadata.EditorRouteValues.Add("returnUrl", returnUrl);
            }

            var editLink = _urlHelperFactory.GetUrlHelper(Url.ActionContext).Action(metadata.EditorRouteValues["action"].ToString(),
                metadata.EditorRouteValues);
            return Redirect(editLink);
        }
    }
}
