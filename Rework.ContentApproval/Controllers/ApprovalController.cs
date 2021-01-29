using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Routing;
using YesSql;

namespace Rework.ContentApproval.Controllers
{
    public class ApprovalController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INotifier _notifier;
        private readonly ISession _session;
        private readonly IHtmlLocalizer H;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public ApprovalController(
            IAuthorizationService authorizationService,
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            YesSql.ISession session,
            IHtmlLocalizer<ApprovalController> htmlLocalizer,
            IUpdateModelAccessor updateModelAccessor)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _session = session;

            H = htmlLocalizer;
            _updateModelAccessor = updateModelAccessor;
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> EditAndPublishPOST(string contentItemId, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
        {
            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";
            var content = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (content == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, content))
            {
                return Forbid();
            }
            return await EditPOST(contentItemId, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content has been published."]
                    : H["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }

        private async Task<IActionResult> EditPOST(string contentItemId, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);
if (contentItem == null)
{
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
{
return Forbid();
}

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View("Edit", model);
            }

            await conditionallyPublish(contentItem);

            if (returnUrl == null)
            {
                return RedirectToAction("Edit", new RouteValueDictionary { { "ContentItemId", contentItem.ContentItemId } });
            }
            else if (stayOnSamePage)
            {
                return RedirectToAction("Edit", new RouteValueDictionary { { "ContentItemId", contentItem.ContentItemId }, { "returnUrl", returnUrl } });
            }
            else
            {
                return LocalRedirect(returnUrl);
            }
        }
    }
}
