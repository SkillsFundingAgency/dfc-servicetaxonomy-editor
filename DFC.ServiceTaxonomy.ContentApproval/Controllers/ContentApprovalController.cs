using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
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

        public ContentApprovalController(IAuthorizationService authorizationService, IContentManager contentManager, INotifier notifier, IHtmlLocalizer<ContentApprovalController> htmlLocalizer)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            _notifier = notifier;
            H = htmlLocalizer;
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

            contentItem.Alter<ContentApprovalPart>(p => p.ApprovalStatus = ContentApprovalStatus.ReadyForReview);
            await _contentManager.SaveDraftAsync(contentItem);
            _notifier.Success(H[$"{contentItem.DisplayText} is now ready to be reviewed."]);

            return Redirect(returnUrl);
        }
    }
}
