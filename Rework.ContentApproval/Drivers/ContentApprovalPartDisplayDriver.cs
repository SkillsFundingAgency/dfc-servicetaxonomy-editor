using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using Rework.ContentApproval.Common;
using Rework.ContentApproval.Models;
using Rework.ContentApproval.ViewModels;
using Rework.ContentApproval.Workflows.Activities;
using System.Threading.Tasks;
using MimeKit.Cryptography;

namespace Rework.ContentApproval.Drivers
{
    public class ContentApprovalPartDisplayDriver : ContentPartDisplayDriver<ContentApprovalPart>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public ContentApprovalPartDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(ContentApprovalPart part, BuildPartEditorContext context)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var baseShapeName = GetEditorShapeType(context);

            //todo: just create model once

            var results = new List<IDisplayResult>();

            // Have to do Publish Content permissions first
            if (await _authorizationService.AuthorizeAsync(httpContext?.User, OrchardCore.Contents.Permissions.PublishContent, part.ContentItem))
            {
                var shapeName = $"{ baseShapeName }_ApprovalResponse";
                results.Add(Initialize<ApprovalResponseViewModel>(shapeName,
                    model => PopulateApprovalResponseViewModel(part, model))
                .Location("Actions:First"));
            }
            else if (await _authorizationService.AuthorizeAsync(httpContext?.User, Permissions.RequestApproval, part.ContentItem))
            {
                var shapeName = $"{ baseShapeName }_ApprovalRequest";
                results.Add(Initialize<ApprovalRequestViewModel>(shapeName,
                    model => PopulateApprovalRequestViewModel(part, model))
                .Location("Actions:Last"));
            }
            else
            {
                return null;
            }

            // difference between above and below styles??
            // dynamic with no view model vs strongly typed with viewmodel??
            // results.Add(Dynamic("Content_PublishButton").Location("Actions:10")
            //     .RenderWhen(() => _authorizationService.AuthorizeAsync(context.User, CommonPermissions.PublishContent, contentItem)));

            results.Add(Initialize<ApprovalRequestViewModel>($"{ baseShapeName }_ForcePublish",
                    model => PopulateApprovalRequestViewModel(part, model))
                .Location("Actions:15"));

            return Combine(results.ToArray());
        }

        //    [FormValueRequired("submit.Publish")]

        public override async Task<IDisplayResult> UpdateAsync(ContentApprovalPart part, IUpdateModel updater)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            //todo: forcepublish (own/others) permission

            if (httpContext.Request.Form["submit.Publish"] == "submit.ForcePublish"
                || httpContext.Request.Form["submit.Publish"] == "submit.ForcePublishAndContinue")
            {

            }

            // Have to do Publish Content permissions first
            if (await _authorizationService.AuthorizeAsync(httpContext?.User, OrchardCore.Contents.Permissions.PublishContent, part.ContentItem))
            {
                var viewModel = new ApprovalResponseViewModel();

                await updater.TryUpdateModelAsync(viewModel, Prefix);

                if (httpContext.Request.Form["submit.Publish"] == "submit.Publish"
                    || httpContext.Request.Form["submit.Publish"] == "submit.PublishAndContinue")
                {
                    part.Status = Settings.ContentApproved;
                    part.NotificationNeeded = nameof(ApprovalResponseEvent);
                }
                else if (httpContext.Request.Form["submit.Publish"] == "submit.Approved")
                {
                    part.Status = Settings.ContentApproved;
                    part.Notes = viewModel.Notes;
                    part.NotificationNeeded = nameof(ApprovalResponseEvent);
                }
                else if (httpContext.Request.Form["submit.Save"] == "submit.NeedsRevised")
                {
                    part.Status = Settings.ContentNeedsRevised;
                    part.Notes = viewModel.Notes;
                    part.NotificationNeeded = nameof(ApprovalResponseEvent);
                }
                else if (httpContext.Request.Form["submit.Save"] == "submit.Rejected")
                {
                    part.Status = Settings.ContentRejected;
                    part.Notes = viewModel.Notes;
                    part.NotificationNeeded = nameof(ApprovalResponseEvent);
                }
            }
            else if (await _authorizationService.AuthorizeAsync(httpContext?.User, Permissions.RequestApproval, part.ContentItem))
            {
                var viewModel = new ApprovalRequestViewModel();

                await updater.TryUpdateModelAsync(viewModel, Prefix);

                if (httpContext.Request.Form["submit.Save"] == "submit.RequestApproval")
                {
                    part.Status = Settings.ContentApprovalRequested;
                    part.Notes = viewModel.Notes;
                    part.NotificationNeeded = nameof(ApprovalRequestEvent);
                }
            }


            return Edit(part);
        }

        private void PopulateApprovalRequestViewModel(ContentApprovalPart part, ApprovalRequestViewModel viewModel)
        {
            viewModel.Status = part.Status;
            viewModel.Notes = part.Notes;
        }

        private void PopulateApprovalResponseViewModel(ContentApprovalPart part, ApprovalResponseViewModel viewModel)
        {
            viewModel.Status = part.Status;
            viewModel.Notes = part.Notes;
        }

        // does the item get published anyway?
        // is admincontroller partial
        // can we have a completely separate action?
        // can we get the previous version from history and resave it?
        /*
        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> CreateAndPublishPOST(string id, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
        {
            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";
            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = await _contentManager.NewAsync(id);

            // Set the current user as the owner to check for ownership permissions on creation
            dummyContent.Owner = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, dummyContent))
            {
                return Forbid();
            }

            return await CreatePOST(id, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content has been published."]
                    : H["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }

        private async Task<IActionResult> CreatePOST(string id, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
        {
            var contentItem = await _contentManager.NewAsync(id);

            // Set the current user as the owner to check for ownership permissions on creation
            contentItem.Owner = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            if (ModelState.IsValid)
            {
                await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);
            }

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View(model);
            }

            await conditionallyPublish(contentItem);

            if ((!string.IsNullOrEmpty(returnUrl)) && (!stayOnSamePage))
            {
                return LocalRedirect(returnUrl);
            }

            var adminRouteValues = (await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem)).AdminRouteValues;

            if (!string.IsNullOrEmpty(returnUrl))
            {
                adminRouteValues.Add("returnUrl", returnUrl);
            }

            return RedirectToRoute(adminRouteValues);
        }
        */
    }
}
