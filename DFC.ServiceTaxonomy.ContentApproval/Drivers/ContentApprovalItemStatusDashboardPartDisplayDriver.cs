using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.Services;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.ContentApproval.Drivers
{
    class ContentApprovalItemStatusDashboardPartDisplayDriver : ContentPartDisplayDriver<ContentApprovalItemStatusDashboardPart>
    {
        private readonly IContentItemsApprovalService _contentItemsApprovalService;

        public ContentApprovalItemStatusDashboardPartDisplayDriver(
            IContentItemsApprovalService contentItemsApprovalService)
        {
            _contentItemsApprovalService = contentItemsApprovalService;
        }

        public override Task<IDisplayResult> DisplayAsync(ContentApprovalItemStatusDashboardPart part, BuildPartDisplayContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<ContentApprovalItemStatusDashboardPartViewModel>(
                    GetDisplayShapeType(context), async m => await BuildDisplayViewModel(m, part, context.Updater))
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
            ContentApprovalItemStatusDashboardPart part,
            IUpdateModel updater)
        {
            await BuildViewModel(model, part);

            model.ContentItemsApprovalCounts = await _contentItemsApprovalService.GetManageContentItemCounts(part.Card, updater);
        }
    }
}
