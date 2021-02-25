using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Content.Services.Interface;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Services;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using YesSql;

namespace DFC.ServiceTaxonomy.ContentApproval.Drivers
{
    class ContentApprovalItemStatusDashboardPartDisplayDriver : ContentPartDisplayDriver<ContentApprovalItemStatusDashboardPart>
    {
        private readonly ISession _session;
        private readonly IContentItemsService _contentItemsService;
        private readonly IContentItemsApprovalService _contentItemsApprovalService;
        private readonly DefaultContentsAdminListFilter _defaultContentsAdminListFilter;

        public ContentApprovalItemStatusDashboardPartDisplayDriver(
            ISession session,
            IContentItemsService contentItemsService,
            IContentItemsApprovalService contentItemsApprovalService,
            DefaultContentsAdminListFilter defaultContentsAdminListFilter)
        {
            _session = session;
            _contentItemsService = contentItemsService;
            _contentItemsApprovalService = contentItemsApprovalService;
            _defaultContentsAdminListFilter = defaultContentsAdminListFilter;
        }

        public override Task<IDisplayResult> DisplayAsync(ContentApprovalItemStatusDashboardPart part, BuildPartDisplayContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<ContentApprovalItemStatusDashboardPartViewModel>(
                    GetDisplayShapeType(context), async m => await BuildDisplayViewModel(m, part))
                    .Location("DetailAdmin", "Content:10"));
        }

        public override Task<IDisplayResult> EditAsync(ContentApprovalItemStatusDashboardPart part, BuildPartEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<ContentApprovalItemStatusDashboardPartViewModel>(GetEditorShapeType(context), m => BuildViewModel(m, part)));
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentApprovalItemStatusDashboardPart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Card);

            return await EditAsync(model, context);
        }

        private ValueTask BuildViewModel(ContentApprovalItemStatusDashboardPartViewModel model, ContentApprovalItemStatusDashboardPart part)
        {
            model.Card = part.Card;

            return default;
        }

        private async Task BuildDisplayViewModel(
            ContentApprovalItemStatusDashboardPartViewModel model,
            ContentApprovalItemStatusDashboardPart part)
        {
            await BuildViewModel(model, part);

            //todo: move out of here into a service

            var filterOptions = new ContentOptionsViewModel();

            //todo: exclude non listable?
            //todo: only items user has permission for?
            // use DefaultContentsAdminListFilter instead and count??
            // might be less performant, but at least the count should match what's shown
            // model.NumberOfContentItemsInState = part.Card switch
            // {
            //     DashboardItemsStatusCard.InDraft => await _contentItemsService.GetDraftCount(),
            //     DashboardItemsStatusCard.Published => await _contentItemsService.GetPublishedCount(),
            //     _ => 99
            // };

            switch (part.Card)
            {
                case DashboardItemsStatusCard.InDraft:
                    filterOptions.ContentsStatus = ContentsStatus.Draft;
                    break;
                case DashboardItemsStatusCard.Published:
                    filterOptions.ContentsStatus = ContentsStatus.Published;
                    break;
                default:
                    throw new NotImplementedException();
            }

            var query = _session.Query<ContentItem>();

            await _defaultContentsAdminListFilter.FilterAsync(filterOptions, query, null);

            model.NumberOfContentItemsInState = await query.CountAsync();
        }
    }
}
