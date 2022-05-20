using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using DFC.ServiceTaxonomy.UnpublishLater.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.UnpublishLater.Drivers
{
    public class UnpublishLaterPartDisplayDriver : ContentPartDisplayDriver<UnpublishLaterPart>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILocalClock _localClock;

        public UnpublishLaterPartDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            ILocalClock localClock)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _localClock = localClock;
        }

        public override IDisplayResult Display(UnpublishLaterPart part, BuildPartDisplayContext context)
        {
            return Initialize<UnpublishLaterPartViewModel>(
                $"{nameof(UnpublishLaterPart)}_SummaryAdmin",
                model => PopulateViewModel(part, model))
            .Location("SummaryAdmin", "Meta:25");
        }

        public override IDisplayResult Edit(UnpublishLaterPart part, BuildPartEditorContext context)
        {
            var results = new List<IDisplayResult>();

            if(DisplayEditor(part.ContentItem))
            {
                results.Add(Initialize<UnpublishLaterPartViewModel>(GetEditorShapeType(context),
                model => PopulateViewModel(part, model))
            .Location("Actions:10"));
            }
            else if(!context.Updater.ModelState.IsValid)
            {
                part.ScheduledUnpublishUtc = part.Content.ScheduledUnpublishUtc.Value;
                results.Add(Initialize<UnpublishLaterPartViewModel>(GetEditorShapeType(context),
                model => PopulateViewModel(part, model))
            .Location("Actions:10"));
            }

            return Combine(results.ToArray());
        }

        private bool DisplayEditor(ContentItem contentItem)
        {
            return contentItem.Published && contentItem.Latest;
        }


        public override async Task<IDisplayResult> UpdateAsync(UnpublishLaterPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (await _authorizationService.AuthorizeAsync(httpContext?.User, CommonPermissions.PublishContent, part.ContentItem))
            {
                var viewModel = new UnpublishLaterPartViewModel();

                await updater.TryUpdateModelAsync(viewModel, Prefix);
               
                if (httpContext!.Request.Form[Constants.Submit_Publish_Key] == Constants.Submit_Cancel_Unpublish_Later)
                {
                    part.ScheduledUnpublishUtc = null;
                }
                else if(httpContext!.Request.Form[Constants.Submit_Publish_Key] == Constants.Submit_Unpublish_Later)
                {
                    if(viewModel.ScheduledUnpublishLocalDateTime.HasValue)
                    {
                        part.ScheduledUnpublishUtc = await _localClock.ConvertToUtcAsync(viewModel.ScheduledUnpublishLocalDateTime.Value);
                    }
                    else
                    {
                        updater.ModelState.AddModelError("ScheduledUnpublishLocalDateTime", "Please select a time for unpublishing.");
                    }
                }
            }

            return Edit(part, context);
        }

        private async ValueTask PopulateViewModel(UnpublishLaterPart part, UnpublishLaterPartViewModel viewModel)
        {
            viewModel.ContentItem = part.ContentItem;
            viewModel.ScheduledUnpublishUtc = part.ScheduledUnpublishUtc;
            viewModel.ScheduledUnpublishLocalDateTime = part.ScheduledUnpublishUtc.HasValue ?
                (await _localClock.ConvertToLocalAsync(part.ScheduledUnpublishUtc.Value)).DateTime :
                (DateTime?)null;
        }
    }
}
