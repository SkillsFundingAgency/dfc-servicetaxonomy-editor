using System.Threading.Tasks;
using DFC.ServiceTaxonomy.ContentApproval.Models;
using DFC.ServiceTaxonomy.ContentApproval.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace DFC.ServiceTaxonomy.ContentApproval.Drivers
{
    //todo DashboardItemStatusPart
    class ContentApprovalDashboardPartDisplayDriver : ContentPartDisplayDriver<ContentApprovalDashboardPart>
    {
        public override Task<IDisplayResult> DisplayAsync(ContentApprovalDashboardPart part, BuildPartDisplayContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<ContentApprovalDashboardPartViewModel>(
                    GetDisplayShapeType(context), async m => await BuildDisplayViewModel(m, part))
                    .Location("DetailAdmin", "Content:10"));
        }

        public override Task<IDisplayResult> EditAsync(ContentApprovalDashboardPart part, BuildPartEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<ContentApprovalDashboardPartViewModel>(GetEditorShapeType(context), m => BuildViewModel(m, part)));
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentApprovalDashboardPart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Card);

            return await EditAsync(model, context);
        }

        private ValueTask BuildViewModel(ContentApprovalDashboardPartViewModel model, ContentApprovalDashboardPart part)
        {
            model.Card = part.Card;

            return default;
        }

        private ValueTask BuildDisplayViewModel(
            ContentApprovalDashboardPartViewModel model,
            ContentApprovalDashboardPart part)
        {
            BuildViewModel(model, part);

            model.NumberOfContentItemsInState = 100;

            return default;
        }
    }
}
